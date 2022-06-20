namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System
open System.Collections.Generic
open System.Linq
open System.Threading
open System.Threading.Tasks
open TypeE

type ParserRostendTask(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let count = 600
    let countRegion = 30

    let strtPg =
        "https://rostender.info/tender?page="

    let strtPgRegion =
        "https://rostender.info/region/"

    member val locker = Object()
    member val listTenders = Queue<RosTendRecNew>()

    override this.Parsing() =
        for i in 1..count do
            try
                this.ParserPage <| sprintf "%s%d" strtPg i
            with
                | ex -> Logging.Log.logger ex

        try
            this.ParserPageRegion <| sprintf "%s" strtPgRegion
        with
            | ex -> Logging.Log.logger ex

    member private this.ParserPageRegion(url: string) =
        let Page =
            Download.DownloadStringUtf8Bot url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let mutable tens =
                documents.QuerySelectorAll("ul.sections-table-subs li a")

            for p in tens do
                let HrefT = p.GetAttribute("href").Trim()

                if HrefT <> null then
                    let Href =
                        sprintf "https://rostender.info%s" HrefT

                    for i in 1..countRegion do
                        try
                            this.ParserPage <| sprintf "%s?page=%d" Href i
                        with
                            | ex -> Logging.Log.logger ex

                ()



        ()

    member private this.ParserPage(url: string) =
        let Page =
            Download.DownloadStringUtf8Bot url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let mutable tens =
                documents.QuerySelectorAll("div.table-constructor div.tender-row__wrapper")

            if tens.Length > 0 then
                let tensN = tens //.Skip(1)
                this.ThreadWorker tensN

            ()

    member private this.ThreadWorker(tensN: IEnumerable<_>) =
        let listElem = List<_>(tensN)

        while listElem.Count > 10 do
            let ls = listElem.Take(10)
            listElem.RemoveAllFromList(ls)
            this.Worker(List<_>(ls))
            ()

        this.Worker(List<_>(listElem))

    member private this.Worker(l: List<_>) =
        Task.Factory.StartNew(fun () -> Parallel.ForEach(l, this.TaskerParall) |> ignore)
        |> ignore

        if l.Count > 0 then
            //use con = new MySqlConnection(stn.ConStr)
            let cons =
                new Task(fun () -> this.ConsumerTender l.Count)

            cons.Start()
            cons.Wait()

        ()

    member private this.TaskerParall(t: IElement) =
        try
            this.AddTenderToList t
        with
            | ex -> Logging.Log.logger ex

    member private this.AddTenderToList(t: IElement) =
        let PurName =
            match t.QuerySelector("div.tender-info a.description") with
            | null -> ""
            | ur -> ur.TextContent.Trim()

        let HrefT =
            match t.QuerySelector("div.tender-info a.description") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()

        let Href =
            sprintf "https://rostender.info%s" HrefT

        let PurNumT =
            match t.QuerySelector("span.tender__number") with
            | null -> ""
            | ur -> ur.TextContent.Trim()

        let PurNum =
            match PurNumT.Get1FromRegexp @"Тендер\s*№\s*(\d+)" with
            | Some x -> x.Trim()
            | None -> ""

        let mutable PubDateT =
            match t.QuerySelector("span.tender__date-start") with
            | null -> ""
            | ur ->
                ur
                    .TextContent
                    .Trim()
                    .Replace("от", "")
                    .RegexDeleteWhitespace()

        PubDateT <-
            match PubDateT.Get1FromRegexp @"(\d{2}\.\d{2}\.\d{2})" with
            | Some x -> x.Trim()
            | None -> ""

        let datePub =
            match PubDateT.DateFromString("dd.MM.yy") with
            | Some d -> d
            | None -> DateTime.MinValue

        let mutable EndDateT =
            match t.QuerySelector("div.tender-date-info:contains('Окончание')") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let timeEnd =
            match t.QuerySelector("span.tender__countdown-container") with
            | null -> "00:00"
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        EndDateT <-
            match EndDateT.Get1FromRegexp @"(\d{2}\.\d{2}\.\d{4})" with
            | Some x -> x.Trim()
            | None -> ""

        if timeEnd <> "" then
            EndDateT <- sprintf "%s %s" EndDateT timeEnd

        let dateEnd =
            match EndDateT.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> datePub.AddDays(2.)

        let mutable UpdDateT =
            match t.QuerySelector("div.tender-date-info:contains('Дата изменения:')") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        UpdDateT <-
            match UpdDateT.Get1FromRegexp @"(\d{2}\.\d{2}\.\d{4})" with
            | Some x -> x.Trim()
            | None -> ""

        let dateUpd =
            match UpdDateT.DateFromString("dd.MM.yyyy") with
            | Some d -> d
            | None -> DateTime.Now

        let region =
            match t.QuerySelector("div.region-links-in-cabinet div a") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let delivPlace =
            match t.QuerySelector("div.tender-address") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let NmckT =
            match t.QuerySelector("div.starting-price__price") with
            | null -> ""
            | ur -> ur.TextContent.HtmlDecode().Trim()

        let Nmck =
            match NmckT.Get1FromRegexp @"([\d\s,\.]+)" with
            | Some x -> x.Trim().RegexDeleteWhitespace()
            | None -> ""

        let Currency = "₽"

        let Page = "" //Download.DownloadString1251Bot Href

        let tn =
            { Href = Href
              PurNum = PurNum
              PurName = PurName
              DatePub = datePub
              DateEnd = dateEnd
              DateUpd = dateUpd
              Region = region
              Nmck = Nmck
              DelivPlace = delivPlace
              Currency = Currency
              Page = Page }

        Monitor.Enter(this.locker)

        if this.listTenders.Count >= 5 then
            Monitor.Wait(this.locker) |> ignore

        this.listTenders.Enqueue(tn)
        Monitor.PulseAll(this.locker)
        Monitor.Exit(this.locker)
        ()

    member private this.ConsumerTender(num: int) =
        (*use con = new MySqlConnection(stn.ConStr)
        con.Open()*)
        for i in 1..num do
            Monitor.Enter(this.locker)

            if this.listTenders.Count < 1 then
                Monitor.Wait(this.locker) |> ignore

            let t = this.listTenders.Dequeue()

            try
                this.TenderChecker t
            with
                | ex -> Logging.Log.logger ex

            Monitor.PulseAll(this.locker)
            Monitor.Exit(this.locker)

    member private this.TenderChecker(tn: RosTendRecNew) =
        match tn.PurNum with
        | "" ->
            raise
            <| NullReferenceException(sprintf "PurNum not found in %s" tn.Href)
        | _ -> ()

        match tn.DatePub with
        | a when a = DateTime.MinValue ->
            raise
            <| NullReferenceException(sprintf "PubDate not found in %s" tn.Href)
        | _ -> ()

        match tn.PurName with
        | "" ->
            raise
            <| NullReferenceException(sprintf "PurName not found in %s" tn.Href)
        | _ -> ()
        (*match tn.Page with
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
            | None -> DateTime.MinValue*)

        try
            let T =
                TenderRosTendNew(set, tn, 82, "ООО Тендеры и закупки", "http://rostender.info", "")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, tn.Href)

        ()
