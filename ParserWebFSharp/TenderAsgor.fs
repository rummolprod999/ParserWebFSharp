namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open MySql.Data.MySqlClient
open System
open System.Data
open System.Linq
open TypeE

type TenderAsgor(stn : Settings.T, tn : AeroRec, typeFz : int, etpName : string, etpUrl : string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0
    
    override this.Parsing() = ()
