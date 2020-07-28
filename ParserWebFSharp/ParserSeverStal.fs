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

type ParserSeverStal(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"https://www.severstal.com/rus/suppliers/srm/tenders/"|]

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
            let tens = documents.QuerySelectorAll("div.box.procedures table.table tbody tr").ToList().Skip(1)
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
        
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError "td:nth-child(2)" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! purNum = t.GsnDocWithError "td:nth-child(1)" <| sprintf "purNum not found %s %s " url (t.TextContent)
            let! href = t.GsnAtrDocWithError "td:nth-child(2) a" "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let! addInfo = t.GsnDocWithError "td:nth-child(3)" <| sprintf "addInfo not found %s %s " url (t.TextContent)
            let! dates = t.GsnDocWithError "td:nth-child(4)" <| sprintf "dates not found %s %s " url (t.TextContent)
            let dates = dates.RegexCutWhitespace()
            let! dateEndT = dates.Get1Doc "(\d{2}\.\d{2}\.\d{4}\s{1}\d{2}:\d{2}:\d{2})" <| sprintf "dateEndT not found %s %s " href (dates)
            let dateEnd = dateEndT.DateFromStringOrMin("dd.MM.yyyy HH:mm:ss")
            let tend = {  Href = href
                          PurName = purName
                          PurNum = purNum
                          AddInfo = addInfo
                          DateEnd = dateEnd
                          DatePub = DateTime.Now}          
            let T = TenderSeverStal(set, tend, 262, "ПАО «Северсталь»", "https://www.severstal.com/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()