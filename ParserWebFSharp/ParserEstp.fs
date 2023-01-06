namespace ParserWeb

open System.Web
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserEstp(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://estp.ru/purchases?page="

    override __.Parsing() =
        for i in 20..-1..1 do
            try
                __.ParsingPage(sprintf "%s%d" url i)
            with
                | ex -> Logging.Log.logger ex


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let htmlDoc = HtmlDocument()
            htmlDoc.LoadHtml(s)

            let nav =
                (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator

            let tens =
                nav
                    .CurrentDocument
                    .DocumentNode
                    .SelectNodesOrEmpty("//div[contains(@class, 'Card_card__ERPiY ListItem')]")
                    .ToList()

            tens.Reverse()

            for t in tens do
                try
                    __.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

            ()

        ()

    member private __.ParsingTender (t: HtmlNode) (_: string) =
        let builder = DocumentBuilder()

        let res =
            builder {

                let! purNum =
                    t.GsnDocWithError ".//span[contains(.,  'Код закупки: ')]/span"
                    <| sprintf "purNum not found %s %s " url (t.InnerText)

                let! hrefT =
                    t.GsnAtrDocWithError ".//div[contains(@class, 'ListItem_title')]//a"
                    <| "href"
                    <| sprintf "hrefT not found %s %s " url (t.InnerText)

                let href = sprintf "http://estp.ru%s" hrefT
                let! status = t.GsnDoc ".//div[contains(@class, 'Phase_title__')]//span"
                let! pwName = t.GsnDoc ".//div[contains(@class, 'Procedure_title')]"
                let! orgName = t.GsnDoc ".//span[contains(@class, 'Organizer_value')]"

                let! purName =
                    t.GsnDocWithError ".//div[contains(@class, 'ListItem_title')]//a"
                    <| sprintf "purName not found %s %s " url (t.InnerText)

                let! region = t.GsnDoc ".//span[contains(@class, 'Regions_value')]"

                let! datePubT =
                    t.GsnDocWithError ".//div[contains(@class, 'PublishDate_publishDate')]"
                    <| sprintf "datePubT not found %s %s " href (t.InnerText)
                let datePubT = datePubT.ReplaceDate()
                let! datePubT = datePubT.Get1OptionalDoc(@"(\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2})")
                let! datePub =
                    datePubT.DateFromStringDoc("dd.MM.yyyy HH:mm", sprintf "datePub not found %s %s " href (datePubT))

                let! dateEndT =
                    t.GsnDoc ".//div[contains(@class, 'Phase_date__')]"

                let dateEnd = dateEndT.DateFromStringOrPubPlus2("dd.MM.yyyy HH:mm", datePub)
                let! nmckT = t.GsnDoc ".//span[contains(@class, 'CurrencyValue_value')]"
                let nmck = nmckT.GetPriceFromStringKz().Replace("₽", "")
                let currency = "₽"

                let tend =
                    { Href = href
                      PurName = HttpUtility.HtmlDecode(purName)
                      PurNum = purNum
                      PlacingWay = pwName
                      DateEnd = dateEnd
                      OrgName = HttpUtility.HtmlDecode(orgName)
                      Price = nmck
                      Status = status
                      Region = region
                      Currency = currency
                      DatePub = datePub }

                let T =
                    TenderEstp(set, tend, 308, "ESTP.RU", "http://estp.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
