namespace ParserWeb

open TypeE
open HtmlAgilityPack
open System.Linq

type ParserOsnova(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="https://tender.gk-osnova.ru/site?page="

    override __.Parsing() =
            for i in 5..-1..1 do
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
            let tens = nav.CurrentDocument.DocumentNode.SelectNodesOrEmpty("//div[@class = 'grid-view']//tbody/tr").ToList()
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
            let! purName = t.GsnDocWithError "./td[1]/a" <| sprintf "purName not found %s %s " url (t.InnerText)
            let! hrefT = t.GsnAtrDocWithError "./td[1]/a" <| "href" <| sprintf "hrefT not found %s %s " url (t.InnerText)
            let href = sprintf "https://tender.gk-osnova.ru%s" hrefT
            let! purNum = href.Get1Doc "id=(\d+)$" <| sprintf "purNum not found %s %s " url (t.InnerText)
            let! dates = t.GsnDocWithError "./td[2]" <| sprintf "dates not found %s %s " url (t.InnerText)
            let! dateEndT = dates.Get1Doc "с\s+(\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2}:\d{2})" <| sprintf "dateEndT not found %s %s " url (dates)
            let dateEnd = dateEndT.DateFromStringOrMin("dd.MM.yyyy HH:mm:ss")
            let! datePubT = dates.Get1Doc "по\s+(\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2}:\d{2})" <| sprintf "datePubT not found %s %s " url (dates)
            let datePub = datePubT.DateFromStringOrMin("dd.MM.yyyy HH:mm:ss")
            let tend = {  OsnovaRec.Href = href
                          PurName = purName
                          PurNum = purNum
                          DateEnd = dateEnd
                          DatePub = datePub}          
            let T = TenderOsnova(set, tend, 287, "ГК \"ОСНОВА\"", "https://tender.gk-osnova.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()