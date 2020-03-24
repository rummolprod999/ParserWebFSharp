namespace ParserWeb

open System
open System.Linq.Expressions
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq
open System.Linq.Expressions
open System.Web
open Tools
open System.Collections.Generic

type ParserApps(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"https://apps.chtpz.ru/tender/Tender.aspx/Index"|]

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
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("ul li a[href *='ViewTender']").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let purName = t.TextContent
            let href = t.GetAttribute("href")
            let href = sprintf "https://apps.chtpz.ru%s" href
            let! purNum = href.Get1Doc "/(\d+)$" <| sprintf "purNum not found %s %s " href (t.TextContent)
            let tend = {  AppsRec.Href = href
                          PurName = purName
                          PurNum = purNum}          
            let T = TenderApps(set, tend, 255, "АО «ПНТЗ»", "https://apps.chtpz.ru")
            T.Parsing()
            return ""
            }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()