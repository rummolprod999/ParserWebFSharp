namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open TypeE

type ParserNeft(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let url = "https://zakupki.nefteavtomatika.ru/zakupki/list?active=1&page="
    
    override this.Parsing() =
        for i in 1..20 do
            try 
                let urlT = sprintf "%s%d" url i
                this.ParserPage urlT
            with ex -> Logging.Log.logger ex
    
    member private this.ParserPage(urlT : string) =
        let Page = Download.DownloadString urlT
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get start page", urlT)
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("table.table tr.registerBox")
            for t in tens do
                try 
                    this.ParsingTender t
                with ex -> Logging.Log.logger ex
            ()
    
    member private this.GetDateS (input : string) (regex : string) : string option =
        match input with
        | Tools.RegexMatch1 regex gr1 -> Some(gr1)
        | _ -> None
    
    member private this.ParsingTender(t : IElement) =
        let urlT =
            match t.QuerySelector("td a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        match urlT with
        | null | "" -> raise <| System.NullReferenceException(sprintf "urlT not found in %s" urlT)
        | _ -> ()
        let url = sprintf "https://zakupki.nefteavtomatika.ru%s" urlT
        let purNameT = t.QuerySelector("td a b")
        match purNameT with
        | null -> raise <| System.NullReferenceException(sprintf "purName not found in %s" url)
        | _ -> ()
        let purName = purNameT.TextContent.Trim()
        let purNumT = t.QuerySelector("td:nth-child(1) div b")
        match purNumT with
        | null -> raise <| System.NullReferenceException(sprintf "purNum not found in %s" url)
        | _ -> ()
        let purNum = purNumT.TextContent.Trim().Replace("№", "")
        let orgNameT = t.QuerySelector("td:nth-child(1) div:nth-child(2)")
        
        let orgName =
            match orgNameT with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let DateT = t.QuerySelector("td:nth-child(3) div")
        match DateT with
        | null -> raise <| System.NullReferenceException(sprintf "Date not found in %s" url)
        | _ -> ()
        let DateS = DateT.TextContent.Trim()
        
        let pubDateS =
            match this.GetDateS DateS @"Дата начала подачи заявок.*(\d{2}.\d{2}.\d{4})" with
            | Some dtP -> dtP
            | None -> raise <| System.Exception(sprintf "cannot apply regex to datePub %s" url)
        
        let datePub =
            match pubDateS.DateFromString("dd.MM.yyyy") with
            | Some d -> d.AddHours(-2.)
            | None -> raise <| System.Exception(sprintf "cannot parse datePub %s" pubDateS)
        
        let endDateS =
            match this.GetDateS DateS @"Дата окончания подачи заявок.*(\d{2}.\d{2}.\d{4} \d{2}:\d{2})" with
            | Some dtP -> dtP
            | None -> raise <| System.Exception(sprintf "cannot apply regex to dateEnd %s" url)
        
        let dateEnd =
            match endDateS.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d.AddHours(-2.)
            | None -> raise <| System.Exception(sprintf "cannot parse dateEnd %s" endDateS)
        
        let ten =
            { NeftRec.Href = url
              PurNum = purNum
              PurName = purName
              OrgName = orgName
              DatePub = datePub
              DateEnd = dateEnd }
        
        try 
            let T = TenderNeft(set, ten)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
        ()
