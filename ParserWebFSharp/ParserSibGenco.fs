namespace ParserWeb

open System.Web
open TypeE
open System
open HtmlAgilityPack
open System.Linq

type ParserSibGenco(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://sibgenco.ru/tenders/?PAGEN_1="

    override __.Parsing() =
        for i in 10..-1..1 do
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
                    .SelectNodesOrEmpty("//div[@class = 'tenders__table-inner']//li[position() > 1]/div")
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
                let! purName =
                    t.GsnDocWithError "./div[3]/a"
                    <| sprintf "purName not found %s %s " url (t.InnerText)

                let! hrefT =
                    t.GsnAtrDocWithError "./div[3]/a"
                    <| "href"
                    <| sprintf "hrefT not found %s %s " url (t.InnerText)

                let href =
                    sprintf "https://sibgenco.ru%s" hrefT

                let! purNum =
                    t.GsnDocWithError "./div[2]"
                    <| sprintf "purNum not found %s %s " url (t.InnerText)
                let purNum = match purNum with
                                | x when (not <| String.IsNullOrEmpty(x)) -> purNum
                                | _ -> Tools.createMD5 purName
                let! status =
                    t.GsnDocWithError "./div[5]"
                    <| sprintf "status not found %s %s " url (t.InnerText)

                let! pwName =
                    t.GsnDocWithError "./div[4]"
                    <| sprintf "pwName not found %s %s " url (t.InnerText)

                let! datePubT =
                    t.GsnDocWithError
                        ".//div[contains(@class, 'tenders-table__cell--date')]/div[1]"
                    <| sprintf "datePubT not found %s %s " url (t.InnerText)

                let! datePubT =
                    datePubT
                        .ReplaceDateSib()
                        .RegexCutWhitespace()
                        .Replace("(МСК+4)", "")
                        .Get1Doc "(\d{2}:\d{2}\s+\d{2}\.\d{2}\.\d{4})"
                    <| sprintf "datePubT not found %s %s " url (datePubT)
    
                let datePub =
                    datePubT.DateFromStringOrMin("HH:mm dd.MM.yyyy")

                let! dateEndT =
                    t.GsnDocWithError
                        ".//div[contains(@class, 'tenders-table__cell--date')]/div[2]"
                    <| sprintf "dateEndT not found %s %s " url (t.InnerText)

                let! dateEndT =
                    dateEndT
                        .ReplaceDateSib()
                        .RegexCutWhitespace()
                        .Replace("(МСК+4)", "")
                        .Get1Doc "(\d{2}:\d{2}\s+\d{2}\.\d{2}\.\d{4})"
                    <| sprintf "dateEndT not found %s %s " url (datePubT)

                let dateEnd =
                    dateEndT.DateFromStringOrMin("HH:mm dd.MM.yyyy")

                let tend =
                    { Href = href
                      PurName = HttpUtility.HtmlDecode(purName)
                      PurNum = purNum
                      PwName = pwName
                      Status = status
                      DateEnd = dateEnd
                      DatePub = datePub }

                let T =
                    TenderSibGenco(set, tend, 289, "ООО «Сибирская генерирующая компания»", "https://sibgenco.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
