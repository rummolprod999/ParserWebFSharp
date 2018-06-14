namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open MySql.Data.MySqlClient
open System
open System.Data
open System.Linq
open System.Text.RegularExpressions
open System.Threading
open TypeE

type TenderRossel(stn : Settings.T, tn : RosSelRec) =
    inherit Tender("АО «ЕЭТП» «Росэлторг»", "https://www.roseltorg.ru")
    let settings = stn
    let typeFz = 42
    static member val tenderCount = ref 0
    
    member private this.GetDateS(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"(\d{2}\.\d{2}\.\d{2} \d{2}:\d{2}:\d{2})" gr1 -> Some(gr1)
        | _ -> None
    
    member private this.GetPriceClear(s : string) : string =
        let p = s.Replace(",", ".")
        let b = Regex.Replace(p, @"\s+", "")
        b
    
    member private this.GetPrice(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"^([\d, ]+)\s" gr1 -> Some(gr1)
        | _ -> None
    
    member private this.GetApplGuarAmount(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"(\d[\d, ]+)\s" gr1 -> Some(gr1)
        | _ -> None
    
    member private this.ParsingDocs (con : MySqlConnection) (idTender : int) (elem : IElement) =
        let docName = elem.TextContent.Trim()
        match docName with
        | "" -> ()
        | x -> 
            let hrefT = elem.GetAttribute("href")
            let href = hrefT
            let addAttach =
                sprintf "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url" 
                    stn.Prefix
            let cmd5 = new MySqlCommand(addAttach, con)
            cmd5.Parameters.AddWithValue("@id_tender", idTender) |> ignore
            cmd5.Parameters.AddWithValue("@file_name", x) |> ignore
            cmd5.Parameters.AddWithValue("@url", href) |> ignore
            cmd5.ExecuteNonQuery() |> ignore
        ()
    
    override this.Parsing() =
        let Page = Download.DownloadString tn.Href
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", tn.Href)
        | s -> this.ParserPage(s)
    
    member private this.ParserPage(p : string) =
        let parser = new HtmlParser()
        let doc = parser.Parse(p)
        let purNameT = doc.QuerySelector("h1")
        match purNameT with
        | null -> raise <| System.NullReferenceException(sprintf "purName not found in %s" tn.Href)
        | _ -> ()
        let purName = purNameT.TextContent.Trim()
        let pubDateT = doc.QuerySelector("td:contains('Дата публикации:') + td > p")
        match pubDateT with
        | null -> raise <| System.NullReferenceException(sprintf "pubDate not found in %s" tn.Href)
        | _ -> ()
        let mutable pubDateS = pubDateT.TextContent.Trim()
        match this.GetDateS(pubDateS) with
        | Some dtP -> pubDateS <- dtP
        | None -> raise <| System.Exception(sprintf "can not apply regex to datePub %s" tn.Href)
        let datePub =
            match pubDateS.DateFromString("dd.MM.yy HH:mm:ss") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "can not parse datePub %s" pubDateS)
        
        let endDateT = doc.QuerySelector("td:contains('Дата окончания регистрации:') + td > p")
        match endDateT with
        | null -> raise <| System.NullReferenceException(sprintf "endDate not found in %s" tn.Href)
        | _ -> ()
        let mutable endDateS = endDateT.TextContent.Trim()
        match this.GetDateS(endDateS) with
        | Some dtP -> endDateS <- dtP
        | None -> raise <| System.Exception(sprintf "can not apply regex to endDate %s" tn.Href)
        let endDate =
            match endDateS.DateFromString("dd.MM.yy HH:mm:ss") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "can not parse endDate %s" endDateS)
        
        let dateUpd = DateTime.Now
        let biddingDateT = doc.QuerySelector("td:contains('Дата проведения торгов:') + td > p")
        
        let mutable biddingDateS =
            match biddingDateT with
            | null -> ""
            | xx -> xx.TextContent.Trim()
        match this.GetDateS(biddingDateS) with
        | Some dtP -> biddingDateS <- dtP
        | None -> biddingDateS <- ""
        let biddingDate =
            match biddingDateS.DateFromString("dd.MM.yy HH:mm:ss") with
            | Some d -> d
            | None -> DateTime.MinValue
        
        let scoringDateT = doc.QuerySelector("td:contains('Дата и время рассмотрения заявок:') + td > p")
        
        let mutable scoringDateS =
            match scoringDateT with
            | null -> ""
            | xx -> xx.TextContent.Trim()
        match this.GetDateS(scoringDateS) with
        | Some dtP -> scoringDateS <- dtP
        | None -> scoringDateS <- ""
        let scoringDate =
            match scoringDateS.DateFromString("dd.MM.yy HH:mm:ss") with
            | Some d -> d
            | None -> DateTime.MinValue
        
        let status = (this.GetDefaultFromNull <| doc.QuerySelector("div.g-proc-num + p")).Replace("Статус: ", "")
        use con = new MySqlConnection(stn.ConStr)
        con.Open()
        let href = tn.Href
        let selectTend =
            sprintf 
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND scoring_date = @scoring_date AND doc_publish_date = @doc_publish_date AND bidding_date = @bidding_date AND notice_version = @notice_version" 
                stn.Prefix
        let cmd : MySqlCommand = new MySqlCommand(selectTend, con)
        cmd.Prepare()
        cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
        cmd.Parameters.AddWithValue("@end_date", endDate) |> ignore
        cmd.Parameters.AddWithValue("@scoring_date", scoringDate) |> ignore
        cmd.Parameters.AddWithValue("@doc_publish_date", datePub) |> ignore
        cmd.Parameters.AddWithValue("@bidding_date", biddingDate) |> ignore
        cmd.Parameters.AddWithValue("@notice_version", status) |> ignore
        let reader : MySqlDataReader = cmd.ExecuteReader()
        if reader.HasRows then reader.Close()
        else 
            reader.Close()
            let mutable cancelStatus = 0
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
                //printfn "%A" <| (row.["date_version"])
                match dateUpd >= ((row.["date_version"]) :?> DateTime) with
                | true -> row.["cancel"] <- 1
                | false -> cancelStatus <- 1
            let commandBuilder = new MySqlCommandBuilder(adapter)
            commandBuilder.ConflictOption <- ConflictOption.OverwriteChanges
            adapter.Update(dt) |> ignore
            let Printform = href
            let IdOrg = ref 0
            let OrgName =
                this.GetDefaultFromNull 
                <| doc.QuerySelector("h2:contains('Организатор') + table tr:nth-child(1) td:nth-child(2) > p")
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
                            "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address, contact_phone = @contact_phone" 
                            stn.Prefix
                    let contactPerson =
                        this.GetDefaultFromNull 
                        <| doc.QuerySelector("h2:contains('Организатор') + table tr:nth-child(2) td:nth-child(2) > p")
                    let postAddress = ""
                    let factAddress = ""
                    let phone =
                        this.GetDefaultFromNull 
                        <| doc.QuerySelector("h2:contains('Организатор') + table tr:nth-child(3) td:nth-child(2) > p")
                    let cmd5 = new MySqlCommand(addOrganizer, con)
                    cmd5.Parameters.AddWithValue("@full_name", OrgName) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_person", contactPerson) |> ignore
                    cmd5.Parameters.AddWithValue("@post_address", postAddress) |> ignore
                    cmd5.Parameters.AddWithValue("@fact_address", factAddress) |> ignore
                    cmd5.Parameters.AddWithValue("@contact_phone", phone) |> ignore
                    cmd5.ExecuteNonQuery() |> ignore
                    IdOrg := int cmd5.LastInsertedId
                    ()
            let PlacingWayName =
                this.GetDefaultFromNull <| doc.QuerySelector("h1 + table tr:nth-child(2) td:nth-child(2) > p")
            let idPlacingWay = ref 0
            match PlacingWayName with
            | "" -> ()
            | x -> idPlacingWay := this.GetPlacingWay con PlacingWayName settings
            let idEtp = this.GetEtp con settings
            let numVersion = 1
            let mutable idRegion = 0
            let region = this.GetDefaultFromNull <| doc.QuerySelector("p[title*='Регион заказчика']")
            let regionS = Tools.GetRegionString(region)
            if regionS <> "" then 
                let selectReg = sprintf "SELECT id FROM %sregion WHERE name LIKE @name" stn.Prefix
                let cmd46 = new MySqlCommand(selectReg, con)
                cmd46.Prepare()
                cmd46.Parameters.AddWithValue("@name", "%" + regionS + "%") |> ignore
                let reader36 = cmd46.ExecuteReader()
                match reader36.HasRows with
                | true -> 
                    reader36.Read() |> ignore
                    idRegion <- reader36.GetInt32("id")
                    reader36.Close()
                | false -> reader36.Close()
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
            cmd9.Parameters.AddWithValue("@notice_version", status) |> ignore
            cmd9.Parameters.AddWithValue("@xml", href) |> ignore
            cmd9.Parameters.AddWithValue("@print_form", Printform) |> ignore
            cmd9.Parameters.AddWithValue("@id_region", idRegion) |> ignore
            cmd9.ExecuteNonQuery() |> ignore
            idTender := int cmd9.LastInsertedId
            incr TenderRossel.tenderCount
            let lotNumber = 1
            let idLot = ref 0
            let documents = doc.QuerySelectorAll("div.w-files-b h2:contains('Документация') + ul li a")
            documents |> Seq.iter (this.ParsingDocs con !idTender)
            let priceT = this.GetDefaultFromNull <| doc.QuerySelector("div.w-price-b p")
            
            let price =
                match this.GetPrice(priceT) with
                | Some dtP -> this.GetPriceClear dtP
                | None -> ""
            
            let currency =
                match doc.QuerySelector("div.w-price-b p span") with
                | null -> ""
                | x -> x.GetAttribute("title").Trim()
            
            let insertLot =
                sprintf 
                    "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number, max_price = @max_price, currency = @currency" 
                    stn.Prefix
            let cmd12 = new MySqlCommand(insertLot, con)
            cmd12.Parameters.AddWithValue("@id_tender", !idTender) |> ignore
            cmd12.Parameters.AddWithValue("@lot_number", lotNumber) |> ignore
            cmd12.Parameters.AddWithValue("@max_price", price) |> ignore
            cmd12.Parameters.AddWithValue("@currency", currency) |> ignore
            cmd12.ExecuteNonQuery() |> ignore
            idLot := int cmd12.LastInsertedId
            let idCustomer = ref 0
            let CustomerName =
                this.GetDefaultFromNull <| doc.QuerySelector("td:contains('Название организации:') + td > p")
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
                    let inn = this.GetDefaultFromNull <| doc.QuerySelector("td:contains('ИНН заказчика:') + td > p")
                    let cmd14 = new MySqlCommand(insertCustomer, con)
                    cmd14.Prepare()
                    cmd14.Parameters.AddWithValue("@reg_num", RegNum) |> ignore
                    cmd14.Parameters.AddWithValue("@full_name", CustomerName) |> ignore
                    cmd14.Parameters.AddWithValue("@inn", inn) |> ignore
                    cmd14.ExecuteNonQuery() |> ignore
                    idCustomer := int cmd14.LastInsertedId
            let insertLotitem =
                sprintf "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name" 
                    stn.Prefix
            let cmd19 = new MySqlCommand(insertLotitem, con)
            cmd19.Prepare()
            cmd19.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
            cmd19.Parameters.AddWithValue("@id_customer", !idCustomer) |> ignore
            cmd19.Parameters.AddWithValue("@name", purName) |> ignore
            cmd19.ExecuteNonQuery() |> ignore
            let delivPlace = this.GetDefaultFromNull <| doc.QuerySelector("td:contains('Место поставки') + td > p")
            let applGuarAmountT = this.GetDefaultFromNull <| doc.QuerySelector("p:contains('Обеспечение заявки:')")
            
            let applGuarAmount =
                match this.GetApplGuarAmount(applGuarAmountT) with
                | Some dtP -> this.GetPriceClear dtP
                | None -> ""
            
            let contrGuarAmountT = this.GetDefaultFromNull <| doc.QuerySelector("p:contains('Обеспечение контракта:')")
            
            let contrGuarAmount =
                match this.GetApplGuarAmount(contrGuarAmountT) with
                | Some dtP -> this.GetPriceClear dtP
                | None -> ""
            
            let insertCustomerRequirement =
                sprintf 
                    "INSERT INTO %scustomer_requirement SET id_lot = @id_lot, id_customer = @id_customer, delivery_place = @delivery_place, max_price = @max_price, application_guarantee_amount = @application_guarantee_amount, contract_guarantee_amount = @contract_guarantee_amount" 
                    stn.Prefix
            let cmd16 = new MySqlCommand(insertCustomerRequirement, con)
            cmd16.Prepare()
            cmd16.Parameters.AddWithValue("@id_lot", !idLot) |> ignore
            cmd16.Parameters.AddWithValue("@id_customer", idCustomer) |> ignore
            cmd16.Parameters.AddWithValue("@delivery_place", delivPlace) |> ignore
            cmd16.Parameters.AddWithValue("@max_price", price) |> ignore
            cmd16.Parameters.AddWithValue("@application_guarantee_amount", applGuarAmount) |> ignore
            cmd16.Parameters.AddWithValue("@contract_guarantee_amount", contrGuarAmount) |> ignore
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