namespace ParserWeb

open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserGmt(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"http://gazprom-gmt.ru/purchase/current"|]

    override __.Parsing() =
        for url in urls do
            try
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex
        ()


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadStringRts url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("tr.tendertable_tr").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError "a.tenders_link" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! href = t.GsnAtrDocWithError "a.tenders_link" "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let href = sprintf "http://gazprom-gmt.ru%s" href
            let! purNum = t.GsnDocWithError "td.tendertable_td-number" <| sprintf "purNum not found %s %s " url (t.TextContent)
            let purNum = purNum.Replace("№", "").Trim()
            let! pwName = t.GsnDocWithError "td.tendertable_td-type" <| sprintf "purNum not found %s %s " url (t.TextContent)
            let pwName = pwName.Replace("Способ закупки", "").Trim()
            
            let! datePubT = t.GsnDocWithError "td.tendertable_td-start" <| sprintf "datePubT not found %s %s " url (t.TextContent)
            let datePubT = datePubT.Replace("Дата начала подачи заявок", "").Trim()
            let! datePub = datePubT.DateFromStringDoc ("dd.MM.yyyy", sprintf "datePub not found %s %s " href datePubT)
            
            let! dateEndT = t.GsnDocWithError "td.tendertable_td-end" <| sprintf "dateEndT not found %s %s " url (t.TextContent)
            let dateEndT = dateEndT.Replace("Дата окончания подачи заявок", "").Trim()
            let! dateEnd = dateEndT.DateFromStringDoc ("dd.MM.yyyy", sprintf "dateEnd not found %s %s " href dateEndT)
            let tend = {  GmtRec.Href = href
                          DateEnd = dateEnd
                          DatePub = datePub
                          PurNum = purNum
                          PurName = purName
                          PwName = pwName}
            let T = TenderGmt(set, tend, 252, "ООО «Газпром газомоторное топливо»", "http://gazprom-gmt.ru")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()