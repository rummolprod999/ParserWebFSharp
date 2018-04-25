namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open MySql.Data.MySqlClient
open System
open System.Data
open System.Linq
open TypeE

type TenderLsr(stn : Settings.T, urlT : string, purNum : string) = 
    inherit Tender("«Группа ЛСР»", 
                   "http://zakupki.lsrgroup.ru")
    let settings = stn
    let typeFz = 34
    static member val tenderCount = ref 0
    
    override this.Parsing() =
        ()