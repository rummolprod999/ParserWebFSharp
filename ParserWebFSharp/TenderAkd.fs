namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open MySql.Data.MySqlClient
open System
open System.Data
open TypeE

type TenderAkd(stn : Settings.T, urlT : string, purNum : string) =
    inherit Tender("Электронная торговая площадка для проведения торгов - Аукционный Конкурсный Дом", 
                   "http://www.a-k-d.ru/tender")
    let settings = stn
    let typeFz = 33
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0
    
    override this.Parsing() =
        let Page = Download.DownloadString urlT
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", urlT)
        | s -> this.ParserPage(s)
        ()
    
    member private this.GetPriceS(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"^(\d+[\d \.]*\.\d{2})" gr1 -> Some(gr1)
        | _ -> None
    
    member private this.ParsingDocs (con : MySqlConnection) (idTender : int) (elem : IElement) =
        let docName = this.GetDefaultFromNull <| elem.QuerySelector("td.name a")
        match docName with
        | "" -> ()
        | x -> 
            let hrefT = elem.QuerySelector("td.name a").GetAttribute("href")
            let href = sprintf "http://www.a-k-d.ru%s" hrefT
            let desc = this.GetDefaultFromNull <| elem.QuerySelector("td:last-child")
            let addAttach =
                sprintf 
                    "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url, description = @description" 
                    stn.Prefix
            let cmd5 = new MySqlCommand(addAttach, con)
            cmd5.Parameters.AddWithValue("@id_tender", idTender) |> ignore
            cmd5.Parameters.AddWithValue("@file_name", x) |> ignore
            cmd5.Parameters.AddWithValue("@url", href) |> ignore
            cmd5.Parameters.AddWithValue("@description", desc) |> ignore
            cmd5.ExecuteNonQuery() |> ignore
        ()
    
    member private this.ParserPage(p : string) =
        let parser = HtmlParser()
        let doc = parser.Parse(p)
        let pubDateT = doc.QuerySelector("th:contains('Публикация электронной процедуры') + td > span")
        match pubDateT with
        | null -> raise <| System.NullReferenceException(sprintf "pubDate not found in %s" urlT)
        | _ -> ()
        let pubDateS = pubDateT.TextContent.Replace("г.", "").Trim().ReplaceDate().RegexReplace()
        
        let datePub =
            match pubDateS.DateFromString("d.MM.yyyy") with
            | Some d -> d
            | None -> 
                match pubDateS.DateFromString("d.MM.yyyy HH:mm") with
                | Some d -> d
                | None -> raise <| System.Exception(sprintf "cannot parse datePub %s, %s" pubDateS urlT)
        
        let endDateT = doc.QuerySelector("th:contains('Окончание приема заявок') + td > span")
        
        let endDateS =
            match endDateT with
            | null -> ""
            | _ -> endDateT.TextContent.Replace("г.", "").Trim().ReplaceDate().RegexReplace()
        
        let endDate =
            match endDateS.DateFromString("d.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> 
                match endDateS.DateFromString("d.MM.yyyy") with
                | Some d -> d
                | None -> DateTime.MinValue
        
        let scoringDateT = doc.QuerySelector("th:contains('Дата окончания') + td > span")
        
        let scoringDateS =
            match scoringDateT with
            | null -> ""
            | _ -> scoringDateT.TextContent.Replace("г.", "").Trim().ReplaceDate().RegexReplace()
        
        let scoringDate =
            match scoringDateS.DateFromString("d.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> 
                match scoringDateS.DateFromString("d.MM.yyyy") with
                | Some d -> d
                | None -> DateTime.MinValue
        
        let biddingDateT = doc.QuerySelector("th:contains('Начало') + td > span")
        
        let biddingDateS =
            match biddingDateT with
            | null -> ""
            | _ -> biddingDateT.TextContent.Replace("г.", "").Trim().ReplaceDate().RegexReplace()
        
        let biddingDate =
            match biddingDateS.DateFromString("d.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> 
                match biddingDateS.DateFromString("d.MM.yyyy") with
                | Some d -> d
                | None -> DateTime.MinValue
        
        let dateUpd = DateTime.Now
        let NoticeVersion = this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Статус') + td > span")
        use con = new MySqlConnection(stn.ConStr)
        con.Open()
        let href = urlT
        let selectTend =
            sprintf 
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND  doc_publish_date = @doc_publish_date AND type_fz = @type_fz AND end_date = @end_date AND notice_version = @notice_version" 
                stn.Prefix
        let cmd : MySqlCommand = new MySqlCommand(selectTend, con)
        cmd.Prepare()
        cmd.Parameters.AddWithValue("@purchase_number", purNum) |> ignore
        cmd.Parameters.AddWithValue("@doc_publish_date", datePub) |> ignore
        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
        cmd.Parameters.AddWithValue("@end_date", endDate) |> ignore
        cmd.Parameters.AddWithValue("@notice_version", NoticeVersion) |> ignore
        let reader : MySqlDataReader = cmd.ExecuteReader()
        if reader.HasRows then reader.Close()
        else 
            reader.Close()
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
            let Printform = href
            let IdOrg = ref 0
            let OrgName = this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Организатор торгов') + td")
            match OrgName with
            | "" -> ()
            | _ -> 
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
                            "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address" 
                            stn.Prefix
                    let contactPerson =
                        this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Контактное лицо') + td")
                    let postAddress = this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Почтовый адрес') + td")
                    let factAddress =
                        this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Фактический адрес') + td")
                    let cmd5 = new MySqlCommand(addOrganizer, con)
                    cmd5.Parameters.AddWithValue("@full_name", OrgName) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_person", contactPerson) |> ignore
                    cmd5.Parameters.AddWithValue("@post_address", postAddress) |> ignore
                    cmd5.Parameters.AddWithValue("@fact_address", factAddress) |> ignore
                    cmd5.ExecuteNonQuery() |> ignore
                    IdOrg := int cmd5.LastInsertedId
                    ()
            let idPlacingWay = ref 0
            let PlacingWayName =
                this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Наименование способа размещения') + td")
            match PlacingWayName with
            | "" -> ()
            | _ -> idPlacingWay := this.GetPlacingWay con PlacingWayName settings
            let idEtp = this.GetEtp con settings
            let numVersion = 1
            let mutable idRegion = 0
            let purName =
                this.GetDefaultFromNull 
                <| doc.QuerySelector("th:contains('Наименование электронной процедуры') + td > span")
            let idTender = ref 0
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
            cmd9.Parameters.AddWithValue("@scoring_date", scoringDate) |> ignore
            cmd9.Parameters.AddWithValue("@bidding_date", biddingDate) |> ignore
            cmd9.Parameters.AddWithValue("@cancel", cancelStatus) |> ignore
            cmd9.Parameters.AddWithValue("@date_version", dateUpd) |> ignore
            cmd9.Parameters.AddWithValue("@num_version", numVersion) |> ignore
            cmd9.Parameters.AddWithValue("@notice_version", NoticeVersion) |> ignore
            cmd9.Parameters.AddWithValue("@xml", href) |> ignore
            cmd9.Parameters.AddWithValue("@print_form", Printform) |> ignore
            cmd9.Parameters.AddWithValue("@id_region", idRegion) |> ignore
            cmd9.ExecuteNonQuery() |> ignore
            idTender := int cmd9.LastInsertedId
            match updated with
            | true -> incr TenderAkd.tenderUpCount
            | false -> incr TenderAkd.tenderCount
            let documents = doc.QuerySelectorAll("tbody.zen-direct-upload-container > tr")
            documents |> Seq.iter (this.ParsingDocs con !idTender)
            let lotNumber = 1
            let idLot = ref 0
            let maxPriceT = this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Начальная цена') + td > span")
            
            let maxPrice =
                match this.GetPriceS(maxPriceT) with
                | None -> ""
                | Some pr -> pr.RegexDeleteWhitespace()
            
            let financeSource =
                this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Источник финансирования') + td")
            let insertLot =
                sprintf 
                    "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, finance_source = @finance_source" 
                    stn.Prefix
            let cmd12 = new MySqlCommand(insertLot, con)
            cmd12.Parameters.AddWithValue("@id_tender", !idTender) |> ignore
            cmd12.Parameters.AddWithValue("@lot_number", lotNumber) |> ignore
            cmd12.Parameters.AddWithValue("@max_price", maxPrice) |> ignore
            cmd12.Parameters.AddWithValue("@finance_source", financeSource) |> ignore
            cmd12.ExecuteNonQuery() |> ignore
            idLot := int cmd12.LastInsertedId
            let idCustomer = ref 0
            let CustomerName = this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Заказчик') + td")
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
                        sprintf "INSERT INTO %scustomer SET reg_num = @reg_num, full_name = @full_name" stn.Prefix
                    let RegNum = Guid.NewGuid().ToString()
                    let cmd14 = new MySqlCommand(insertCustomer, con)
                    cmd14.Prepare()
                    cmd14.Parameters.AddWithValue("@reg_num", RegNum) |> ignore
                    cmd14.Parameters.AddWithValue("@full_name", CustomerName) |> ignore
                    cmd14.ExecuteNonQuery() |> ignore
                    idCustomer := int cmd14.LastInsertedId
            let insertLotitem =
                sprintf 
                    "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name, sum = @sum" 
                    stn.Prefix
            let cmd19 = new MySqlCommand(insertLotitem, con)
            cmd19.Prepare()
            cmd19.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
            cmd19.Parameters.AddWithValue("@id_customer", !idCustomer) |> ignore
            cmd19.Parameters.AddWithValue("@name", purName) |> ignore
            cmd19.Parameters.AddWithValue("@sum", maxPrice) |> ignore
            cmd19.ExecuteNonQuery() |> ignore
            let delivTerm = this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Сроки исполнения') + td")
            let delivTerm1 = this.GetDefaultFromNull <| doc.QuerySelector("th:contains('Сроки и условия оплаты') + td")
            match (delivTerm, delivTerm1) with
            | (_, _) & ("", "") -> ()
            | (x, y) -> 
                let deliv = sprintf "Сроки исполнения: %s \n Сроки и условия оплаты: %s" x y
                let insertCustomerRequirement =
                    sprintf 
                        "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_term = @delivery_term" 
                        stn.Prefix
                let cmd16 = new MySqlCommand(insertCustomerRequirement, con)
                cmd16.Prepare()
                cmd16.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
                cmd16.Parameters.AddWithValue("@id_customer", !idCustomer) |> ignore
                cmd16.Parameters.AddWithValue("@delivery_term", deliv) |> ignore
                cmd16.ExecuteNonQuery() |> ignore
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
            ()
