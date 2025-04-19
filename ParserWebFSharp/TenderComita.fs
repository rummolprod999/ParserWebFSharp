namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open System.Threading
open Newtonsoft.Json.Linq
open TypeE
open OpenQA.Selenium.Chrome
open NewtonExt

type TenderComita(stn: Settings.T, tn: ComitaRec, typeFz: int, etpName: string, etpUrl: string, driver: ChromeDriver) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0

    override this.Parsing() =
        let dateUpd = DateTime.Now
        use con = new MySqlConnection(stn.ConStr)
        con.Open()

        let selectTend =
            sprintf
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND doc_publish_date = @doc_publish_date AND notice_version = @notice_version"
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

        cmd.Parameters.AddWithValue("@notice_version", tn.Status)
        |> ignore

        let reader: MySqlDataReader =
            cmd.ExecuteReader()

        if reader.HasRows then
            reader.Close()
        else
            reader.Close()
            let href = tn.Href
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
            driver.Navigate().GoToUrl(tn.Href)
            Thread.Sleep(5000)
            driver.SwitchTo().DefaultContent() |> ignore
            
            let Page = Download.DownloadString (tn.Href.Replace("openProcedure", "rest"))
            let s =
                match Page with
                | null
                | "" ->
                    raise
                    <| Exception(sprintf "cannot get Page %s" tn.Href)
                | s -> s
            let t = JObject.Parse(s)
            
            let OrgName =
                match driver.findElementWithoutException ("//div[span[contains(., 'Опубликовано:')]]") with
                | "" -> ""
                | x -> x.Replace("Опубликовано:", "").Trim()

            if OrgName <> "" then
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
                            "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address, contact_phone = @contact_phone, inn = @inn, contact_email = @contact_email"
                            stn.Prefix

                    let contactPerson =
                        match driver.findElementWithoutException ("//div[span[contains(., 'Контактное лицо:')]]") with
                        | "" -> ""
                        | x -> x.Replace("Контактное лицо:", "").Trim()

                    let postAddress = ""
                    let factAddress = ""

                    let phone =
                        match driver.findElementWithoutException ("//div[span[contains(., 'Телефон:')]]") with
                        | "" -> ""
                        | x -> x.Replace("Телефон:", "").Trim()

                    let email =
                        match driver.findElementWithoutException ("//div[span[contains(., 'Электронная почта:')]]") with
                        | "" -> ""
                        | x -> x.Replace("Электронная почта:", "").Trim()

                    let inn = GetStringFromJtoken t "result.customer.inn"

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

                    cmd5.Parameters.AddWithValue("@contact_email", email)
                    |> ignore

                    cmd5.ExecuteNonQuery() |> ignore
                    IdOrg := int cmd5.LastInsertedId
                    ()

            let idPlacingWay = ref 0

            let PwayName =
                match
                    driver.findElementWithoutException
                        ("//div[contains(@class, 'procedure-line') and contains(@class, 'purchase')]/b")
                    with
                | "" -> ""
                | x -> x.Trim()

            match PwayName with
            | "" -> ()
            | _ ->
                idPlacingWay
                := this.GetPlacingWay con PwayName settings

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

            cmd9.Parameters.AddWithValue("@notice_version", tn.Status)
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
            | true -> incr TenderComita.tenderUpCount
            | false -> incr TenderComita.tenderCount
            
            let docs = t.GetElements("result.files")
            this.AddDocs(docs, !idTender, con)
            let prot = t.GetElements("result.protocols")
            this.AddProt(prot, !idTender, con, href)
            let idCustomer = ref 0

            let CusName =
                match driver.findElementWithoutException ("//div[span[contains(., 'Заказчик:')]]") with
                | "" -> ""
                | x -> x.Replace("Заказчик:", "").Trim()

            if CusName <> "" then
                let selectCustomer =
                    sprintf "SELECT id_customer FROM %scustomer WHERE full_name = @full_name" stn.Prefix

                let cmd3 =
                    new MySqlCommand(selectCustomer, con)

                cmd3.Prepare()

                cmd3.Parameters.AddWithValue("@full_name", CusName)
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
                    let inn = GetStringFromJtoken t "result.customer.inn"

                    let cmd14 =
                        new MySqlCommand(insertCustomer, con)

                    cmd14.Prepare()

                    cmd14.Parameters.AddWithValue("@reg_num", RegNum)
                    |> ignore

                    cmd14.Parameters.AddWithValue("@full_name", CusName)
                    |> ignore

                    cmd14.Parameters.AddWithValue("@inn", inn)
                    |> ignore

                    cmd14.ExecuteNonQuery() |> ignore
                    idCustomer := int cmd14.LastInsertedId

            let lots =
                t.GetElements("result.lots")

            let LotNum = ref 1

            for lot in lots do
                let LotName = GetStringFromJtoken lot "name"

                let Nmck = GetDecimalFromJtoken lot "price"

                let idLot = ref 0

                let insertLot =
                    sprintf
                        "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency"
                        stn.Prefix

                let cmd12 = new MySqlCommand(insertLot, con)

                cmd12.Parameters.AddWithValue("@id_tender", !idTender)
                |> ignore

                cmd12.Parameters.AddWithValue("@lot_number", !LotNum)
                |> ignore

                cmd12.Parameters.AddWithValue("@max_price", Nmck)
                |> ignore

                cmd12.Parameters.AddWithValue("@currency", tn.Currency)
                |> ignore

                cmd12.ExecuteNonQuery() |> ignore
                idLot := int cmd12.LastInsertedId
                incr LotNum
                
                let purObj =  lot.GetElements("items")
                if purObj.Count < 1 then
                    let insertLotitem =
                        sprintf
                            "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum"
                            stn.Prefix

                    let cmd19 =
                        new MySqlCommand(insertLotitem, con)

                    cmd19.Prepare()

                    cmd19.Parameters.AddWithValue("@id_lot", !idLot)
                    |> ignore

                    cmd19.Parameters.AddWithValue("@id_customer", !idCustomer)
                    |> ignore

                    cmd19.Parameters.AddWithValue("@name", LotName)
                    |> ignore

                    cmd19.Parameters.AddWithValue("@sum", Nmck)
                    |> ignore

                    cmd19.ExecuteNonQuery() |> ignore
                    ()
                else
                    for p in purObj do
                        let okpd2 = GetStringFromJtoken p "okpd2Code"
                        let okpd2Name = GetStringFromJtoken p "okpd2Name"
                        let name = GetStringFromJtoken p "name"
                        let insertLotitem =
                                        sprintf
                                            "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, okpd_name = @okpd_name, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei, okpd2_code = @okpd2_code"
                                            stn.Prefix

                        let cmd19 =
                            new MySqlCommand(insertLotitem, con)

                        cmd19.Prepare()

                        cmd19.Parameters.AddWithValue("@id_lot", !idLot)
                        |> ignore

                        cmd19.Parameters.AddWithValue("@id_customer", !idCustomer)
                        |> ignore

                        cmd19.Parameters.AddWithValue("@name", name)
                        |> ignore

                        cmd19.Parameters.AddWithValue("@okpd_name", okpd2Name)
                        |> ignore

                        cmd19.Parameters.AddWithValue("@quantity_value", "")
                        |> ignore

                        cmd19.Parameters.AddWithValue("@customer_quantity_value", "")
                        |> ignore

                        cmd19.Parameters.AddWithValue("@okei", "")
                        |> ignore

                        cmd19.Parameters.AddWithValue("@okpd2_code", okpd2)
                        |> ignore

                        cmd19.ExecuteNonQuery() |> ignore
                        ()
                        ()

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
    
    member private this.AddDocs(docs, idTender, con: MySqlConnection) =
        for d in docs do
            let docUrl = GetStringFromJtoken d "url"

            let docName =
                GetStringFromJtoken d "fileName"

            let addAttach =
                    sprintf
                        "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description"
                        stn.Prefix

            let cmd5 = new MySqlCommand(addAttach, con)

            cmd5.Parameters.AddWithValue("@id_tender", idTender)
            |> ignore

            cmd5.Parameters.AddWithValue("@file_name", docName)
            |> ignore

            cmd5.Parameters.AddWithValue("@url", docUrl)
            |> ignore

            cmd5.Parameters.AddWithValue("@description", "")
            |> ignore

            cmd5.ExecuteNonQuery() |> ignore
            ()
    
    member private this.AddProt(docs, idTender, con: MySqlConnection, href) =
        for d in docs do
            let id = GetStringFromJtoken d "id"
            
            let prot = d.GetElements("files")
            for p in prot do
                let id2 = GetStringFromJtoken p "id"
                let docName =
                    GetStringFromJtoken p "fileName"
                let url = href.Replace("openProcedure", "rest/fs/file") + "/protocols/"  + id + "/files/" + id2

                let addAttach =
                        sprintf
                            "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description"
                            stn.Prefix

                let cmd5 = new MySqlCommand(addAttach, con)

                cmd5.Parameters.AddWithValue("@id_tender", idTender)
                |> ignore

                cmd5.Parameters.AddWithValue("@file_name", docName)
                |> ignore

                cmd5.Parameters.AddWithValue("@url", url)
                |> ignore

                cmd5.Parameters.AddWithValue("@description", "")
                |> ignore

                cmd5.ExecuteNonQuery() |> ignore
            ()
