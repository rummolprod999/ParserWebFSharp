namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq
open TypeE

type ParserDomRu(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"https://zakupki.domru.ru/"|]

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
            let tens = documents.QuerySelectorAll("table.contractor_search_table tr[id^='For_']").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError "td:nth-child(2) a" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! href = t.GsnAtrDocWithError "td:nth-child(2) a" "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let href = sprintf "https://zakupki.domru.ru%s" href
            let! pwName = t.GsnDocWithError "td:nth-child(1)" <| sprintf "pwName not found %s %s " url (t.TextContent)
            let! nmck = t.GsnDoc "td:nth-child(3)" 
            let nmck = nmck.GetPriceFromString @"([\d,.   \s]+)"
            let! datePubT = t.GsnDocWithError "td:nth-child(4)" <| sprintf "datePubT not found %s %s " url (t.TextContent)
            let datePubT = datePubT.RegexReplace().ReplaceDateAriba()
            let datePub = datePubT.DateFromStringOrMin("dd.MM.yyyy")
            let! dateEndT = t.GsnDocWithError "td:nth-child(5)" <| sprintf "dateEndT not found %s %s " url (t.TextContent)
            let dateEndT = dateEndT.RegexReplace().ReplaceDateAriba()
            let dateEnd = dateEndT.DateFromStringOrMin("dd.MM.yyyy HH:mm")
            let ten =
                { Href=href
                  PurNum = "Портал закупок Дом.ru"
                  PurName=purName
                  DateEnd=dateEnd
                  DatePub=datePub
                  PwName=pwName
                  Nmck=nmck}
            let T = TenderDomRu(set, ten, 280, "Портал закупок Дом.ru", "https://zakupki.domru.ru/")
            T.Parsing()
            printfn "%A" ten
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()