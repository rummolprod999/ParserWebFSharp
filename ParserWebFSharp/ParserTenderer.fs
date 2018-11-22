namespace ParserWeb

open HtmlAgilityPack
open System
open System.Linq
open TypeE
open AngleSharp
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Collections.Generic

type ParserTenderer(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 200
    let spage = "http://www.tenderer.ru/kom/"
    let listPathTenders = new List<string>()
    
    override this.Parsing() =
        this.GetPathList spage
        for url in listPathTenders do
            try 
                this.ParserUrl url
            with ex -> Logging.Log.logger ex
    
    member private this.GetPathList stpage =
        let Page = Download.DownloadString1251CookiesTenderer stpage
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get start page", stpage)
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tensurl = documents.QuerySelectorAll("div.page_tenders li a[href $='index.html']")
            tensurl |> Seq.iter (fun x -> listPathTenders.Add(x.GetAttribute("href").Trim()))
            ()
    
    member private this.ParserUrl url =
        this.ParserPage url
        let mutable next = true
        for i in 2..pageC do
            try 
                let urlT = url.Replace("index.html", sprintf "index%d.html" i)
                if next then next <- this.NextPage urlT
            with ex -> 
                Logging.Log.logger ex
                next <- false
        ()
    
    member private this.NextPage url : bool =
        let Page = Download.DownloadString1251CookiesTenderer url
        match Page with
        | null | "" -> false
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let mutable tens = documents.QuerySelectorAll("div.tender_table ul li")
            if tens.Length = 0 then false
            else 
                for t in tens.Skip(1) do
                    try 
                        this.ParserTender t url
                    with ex -> Logging.Log.logger ex
                true
    
    member private this.ParserPage url =
        let Page = Download.DownloadString1251CookiesTenderer url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("div.tender_table ul li").Skip(1)
            for t in tens do
                try 
                    this.ParserTender t url
                with ex -> Logging.Log.logger ex
    
    member private this.ParserTender (t : IElement) (url : string) =
        let mutable href =
            match t.QuerySelector("div:nth-child(3) > a:nth-child(1)") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        match href with
        | "" -> raise <| System.NullReferenceException(sprintf "Href not found in %s" url)
        | x when not (x.StartsWith("http://")) -> href <- sprintf "http://www.tenderer.ru%s" href
        | _ -> ()
        let purName =
            match t.QuerySelector("div:nth-child(3) > a:nth-child(1)") with
            | null -> raise <| System.NullReferenceException(sprintf "PurName not found in %s" href)
            | ur -> ur.TextContent.Trim()
        
        let purNum =
            match t.QuerySelector("div:nth-child(1)") with
            | null -> raise <| System.NullReferenceException(sprintf "PurNum not found in %s" href)
            | ur -> ur.TextContent.Trim()
        
        let pubDateT =
            match t.QuerySelector("div:nth-child(4)") with
            | null -> raise <| System.NullReferenceException(sprintf "pubDateT not found in %s" href)
            | ur -> ur.TextContent.Trim()
        
        let datePub =
            match pubDateT.DateFromString("dd-MM-yyyy") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "can not parse datePub %s" pubDateT)
        
        let biddingDateT =
            match t.QuerySelector("div:nth-child(3) span:contains('Дата проведения')") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let biddingDateTT =
            match biddingDateT.Get1FromRegexp @"(\d{2}.\d{2}.\d{4})" with
            | Some x -> x.Trim()
            | None -> ""
        
        let dateBidding =
            match biddingDateTT.DateFromString("dd.MM.yyyy") with
            | Some d -> d
            | None -> DateTime.MinValue
        
        let endDateT =
            match t.QuerySelector("div:nth-child(3) span:contains('Окончание приема заявок')") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let endDateTT =
            match endDateT.Get1FromRegexp @"(\d{2}.\d{2}.\d{4})" with
            | Some x -> x.Trim()
            | None -> ""
        
        let mutable dateEnd =
            match endDateTT.DateFromString("dd.MM.yyyy") with
            | Some d -> d
            | None -> dateBidding
        
        match dateEnd with
        | x when x = DateTime.MinValue -> dateEnd <- datePub
        | _ -> ()
        let regionName =
            match t.QuerySelector("div:nth-child(3) span:contains('Регион проведения тендера') a") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let orgName =
            match t.QuerySelector("div:nth-child(3) span:contains('Закупочная организация') a") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let PriceT =
            match t.QuerySelector("div:nth-child(3) span:contains('Цена контракта')") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let Nmck =
            match PriceT.Get1FromRegexp @"Цена контракта:\s*([\d\s\.,]+)\s*руб" with
            | Some x -> x.RegexDeleteWhitespace().Replace(",", ".").Trim()
            | None -> ""
        
        let currency =
            if Nmck <> "" then "руб."
            else ""
        
        let ten =
            { Href = href
              PurNum = purNum
              PurName = purName
              DatePub = datePub
              DateEnd = dateEnd
              DateBidding = dateBidding
              Nmck = Nmck
              Currency = currency
              OrgName = orgName
              RegionName = regionName }
        
        try 
            let T = TenderTenderer(set, ten, 129, "TENDERER.RU", "http://www.tenderer.ru")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
        ()
