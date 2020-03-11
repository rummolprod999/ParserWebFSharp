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

type ParserKsk(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"http://www.gt-ksk.com/about/tenders.php"|]

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
            let tens = documents.QuerySelectorAll("table.lot").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let href = url
            let! purName = t.GsnDocWithError "div.lot_name a" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! purNum = t.GsnDocWithError "div.lot_number" <| sprintf "purNum not found %s %s " url (t.TextContent)
            let! pwName = t.GsnDocWithError "div.lot_type" <| sprintf "pwName not found %s %s " url (t.TextContent)
            let! status = t.GsnDocWithError "div.lot_status" <| sprintf "status not found %s %s " url (t.TextContent)
            let! dates = t.GsnDocWithError "div.lot_date" <| sprintf "dates not found %s %s " url (t.TextContent)
            let! endDateT = dates.Get1OptionalDoc "—\s+(\d{2}.\d{2}.\d{4} \d{2}:\d{2}:\d{2})"
            let! dateEnd = endDateT.DateFromStringDoc ("dd.MM.yyyy H:mm:ss", sprintf "dateEnd not found %s %s " href endDateT)
            let! dateStartT = dates.Get1OptionalDoc "^(\d{2}.\d{2}.\d{4})"
            let! datePub = dateStartT.DateFromStringDoc ("dd.MM.yyyy", sprintf "datePub not found %s %s " href dateStartT)
            let! prices = t.GsnDocWithError "div.lot_price" <| sprintf "prices not found %s %s " url (t.TextContent)
            let! priceT =  prices.Get1OptionalDoc "Стоимость закупки:\s+([\d\s,]+)"
            let price = priceT.GetPriceFromString()
            let docs = t.QuerySelectorAll("div.lot_docs ul li a").ToList()
            let tend = {  Href = href
                          DateEnd = dateEnd
                          DatePub = datePub
                          PurNum = purNum
                          PurName = purName
                          Status = status
                          PwName = pwName
                          Nmck = price
                          DocList = docs}
            let T = TenderKsk(set, tend, 246, "АО \"КСК\"", "http://www.gt-ksk.com/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()