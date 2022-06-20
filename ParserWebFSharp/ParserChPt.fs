namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System
open TypeE

type ParserChPt(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let url = "https://tp.chpt.ru/"

    override this.Parsing() =
        let Page = Download.DownloadString url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents.QuerySelectorAll("ul.groups_list li:not(.head)")

            for t in tens do
                try
                    this.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

            ()

        ()

    member private this.ParsingTender (t: IElement) (url: string) =
        let PurNum =
            match t.QuerySelector("div:nth-child(1) a") with
            | null ->
                raise
                <| NullReferenceException(sprintf "PurNum not found in %s" url)
            | ur -> ur.TextContent.Trim()

        let PurName =
            match t.QuerySelector("div.name a") with
            | null ->
                raise
                <| NullReferenceException(sprintf "PurName not found in %s" url)
            | ur -> ur.TextContent.Trim()

        let HrefT =
            match t.QuerySelector("div:nth-child(2) a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()

        let Href =
            sprintf "https://tp.chpt.ru%s" HrefT

        let Nmck =
            match t.QuerySelector("div:nth-child(5) span strong") with
            | null -> ""
            | ur -> ur.TextContent.RegexDeleteWhitespace().Trim()

        let Currency = "ք"

        let EndDateT1 =
            match t.QuerySelector("div:nth-child(6) span:nth-child(1)") with
            | null ->
                raise
                <| NullReferenceException(sprintf "EndDateT1 not found in %s" url)
            | ur -> ur.TextContent.RegexCutWhitespace().Trim()

        let EndDateT2 =
            match t.QuerySelector("div:nth-child(6) span:nth-child(2)") with
            | null ->
                raise
                <| NullReferenceException(sprintf "EndDateT2 not found in %s" url)
            | ur -> ur.TextContent.RegexCutWhitespace().Trim()

        let EndDateT =
            sprintf "%s %s" EndDateT1 EndDateT2

        let dateEnd =
            match EndDateT.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d
            | None ->
                raise
                <| Exception(sprintf "cannot parse dateEnd %s" EndDateT)

        let datePub = DateTime.Now

        let ten: ChPtRec =
            { Href = Href
              PurNum = PurNum
              PurName = PurName
              DatePub = datePub
              DateEnd = dateEnd
              Nmck = Nmck
              Currency = Currency }

        try
            let T =
                TenderChPt(set, ten, 84, "ООО «Чебаркульская птица»", "https://tp.chpt.ru/")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, url)

        ()
