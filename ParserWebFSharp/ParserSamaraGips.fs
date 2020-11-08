namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq
open TypeE

type ParserSamaraGips(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"https://samaragips.ru/tender/"|]

    override __.Parsing() =
        for url in urls do
            try
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex
        ()


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        ()