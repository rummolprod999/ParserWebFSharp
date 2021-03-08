namespace ParserWeb

open System.Collections.Generic
open Newtonsoft.Json.Linq
open NewtonExt
open DocumentBuilderNewton

type ParserNeftNew(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let url = sprintf "https://apizakupki.nefteavtomatika.ru/api/purchases?page=%d&order_by\[number\]=desc&per_page=10"
    
    override this.Parsing() =
        for i in 1..20 do
            try 
                let urlT = url i
                this.ParserPage urlT
            with ex -> Logging.Log.logger ex
    
    member private this.ParserPage(urlT : string) =
        let Page = Download.DownloadString urlT
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page")
        | _ -> this.ParsingResultJson Page
        ()
    
    member private this.ParsingResultJson(res: string) =
        let j = JObject.Parse(res)
        let tenders = j.GetElements("data")
        for t in tenders do
            try
                this.ParsingTender(t)
            with ex -> Logging.Log.logger ex
        ()
    
    member private this.ParsingTender(t: JToken) =
        let builder = DocumentBuilder()
        let res =
                   builder {
                       let! id = t.StDInt "id" <| sprintf "id not found %s" (t.ToString())
                       let! purName = t.StDString "description" <| sprintf "purName not found %s" (t.ToString())
                       let! purName1 = t.StDString "title" <| sprintf "purName not found %s" (t.ToString())
                       let purName = if purName <> purName1
                                     then sprintf "%s %s" purName1 purName
                                     else purName1
                       let! orgName = t.StDString "company" <| sprintf "orgName not found %s" (t.ToString())
                       let orgContactName = GetStringFromJtoken t "author"
                       let status = GetStringFromJtoken t "status.label"
                       let! datePub = t.StDDateTime "status.start_at" <| sprintf "datePub not found %s" (t.ToString())
                       let! dateEnd = t.StDDateTime "status.end_at" <| sprintf "dateEnd not found %s" (t.ToString())
                       let docs = t.GetElements("files")
                       let DocsList = List<DocSibServ>()
                       this.AddDocs(docs, DocsList, id)
                       let tend =   { Href = sprintf "https://zakupki.nefteavtomatika.ru/purchases/%d/show" id
                                      PurNum = string id
                                      PurName = purName
                                      DatePub = datePub
                                      DateEnd = dateEnd
                                      Status = status
                                      OrgName = orgName
                                      ContactPerson = orgContactName
                                      DocList = DocsList}
                       let T = TenderNeftNew(set, tend, 55, "АО «Нефтеавтоматика»", "https://zakupki.nefteavtomatika.ru")
                       T.Parsing()
                       return ""
                       }
        match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger (r, t.ToString())
        ()
        
        member private this.AddDocs(docs, DocsList, id) =
            for d in docs do
               let idDoc = GetIntFromJtoken d "..id"
               let docUrl = sprintf "https://apizakupki.nefteavtomatika.ru/api/purchases/%d/files/%d/download" id idDoc
               let docName = GetStringFromJtoken d "..label"
               DocsList.Add({ name = docName
                              url = docUrl })
               ()