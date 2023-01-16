namespace ParserWeb

open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html

type ParserRosAgro(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let spage =
        "https://www.rosagroleasing.ru/company/tenders/"

    override __.Parsing() =
        try
            __.ParsingPage(spage)
        with
            | ex -> Logging.Log.logger ex

    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents.QuerySelectorAll("tbody.grid_rows > tr")

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
                let! href =
                    t.GsnAtrDocWithError "td:nth-of-type(1) > a" "href"
                    <| sprintf "href not found %s %s " url (t.TextContent)

                let href =
                    sprintf "https://www.rosagroleasing.ru/company/tenders/%s" href

                let! purName =
                    t.GsnDocWithError "td:nth-of-type(1) > a"
                    <| sprintf "purName not found %s %s " url (t.TextContent)

                let! pwName =
                    t.GsnDocWithError "td:nth-of-type(3)"
                    <| sprintf "pwName not found %s %s " url (t.TextContent)

                let! purNum =
                    t.GsnAtrDocWithError "td:nth-of-type(1) > a" "href" <| sprintf "href not found %s %s " url (t.TextContent)
                let purNum = purNum.Replace("/", "")

                let! dateEndT =
                    t.GsnDocWithError "td:nth-of-type(5)"
                    <| sprintf "dateEndT not found %s %s " url (t.TextContent)

                let! dateEnd =
                    dateEndT.DateFromStringDoc("yyyy-MM-dd HH:mm:ss", sprintf "dateEnd not found %s %s " href dateEndT)

                let! datePubT =
                    t.GsnDocWithError "td:nth-of-type(7)"
                    <| sprintf "datePubT not found %s %s " url (t.TextContent)

                let! datePub =
                    datePubT.DateFromStringDoc("dd.MM.yyyy", sprintf "datePub not found %s %s " href datePubT)

                let! nmck =
                    t.GsnDocWithError "td:nth-of-type(4)"
                    <| sprintf "nmck not found %s %s " url (t.TextContent)

                let nmck = nmck.GetPriceFromStringKz()

                let tend =
                    { RosAgroRec.Href = href
                      DateEnd = dateEnd
                      DatePub = datePub
                      PurName = purName
                      PurNum = purNum
                      Nmck = nmck
                      PwName = pwName }

                let T =
                    TenderRosAgro(set, tend, 231, "АО «Росагролизинг»", "https://www.rosagroleasing.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
