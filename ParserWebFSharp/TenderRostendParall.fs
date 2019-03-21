namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data

type TenderRosTendParall(stn : Settings.T, tn : RosTendRec, typeFz : int, etpName : string, etpUrl : string, dateEnd : DateTime, noticeVer : string, con: MySqlConnection) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0
    override this.Parsing() =
        let dateUpd = DateTime.Now
        let href = tn.Href
        let selectTend =
            sprintf 
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND doc_publish_date = @doc_publish_date AND notice_version = @notice_version" 
                stn.Prefix
        let cmd : MySqlCommand = new MySqlCommand(selectTend, con)
        cmd.Prepare()
        cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
        cmd.Parameters.AddWithValue("@end_date", dateEnd) |> ignore
        cmd.Parameters.AddWithValue("@doc_publish_date", tn.DatePub) |> ignore
        cmd.Parameters.AddWithValue("@notice_version", noticeVer) |> ignore
        let reader : MySqlDataReader = cmd.ExecuteReader()
        if reader.HasRows then reader.Close()
        else 
            reader.Close()
            let mutable cancelStatus = 0
            let mutable updated = false
            let selectDateT =
                sprintf 
                    "SELECT id_tender, date_version, cancel FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz" 
                    stn.Prefix
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
                //printfn "%A" <| (row.["date_version"])
                match dateUpd >= ((row.["date_version"]) :?> DateTime) with
                | true -> row.["cancel"] <- 1
                | false -> cancelStatus <- 1
            let commandBuilder = new MySqlCommandBuilder(adapter)
            commandBuilder.ConflictOption <- ConflictOption.OverwriteChanges
            adapter.Update(dt) |> ignore
            let Printform = href
            let IdOrg = ref 0
            let idPlacingWay = ref 0
            let idEtp = this.GetEtp con settings
            let numVersion = 1
            let mutable idRegion = 0
            let regionS = Tools.GetRegionString(tn.Region)
            if regionS <> "" then 
                let selectReg = sprintf "SELECT id FROM %sregion WHERE name LIKE @name" stn.Prefix
                let cmd46 = new MySqlCommand(selectReg, con)
                cmd46.Prepare()
                cmd46.Parameters.AddWithValue("@name", "%" + regionS + "%") |> ignore
                let reader36 = cmd46.ExecuteReader()
                match reader36.HasRows with
                | true -> 
                    reader36.Read() |> ignore
                    idRegion <- reader36.GetInt32("id")
                    reader36.Close()
                | false -> reader36.Close()
            let idTender = ref 0
            let insertTender =
                String.Format
                    ("INSERT INTO {0}tender SET id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form, id_region = @id_region", 
                     stn.Prefix)
            let cmd9 = new MySqlCommand(insertTender, con)
            cmd9.Prepare()
            cmd9.Parameters.AddWithValue("@id_xml", tn.PurNum) |> ignore
            cmd9.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
            cmd9.Parameters.AddWithValue("@doc_publish_date", tn.DatePub) |> ignore
            cmd9.Parameters.AddWithValue("@href", href) |> ignore
            cmd9.Parameters.AddWithValue("@purchase_object_info", tn.PurName) |> ignore
            cmd9.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
            cmd9.Parameters.AddWithValue("@id_organizer", !IdOrg) |> ignore
            cmd9.Parameters.AddWithValue("@id_placing_way", !idPlacingWay) |> ignore
            cmd9.Parameters.AddWithValue("@id_etp", idEtp) |> ignore
            cmd9.Parameters.AddWithValue("@end_date", dateEnd) |> ignore
            cmd9.Parameters.AddWithValue("@scoring_date", DateTime.MinValue) |> ignore
            cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue) |> ignore
            cmd9.Parameters.AddWithValue("@cancel", cancelStatus) |> ignore
            cmd9.Parameters.AddWithValue("@date_version", dateUpd) |> ignore
            cmd9.Parameters.AddWithValue("@num_version", numVersion) |> ignore
            cmd9.Parameters.AddWithValue("@notice_version", noticeVer) |> ignore
            cmd9.Parameters.AddWithValue("@xml", href) |> ignore
            cmd9.Parameters.AddWithValue("@print_form", Printform) |> ignore
            cmd9.Parameters.AddWithValue("@id_region", idRegion) |> ignore
            cmd9.ExecuteNonQuery() |> ignore
            idTender := int cmd9.LastInsertedId
            match updated with
            | true -> incr TenderRosTendParall.tenderUpCount
            | false -> incr TenderRosTendParall.tenderCount
            let idCustomer = ref 0
            let idLot = ref 0
            let insertLot =
                sprintf 
                    "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency" 
                    stn.Prefix
            let cmd12 = new MySqlCommand(insertLot, con)
            cmd12.Parameters.AddWithValue("@id_tender", !idTender) |> ignore
            cmd12.Parameters.AddWithValue("@lot_number", 1) |> ignore
            cmd12.Parameters.AddWithValue("@max_price", tn.Nmck) |> ignore
            cmd12.Parameters.AddWithValue("@currency", tn.Currency) |> ignore
            cmd12.ExecuteNonQuery() |> ignore
            idLot := int cmd12.LastInsertedId
            let insertLotitem =
                sprintf 
                    "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum" 
                    stn.Prefix
            let cmd19 = new MySqlCommand(insertLotitem, con)
            cmd19.Prepare()
            cmd19.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
            cmd19.Parameters.AddWithValue("@id_customer", !idCustomer) |> ignore
            cmd19.Parameters.AddWithValue("@name", tn.PurName) |> ignore
            cmd19.Parameters.AddWithValue("@sum", tn.Nmck) |> ignore
            cmd19.ExecuteNonQuery() |> ignore
            if tn.DelivPlace <> "" then 
                let insertCustomerRequirement =
                    sprintf 
                        "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place" 
                        stn.Prefix
                let cmd16 = new MySqlCommand(insertCustomerRequirement, con)
                cmd16.Prepare()
                cmd16.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
                cmd16.Parameters.AddWithValue("@id_customer", !idCustomer) |> ignore
                cmd16.Parameters.AddWithValue("@delivery_place", tn.DelivPlace) |> ignore
                cmd16.ExecuteNonQuery() |> ignore
            try 
                this.AddVerNumber con tn.PurNum stn typeFz
            with ex -> 
                Logging.Log.logger "Ошибка добавления версий тендера"
                Logging.Log.logger ex
            try 
                this.TenderKwords con (!idTender) stn
            with ex -> 
                Logging.Log.logger "Ошибка добавления kwords тендера"
                Logging.Log.logger ex
            ()
        ()
