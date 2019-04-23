namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserTsm(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(60.)
    let url = "https://tender.tsm.ru/trades?page=purchases"
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
            dr.FindElement(By.XPath("//div[@id = 'trades']//um-trade-list-item")).Displayed) |> ignore
        this.ParserListTenders driver
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex) 
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : SamoletRec) =
        try 
            let T = TenderTsm(set, t, 185, "ООО \"Трансстроймеханизация\"", "https://tender.tsm.ru/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        let tenders =
            driver.FindElementsByXPath("//div[@id = 'trades']//um-trade-list-item")
        for t in tenders do
            this.ParserTenders driver t
        ()
    
    member private this.ParserTenders (driver : ChromeDriver) (i : IWebElement) =
        let builder = new TenderBuilder()
        let result =
            builder { 
                let! hrefT = i.findWElementWithoutException 
                                   (".//a", sprintf "hrefT not found %s" i.Text)
                let! href = hrefT.findAttributeWithoutException ("href", "hrefT not found")
                let! purName = i.findElementWithoutException 
                                   (".//a/span", sprintf "purName not found %s" i.Text)
                let! purNum =  i.findElementWithoutException(".//div[@class = 'header-row']/span[contains(@class, 'registered-number')]", sprintf "purName not found %s" i.Text)
                let delivPlace  = ""
                let dateEnd = DateTime.Now
                let datePub = DateTime.Now
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