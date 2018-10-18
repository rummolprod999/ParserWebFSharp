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

type ParserEshopRzd(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "https://eshoprzd.ru/purchases"
    let listTenders = new List<EshopRzdRec>()
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
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//div[contains(@class, 'purchase-item') and contains(@class, 'ng-scope')]")).Displayed) 
        |> ignore
        this.ParserListTenders driver
        this.GetNextPage driver wait
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
    
    member private this.GetNextPage (driver : ChromeDriver) (wait : WebDriverWait) =
        for i in 2..5 do
            try 
                driver.SwitchTo().DefaultContent() |> ignore
                this.Clicker driver <| sprintf "//ul[contains(@class, 'pagination-sm')]//li/a[. = '%d']" i
                Thread.Sleep(5000)
                wait.Until(fun dr -> dr.FindElement(By.XPath("//div[contains(@class, 'purchase-item') and contains(@class, 'ng-scope')]")).Displayed) |> ignore
                this.ParserListTenders driver
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//div[contains(@class, 'purchase-item') and contains(@class, 'ng-scope')]")
        for t in tenders do
            try 
                this.ParserTenders driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : EshopRzdRec) =
        try 
            let T = TenderEshopRzd(set, t, 113, "Электронный магазин ОАО \"РЖД\"", "https://eshoprzd.ru/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.ParserTenders (driver : ChromeDriver) (t : IWebElement) =
        let builder = new TenderBuilder()
        
        let result =
            builder { 
                let! purName = t.findElementWithoutException 
                                   (".//a[contains(@class,'name') and contains(@class,'ng-binding')]", 
                                    "purName not found")
                let hrefT = t.FindElement(By.XPath(".//a[contains(@class,'name') and contains(@class,'ng-binding')]"))
                let href = hrefT.GetAttribute("href")
                let! purNumT = t.findElementWithoutException 
                                   (".//a[contains(@class,'name') and contains(@class,'ng-binding')]/span", 
                                    "purNumT not found")
                let purNum = purNumT.Replace("/", "").Replace("№", "").RegexDeleteWhitespace()
                let! status = t.findElementWithoutExceptionOptional (".//div[contains(@class,'status')]", "")
                let! priceT = t.findElementWithoutExceptionOptional (".//div[contains(@class, 'money')]/b", "")
                let priceTT = priceT.Replace("&nbsp;", "").Replace(",", ".").RegexDeleteWhitespace()
                let! price = priceTT.Get1Optional(@"^([\d\.]+)")
                let! currency = priceTT.Get1Optional(@"^[\d\.]+(.{1})\(")
                let! nameCusT = t.findElementWithoutExceptionOptional (".//div[span[contains(., 'Заказчик:')]]", "")
                let nameCus = nameCusT.Replace("Заказчик:", "").Trim()
                let! datePubT = t.findElementWithoutException 
                                    (".//div[span[contains(., 'Дата публикации:')]]", "datePubT not found")
                let! datePubR = datePubT.Get1(@"(\d{2}\.\d{2}\.\d{4})", sprintf "datePubR not found %s" datePubT)
                let! datePub = datePubR.DateFromString("dd.MM.yyyy", "datePub not found")
                let! dateEndT = t.findElementWithoutException 
                                    (".//div[span[contains(., 'Подача оферт до')]]", "dateEndT not found")
                let! dateEndR = dateEndT.RegexCutWhitespace()
                                        .Get1(@"(\d{2}\.\d{2}\.\d{4}\s+\d{2}:\d{2})", sprintf "dateEndR not found %s" dateEndT)
                let! dateEnd = dateEndR.DateFromString("dd.MM.yyyy HH:mm", "dateEnd not found")
                let! regionT = t.findElementWithoutExceptionOptional (".//span[contains(@class, 'region')]", "")
                let region = regionT.RegexCutWhitespace()
                
                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      CusName = nameCus
                      DatePub = datePub
                      DateEnd = dateEnd
                      Status = status
                      Nmck = price
                      Currency = currency
                      RegionName = region }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
        ()
