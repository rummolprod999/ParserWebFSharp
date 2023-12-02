namespace ParserWeb

open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserVolgZmo(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://szvo.gov35.ru/web/gz/em_reestr?p_p_id=EM_WAR_EMportlet&p_p_lifecycle=0&p_p_state=normal&p_p_mode=view&p_p_col_id=column-1&p_p_col_count=1&_EM_WAR_EMportlet_mvcPath=%2Fview.jsp&_EM_WAR_EMportlet_purchType=open&_EM_WAR_EMportlet_purchName=&_EM_WAR_EMportlet_clientName=&_EM_WAR_EMportlet_startPrice=&_EM_WAR_EMportlet_startDatePub=&_EM_WAR_EMportlet_endDatePub=&_EM_WAR_EMportlet_okpdCode=&_EM_WAR_EMportlet_endPrice=&_EM_WAR_EMportlet_keywords=&_EM_WAR_EMportlet_advancedSearch=false&_EM_WAR_EMportlet_andOperator=true&_EM_WAR_EMportlet_resetCur=false&_EM_WAR_EMportlet_delta=75"

    override __.Parsing() =
        try
            __.ParsingPage(url)
        with
            | ex -> Logging.Log.logger ex

    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents
                    .QuerySelectorAll("table tbody tr.results-row")
                    .ToList()

            for t in tens do
                try
                    __.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

            ()

        ()

    member private __.ParsingTender (t: IElement) (url: string) =
        let builder = DocumentBuilder()

        let res =
            builder {
                let! href =
                    t.GsnAtrDocWithError "td a" "href"
                    <| sprintf "href not found %s %s " url (t.TextContent)

                let! purNum =
                    t.GsnDocWithError "td:nth-of-type(2)"
                    <| sprintf "purNum not found %s %s " url (t.TextContent)

                let! purName =
                    t.GsnDocWithError "td:nth-of-type(3)"
                    <| sprintf "purName not found %s %s " url (t.TextContent)

                let! cusName =
                    t.GsnDocWithError "td:nth-of-type(4)"
                    <| sprintf "cusName not found %s %s " url (t.TextContent)

                let! nmck =
                    t.GsnDocWithError "td:nth-of-type(7)"
                    <| sprintf "nmck not found %s %s " url (t.TextContent)

                let nmck = nmck.GetPriceFromStringKz()

                let! datePubT =
                    t.GsnDocWithError "td:nth-of-type(8)"
                    <| sprintf "datePubT not found %s %s " url (t.TextContent)

                let! datePub =
                    datePubT.DateFromStringDoc("dd.MM.yyyy", sprintf "datePub not found %s %s " href datePubT)

                let! endDateT =
                    t.GsnDocWithError "td:nth-of-type(9)"
                    <| sprintf "endDateT not found %s %s " url (t.TextContent)

                let! dateEnd =
                    endDateT.DateFromStringDoc("dd.MM.yyyy HH:mm", sprintf "dateEnd not found %s %s " href endDateT)

                let! status =
                    t.GsnAtrDoc "td:nth-of-type(10)"
                    <| sprintf "status not found %s %s " url (t.TextContent)

                let tend =
                    { Href = href
                      DateEnd = dateEnd
                      DatePub = datePub
                      PurNum = purNum
                      PurName = purName
                      CusName = cusName
                      Nmck = nmck
                      Status = status }

                let T =
                    TenderVolgZmo(
                        set,
                        tend,
                        234,
                        "Комитет Государственного заказа Вологодской области",
                        "https://szvo.gov35.ru/"
                    )

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
