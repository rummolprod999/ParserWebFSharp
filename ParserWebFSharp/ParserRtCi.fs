namespace ParserWeb

open System.Web
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserRtCi(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="https://zakupki.rt-ci.ru/procurement/?PAGEN_1="

    override __.Parsing() =
            for i in 10..-1..1 do
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
            let tens = nav.CurrentDocument.DocumentNode.SelectNodesOrEmpty("//a[@class = 'card-procurement-info']").ToList()
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
            let! purName = t.GsnDocWithError ".//span[. = 'Предмет закупки']/following-sibling::span" <| sprintf "purName not found %s %s " url (t.InnerText)
            let! hrefT = t.GsnAtrDocWithError "." <| "href" <| sprintf "hrefT not found %s %s " url (t.InnerText)
            let href = sprintf "https://zakupki.rt-ci.ru%s" hrefT
            let! pwName = t.GsnDocWithError ".//div[@class = 'h4 card-procurement-info__head']" <| sprintf "pwName not found %s %s " url (t.InnerText)
            let! purNum = t.GsnDocWithError ".//div[@class = 'card-procurement-info__number']" <| sprintf "purNum not found %s %s " url (t.InnerText)
            let purNum = purNum.Replace("№", "").Trim()
            let! datePubT = t.GsnDocWithError ".//span[. = 'Дата размещения']/following-sibling::span" <| sprintf "datePubT not found %s %s " url (t.InnerText)
            let datePub = datePubT.DateFromStringOrMin("dd.MM.yyyy")
            let! dateEndT = t.GsnDoc ".//span[. = 'Дата и время окончания подачи']/following-sibling::span"
            let dateEnd = dateEndT.DateFromStringOrMin("dd.MM.yyyy HH:mm")
            let tend = {  Href = href
                          PurName = HttpUtility.HtmlDecode(purName)
                          PurNum = purNum
                          PwName = pwName
                          DateEnd = dateEnd
                          DatePub = datePub}          
            let T = TenderRtCi(set, tend, 295, "РТ-Комплектимпекс", "https://zakupki.rt-ci.ru/")
            T.Parsing()
            return ""
            }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()