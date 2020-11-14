namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data

type TenderDme(stn: Settings.T, tn: DmeRec, typeFz: int, etpName: string, etpUrl: string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() =
        let builder = DocumentBuilder()
        use con = new MySqlConnection(stn.ConStr)
        let res =
                   builder {
                       printfn "%A" tn
                       return ""
                       }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
    
    member private this.GetPos(con: MySqlConnection, idLot: int, idCustomer: int) =
            let insertLotitem = sprintf "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, price = @price, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei, okpd2_code = @okpd2_code" stn.Prefix
            let cmd19 = new MySqlCommand(insertLotitem, con)
            cmd19.Prepare()
            cmd19.Parameters.AddWithValue("@id_lot", idLot) |> ignore
            cmd19.Parameters.AddWithValue("@id_customer", idCustomer) |> ignore
            cmd19.Parameters.AddWithValue("@name", tn.PurName) |> ignore
            cmd19.Parameters.AddWithValue("@sum", "") |> ignore
            cmd19.Parameters.AddWithValue("@price", "") |> ignore
            cmd19.Parameters.AddWithValue("@quantity_value", "") |> ignore
            cmd19.Parameters.AddWithValue("@customer_quantity_value", "") |> ignore
            cmd19.Parameters.AddWithValue("@okei", "") |> ignore
            cmd19.Parameters.AddWithValue("@okpd2_code", "") |> ignore
            cmd19.ExecuteNonQuery() |> ignore
        
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
