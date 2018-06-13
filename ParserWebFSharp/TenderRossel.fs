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

type TenderRossel(stn : Settings.T) = 
    inherit Tender("АО «ЕЭТП» «Росэлторг»", "https://www.roseltorg.ru")
    let settings = stn
    let typeFz = 42
    static member val tenderCount = ref 0
    
    override this.Parsing() =
        ()