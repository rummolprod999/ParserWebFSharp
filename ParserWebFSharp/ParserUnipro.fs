namespace ParserWeb

open System
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserUnipro(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"http://unipro.energy/purchase/announcement/?PAGEN_1="|]

    override __.Parsing() =
        for url in urls do
            for i = 1 to 5 do
                let ur = sprintf "%s%d" url i
                try
                    __.ParsingPage(ur)
                with ex -> Logging.Log.logger ex
        ()


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("#table_procurement tbody tr").ToList()
            let mutable count = 0
            for t in tens do
                    if count%2 = 0 then
                        try
                            __.ParsingTender t tens.[count+1] url
                        with ex -> Logging.Log.logger ex
                    count <- count + 1
                    
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (t1: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError "td.table_title" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! href = t1.GsnAtrDocWithError "a" "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let href = sprintf "http://unipro.energy%s" href
            let! purNum = href.Get1Doc "/(\d+)/$" <| sprintf "purNum not found %s %s " href (t.TextContent)
            let! dates = t.GsnDocWithError "td.table_time" <| sprintf "dates not found %s %s " url (t.TextContent)
            let! datePubT = dates.Get1Doc "^(\d{2}\.\d{2}\.\d{2})" <| sprintf "datePubT not found %s %s " href (dates)
            let datePubT = datePubT.RegexCutWhitespace().Trim()
            let! datePub = datePubT.DateFromStringDoc ("dd.MM.yy", sprintf "datePub not found %s %s " href datePubT)
            let mutable dateEndT = dates.Get1FromRegexpOrDefaul "Продлено до (\d{2}\.\d{2}\.\d{2})"
            if dateEndT = "" then
                dateEndT <- dates.Get1FromRegexpOrDefaul "(\d{2}\.\d{2}\.\d{2} \d{2}:\d{2}:\d{2})"
            let mutable dateEnd = dateEndT.DateFromStringOrMin("dd.MM.yy")
            if dateEnd = DateTime.MinValue then
                dateEnd <- dateEndT.DateFromStringOrMin("dd.MM.yy HH:mm:ss")
            let tend = {  Href = href
                          DateEnd = dateEnd
                          DatePub = datePub
                          PurNum = purNum
                          PurName = purName}
            let T = TenderUnipro(set, tend, 254, "ПАО «Юнипро»", "http://unipro.energy/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()