namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open TypeE
open Tools

type TenderSmart(stn : Settings.T, tn : SmartRec, typeFz : int, etpName : string, etpUrl : string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() = ()