namespace ParserWeb

open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserRusal(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls =
        [| "https://rusal.ru/suppliers/selection/zhd-avia-avto-konteynernye-perevozki/"
           "https://rusal.ru/suppliers/selection/materialno-tekhnicheskie-resursy/"
           "https://rusal.ru/suppliers/selection/perevalka-i-ekspedirovanie-gruzov-v-morskikh-portakh/"
           "https://rusal.ru/suppliers/selection/prochie-uslugi/"
           "https://rusal.ru/suppliers/selection/stroitelno-montazhnye-i-remontnye-raboty/"
           "https://rusal.ru/suppliers/selection/frakht/" |]

    override __.Parsing() =
        for url in urls do
            try
                __.ParsingPage(url)
            with
                | ex -> Logging.Log.logger ex

        ()


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
                    .QuerySelectorAll("tr.js-pagination-item")
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
                let href = t.GetAttribute("data-href-blank")
                let href = sprintf "https://rusal.ru%s" href

                let! purName =
                    t.GsnDocWithError "td:nth-of-type(4)"
                    <| sprintf "purName not found %s %s " url (t.TextContent)

                let! purNum =
                    t.GsnDocWithError "td:nth-of-type(1)"
                    <| sprintf "purNum not found %s %s " url (t.TextContent)

                let! datePubT =
                    t.GsnDocWithError "td:nth-of-type(2)"
                    <| sprintf "datePubT not found %s %s " url (t.TextContent)

                let! datePub =
                    datePubT.DateFromStringDoc("dd.MM.yyyy", sprintf "datePub not found %s %s " href datePubT)

                let! endDateT =
                    t.GsnDocWithError "td:nth-of-type(3)"
                    <| sprintf "endDateT not found %s %s " url (t.TextContent)

                let! dateEnd =
                    endDateT.DateFromStringDoc("dd.MM.yyyy", sprintf "dateEnd not found %s %s " href endDateT)

                let tend =
                    { RusalRec.Href = href
                      DateEnd = dateEnd
                      DatePub = datePub
                      PurNum = purNum
                      PurName = purName }

                let T =
                    TenderRusal(set, tend, 236, "ОК РУСАЛ", "https://rusal.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
