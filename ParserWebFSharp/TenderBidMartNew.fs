namespace ParserWeb

open System.Threading
open MySql.Data.MySqlClient
open System
open System.Data
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open Tools
open TypeE
open System.Collections.Generic
open System.Web

type TenderBidMartNew(stn: Settings.T, tn: BidMartNewRec, typeFz: int, etpName: string, etpUrl: string, driver : ChromeDriver) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() =
        let builder = TenderBuilder()
        use con = new MySqlConnection(stn.ConStr)
        let res =
                   builder {
                        con.Open()
                        let selectTend = sprintf "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date" stn.Prefix
                        let cmd: MySqlCommand = new MySqlCommand(selectTend, con)
                        cmd.Prepare()
                        cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
                        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
                        cmd.Parameters.AddWithValue("@end_date", tn.DateEnd) |> ignore
                        let reader: MySqlDataReader = cmd.ExecuteReader()
                        if reader.HasRows then reader.Close()
                                               return! Error ""
                        reader.Close()
                        let wait = new WebDriverWait(driver, TimeSpan.FromSeconds(60.))
                        driver.Navigate().GoToUrl(tn.Href)
                        Thread.Sleep(1000)
                        driver.SwitchTo().DefaultContent() |> ignore
                        wait.Until (fun dr -> dr.FindElement(By.XPath("//div[@class = 'header']")).Displayed) |> ignore
                        let body = driver.FindElement(By.XPath("//body"))
                        let! pubDateT = body.findElementWithoutException("//label[. = 'Дата создания закупки']/following-sibling::span", sprintf <|"pubDateT not found %s" <| tn.Href)
                        printfn "%s" pubDateT
                        return ""
                   }
        match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
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