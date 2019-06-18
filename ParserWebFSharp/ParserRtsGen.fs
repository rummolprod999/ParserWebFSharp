namespace ParserWeb

open System
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Web

type ParserRtsGen(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 2 //TODO change it
    let spage = "https://223.rts-tender.ru/supplier/auction/Trade/Search.aspx"

    override __.Parsing() = ()