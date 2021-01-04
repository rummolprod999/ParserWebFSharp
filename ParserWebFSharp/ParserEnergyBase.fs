namespace ParserWeb

open System.Web
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserEnergyBase(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="https://zakupki.rt-ci.ru/procurement/?PAGEN_1="

    override __.Parsing() =
            for i in 10..-1..1 do
            try
                __.ParsingPage(sprintf "%s%d" url i)
            with ex -> Logging.Log.logger ex


    member private __.ParsingPage(url: string) =
        ()