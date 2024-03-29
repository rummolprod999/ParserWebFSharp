namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html

type ParserAkd(stn: Settings.T) =
    inherit Parser()
    let _ = stn

    let url =
        "http://www.a-k-d.ru/page/torg_list_buy?page="

    override this.Parsing() =
        for i = 1 to 5 do
            let urlP = sprintf "%s%d" url i

            try
                this.ParserPage(urlP)
            with
                | ex -> Logging.Log.logger (ex, urlP)

        ()

    member private this.ParserPage(page: string) =
        let Page = Download.DownloadString page

        match Page with
        | null
        | "" -> Logging.Log.logger ("Don't get page", page)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents.QuerySelectorAll("div.list-items div.tender-item")

            for t in tens do
                try
                    this.ParsingTender t
                with
                    | ex -> Logging.Log.logger ex

            let tens =
                documents.QuerySelectorAll("div.children-tender-rows div.children-fields")

            for t in tens do
                try
                    this.ParsingTender t
                with
                    | ex -> Logging.Log.logger ex

            ()

    member private this.ParsingTender(t: IElement) =
        let urlT =
            match t.QuerySelector("div.tender-number a") with
            | null -> ("", "")
            | ur -> (ur.GetAttribute("href").Trim(), ur.TextContent.Trim())

        match urlT with
        | (_, "")
        | ("", _) -> Logging.Log.logger ("cannot find href or purNum on page ", url)
        | urlTn, purNum ->
            let urlTen =
                sprintf "http://www.a-k-d.ru%s" urlTn

            try
                let T = TenderAkd(stn, urlTen, purNum)
                T.Parsing()
            with
                | ex -> Logging.Log.logger (ex, urlTen)

        ()
