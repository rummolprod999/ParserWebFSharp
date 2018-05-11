namespace ParserWeb

open MySql.Data.MySqlClient
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open System.Data
open System.Linq
open TypeE

type TenderButb(stn : Settings.T, purNum : string, datePub : DateTime, endDate : DateTime, biddingDate : DateTime, driver : ChromeDriver, wait : WebDriverWait, page : int, status : string) = 
    inherit Tender("ОАО «Белорусская универсальная товарная биржа»", "http://zakupki.butb.by")
    let settings = stn
    let typeFz = 36
    static member val tenderCount = ref 0
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
                //printfn "%A" <| (row.["date_version"])
                match dateUpd >= ((row.["date_version"]) :?> DateTime) with
                | true -> row.["cancel"] <- 1
                | false -> cancelStatus <- 1
            let commandBuilder = new MySqlCommandBuilder(adapter)
            commandBuilder.ConflictOption <- ConflictOption.OverwriteChanges
            adapter.Update(dt) |> ignore
            let Printform = "http://zakupki.butb.by/auctions/reestrauctions.html"
            let IdOrg = ref 0
            let OrgName = 
                this.GetDefaultFromNullS 
                <| driver.FindElement
                       (By.XPath
                            (String.Format("//table[contains(@id, 'auctionList')]/tbody/tr[{0}]/td[8]/span[1]", page)))
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
