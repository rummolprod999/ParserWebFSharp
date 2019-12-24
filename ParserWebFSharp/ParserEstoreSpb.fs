namespace ParserWeb

open System
open System.Linq.Expressions
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Web

type ParserEstoreSpb(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 50
    let spage = "https://estore.gz-spb.ru/electronicshop/catalog/procedure/index/?page="

    override __.Parsing() =
        for i in pageC .. -1 .. 1 do
            try
                let url = sprintf "%s%d" spage i
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex

    member private __.ParsingPage(url: string) =
         let Page = Download.DownloadString url
         match Page with
            | null | "" -> Logging.Log.logger ("Dont get page", url)
            | s ->
                let parser = new HtmlParser()
                let documents = parser.Parse(s)
                let tens = documents.QuerySelectorAll("table > tbody > tr[id ^= 'rowId']")
                for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
                ()
         ()
    
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! href = t.GsnAtrDocWithError "td:nth-of-type(1) > a" "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let href = sprintf "https://estore.gz-spb.ru%s" href
            let! purName = t.GsnDocWithError "td:nth-of-type(1) > a" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! purNum = href.Get1Doc "view/(\d+)/" <| sprintf "purNum not found %s %s " url (t.TextContent)
            let! orgName = t.GsnDocWithError "td:nth-of-type(2)" <| sprintf "orgName not found %s %s " url (t.TextContent)
            let! dateEndT = t.GsnDocWithError "td:nth-of-type(4)" <| sprintf "dateEndT not found %s %s " url (t.TextContent)
            let! dateEnd = dateEndT.DateFromStringDoc ("HH:mm dd.MM.yyyy", sprintf "dateEnd not found %s %s " href dateEndT)
            let! datePubT = t.GsnDocWithError "td:nth-of-type(3)" <| sprintf "datePubT not found %s %s " url (t.TextContent)
            let! datePub = datePubT.DateFromStringDoc ("HH:mm dd.MM.yyyy", sprintf "datePub not found %s %s " href datePubT)
            let! nmck = t.GsnDocWithError "td:nth-of-type(5)" <| sprintf "nmck not found %s %s " url (t.TextContent)
            let nmck = nmck.GetPriceFromStringKz()
            let! status = t.GsnDocWithError "td:nth-of-type(6)" <| sprintf "status not found %s %s " url (t.TextContent)
            let tend = {Href = href
                        DateEnd = dateEnd
                        DatePub = datePub
                        PurName = purName
                        PurNum = purNum
                        OrgName = orgName
                        Status = status
                        Nmck = nmck }
            let T = TenderEstoreSpb(set, tend, 230, "Комитет по государственному заказу Санкт-Петербурга АИС ГЗ", "https://estore.gz-spb.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()