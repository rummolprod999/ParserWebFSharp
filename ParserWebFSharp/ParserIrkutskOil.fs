namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html

type ParserIrkutskOil(stn : Settings.T) = 
    inherit Parser()
    let _ = stn
    let url = "https://tenders.irkutskoil.ru/tenders.php"
    
    override this.Parsing() = 
        let Page = Download.DownloadStringIrkutsk url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get start page", url)
        | s -> 
            let parser = HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("table.lot_list tr.Info")
            for t in tens do
                try 
                    this.ParsingTender t
                with ex -> Logging.Log.logger ex
            ()
    
    member private this.ParsingTender(t : IElement) = 
        let urlT = 
            match t.QuerySelector("td a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        match urlT with
        | "" -> Logging.Log.logger ("cannot find href on page ", url)
        | url -> 
            try 
                let T = TenderIrkutskOil(stn, url)
                T.Parsing()
            with ex -> Logging.Log.logger (ex, url)
        ()
