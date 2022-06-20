namespace ParserWeb

open System
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserNeftisa(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://www.neftisa.ru/tenders/?PAGEN_1="

    override __.Parsing() =
        for i in 2..-1..1 do
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
                    .SelectNodesOrEmpty("//div[@class = 'tenders__item']")
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
                    t.GsnDocWithError ".//a[contains(@class, 'tenders__item-title')]"
                    <| sprintf "purName not found %s %s " url (t.InnerText)

                let! hrefT =
                    t.GsnAtrDocWithError ".//a[contains(@class, 'tenders__item-title')]"
                    <| "href"
                    <| sprintf "hrefT not found %s %s " url (t.InnerText)

                let href =
                    sprintf "https://www.neftisa.ru%s" hrefT

                let! purNum =
                    t.GsnDocWithError ".//p[@class = 'tenders__item-num']"
                    <| sprintf "purNum not found %s %s " url (t.InnerText)

                let datePub = DateTime.Today
                let dateEnd = datePub.AddDays(2.)
                let! region = t.GsnDoc ".//span[. = 'Регион']/following-sibling::p"
                let! orgName = t.GsnDoc ".//span[contains(., 'Организатор')]/following-sibling::p"

                let tend =
                    { Href = href
                      PurName = purName
                      PurNum = purNum
                      Region = region
                      OrgName = orgName
                      DatePub = datePub
                      DateEnd = dateEnd }

                let T =
                    TenderNeftisa(set, tend, 318, "АО «НК «Нефтиса»", "https://www.neftisa.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
