namespace ParserWeb

open AngleSharp
open AngleSharp.Dom
open AngleSharp.Parser.Html
open OpenQA.Selenium
open System
open System.Linq
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
            let parser = new HtmlParser()
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
            match NmckT.Get1FromRegexp @"([\d ,]+)\s" with
            | Some x -> Regex.Replace(x.Replace(",", ".").Trim(), @"\s+", "")
            | None -> ""
        
        printfn "%s" Nmck
