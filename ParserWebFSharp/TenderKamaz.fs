namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open OpenQA.Selenium.Chrome

type TenderKamaz(stn: Settings.T, tn: KamazRec, typeFz: int, etpName: string, etpUrl: string, _driver: ChromeDriver) =
    inherit Tender(etpName, etpUrl)
    let _ = stn
    let _ = TimeSpan.FromSeconds(30.)
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() =
        let builder = TenderBuilder()
        use con = new MySqlConnection(stn.ConStr)
        let _ = DateTime.Now
        let res =
                   builder {return ""}
        match res with
            | Success _ -> ()
            | Error e when e = "" -> ()
            | Error r -> Logging.Log.logger r
        ()
    
    member private this.SetCancelStatus(con: MySqlConnection, dateUpd: DateTime) =
        let mutable cancelStatus = 0
        let mutable updated = false
        let selectDateT = sprintf "SELECT id_tender, date_version, cancel FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz" stn.Prefix
        let cmd2 = new MySqlCommand(selectDateT, con)
        cmd2.Prepare()
        cmd2.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
        cmd2.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
        let adapter = new MySqlDataAdapter()
        adapter.SelectCommand <- cmd2
        let dt = new DataTable()
        adapter.Fill(dt) |> ignore
        for row in dt.Rows do
            updated <- true
            match dateUpd >= ((row.["date_version"]) :?> DateTime) with
            | true -> row.["cancel"] <- 1
            | false -> cancelStatus <- 1

        let commandBuilder = new MySqlCommandBuilder(adapter)
        commandBuilder.ConflictOption <- ConflictOption.OverwriteChanges
        adapter.Update(dt) |> ignore
        (cancelStatus, updated)