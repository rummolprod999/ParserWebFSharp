namespace ParserWeb

open System.Web
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserEstp(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="http://estp.ru/auctions/estp/?PAGEN_2="

    override __.Parsing() =
            for i in 20..-1..1 do
            try
                __.ParsingPage(sprintf "%s%d" url i)
            with ex -> Logging.Log.logger ex


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let htmlDoc = HtmlDocument()
            htmlDoc.LoadHtml(s)
            let nav = (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator
            let tens = nav.CurrentDocument.DocumentNode.SelectNodesOrEmpty("//li[@class = 'estp-tenders-list__i']").ToList()
            tens.Reverse()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: HtmlNode) (_: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! typeT = t.GsnDoc ".//span[. = 'Электронный магазин']"
            if typeT = "" then return ""
            let! purNum = t.GsnDocWithError ".//dt[. = 'Код закупки:']/following-sibling::dd/strong" <| sprintf "purNum not found %s %s " url (t.InnerText)
            let! hrefT = t.GsnAtrDocWithError ".//dd[@class = 'estp-tenders__data-definition']/a" <| "href" <| sprintf "hrefT not found %s %s " url (t.InnerText)
            let href = sprintf "http://estp.ru%s" hrefT
            let! status = t.GsnDoc ".//dt[. = 'Этап:']/following-sibling::dd/strong"
            let! pwName = t.GsnDoc ".//dt[. = 'Способ закупки:']/following-sibling::dd"
            let! orgName = t.GsnDoc ".//dt[. = 'Организатор торгов:']/following-sibling::dd"
            let! purName = t.GsnDocWithError ".//dd[@class = 'estp-tenders__data-definition']/a" <| sprintf "purName not found %s %s " url (t.InnerText)
            let! region = t.GsnDoc ".//dt[. = 'Регион:']/following-sibling::dd"
            let! datePubT = t.GsnDocWithError ".//dt[. = 'Дата публикации: (МСК)']/following-sibling::dd/time" <| sprintf "datePubT not found %s %s " href (t.InnerText)
            let! datePub = datePubT.DateFromStringDoc("dd.MM.yyyy HH:mm", sprintf "datePub not found %s %s " href (datePubT))
            let! dateEndT = t.GsnDocWithError ".//dt[. = 'Дата окончания приема заявок: (МСК)']/following-sibling::dd/time" <| sprintf "dateEndT not found %s %s " href (t.InnerText)
            let! dateEnd = dateEndT.DateFromStringDoc("dd.MM.yyyy HH:mm", sprintf "dateEnd not found %s %s " href (dateEndT))
            let! nmckT = t.GsnDoc ".//span[@class = 'estp-tenders__price-value']"
            let nmck = nmckT.GetPriceFromString()
            let! currency = nmckT.Get1OptionalDoc("(\w+)$")
            let tend = {  Href = href
                          PurName = HttpUtility.HtmlDecode(purName)
                          PurNum = purNum
                          PlacingWay = pwName
                          DateEnd = dateEnd
                          OrgName = HttpUtility.HtmlDecode(orgName)
                          Price = nmck
                          Status = status
                          Region = region
                          Currency = currency
                          DatePub = datePub}          
            let T = TenderEstp(set, tend, 308, "ESTP.RU", "http://estp.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        ()