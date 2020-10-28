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
    let _ = stn

    let urls = [|"https://zakupki.domru.ru/"|]

    override __.Parsing() =
        for url in urls do
            try
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex
        ()


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("table.contractor_search_table tr[id^='For_']").ToList().Skip(1)
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (_: string) =
        let builder = DocumentBuilder()
        let res = builder {
            printfn "%s" t.TextContent
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()