namespace ParserWeb

open System
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html

type ParserTurk(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 2000
    
    override this.Parsing() =
        for i in 1..pageC do
            try 
                let url = sprintf "https://turkmenportal.com/catalog/a/index?path=tendery-turkmenistana&page=%d" i
                this.ParsingPage url
            with ex -> Logging.Log.logger ex
    
    member private this.ParsingPage(url : string) = ()