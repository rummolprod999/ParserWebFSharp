namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System.Threading
open TypeE
open Tools

type TenderEten(stn: Settings.T, tn: EtenRec, typeFz: int, etpName: string, etpUrl: string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() =
        let builder = TenderBuilder()
        use con = new MySqlConnection(stn.ConStr)
        let dateUpd = DateTime.Now
        ()