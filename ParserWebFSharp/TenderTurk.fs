namespace ParserWeb

open MySql.Data.MySqlClient
open System
open System.Data

type TenderTurk(stn : Settings.T, typeFz : int, etpName : string, etpUrl : string, purNum: string, purName: string, datePub: DateTime, dateEnd: DateTime, href: string, address: string, contacts: string, orgName: string) =
    inherit Tender(etpName, etpUrl)
    let settings = stn
    static member val tenderCount = ref 0
    static member val tenderUpCount = ref 0
    override this.Parsing() = ()