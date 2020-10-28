namespace ParserWeb

open System
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Web

type ParserSmart(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 20
    let spage = "https://smarttender.biz/komertsiyni-torgy/?p="

    override __.Parsing() =
        for i in 1..pageC do
            try
                let url = sprintf "%s%d" spage i
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex

    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("#tenders > tbody > tr.head")
            for t in tens do
                try
                    __.ParsingTender t url
                with ex -> Logging.Log.logger ex
            ()
        ()

    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
                       let! href = t.GsnAtrDocWithError "a.linkSubjTrading" "href" <| sprintf "href not found %s %s " url (t.TextContent)
                       let! purName = t.GsnDocWithError "a.linkSubjTrading" <| sprintf "purName not found %s %s " url (t.TextContent)
                       let! purNum = t.GsnDocWithError "td.col1 > span" <| sprintf "purNum not found %s %s " url (t.TextContent)
                       let! orgName = t.GsnDocWithError "td.col3 a.organizer-popover" <| sprintf "orgName not found %s %s " url (t.TextContent)
                       let! pwName = t.GsnDoc "td.col5"
                       let! status = t.GsnDoc "td.col5 + td.col5"
                       let! dateEndT = t.GsnDocWithError "td.col6 > div" <| sprintf "dateEndT not found %s %s " url (t.TextContent)
                       let! dateEndS = dateEndT.Get1OptionalDoc("(\d{2}\.\d{2}\.\d{4}.+\d{2}:\d{2})")
                       let dateEndS = HttpUtility.HtmlDecode(dateEndS)
                       let dateEndS = dateEndS.RegexCutWhitespace()
                       let! dateEnd = dateEndS.DateFromStringDoc ("dd.MM.yyyy HH:mm", sprintf "dateEndT not found %s %s " href dateEndT)
                       let tend = { Href = href
                                    PurNum = purNum
                                    PurName = purName
                                    OrgName = orgName
                                    DatePub = DateTime.Now
                                    DateEnd = dateEnd
                                    status = status
                                    PwayName = pwName }
                       let T = TenderSmart(set, tend, 194, "SmartTender", "https://smarttender.biz/")
                       T.Parsing()
                       return ""
                   }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        ()
