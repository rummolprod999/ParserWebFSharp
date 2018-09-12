namespace ParserWeb

open AngleSharp
open AngleSharp
open AngleSharp
open AngleSharp.Dom
open AngleSharp.Parser.Html
open OpenQA.Selenium
open System
open MySql.Data.MySqlClient
open System.Collections.Generic
open System.Linq
open System.Text.RegularExpressions
open System.Threading
open System.Threading.Tasks
open TypeE

type ParserTPlusParall(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let count = 1000
    let TypeFz = 96
    let strtPg = sprintf "https://tenderplus.kz/zakupki?page=%d&lot-sort=-pub_date"
    member val locker = new Object()
    member val listTenders = new Queue<TPlusRec>()
    
    override this.Parsing() =
        for i in 1..count do
            try 
                this.ParserPage <| strtPg i
            with ex -> Logging.Log.logger ex
    
    member private this.ParserPage(url : string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let mutable tens = documents.QuerySelectorAll("div.tenders-list > div[data-key]")
            for t in tens do
                try 
                    let task = new Task(fun () -> this.AddTenderToList t)
                    task.Start()
                //task.Wait()
                with ex -> Logging.Log.logger ex
            let cn = tens.Length - 1
            let cons = new Task(fun () -> this.ConsumerTender cn)
            cons.Start()
            cons.Wait()
            ()
        ()
    
    member private this.AddTenderToList(t : IElement) =
        let PurName =
            match t.QuerySelector("h2.title a") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let Href =
            match t.QuerySelector("h2.title a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        
        let purNum =
            match Href.Get1FromRegexp(@"/(\d+)$") with
            | Some e -> e
            | None -> ""
        
        let region =
            match t.QuerySelector("h2.title + p") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()
        
        let Nmck =
            match t.QuerySelector("div.amount") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexDeleteWhitespace()
        
        let dateT =
            match t.QuerySelector("div.date") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()
        
        let dateEndT =
            match dateT with
            | Tools.RegexMatch2 @"Завершение:\s(\d{2}\.\d{2}\.\d{2})\s(\d{1,2}:\d{2})" (gr1, gr2) -> 
                Some(sprintf "%s %s" gr1 gr2)
            | _ -> None
        
        let dateEnd =
            match dateEndT with
            | None -> DateTime.MinValue
            | Some e -> 
                match e.DateFromString("dd.MM.yy H:mm") with
                | Some d -> d
                | None -> DateTime.MinValue
        
        let datePubT =
            match dateT with
            | Tools.RegexMatch2 @"Опубликован:\s(\d{2}\.\d{2}\.\d{2})\s(\d{1,2}:\d{2})" (gr1, gr2) -> 
                Some(sprintf "%s %s" gr1 gr2)
            | _ -> None
        
        let datePub =
            match datePubT with
            | None -> DateTime.MinValue
            | Some e -> 
                match e.DateFromString("dd.MM.yy H:mm") with
                | Some d -> d
                | None -> DateTime.MinValue
        
        let status =
            match t.QuerySelector("div.status") with
            | null -> ""
            | ur -> ur.TextContent.Replace("Статус:", "").Trim()
        
        let mutable Page = ""
        let exist = this.TenderExister purNum datePub dateEnd status
        match exist with
        | NoExist -> Page <- Download.DownloadString Href
        | Exist -> ()
        let tn =
            { Href = Href
              PurNum = purNum
              PurName = PurName
              DatePub = datePub
              DateEnd = dateEnd
              Nmck = Nmck
              Status = status
              region = region
              Page = Page
              Exist = exist }
        Monitor.Enter(this.locker)
        if this.listTenders.Count >= 5 then Monitor.Wait(this.locker) |> ignore
        this.listTenders.Enqueue(tn)
        Monitor.PulseAll(this.locker)
        Monitor.Exit(this.locker)
        ()
    
    member private this.ConsumerTender(num : int) =
        for i in 1..num do
            Monitor.Enter(this.locker)
            if this.listTenders.Count < 1 then Monitor.Wait(this.locker) |> ignore
            let t = this.listTenders.Dequeue()
            try 
                this.TenderChecker t
            with ex -> Logging.Log.logger ex
            Monitor.PulseAll(this.locker)
            Monitor.Exit(this.locker)
    
    member private this.TenderChecker(tn : TPlusRec) =
        match tn.PurNum with
        | "" -> raise <| System.NullReferenceException(sprintf "PurNum not found in %s" tn.Href)
        | _ -> ()
        match tn.DatePub with
        | a when a = DateTime.MinValue -> 
            raise <| System.NullReferenceException(sprintf "PubDate not found in %s" tn.Href)
        | _ -> ()
        match tn.PurName with
        | "" -> raise <| System.NullReferenceException(sprintf "PurName not found in %s" tn.Href)
        | _ -> ()
        match tn.Exist with
        | Exist -> ()
        | NoExist -> 
            match tn.Page with
            | "" -> raise <| System.NullReferenceException(sprintf "Page not found in %s" tn.Href)
            | _ -> ()
            try 
                let T = TenderTplus(set, tn, TypeFz, "TenderPlus", "https://tenderplus.kz", tn.Page)
                T.Parsing()
            with ex -> Logging.Log.logger (ex, tn.Href)
    
    member private this.TenderExister (purNum : string) (datePub : DateTime) (dateEnd : DateTime) (status : string) : Exist =
        match purNum with
        | "" -> Exist
        | _ -> 
            match datePub with
            | x when x = DateTime.MinValue -> Exist
            | _ -> 
                use con = new MySqlConnection(stn.ConStr)
                con.Open()
                let selectTend =
                    sprintf 
                        "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND doc_publish_date = @doc_publish_date AND notice_version = @notice_version" 
                        stn.Prefix
                let cmd : MySqlCommand = new MySqlCommand(selectTend, con)
                cmd.Prepare()
                cmd.Parameters.AddWithValue("@purchase_number", purNum) |> ignore
                cmd.Parameters.AddWithValue("@type_fz", TypeFz) |> ignore
                cmd.Parameters.AddWithValue("@end_date", dateEnd) |> ignore
                cmd.Parameters.AddWithValue("@doc_publish_date", datePub) |> ignore
                cmd.Parameters.AddWithValue("@notice_version", status) |> ignore
                let reader : MySqlDataReader = cmd.ExecuteReader()
                if reader.HasRows then 
                    reader.Close()
                    Exist
                else 
                    reader.Close()
                    NoExist
