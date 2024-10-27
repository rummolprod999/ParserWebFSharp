namespace ParserWeb

open TypeE
open HtmlAgilityPack
open System.Linq

type ParserOsnova(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://tender.gk-osnova.ru/site?page="

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
                    .SelectNodesOrEmpty("//div[contains(@class,'tender-head main-tender-head')]")
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
                let! purName = t.GsnDocWithError ".//h2/a" "" // <| sprintf "purName not found %s %s " url (t.InnerText)

                let! hrefT =
                    t.GsnAtrDocWithError ".//h2/a"
                    <| "href"
                    <| sprintf "hrefT not found %s %s " url (t.InnerText)

                let href =
                    sprintf "https://tender.gk-osnova.ru%s" hrefT

                let! purNum =
                    href.Get1Doc "id=(\d+)$"
                    <| sprintf "purNum not found %s %s " url (t.InnerText)

                let! dates1 =
                    t.GsnDocWithError ".//div[@class = 'date__row-container'][1]"
                    <| sprintf "dates1 not found %s %s " url (t.InnerText)

                let! dates2 =
                    t.GsnDocWithError ".//div[@class = 'date__row-container'][2]"
                    <| sprintf "dates2 not found %s %s " url (t.InnerText)

                let! dateEndT =
                    dates2
                        .RegexCutWhitespace()
                        .Trim()
                        .Get1Doc "по\s+(\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2})"
                    <| sprintf "dateEndT not found %s %s " url (dates2.RegexCutWhitespace().Trim())

                let dateEnd =
                    dateEndT.DateFromStringOrMin("dd.MM.yyyy HH:mm")

                let! datePubT =
                    dates1
                        .RegexCutWhitespace()
                        .Trim()
                        .Get1Doc "(\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2})"
                    <| sprintf "datePubT not found %s %s " url (dates1.RegexCutWhitespace().Trim())

                let datePub =
                    datePubT.DateFromStringOrMin("dd.MM.yyyy HH:mm")

                let! status = t.GsnDoc ".//span[contains(@class, 'status-badge')]"

                let tend =
                    { OsnovaRec.Href = href
                      PurName = purName
                      PurNum = purNum
                      Status = status
                      DateEnd = dateEnd
                      DatePub = datePub }

                let T =
                    TenderOsnova(set, tend, 287, "ГК \"ОСНОВА\"", "https://tender.gk-osnova.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
