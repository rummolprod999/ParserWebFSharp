namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open OpenQA.Selenium.Chrome
open System
open System.Text.RegularExpressions
open TypeE

type ParserStroyTorgi(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let _ = TimeSpan.FromSeconds(120.)
    let url =
        "https://stroytorgi.ru/trades/page20?category=0&filter_category=0&filter_sum_min=0&filter_sum_max=9999999999999"
    let options = ChromeOptions()
    let CreateDriver opt = lazy (new ChromeDriver("/usr/local/bin", opt))
    let mutable _ = null
    
    do 
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
    
    (*driver <- (CreateDriver options).Force()
        driver.Manage().Cookies.DeleteAllCookies()
        driver.Manage().Timeouts().PageLoad <- timeoutB*)
    override this.Parsing() =
        try 
            try 
                this.ParserList()
            with ex -> Logging.Log.logger ex
        finally
            ()
    
    member private this.ParserList() =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s -> 
            let parser = HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("ul.table-auction li[data-href^= '/trades/']")
            for t in tens do
                try 
                    this.ParsingTender t
                with ex -> Logging.Log.logger ex
            ()
    
    member private this.ParsingTender(t : IElement) =
        let UrlT = t.GetAttribute("data-href").Trim()
        match UrlT with
        | "" | null -> raise <| System.NullReferenceException(sprintf "UrlT not found in %s" url)
        | _ -> ()
        let Url = sprintf "https://stroytorgi.ru%s" UrlT
        
        let mutable purNum =
            match t.QuerySelector("div:nth-child(1) div:nth-child(2)") with
            | null -> raise <| System.NullReferenceException(sprintf "purNum not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let category =
            match t.QuerySelector("div:nth-child(2) div:nth-child(2) p") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let purNameT = t.QuerySelectorAll("div:nth-child(2) div:nth-child(2) ul li a")
        let purName = ref category
        purNameT |> Seq.iter (fun i -> purName := sprintf "%s %s" !purName (i.TextContent.Trim()))
        let orgName =
            match t.QuerySelector("div.table-auction_group__organizer div:nth-child(2) div") with
            | null -> 
                match t.QuerySelector("div.table-auction_group__organizer div:nth-child(2)") with
                | null -> ""
                | ur -> ur.TextContent.Trim()
            | ur -> ur.TextContent.Trim()
        
        let status =
            match t.QuerySelector("div.table-auction_group__protocol div:nth-child(2) div") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let PriceT =
            match t.QuerySelector("div.table-auction_group__price div:nth-child(2)") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let mutable Price =
            match PriceT.Get1FromRegexp @"([\d+| ]+)" with
            | Some x -> x.Trim()
            | None -> ""
        
        Price <- Regex.Replace(Price.ToString(), @"\s+", "")
        let Currency =
            match PriceT.Get1FromRegexp @"\s([^\s]+)$" with
            | Some x -> x.Trim()
            | None -> ""
        
        let ten =
            { Url = Url
              PurNum = purNum
              PurName = !purName
              OrgName = orgName
              Status = status
              Price = Price
              Currency = Currency }
        
        try 
            let T = TenderStroyTorgi(set, ten, 69, "ЭТП «СтройТорги»", "https://stroytorgi.ru/trades")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
        ()
