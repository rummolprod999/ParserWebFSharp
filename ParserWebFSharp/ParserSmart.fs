namespace ParserWeb

open System
open System.Linq
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Collections.Generic

type ParserSmart(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 10
    let spage = "https://smarttender.biz/komertsiyni-torgy/?p="

    override __.Parsing() =
        for i in 1..pageC do
            try
                let url = sprintf "%s%d" spage i
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex

    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("#tenders tbody tr")
            for t in tens do
                try
                    __.ParsingTender t url
                with ex -> Logging.Log.logger ex
            ()
        ()

    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
                       return ""
                   }
        match res with
                | Succ r -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        ()
