namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System
open TypeE
open System.Linq

type ParserAero(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let _ = 10

    override this.Parsing() =
        for i in 1..1 do
            try
                let url = "https://www.aeroflot.ru/ru-ru/about/retail_center/monitoring-cen"

                this.ParsingPage url
            with
                | ex -> Logging.Log.logger ex

    member private this.ParsingPage(url: string) =
        let Page = Download.DownloadStringBot url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let mutable tens =
                documents.QuerySelectorAll("div.main-module__wrapper div.main-module__row")
            if tens.Length > 0 then
                let tensN = tens.Skip(1).Reverse()
                for t in tensN do
                    try
                        this.ParsingTender t url
                    with
                        | ex -> Logging.Log.logger ex

            ()

    member private this.ParsingTender (t: IElement) (url: string) =
        
        let HrefDoc =
            "https://www.aeroflot.ru/ru-ru/about/retail_center/monitoring-cen"

        let PurName =
            match t.QuerySelector("div.col--12 p:nth-of-type(2)") with
            | null ->
                raise
                <| NullReferenceException(sprintf "PurName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        let purNum = Tools.createMD5 PurName
        let pWay = ""

        let status = ""

        let PubDateT =
            match t.QuerySelector("div:nth-of-type(1) p:nth-of-type(2)") with
            | null ->
                raise
                <| NullReferenceException(sprintf "PubDateT not found in %s" url)
            | ur -> ur.TextContent.Trim()

        let datePub =
            match PubDateT.DateFromString("dd.MM.yyyy") with
            | Some d -> d
            | None ->
                raise
                <| Exception(sprintf "cannot parse datePub %s" PubDateT)

        let EndDateT = ""

        let dateEnd =
            EndDateT.DateFromStringOrPubPlus2("dd.MM.yyyy HH:mm", datePub)
        let docs =
                    t.QuerySelectorAll("div:nth-of-type(3) a").ToList()

        let ten =
            { Href = HrefDoc
              PurNum = purNum
              PurName = PurName
              PwayName = pWay
              DatePub = datePub
              DateEnd = dateEnd
              status = status 
              DocList = docs }

        try
            let T =
                TenderAero(set, ten, 59, "ПАО «Аэрофлот»", "https://www.aeroflot.ru")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, url)

        ()
