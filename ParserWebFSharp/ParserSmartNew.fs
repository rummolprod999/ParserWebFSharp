namespace ParserWeb

open System
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Web
open Newtonsoft.Json.Linq
open NewtonExt
open DocumentBuilderNewton

type ParserSmartNew(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 20

    override __.Parsing() =
        for i in 1..pageC do
            try
                __.ParsingPage(i)
            with ex -> Logging.Log.logger ex
    
    member private __.ParsingPage(page: int) =
        let Page = Download.DownloadSmartTender page
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page")
        | s -> __.ParsingResultJson Page
        ()
    
    member private __.ParsingResultJson(res: string) =
        let j = JObject.Parse(res)
        let tenders = j.GetElements("Tenders")
        for t in tenders do
            try
                __.ParsingTender(t)
            with ex -> Logging.Log.logger ex
        ()
    
    member private __.ParsingTender(t: JToken) =
        let builder = DocumentBuilder()
        let res =
                   builder {
                       let! id = t.StDInt "Id" <| sprintf "id not found %s" (t.ToString())
                       let! purNum = t.StDString "Number" <| sprintf "purNum not found %s" (t.ToString())
                       let! purName = t.StDString "Subject" <| sprintf "purName not found %s" (t.ToString())
                       let! nmck = t.StDString "InitialRate.Amount" <| sprintf "nmck not found %s" (t.ToString())
                       let! orgName = t.StDString "Organizer.Title" <| sprintf "orgName not found %s" (t.ToString())
                       let orgContactName = GetStringFromJtoken t "Organizer.Contact.Name"
                       let orgContactEmail = GetStringFromJtoken t "Organizer.Contact.Email"
                       let orgContactPhone = GetStringFromJtoken t "Organizer.Contact.Phone"
                       let! datePub = t.StDDateTime "TenderingPeriod.DateStart" <| sprintf "datePub not found %s" (t.ToString())
                       let! dateEnd = t.StDDateTime "TenderingPeriod.DateEnd" <| sprintf "dateEnd not found %s" (t.ToString())
                       let! status = t.StDString "StatusInfo.Title" <| sprintf "status not found %s" (t.ToString())
                       let! pwName = t.StDString "BiddingTypeInfo.Title" <| sprintf "pwName not found %s" (t.ToString())
                       let tend = { Id = id
                                    PurNum = purNum
                                    PurName = purName
                                    Nmck = nmck
                                    OrgName = orgName
                                    OrgContactName = orgContactName
                                    OrgContactEmail = orgContactEmail
                                    OrgContactPhone = orgContactPhone
                                    DatePub = datePub
                                    DateEnd = dateEnd
                                    status = status
                                    PwayName = pwName }
                       let T = TenderSmartNew(set, tend, 194, "SmartTender", "https://smarttender.biz/")
                       T.Parsing()
                       return ""
                   }
        match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger (r, t.ToString())
        ()