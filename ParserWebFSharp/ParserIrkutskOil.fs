namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html

type ParserIrkutskOil(stn: Settings.T) =
    inherit Parser()
    let _ = stn

    let url =
        "https://lkk.irkutskoil.ru/active-tenders/list"

    override this.Parsing() =
        let Page =
            Download.DownloadStringIrkutsk url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get start page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents.QuerySelectorAll("div.tender.row")

            for t in tens do
                try
                    this.ParsingTender t
                with
                    | ex -> Logging.Log.logger ex

            ()

    member private this.ParsingTender(t: IElement) =
        let urlT =
            match t.QuerySelector("div a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()

        match urlT with
        | "" -> Logging.Log.logger ("cannot find href on page ", url)
        | url ->
            try
                let urlN = "https://lkk.irkutskoil.ru" + url
                let T = TenderIrkutskOil(stn, urlN)
                T.Parsing()
            with
                | ex -> Logging.Log.logger (ex, url)

        ()
