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

type ParserRusal(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"https://rusal.ru/suppliers/selection/build/build_list/?mode=curr"; "https://rusal.ru/suppliers/selection/freight/freight_list/?mode=curr"; "https://rusal.ru/suppliers/selection/transport/transport_list/?mode=curr"; "https://rusal.ru/suppliers/selection/ports/ports_list/?mode=curr"; "https://rusal.ru/suppliers/selection/other/other_list/?mode=curr"; "https://rusal.ru/suppliers/selection/mtr/mtr_list/?mode=curr"|]

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
            let tens = documents.QuerySelectorAll("table.rgMasterTable tr[id ^= 'bx_']").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! href = t.GsnAtrDocWithError "td a" "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let href = sprintf "https://rusal.ru%s" href
            let! purName = t.GsnDocWithError "td a" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! purNum = t.GsnDocWithError "td:nth-of-type(2)" <| sprintf "purNum not found %s %s " url (t.TextContent)
            let! datePubT = t.GsnDocWithError "td:nth-of-type(1)" <| sprintf "datePubT not found %s %s " url (t.TextContent)
            let! datePub = datePubT.DateFromStringDoc ("dd.MM.yyyy", sprintf "datePub not found %s %s " href datePubT)
            let! endDateT = t.GsnDocWithError "td:nth-of-type(6)" <| sprintf "endDateT not found %s %s " url (t.TextContent)
            let! dateEnd = endDateT.DateFromStringDoc ("dd.MM.yyyy", sprintf "dateEnd not found %s %s " href endDateT)
            let tend = {  Href = href
                          DateEnd = dateEnd
                          DatePub = datePub
                          PurNum = purNum
                          PurName = purName}
            let T = TenderRusal(set, tend, 236, "ОК РУСАЛ", "https://rusal.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()
