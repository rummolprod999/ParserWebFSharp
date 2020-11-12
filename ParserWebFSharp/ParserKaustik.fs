namespace ParserWeb

open TypeE
open HtmlAgilityPack
open System.Linq

type ParserKaustik(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="https://www.kaustik.ru/ru/index.php/partneram/konkursy/tekushchie-konkursy"

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
            htmlDoc.LoadHtml(Page)
            let nav = (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator
            let tens = nav.CurrentDocument.DocumentNode.SelectNodesOrEmpty("//div[@class = 'item_fulltext']/h3")
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: HtmlNode) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            printfn "%s" t.InnerText
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()