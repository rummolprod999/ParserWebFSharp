namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open HtmlAgilityPack
open TypeE
open Tools

type TenderRtCi(stn: Settings.T, tn: RtCiRec, typeFz: int, etpName: string, etpUrl: string) =
    inherit Tender(etpName, etpUrl)
    let _ = stn
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
                        "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND  doc_publish_date = @doc_publish_date AND type_fz = @type_fz AND end_date = @end_date"
                        stn.Prefix

                let cmd: MySqlCommand =
                    new MySqlCommand(selectTend, con)

                cmd.Prepare()

                cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum)
                |> ignore

                cmd.Parameters.AddWithValue("@doc_publish_date", tn.DatePub)
                |> ignore

                cmd.Parameters.AddWithValue("@type_fz", typeFz)
                |> ignore

                cmd.Parameters.AddWithValue("@end_date", tn.DateEnd)
                |> ignore

                let reader: MySqlDataReader =
                    cmd.ExecuteReader()

                if reader.HasRows then
                    reader.Close()
                    return! Err ""

                reader.Close()

                let Page =
                    Download.DownloadStringRts tn.Href

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

                let orgName =
                    InlineHtmlNavigator
                        nav
                        "(//div[@class = 'fields-table__label text-default' and contains(., 'Наименование организации')]/following-sibling::div)[1]"

                let orgName = orgName.HtmlDecode()
                let IdOrg = ref 0

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
                            InlineHtmlNavigator
                                nav
                                "(//div[@class = 'fields-table__label text-default' and contains(., 'Контактное лицо')]/following-sibling::div)[2]"

                        let postAddress =
                            InlineHtmlNavigator
                                nav
                                "(//div[@class = 'fields-table__label text-default' and contains(., 'Почтовый адрес')]/following-sibling::div)[2]"

                        let factAddress =
                            InlineHtmlNavigator
                                nav
                                "(//div[@class = 'fields-table__label text-default' and contains(., 'Место нахождения')]/following-sibling::div)[2]"

                        let phone =
                            InlineHtmlNavigator
                                nav
                                "(//div[@class = 'fields-table__label text-default' and contains(., 'Телефон')]/following-sibling::div)[2]"

                        let inn = ""

                        let email =
                            InlineHtmlNavigator
                                nav
                                "(//div[@class = 'fields-table__label text-default' and contains(., 'Электронная почта')]/following-sibling::div)[2]"

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

                let idEtp = this.GetEtp con stn
                let idPlacingWay = ref 0

                match tn.PwName with
                | "" -> ()
                | _ ->
                    idPlacingWay
                    := this.GetPlacingWay con tn.PwName stn

                let scoringDateT =
                    InlineHtmlNavigator
                        nav
                        "//div[@class = 'fields-table__label text-default' and contains(., 'Дата рассмотрения, оценки и')]/following-sibling::div"

                let scoringDate =
                    scoringDateT.DateFromStringOrMin("dd.MM.yyyy")

                let idTender = ref 0
                let numVersion = 1
                let status = ""

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

                cmd9.Parameters.AddWithValue("@scoring_date", scoringDate)
                |> ignore

                cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue)
                |> ignore

                cmd9.Parameters.AddWithValue("@cancel", cancelStatus)
                |> ignore

                cmd9.Parameters.AddWithValue("@date_version", dateUpd)
                |> ignore

                cmd9.Parameters.AddWithValue("@num_version", numVersion)
                |> ignore

                cmd9.Parameters.AddWithValue("@notice_version", status)
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
                | true -> incr TenderRtCi.tenderUpCount
                | false -> incr TenderRtCi.tenderCount

                let idCustomer = ref 0

                let cusName =
                    InlineHtmlNavigator
                        nav
                        "(//div[@class = 'fields-table__label text-default' and contains(., 'Наименование организации')]/following-sibling::div)[1]"

                let cusName = cusName.HtmlDecode()

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

                this.GetAttachments(con, !idTender, htmlDoc)
                this.GetLots(con, !idTender, !idCustomer, htmlDoc)
                this.AddVerNumber con tn.PurNum stn typeFz
                this.TenderKwords con (!idTender) stn
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

    member private this.GetAttachments(con: MySqlConnection, idTender: int, doc: HtmlDocument) =
        let documents =
            doc.DocumentNode.SelectNodes("//a[@class = 'link-action' and contains(@href, 'upload')]")

        if documents <> null then
            for d in documents do
                let docName = d.Gsn(".//span")
                let docUrl = d.GsnAtr "." "href"

                let docUrl =
                    sprintf "https://zakupki.rt-ci.ru%s" docUrl

                if docUrl <> "" then
                    let addAttach =
                        sprintf
                            "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url"
                            stn.Prefix

                    let cmd5 = new MySqlCommand(addAttach, con)

                    cmd5.Parameters.AddWithValue("@id_tender", idTender)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@file_name", docName)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@url", docUrl)
                    |> ignore

                    cmd5.ExecuteNonQuery() |> ignore

        ()

    member private this.GetLots(con: MySqlConnection, idTender: int, idCustomer, doc: HtmlDocument) =
        let lotNum = ref 1

        let lots =
            doc.DocumentNode.SelectNodes("//div[contains(@class, 'notification-lots__item')]")

        if lots <> null then
            for l in lots do
                let idLot = ref 0

                let lotName =
                    l.Gsn(".//div[contains(@class, 'accordion-notification-lots__desc')]")

                let lotName = lotName.HtmlDecode()

                let NmckT =
                    l.Gsn(
                        ".//div[@class = 'fields-table__label text-default' and contains(., 'Начальная стоимость')]/following-sibling::div"
                    )

                let nmckList =
                    NmckT.Get2ListRegexp "([\d,\s]+)\s+\((.+)\)"

                let nmck = nmckList.[0].GetPriceFromString()

                let insertLot =
                    sprintf
                        "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, lot_name = @lot_name"
                        stn.Prefix

                let cmd12 = new MySqlCommand(insertLot, con)

                cmd12.Parameters.AddWithValue("@id_tender", idTender)
                |> ignore

                cmd12.Parameters.AddWithValue("@lot_number", !lotNum)
                |> ignore

                cmd12.Parameters.AddWithValue("@max_price", nmck)
                |> ignore

                cmd12.Parameters.AddWithValue("@currency", nmckList.[1])
                |> ignore

                cmd12.Parameters.AddWithValue("@lot_name", lotName)
                |> ignore

                cmd12.ExecuteNonQuery() |> ignore
                idLot := int cmd12.LastInsertedId
                incr lotNum

                let OkeiT =
                    l.Gsn(
                        ".//div[@class = 'fields-table__label text-default' and contains(., 'Количество товара и код ОКЕИ')]/following-sibling::div"
                    )

                let okeiList =
                    OkeiT.Get2ListRegexp "([\d,\s]+)\s+(.+)"

                let quantity =
                    okeiList.[0].GetPriceFromString()

                let insertLotitem =
                    sprintf
                        "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, price = @price, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei, okpd2_code = @okpd2_code"
                        stn.Prefix

                let cmd19 =
                    new MySqlCommand(insertLotitem, con)

                cmd19.Prepare()

                cmd19.Parameters.AddWithValue("@id_lot", !idLot)
                |> ignore

                cmd19.Parameters.AddWithValue("@id_customer", idCustomer)
                |> ignore

                cmd19.Parameters.AddWithValue("@name", lotName)
                |> ignore

                cmd19.Parameters.AddWithValue("@sum", nmck)
                |> ignore

                cmd19.Parameters.AddWithValue("@price", "")
                |> ignore

                cmd19.Parameters.AddWithValue("@quantity_value", quantity)
                |> ignore

                cmd19.Parameters.AddWithValue("@customer_quantity_value", quantity)
                |> ignore

                cmd19.Parameters.AddWithValue("@okei", okeiList.[1])
                |> ignore

                cmd19.Parameters.AddWithValue("@okpd2_code", "")
                |> ignore

                cmd19.ExecuteNonQuery() |> ignore

                let delivPlace =
                    l.Gsn(
                        ".//div[@class = 'fields-table__label text-default' and contains(., 'Место поставки товара')]/following-sibling::div"
                    )

                if delivPlace <> "" then
                    let insertCustomerRequirement =
                        sprintf
                            "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, application_guarantee_amount = @application_guarantee_amount, contract_guarantee_amount = @contract_guarantee_amount, delivery_term = @delivery_term"
                            stn.Prefix

                    let cmd16 =
                        new MySqlCommand(insertCustomerRequirement, con)

                    cmd16.Prepare()

                    cmd16.Parameters.AddWithValue("@id_lot", !idLot)
                    |> ignore

                    cmd16.Parameters.AddWithValue("@id_customer", idCustomer)
                    |> ignore

                    cmd16.Parameters.AddWithValue("@delivery_place", delivPlace)
                    |> ignore

                    cmd16.Parameters.AddWithValue("@application_guarantee_amount", "")
                    |> ignore

                    cmd16.Parameters.AddWithValue("@contract_guarantee_amount", "")
                    |> ignore

                    cmd16.Parameters.AddWithValue("@delivery_term", "")
                    |> ignore

                    cmd16.ExecuteNonQuery() |> ignore

                ()

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
