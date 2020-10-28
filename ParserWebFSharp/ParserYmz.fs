namespace ParserWeb

open System
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq
open Tools

type ParserYmz(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"https://www.ymzmotor.ru/about/tenders/"|]

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
            let tens = documents.QuerySelectorAll("ol.list_tender li div").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError "p.tender-item__title a" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! href = t.GsnAtrDocWithError "p.tender-item__title a" "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let href = sprintf "https://www.ymzmotor.ru%s" href
            let purNum = createMD5(purName)
            let datePub = DateTime.Now
            let! dateEndT = t.GsnDocWithError "div.tender-item__descr p" <| sprintf "dateEndT not found %s %s " url (t.TextContent)
            let dateEndT = dateEndT.Replace("Срок подачи заявки до", "").Trim()
            let! dateEnd = dateEndT.DateFromStringDoc ("dd.MM.yyyy", sprintf "dateEnd not found %s %s " href dateEndT)
            let tend = {  Href = href
                          DateEnd = dateEnd
                          DatePub = datePub
                          PurNum = purNum
                          PurName = purName}
            let T = TenderYmz(set, tend, 253, "«ЯМЗ»", "https://www.ymzmotor.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()