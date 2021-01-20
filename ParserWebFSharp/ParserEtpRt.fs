namespace ParserWeb

open System.Web
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserEtpRt(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="https://etp-rt.ru/search/open?page="

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
            let tens = nav.CurrentDocument.DocumentNode.SelectNodesOrEmpty("//div[@class = 'lot_single']").ToList()
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
            let! hrefT = t.GsnAtrDocWithError "./a" <| "href" <| sprintf "hrefT not found %s %s " url (t.InnerText)
            let href = sprintf "https://etp-rt.ru%s" hrefT
            let! purNum = href.Get1Doc "/(\d+)$" <| sprintf "purNum not found %s %s " href (t.InnerText)
            let tend = {  Href = href
                          PurNum = purNum}          
            let T = TenderEtpRt(set, tend, 303, "Электронная торговая площадка etp-rt.ru", "https://etp-rt.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        ()