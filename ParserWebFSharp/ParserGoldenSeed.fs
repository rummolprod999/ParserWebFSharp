namespace ParserWeb

open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserGoldenSeed(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="https://www.goldenseed.ru/tenders/?PAGEN_2="

    override __.Parsing() =
        for i in 5..-1..1 do
            try
                __.ParsingPage(sprintf "%s%d" url i)
            with ex -> Logging.Log.logger ex
        ()


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("div.vacancy-filter-result-item-left").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError "div.vacancy-filter-result-item-title > a" <| sprintf "purName not found %s %s " url (t.TextContent)
            let purNum = Tools.createMD5 purName
            let! status = t.GsnDocWithError "ul.vacancy-filter-result-item-tags li:nth-child(2) a" <| sprintf "status not found %s %s " url (t.TextContent)
            let! dates = t.GsnDocWithError "ul.vacancy-filter-result-item-tags li:nth-child(1) a" <| sprintf "dates not found %s %s " url (t.TextContent)
            let! dateEndT = dates.Get1Doc "(\d{2}\.\d{2}\.\d{4})$" <| sprintf "dateEndT not found %s %s " url (dates)
            let dateEnd = dateEndT.DateFromStringOrMin("dd.MM.yyyy")
            let! datePubT = dates.Get1Doc "^(\d{2}\.\d{2}\.\d{4})" <| sprintf "datePubT not found %s %s " url (dates)
            let datePub = datePubT.DateFromStringOrMin("dd.MM.yyyy")
            let! typeT = t.GsnDocWithError "ul.vacancy-filter-result-item-tags li:nth-child(3) a" <| sprintf "typeT not found %s %s " url (t.TextContent)
            let! cusName = purName.Get1OptionalDoc "для нужд\s*(?:[ф|Ф]илиала)*\s*(.+)$"
            let tend = {  Href = url
                          PurName = purName
                          PurNum = purNum
                          CusName = cusName
                          Type = typeT
                          DateEnd = dateEnd
                          DatePub = datePub
                          Status = status }          
            let T = TenderGoldenSeed(set, tend, 283, "ГК «Юг Руси»", "https://www.goldenseed.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()