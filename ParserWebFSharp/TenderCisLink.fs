namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Remote
open OpenQA.Selenium.Support.UI
open System.Threading
open TypeE
open Tools

type TenderCisLink(stn: Settings.T, tn: CisLinkRec, typeFz: int, etpName: string, etpUrl: string, driver: ChromeDriver) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() = ()