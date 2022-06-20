namespace ParserWeb

open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html

type ParserTurk(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 5

    override this.Parsing() =
        for i in 1..pageC do
            try
                let url =
                    sprintf "https://turkmenportal.com/catalog/a/index?path=tendery-turkmenistana&page=%d" i

                this.ParsingPage url
            with
                | ex -> Logging.Log.logger ex

    member private this.ParsingPage(url: string) =
        let Page = Download.DownloadString url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents.QuerySelectorAll("div.items > div[data-key]")

            for t in tens do
                try
                    this.ParsingTenderAngle t url
                with
                    | ex -> Logging.Log.logger ex

        ()

    member private this.ParsingTenderAngle (t: IElement) (url: string) =
        let builder = DocumentBuilder()

        let res =
            builder {
                let! href =
                    t.GsnAtrDocWithError "div.client-title a" "href"
                    <| sprintf "href not found %s %s " url (t.TextContent)

                let! purNum =
                    href.Get1Doc "(\d+)$"
                    <| sprintf "PurNum not found %s %s " url href

                let! purName =
                    t.GsnDocWithError "div.client-title a"
                    <| sprintf "purName not found %s %s " url (t.TextContent)

                let! orgName = purName.Get1OptionalDoc "^(.+)объявляет"
                let! startDate = purName.Get1OptionalDoc "(?<=:\s)(\d{2}.\d{2}.\d{4})"
                let! pubDate = startDate.DateFromStringDocNow("dd.MM.yyyy")
                let! stopDate = purName.Get1OptionalDoc "(\d{2}.\d{2}.\d{4})$"
                let! endDate = stopDate.DateFromStringDocMin("dd.MM.yyyy")
                let! address = t.GsnDoc "div.client-details-item:nth-child(3)"
                let! contacts = t.GsnDoc "div.client-details-item:nth-child(4)"

                let T =
                    TenderTurk(
                        set,
                        206,
                        "turkmenportal.com",
                        "https://turkmenportal.com/",
                        purNum,
                        purName,
                        pubDate,
                        endDate,
                        href,
                        address,
                        contacts,
                        orgName
                    )

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
