namespace ParserWeb

open System
open System.Collections.Generic
open System.Text
open TypeE
open HtmlAgilityPack
open System.Linq

type ParserSngb(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url = "https://www.sngb.ru/tenders"

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
                nav
                    .CurrentDocument
                    .DocumentNode
                    .SelectNodesOrEmpty("//table[@class = 'table_fix']/tbody")
                    .ToList()

            tens.Reverse()

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
                    t.GsnDocWithError ".//td[contains(., 'Предмет тендера')]/following-sibling::td"
                    <| sprintf "purName not found %s %s " url (t.InnerText)

                let purNum = Tools.createMD5 (purName)
                let href = "https://www.sngb.ru/tenders"
                let datePub = DateTime.Now
                let! dateEndT = t.GsnDoc ".//td[contains(., 'Срок подачи заявок')]/following-sibling::td"

                let dateEnd =
                    match dateEndT.DateFromStringOrMin("dd.MM.yyyy") with
                    | x when x = DateTime.MinValue -> datePub.AddDays(2.)
                    | x -> x

                let delivTerm = StringBuilder()

                match t.Gsn ".//td[contains(., 'Порядок оплаты')]/following-sibling::td" with
                | x when (not <| String.IsNullOrEmpty(x)) ->
                    delivTerm
                        .Append("Порядок оплаты: ")
                        .Append(x)
                        .AppendLine()
                    |> ignore
                | _ -> ()

                match t.Gsn ".//td[contains(., 'Срок исполнения обязательства')]/following-sibling::td" with
                | x when (not <| String.IsNullOrEmpty(x)) ->
                    delivTerm
                        .Append("Срок исполнения обязательства: ")
                        .Append(x)
                        .AppendLine()
                    |> ignore
                | _ -> ()

                match t.Gsn ".//td[contains(., 'Гарантийный срок')]/following-sibling::td" with
                | x when (not <| String.IsNullOrEmpty(x)) ->
                    delivTerm
                        .Append("Гарантийный срок: ")
                        .Append(x)
                        .AppendLine()
                    |> ignore
                | _ -> ()

                match t.Gsn ".//td[contains(., 'Дополнительные требования')]/following-sibling::td" with
                | x when (not <| String.IsNullOrEmpty(x)) ->
                    delivTerm
                        .Append("Дополнительные требования: ")
                        .Append(x)
                        .AppendLine()
                    |> ignore
                | _ -> ()

                match t.Gsn ".//td[contains(., 'Дополнительная информация')]/following-sibling::td" with
                | x when (not <| String.IsNullOrEmpty(x)) ->
                    delivTerm
                        .Append("Дополнительная информация: ")
                        .Append(x)
                        .AppendLine()
                    |> ignore
                | _ -> ()

                match t.Gsn ".//td[contains(., 'Порядок оплаты')]/following-sibling::td" with
                | x when (not <| String.IsNullOrEmpty(x)) ->
                    delivTerm
                        .Append("Порядок оплаты: ")
                        .Append(x)
                        .AppendLine()
                    |> ignore
                | _ -> ()

                match t.Gsn ".//td[contains(., 'Прочее')]/following-sibling::td" with
                | x when (not <| String.IsNullOrEmpty(x)) ->
                    delivTerm
                        .Append("Прочее: ")
                        .Append(x)
                        .AppendLine()
                    |> ignore
                | _ -> ()

                let! delivPlace = t.GsnDoc ".//td[contains(., 'Место проведения работ')]/following-sibling::td"
                let docs = __.CreateDocs(t)

                let tend =
                    { Href = href
                      PurName = purName
                      PurNum = purNum
                      DelivTerm = delivTerm.ToString().Trim()
                      DocList = docs
                      DelivPlace = delivPlace
                      DatePub = datePub
                      DateEnd = dateEnd }

                let T =
                    TenderSngb(set, tend, 359, "АО БАНК «СНГБ»", "https://www.sngb.ru/")

                T.Parsing()
                return ""
            }

        match res with
        | Succ _ -> ()
        | Err e when e = "" -> ()
        | Err r -> Logging.Log.logger r

        ()

    member private __.CreateDocs(t: HtmlNode) =
        let docList = List<DocSibServ>()
        let docs = t.SelectNodes(".//a")

        match docs with
        | null -> ()
        | _ ->
            for doc in docs do
                try
                    let docName = doc.InnerText

                    let mutable docUrl =
                        doc.getAttrWithoutException ("href")

                    if not <| docUrl.StartsWith("https://www.sngb.ru") then
                        docUrl <- sprintf "%s%s" "https://www.sngb.ru" docUrl

                    if docName <> "" && docUrl <> "" then
                        let d =
                            { name = docName.RegexCutWhitespace()
                              url = docUrl }

                        docList.Add(d)
                with
                    | ex -> Logging.Log.logger ex

            ()

        docList
