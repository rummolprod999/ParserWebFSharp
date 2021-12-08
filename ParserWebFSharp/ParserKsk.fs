namespace ParserWeb

open System
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserKsk(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"http://www.gt-ksk.com/about/tenders/"|]

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
            let tens = documents.QuerySelectorAll("div.tenders__block.tenders-block").ToList()
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
            let! purName = t.GsnDocWithError "div.tenders-block__text" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! purNum = t.GsnDocWithError "div.tenders-block__status-num" <| sprintf "purNum not found %s %s " url (t.TextContent)
            let purNum = purNum.Replace("№", "").Trim()
            let purNum = sprintf "%s-%s" purNum (Tools.createMD5 purName)
            let! pwName = t.GsnDoc "div.tenders-block__row-item:nth-child(2)"
            let pwName = pwName.Replace("Способ закупки:", "").Trim()
            let! status = t.GsnDoc "div.lot_status"
            let! dates = t.GsnDocWithError "div.tenders-block__date" <| sprintf "dates not found %s %s " url (t.TextContent)
            
            let mutable datePubW = DateTime.MinValue
            let! dateStartT = dates.Get1OptionalDoc "^(\d{2}.\d{2}.\d{4}\s\d{2}:\d{2}:\d{2})"
            if dateStartT <> ""
                then
                    let! datePub = dateStartT.DateFromStringDocMin ("dd.MM.yyyy HH:mm:ss")
                    datePubW <- datePub
                else
                    let! dateStartT = dates.Get1OptionalDoc "^(\d{2}.\d{2}.\d{4})"
                    let! datePub = dateStartT.DateFromStringDoc ("dd.MM.yyyy", sprintf "datePub not found %s %s " href dateStartT)
                    datePubW <- datePub
            
            let mutable dateEndW = DateTime.MinValue
            let! endDateT = dates.Get1OptionalDoc "—\s+(\d{2}.\d{2}.\d{4} \d{2}:\d{2}:\d{2})"
            if endDateT <> ""
                then
                    let! dateEnd = endDateT.DateFromStringDocMin ("dd.MM.yyyy HH:mm:ss")
                    dateEndW <- dateEnd
                    ()
                else
                    let! endDateT = dates.Get1OptionalDoc "—\s*(\d{2}.\d{2}.\d{4})"
                    let! dateEnd = endDateT.DateFromStringDocMin ("dd.MM.yyyy")
                    dateEndW <- dateEnd
                    ()
            
            if dateEndW = DateTime.MinValue then dateEndW <- datePubW.AddDays(2.)
            
            let! prices = t.GsnDoc "div.tenders-block__hide div.tenders-block__row-item"
            let prices = prices.Replace("Стоимость закупки:", "").Trim()
            let! priceT =  prices.Get1OptionalDoc "([\d\s,]+)"
            let price = priceT.GetPriceFromString()
            let docs = t.QuerySelectorAll("li a").ToList()
            let tend = {  Href = href
                          DateEnd = dateEndW
                          DatePub = datePubW
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