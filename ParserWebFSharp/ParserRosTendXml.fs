namespace ParserWeb

open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System
open System.IO
open System.Linq
open System.Text
open System.Threading
open System.Xml

type ParserRosTendXml(stn : Settings.T) =
    inherit Parser()
    let set = stn
    
    override this.Parsing() =
        let dt = DateTime.Now.AddDays(-1.)
        let export = String.Format("export-{0:yyyy}-{0:MM}-{0:dd}-00-00-23-59", dt)
        let url = String.Format("http://export.rostender.info/notifications/{0}.tar.gz", export)
        let patharch = String.Format("{0}{1}{2}.tar.gz", stn.TempPathTenders, Path.DirectorySeparatorChar, export)
        let file = Download.DownloadFileSimple url patharch
        match file with
        | null -> ()
        | _ -> 
            Tools.UnzippedTargz patharch stn.TempPathTenders
            let file = String.Format("{0}{1}{2}.xml", stn.TempPathTenders, Path.DirectorySeparatorChar, export)
            try 
                this.ParsingFile file
            with ex -> Logging.Log.logger ex
        ()
    
    member private this.ParsingFile(f : string) =
        let fileInf = FileInfo(f)
        match fileInf.Exists with
        | false -> ()
        | true -> 
            use sr = new StreamReader(f, Encoding.Default)
            let ftext = sr.ReadToEnd()
            let doc = XmlDocument()
            doc.LoadXml(ftext)
            let jsons = JsonConvert.SerializeXmlNode(doc)
            let json = JObject.Parse(jsons)
            try 
                this.ParsingJson json
            with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private this.ParsingJson(json : JObject) =
        let tenders =
            query { 
                for t in json.["export"].["tenders"].["tender"] do
                    select t
            }
        for tt in tenders do
            try 
                this.ParserTender tt
            with ex -> Logging.Log.logger ex
        ()
    
    member private this.ParserTender(token : JToken) =
        let PurNum = (Tools.teststring <| (token.SelectToken("tenderNumber")))
        match PurNum with
        | "" -> raise <| NullReferenceException(sprintf "PurNum not found in %s" "")
        | _ -> ()
        let Href = (Tools.teststring <| (token.SelectToken("href")))
        match Href with
        | "" -> raise <| NullReferenceException(sprintf "Href not found in %s" PurNum)
        | _ -> ()
        let PurName = (Tools.teststring <| (token.SelectToken("name")))
        match PurName with
        | "" -> raise <| NullReferenceException(sprintf "PurName not found in %s" PurNum)
        | _ -> ()
        (*let mutable verNum = (Tools.teststring <| (token.SelectToken("versionNumber")))
        match verNum with
        | "" -> verNum <- "1"
        | _ -> ()*)
        let _ = ""
        let PubDateT = JsonConvert.SerializeObject(token.SelectToken("publishDate"))
        match PubDateT with
        | null -> raise <| NullReferenceException(sprintf "PubDate not found in %s" Href)
        | _ -> ()
        let mutable pubDate = Tools.testdate PubDateT
        match pubDate with
        | a when a = DateTime.MinValue -> 
            raise <| NullReferenceException(sprintf "PubDate not found in %s" PubDateT)
        | _ -> pubDate <- pubDate.AddHours(1.)
        let EndDateT = JsonConvert.SerializeObject(token.SelectToken("endDate"))
        let mutable endDate = Tools.testdate EndDateT
        if endDate <> DateTime.MinValue then endDate <- endDate.AddHours(1.)
        let Nmck = (Tools.teststring <| (token.SelectToken("price"))).Replace(",", ".")
        let delivPlace = (Tools.teststring <| (token.SelectToken("place")))
        let regionT = token.SelectTokens("..regionName")
        let mutable region = ""
        region <- (Tools.teststring <| regionT.FirstOrDefault())
        let tn =
            { Href = Href
              PurNum = PurNum
              PurName = PurName
              DatePub = pubDate
              Region = region
              Nmck = Nmck
              DelivPlace = delivPlace
              Currency = ""
              Page = "" }
        try 
            let T = TenderRosTend(set, tn, 82, "ООО Тендеры и закупки", "http://rostender.info", endDate, "")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, tn.Href)
        Thread.Sleep(3)
        ()
