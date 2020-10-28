namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserComita(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(120.)
    let url = "https://etp.comita.ru/commercialProcedures"
    let listTenders = List<ComitaRec>()
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
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//div[contains(@class, 'procedure-item') and contains(@class, 'ng-scope')]")).Displayed) 
        |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//div[contains(@class, 'procedure-item') and contains(@class, 'ng-scope')]")
        for t in tenders do
            try 
                this.ParserTenders driver t
            with ex -> Logging.Log.logger (ex)
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : ComitaRec) =
        try 
            let T = TenderComita(set, t, 105, "АО «Комита»", "https://etp.comita.ru/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.ParserTenders (_ : ChromeDriver) (t : IWebElement) =
        let builder = TenderBuilder()
        
        let result =
            builder { 
                let! purNum = t.findElementWithoutException (".//div[@class = 'title']/a", "purNum not found")
                let hrefT = t.FindElement(By.XPath(".//div[@class = 'title']/a"))
                let href = hrefT.GetAttribute("href")
                let! purName = t.findElementWithoutException 
                                   (".//div[contains(@class,'text') and contains(@class,'ng-binding')]", 
                                    "purName not found")
                let! status = t.findElementWithoutExceptionOptional 
                                  (".//div[@class = 'status']/div[contains(@class,'red')]", "")
                let! priceT = t.findElementWithoutExceptionOptional (".//div[contains(@class, 'cost')]/div", "")
                let price = priceT.Replace("&nbsp;", "").Replace(",", ".").RegexDeleteWhitespace()
                let! currencyT = t.findElementWithoutExceptionOptional (".//div[contains(@class, 'cost')]/span", "")
                let! currency = currencyT.Get1Optional(@"^(\w+)\s+")
                let! datePubT = t.findElementWithoutException 
                                    (".//div[contains(@class,'acceptance-date')][1]/data", "datePubT not found")
                let! datePubR = datePubT.Get1(@"^(\d{2}\.\d{2}\.\d{4})\s+", "datePubR not found")
                let! datePub = datePubR.DateFromString("dd.MM.yyyy", "datePub not found")
                let! dateEndT = t.findElementWithoutException 
                                    (".//div[contains(@class,'acceptance-date')][2]/data", "dateEndT not found")
                let! dateEndR = dateEndT.RegexCutWhitespace()
                                        .Get1(@"^(\d{2}\.\d{2}\.\d{4}\s+\d{2}:\d{2})", "dateEndR not found")
                let! dateEnd = dateEndR.DateFromString("dd.MM.yyyy HH:mm", "dateEnd not found")
                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub
                      DateEnd = dateEnd
                      Nmck = price
                      Currency = currency
                      Status = status }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success _ -> ()
        | Error e -> Logging.Log.logger e
        ()
