namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open MySql.Data.MySqlClient
open System
open System.Collections.Generic
open System.Linq
open System.Threading
open System.Threading.Tasks
open TypeE

type ParserRostendTask(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let count = 500
    let strtPg = "http://rostender.info/tender?pg="
    member val locker = new Object()
    member val listTenders = new Queue<RosTendRec>()

    override this.Parsing() =
        for i in 1..count do
            try
                this.ParserPage <| sprintf "%s%d" strtPg i
            with ex -> Logging.Log.logger ex

    member private this.ParserPage(url : string) =
        let Page = Download.DownloadString1251Bot url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let mutable tens = documents.QuerySelectorAll("table.b-new-tenders-table tbody tr")
            if tens.Length > 0 then
                let tensN = tens.Skip(1)
                this.ThreadWorker tensN
            ()

    member private this.ThreadWorker(tensN : IEnumerable<_>) =
        let listElem = new List<_>(tensN)
        while listElem.Count > 10 do
            let ls = listElem.Take(10)
            listElem.RemoveAllFromList(ls)
            this.Worker(new List<_>(ls))
            ()
        this.Worker(new List<_>(listElem))

    member private this.Worker(l : List<_>) =
        Task.Factory.StartNew(fun () -> Parallel.ForEach(l, this.TaskerParall) |> ignore) |> ignore
        if l.Count > 0 then
            //use con = new MySqlConnection(stn.ConStr)
            let cons = new Task(fun () -> this.ConsumerTender l.Count)
            cons.Start()
            cons.Wait()
        ()

    member private this.TaskerParall(t : IElement) =
            try
                this.AddTenderToList t
            with ex -> Logging.Log.logger ex

    member private this.AddTenderToList(t : IElement) =
        let PurName =
            match t.QuerySelector("h3 a") with
            | null -> ""
            | ur -> ur.TextContent.Trim()

        let HrefT =
            match t.QuerySelector("h3 a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()

        let Href = sprintf "http://rostender.info%s" HrefT

        let PurNumT =
            match t.QuerySelector("div.b-new-tenders-table-firstcell-overal") with
            | null -> ""
            | ur -> ur.TextContent.Trim()

        let PurNum =
            match PurNumT.Get1FromRegexp @"Тендер\s*№\s*(\d+)" with
            | Some x -> x.Trim()
            | None -> ""

        let mutable PubDateT =
            match t.QuerySelector("div.b-new-tenders-table-firstcell-overal span") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        PubDateT <- match PubDateT.Get1FromRegexp @"(\d{2}\.\d{2}\.\d{2})" with
                    | Some x -> x.Trim()
                    | None -> ""
        let datePub =
            match PubDateT.DateFromString("dd.MM.yy") with
            | Some d -> d
            | None -> DateTime.MinValue

        let region =
            match t.QuerySelector("div.b-new-tenders-table-firstcell-reg a") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let delivPlace =
            match t.QuerySelector("td:nth-child(2) strong") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let NmckT =
            match t.QuerySelector("td:nth-child(3)") with
            | null -> ""
            | ur -> ur.TextContent.Trim()

        let Nmck =
            match NmckT.Get1FromRegexp @"([\d\s,\.]+)\s+.+" with
            | Some x -> x.Trim().RegexDeleteWhitespace()
            | None -> ""

        let Currency =
            match NmckT.Get1FromRegexp @"([^\d\s]+)$" with
            | Some x -> x.Trim().RegexDeleteWhitespace()
            | None -> ""

        let Page = Download.DownloadString1251Bot Href

        let tn =
            { Href = Href
              PurNum = PurNum
              PurName = PurName
              DatePub = datePub
              Region = region
              Nmck = Nmck
              DelivPlace = delivPlace
              Currency = Currency
              Page = Page }
        Monitor.Enter(this.locker)
        if this.listTenders.Count >= 5 then Monitor.Wait(this.locker) |> ignore
        this.listTenders.Enqueue(tn)
        Monitor.PulseAll(this.locker)
        Monitor.Exit(this.locker)
        ()

    member private this.ConsumerTender(num : int) =
        use con = new MySqlConnection(stn.ConStr)
        con.Open()
        for i in 1..num do
            Monitor.Enter(this.locker)
            if this.listTenders.Count < 1 then Monitor.Wait(this.locker) |> ignore
            let t = this.listTenders.Dequeue()
            try
                this.TenderChecker t con
            with ex -> Logging.Log.logger ex
            Monitor.PulseAll(this.locker)
            Monitor.Exit(this.locker)

    member private this.TenderChecker(tn : RosTendRec) (con: MySqlConnection) =
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
        match tn.Page with
        | "" -> raise <| System.NullReferenceException(sprintf "Page not found in %s" tn.Href)
        | _ -> ()
        let parser = new HtmlParser()
        let documents = parser.Parse(tn.Page)

        let EndDateT =
            match documents.QuerySelector("td:contains('Окончание приёма предложений:') + td strong") with
            | null -> ""
            | ur ->
                ur.TextContent.Trim().Replace("«", "").Replace("»", "").Replace("г.", "").RegexCutWhitespace()
                  .ReplaceDate().RegexDeleteWhitespace()

        let dateEnd =
            match EndDateT.DateFromString("dd.MM.yyyy") with
            | Some d -> d
            | None -> DateTime.MinValue

        try
            let T = TenderRosTendParall(set, tn, 82, "ООО Тендеры и закупки", "http://rostender.info", dateEnd, "", con)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, tn.Href)
        ()
