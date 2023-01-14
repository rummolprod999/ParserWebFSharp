namespace ParserWeb

open Newtonsoft.Json.Linq
open TypeE
open AngleSharp.Dom
open NewtonExt
open Newtonsoft.Json.Linq
open AngleSharp.Parser.Html
open System.Linq
open DocumentBuilderNewton

type ParserGmt(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let urls =
        [| "https://gmt.gazprom.ru/IMP-tenders?p=0&type=active&is_days_homepage=true" |]

    override __.Parsing() =
        for url in urls do
            try
                __.ParsingPage(url)
            with
                | ex -> Logging.Log.logger ex

        ()


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadStringRts url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let j = JObject.Parse(Page)
            let tenders = j.GetElements("content.main.tenders")

            for t in tenders do
                try
                    __.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

            ()

        ()

    member private __.ParsingTender (t: JToken) (url: string) =
        let builder = DocumentBuilder()

        let res =
            builder {
                let! purName =
                    t.StDString "description"
                    <| sprintf "purName not found %s" (t.ToString())

                let! href =
                    t.StDString "href"
                    <| sprintf "href not found %s" (t.ToString())

                let href =
                    sprintf "https://gmt.gazprom.ru%s" href

                let! purNum =
                    t.StDString "name"
                    <| sprintf "purNum not found %s" (t.ToString())

                let purNum = purNum.Replace("№", "").Trim()

                let! pwName =
                     t.StDStringOrEmpty "tenderType.name"
                    <| sprintf "pwName not found %s" (t.ToString())

                let! datePubTY =
                    t.StDString "dateStart.year"
                    <| sprintf "dateStart.year not found %s" (t.ToString())
                
                let! datePubTM =
                    t.StDString "dateStart.month"
                    <| sprintf "dateStart.month not found %s" (t.ToString())
                let! datePubTD =
                    t.StDString "dateStart.day"
                    <| sprintf "dateStart.day not found %s" (t.ToString())
                let! datePubTH =
                    t.StDString "dateStart.hour"
                    <| sprintf "dateStart.hour not found %s" (t.ToString())
                let! datePubTMin =
                    t.StDString "dateStart._minute"
                    <| sprintf "dateStart._minute not found %s" (t.ToString())
                let! datePubTSec =
                    t.StDString "dateStart._second"
                    <| sprintf "dateStart._second not found %s" (t.ToString())
                let datePubT = sprintf "%s.%s.%s %s:%s:%s" datePubTD datePubTM datePubTY datePubTH datePubTMin datePubTSec

                let datePub =
                    datePubT.DateFromStringOrMin("d.M.yyyy H:m:ss")

                let! dateEndTY =
                    t.StDString "dateEnd.year"
                    <| sprintf "dateEnd.year not found %s" (t.ToString())
                
                let! dateEndTM =
                    t.StDString "dateEnd.month"
                    <| sprintf "dateStart.month not found %s" (t.ToString())
                let! dateEndTD =
                    t.StDString "dateEnd.day"
                    <| sprintf "dateEnd.day not found %s" (t.ToString())
                let! dateEndTH =
                    t.StDString "dateEnd.hour"
                    <| sprintf "dateEnd.hour not found %s" (t.ToString())
                let! dateEndTMin =
                    t.StDString "dateEnd._minute"
                    <| sprintf "dateEnd._minute not found %s" (t.ToString())
                let! dateEndTSec =
                    t.StDString "dateEnd._second"
                    <| sprintf "dateEnd._second not found %s" (t.ToString())
                let dateEndT = sprintf "%s.%s.%s %s:%s:%s" dateEndTD dateEndTM dateEndTY dateEndTH dateEndTMin dateEndTSec

                let dateEnd =
                    dateEndT.DateFromStringOrMin("d.M.yyyy H:m:ss")

                let tend =
                    { GmtRec.Href = href
                      DateEnd = dateEnd
                      DatePub = datePub
                      PurNum = purNum
                      PurName = purName
                      PwName = pwName }

                let T =
                    TenderGmt(set, tend, 252, "ООО «Газпром газомоторное топливо»", "http://gazprom-gmt.ru")

                T.Parsing()
                return ""
            }

        match res with
        | Success _ -> ()
        | Error e when e = "" -> ()
        | Error r -> Logging.Log.logger r

        ()
