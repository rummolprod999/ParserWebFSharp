namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System
open MySql.Data.MySqlClient
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks
open TypeE

type ParserTPlusParall(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let count = 1000
    let TypeFz = 96

    let strtPg =
        sprintf "https://tenderplus.kz/zakupki?page=%d&lot-sort=-pub_date"

    member val locker = Object()
    member val listTenders = Queue<TPlusRec>()

    override this.Parsing() =
        for i in 1..count do
            try
                this.ParserPage <| strtPg i
            with
                | ex -> Logging.Log.logger ex

    member private this.ParserPage(url: string) =
        let Page = Download.DownloadStringRts url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let mutable tens =
                documents.QuerySelectorAll("div.tenders-list div.tender-teaser")

            for t in tens do
                try
                    let task =
                        new Task(fun () -> this.AddTenderToList t)

                    task.Start()
                //task.Wait()
                with
                    | ex -> Logging.Log.logger ex

            let cn = tens.Length - 1

            let cons =
                new Task(fun () -> this.ConsumerTender cn)

            cons.Start()
            cons.Wait()
            ()

        ()

    member private this.AddTenderToList(t: IElement) =
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
            match t.QuerySelector("h2.title + span") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let Nmck =
            match t.QuerySelector("div.amount") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexDeleteWhitespace()

        let dateTEnd =
            match t.QuerySelector("div.date > span:nth-of-type(3)") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let dateEndT =
            match dateTEnd with
            | Tools.RegexMatch1 @"(\d{2}\.\d{2}\.\d{4})" (gr1) -> Some(sprintf "%s" gr1)
            | _ -> None

        let dateEnd =
            match dateEndT with
            | None -> DateTime.MinValue
            | Some e ->
                match e.DateFromString("dd.MM.yyyy") with
                | Some d -> d
                | None -> DateTime.MinValue

        let dateTPub =
            match t.QuerySelector("div.date > span:nth-of-type(1)") with
            | null -> ""
            | ur -> ur.TextContent.Trim().RegexCutWhitespace()

        let datePubT =
            match dateTPub with
            | Tools.RegexMatch1 @"(\d{2}\.\d{2}\.\d{4})" (gr1) -> Some(sprintf "%s" gr1)
            | _ -> None

        let datePub =
            match datePubT with
            | None -> DateTime.MinValue
            | Some e ->
                match e.DateFromString("dd.MM.yyyy") with
                | Some d -> d
                | None -> DateTime.MinValue

        let status =
            match t.QuerySelector("div.status") with
            | null -> ""
            | ur -> ur.TextContent.Replace("Статус:", "").Trim()

        let mutable Page = ""
        let mutable exist = Exist

        try
            exist <- this.TenderExister purNum datePub dateEnd status

            match exist with
            | NoExist -> Page <- Download.DownloadStringRts Href
            | Exist -> ()
        with
            | ex -> Logging.Log.logger ex

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

        if this.listTenders.Count >= 5 then
            Monitor.Wait(this.locker) |> ignore

        this.listTenders.Enqueue(tn)
        Monitor.PulseAll(this.locker)
        Monitor.Exit(this.locker)
        ()

    member private this.ConsumerTender(num: int) =
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

    member private this.TenderChecker(tn: TPlusRec) =
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

        match tn.Exist with
        | Exist -> ()
        | NoExist ->
            match tn.Page with
            | "" ->
                raise
                <| NullReferenceException(sprintf "Page not found in %s" tn.Href)
            | _ -> ()

            try
                let T =
                    TenderTplus(set, tn, TypeFz, "TenderPlus", "https://tenderplus.kz", tn.Page)

                T.Parsing()
            with
                | ex -> Logging.Log.logger (ex, tn.Href)

    member private this.TenderExister
        (purNum: string)
        (datePub: DateTime)
        (dateEnd: DateTime)
        (status: string)
        : Exist =
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

                let cmd: MySqlCommand =
                    new MySqlCommand(selectTend, con)

                cmd.Prepare()

                cmd.Parameters.AddWithValue("@purchase_number", purNum)
                |> ignore

                cmd.Parameters.AddWithValue("@type_fz", TypeFz)
                |> ignore

                cmd.Parameters.AddWithValue("@end_date", dateEnd)
                |> ignore

                cmd.Parameters.AddWithValue("@doc_publish_date", datePub)
                |> ignore

                cmd.Parameters.AddWithValue("@notice_version", status)
                |> ignore

                let reader: MySqlDataReader =
                    cmd.ExecuteReader()

                if reader.HasRows then
                    reader.Close()
                    Exist
                else
                    reader.Close()
                    NoExist
