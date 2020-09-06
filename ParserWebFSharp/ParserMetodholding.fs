namespace ParserWeb

open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq
open System.Collections.Generic

type ParserMetodholding(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"http://metholding.com/partners/purchase/SRM/status/"|]

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
            let tens = documents.QuerySelectorAll("td > a[onclick^='send(this,']").ToList()
            for t in tens do
                    try
                        __.ParsingSection t url
                    with ex -> Logging.Log.logger ex
            ()
    
    member private __.ParsingSection (t: IElement) (url: string) =
        let attr = t.GetAttribute("onclick")
        match attr.Get2FromRegexp("send\(this,'(.+?)','(.+?)'\)") with
        | None -> ()
        | Some(org, section) -> __.ParsingOrg org section url
        
        ()
    
    
    member private __.ParsingOrg org section url =
        let parametrs = new Dictionary<string, string>()
        parametrs.Add("SENDITEM", org)
        parametrs.Add("SENDVALUE", section)
        let Page = Download.DownloadPost(parametrs, url)
        let parser = new HtmlParser()
        let documents = parser.Parse(Page)
        let tens = documents.QuerySelectorAll("table.info_table tbody tr").ToList().Skip(1)
        for t in tens do
                    try
                        __.ParsingTender t url section
                    with ex -> Logging.Log.logger ex
        ()
    
    member private __.ParsingTender (t: IElement) (url: string) (section: string)=
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError "td:nth-child(2) > a" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! purNum = t.GsnDocWithError "td:nth-child(1)" <| sprintf "purNum not found %s %s " url (t.TextContent)
            let! attr = t.GsnAtrDocWithError "td:nth-child(2) > a" "onclick" <| sprintf "attr not found %s %s " url (t.TextContent)
            let mutable attr1 = ""
            let mutable attr2 = ""
            match attr.Get2FromRegexp("send\(this,'(.+?)','(.+?)'\)") with
                    | None -> ()
                    | Some(org, num) -> attr1 <- org
                                        attr2 <- num
            if attr1 = "" || attr2 = "" then return! Err "attrs not found"
            let! datePubT = t.GsnDocWithError "td:nth-child(3)" <| sprintf "datePubT not found %s %s " url (t.TextContent)
            let datePub = datePubT.DateFromStringOrMin("dd.MM.yyyy")
            let! dateEndT = t.GsnDocWithError "td:nth-child(4)" <| sprintf "dateEndT not found %s %s " url (t.TextContent)
            let dateEnd = dateEndT.DateFromStringOrMin("dd.MM.yyyy")
            let tend = {  Href = url
                          PurName = purName
                          PurNum = purNum
                          CusName = section
                          DateEnd = dateEnd
                          DatePub = datePub
                          Attr = attr1}          
            let T = TenderMetodholding(set, tend, 270, "ПМХ", "http://metholding.com/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()

