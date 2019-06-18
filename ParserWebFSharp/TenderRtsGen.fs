namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open TypeE
open HtmlAgilityPack
open Tools

type TenderRtsGen(stn: Settings.T, tn: RtsGenRec, typeFz: int, etpName: string, etpUrl: string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() =
        let builder = DocumentBuilder()
        use con = new MySqlConnection(stn.ConStr)
        let res =
                   builder {
                        con.Open()
                        let selectTend = sprintf "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND notice_version = @notice_version" stn.Prefix
                        let cmd: MySqlCommand = new MySqlCommand(selectTend, con)
                        cmd.Prepare()
                        cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
                        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
                        cmd.Parameters.AddWithValue("@end_date", tn.DateEnd) |> ignore
                        cmd.Parameters.AddWithValue("@notice_version", tn.status) |> ignore
                        let reader: MySqlDataReader = cmd.ExecuteReader()
                        if reader.HasRows then reader.Close()
                                               return! Err ""
                        reader.Close()
                        let Page = Download.DownloadString tn.Href
                        if Page = "" || Page = null then return! Err (sprintf "%s" tn.Href)
                        let htmlDoc = new HtmlDocument()
                        htmlDoc.LoadHtml(Page)
                        let nav = (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator
                        let dateUpd = DateTime.Now
                        let (cancelStatus, updated) = this.SetCancelStatus(con, dateUpd)
                        let Printform = tn.Href
                        let IdOrg = ref 0
                        let orgName = nav.Gsn "//label[contains(., 'Наименование организации')]/following-sibling::span/a"
                        let orgName = if orgName <> "" then orgName else tn.OrgName
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
                                let addOrganizer = sprintf "INSERT INTO %sorganizer SET full_name = @full_name, contact_person = @contact_person, post_address = @post_address, fact_address = @fact_address, contact_phone = @contact_phone, inn = @inn, contact_email = @contact_email" stn.Prefix
                                let contactPerson = InlineHtmlNavigator nav "//label[contains(., 'Ответственное должностное лицо')]/following-sibling::span"
                                let postAddress = InlineHtmlNavigator nav "//label[contains(., 'Почтовый адрес')]/following-sibling::span]"
                                let factAddress = InlineHtmlNavigator nav "//label[contains(., 'Место нахождения')]/following-sibling::span"
                                let phone = InlineHtmlNavigator nav "//label[contains(., 'Телефон')]/following-sibling::span"
                                let inn = InlineHtmlNavigator nav "//label[contains(., 'ИНН организации')]/following-sibling::span"
                                let email = InlineHtmlNavigator nav "//label[contains(., 'E-mail адрес')]/following-sibling::span"
                                let cmd5 = new MySqlCommand(addOrganizer, con)
                                cmd5.Parameters.AddWithValue("@full_name", orgName) |> ignore
                                cmd5.Parameters.AddWithValue("@contact_person", contactPerson) |> ignore
                                cmd5.Parameters.AddWithValue("@post_address", postAddress) |> ignore
                                cmd5.Parameters.AddWithValue("@fact_address", factAddress) |> ignore
                                cmd5.Parameters.AddWithValue("@contact_phone", phone) |> ignore
                                cmd5.Parameters.AddWithValue("@inn", inn) |> ignore
                                cmd5.Parameters.AddWithValue("@contact_email", email) |> ignore
                                cmd5.ExecuteNonQuery() |> ignore
                                IdOrg := int cmd5.LastInsertedId
                                ()
                                
                        let idEtp = this.GetEtp con settings
                        let numVersion = 1
                        let idPlacingWay = ref 0
                        match tn.PwayName with
                        | "" -> ()
                        | _ -> idPlacingWay := this.GetPlacingWay con tn.PwayName settings
                        let idRegion = this.GetReginId con tn.RegionName settings
                        
                        return ""
                        }
        match res with
                | Succ r -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        ()

    
    
    member private this.SetCancelStatus(con: MySqlConnection, dateUpd: DateTime) =
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