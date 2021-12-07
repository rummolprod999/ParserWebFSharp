namespace ParserWeb

open System.Threading
open System.Web
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserEnergyBase(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="https://energybase.ru/tender?page="

    override __.Parsing() =
            for i in 100..-1..1 do
            try
                __.ParsingPage(sprintf "%s%d" url i)
                Thread.Sleep(10000)
            with ex -> Logging.Log.logger ex


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadUseProxy (Settings.UseProxy, url)
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let htmlDoc = HtmlDocument()
            htmlDoc.LoadHtml(s)
            let nav = (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator
            let tens = nav.CurrentDocument.DocumentNode.SelectNodesOrEmpty("//div[@class = 'tender-card']").ToList()
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
            let! purName = t.GsnDocWithError ".//div[contains(@class, 'tender-card__title')]" <| sprintf "purName not found %s %s " url (t.InnerText)
            let! hrefT = t.GsnAtrDocWithError ".//div[contains(@class, 'tender-card__title')]/a" <| "href" <| sprintf "hrefT not found %s %s " url (t.InnerText)
            let href = sprintf "https://energybase.ru%s" hrefT
            let! pwName = t.GsnDoc ".//div[contains(@class, 'tender-item__purchase-code-name')]"
            let! purNum = t.GsnDocWithError ".//div[span[contains(., 'Регистрационный номер')]]" <| sprintf "purNum not found %s %s " url (t.InnerText)
            let purNum = purNum.Replace("Регистрационный номер", "").Trim()
            let! datePubT = t.GsnDocWithError ".//span[contains(., 'Дата объявления тендера / закупочной процедуры')]/.." <| sprintf "datePubT not found %s %s " href (t.InnerText)
            let datePubT = datePubT.ReplaceDate()
            let! datePubT = datePubT.Get1OptionalDoc(@"\d{2}\.\d{2}\.\d{4} г\., \d{2}:\d{2}")
            let! datePub = datePubT.DateFromStringDoc("dd.MM.yyyy г., HH:mm", sprintf "datePub not found %s %s " href (datePubT))
            let! dateEndT = t.GsnDoc ".//span[contains(., 'Дата и время окончания подачи заявок')]/.."
            let dateEndT = dateEndT.ReplaceDate()
            let! dateEndT = dateEndT.Get1OptionalDoc(@"\d{2}\.\d{2}\.\d{4} г\., \d{2}:\d{2}")
            let! dateEnd = dateEndT.DateFromStringDocMin("dd.MM.yyyy г., HH:mm")
            let! dateScoringT = t.GsnDoc ".//span[contains(., 'Дата и время рассмотрения заявок')]/following-sibling::span"
            let dateScoringT = dateScoringT.ReplaceDate()
            let! dateScoring = dateScoringT.DateFromStringDocMin("dd.MM.yyyy г., HH:mm")
            let! cusName = t.GsnDoc ".//span[@class = 'property-label' and contains(., 'Заказчик')]/following-sibling::a"
            let! nmckT = t.GsnDoc ".//div[@class = 'tender-card__sum']"
            let nmck = nmckT.GetPriceFromString()
            let! currency = nmckT.Get1OptionalDoc("\s+(.{1})$")
            let tend = {  Href = href
                          PurName = HttpUtility.HtmlDecode(purName)
                          PurNum = purNum
                          PwName = pwName
                          DateEnd = dateEnd
                          CusName = HttpUtility.HtmlDecode(cusName)
                          Nmck = nmck
                          DateScoring = dateScoring
                          Currency = currency
                          DatePub = datePub}          
            let T = TenderEnergyBase(set, tend, 300, "Energybase.ru", "https://energybase.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()