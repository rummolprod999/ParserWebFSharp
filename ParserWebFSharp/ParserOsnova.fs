namespace ParserWeb

open System
open System.Web
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
            printfn "%O" t.InnerText
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()