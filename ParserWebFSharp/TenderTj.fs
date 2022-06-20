namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open TypeE
open HtmlAgilityPack
open Tools

type TenderTj(stn: Settings.T, tn: TjRec, typeFz: int, etpName: string, etpUrl: string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() =
        let builder = DocumentBuilder()
        use con = new MySqlConnection(stn.ConStr)

        let res =
            builder {
                con.Open()

                let selectTend =
                    sprintf
                        "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND notice_version = @notice_version"
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

                cmd.Parameters.AddWithValue("@notice_version", tn.status)
                |> ignore

                let reader: MySqlDataReader =
                    cmd.ExecuteReader()

                if reader.HasRows then
                    reader.Close()
                    return! Err ""

                reader.Close()
                let Page = Download.DownloadString tn.Href

                if Page = "" || Page = null then
                    return! Err(sprintf "%s" tn.Href)

                let htmlDoc = HtmlDocument()
                htmlDoc.LoadHtml(Page)

                let nav =
                    (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator

                let dateUpd = DateTime.Now

                let (cancelStatus, updated) =
                    this.SetCancelStatus(con, dateUpd)

                let Printform = tn.Href
                let IdOrg = ref 0

                let orgName =
                    nav.Gsn "//th[contains(., 'Организатор:')]/following-sibling::td"

                let orgName =
                    if orgName <> "" then
                        orgName
                    else
                        tn.OrgName

                if orgName <> "" then
                    let selectOrg =
                        sprintf "SELECT id_organizer FROM %sorganizer WHERE full_name = @full_name" stn.Prefix

                    let cmd3 = new MySqlCommand(selectOrg, con)
                    cmd3.Prepare()

                    cmd3.Parameters.AddWithValue("@full_name", orgName)
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
                                "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address, contact_phone = @contact_phone, inn = @inn, contact_email = @contact_email"
                                stn.Prefix

                        let contactPerson =
                            InlineHtmlNavigator nav "//th[contains(., 'Контактная информация:')]/following-sibling::td"

                        let postAddress = ""
                        let factAddress = ""
                        let phone = ""
                        let inn = ""
                        let email = ""

                        let cmd5 =
                            new MySqlCommand(addOrganizer, con)

                        cmd5.Parameters.AddWithValue("@full_name", orgName)
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

                        cmd5.Parameters.AddWithValue("@contact_email", email)
                        |> ignore

                        cmd5.ExecuteNonQuery() |> ignore
                        IdOrg := int cmd5.LastInsertedId
                        ()

                let idEtp = this.GetEtp con settings
                let numVersion = 1
                let idPlacingWay = ref 0
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

                cmd9.Parameters.AddWithValue("@href", tn.Href)
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

                cmd9.Parameters.AddWithValue("@xml", tn.Href)
                |> ignore

                cmd9.Parameters.AddWithValue("@print_form", Printform)
                |> ignore

                cmd9.Parameters.AddWithValue("@id_region", 0)
                |> ignore

                cmd9.ExecuteNonQuery() |> ignore
                idTender := int cmd9.LastInsertedId

                match updated with
                | true -> incr TenderTj.tenderUpCount
                | false -> incr TenderTj.tenderCount

                this.GetLots(con, !idTender, htmlDoc)
                this.AddVerNumber con tn.PurNum stn typeFz
                this.TenderKwords con (!idTender) stn
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()

    member private this.GetLots(con: MySqlConnection, idTender: int, doc: HtmlDocument) =
        let nav =
            (doc.CreateNavigator()) :?> HtmlNodeNavigator

        let idLot = ref 0

        let insertLot =
            sprintf
                "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency"
                stn.Prefix

        let cmd12 = new MySqlCommand(insertLot, con)

        cmd12.Parameters.AddWithValue("@id_tender", idTender)
        |> ignore

        cmd12.Parameters.AddWithValue("@lot_number", 1)
        |> ignore

        cmd12.Parameters.AddWithValue("@max_price", "")
        |> ignore

        cmd12.Parameters.AddWithValue("@currency", "")
        |> ignore

        cmd12.ExecuteNonQuery() |> ignore
        idLot := int cmd12.LastInsertedId

        let lotName =
            nav.Gsn "//th[contains(., 'Наименование лота:')]/following-sibling::td/strong"

        let cusName =
            nav.Gsn "//th[contains(., 'Закупающая организация:')]/following-sibling::td/strong"

        let cusName =
            if cusName <> "" then
                cusName
            else
                tn.OrgName

        let idCustomer = ref 0

        if cusName <> "" then
            let selectCustomer =
                sprintf "SELECT id_customer FROM %scustomer WHERE full_name = @full_name" stn.Prefix

            let cmd3 =
                new MySqlCommand(selectCustomer, con)

            cmd3.Prepare()

            cmd3.Parameters.AddWithValue("@full_name", cusName)
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

                cmd14.Parameters.AddWithValue("@full_name", cusName)
                |> ignore

                cmd14.Parameters.AddWithValue("@inn", inn)
                |> ignore

                cmd14.ExecuteNonQuery() |> ignore
                idCustomer := int cmd14.LastInsertedId

        let delivPlace =
            nav.Gsn "//th[contains(., 'Место поставки:')]/following-sibling::td"

        let delivTerm =
            nav.Gsn "//th[contains(., 'Срок поставки:')]/following-sibling::td"

        let applAmount = "" //"nav.Gsn "//th[contains(., 'Гарантийное обеспечение заявки:')]/following-sibling::td"

        let applAmount =
            applAmount.GetPriceFromString()

        let contrAmount =
            nav.Gsn "//th[contains(., 'Гарантийное обеспечение исполнения контракта:')]/following-sibling::td"

        let contrAmount =
            contrAmount.GetPriceFromString()

        if delivPlace <> "" || delivTerm <> "" then
            let insertCustomerRequirement =
                sprintf
                    "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, application_guarantee_amount = @application_guarantee_amount, contract_guarantee_amount = @contract_guarantee_amount, delivery_term = @delivery_term"
                    stn.Prefix

            let cmd16 =
                new MySqlCommand(insertCustomerRequirement, con)

            cmd16.Prepare()

            cmd16.Parameters.AddWithValue("@id_lot", !idLot)
            |> ignore

            cmd16.Parameters.AddWithValue("@id_customer", !idCustomer)
            |> ignore

            cmd16.Parameters.AddWithValue("@delivery_place", delivPlace)
            |> ignore

            cmd16.Parameters.AddWithValue("@application_guarantee_amount", applAmount)
            |> ignore

            cmd16.Parameters.AddWithValue("@contract_guarantee_amount", contrAmount)
            |> ignore

            cmd16.Parameters.AddWithValue("@delivery_term", delivTerm)
            |> ignore

            cmd16.ExecuteNonQuery() |> ignore

        let insertLotitem =
            sprintf
                "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, price = @price, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei, okpd_name = @okpd_name"
                stn.Prefix

        let cmd19 =
            new MySqlCommand(insertLotitem, con)

        cmd19.Prepare()

        cmd19.Parameters.AddWithValue("@id_lot", !idLot)
        |> ignore

        cmd19.Parameters.AddWithValue("@id_customer", !idCustomer)
        |> ignore

        cmd19.Parameters.AddWithValue("@name", lotName)
        |> ignore

        cmd19.Parameters.AddWithValue("@sum", "")
        |> ignore

        cmd19.Parameters.AddWithValue("@price", "")
        |> ignore

        cmd19.Parameters.AddWithValue("@quantity_value", "")
        |> ignore

        cmd19.Parameters.AddWithValue("@customer_quantity_value", "")
        |> ignore

        cmd19.Parameters.AddWithValue("@okei", "")
        |> ignore

        cmd19.Parameters.AddWithValue("@okpd_name", "")
        |> ignore

        cmd19.ExecuteNonQuery() |> ignore

        let requirement =
            nav.Gsn "//th[contains(., 'Обеспечение сопутствующих условий:')]/following-sibling::td"

        let insertRequirement =
            sprintf "INSERT INTO %srequirement SET id_lot = @id_lot, content = @content" stn.Prefix

        let cmd44 =
            new MySqlCommand(insertRequirement, con)

        cmd44.Prepare()

        cmd44.Parameters.AddWithValue("@id_lot", !idLot)
        |> ignore

        cmd44.Parameters.AddWithValue("@content", requirement)
        |> ignore

        cmd44.ExecuteNonQuery() |> ignore
        ()

    member private this.SetCancelStatus(con: MySqlConnection, dateUpd: DateTime) =
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

            match dateUpd >= ((row.["date_version"]) :?> DateTime) with
            | true -> row.["cancel"] <- 1
            | false -> cancelStatus <- 1

        let commandBuilder =
            new MySqlCommandBuilder(adapter)

        commandBuilder.ConflictOption <- ConflictOption.OverwriteChanges
        adapter.Update(dt) |> ignore
        (cancelStatus, updated)
