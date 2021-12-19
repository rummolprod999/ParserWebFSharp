namespace ParserWeb

open System
open System.Web
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserBelorusNeft(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="http://www.belorusneft-siberia.ru/sitesib/ru/addUp/purchases/current/"

    override __.Parsing() =
            try
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let htmlDoc = HtmlDocument()
            htmlDoc.LoadHtml(s)
            let nav = (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator
            let tens = nav.CurrentDocument.DocumentNode.SelectNodesOrEmpty("//div[@class = 'b-tenders__item']").ToList()
            tens.Reverse()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: HtmlNode) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError ".//div[contains(@class, 'b-tenders__item_top')]" <| sprintf "purName not found %s %s " url (t.InnerText)
            let! hrefT = t.GsnAtrDocWithError ".//a[contains(@class, 'b-tenders__item_file')]" <| "href" <| sprintf "hrefT not found %s %s " url (t.InnerText)
            let href = sprintf "http://www.belorusneft-siberia.ru%s" hrefT
            let purNum = Tools.createMD5 href
            let! datePubT = t.GsnDocWithError ".//div[contains(@class, 'b-tenders__item_beginning')]/span" <| sprintf "datePubT not found %s %s " url (t.InnerText)
            let datePub = datePubT.DateFromStringOrMin("dd.MM.yyyy HH:mm")
            let! dateEndT = t.GsnDoc ".//div[contains(@class, 'b-tenders__item_end')]/span"
            let dateEnd = match dateEndT.DateFromStringOrMin("dd.MM.yyyy HH:mm") with
                          | x when x = DateTime.MinValue -> datePub.AddDays(2.)
                          | x -> x
            let tend = {  Href = href
                          PurName = purName
                          PurNum = purNum
                          Status = ""
                          DatePub = datePub
                          DateEnd = dateEnd}          
            let T = TenderBelorusNeft(set, tend, 330, "ООО «Белоруснефть-Сибирь»", "http://www.belorusneft-siberia.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()