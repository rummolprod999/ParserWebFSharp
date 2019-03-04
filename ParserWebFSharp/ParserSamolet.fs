namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserSamolet(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "https://samoletgroup.ru/tenders/"
    let listTenders = new List<SamoletRec>()
    let options = ChromeOptions()
    
    do 
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
    
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
            dr.FindElement(By.XPath("//div[contains(@class, 'tender-list__list')]/a[@class = 'tender-card']")).Displayed) 
        |> ignore
        this.ParserListTenders driver
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : SamoletRec) =
        try 
            let T = TenderSamolet(set, t, 130, "АО «Группа компаний «Самолет»", "https://samoletgroup.ru/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        let tenders =
            driver.FindElementsByXPath("//div[contains(@class, 'tender-list__list')]/a[@class = 'tender-card']")
        for t in tenders do
            this.ParserTenders driver t
        ()
    
    member private this.ParserTenders (driver : ChromeDriver) (i : IWebElement) =
        let builder = new TenderBuilder()
        
        let result =
            builder { 
                let! href = i.findAttributeWithoutException ("href", "hrefT not found")
                let! purName = i.findElementWithoutException 
                                   (".//div[@class = 'tender-card__name']", sprintf "purName not found %s" i.Text)
                let delivPlace = i.findElementWithoutException (".//div[@class = 'tender-card__project-name']")
                let! dateEndTT = i.findElementWithoutException 
                                     (".//div[. = 'Прием заявок']/following-sibling::div", 
                                      sprintf "dateEndTT not found %s" i.Text)
                let! dateEndT = dateEndTT.Get1("(\d{2}\.\d{2}\.\d{4})", sprintf "datePubT not found %s" dateEndTT)
                let! dateEnd = dateEndT.DateFromString("dd.MM.yyyy", sprintf "datePub not parse %s" dateEndT)
                let datePub = DateTime.Now
                let purNum = Tools.createMD5 purName
                
                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub
                      DateEnd = dateEnd
                      DelivPlace = delivPlace }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
