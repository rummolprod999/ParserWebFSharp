namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System
open TypeE

type ParserAero(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 10
    
    override this.Parsing() =
        for i in 1..10 do
            try 
                let url =
                    sprintf 
                        "https://www.aeroflot.ru/ru-ru/about/retail_center/etp/active?page=%d&order_by=field_datetime_range&sort=desc" 
                        i
                this.ParsingPage url
            with ex -> Logging.Log.logger ex
    
    member private this.ParsingPage(url : string) =
        let Page = Download.DownloadStringBot url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("div.view-content table.list tbody tr")
            for t in tens do
                try 
                    this.ParsingTender t url
                with ex -> Logging.Log.logger ex
            ()
    
    member private this.ParsingTender (t : IElement) (url : string) =
        let HrefDocT =
            match t.QuerySelector("td.views-field-title a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        match HrefDocT with
        | "" | null -> raise <| System.NullReferenceException(sprintf "HrefDocT not found in %s" url)
        | _ -> ()
        let HrefDoc = sprintf "https://www.aeroflot.ru%s" HrefDocT
        
        let PurName =
            match t.QuerySelector("td.views-field-title a") with
            | null -> raise <| System.NullReferenceException(sprintf "PurName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let mutable FullN =
            match t.QuerySelector("td.views-field-title") with
            | null -> raise <| System.NullReferenceException(sprintf "FullN not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        FullN <- FullN.Replace(PurName, "")
        let purNum =
            match FullN.Get1FromRegexp @"№\s*(\d+)," with
            | Some x -> x.Trim()
            | None -> raise <| System.NullReferenceException(sprintf "purNum not found in %s %s" url PurName)
        
        let pWay =
            match FullN.Get1FromRegexp @"(.+?)," with
            | Some x -> x.Trim()
            | None -> ""
        
        let status =
            match t.QuerySelector("td:nth-child(4)") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let PubDateT =
            match t.QuerySelector("td:nth-child(2) span") with
            | null -> raise <| System.NullReferenceException(sprintf "PubDateT not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let datePub =
            match PubDateT.DateFromString("dd.MM.yyyy - HH:mm") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "can not parse datePub %s" PubDateT)
        
        let EndDateT =
            match t.QuerySelector("td:nth-child(3) span") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let dateEnd =
            match EndDateT.DateFromString("dd.MM.yyyy - HH:mm") with
            | Some d -> d
            | None -> DateTime.MinValue
        
        let ten =
            { Href = HrefDoc
              PurNum = purNum
              PurName = PurName
              PwayName = pWay
              DatePub = datePub
              DateEnd = dateEnd
              status = status }
        
        try 
            let T = TenderAero(set, ten, 59, "ПАО «Аэрофлот»", "https://www.aeroflot.ru")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
        ()
