namespace ParserWeb

open System
open System.Linq.Expressions
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq
open System.Collections.Generic
open System.Linq.Expressions
open System.Threading
open System.Web
open Tools
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserBidMartNew(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let url = "https://www.bidmart.by/b2c/sellers/tender"
    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = new List<BidMartNewRec>()
    let options = ChromeOptions()
    let pageReloader (driver: ChromeDriver) (x: int) =
                for i in 1..x do
                    let jse = driver :> IJavaScriptExecutor
                    jse.ExecuteScript("document.getElementsByClassName('list alt1-scroll')[0].scrollBy(0, 500)", "") |> ignore
                    Thread.Sleep(100)
    do 
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
    override __.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        driver.Manage().Window.Maximize()
        try 
            try 
                __.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()
    
    member private __.Auth(driver : ChromeDriver) =
        driver.FindElement(By.XPath("//input[@type = 'email']")).SendKeys(Settings.UserBidMart)
        driver.FindElement(By.XPath("//input[@type = 'password']")).SendKeys(Settings.PassBidMart)
        Thread.Sleep(3000)
        driver.FindElement(By.XPath("//button[. = 'Войти']")).Click()
        Thread.Sleep(3000)
        
        ()
    member private __.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@type = 'email']")).Displayed) |> ignore
        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        Thread.Sleep(3000)
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//tr[contains(@class, 'first radius-top radius-bottom')]")).Displayed) |> ignore
        __.GetListTenders(driver)
        __.ParserListTenders(driver)
        for t in listTenders do
            try 
                __.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : BidMartNewRec) =
        try 
            let T = TenderBidMartNew(set, t, 102, "ООО «Бидмартс»", "https://www.bidmart.by/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    member private this.GetListTenders(driver : ChromeDriver) =
        pageReloader driver 1 //TODO change it
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        ()
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//tr[contains(@class, 'first radius-top radius-bottom')]")
        for t in tenders do
            this.ParserTenders t
        ()
        
    member private this.ParserTenders (i : IWebElement) =
        let builder = TenderBuilder()
        let res = builder {
            let! hrefT = i.findWElementWithoutException(".//a[contains(@class, 'link-color')]", sprintf "hrefT not found, text the element - %s" i.Text)
            let! href = hrefT.findAttributeWithoutException ("href", "href not found")
            let! purNum = href.Get1 ("/(\d+)$", sprintf "purNum not found %s" href )
            let! purName = i.findElementWithoutException(".//a[@class = 'link-color']", sprintf "purName not found %s" i.Text)
            let! delivPlace = i.findElementWithoutException("./td[6]", sprintf "delivPlace not found %s" i.Text)
            let! status = i.findElementWithoutException("./td[3]", sprintf "status not found %s" i.Text)
            let! cusName = i.findElementWithoutException("./td[4]", sprintf "cusName not found %s" i.Text)
            let! endDateT = i.findElementWithoutException("./td[5]", sprintf "endDateT not found %s" i.Text)
            let endDateT = sprintf "%s.%s" <| DateTime.Now.Year.ToString() <| endDateT
            let! dateEnd = endDateT.DateFromString("yyyy.dd.MM HH:mm", sprintf "endDate not parse %s" endDateT)
            let! price = i.findElementWithoutException("./td[7]", sprintf "price not found %s" i.Text)
            let! nmck = price.Get1 ("^([\s\d,]+)", sprintf "nmck not found %s" href )
            let nmck = nmck.GetPriceFromString()
            let! currency = price.Get1 ("(\w+)$", sprintf "currency not found %s" href )
            let currency = currency.Trim()
            let datePub = DateTime.MinValue
            let ten =
                    { Href = href
                      PurName = purName
                      PurNum = purNum
                      Status = status
                      DelivPlace = delivPlace
                      CusName = cusName
                      Nmck = nmck
                      Currency = currency
                      DateEnd = dateEnd
                      DatePub = datePub }
            listTenders.Add(ten)
            return ""
        }
        match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
        
        ()