namespace ParserWeb

open AngleSharp.Dom
open MySql.Data.MySqlClient
open System
open System.Data
open TypeE

type TenderAero(stn: Settings.T, tn: AeroRec, typeFz: int, etpName: string, etpUrl: string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0

    member private this.ParsingDocs (con: MySqlConnection) (idTender: int) (elem: IElement) =
        let docName =
            match elem.QuerySelector("td a") with
            | null -> ""
            | ur -> ur.TextContent.Trim()

        match docName with
        | "" -> ()
        | x ->
            let hrefT =
                elem.QuerySelector("td a").GetAttribute("href")

            let href =
                sprintf "https://www.aeroflot.ru%s" hrefT

            let addAttach =
                sprintf
                    "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url"
                    stn.Prefix

            let cmd5 = new MySqlCommand(addAttach, con)

            cmd5.Parameters.AddWithValue("@id_tender", idTender)
            |> ignore

            cmd5.Parameters.AddWithValue("@file_name", x)
            |> ignore

            cmd5.Parameters.AddWithValue("@url", href)
            |> ignore

            cmd5.ExecuteNonQuery() |> ignore

        ()

    override this.Parsing() =
        let dateUpd = DateTime.Now
        use con = new MySqlConnection(stn.ConStr)
        con.Open()
        let href = tn.Href

        let selectTend =
            sprintf
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND doc_publish_date = @doc_publish_date"
                stn.Prefix

        let cmd: MySqlCommand =
            new MySqlCommand(selectTend, con)

        cmd.Prepare()

        cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum)
        |> ignore

        cmd.Parameters.AddWithValue("@type_fz", typeFz)
        |> ignore

        cmd.Parameters.AddWithValue("@end_date", tn.DateEnd)
        |> ignore

        cmd.Parameters.AddWithValue("@doc_publish_date", tn.DatePub)
        |> ignore

        let reader: MySqlDataReader =
            cmd.ExecuteReader()

        if reader.HasRows then
            reader.Close()
        else
            reader.Close()

            let mutable cancelStatus = 0
            let mutable updated = false

            let selectDateT =
                sprintf
                    "SELECT id_tender, date_version, cancel FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz"
                    stn.Prefix

            let cmd2 =
                new MySqlCommand(selectDateT, con)

            cmd2.Prepare()

            cmd2.Parameters.AddWithValue("@purchase_number", tn.PurNum)
            |> ignore

            cmd2.Parameters.AddWithValue("@type_fz", typeFz)
            |> ignore

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

            let commandBuilder =
                new MySqlCommandBuilder(adapter)

            commandBuilder.ConflictOption <- ConflictOption.OverwriteChanges
            adapter.Update(dt) |> ignore
            let Printform = href
            let IdOrg = ref 0
            let OrgName = "ПАО «Аэрофлот»"

            let selectOrg =
                sprintf "SELECT id_organizer FROM %sorganizer WHERE full_name = @full_name" stn.Prefix

            let cmd3 = new MySqlCommand(selectOrg, con)
            cmd3.Prepare()

            cmd3.Parameters.AddWithValue("@full_name", OrgName)
            |> ignore

            let reader = cmd3.ExecuteReader()

            match reader.HasRows with
            | true ->
                reader.Read() |> ignore
                IdOrg := reader.GetInt32("id_organizer")
                reader.Close()
            | false ->
                reader.Close()

                let addOrganizer =
                    sprintf
                        "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address, contact_phone = @contact_phone, inn = @inn"
                        stn.Prefix

                let contactPerson = ""

                let postAddress =
                    "Российская Федерация, 119002, г. Москва, ул. Арбат, д. 10"

                let factAddress =
                    "Российская Федерация, 119002, г. Москва, ул. Арбат, д. 10"

                let phone = ""
                let inn = "7712040126"

                let cmd5 =
                    new MySqlCommand(addOrganizer, con)

                cmd5.Parameters.AddWithValue("@full_name", OrgName)
                |> ignore

                cmd5.Parameters.AddWithValue("@contact_person", contactPerson)
                |> ignore

                cmd5.Parameters.AddWithValue("@post_address", postAddress)
                |> ignore

                cmd5.Parameters.AddWithValue("@fact_address", factAddress)
                |> ignore

                cmd5.Parameters.AddWithValue("@contact_phone", phone)
                |> ignore

                cmd5.Parameters.AddWithValue("@inn", inn)
                |> ignore

                cmd5.ExecuteNonQuery() |> ignore
                IdOrg := int cmd5.LastInsertedId
                ()

            let idPlacingWay = ref 0

            match tn.PwayName with
            | "" -> ()
            | _ ->
                idPlacingWay
                := this.GetPlacingWay con tn.PwayName settings

            let idEtp = this.GetEtp con settings
            let numVersion = 1
            let idRegion = 0
            let idTender = ref 0

            let insertTender =
                String.Format(
                    "INSERT INTO {0}tender SET id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form, id_region = @id_region",
                    stn.Prefix
                )

            let cmd9 =
                new MySqlCommand(insertTender, con)

            cmd9.Prepare()

            cmd9.Parameters.AddWithValue("@id_xml", tn.PurNum)
            |> ignore

            cmd9.Parameters.AddWithValue("@purchase_number", tn.PurNum)
            |> ignore

            cmd9.Parameters.AddWithValue("@doc_publish_date", tn.DatePub)
            |> ignore

            cmd9.Parameters.AddWithValue("@href", href)
            |> ignore

            cmd9.Parameters.AddWithValue("@purchase_object_info", tn.PurName)
            |> ignore

            cmd9.Parameters.AddWithValue("@type_fz", typeFz)
            |> ignore

            cmd9.Parameters.AddWithValue("@id_organizer", !IdOrg)
            |> ignore

            cmd9.Parameters.AddWithValue("@id_placing_way", !idPlacingWay)
            |> ignore

            cmd9.Parameters.AddWithValue("@id_etp", idEtp)
            |> ignore

            cmd9.Parameters.AddWithValue("@end_date", tn.DateEnd)
            |> ignore

            cmd9.Parameters.AddWithValue("@scoring_date", DateTime.MinValue)
            |> ignore

            cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue)
            |> ignore

            cmd9.Parameters.AddWithValue("@cancel", cancelStatus)
            |> ignore

            cmd9.Parameters.AddWithValue("@date_version", dateUpd)
            |> ignore

            cmd9.Parameters.AddWithValue("@num_version", numVersion)
            |> ignore

            cmd9.Parameters.AddWithValue("@notice_version", tn.status)
            |> ignore

            cmd9.Parameters.AddWithValue("@xml", href)
            |> ignore

            cmd9.Parameters.AddWithValue("@print_form", Printform)
            |> ignore

            cmd9.Parameters.AddWithValue("@id_region", idRegion)
            |> ignore

            cmd9.ExecuteNonQuery() |> ignore
            idTender := int cmd9.LastInsertedId

            match updated with
            | true -> incr TenderAero.tenderUpCount
            | false -> incr TenderAero.tenderCount

            this.GetAttachments(con, !idTender)
            let CustomerName = OrgName

            let idCustomer = ref 0

            if CustomerName <> "" then
                let selectCustomer =
                    sprintf "SELECT id_customer FROM %scustomer WHERE full_name = @full_name" stn.Prefix

                let cmd3 =
                    new MySqlCommand(selectCustomer, con)

                cmd3.Prepare()

                cmd3.Parameters.AddWithValue("@full_name", CustomerName)
                |> ignore

                let reader = cmd3.ExecuteReader()

                match reader.HasRows with
                | true ->
                    reader.Read() |> ignore
                    idCustomer := reader.GetInt32("id_customer")
                    reader.Close()
                | false ->
                    reader.Close()

                    let insertCustomer =
                        sprintf
                            "INSERT INTO %scustomer SET reg_num = @reg_num, full_name = @full_name, inn = @inn"
                            stn.Prefix

                    let RegNum = Guid.NewGuid().ToString()
                    let inn = ""

                    let cmd14 =
                        new MySqlCommand(insertCustomer, con)

                    cmd14.Prepare()

                    cmd14.Parameters.AddWithValue("@reg_num", RegNum)
                    |> ignore

                    cmd14.Parameters.AddWithValue("@full_name", CustomerName)
                    |> ignore

                    cmd14.Parameters.AddWithValue("@inn", inn)
                    |> ignore

                    cmd14.ExecuteNonQuery() |> ignore
                    idCustomer := int cmd14.LastInsertedId

            let idLot = ref 0

            let priceT = ""

            let maxPrice =
                match priceT.Get1FromRegexp @"^([\d\.,]+)" with
                | Some x -> x.Trim()
                | None -> ""

            let currency =
                match priceT.Get1FromRegexp @"([\w]+)$" with
                | Some x -> x.Trim()
                | None -> ""

            let insertLot =
                sprintf
                    "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency"
                    stn.Prefix

            let cmd12 = new MySqlCommand(insertLot, con)

            cmd12.Parameters.AddWithValue("@id_tender", !idTender)
            |> ignore

            cmd12.Parameters.AddWithValue("@lot_number", 1)
            |> ignore

            cmd12.Parameters.AddWithValue("@max_price", maxPrice)
            |> ignore

            cmd12.Parameters.AddWithValue("@currency", currency)
            |> ignore

            cmd12.ExecuteNonQuery() |> ignore
            idLot := int cmd12.LastInsertedId

            let delivTerm = ""

            if delivTerm <> "" then
                let insertCustomerRequirement =
                    sprintf
                        "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_term = @delivery_term"
                        stn.Prefix

                let cmd16 =
                    new MySqlCommand(insertCustomerRequirement, con)

                cmd16.Prepare()

                cmd16.Parameters.AddWithValue("@id_lot", !idLot)
                |> ignore

                cmd16.Parameters.AddWithValue("@id_customer", !idCustomer)
                |> ignore

                cmd16.Parameters.AddWithValue("@delivery_term", delivTerm)
                |> ignore

                cmd16.ExecuteNonQuery() |> ignore

            let quantT = ""

            let quantity =
                match quantT.Get1FromRegexp @"^([\d\.,]+)" with
                | Some x -> x.Trim()
                | None -> ""

            let okei =
                match quantT.Get1FromRegexp @"\s+(.+)$" with
                | Some x -> x.Trim()
                | None -> ""

            let insertLotitem =
                sprintf
                    "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, okpd_name = @okpd_name, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei"
                    stn.Prefix

            let cmd19 =
                new MySqlCommand(insertLotitem, con)

            cmd19.Prepare()

            cmd19.Parameters.AddWithValue("@id_lot", !idLot)
            |> ignore

            cmd19.Parameters.AddWithValue("@id_customer", !idCustomer)
            |> ignore

            cmd19.Parameters.AddWithValue("@name", tn.PurName)
            |> ignore

            cmd19.Parameters.AddWithValue("@okpd_name", "")
            |> ignore

            cmd19.Parameters.AddWithValue("@quantity_value", quantity)
            |> ignore

            cmd19.Parameters.AddWithValue("@customer_quantity_value", quantity)
            |> ignore

            cmd19.Parameters.AddWithValue("@okei", okei)
            |> ignore

            cmd19.ExecuteNonQuery() |> ignore

            try
                this.AddVerNumber con tn.PurNum stn typeFz
            with
                | ex ->
                    Logging.Log.logger "Ошибка добавления версий тендера"
                    Logging.Log.logger ex

            try
                this.TenderKwords con (!idTender) stn
            with
                | ex ->
                    Logging.Log.logger "Ошибка добавления kwords тендера"
                    Logging.Log.logger ex

            ()

    
    member private this.GetAttachments(con: MySqlConnection, idTender: int) =
        for doc in tn.DocList do
            let urlDoc = doc.GetAttribute("href").Trim()

            let url =
                sprintf "https://www.aeroflot.ru%s" urlDoc

            let name = doc.TextContent.Trim()

            let addAttach =
                sprintf
                    "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description"
                    stn.Prefix

            let cmd5 = new MySqlCommand(addAttach, con)

            cmd5.Parameters.AddWithValue("@id_tender", idTender)
            |> ignore

            cmd5.Parameters.AddWithValue("@file_name", name)
            |> ignore

            cmd5.Parameters.AddWithValue("@url", url)
            |> ignore

            cmd5.Parameters.AddWithValue("@description", "")
            |> ignore

            cmd5.ExecuteNonQuery() |> ignore