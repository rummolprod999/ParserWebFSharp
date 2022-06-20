namespace ParserWeb

open TypeE
open HtmlAgilityPack

type ParserKaustik(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://www.kaustik.ru/ru/index.php/partneram/konkursy/tekushchie-konkursy"

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
            let htmlDoc = HtmlDocument()
            htmlDoc.LoadHtml(s)

            let nav =
                (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator

            let tens =
                nav.CurrentDocument.DocumentNode.SelectNodesOrEmpty("//div[@class = 'item_fulltext']/h3")

            for t in tens do
                try
                    __.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

            ()

        ()

    member private __.ParsingTender (t: HtmlNode) (url: string) =
        let builder = DocumentBuilder()

        let res =
            builder {
                let! purName =
                    t.GsnDocWithError "."
                    <| sprintf "purName not found %s %s " url (t.InnerText)

                let! noticeVer =
                    t.GsnDocWithError "./following-sibling::p[1]"
                    <| sprintf "noticeVer not found %s %s " url (t.InnerText)

                let purNum = Tools.createMD5 purName
                let! hrefT = t.GsnAtrDoc "./following-sibling::a[1]" "href"

                let href =
                    sprintf "https://www.kaustik.ru%s" hrefT

                let attName = "Конкурсная документация"

                let! pubDateT =
                    purName.Get1Doc "^(\d{2}\.\d{2}\.\d{4})"
                    <| sprintf "datePubT not found %s %s " url (purName)

                let datePub =
                    pubDateT.DateFromStringOrMin("dd.MM.yyyy")

                let! dateEndT =
                    t.GsnDocWithError "./following-sibling::p[2]/strong"
                    <| sprintf "dateEndT not found %s %s " url (t.InnerText)

                let dateEndT =
                    dateEndT
                        .Replace("&nbsp;", " ")
                        .RegexCutWhitespace()
                        .ReplaceDate()

                let dateEndList =
                    dateEndT.Get2ListRegexp "(\d{2}:\d{2}).+(\d{2}\.\d{2}\.\d{4})"

                let dateEndS =
                    (sprintf "%s %s" dateEndList.[1] dateEndList.[0])
                        .Trim()

                let! dateEnd = dateEndS.DateFromStringDocMin("dd.MM.yyyy HH:mm")

                let tend =
                    { Href = url
                      PurName = purName
                      PurNum = purNum
                      AttachName = attName
                      AttachUrl = if hrefT = "" then url else href
                      DateEnd = dateEnd
                      DatePub = datePub
                      NoticeVer = noticeVer }

                let T =
                    TenderKaustik(set, tend, 284, "АО «КАУСТИК»", "https://www.kaustik.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
