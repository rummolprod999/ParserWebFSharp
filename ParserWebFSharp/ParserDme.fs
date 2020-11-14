namespace ParserWeb

open System
open TypeE
open HtmlAgilityPack

type ParserDme(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url ="https://market.dme.aero/purchase.aspx"

    override __.Parsing() =
            try
                __.ParsingPage(url)
            with ex -> Logging.Log.logger ex


    member private __.ParsingPage(url: string) =
        let Page = Download.DownloadString url
        match Page with
        | null | "" -> Logging.Log.logger ("Dont get page", url)
        | s ->
            let htmlDoc = HtmlDocument()
            htmlDoc.LoadHtml(s)
            let nav = (htmlDoc.CreateNavigator()) :?> HtmlNodeNavigator
            let tens = nav.CurrentDocument.DocumentNode.SelectNodesOrEmpty("//table[@id = 'sale']//tr[@class = 'rows']")
            for t in tens do
                    try
                        __.ParsingTender t url
                    with ex -> Logging.Log.logger ex
            ()
        ()
    
    member private __.ParsingTender (t: HtmlNode) (url: string) =
        let builder = DocumentBuilder()
        let res = builder {
            let! purName = t.GsnDocWithError "./td[1]" <| sprintf "purName not found %s %s " url (t.InnerText)
            let! purType = t.GsnDocWithError "./td[3]/a" <| sprintf "purType not found %s %s " url (t.InnerText)
            let purName = if purName = "" then purType else purName
            let! pwName = t.GsnDocWithError "./td[2]" <| sprintf "pwName not found %s %s " url (t.InnerText)
            let! cusName = t.GsnDocWithError "./td[4]/a" <| sprintf "cusName not found %s %s " url (t.InnerText)
            let! dateEndT = t.GsnDocWithError "./td[5]" <| sprintf "dateEndT not found %s %s " url (t.InnerText)
            let dateEnd = dateEndT.DateFromStringOrMin("dd.MM.yyyy")
            let! personName = t.GsnDocWithError "./td[6]" <| sprintf "personName not found %s %s " url (t.InnerText)
            let! personEmail = (t.GsnDoc "./td[7]/div[contains(., 'Почта:')]")
            let personEmail  = personEmail.Replace("Почта:", "").Trim()
            let! personPhone = (t.GsnDoc "./td[7]/div[contains(., 'телефон:')]")
            let personPhone  = personPhone.Replace("Рабочий телефон:", "").Replace("Мобильный телефон:", "").Trim()
            let tend = {  Href = url
                          PurName = purName
                          PurNum = Tools.createMD5 purName
                          CusName = cusName
                          PwName = pwName
                          PersonName = personName
                          PersonPhone = personPhone
                          PersonEmail = personEmail
                          DateEnd = dateEnd
                          DatePub = DateTime.Now}          
            let T = TenderDme(set, tend, 285, "Московский аэропорт Домодедово", "https://market.dme.aero/")
            T.Parsing()
            return ""
        }
        match res with
                | Succ _ -> ()
                | Err e when e = "" -> ()
                | Err r -> Logging.Log.logger r
        
        ()