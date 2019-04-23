namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System.Threading
open TypeE
open Tools

type TenderTsm(stn : Settings.T, tn : SamoletRec, typeFz : int, etpName : string, etpUrl : string, driver : ChromeDriver) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() =
            let inline InlineFEWEBuilder (x : ^a) (s : string) (exc : string) =
                try
                    let res = (^a : (member FindElement : By -> IWebElement) (x, By.XPath(s)))
                    match res with
                    | null -> Error(exc)
                    | r -> Success(r.Text.Trim())
                with ex -> Error(exc)
            let builder = TenderBuilder()
            use con = new MySqlConnection(stn.ConStr)
            let dateUpd = DateTime.Now
            let res =
                   builder {
                        driver.Navigate().GoToUrl(tn.Href)
                        let wait = new WebDriverWait(driver, timeoutB)
                        Thread.Sleep(5000)
                        driver.SwitchTo().DefaultContent() |> ignore
                        wait.Until (fun dr -> (dr.FindElement (By.XPath ("//div[contains(@class, 'mat-tab-label-content') and contains(., 'Извещение и документация')]"))).Displayed) |> ignore
                        wait.Until(fun dr -> (dr.FindElement(By.XPath("//iframe"))).Displayed) |> ignore
                        driver.SwitchTo().Frame(0) |> ignore
                        wait.Until (fun dr -> (dr.FindElement (By.XPath ("//td[contains(., 'Начало приема предложений')]/following-sibling::td//div[@class = 'translate-text-ru']"))).Displayed) |> ignore
                        let! datePubTT = driver.findElementWithoutException ("//td[contains(., 'Начало приема предложений')]/following-sibling::td//div[@class = 'translate-text-ru']", sprintf "datePubTT not found %s" tn.Href)
                        let! datePub = datePubTT.DateFromString("dd.MM.yyyy HH:mm", sprintf "datePub not parse %s" datePubTT)
                        let! dateEndTT = driver.findElementWithoutException ("//td[contains(., 'Окончание приема предложений')]/following-sibling::td//div[@class = 'translate-text-ru']/div", sprintf "dateEndTT not found %s" tn.Href)
                        let! dateEndT = dateEndTT.Get1 ("(\d{2}\.\d{2}\.\d{4}\s+\d+:\d+)", sprintf "dateEndT not found %s" dateEndTT)
                        let! dateEnd = dateEndT.DateFromString("dd.MM.yyyy HH:mm", sprintf "dateEnd not parse %s" dateEndT)
                        let status = driver.findElementWithoutException ("//td[contains(., 'Статус торгов')]/following-sibling::td//div[@class = 'translate-text-ru']")
                        con.Open()
                        let selectTend = sprintf "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND doc_publish_date = @doc_publish_date AND notice_version = @notice_version" stn.Prefix
                        let cmd : MySqlCommand = new MySqlCommand(selectTend, con)
                        cmd.Prepare()
                        cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
                        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
                        cmd.Parameters.AddWithValue("@end_date", dateEnd) |> ignore
                        cmd.Parameters.AddWithValue("@doc_publish_date", datePub) |> ignore
                        cmd.Parameters.AddWithValue("@notice_version", status) |> ignore
                        let reader : MySqlDataReader = cmd.ExecuteReader()
                        if reader.HasRows then reader.Close()
                                               return! Error ""
                        reader.Close()
                        let (cancelStatus, updated) = this.SetCancelStatus(con, dateUpd)
                        let Printform = tn.Href

                        let IdOrg = ref 0
                        let! orgName = InlineFEWEBuilder driver "//td[contains(., 'Наименование')]/following-sibling::td//div[@class = 'translate-text-ru']" (sprintf "org name not found %s" tn.Href)
                        if orgName <> "" then
                            let selectOrg = sprintf "SELECT id_organizer FROM %sorganizer WHERE full_name = @full_name" stn.Prefix
                            let cmd3 = new MySqlCommand(selectOrg, con)
                            cmd3.Prepare()
                            cmd3.Parameters.AddWithValue("@full_name", orgName) |> ignore
                            let reader = cmd3.ExecuteReader()
                            match reader.HasRows with
                            | true ->
                                reader.Read() |> ignore
                                IdOrg := reader.GetInt32("id_organizer")
                                reader.Close()
                            | false ->
                                reader.Close()
                                let addOrganizer = sprintf "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address, contact_phone = @contact_phone, inn = @inn" stn.Prefix
                                let contactPerson = InlineFEWE driver "//td[contains(., 'Контактное лицо')]/following-sibling::td//div[@class = 'translate-text-ru']"
                                let postAddress = InlineFEWE driver "//td[contains(., 'Почтовый адрес')]/following-sibling::td//div[@class = 'translate-text-ru']"
                                let factAddress = InlineFEWE driver "//td[contains(., 'Юридический адрес (место нахождения)')]/following-sibling::td//div[@class = 'translate-text-ru']"
                                let phone = InlineFEWE driver "//td[contains(., 'Номер контактного телефона')]/following-sibling::td//div[@class = 'translate-text-ru']"
                                let inn = ""
                                let cmd5 = new MySqlCommand(addOrganizer, con)
                                cmd5.Parameters.AddWithValue("@full_name", orgName) |> ignore
                                cmd5.Parameters.AddWithValue("@contact_person", contactPerson) |> ignore
                                cmd5.Parameters.AddWithValue("@post_address", postAddress) |> ignore
                                cmd5.Parameters.AddWithValue("@fact_address", factAddress) |> ignore
                                cmd5.Parameters.AddWithValue("@contact_phone", phone) |> ignore
                                cmd5.Parameters.AddWithValue("@inn", inn) |> ignore
                                cmd5.ExecuteNonQuery() |> ignore
                                IdOrg := int cmd5.LastInsertedId
                                ()
                        let idPlacingWay = ref 0
                        let idEtp = this.GetEtp con settings
                        let numVersion = 1
                        let idRegion = 0
                        let idTender = ref 0
                        let insertTender = String.Format ("INSERT INTO {0}tender SET id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form, id_region = @id_region", stn.Prefix)
                        let cmd9 = new MySqlCommand(insertTender, con)
                        cmd9.Prepare()
                        cmd9.Parameters.AddWithValue("@id_xml", tn.PurNum) |> ignore
                        cmd9.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
                        cmd9.Parameters.AddWithValue("@doc_publish_date", datePub) |> ignore
                        cmd9.Parameters.AddWithValue("@href", tn.Href) |> ignore
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
                        cmd9.Parameters.AddWithValue("@notice_version", status) |> ignore
                        cmd9.Parameters.AddWithValue("@xml", tn.Href) |> ignore
                        cmd9.Parameters.AddWithValue("@print_form", Printform) |> ignore
                        cmd9.Parameters.AddWithValue("@id_region", idRegion) |> ignore
                        cmd9.ExecuteNonQuery() |> ignore
                        idTender := int cmd9.LastInsertedId
                        match updated with
                        | true -> incr TenderTsm.tenderUpCount
                        | false -> incr TenderTsm.tenderCount
                        let idCustomer = ref 0
                        if orgName <> "" then
                            let selectCustomer =
                                sprintf "SELECT id_customer FROM %scustomer WHERE full_name = @full_name" stn.Prefix
                            let cmd3 = new MySqlCommand(selectCustomer, con)
                            cmd3.Prepare()
                            cmd3.Parameters.AddWithValue("@full_name", orgName) |> ignore
                            let reader = cmd3.ExecuteReader()
                            match reader.HasRows with
                            | true ->
                                reader.Read() |> ignore
                                idCustomer := reader.GetInt32("id_customer")
                                reader.Close()
                            | false ->
                                reader.Close()
                                let insertCustomer =
                                    sprintf "INSERT INTO %scustomer SET reg_num = @reg_num, full_name = @full_name, inn = @inn"
                                        stn.Prefix
                                let RegNum = Guid.NewGuid().ToString()
                                let inn = ""
                                let cmd14 = new MySqlCommand(insertCustomer, con)
                                cmd14.Prepare()
                                cmd14.Parameters.AddWithValue("@reg_num", RegNum) |> ignore
                                cmd14.Parameters.AddWithValue("@full_name", orgName) |> ignore
                                cmd14.Parameters.AddWithValue("@inn", inn) |> ignore
                                cmd14.ExecuteNonQuery() |> ignore
                                idCustomer := int cmd14.LastInsertedId

                        let attach = match driver.FindElementByXPath("//a[contains(., 'Скачать все приложенные файлы')]") with
                                     | null -> ""
                                     | at -> at.GetAttribute("href")
                        if attach <> "" then
                              let addAttach = sprintf "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description" stn.Prefix
                              let cmd5 = new MySqlCommand(addAttach, con)
                              cmd5.Parameters.AddWithValue("@id_tender", idTender) |> ignore
                              cmd5.Parameters.AddWithValue("@file_name", "Скачать все приложенные файлы") |> ignore
                              cmd5.Parameters.AddWithValue("@url", attach) |> ignore
                              cmd5.Parameters.AddWithValue("@description", "") |> ignore
                              cmd5.ExecuteNonQuery() |> ignore

                        let idLot = ref 0
                        let currency = InlineFEWE driver "//td[contains(., 'Валюта')]/following-sibling::td//div[@class = 'translate-text-ru']"
                        let insertLot = sprintf "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency" stn.Prefix
                        let cmd12 = new MySqlCommand(insertLot, con)
                        cmd12.Parameters.AddWithValue("@id_tender", !idTender) |> ignore
                        cmd12.Parameters.AddWithValue("@lot_number", 1) |> ignore
                        cmd12.Parameters.AddWithValue("@max_price", "") |> ignore
                        cmd12.Parameters.AddWithValue("@currency", currency) |> ignore
                        cmd12.ExecuteNonQuery() |> ignore
                        idLot := int cmd12.LastInsertedId
                        this.GetPurchaseObjects(driver, con, !idLot, !idCustomer)
                        this.AddVerNumber con tn.PurNum stn typeFz
                        this.TenderKwords con (!idTender) stn
                        return ""
                   }
            match res with
                | Success r -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
            ()

    member private this.SetCancelStatus(con : MySqlConnection, dateUpd : DateTime) =
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

    member private this.GetPurchaseObjects(driver : ChromeDriver, con : MySqlConnection, idLot : int, idCustomer : int) =
        let delivPlace = InlineFEWE driver "//td[contains(., 'Условия поставки товаров')]/following-sibling::td//div[@class = 'translate-text-ru']"
        let purObjects = driver.FindElements(By.XPath("//table[@class = 'data competitive-table-data']/tbody/tr"))
        for po in purObjects do
            if not (po.Text.Contains("Номенклатурный")) then
                let poName1 = InlineFEWE po "./td[3]"
                let poName2 = InlineFEWE po "./td[4]"
                let poName = (sprintf "%s %s" poName1 poName2).Trim()
                let okei = InlineFEWE po "./td[5]"
                let quant = InlineFEWE po "./td[6]"
                let delivTerm = InlineFEWE po "./td[7]"
                let insertLotitem = sprintf "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum, price = @price, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei" stn.Prefix
                let cmd19 = new MySqlCommand(insertLotitem, con)
                cmd19.Prepare()
                cmd19.Parameters.AddWithValue("@id_lot", idLot) |> ignore
                cmd19.Parameters.AddWithValue("@id_customer", idCustomer) |> ignore
                cmd19.Parameters.AddWithValue("@name", poName) |> ignore
                cmd19.Parameters.AddWithValue("@sum", "") |> ignore
                cmd19.Parameters.AddWithValue("@price", "") |> ignore
                cmd19.Parameters.AddWithValue("@quantity_value", quant) |> ignore
                cmd19.Parameters.AddWithValue("@customer_quantity_value", quant) |> ignore
                cmd19.Parameters.AddWithValue("@okei", okei) |> ignore
                cmd19.ExecuteNonQuery() |> ignore
                if delivPlace <> "" || delivTerm <> "" then
                        let insertCustomerRequirement = sprintf "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, delivery_term = @delivery_term" stn.Prefix
                        let cmd16 = new MySqlCommand(insertCustomerRequirement, con)
                        cmd16.Prepare()
                        cmd16.Parameters.AddWithValue("@id_lot", idLot) |> ignore
                        cmd16.Parameters.AddWithValue("@id_customer", idCustomer) |> ignore
                        cmd16.Parameters.AddWithValue("@delivery_place", delivPlace) |> ignore
                        cmd16.Parameters.AddWithValue("@delivery_term", delivTerm) |> ignore
                        cmd16.ExecuteNonQuery() |> ignore

            ()

        ()
