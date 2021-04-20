namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System.Threading
open TypeE

type TenderSamolet(stn : Settings.T, tn : SamoletRec, typeFz : int, etpName : string, etpUrl : string, driver : ChromeDriver) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0

    override this.Parsing() =
        driver.Navigate().GoToUrl(tn.Href)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        let wait = WebDriverWait(driver, timeoutB)
        wait.Until
            (fun dr ->
            (dr.FindElement (By.XPath ("//div[contains(@class, 'mat-tab-label-content') and contains(., 'Извещение и документация')]"))).Displayed) |> ignore
        let dateEndT = driver.findElementWithoutException ("//div[. = 'Окончание представления предложений']/following-sibling::div")
        let dateEndT = match dateEndT with
                       | x when dateEndT.Contains("Завтра в") -> dateEndT.Replace("Завтра в", DateTime.Now.AddDays(1.).ToString("dd.MM.yyyy"))
                       | x when dateEndT.Contains("Сегодня в") -> dateEndT.Replace("Сегодня в", DateTime.Now.ToString("dd.MM.yyyy"))
                       | _ -> dateEndT
        let dateEndT = dateEndT.Get1FromRegexpOrDefaul("(\d{2}.\d{2}.\d{4} \d{2}:\d{2})")
        let dateEnd =
            match dateEndT.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> raise <| Exception(sprintf "cannot parse dateEndT %s, %s" dateEndT tn.Href)
        
        let datePubT = driver.findElementWithoutException ("//div[contains(., 'Начало представления предложений ')]/following-sibling::div")
        let datePubT = match datePubT with
                       | x when datePubT.Contains("Завтра в") -> datePubT.Replace("Завтра в", DateTime.Now.AddDays(1.).ToString("dd.MM.yyyy"))
                       | x when datePubT.Contains("Сегодня в") -> datePubT.Replace("Сегодня в", DateTime.Now.ToString("dd.MM.yyyy"))
                       | x when datePubT.Contains("Вчера в") -> datePubT.Replace("Вчера в", DateTime.Now.AddDays(-1.).ToString("dd.MM.yyyy"))
                       | _ -> datePubT
        let datePubT = datePubT.Get1FromRegexpOrDefaul("(\d{2}.\d{2}.\d{4} \d{2}:\d{2})")
        let datePub =
            match datePubT.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> raise <| Exception(sprintf "cannot parse datePubT %s, %s" datePubT tn.Href)
            
        let dateScoringT = driver.findElementWithoutException ("//div[. = 'Дата подведения итогов (ТК)']/following-sibling::div")
        let dateScoring =
            match dateScoringT.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> DateTime.MinValue
        let status = driver.findElementWithoutException ("//div[. = 'Наименование лота']/following-sibling::div/div/span")
        let dateUpd = DateTime.Now
        use con = new MySqlConnection(stn.ConStr)
        con.Open()
        let selectTend =
            sprintf
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND notice_version = @notice_version"
                stn.Prefix
        let cmd : MySqlCommand = new MySqlCommand(selectTend, con)
        cmd.Prepare()
        cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
        cmd.Parameters.AddWithValue("@end_date", dateEnd) |> ignore
        cmd.Parameters.AddWithValue("@notice_version", status) |> ignore
        let reader : MySqlDataReader = cmd.ExecuteReader()
        if reader.HasRows then reader.Close()
        else
            reader.Close()
            let href = tn.Href
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
            let OrgName = "ПАО \"ГК \"САМОЛЕТ"
            if OrgName <> "" then
                let selectOrg = sprintf "SELECT id_organizer FROM %sorganizer WHERE full_name = @full_name" stn.Prefix
                let cmd3 = new MySqlCommand(selectOrg, con)
                cmd3.Prepare()
                cmd3.Parameters.AddWithValue("@full_name", OrgName) |> ignore
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
                    let contactPerson = driver.findElementWithoutException ("//div[contains(., 'Контактное лицо')]/following-sibling::div/div")
                    let postAddress = "МОСКВА Г, ИВАНА ФРАНКО УЛ, ДОМ 8, ЭТАЖ/КОМН. 10/23"
                    let factAddress = ""
                    let phone = driver.findElementWithoutException ("//div[contains(., 'Адрес электронной почты')]/following-sibling::div/div")
                    let inn = "9731004688"
                    let cmd5 = new MySqlCommand(addOrganizer, con)
                    cmd5.Parameters.AddWithValue("@full_name", OrgName) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_person", contactPerson) |> ignore
                    cmd5.Parameters.AddWithValue("@post_address", postAddress) |> ignore
                    cmd5.Parameters.AddWithValue("@fact_address", factAddress) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_phone", phone) |> ignore
                    cmd5.Parameters.AddWithValue("@inn", inn) |> ignore
                    cmd5.ExecuteNonQuery() |> ignore
                    IdOrg := int cmd5.LastInsertedId
                    ()
            let placingWay = driver.findElementWithoutException ("//div[contains(., 'Способ закупки')]/following-sibling::div")
            let idPlacingWay = ref 0
            match placingWay with
            | "" -> ()
            | _ -> idPlacingWay := this.GetPlacingWay con placingWay settings
            let idEtp = this.GetEtp con settings
            let numVersion = 1
            let idRegion = 0
            let idTender = ref 0
            let insertTender =
                String.Format
                    ("INSERT INTO {0}tender SET id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form, id_region = @id_region",
                     stn.Prefix)
            let cmd9 = new MySqlCommand(insertTender, con)
            cmd9.Prepare()
            cmd9.Parameters.AddWithValue("@id_xml", tn.PurNum) |> ignore
            cmd9.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
            cmd9.Parameters.AddWithValue("@doc_publish_date", datePub) |> ignore
            cmd9.Parameters.AddWithValue("@href", href) |> ignore
            cmd9.Parameters.AddWithValue("@purchase_object_info", tn.PurName) |> ignore
            cmd9.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
            cmd9.Parameters.AddWithValue("@id_organizer", !IdOrg) |> ignore
            cmd9.Parameters.AddWithValue("@id_placing_way", !idPlacingWay) |> ignore
            cmd9.Parameters.AddWithValue("@id_etp", idEtp) |> ignore
            cmd9.Parameters.AddWithValue("@end_date", dateEnd) |> ignore
            cmd9.Parameters.AddWithValue("@scoring_date", dateScoring) |> ignore
            cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue) |> ignore
            cmd9.Parameters.AddWithValue("@cancel", cancelStatus) |> ignore
            cmd9.Parameters.AddWithValue("@date_version", dateUpd) |> ignore
            cmd9.Parameters.AddWithValue("@num_version", numVersion) |> ignore
            cmd9.Parameters.AddWithValue("@notice_version", status) |> ignore
            cmd9.Parameters.AddWithValue("@xml", href) |> ignore
            cmd9.Parameters.AddWithValue("@print_form", Printform) |> ignore
            cmd9.Parameters.AddWithValue("@id_region", idRegion) |> ignore
            cmd9.ExecuteNonQuery() |> ignore
            idTender := int cmd9.LastInsertedId
            match updated with
            | true -> incr TenderSamolet.tenderUpCount
            | false -> incr TenderSamolet.tenderCount
            let idCustomer = ref 0
            if OrgName <> "" then
                let selectCustomer =
                    sprintf "SELECT id_customer FROM %scustomer WHERE full_name = @full_name" stn.Prefix
                let cmd3 = new MySqlCommand(selectCustomer, con)
                cmd3.Prepare()
                cmd3.Parameters.AddWithValue("@full_name", OrgName) |> ignore
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
                    let inn = "9731004688"
                    let cmd14 = new MySqlCommand(insertCustomer, con)
                    cmd14.Prepare()
                    cmd14.Parameters.AddWithValue("@reg_num", RegNum) |> ignore
                    cmd14.Parameters.AddWithValue("@full_name", OrgName) |> ignore
                    cmd14.Parameters.AddWithValue("@inn", inn) |> ignore
                    cmd14.ExecuteNonQuery() |> ignore
                    idCustomer := int cmd14.LastInsertedId
            let attachments = driver.FindElements(By.XPath("//um-files-dropdown/a"))
            for att in attachments do
                        let urlAtt = att.GetAttribute("href")
                        let urlName = att.Text.Trim()
                        if urlAtt <> "" && urlName <> "" then
                             let addAttach = sprintf "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description" stn.Prefix
                             let cmd5 = new MySqlCommand(addAttach, con)
                             cmd5.Prepare()
                             cmd5.Parameters.AddWithValue("@id_tender", !idTender) |> ignore
                             cmd5.Parameters.AddWithValue("@file_name", urlName) |> ignore
                             cmd5.Parameters.AddWithValue("@url", urlAtt) |> ignore
                             cmd5.Parameters.AddWithValue("@description", "") |> ignore
                             cmd5.ExecuteNonQuery() |> ignore
                        ()
            let lots = driver.FindElements(By.XPath("//mat-expansion-panel"))
            let lotNum = ref 1
            for l in lots do
                let lotName = l.findElementWithoutException (".//div[. = 'Наименование лота']/following-sibling::div/div/span")
                let lotName = match lotName with
                              | "" -> tn.PurName
                              | x -> x
                let idLot = ref 0
                let insertLot =
                    sprintf
                        "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, lot_name = @lot_name"
                        stn.Prefix
                let cmd12 = new MySqlCommand(insertLot, con)
                cmd12.Parameters.AddWithValue("@id_tender", !idTender) |> ignore
                cmd12.Parameters.AddWithValue("@lot_number", !lotNum) |> ignore
                cmd12.Parameters.AddWithValue("@max_price", "") |> ignore
                cmd12.Parameters.AddWithValue("@currency", "RUB") |> ignore
                cmd12.Parameters.AddWithValue("@lot_name", lotName) |> ignore
                cmd12.ExecuteNonQuery() |> ignore
                idLot := int cmd12.LastInsertedId
                let insertLotitem =
                    sprintf
                        "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum"
                        stn.Prefix
                let cmd19 = new MySqlCommand(insertLotitem, con)
                let PoName = match lotName with
                             | "" -> tn.PurName
                             | _ -> lotName
                cmd19.Prepare()
                cmd19.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
                cmd19.Parameters.AddWithValue("@id_customer", !idCustomer) |> ignore
                cmd19.Parameters.AddWithValue("@name", PoName) |> ignore
                cmd19.Parameters.AddWithValue("@sum", "") |> ignore
                cmd19.ExecuteNonQuery() |> ignore
                incr lotNum
                let delivlace = driver.findElementWithoutException ("//div[contains(., 'Условия поставки')]/following-sibling::div")
                let delivTerm1 = driver.findElementWithoutException ("//div[contains(., 'Сроки поставки')]/following-sibling::div")
                let delivTerm2 = driver.findElementWithoutException ("//div[contains(., 'Условия оплаты')]/following-sibling::div")
                let delivTerm3 = driver.findElementWithoutException ("//div[contains(., 'Пояснение условий поставки')]/following-sibling::div")
                let delivTerm = sprintf "Сроки поставки: %s\nУсловия оплаты:%s\nПояснение условий поставки:%s" delivTerm1 delivTerm2 delivTerm3
                if delivlace <> "" || delivTerm <> "" then
                    let insertCustomerRequirement =
                        sprintf
                            "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, delivery_term = @delivery_term"
                            stn.Prefix
                    let cmd16 = new MySqlCommand(insertCustomerRequirement, con)
                    cmd16.Prepare()
                    cmd16.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
                    cmd16.Parameters.AddWithValue("@id_customer", !idCustomer) |> ignore
                    cmd16.Parameters.AddWithValue("@delivery_place", delivlace) |> ignore
                    cmd16.Parameters.AddWithValue("@delivery_term", delivTerm) |> ignore
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
