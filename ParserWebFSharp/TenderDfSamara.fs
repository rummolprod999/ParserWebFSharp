namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type TenderDfSamara
    (
        stn: Settings.T,
        tn: YarRegionRec,
        typeFz: int,
        etpName: string,
        etpUrl: string,
        driver: ChromeDriver
    ) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0

    override this.Parsing() =
        let dateUpd = DateTime.Now

        let wait =
            WebDriverWait(driver, TimeSpan.FromSeconds(60.))
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//div[@class = 'text-control']//strong[contains(., '№')]")
                )
                .Displayed)
        |> ignore
        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//div[. = 'Опубликовано']/following-sibling::div/p")
                )
                .Displayed)
        |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        if driver.Url = "https://torgi.dfsamara.ru/vitrina-magazina-malogo-obema/" then
            failwith "start page"

        let PurNum =
            (driver.findElementWithoutException ("//div[@class = 'text-control']//strong[contains(., '№')]"))
                .Replace("№", "")
                .Replace(".", "")
                .RegexDeleteWhitespace()

        if PurNum = "" then
            failwith ("empty purnum " + driver.Url)

        let dateEndTT =
            driver.findElementWithoutException ("//div[. = 'Окончание подачи заявок']/following-sibling::div")

        let dateEndT =
            dateEndTT.Get1FromRegexpOrDefaul(@"(\d{2}\.\d{2}\.\d{4}\s+\d{2}:\d{2}:\d{2})")

        let DateEnd =
            dateEndT.DateFromStringOrMin("dd.MM.yyyy HH:mm:ss")

        let datePubTT =
            driver.findElementWithoutException ("//div[. = 'Опубликовано']/following-sibling::div/p")

        let datePubT =
            datePubTT.Get1FromRegexpOrDefaul(@"(\d{2}\.\d{2}\.\d{4})")

        let DatePub =
            datePubT.DateFromStringOrMin("dd.MM.yyyy")

        if DatePub = DateTime.MinValue then
            failwith ("empty date pub " + driver.Url)

        let Status =
            driver.findElementWithoutException ("//div[. = 'Состояние']/following-sibling::div/span")

        use con = new MySqlConnection(stn.ConStr)
        con.Open()

        let selectTend =
            sprintf
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND doc_publish_date = @doc_publish_date AND notice_version = @notice_version"
                stn.Prefix

        let cmd: MySqlCommand =
            new MySqlCommand(selectTend, con)

        cmd.Prepare()

        cmd.Parameters.AddWithValue("@purchase_number", PurNum)
        |> ignore

        cmd.Parameters.AddWithValue("@type_fz", typeFz)
        |> ignore

        cmd.Parameters.AddWithValue("@doc_publish_date", DatePub)
        |> ignore

        cmd.Parameters.AddWithValue("@notice_version", Status)
        |> ignore

        let reader: MySqlDataReader =
            cmd.ExecuteReader()

        if reader.HasRows then
            reader.Close()
        else
            reader.Close()
            let timeoutB = TimeSpan.FromSeconds(30.)
            let href = driver.Url
            let mutable cancelStatus = 0
            let mutable updated = false

            let selectDateT =
                sprintf
                    "SELECT id_tender, date_version, cancel FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz"
                    stn.Prefix

            let cmd2 =
                new MySqlCommand(selectDateT, con)

            cmd2.Prepare()

            cmd2.Parameters.AddWithValue("@purchase_number", PurNum)
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
            let mutable inn = ""

            let PurName =
                driver.findElementWithoutException ("//h2/p")

            let CusName =
                driver.findElementWithoutException ("//div[. = 'Заказчик']/following-sibling::div/p")

            if CusName <> "" then
                let selectOrg =
                    sprintf "SELECT id_organizer FROM %sorganizer WHERE full_name = @full_name" stn.Prefix

                let cmd3 = new MySqlCommand(selectOrg, con)
                cmd3.Prepare()

                cmd3.Parameters.AddWithValue("@full_name", CusName)
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
                            "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address, contact_phone = @contact_phone, inn = @inn, contact_email = @contact_email, kpp = @kpp"
                            stn.Prefix

                    let contactPerson =
                        driver.findElementWithoutException (
                            "//div[. = 'Ответственное должностное лицо']/following-sibling::div"
                        )

                    let postAddress = ""
                    let factAddress = ""
                    let phone = ""
                    let email = ""
                    let kpp = ""

                    let cmd5 =
                        new MySqlCommand(addOrganizer, con)

                    cmd5.Parameters.AddWithValue("@full_name", CusName)
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

                    cmd5.Parameters.AddWithValue("@kpp", kpp)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@contact_email", email)
                    |> ignore

                    cmd5.ExecuteNonQuery() |> ignore
                    IdOrg := int cmd5.LastInsertedId
                    ()

            let idPlacingWay = ref 0

            let PwayName = ""

            match PwayName with
            | "" -> ()
            | _ ->
                idPlacingWay
                := this.GetPlacingWay con PwayName settings

            let idEtp = this.GetEtp con settings
            let numVersion = 1
            let mutable idRegion = 0
            let regionS = Tools.GetRegionString("самар")

            if regionS <> "" then
                let selectReg =
                    sprintf "SELECT id FROM %sregion WHERE name LIKE @name" stn.Prefix

                let cmd46 = new MySqlCommand(selectReg, con)
                cmd46.Prepare()

                cmd46.Parameters.AddWithValue("@name", "%" + regionS + "%")
                |> ignore

                let reader36 = cmd46.ExecuteReader()

                match reader36.HasRows with
                | true ->
                    reader36.Read() |> ignore
                    idRegion <- reader36.GetInt32("id")
                    reader36.Close()
                | false -> reader36.Close()

            let idTender = ref 0

            let insertTender =
                String.Format(
                    "INSERT INTO {0}tender SET id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form, id_region = @id_region",
                    stn.Prefix
                )

            let cmd9 =
                new MySqlCommand(insertTender, con)

            cmd9.Prepare()

            cmd9.Parameters.AddWithValue("@id_xml", PurNum)
            |> ignore

            cmd9.Parameters.AddWithValue("@purchase_number", PurNum)
            |> ignore

            cmd9.Parameters.AddWithValue("@doc_publish_date", DatePub)
            |> ignore

            cmd9.Parameters.AddWithValue("@href", href)
            |> ignore

            cmd9.Parameters.AddWithValue("@purchase_object_info", PurName)
            |> ignore

            cmd9.Parameters.AddWithValue("@type_fz", typeFz)
            |> ignore

            cmd9.Parameters.AddWithValue("@id_organizer", !IdOrg)
            |> ignore

            cmd9.Parameters.AddWithValue("@id_placing_way", !idPlacingWay)
            |> ignore

            cmd9.Parameters.AddWithValue("@id_etp", idEtp)
            |> ignore

            cmd9.Parameters.AddWithValue("@end_date", DateEnd)
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

            cmd9.Parameters.AddWithValue("@notice_version", Status)
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
            | true -> incr TenderDfSamara.tenderUpCount
            | false -> incr TenderDfSamara.tenderCount

            let idCustomer = ref 0

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

            let documents =
                driver.findElementsWithoutException (
                    "(//td[label[contains(., 'Проект контракта')]] | //td[label[contains(., 'Техническое задание')]])/following-sibling::td/span/a"
                )

            for doc in documents do
                let docName = doc.Text.RegexCutWhitespace()

                match docName with
                | "" -> ()
                | x ->
                    let href = doc.GetAttribute("href")

                    let addAttach =
                        sprintf
                            "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url"
                            stn.Prefix

                    let cmd5 = new MySqlCommand(addAttach, con)

                    cmd5.Parameters.AddWithValue("@id_tender", !idTender)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@file_name", x)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@url", href)
                    |> ignore

                    cmd5.ExecuteNonQuery() |> ignore

            let idLot = ref 0
            let fin_source = ""

            let Nmck =
                (driver.findElementWithoutException (
                    "//div[@class = 'text-control']//strong[contains(., '№')]/following-sibling::strong"
                ))
                    .GetPriceFromString()

            let insertLot =
                sprintf
                    "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, finance_source = @finance_source"
                    stn.Prefix

            let cmd12 = new MySqlCommand(insertLot, con)

            cmd12.Parameters.AddWithValue("@id_tender", !idTender)
            |> ignore

            cmd12.Parameters.AddWithValue("@lot_number", 1)
            |> ignore

            cmd12.Parameters.AddWithValue("@max_price", Nmck)
            |> ignore

            cmd12.Parameters.AddWithValue("@finance_source", fin_source)
            |> ignore

            cmd12.Parameters.AddWithValue("@currency", "руб")
            |> ignore

            cmd12.ExecuteNonQuery() |> ignore
            idLot := int cmd12.LastInsertedId

            let delivPlace =
                driver.findElementWithoutException (
                    "//td[label[contains(., 'Место поставки товара, выполнения работ, оказания услуг')]]/following-sibling::td/span"
                )

            let delivTerm =
                driver.findElementWithoutException (
                    "//td[label[contains(., 'Срок поставки товара, выполнения работ, оказания услуг')]]/following-sibling::td/span"
                )

            if delivPlace <> "" || delivTerm <> "" then
                let insertCustomerRequirement =
                    sprintf
                        "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, delivery_term = @delivery_term"
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

                cmd16.Parameters.AddWithValue("@delivery_term", delivTerm)
                |> ignore

                cmd16.ExecuteNonQuery() |> ignore

            let purObjects =
                driver.findElementsWithoutException (
                    "//tr[contains(@class, 'x-grid-row') and td[div[div[@class= 'x-grid-row-expander']]]]"
                )

            for po in purObjects do
                let name =
                    po.findElementWithoutException ("./td[3]/div")

                let quantT =
                    po.findElementWithoutException ("./td[6]/div")

                let quantP =
                    quantT.Get2ListRegexp(@"([\d,]+)\s+-\s+(.+)")

                let quant = quantP.[0].Replace(",", ".")
                let okei = quantP.[1]

                let okpd2F =
                    po.findElementWithoutException ("./td[5]/div")

                let okpd2P =
                    okpd2F.Get2ListRegexp(@"([\d.]+)\s+-\s+(.+)")

                let okpd2 = okpd2P.[0]
                let okpd2Name = okpd2P.[1]

                let insertLotitem =
                    sprintf
                        "INSERT INTO %spurchase_object SET id_lot = @id_lot, name = @name, quantity_value = @quantity_value, customer_quantity_value = @customer_quantity_value, okei = @okei, okpd2_code = @okpd2_code, okpd_name = @okpd_name, id_customer = @id_customer"
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

                cmd19.Parameters.AddWithValue("@quantity_value", quant)
                |> ignore

                cmd19.Parameters.AddWithValue("@customer_quantity_value", quant)
                |> ignore

                cmd19.Parameters.AddWithValue("@okei", okei)
                |> ignore

                cmd19.Parameters.AddWithValue("@okpd2_code", okpd2)
                |> ignore

                cmd19.Parameters.AddWithValue("@okpd_name", okpd2Name)
                |> ignore

                cmd19.ExecuteNonQuery() |> ignore
                ()

            try
                this.AddVerNumber con PurNum stn typeFz
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
