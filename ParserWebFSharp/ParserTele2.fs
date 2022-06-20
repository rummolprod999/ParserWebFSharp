namespace ParserWeb

open System
open System.Web
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserTele2(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://msk.tele2.ru/about/cooperation/tender"

    override __.Parsing() =
        try
            __.ParsingPage(url)
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
                    .SelectNodesOrEmpty("//div[contains(@class, 'article-content')]//tbody/tr[td[@rowspan = '3']]")
                    .ToList()

            tens.Reverse()

            for t in tens do
                try
                    __.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

            ()

        ()

    member private __.ParsingTender (t: HtmlNode) (url: string) =
        let builder = DocumentBuilder()

        let res =
            builder {
                let! purName =
                    t.GsnDocWithError "./td[1]"
                    <| sprintf "purName not found %s %s " url (t.InnerText)

                let! delivTerm =
                    t.GsnDocWithError "./td[2]"
                    <| sprintf "delivTerm not found %s %s " url (t.InnerText)

                let! delivPlace =
                    t.GsnDocWithError "./following-sibling::tr[1]/td"
                    <| sprintf "delivPlace not found %s %s " url (t.InnerText)

                let tend =
                    { Href = url
                      PurName = purName
                      PurNum = Tools.createMD5 purName
                      DelivPlace = HttpUtility.HtmlDecode(delivPlace)
                      DelivTerm = HttpUtility.HtmlDecode(delivTerm)
                      DateEnd = DateTime.MinValue
                      DatePub = DateTime.Now }

                let T =
                    TenderTele2(set, tend, 286, "Теле2 Россия Интернешнл Селлулар БВ", "https://msk.tele2.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
