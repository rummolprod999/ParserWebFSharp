namespace ParserWeb

open MySql.Data.MySqlClient
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open System.Data
open System.Linq
open System.Text.RegularExpressions
open System.Threading
open TypeE

type TenderButb(stn : Settings.T, purNum : string, datePub : DateTime, endDate : DateTime, biddingDate : DateTime, driver : ChromeDriver, wait : WebDriverWait, page : int, status : string) =
    inherit Tender("ОАО «Белорусская универсальная товарная биржа»", "http://zakupki.butb.by")
    let settings = stn
    let typeFz = 36
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0
    
    override this.Parsing() =
        let dateUpd = datePub
        use con = new MySqlConnection(stn.ConStr)
        con.Open()
        let selectTend =
            sprintf 
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND bidding_date = @bidding_date AND doc_publish_date = @doc_publish_date AND notice_version = @notice_version" 
                stn.Prefix
        let cmd : MySqlCommand = new MySqlCommand(selectTend, con)
        cmd.Prepare()
        cmd.Parameters.AddWithValue("@purchase_number", purNum) |> ignore
        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
        cmd.Parameters.AddWithValue("@end_date", endDate) |> ignore
        cmd.Parameters.AddWithValue("@bidding_date", biddingDate) |> ignore
        cmd.Parameters.AddWithValue("@doc_publish_date", datePub) |> ignore
        cmd.Parameters.AddWithValue("@notice_version", status) |> ignore
        let reader : MySqlDataReader = cmd.ExecuteReader()
        if reader.HasRows then reader.Close()
        else 
            reader.Close()
            let idTender = ref 0
            this.Clicker driver <| String.Format("//table[contains(@id, 'auctionList')]/tbody/tr[{0}]/td[2]/a", page)
            driver.SwitchTo().DefaultContent() |> ignore
            wait.Until(fun dr -> dr.FindElement(By.XPath("//input[@value = 'Вернуться']")).Displayed) |> ignore
            let mutable cancelStatus = 0
            let mutable updated = false
            let selectDateT =
                sprintf 
                    "SELECT id_tender, date_version, cancel FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz" 
                    stn.Prefix
            let cmd2 = new MySqlCommand(selectDateT, con)
            cmd2.Prepare()
            cmd2.Parameters.AddWithValue("@purchase_number", purNum) |> ignore
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
            let Printform = "http://zakupki.butb.by/auctions/reestrauctions.html"
            let IdOrg = ref 0
            (*wait.Until(fun dr -> dr.FindElement(By.XPath("//tr[contains(., 'Сведения об организаторе')]/following-sibling::tr[contains(., 'Полное наименование')]/td[2]")).Displayed) |> ignore*)
            let OrgName =
                this.GetDefaultFromNullS 
                <| this.checkElement 
                       (driver, 
                        "//tr[contains(., 'Сведения об организаторе')]/following-sibling::tr[contains(., 'Полное наименование')]/td[2]")
            match OrgName with
            | "" -> ()
            | x -> 
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
                            "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address, contact_email = @contact_email, inn = @inn" 
                            stn.Prefix
                    let postAddress = ""
                    let contactPerson = ""
                    let factAddress =
                        this.GetDefaultFromNullS 
                        <| this.checkElement 
                               (driver, 
                                "//tr[contains(., 'Сведения об организаторе')]/following-sibling::tr[contains(., 'Место нахождения')]/td[2]")
                    let inn =
                        this.GetDefaultFromNullS 
                        <| this.checkElement 
                               (driver, 
                                "//tr[contains(., 'Сведения об организаторе')]/following-sibling::tr[contains(., 'УНП')]/td[2]")
                    let email =
                        this.GetDefaultFromNullS 
                        <| this.checkElement 
                               (driver, 
                                "//tr[contains(., 'Сведения об организаторе')]/following-sibling::tr[contains(., 'Адрес электронной почты')]/td[2]")
                    let cmd5 = new MySqlCommand(addOrganizer, con)
                    cmd5.Parameters.AddWithValue("@full_name", OrgName) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_person", contactPerson) |> ignore
                    cmd5.Parameters.AddWithValue("@post_address", postAddress) |> ignore
                    cmd5.Parameters.AddWithValue("@fact_address", factAddress) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_email", email) |> ignore
                    cmd5.Parameters.AddWithValue("@inn", inn) |> ignore
                    cmd5.ExecuteNonQuery() |> ignore
                    IdOrg := int cmd5.LastInsertedId
                    ()
            let idPlacingWay = ref 0
            let PlacingWayName =
                this.GetDefaultFromNullS 
                <| this.checkElement 
                       (driver, 
                        "//tr[contains(., 'Регистрационный номер')]/following-sibling::tr[contains(., 'Вид процедуры закупки')]/td[2]")
            match PlacingWayName with
            | "" -> ()
            | x -> idPlacingWay := this.GetPlacingWay con PlacingWayName settings
            let idEtp = this.GetEtp con settings
            let numVersion = 1
            let mutable idRegion = 0
            let purName =
                this.GetDefaultFromNullS 
                <| this.checkElement 
                       (driver, 
                        "//tr[contains(., 'Вид закупки')]/following-sibling::tr[contains(., 'Наименование закупки')]/td[2]")
            let href = "http://zakupki.butb.by/auctions/viewinvitation.html"
            let insertTender =
                String.Format
                    ("INSERT INTO {0}tender SET id_xml = @id_xml, purchase_number = @purchase_number, doc_publish_date = @doc_publish_date, href = @href, purchase_object_info = @purchase_object_info, type_fz = @type_fz, id_organizer = @id_organizer, id_placing_way = @id_placing_way, id_etp = @id_etp, end_date = @end_date, scoring_date = @scoring_date, bidding_date = @bidding_date, cancel = @cancel, date_version = @date_version, num_version = @num_version, notice_version = @notice_version, xml = @xml, print_form = @print_form, id_region = @id_region", 
                     stn.Prefix)
            let cmd9 = new MySqlCommand(insertTender, con)
            cmd9.Prepare()
            cmd9.Parameters.AddWithValue("@id_xml", purNum) |> ignore
            cmd9.Parameters.AddWithValue("@purchase_number", purNum) |> ignore
            cmd9.Parameters.AddWithValue("@doc_publish_date", datePub) |> ignore
            cmd9.Parameters.AddWithValue("@href", href) |> ignore
            cmd9.Parameters.AddWithValue("@purchase_object_info", purName) |> ignore
            cmd9.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
            cmd9.Parameters.AddWithValue("@id_organizer", !IdOrg) |> ignore
            cmd9.Parameters.AddWithValue("@id_placing_way", !idPlacingWay) |> ignore
            cmd9.Parameters.AddWithValue("@id_etp", idEtp) |> ignore
            cmd9.Parameters.AddWithValue("@end_date", endDate) |> ignore
            cmd9.Parameters.AddWithValue("@scoring_date", DateTime.MinValue) |> ignore
            cmd9.Parameters.AddWithValue("@bidding_date", biddingDate) |> ignore
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
            | true -> incr TenderButb.tenderUpCount
            | false -> incr TenderButb.tenderCount
            let documents =
                driver.FindElements
                    (By.XPath
                         ("//table[contains(., 'ДОКУМЕНТЫ')]/following-sibling::table[contains(., 'Тип документа')]/tbody/tr"))
            documents |> Seq.iter (this.ParsingDocs con !idTender)
            let cusInn =
                this.GetDefaultFromNullS 
                <| this.checkElement 
                       (driver, 
                        "//tr[contains(., 'Сведения о заказчике')]/following-sibling::tr[contains(., 'УНП')]/td[2]")
            let cusAddr =
                this.GetDefaultFromNullS 
                <| this.checkElement 
                       (driver, 
                        "//tr[contains(., 'Сведения о заказчике')]/following-sibling::tr[contains(., 'Место нахождения')]/td[2]")
            let idCustomer = ref 0
            match cusAddr with
            | "" -> ()
            | xc -> 
                let CustomerName =
                    this.GetDefaultFromNullS 
                    <| this.checkElement 
                           (driver, 
                            "//tr[contains(., 'Сведения о заказчике')]/following-sibling::tr[contains(., 'Полное наименование')]/td[2]")
                if CustomerName <> "" then 
                    let selectCustomer =
                        sprintf "SELECT id_customer FROM %scustomer WHERE full_name = @full_name" stn.Prefix
                    let cmd3 = new MySqlCommand(selectCustomer, con)
                    cmd3.Prepare()
                    cmd3.Parameters.AddWithValue("@full_name", CustomerName) |> ignore
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
                        let cmd14 = new MySqlCommand(insertCustomer, con)
                        cmd14.Prepare()
                        cmd14.Parameters.AddWithValue("@reg_num", RegNum) |> ignore
                        cmd14.Parameters.AddWithValue("@full_name", CustomerName) |> ignore
                        cmd14.Parameters.AddWithValue("@inn", cusInn) |> ignore
                        cmd14.ExecuteNonQuery() |> ignore
                        idCustomer := int cmd14.LastInsertedId
            let requirement =
                this.GetDefaultFromNullS 
                <| this.checkElement (driver, "//tr[contains(., 'Требования к составу участников')]/td[2]//textarea")
            let paginator = this.GetDefaultFromNullS <| this.checkElement (driver, "//span[@class = 'paginator']")
            let pricecheckT =
                this.GetDefaultFromNullS 
                <| this.checkElement 
                       (driver, 
                        "//table[contains(., 'СВЕДЕНИЯ О ЛОТЕ')]/following-sibling::table[contains(., 'Стоимость,')]/tbody/tr")
            
            let pricecheck =
                match pricecheckT with
                | "" -> false
                | _ -> true
            match paginator with
            | "" -> Logging.Log.logger ("can not find lots page count on", purNum)
            | pg -> 
                let pageT =
                    match this.GetCountPage(pg) with
                    | None -> 
                        Logging.Log.logger ("can not find lots page count on", purNum)
                        "1"
                    | Some pr -> pr.RegexDeleteWhitespace()
                
                let page = Int32.Parse(pageT)
                if page > 1 then 
                    try 
                        let lots =
                            driver.FindElements
                                (By.XPath
                                     ("//table[contains(., 'СВЕДЕНИЯ О ЛОТЕ')]/following-sibling::table[contains(., '№ лота')]/tbody/tr"))
                        lots |> Seq.iter (this.GetLots(driver, !idTender, !idCustomer, requirement, con, pricecheck))
                    with ex -> Logging.Log.logger ex
                    for pge in 1..(page - 1) do
                        try 
                            driver.SwitchTo().DefaultContent() |> ignore
                            wait.Until
                                (fun dr -> dr.FindElement(By.XPath("//a[img[@title = 'Следующая страница']]")).Displayed) 
                            |> ignore
                            this.Clicker driver "//a[img[@title = 'Следующая страница']]"
                            Thread.Sleep(5000)
                            driver.SwitchTo().DefaultContent() |> ignore
                            try 
                                let lots =
                                    driver.FindElements
                                        (By.XPath
                                             ("//table[contains(., 'СВЕДЕНИЯ О ЛОТЕ')]/following-sibling::table[contains(., '№ лота')]/tbody/tr"))
                                lots 
                                |> Seq.iter (this.GetLots(driver, !idTender, !idCustomer, requirement, con, pricecheck))
                            with ex -> Logging.Log.logger ex
                        with ex -> Logging.Log.logger ex
                else 
                    try 
                        let lots =
                            driver.FindElements
                                (By.XPath
                                     ("//table[contains(., 'СВЕДЕНИЯ О ЛОТЕ')]/following-sibling::table[contains(., '№ лота')]/tbody/tr"))
                        lots |> Seq.iter (this.GetLots(driver, !idTender, !idCustomer, requirement, con, pricecheck))
                    with ex -> Logging.Log.logger ex
                ()
            try 
                this.AddVerNumber con purNum stn typeFz
            with ex -> 
                Logging.Log.logger "Ошибка добавления версий тендера"
                Logging.Log.logger ex
            try 
                this.TenderKwords con (!idTender) stn
            with ex -> 
                Logging.Log.logger "Ошибка добавления kwords тендера"
                Logging.Log.logger ex
            this.Clicker driver "//input[@value = 'Вернуться']"
            ()
    
    member private this.ParsingDocs (con : MySqlConnection) (idTender : int) (elem : IWebElement) =
        let docName = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[3]")
        match docName with
        | "" -> ()
        | x -> 
            let hrefTT = this.checkElement (elem, ".//td[4]//a")
            
            let hrefT =
                match hrefTT with
                | null -> ""
                | r -> r.GetAttribute("href")
            
            let href = hrefT
            let descr = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[2]")
            let addAttach =
                sprintf 
                    "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description" 
                    stn.Prefix
            let cmd5 = new MySqlCommand(addAttach, con)
            cmd5.Parameters.AddWithValue("@id_tender", idTender) |> ignore
            cmd5.Parameters.AddWithValue("@file_name", x) |> ignore
            cmd5.Parameters.AddWithValue("@url", href) |> ignore
            cmd5.Parameters.AddWithValue("@description", descr) |> ignore
            cmd5.ExecuteNonQuery() |> ignore
        ()
    
    member private this.GetCountPage(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"Страница.+/.+(\d)." gr1 -> Some(gr1)
        | _ -> None
    
    member private this.GetQuantity(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"(\d+[\s,]*\d*)" gr1 -> Some(gr1)
        | _ -> None
    
    member private this.GetOkei(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"([А-Яа-я\.]+)$" gr1 -> Some(gr1)
        | _ -> None
    
    member private this.GetPrice(s : string) : string =
        let p = s.Replace(",", ".")
        let b = Regex.Replace(p, @"\s+", "")
        b
    
    member private this.GetLots (driver : ChromeDriver, idTender : int, idCustomer : int, requirement : string, 
                                 con : MySqlConnection, pricecheck : bool) (elem : IWebElement) =
        let idLot = ref 0
        let lotNumT = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[1]/span")
        
        let lotNum =
            match lotNumT with
            | "" -> 0
            | _ -> Int32.Parse(lotNumT)
        
        let mutable price = ""
        if pricecheck then 
            let priceTT = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[8]/span[1]")
            price <- this.GetPrice(priceTT)
        let finsource = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[7]/span")
        let currency = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[8]/span[2]")
        let insertLot =
            sprintf 
                "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency, finance_source = @finance_source" 
                stn.Prefix
        let cmd12 = new MySqlCommand(insertLot, con)
        cmd12.Parameters.AddWithValue("@id_tender", idTender) |> ignore
        cmd12.Parameters.AddWithValue("@lot_number", lotNum) |> ignore
        cmd12.Parameters.AddWithValue("@max_price", price) |> ignore
        cmd12.Parameters.AddWithValue("@currency", currency) |> ignore
        cmd12.Parameters.AddWithValue("@finance_source", finsource) |> ignore
        cmd12.ExecuteNonQuery() |> ignore
        idLot := int cmd12.LastInsertedId
        match requirement with
        | "" -> ()
        | r -> 
            let addReq = sprintf "INSERT INTO %srequirement SET id_lot = @id_lot, content = @content" stn.Prefix
            let cmd = new MySqlCommand(addReq, con)
            cmd.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
            cmd.Parameters.AddWithValue("@content", requirement) |> ignore
        let okpd2 = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[2]/span")
        let name = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[3]/a/span")
        let quantityT = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[4]/span")
        
        let quantity =
            match this.GetQuantity(quantityT) with
            | None -> ""
            | Some pr -> this.GetPrice(pr.Trim())
        
        let okei =
            match this.GetOkei(quantityT) with
            | None -> ""
            | Some pr -> pr.Trim()
        
        let insertLotitem =
            sprintf 
                "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, okpd2_code = @okpd2_code, quantity_value = @quantity_value, sum = @sum, okei = @okei, customer_quantity_value = @customer_quantity_value" 
                stn.Prefix
        let cmd19 = new MySqlCommand(insertLotitem, con)
        cmd19.Prepare()
        cmd19.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
        cmd19.Parameters.AddWithValue("@id_customer", idCustomer) |> ignore
        cmd19.Parameters.AddWithValue("@name", name) |> ignore
        cmd19.Parameters.AddWithValue("@okpd2_code", okpd2) |> ignore
        cmd19.Parameters.AddWithValue("@quantity_value", quantity) |> ignore
        cmd19.Parameters.AddWithValue("@sum", price) |> ignore
        cmd19.Parameters.AddWithValue("@okei", okei) |> ignore
        cmd19.Parameters.AddWithValue("@customer_quantity_value", quantity) |> ignore
        cmd19.ExecuteNonQuery() |> ignore
        let delivPlace = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[5]/span")
        let delivTerm = this.GetDefaultFromNullS <| this.checkElement (elem, ".//td[6]/span")
        let insertCustomerRequirement =
            sprintf 
                "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_term = @delivery_term, delivery_place = @delivery_place, max_price = @max_price" 
                stn.Prefix
        let cmd16 = new MySqlCommand(insertCustomerRequirement, con)
        cmd16.Prepare()
        cmd16.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
        cmd16.Parameters.AddWithValue("@id_customer", idCustomer) |> ignore
        cmd16.Parameters.AddWithValue("@delivery_term", delivTerm) |> ignore
        cmd16.Parameters.AddWithValue("@delivery_place", delivPlace) |> ignore
        cmd16.Parameters.AddWithValue("@max_price", price) |> ignore
        cmd16.ExecuteNonQuery() |> ignore
        ()
