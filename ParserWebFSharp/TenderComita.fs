namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open System.Threading
open TypeE
open OpenQA.Selenium.Chrome

type TenderComita(stn : Settings.T, tn : ComitaRec, typeFz : int, etpName : string, etpUrl : string, driver : ChromeDriver) =
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
        let cmd : MySqlCommand = new MySqlCommand(selectTend, con)
        cmd.Prepare()
        cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
        cmd.Parameters.AddWithValue("@end_date", tn.DateEnd) |> ignore
        cmd.Parameters.AddWithValue("@doc_publish_date", tn.DatePub) |> ignore
        cmd.Parameters.AddWithValue("@notice_version", tn.Status) |> ignore
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
            driver.Navigate().GoToUrl(tn.Href)
            Thread.Sleep(5000)
            driver.SwitchTo().DefaultContent() |> ignore
            let OrgName =
                match driver.findElementWithoutException ("//div[span[contains(., 'Опубликовано:')]]") with
                | "" -> ""
                | x -> x.Replace("Опубликовано:", "").Trim()
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
                    
                    let inn = ""
                    let cmd5 = new MySqlCommand(addOrganizer, con)
                    cmd5.Parameters.AddWithValue("@full_name", OrgName) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_person", contactPerson) |> ignore
                    cmd5.Parameters.AddWithValue("@post_address", postAddress) |> ignore
                    cmd5.Parameters.AddWithValue("@fact_address", factAddress) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_phone", phone) |> ignore
                    cmd5.Parameters.AddWithValue("@inn", inn) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_email", email) |> ignore
                    cmd5.ExecuteNonQuery() |> ignore
                    IdOrg := int cmd5.LastInsertedId
                    ()
            let idPlacingWay = ref 0
            
            let PwayName =
                match driver.findElementWithoutException 
                          ("//div[contains(@class, 'procedure-line') and contains(@class, 'purchase')]/b") with
                | "" -> ""
                | x -> x.Trim()
            match PwayName with
            | "" -> ()
            | _ -> idPlacingWay := this.GetPlacingWay con PwayName settings
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
            cmd9.Parameters.AddWithValue("@doc_publish_date", tn.DatePub) |> ignore
            cmd9.Parameters.AddWithValue("@href", href) |> ignore
            cmd9.Parameters.AddWithValue("@purchase_object_info", tn.PurName) |> ignore
            cmd9.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
            cmd9.Parameters.AddWithValue("@id_organizer", !IdOrg) |> ignore
            cmd9.Parameters.AddWithValue("@id_placing_way", !idPlacingWay) |> ignore
            cmd9.Parameters.AddWithValue("@id_etp", idEtp) |> ignore
            cmd9.Parameters.AddWithValue("@end_date", tn.DateEnd) |> ignore
            cmd9.Parameters.AddWithValue("@scoring_date", DateTime.MinValue) |> ignore
            cmd9.Parameters.AddWithValue("@bidding_date", DateTime.MinValue) |> ignore
            cmd9.Parameters.AddWithValue("@cancel", cancelStatus) |> ignore
            cmd9.Parameters.AddWithValue("@date_version", dateUpd) |> ignore
            cmd9.Parameters.AddWithValue("@num_version", numVersion) |> ignore
            cmd9.Parameters.AddWithValue("@notice_version", tn.Status) |> ignore
            cmd9.Parameters.AddWithValue("@xml", href) |> ignore
            cmd9.Parameters.AddWithValue("@print_form", Printform) |> ignore
            cmd9.Parameters.AddWithValue("@id_region", idRegion) |> ignore
            cmd9.ExecuteNonQuery() |> ignore
            idTender := int cmd9.LastInsertedId
            match updated with
            | true -> incr TenderComita.tenderUpCount
            | false -> incr TenderComita.tenderCount
            let idCustomer = ref 0
            
            let CusName =
                match driver.findElementWithoutException ("//div[span[contains(., 'Заказчик:')]]") with
                | "" -> ""
                | x -> x.Replace("Заказчик:", "").Trim()
            if CusName <> "" then 
                let selectCustomer =
                    sprintf "SELECT id_customer FROM %scustomer WHERE full_name = @full_name" stn.Prefix
                let cmd3 = new MySqlCommand(selectCustomer, con)
                cmd3.Prepare()
                cmd3.Parameters.AddWithValue("@full_name", CusName) |> ignore
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
                    cmd14.Parameters.AddWithValue("@full_name", CusName) |> ignore
                    cmd14.Parameters.AddWithValue("@inn", inn) |> ignore
                    cmd14.ExecuteNonQuery() |> ignore
                    idCustomer := int cmd14.LastInsertedId
            let lots = driver.findElementsWithoutException ("//tbody[@ng-repeat = 'data in dataTableLots']/tr")
            let LotNum = ref 1
            for lot in lots do
                let LotName =
                    match lot.findElementWithoutException ("./td[@class = 'ng-binding'][2]") with
                    | "" -> ""
                    | x -> x.Trim()
                
                let Nmck =
                    match lot.findElementWithoutException ("./td[contains(@class, 'price')]/span/span[1]") with
                    | "" -> ""
                    | x -> x.Replace(",", ".").RegexDeleteWhitespace().Trim()
                
                let idLot = ref 0
                let insertLot =
                    sprintf 
                        "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency" 
                        stn.Prefix
                let cmd12 = new MySqlCommand(insertLot, con)
                cmd12.Parameters.AddWithValue("@id_tender", !idTender) |> ignore
                cmd12.Parameters.AddWithValue("@lot_number", !LotNum) |> ignore
                cmd12.Parameters.AddWithValue("@max_price", Nmck) |> ignore
                cmd12.Parameters.AddWithValue("@currency", tn.Currency) |> ignore
                cmd12.ExecuteNonQuery() |> ignore
                idLot := int cmd12.LastInsertedId
                incr LotNum
                let insertLotitem =
                    sprintf 
                        "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum" 
                        stn.Prefix
                let cmd19 = new MySqlCommand(insertLotitem, con)
                cmd19.Prepare()
                cmd19.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
                cmd19.Parameters.AddWithValue("@id_customer", !idCustomer) |> ignore
                cmd19.Parameters.AddWithValue("@name", LotName) |> ignore
                cmd19.Parameters.AddWithValue("@sum", Nmck) |> ignore
                cmd19.ExecuteNonQuery() |> ignore
                ()
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
