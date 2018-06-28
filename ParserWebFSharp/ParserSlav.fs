namespace ParserWeb

open AngleSharp
open AngleSharp.Dom
open AngleSharp.Parser.Html
open OpenQA.Selenium
open System
open System.Linq
open TypeE

type ParserSlav(stn : Settings.T) =
    inherit Parser()
    let set = stn
    
    override this.Parsing() =
        try 
            let url = "http://sn-mng.ru/zakupki-i-realizatsiya/zakupki/"
            this.ParserMegion url
        with ex -> Logging.Log.logger ex
        try 
            let url = "http://www.refinery.yaroslavl.su/index.php?module=tend"
            this.ParserYanos url
        with ex -> Logging.Log.logger ex
        try 
            let url = "http://www.slavneft.ru/supplier/procurement/"
            this.ParserNgre url
        with ex -> Logging.Log.logger ex
    
    member private this.ParserMegion(url : string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get start page", url)
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("tr.zakupki-item")
            for t in tens do
                try 
                    this.ParsingTenderMegion t url
                with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private this.ParsingTenderMegion (t : IElement) (url : string) =
        let HrefDocT =
            match t.QuerySelector("a.file-link") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        match HrefDocT with
        | "" | null -> raise <| System.NullReferenceException(sprintf "HrefDocT not found in %s" url)
        | _ -> ()
        let HrefDoc = sprintf "http://sn-mng.ru%s" HrefDocT
        
        let NameDoc =
            match t.QuerySelector("a.file-link") with
            | null -> raise <| System.NullReferenceException(sprintf "NameDoc not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let PurName =
            match t.QuerySelector("div.description") with
            | null -> raise <| System.NullReferenceException(sprintf "PurName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let purNum = NameDoc.Replace("ПДО №", "").Trim()
        
        let CusName =
            match t.QuerySelector("td:nth-child(2)") with
            | null -> raise <| System.NullReferenceException(sprintf "CusName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let OrgName =
            match t.QuerySelector("td:nth-child(3)") with
            | null -> raise <| System.NullReferenceException(sprintf "OrgName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let PubDateT =
            match t.QuerySelector("td:nth-child(4)") with
            | null -> raise <| System.NullReferenceException(sprintf "PubDateT not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let datePub =
            match PubDateT.DateFromString("dd.MM.yy") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "can not parse datePub %s" PubDateT)
        
        let EndDateT =
            match t.QuerySelector("td:nth-child(5)") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let dateEnd =
            match EndDateT.DateFromString("dd.MM.yy") with
            | Some d -> d
            | None -> DateTime.MinValue
        
        let status =
            match t.QuerySelector("td:nth-child(6)") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let ten =
            { HrefDoc = HrefDoc
              HrefName = NameDoc
              PurNum = purNum
              PurName = PurName
              OrgName = OrgName
              CusName = CusName
              DatePub = datePub
              DateEnd = dateEnd
              status = status
              typeT = MEGION }
        
        try 
            let T = TenderSlav(set, ten, 56, "ОАО «Славнефть-Мегионнефтегаз»", url)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
        ()
    
    member private this.ParserYanos(url : string) =
        let Page = Download.DownloadString1251 url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get start page", url)
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("div.tenderspage-preview")
            for t in tens do
                try 
                    this.ParsingTenderYanos t url
                with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private this.ParsingTenderYanos (t : IElement) (url : string) =
        let HrefDocT =
            match t.QuerySelector("p.cost-purchase a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        match HrefDocT with
        | "" | null -> raise <| System.NullReferenceException(sprintf "HrefDocT not found in %s" url)
        | _ -> ()
        let HrefDoc = sprintf "http://www.refinery.yaroslavl.su%s" HrefDocT
        
        let NameDoc =
            match t.QuerySelector("p.cost-purchase a") with
            | null -> raise <| System.NullReferenceException(sprintf "NameDoc not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let PurName = NameDoc
        
        let purNum =
            match PurName.Get1FromRegexp @"(\d{3}-\D{2}-\d{4})" with
            | Some x -> x
            | None -> raise <| System.NullReferenceException(sprintf "purNum not found in %s %s" url PurName)
        
        let CusName =
            match t.QuerySelector("p:contains('Заказчик тендера:') b") with
            | null -> raise <| System.NullReferenceException(sprintf "CusName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let OrgName =
            match t.QuerySelector("p:contains('Предприятие — организатор:') b") with
            | null -> raise <| System.NullReferenceException(sprintf "OrgName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let PubDateT =
            match t.QuerySelector("p:contains('Опубликованно:') b") with
            | null -> raise <| System.NullReferenceException(sprintf "PubDateT not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let datePub =
            match PubDateT.DateFromString("dd.MM.yyyy") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "can not parse datePub %s" PubDateT)
        
        let EndDateT =
            match t.QuerySelector("p:contains('Дата окончания приема оферт:') b") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let dateEnd =
            match EndDateT.DateFromString("dd.MM.yyyy") with
            | Some d -> d
            | None -> DateTime.MinValue
        
        let status =
            match t.QuerySelector("p:contains('Состояние:') b") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let ten =
            { HrefDoc = HrefDoc
              HrefName = NameDoc
              PurNum = purNum
              PurName = PurName
              OrgName = OrgName
              CusName = CusName
              DatePub = datePub
              DateEnd = dateEnd
              status = status
              typeT = YANOS }
        
        try 
            let T = TenderSlav(set, ten, 57, "ОАО «Славнефть-ЯНОС»", url)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
        ()
    
    member private this.ParserNgre(url : string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get start page", url)
        | s -> 
            let parser = new HtmlParser()
            let documents = parser.Parse(s)
            let tens = documents.QuerySelectorAll("table.contest tr:not(th)")
            for t in tens do
                try 
                    this.ParsingTenderNgre t url
                with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private this.ParsingTenderNgre (t : IElement) (url : string) =
        let HrefDocT =
            match t.QuerySelector("td:nth-child(1) a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()
        match HrefDocT with
        | "" | null -> raise <| System.NullReferenceException(sprintf "HrefDocT not found in %s" url)
        | _ -> ()
        let HrefDoc = sprintf "http://www.slavneft.ru%s" HrefDocT
        
        let NameDoc =
            match t.QuerySelector("td:nth-child(1) a") with
            | null -> raise <| System.NullReferenceException(sprintf "NameDoc not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let PurName =
            match t.QuerySelector("td:nth-child(1) a") with
            | null -> raise <| System.NullReferenceException(sprintf "PurName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let purNum =
            match PurName.Get1FromRegexp @"ПДО №?\s*(.+)" with
            | Some x -> x
            | None -> raise <| System.NullReferenceException(sprintf "purNum not found in %s %s" url PurName)
        
        let CusName =
            match t.QuerySelector("td:nth-child(2)") with
            | null -> raise <| System.NullReferenceException(sprintf "CusName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let OrgName =
            match t.QuerySelector("td:nth-child(3)") with
            | null -> raise <| System.NullReferenceException(sprintf "OrgName not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let PubDateT =
            match t.QuerySelector("td:nth-child(4)") with
            | null -> raise <| System.NullReferenceException(sprintf "PubDateT not found in %s" url)
            | ur -> ur.TextContent.Trim()
        
        let datePub =
            match PubDateT.DateFromString("dd.MM.yyyy") with
            | Some d -> d
            | None -> raise <| System.Exception(sprintf "can not parse datePub %s" PubDateT)
        
        let EndDateT =
            match t.QuerySelector("td:nth-child(5)") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let dateEnd =
            match EndDateT.DateFromString("dd.MM.yyyy") with
            | Some d -> d
            | None -> DateTime.MinValue
        
        let status =
            match t.QuerySelector("td:nth-child(6)") with
            | null -> ""
            | ur -> ur.TextContent.Trim()
        
        let ten =
            { HrefDoc = HrefDoc
              HrefName = NameDoc
              PurNum = purNum
              PurName = PurName
              OrgName = OrgName
              CusName = CusName
              DatePub = datePub
              DateEnd = dateEnd
              status = status
              typeT = NGRE }
        
        try 
            let T = TenderSlav(set, ten, 58, " ООО «Байкитская НГРЭ»", url)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
        ()
