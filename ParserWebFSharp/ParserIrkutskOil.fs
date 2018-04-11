namespace ParserWeb
open System
open System.Linq
open AngleSharp.Dom
open AngleSharp.Parser.Html

type ParserIrkutskOil(stn : Settings.T) = 
    inherit Parser("«Иркутская нефтяная компания»","https://tenders.irkutskoil.ru/tenders.php")
    let set = stn
    let url = "https://tenders.irkutskoil.ru/tenders.php"
    static member val tenderCount = ref 0
    override this.Parsing() = 
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get start page", url)
        | s ->
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("table.lot_list tr.Info")
            for t in tens do
                try
                    this.ParsingTender t
                with ex -> Logging.Log.logger ex
                    
            ()
        
    member private this.ParsingTender(t: IElement) =
        let  urlT = match t.QuerySelector("td a") with
                    | null -> ""
                    | ur -> ur.GetAttribute("href").Trim()
        printfn "%s" urlT
        ()      
