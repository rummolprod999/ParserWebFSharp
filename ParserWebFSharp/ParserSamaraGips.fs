namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq
open TypeE

type ParserSamaraGips(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls =
        [| "https://samaragips.ru/tender/" |]

    override __.Parsing() =
        for url in urls do
            try
                __.ParsingPage(url)
            with
                | ex -> Logging.Log.logger ex

        ()


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents
                    .QuerySelectorAll("table.items--tenders tr.item--tender")
                    .ToList()
                    .Skip(1)

            for t in tens do
                try
                    __.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

            ()

        ()

    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()

        let res =
            builder {
                let! purName =
                    t.GsnDocWithError "td:nth-child(2) a"
                    <| sprintf "purName not found %s %s " url (t.TextContent)

                let purNum = Tools.createMD5 purName

                let! href =
                    t.GsnAtrDocWithError "td:nth-child(2) a" "href"
                    <| sprintf "href not found %s %s " url (t.TextContent)

                let href =
                    sprintf "https://samaragips.ru%s" href

                let! datePubT =
                    t.GsnDocWithError "td:nth-child(1)"
                    <| sprintf "datePubT not found %s %s " url (t.TextContent)

                let datePub =
                    datePubT.DateFromStringOrMin("dd.MM.yyyy")

                let! status =
                    t.GsnDocWithError "td.item-status"
                    <| sprintf "status not found %s %s " url (t.TextContent)

                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub
                      Status = status }

                let T =
                    TenderSamaraGips(set, ten, 282, "ЗАО \"САМАРСКИЙ ГИПСОВЫЙ КОМБИНАТ\"", "https://samaragips.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
