namespace ParserWeb

open MySql.Data.MySqlClient
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open System.Data
open System.Linq
open System.Text.RegularExpressions
open System.Threading
open TypeE

type TenderStroyTorgi(stn : Settings.T, tn : AeroRec, typeFz : int, etpName : string, etpUrl : string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    
    override this.Parsing() = ()