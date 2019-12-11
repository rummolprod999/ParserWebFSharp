namespace ParserWeb

open System
open System.Linq.Expressions
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Web

type ParserMpkz(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 100
    let spage = "https://mp.kz/?TenderLots=list&sort=2&page="

    override __.Parsing() =
        for i in pageC .. -1 .. 1 do
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
            let tens = documents.QuerySelectorAll("div#lot-list div.row.list-lot")
            for t in tens do
                try
                    __.ParsingTender t url
                with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
                       let! href = t.GsnAtrDocWithError "div.list-lot__name a" "href" <| sprintf "href not found %s %s " url (t.TextContent)
                       let! purNum = t.GsnDocWithError "div.lot__tags span:contains('Тендер') > span" <| sprintf "purNum not found %s %s " url (t.TextContent)
                       let href = sprintf "https://mp.kz%s" href
                       let! purName = t.GsnDocWithError "div.list-lot__name a" <| sprintf "purName not found %s %s " url (t.TextContent)
                       let! orgName = t.GsnDocWithError "small a" <| sprintf "orgName not found %s %s " url (t.TextContent)
                       let! dateEndT = t.GsnDocWithError "small[title='Дата завершения торга']" <| sprintf "dateEndT not found %s %s " url (t.TextContent)
                       let! dateEnd = dateEndT.DateFromStringDoc ("dd.MM.yyyy HH:mm", sprintf "dateEndT not found %s %s " href dateEndT)
                       let! nmck = t.GsnDocWithError "span.list-lot__price" <| sprintf "nmck not found %s %s " url (t.TextContent)
                       let nmck = nmck.GetPriceFromStringKz()
                       let tend = { Href = href
                                    DateEnd = dateEnd
                                    PurName = purName
                                    PurNum = purNum
                                    OrgName = orgName
                                    Nmck = nmck }
                       let T = TenderMpkz(set, tend, 227, "Торговая площадка Mp.kz", "https://mp.kz/")
                       T.Parsing()
                       return ""
                       }
        match res with
                | Succ r -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()