namespace ParserWeb

open System
open System.Linq.Expressions
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq
open System.Linq.Expressions
open System.Web
open Tools

type ParserForScience(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 6
    
    override __.Parsing() =
        for i in pageC .. -1 .. 1 do
            try
                let url = sprintf "https://4science.ru/finsupports?view=list&page=%d&sortField=firstPublished&sortOrder=desc" i
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex
    
    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("div#fin-supports-list-inner > a.fin-list").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! href = t.GsnAtrSelfDocWithError "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let href = sprintf "https://4science.ru%s" href
            let! purName = t.GsnDocWithError "div.fin-list-title" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! pwName = t.GsnDocWithError "div.fin-list-label" <| sprintf "pwName not found %s %s " url (t.TextContent)
            let! datePubT = t.GsnDocWithError "div.fin-list-title span.fin-list-date" <| sprintf "datePubT not found %s %s " url (t.TextContent)
            let purName = purName.Replace(datePubT, "").Trim()
            let! datePub = datePubT.DateFromStringDoc ("dd.MM.yyyy", sprintf "datePub not found %s %s " href datePubT)
            let! endDateT = t.GsnDocWithError "div.fin-list-info-in strong" <| sprintf "endDateT not found %s %s " url (t.TextContent)
            let check, numD = Int32.TryParse(endDateT)
            let endDate = DateTime.Now.AddDays(float numD).Date
            let! orgName = t.GsnDocWithError "div.fin-list-info-in-last" <| sprintf "orgName not found %s %s " url (t.TextContent)
            let! nmckT = t.GsnDoc "div.fin-list-info-in:nth-of-type(2) strong"
            let! nmckPer = t.GsnDoc "div.fin-list-info-in:nth-of-type(2)" 
            let check, nmckNum = Double.TryParse(nmckT)
            let nmck = match nmckPer with
                       | x when x.Contains("тыс") -> nmckNum * 1000.0
                       | x when x.Contains("млн") -> nmckNum * 1000000.0
                       | _ -> nmckNum
            let nmck = nmck.ToString()
            let purNum = createMD5(href)
            let tend = {  Href = href
                          DateEnd = endDate
                          DatePub = datePub
                          PurNum = purNum
                          PurName = purName
                          OrgName = orgName
                          Nmck = nmck
                          PwName = pwName}
            let T = TenderForScience(set, tend, 233, "4science", "https://4science.ru/")
            T.Parsing()
            return ""
            }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()