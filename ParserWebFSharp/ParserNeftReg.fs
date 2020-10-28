namespace ParserWeb

open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserNeftReg(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let spage = "https://etp.neftregion.ru/auctions"

    override __.Parsing() =
        try
                __.ParsingPage(spage)
        with ex -> Logging.Log.logger ex
    
    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("table.tableorg tr[id ^='trauction']").ToList()
            for t in 0..tens.Count-1 do
                if t%2 = 0 then
                    try
                        __.ParsingTender tens.[t] tens.[t+1] url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: IElement) (t1: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! href = t1.GsnAtrDocWithError "a[href ^= '/view-auction']" "href" <| sprintf "href not found %s %s " url (t.TextContent)
            let href = sprintf "https://etp.neftregion.ru%s" href
            let! purName = t.GsnDocWithError "td:nth-of-type(2) > div:nth-of-type(1)" <| sprintf "purName not found %s %s " url (t.TextContent)
            let! pwName = t.GsnDocWithError "td:nth-of-type(3)" <| sprintf "pwName not found %s %s " url (t.TextContent)
            let pwName = pwName.Replace("закупка", "")
            let! purNum = t.GsnDocWithError "td:nth-of-type(1) > span:nth-of-type(1)" <| sprintf "purNum not found %s %s " url (t.TextContent)
            let! nmck = t.GsnDocWithError "td:nth-of-type(4)" <| sprintf "nmck not found %s %s " url (t.TextContent)
            let! status = t.GsnDocWithError "td:nth-of-type(6)" <| sprintf "nmck not found %s %s " url (t.TextContent)
            let nmck = nmck.GetPriceFromStringKz()
            let tend = {Href = href
                        PurName = purName
                        PurNum = purNum
                        Nmck = nmck
                        PwName = pwName
                        Status = status}
            let T = TenderNeftReg(set, tend, 232, "ЭТП мелкооптового рынка нефтепродуктов «НефтьРегион»", "https://etp.neftregion.ru/")
            T.Parsing()
            return ""}
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()