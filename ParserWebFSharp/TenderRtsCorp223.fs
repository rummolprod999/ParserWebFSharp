namespace ParserWeb

open MySql.Data.MySqlClient
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System
open System.Data
open TypeE
open HtmlAgilityPack
open Tools
open System.Web

type TenderRtsCorp223(stn: Settings.T, tn: RtsCorpRec, typeFz: int, etpName: string, etpUrl: string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() = ()