namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open TypeE

type ParserGosYakut(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "http://market.goszakazyakutia.ru/orders/published"

    override this.Parsing() =
        let Page = Download.DownloadString url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents.QuerySelectorAll("div.table-responsive table.table tbody tr")

            for t in tens do
                try
                    this.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

            ()

        ()

    member private this.ParsingTender (t: IElement) (url: string) =
        let PurNum =
            match t.QuerySelector("td:nth-child(2) a") with
            | null ->
                raise
                <| System.NullReferenceException(sprintf "PurNum not found in %s" url)
            | ur -> ur.TextContent.Trim()

        let PurName =
            match t.QuerySelector("td:nth-child(3) a") with
            | null ->
                raise
                <| System.NullReferenceException(sprintf "PurName not found in %s" url)
            | ur -> ur.TextContent.Trim()

        let HrefT =
            match t.QuerySelector("td:nth-child(3) a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()

        let Href =
            sprintf "http://market.goszakazyakutia.ru%s" HrefT

        let CusName =
            match t.QuerySelector("td:nth-child(5) a") with
            | null -> ""
            | ur -> ur.TextContent.Trim()

        let HrefCusT =
            match t.QuerySelector("td:nth-child(5) a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()

        let HrefCus =
            sprintf "http://market.goszakazyakutia.ru%s" HrefCusT

        let NmckT =
            match t.QuerySelector("td:nth-child(4)") with
            | null -> ""
            | ur -> ur.TextContent.Trim()

        let Nmck = NmckT.GetPriceFromString()

        let PwNameT =
            match t.QuerySelector("td:nth-child(2)") with
            | null -> ""
            | ur -> ur.TextContent.Trim()

        let PwName =
            PwNameT.Replace(PurNum, "").Trim()

        let PubDateT =
            match t.QuerySelector("td:nth-child(1)") with
            | null ->
                raise
                <| System.NullReferenceException(sprintf "PubDateT not found in %s" url)
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let dd =
            (fun (s: string) -> s.Replace(",", ""))
            >> (fun (s: string) -> Tools.GetDateFromStringMonth(s))

        let datePub =
            match (dd PubDateT).DateFromString("dd MM yyyy HH:mm") with
            | Some d -> d.AddHours(-6.)
            | None ->
                raise
                <| System.Exception(sprintf "cannot parse datePub %s" PubDateT)

        let EndDateT =
            match t.QuerySelector("td:nth-child(6)") with
            | null ->
                raise
                <| System.NullReferenceException(sprintf "EndDateT not found in %s" url)
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let dateEnd =
            match (dd EndDateT).DateFromString("dd MM yyyy HH:mm") with
            | Some d -> d.AddHours(-6.)
            | None ->
                raise
                <| System.Exception(sprintf "cannot parse dateEnd %s" PubDateT)

        let ten: GosYakutRec =
            { Href = Href
              PurNum = PurNum
              PurName = PurName
              CusName = CusName
              CusUrl = HrefCus
              DatePub = datePub
              DateEnd = dateEnd
              PwayName = PwName
              Nmck = Nmck }

        try
            let T =
                TenderGosYakut(
                    set,
                    ten,
                    76,
                    "«WEB-Маркет закупок» Республики Саха (Якутия)",
                    "http://market.goszakazyakutia.ru"
                )

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, url)

        ()
