namespace ParserWeb

open System
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserUni(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"https://unistream.ru/bank/about/tenders/"|]

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
            let tens = documents.QuerySelectorAll("div.item.document-list__item").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! href = t.GsnAtrDocWithError "div a" "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let href = sprintf "https://unistream.ru%s" href
            let! purName = t.GsnDocWithError "div a" <| sprintf "purName not found %s %s " url (t.TextContent)
            let purNum = Tools.createMD5 href
            let datePub = DateTime.Now
            let! dateEndT = t.GsnDocWithError "div.description.document-list__description" <| sprintf "dateEndT not found %s %s " url (t.TextContent)
            let! endDateT = dateEndT.Get1OptionalDoc "(?<=\s)(\d{2}.\d{2}.\d{4})"
            let! dateEnd = endDateT.DateFromStringDoc ("dd.MM.yyyy", sprintf "dateEnd not found %s %s " href endDateT)
            let tend = {  UniRec.Href = href
                          DateEnd = dateEnd
                          DatePub = datePub
                          PurNum = purNum
                          PurName = purName}
            let T = TenderUni(set, tend, 243, "АО КБ «Юнистрим»", "https://unistream.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()

