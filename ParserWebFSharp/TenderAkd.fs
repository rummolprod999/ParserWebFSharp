namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open MySql.Data.MySqlClient
open System
open System.Data
open System.Linq
open TypeE

type TenderAkd(stn : Settings.T, urlT : string, purNum : string) = 
    inherit Tender("Электронная торговая площадка для проведения торгов - Аукционный Конкурсный Дом", 
                   "http://www.a-k-d.ru/tender")
    let settings = stn
    let typeFz = 33
    static member val tenderCount = ref 0
    
    override this.Parsing() = 
        let Page = Download.DownloadString urlT
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", urlT)
        | s -> this.ParserPage(s)
        ()
    
    member private this.ParserPage(p : string) = 
        let parser = new HtmlParser()
        let doc = parser.Parse(p)
        let pubDateT = doc.QuerySelector("th:contains('Публикация электронной процедуры') + td > span")
        match pubDateT with
        | null -> raise <| System.NullReferenceException(sprintf "pubDate not found in %s" urlT)
        | _ -> ()
        let pubDateS = pubDateT.TextContent.Trim()
        
        let datePub = 
            match pubDateS.DateFromStringRus("dd MMMM yyyy 'г.'") with
            | Some d -> d
            | None -> 
                match pubDateS.DateFromStringRus("dd MMMM yyyy 'г.' HH:mm") with
                | Some d -> d
                | None -> raise <| System.Exception(sprintf "can not parse datePub %s, %s" pubDateS urlT)
        
        let endDateT = doc.QuerySelector("th:contains('Окончание приема заявок') + td > span")
        
        let endDateS = 
            match endDateT with
            | null -> ""
            | _ -> endDateT.TextContent.Trim()
        
        let endDate = 
            match endDateS.DateFromStringRus("dd MMMM yyyy 'г.' HH:mm") with
            | Some d -> d
            | None -> 
                match endDateS.DateFromStringRus("dd MMMM yyyy 'г.'") with
                | Some d -> d
                | None -> DateTime.MinValue
        
        let scoringDateT = doc.QuerySelector("th:contains('Дата окончания') + td > span")
        
        let scoringDateS = 
            match scoringDateT with
            | null -> ""
            | _ -> scoringDateT.TextContent.Trim()
        
        let scoringDate = 
            match scoringDateS.DateFromStringRus("dd MMMM yyyy 'г.' HH:mm") with
            | Some d -> d
            | None -> 
                match scoringDateS.DateFromStringRus("dd MMMM yyyy 'г.'") with
                | Some d -> d
                | None -> DateTime.MinValue
        
        let biddingDateT = doc.QuerySelector("th:contains('Начало') + td > span")
        
        let biddingDateS = 
            match biddingDateT with
            | null -> ""
            | _ -> biddingDateT.TextContent.Trim()
        
        let biddingDate = 
            match biddingDateS.DateFromStringRus("dd MMMM yyyy 'г.' HH:mm") with
            | Some d -> d
            | None -> 
                match biddingDateS.DateFromStringRus("dd MMMM yyyy 'г.'") with
                | Some d -> d
                | None -> DateTime.MinValue
        let dateUpd = datePub
        
        ()
