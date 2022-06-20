namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open HtmlAgilityPack
open TypeE
open Tools

type TenderEtpRt(stn: Settings.T, tn: EtpRtRec, typeFz: int, etpName: string, etpUrl: string) =
    inherit Tender(etpName, etpUrl)
    let _ = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() =
        let builder = DocumentBuilder()
        use con = new MySqlConnection(stn.ConStr)

        let res =
            builder {
                let Page = Download.DownloadString tn.Href

                if Page = "" || Page = null then
                    return! Err(sprintf "%s" tn.Href)

                let htmlDoc = HtmlDocument()
                htmlDoc.LoadHtml(Page)

                let nav =
                    (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator

                let dateUpd = DateTime.Now

                let! purName =
                    nav.GsnDocWithError "//h1"
                    <| sprintf "purName not found %s" tn.Href

                let! datePubT =
                    nav.GsnDocWithError "//div[@class = 'lot_publish_date']/strong"
                    <| sprintf "datePubT not found %s" tn.Href

                let! datePub =
                    datePubT.DateFromStringDoc("dd.MM.yyyy", sprintf "datePub not found %s %s " tn.Href (datePubT))

                let! dateEndT1 =
                    nav.GsnDocWithError "//div[@class = 'lot_deadline']/strong"
                    <| sprintf "dateEndT1 not found %s" tn.Href

                let! dateEndT2 =
                    nav.GsnDocWithError "//div[@class = 'lot_deadline']/span"
                    <| sprintf "dateEndT2 not found %s" tn.Href

                let dateEndT =
                    sprintf "%s %s" dateEndT1 dateEndT2

                let! dateEnd =
                    dateEndT.DateFromStringDoc(
                        "dd.MM.yyyy HH:mm",
                        sprintf "dateEnd not found %s %s " tn.Href (dateEndT)
                    )

                let! dateScoringT =
                    nav.GsnDocWithError "//div[@class = 'lot_rez_date']/strong"
                    <| sprintf "dateScoringT not found %s" tn.Href

                let! dateScoring =
                    dateScoringT.DateFromStringDoc(
                        "dd.MM.yyyy",
                        sprintf "dateScoring not found %s %s " tn.Href (datePubT)
                    )

                let status =
                    InlineHtmlNavigator nav "//div[. = 'Статус']/following-sibling::div"

                con.Open()

                let selectTend =
                    sprintf
                        "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND  doc_publish_date = @doc_publish_date AND type_fz = @type_fz AND end_date = @end_date AND notice_version = @notice_version"
                        stn.Prefix

                let cmd: MySqlCommand =
                    new MySqlCommand(selectTend, con)

                cmd.Prepare()

                cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum)
                |> ignore

                cmd.Parameters.AddWithValue("@doc_publish_date", datePub)
                |> ignore

                cmd.Parameters.AddWithValue("@type_fz", typeFz)
                |> ignore

                cmd.Parameters.AddWithValue("@end_date", dateEnd)
                |> ignore

                cmd.Parameters.AddWithValue("@notice_version", status)
                |> ignore

                let reader: MySqlDataReader =
                    cmd.ExecuteReader()

                if reader.HasRows then
                    reader.Close()
                    return! Err ""

                reader.Close()

                let (cancelStatus, updated) =
                    this.SetCancelStatus(con, dateUpd)

                let Printform = tn.Href
                let IdOrg = ref 0
                let orgName = etpName

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

                        let contactPerson = ""
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

                let idEtp = this.GetEtp con stn

                let pwName =
                    InlineHtmlNavigator nav "//div[. = 'Вид процедуры']/following-sibling::div"

                let idPlacingWay = ref 0

                match pwName with
                | "" -> ()
                | _ -> idPlacingWay := this.GetPlacingWay con pwName stn

                let idTender = ref 0
                let numVersion = 1

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

                cmd9.Parameters.AddWithValue("@doc_publish_date", datePub)
                |> ignore

                cmd9.Parameters.AddWithValue("@href", tn.Href)
                |> ignore

                cmd9.Parameters.AddWithValue("@purchase_object_info", purName)
                |> ignore

                cmd9.Parameters.AddWithValue("@type_fz", typeFz)
                |> ignore

                cmd9.Parameters.AddWithValue("@id_organizer", !IdOrg)
                |> ignore

                cmd9.Parameters.AddWithValue("@id_placing_way", !idPlacingWay)
                |> ignore

                cmd9.Parameters.AddWithValue("@id_etp", idEtp)
                |> ignore

                cmd9.Parameters.AddWithValue("@end_date", dateEnd)
                |> ignore

                cmd9.Parameters.AddWithValue("@scoring_date", dateScoring)
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
                | true -> incr TenderEtpRt.tenderUpCount
                | false -> incr TenderEtpRt.tenderCount

                let idCustomer = ref 0

                let cusName =
                    InlineHtmlNavigator nav "//div[contains(@class, 'row filials')]/span"

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

                        let cmd14 =
                            new MySqlCommand(insertCustomer, con)

                        cmd14.Prepare()

                        cmd14.Parameters.AddWithValue("@reg_num", RegNum)
                        |> ignore

                        cmd14.Parameters.AddWithValue("@full_name", cusName)
                        |> ignore

                        cmd14.Parameters.AddWithValue("@inn", "")
                        |> ignore

                        cmd14.ExecuteNonQuery() |> ignore
                        idCustomer := int cmd14.LastInsertedId

                this.GetLots(con, !idTender, !idCustomer, htmlDoc, purName)
                this.GetAttachments(con, !idTender, htmlDoc)
                this.AddVerNumber con tn.PurNum stn typeFz
                this.TenderKwords con (!idTender) stn
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

    member private this.GetAttachments(con: MySqlConnection, idTender: int, doc: HtmlDocument) =
        let nav =
            (doc.CreateNavigator()) :?> HtmlNodeNavigator

        let specUrl =
            nav.GsnAtr "//a[@id = 'docs']" "href"

        if specUrl <> "" then
            let Page =
                Download.DownloadString
                <| sprintf "https://etp-rt.ru%s" specUrl

            if Page = "" || Page = null then
                failwith
                <| sprintf "cannot download documentation %s" tn.Href

            let htmlDoc = HtmlDocument()
            htmlDoc.LoadHtml(Page)

            let nav =
                (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator

            let docs =
                htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'lot_docs')]//a")

            if docs <> null then
                for d in docs do
                    let docName = d.Gsn(".")
                    let mutable docUrl = d.GsnAtr "." "href"

                    if not (docUrl.StartsWith("http://zakupki.gov.ru")) then
                        docUrl <- sprintf "https://etp-rt.ru%s" docUrl

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

        ()

    member private this.GetLots(con: MySqlConnection, idTender: int, idCustomer, doc: HtmlDocument, purName: String) =
        let lotNum = ref 1

        let nav =
            (doc.CreateNavigator()) :?> HtmlNodeNavigator

        let nmckT =
            InlineHtmlNavigator nav "//div[contains(@class, 'start_price price_list')]//div[@class = 'rouble']"

        let nmck = nmckT.GetPriceFromString()
        let currency = "Рублей"
        let idLot = ref 0

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

        cmd12.Parameters.AddWithValue("@currency", currency)
        |> ignore

        cmd12.Parameters.AddWithValue("@lot_name", purName)
        |> ignore

        cmd12.ExecuteNonQuery() |> ignore
        idLot := int cmd12.LastInsertedId

        let specUrl =
            nav.GsnAtr "//a[@id = 'specs']" "href"

        if specUrl <> "" then
            let Page =
                Download.DownloadString
                <| sprintf "https://etp-rt.ru%s" specUrl

            if Page = "" || Page = null then
                failwith
                <| sprintf "cannot download specification %s" tn.Href

            let htmlDoc = HtmlDocument()
            htmlDoc.LoadHtml(Page)

            let nav =
                (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator

            let lots =
                htmlDoc.DocumentNode.SelectNodes("//table[contains(@class, 'table-condensed')]/tbody//tr[not(@class)]")

            if lots <> null then
                for l in lots do
                    let lotName = l.Gsn("./td[2]")
                    let quantity = l.Gsn("./td[4]")
                    let quantity = quantity.HtmlDecode()
                    let okei = l.Gsn("./td[5]")
                    let price = l.Gsn("./td[6]")
                    //let price = price.HtmlDecode()
                    let price =
                        price
                            .Replace("&nbsp;", "")
                            .Replace(",", ".")
                            .Replace("&le;", "")
                            .RegexDeleteWhitespace()

                    let sum = l.Gsn("./td[7]")
                    //let sum = sum.HtmlDecode()
                    let sum =
                        sum
                            .Replace("&nbsp;", "")
                            .Replace(",", ".")
                            .RegexDeleteWhitespace()

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

                    cmd19.Parameters.AddWithValue("@sum", sum)
                    |> ignore

                    cmd19.Parameters.AddWithValue("@price", price)
                    |> ignore

                    cmd19.Parameters.AddWithValue("@quantity_value", quantity)
                    |> ignore

                    cmd19.Parameters.AddWithValue("@customer_quantity_value", quantity)
                    |> ignore

                    cmd19.Parameters.AddWithValue("@okei", okei)
                    |> ignore

                    cmd19.Parameters.AddWithValue("@okpd2_code", "")
                    |> ignore

                    cmd19.ExecuteNonQuery() |> ignore
                    let delivTerm1 = l.Gsn("./td[9]")
                    let delivTerm2 = l.Gsn("./td[10]")

                    let delivTerm =
                        sprintf "%s\n%s" delivTerm1 delivTerm2

                    if delivTerm1 <> "" || delivTerm2 <> "" then
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

                        cmd16.Parameters.AddWithValue("@delivery_place", "")
                        |> ignore

                        cmd16.Parameters.AddWithValue("@application_guarantee_amount", "")
                        |> ignore

                        cmd16.Parameters.AddWithValue("@contract_guarantee_amount", "")
                        |> ignore

                        cmd16.Parameters.AddWithValue("@delivery_term", delivTerm)
                        |> ignore

                        cmd16.ExecuteNonQuery() |> ignore

                    ()

            ()
        else
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

            cmd19.Parameters.AddWithValue("@name", purName)
            |> ignore

            cmd19.Parameters.AddWithValue("@sum", nmck)
            |> ignore

            cmd19.Parameters.AddWithValue("@price", "")
            |> ignore

            cmd19.Parameters.AddWithValue("@quantity_value", "")
            |> ignore

            cmd19.Parameters.AddWithValue("@customer_quantity_value", "")
            |> ignore

            cmd19.Parameters.AddWithValue("@okei", "")
            |> ignore

            cmd19.Parameters.AddWithValue("@okpd2_code", "")
            |> ignore

            cmd19.ExecuteNonQuery() |> ignore

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
