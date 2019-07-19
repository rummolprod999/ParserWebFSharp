namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data
open TypeE
open HtmlAgilityPack
open Tools

type TenderTj(stn: Settings.T, tn: TjRec, typeFz: int, etpName: string, etpUrl: string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0


    override this.Parsing() = ()