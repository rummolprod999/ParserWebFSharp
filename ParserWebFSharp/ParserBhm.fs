namespace ParserWeb

open System
open System.Collections.Generic
open System.Globalization
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserBhm(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls = [|"http://www.oaobhm.ru/tender/"|]

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
            let tens = documents.QuerySelectorAll("div.tenders__item").ToList()
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
        
    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError "div.tenders__item-text" <| sprintf "purName not found %s %s " url (t.TextContent)
            let purName = purName.RegexCutWhitespace()
            let purNum = Tools.createMD5 purName
            let href = urls.[0]
            let! datePubT = t.GsnDocWithError "div.tenders__item-topbar > div.tenders__item-date" <| sprintf "datePubT not found %s %s " url (t.TextContent)
            let datePub = datePubT.DateFromStringOrMin("dd.MM.yyyy")
            let! dateEndT = t.GsnDocWithError "div.tenders__item-deadline" <| sprintf "dateEndT not found %s %s " url (t.TextContent)
            let dateEnd = if dateEndT.Contains("рабочих дня с даты размещения запроса") then
                                let dateEndP = dateEndT.Get1FromRegexpOrDefaul("(\d+) рабочих дня с даты размещения запроса")
                                datePub.AddDays(float (Int32.Parse(dateEndP)))
                          else
                                let dateEndP = dateEndT.Get1FromRegexpOrDefaul("(\d{2}\.\d{2}\.\d{4})")
                                DateTime.ParseExact(dateEndP, "dd.MM.yyyy", CultureInfo.CreateSpecificCulture("ru-RU"))
            let! personName = t.GsnDoc "div.tenders__item-contact > div:nth-of-type(1)"
            let! personPhone = t.GsnDoc "div.tenders__item-contact > div:nth-last-of-type(1)"
            let! personEmail = t.GsnDoc "div.tenders__item-address > div:nth-last-of-type(1)"
            let tens = t.QuerySelectorAll("div.tenders__item-text table tbody tr").ToList().Skip(1)
            let listP = __.CreateProducts(tens)
            let ten =
                { Href=href
                  PurName=purName
                  PurNum=purNum
                  PersonName=personName
                  PersonPhone=personPhone
                  PersonEmail=personEmail
                  DateEnd=dateEnd
                  DatePub=datePub
                  Products=listP}
            let T = TenderBhm(set, ten, 275, "АО «Борхиммаш»", "http://www.oaobhm.ru/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()
    
    member private __.CreateProducts (elements) =
        let listP = List<BhmProductRec>()
        for p in elements do
            let mutable quant = ""
            let mutable pName = ""
            let mutable okei = ""
            let pNameT = p.QuerySelector("td:nth-child(2) > p")
            if pNameT <> null then pName <- pNameT.TextContent.Trim()
            let okeiT = p.QuerySelector("td:nth-child(5) > p")
            if okeiT <> null then okei <- okeiT.TextContent.Trim()
            let quantT = p.QuerySelector("td:nth-child(6) > p")
            if quantT <> null then quant <- quantT.TextContent.Trim()
            let prod = {Name=pName
                        Okei=okei
                        Quantity=quant}
            listP.Add(prod)
        listP