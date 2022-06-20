namespace ParserWeb

open System.Web
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserRtCi2(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls =
        [ "https://zakupki.rt-ci.ru/interest/?PAGEN_1="
          "https://zakupki.rt-ci.ru/anounce/?PAGEN_1="
          "https://zakupki.rt-ci.ru/invitations/?PAGEN_1=" ]

    override __.Parsing() =
        for url in urls do
            for i in 20..-1..1 do
                try
                    __.ParsingPage(sprintf "%s%d" url i)
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
                nav
                    .CurrentDocument
                    .DocumentNode
                    .SelectNodesOrEmpty("//div[contains(@class, 'table__row-inner')]")
                    .ToList()
                    .Skip(1)
                    .Reverse()

            for t in tens do
                try
                    __.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

            ()

        ()

    member private __.ParsingTender (t: HtmlNode) (_: string) =
        let builder = DocumentBuilder()

        let res =
            builder {
                let! purName =
                    t.GsnDocWithError ".//div[3]"
                    <| sprintf "purName not found %s " (t.InnerText)

                let! hrefT =
                    t.GsnAtrDocWithError ".//div/a"
                    <| "href"
                    <| sprintf "hrefT not found %s " (t.InnerText)

                let href =
                    sprintf "https://zakupki.rt-ci.ru%s" hrefT

                let! cusName =
                    t.GsnDocWithError ".//div[2]"
                    <| sprintf "cusName not found %s" (t.InnerText)

                let! datePubT =
                    t.GsnDocWithError ".//div[1]"
                    <| sprintf "datePubT not found %s" (t.InnerText)

                let datePub =
                    datePubT.DateFromStringOrMin("dd.MM.yyyy")

                let dateEnd = datePub.AddDays(2.)

                let tend =
                    { Href = href
                      PurName = HttpUtility.HtmlDecode(purName)
                      PurNum = Tools.createMD5 (href)
                      CusName = cusName
                      DateEnd = dateEnd
                      DatePub = datePub }

                let T =
                    TenderRtCi2(set, tend, 295, "РТ-Комплектимпекс", "https://zakupki.rt-ci.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()
