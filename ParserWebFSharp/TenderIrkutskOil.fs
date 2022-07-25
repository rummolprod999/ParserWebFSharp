namespace ParserWeb

open AngleSharp.Parser.Html
open MySql.Data.MySqlClient
open System
open System.Data
open TypeE

type TenderIrkutskOil(stn: Settings.T, urlT: string) =
    inherit Tender("«Иркутская нефтяная компания»", "https://tenders.irkutskoil.ru/tenders.php")
    let settings = stn
    let typeFz = 30
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0

    override this.Parsing() =
        let Page = Download.DownloadString urlT

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get start page", urlT)
        | s -> this.ParserPage(s)

        ()

    member private this.GetDateS(input: string) : string option =
        match input with
        | Tools.RegexMatch1 @"(\d{2}\.\d{2}\.\d{4} \d{2}:\d{2})" (gr1) ->
            Some((sprintf "%s" gr1))
        | _ -> None

    member private this.ParserPage(p: string) =
        let parser = HtmlParser()
        let doc = parser.Parse(p)

        let purNumT =
            doc.QuerySelector("div:contains('№ тендера') + div")

        match purNumT with
        | null ->
            raise
            <| NullReferenceException(sprintf "purNum not found in %s" urlT)
        | _ -> ()

        let purNum = purNumT.TextContent.Trim()
        let purNameT = doc.QuerySelector("div:contains('Наименование процедуры') + div")

        match purNameT with
        | null ->
            raise
            <| NullReferenceException(sprintf "purName not found in %s" urlT)
        | _ -> ()

        let purName = purNameT.TextContent.Trim()

        let pubDateT =
            doc.QuerySelector("div:contains('Начало приёма заявок') + div")

        match pubDateT with
        | null ->
            raise
            <| NullReferenceException(sprintf "pubDate not found in %s" urlT)
        | _ -> ()

        let mutable pubDateS =
            pubDateT.TextContent.Trim()

        match this.GetDateS(pubDateS) with
        | Some dtP -> pubDateS <- dtP
        | None ->
            raise
            <| Exception(sprintf "cannot apply regex to datePub %s" urlT)

        let datePub =
            match pubDateS.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d
            | None ->
                raise
                <| Exception(sprintf "cannot parse datePub %s" pubDateS)

        let endDateT =
            doc.QuerySelector("div:contains('Окончание приёма заявок') + div")

        match endDateT with
        | null ->
            raise
            <| NullReferenceException(sprintf "endDate not found in %s" urlT)
        | _ -> ()

        let mutable endDateS =
            endDateT.TextContent.Trim()

        match this.GetDateS(endDateS) with
        | Some dtP -> endDateS <- dtP
        | None ->
            raise
            <| Exception(sprintf "cannot apply regex to endDate %s" urlT)

        let endDate =
            match endDateS.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d
            | None ->
                raise
                <| Exception(sprintf "cannot parse endDate %s" endDateS)

        let dateUpd = datePub
        use con = new MySqlConnection(stn.ConStr)
        con.Open()
        let href = urlT

        let selectTend =
            sprintf
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND date_version = @date_version AND type_fz = @type_fz"
                stn.Prefix

        let cmd: MySqlCommand =
            new MySqlCommand(selectTend, con)

        cmd.Prepare()

        cmd.Parameters.AddWithValue("@purchase_number", purNum)
        |> ignore

        cmd.Parameters.AddWithValue("@date_version", dateUpd)
        |> ignore

        cmd.Parameters.AddWithValue("@type_fz", typeFz)
        |> ignore

        let reader: MySqlDataReader =
            cmd.ExecuteReader()

        if reader.HasRows then
            reader.Close()
        else
            reader.Close()
            let mutable cancelStatus = 0
            let mutable updated = false

            let selectDateT =
                sprintf
                    "SELECT id_tender, date_version, cancel FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz"
                    stn.Prefix

            let cmd2 =
                new MySqlCommand(selectDateT, con)

            cmd2.Prepare()

            cmd2.Parameters.AddWithValue("@purchase_number", purNum)
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
            let NoticeVersion = ""
            let Printform = href
            let IdOrg = ref 0
            let OrgInn = "3808066311"
            let OrgName = this.EtpName

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
                            "INSERT INTO %sorganizer SET full_name = @full_name, contact_phone = @contact_phone, contact_person = @contact_person, contact_email = @contact_email, inn = @inn, kpp = @kpp, post_address = @post_address"
                            stn.Prefix

                    let contactPhone = "+7 (3952) 211-352"
                    let contactPerson = ""
                    let contactEmail = "info@irkutskoil.ru"

                    let postAddress =
                        "Россия, 664007, г. Иркутск, пр-кт Большой Литейный, д. 4"

                    let cmd5 =
                        new MySqlCommand(addOrganizer, con)

                    cmd5.Parameters.AddWithValue("@full_name", OrgName)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@inn", OrgInn)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@kpp", "384901001")
                    |> ignore

                    cmd5.Parameters.AddWithValue("@contact_phone", contactPhone)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@contact_person", contactPerson)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@contact_email", contactEmail)
                    |> ignore

                    cmd5.Parameters.AddWithValue("@post_address", postAddress)
                    |> ignore

                    cmd5.ExecuteNonQuery() |> ignore
                    IdOrg := int cmd5.LastInsertedId

            let idPlacingWay = ref 0
            let idEtp = this.GetEtp con settings
            let numVersion = 1
            let mutable idRegion = 0
            let regionS = Tools.GetRegionString("иркут")

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

            cmd9.Parameters.AddWithValue("@id_xml", purNum)
            |> ignore

            cmd9.Parameters.AddWithValue("@purchase_number", purNum)
            |> ignore

            cmd9.Parameters.AddWithValue("@doc_publish_date", datePub)
            |> ignore

            cmd9.Parameters.AddWithValue("@href", href)
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

            cmd9.Parameters.AddWithValue("@end_date", endDate)
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

            cmd9.Parameters.AddWithValue("@notice_version", NoticeVersion)
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
            | true -> incr TenderIrkutskOil.tenderUpCount
            | false -> incr TenderIrkutskOil.tenderCount

            let lotNumber = 1
            let idLot = ref 0

            let insertLot =
                sprintf "INSERT INTO %slot SET id_tender = @id_tender, lot_number = @lot_number" stn.Prefix

            let cmd12 = new MySqlCommand(insertLot, con)

            cmd12.Parameters.AddWithValue("@id_tender", !idTender)
            |> ignore

            cmd12.Parameters.AddWithValue("@lot_number", lotNumber)
            |> ignore

            cmd12.ExecuteNonQuery() |> ignore
            idLot := int cmd12.LastInsertedId
            let idCustomer = ref 0

            if OrgName <> "" then
                let selectCustomer =
                    sprintf "SELECT id_customer FROM %scustomer WHERE full_name = @full_name" stn.Prefix

                let cmd3 =
                    new MySqlCommand(selectCustomer, con)

                cmd3.Prepare()

                cmd3.Parameters.AddWithValue("@full_name", OrgName)
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

                    cmd14.Parameters.AddWithValue("@full_name", OrgName)
                    |> ignore

                    cmd14.Parameters.AddWithValue("@inn", OrgInn)
                    |> ignore

                    cmd14.ExecuteNonQuery() |> ignore
                    idCustomer := int cmd14.LastInsertedId

            let insertLotitem =
                sprintf
                    "INSERT INTO %spurchase_object SET id_lot = @id_lot, id_customer = @id_customer, name = @name"
                    stn.Prefix

            let cmd19 =
                new MySqlCommand(insertLotitem, con)

            cmd19.Prepare()

            cmd19.Parameters.AddWithValue("@id_lot", !idLot)
            |> ignore

            cmd19.Parameters.AddWithValue("@id_customer", !idCustomer)
            |> ignore

            cmd19.Parameters.AddWithValue("@name", purName)
            |> ignore

            cmd19.ExecuteNonQuery() |> ignore

            let documents =
                doc.QuerySelectorAll("a.tender-detail-link")

            for dc in documents do
                let mutable nameF = "Документация"

                let urlAtt = dc.GetAttribute("href").Trim()

                if urlAtt <> "" then
                    let insertDoc =
                        sprintf
                            "INSERT INTO %sattachment SET id_tender = @id_tender, file_name = @file_name, url = @url"
                            stn.Prefix

                    let cmd20 = new MySqlCommand(insertDoc, con)
                    cmd20.Prepare()

                    cmd20.Parameters.AddWithValue("@id_tender", !idTender)
                    |> ignore

                    cmd20.Parameters.AddWithValue("@file_name", nameF)
                    |> ignore

                    cmd20.Parameters.AddWithValue("@url", "https://lkk.irkutskoil.ru" + urlAtt)
                    |> ignore

                    cmd20.ExecuteNonQuery() |> ignore

            try
                this.AddVerNumber con purNum stn typeFz
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
