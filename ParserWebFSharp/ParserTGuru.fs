namespace ParserWeb

open System
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html

type ParserTGuru(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 2000
    
    override this.Parsing() =
        for i in 1..pageC do
            try 
                let url = sprintf "http://www.tenderguru.ru/kommercheskie_tendery?region=&cat=&page=%d" i
                this.ParsingPage url
            with ex -> Logging.Log.logger ex
    
    member private this.ParsingPage(url : string) =
        let Page = Download.DownloadString1251Cookies url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("div.navigation_filter + table > tbody > tr")
            for t in tens do
                try 
                    this.ParsingTenderAngle t url
                with ex -> Logging.Log.logger ex
        ()
    
    member private this.ParsingTenderAngle (t : IElement) (url : string) =
        let href =
            match t.QuerySelector("td a.tender_link") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        match href with
        | "" -> raise <| System.NullReferenceException(sprintf "Href not found in %s" url)
        | x when not (x.StartsWith("http://www.tenderguru.ru")) -> 
            raise <| System.NullReferenceException(sprintf "Href not contains http://www.tenderguru.ru in %s" url)
        | _ -> ()
        let PurName =
            match t.QuerySelector("td a.tender_link") with
            | null -> raise <| System.NullReferenceException(sprintf "PurName not found in %s" href)
            | ur -> ur.TextContent.Trim()
        
        let text =
            match t.QuerySelector("td") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let purNum =
            match text.Get1FromRegexp @"Номер тендера:\s(\d+)" with
            | Some x -> x.Trim()
            | None -> raise <| System.NullReferenceException(sprintf "PurNum not found in %s" href)
        
        let regionName =
            match text.Get1FromRegexp @"Регион:\s(.+?)\s\(" with
            | Some x -> x.Trim()
            | None -> ""
        
        let pubDateT =
            match text.Get1FromRegexp @"^(\d{2}-\d{2}-\d{4})" with
            | Some x -> x.Trim()
            | None -> raise <| System.NullReferenceException(sprintf "pubDateT not found in %s" href)
        
        let datePub =
            match pubDateT.DateFromString("dd-MM-yyyy") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "cannot parse datePub %s" pubDateT)
        
        let endDateT =
            match text.Get1FromRegexp @"Осталось\s*(\d+)\s*дней" with
            | Some x -> Int32.Parse(x.Trim())
            | None -> 0
        
        let dateEnd = datePub.AddDays(float endDateT)
        
        let OrgName =
            match text.Get1FromRegexp @"Организация, проводящая закупку:\s*(.+?)\s*\(" with
            | Some x -> x.Trim()
            | None -> ""
        
        let Nmck =
            match text.Get1FromRegexp @"Цена контракта:\s*([\d\s\.,]+)\s*рублей " with
            | Some x -> x.RegexDeleteWhitespace().Replace(",", ".").Trim()
            | None -> ""
        
        let ten =
            { Href = href
              PurNum = purNum
              PurName = PurName
              DatePub = datePub
              DateEnd = dateEnd
              Nmck = Nmck
              OrgName = OrgName
              RegionName = regionName }
        
        try 
            let T = TenderTGuru(set, ten, 100, "TenderGURU", "http://www.tenderguru.ru")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
        ()
