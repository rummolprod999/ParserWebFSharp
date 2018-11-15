namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open System.Linq
open System.Threading
open TypeE
open HtmlAgilityPack
open System.Collections.Generic
open System.Web

type TenderNorNic(stn : Settings.T, tn : NorNicRec, typeFz : int, etpName : string, etpUrl : string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0
    override this.Parsing() =
        printfn "%A" tn
        ()