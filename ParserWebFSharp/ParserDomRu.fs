namespace ParserWeb

open System
open System.Collections.Generic
open System.Globalization
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserDomRu(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"https://zakupki.domru.ru/"|]

    override __.Parsing() =
        for url in urls do
            try
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex
        ()


    member private __.ParsingPage(url: string) = ()