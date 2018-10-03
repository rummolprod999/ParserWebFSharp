namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open System.Linq
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

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
                "SELECT id_tender FROM %stender WHERE purchase_number = @purchase_number AND type_fz = @type_fz AND end_date = @end_date AND doc_publish_date = @doc_publish_date" 
                stn.Prefix
        let cmd : MySqlCommand = new MySqlCommand(selectTend, con)
        cmd.Prepare()
        cmd.Parameters.AddWithValue("@purchase_number", tn.PurNum) |> ignore
        cmd.Parameters.AddWithValue("@type_fz", typeFz) |> ignore
        cmd.Parameters.AddWithValue("@end_date", tn.DateEnd) |> ignore
        cmd.Parameters.AddWithValue("@doc_publish_date", tn.DatePub) |> ignore
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
            printfn "%s" OrgName
            ()
