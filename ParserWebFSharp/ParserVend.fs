namespace ParserWeb

open System
open System.Collections.Generic
open System.Linq
open System.Threading
open System.Text.RegularExpressions
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserVend(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "http://vendorportal.ru/Market/OfferInvitationOpen"
    let listTenders = new List<VendRec>()
    let options = ChromeOptions()
    
    do 
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
    
    override this.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        try 
            try 
                this.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()
    
    member private this.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//div[contains(@class, 'pagination-container')]/following-sibling::div[@class = 'divList']")).Displayed) |> ignore
        this.ParserListTenders driver
        this.GetNextPage driver wait
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : VendRec) =
        try 
            let T = TenderVend(set, t, 116, "Портал поставщиков Южного Урала", "http://vendorportal.ru/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.GetNextPage (driver : ChromeDriver) (wait : WebDriverWait) =
        for i in 1..20 do
            try 
                driver.SwitchTo().DefaultContent() |> ignore
                this.Clicker driver <| "//li[@class = 'PagedList-skipToNext']/a"
                Thread.Sleep(5000)
                driver.SwitchTo().DefaultContent() |> ignore
                wait.Until
                    (fun dr -> 
                    dr.FindElement(By.XPath("//div[contains(@class, 'pagination-container')]/following-sibling::div[@class = 'divList']")).Displayed) |> ignore
                this.ParserListTenders driver
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        //driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
        let mutable statement = true
        let mutable count = 3
        while statement && count > 0 do
            driver.SwitchTo().DefaultContent() |> ignore
            let tenders =
                driver.FindElementsByXPath
                    ("//div[contains(@class, 'pagination-container')]/following-sibling::div[@class = 'divList']")
            for t in tenders do
                try 
                    this.ParserTenders driver t
                with
                    | :? OpenQA.Selenium.StaleElementReferenceException -> count <- count-1
                                                                           if count = 0 then Logging.Log.logger ("reload tenders page fail")
                    | ex -> Logging.Log.logger (ex)
                            statement <- false
            statement <- false
        ()
    
    member private this.ParserTenders (driver : ChromeDriver) (t : IWebElement) =
        let builder = new TenderBuilder()
        
        let result =
            builder { 
                let! purName = t.findElementWithoutException ("./span/a", sprintf "purName not found %s" t.Text)
                let hrefT = t.FindElement(By.XPath("./span/a[@class = 'caption']"))
                let! href = hrefT.findAttributeWithoutException ("href", "href not found")
                let! purNumT = t.findElementWithoutException (".//tr/td/div[@class = 'subtitle']", "purNumT not found")
                let purNum = purNumT.Replace("№", "").RegexDeleteWhitespace()
                let orgNameFull =
                    t.findElementWithoutException 
                        (".//td[contains(., 'Наименование заказчика')]/following-sibling::td/a")
                let innCus = orgNameFull.Get1FromRegexpOrDefaul(@"ИНН:.*(\d{10})")
                let! pwName = t.findElementWithoutExceptionOptional 
                                  (".//td[contains(., 'Тип приглашения')]/following-sibling::td/label", "")
                let! dates = t.findElementWithoutException 
                                 (".//td[contains(., 'Срок действия приглашения')]/following-sibling::td", 
                                  sprintf "dates not found %s" t.Text)
                let! datePubT = dates.Get1
                                    ("от\s*(\d{2}\.\d{2}\.\d{4}\s\d{1,2}:\d{2}:\d{2})\sдо", 
                                     sprintf "datePubT not found %s" dates)
                let! datePub = datePubT.DateFromString("dd.MM.yyyy H:mm:ss", sprintf "datePub not parse %s" datePubT)
                let! dateEndT = dates.Get1
                                    ("до\s*(\d{2}\.\d{2}\.\d{4}\s\d{1,2}:\d{2}:\d{2})", 
                                     sprintf "dateEndT not found %s" dates)
                let! dateEnd = dateEndT.DateFromString("dd.MM.yyyy H:mm:ss", sprintf "dateEnd not parse %s" dateEndT)
                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub.AddHours(-2.)
                      DateEnd = dateEnd.AddHours(-2.)
                      PwName = pwName
                      OrgInn = innCus }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
        ()
