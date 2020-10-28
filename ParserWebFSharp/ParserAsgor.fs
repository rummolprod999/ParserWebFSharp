namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Text.RegularExpressions
open TypeE

type ParserAsgor(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let url = "https://etp.asgor.su/admin/TendProcUserSRO2.aspx?Frame=1"
    
    override this.Parsing() =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s -> 
            let parser = HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("#proc-list div.proc")
            for t in tens do
                try 
                    this.ParsingTender t url
                with ex -> Logging.Log.logger ex
            ()
    
    member private this.ParsingTender (t : IElement) (url : string) =
        let PurNum =
            match t.QuerySelector("div.left-info div.num") with
            | null -> raise <| System.NullReferenceException(sprintf "PurNum not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let PurName =
            match t.QuerySelector("div.main-info h2 a") with
            | null -> raise <| System.NullReferenceException(sprintf "PurName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let HrefT =
            match t.QuerySelector("div.main-info h2 a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        
        let mutable Href =
            match HrefT.Get1FromRegexp @"\.(/admin/.+?)""" with
            | Some x -> x.Trim()
            | None -> raise <| System.NullReferenceException(sprintf "Href not found in %s %s" url HrefT)
        
        let PubDateT =
            match t.QuerySelector("div.date div:contains('Опубликовано:') + div") with
            | null -> raise <| System.NullReferenceException(sprintf "PubDateT not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let dateCons = (fun (s : string) -> s.Replace("| ", "")) >> (fun (s : string) -> s.ReplaceDateAsgor())
        
        let datePub =
            match (dateCons PubDateT).DateFromString("d MM yyyy HH:mm") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "cannot parse datePub %s" PubDateT)
        
        let EndDateT =
            match t.QuerySelector("div.date div:contains('Рассмотрение заявок:') + div") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let dateEnd =
            match (dateCons EndDateT).DateFromString("d MM yyyy HH:mm") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "cannot parse dateEnd %s" EndDateT)
        
        Href <- sprintf "https://etp.asgor.su%s" Href
        let OrgName =
            match t.QuerySelector("span:contains('Организатор:') + span") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let CusName =
            match t.QuerySelector("span:contains('Заказчик:') + span") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let Status =
            match t.QuerySelector("div.right-info div.proc-status") with
            | null -> ""
            | ur -> ur.TextContent.Replace("Статус:", "").Trim()
        
        let NameLotsT = t.QuerySelectorAll("div.lots div.lot")
        let mutable Lots = Set.empty
        NameLotsT |> Seq.iter (fun x -> Lots <- Lots.Add(x.TextContent.Trim()))
        let NmckT =
            match t.QuerySelector("span.price-initial nobr") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let mutable Nmck =
            match NmckT.Get1FromRegexp @"([\d \.]+)\s" with
            | Some x -> Regex.Replace(x.Replace(",", ".").Trim(), @"\s+", "")
            | None -> ""
        
        let pwName =
            match t.QuerySelector("div.main-info div.type") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let ten =
            { Href = Href
              PurNum = PurNum
              PurName = PurName
              OrgName = OrgName
              CusName = CusName
              DatePub = datePub
              DateEnd = dateEnd
              status = Status
              PwayName = pwName
              Nmck = Nmck
              NameLots = Lots }
        
        try 
            let T = TenderAsgor(set, ten, 72, "ООО \"АСГОР\"", "https://etp.asgor.su/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
