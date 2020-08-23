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

type ParserBidZaar(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let url = "https://bidzaar.com/procedures/public"
    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = new List<BidzaarRec>()
    let options = ChromeOptions()

    do 
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
        options.AddArguments("window-size=1920,1080")
    override __.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try 
            try 
                __.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()
    
    member private __.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//button[span[contains(., 'ВХОД')]]")).Displayed) |> ignore
        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//div[@class= 'list-item-wrapper ng-star-inserted']/div[@class = 'item'][position() = 1]")).Displayed) |> ignore
        Thread.Sleep(3000)
        __.Scroll(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        __.ParserListTenders(driver)
        for t in listTenders do
            try 
                __.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : BidzaarRec) =
        try 
            let T = TenderBidZaar(set, t, 269, "Bidzaar", "https://bidzaar.com/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    member private __.Scroll(driver : ChromeDriver) =
        try
            for i in 1..5 do
                let jse = driver :> IJavaScriptExecutor
                jse.ExecuteScript("document.getElementsByClassName('cdk-virtual-scroll-viewport scroll-container cdk-virtual-scroll-orientation-vertical')[0].scrollBy(0, 500)", "") |> ignore
                Thread.Sleep(100)
        with ex -> Logging.Log.logger ex
        ()
    member private __.Auth(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.SwitchTo().DefaultContent() |> ignore
        driver.FindElement(By.XPath("//button[span[contains(., 'ВХОД')]]")).Click()
        driver.SwitchTo().Frame(0) |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[contains(@name, 'Email')]")).Displayed) |> ignore
        driver.FindElement(By.XPath("//input[contains(@name, 'Email')]")).SendKeys(Settings.UserBidZaar)
        driver.FindElement(By.XPath("//input[contains(@name, 'Password')]")).SendKeys(Settings.PassBidZaar)
        Thread.Sleep(3000)
        driver.FindElement(By.XPath("//button[contains(.,'Войти')]")).Click()
        Thread.Sleep(3000)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//div[@class= 'list-item-wrapper ng-star-inserted']/div[@class = 'item']")
        for t in tenders do
            this.ParserTenders t
        ()
    
    member private this.ParserTenders (i : IWebElement) =
        let builder = TenderBuilder()
        let res = builder {
            let! hrefT = i.findWElementWithoutException(".//a[contains(@class, 'item ng-star-inserted')]", sprintf "hrefT not found, text the element - %s" i.Text)
            let! href = hrefT.findAttributeWithoutException ("href", "href not found")
            let! purNum = i.findElementWithoutException(".//div[contains(@class, 'number')]", sprintf "purNum not found %s" i.Text)
            let! purName = i.findElementWithoutException(".//div[@class = 'body']/div[@class = 'name']", sprintf "purName not found %s" i.Text)
            let! pwName = i.findElementWithoutException(".//cgn-prs-status[@class = 'status proposal']", sprintf "pwName not found %s" i.Text)
            let! cusName = i.findElementWithoutException(".//cgn-prs-side-info[@class = 'side-info']//div[@class = 'name']", sprintf "cusName not found %s" i.Text)
            let! pubDateT = i.findElementWithoutException(".//div[@class='title' and . = 'Опубликована']/following-sibling::div", sprintf "pubDateT not found %s" i.Text)
            let! datePub = pubDateT.DateFromString("dd.MM.yyyy • HH:mm", sprintf "datePub not parse %s" pubDateT)
            let! endDateT = i.findElementWithoutException(".//div[@class='title' and . = 'Прием предложений до']/following-sibling::div", sprintf "endDateT not found %s" i.Text)
            let! dateEnd1 = endDateT.Get1("(\d{2}\.\d{2}\.\d{4}.+\d{2}:\d{2})", sprintf "dateEnd1 not found %s" endDateT)
            let! dateEnd = dateEnd1.DateFromString("dd.MM.yyyy • HH:mm", sprintf "endDate not parse %s" dateEnd1)
            let ten =
                    { Href = href
                      PurName = purName
                      PurNum = purNum
                      CusName = cusName
                      PwName = pwName
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

