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

type ParserMoek(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let url = "https://www.moek.ru/tenders/#1"
    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = new List<MoekRec>()
    let options = ChromeOptions()
    do 
        options.AddArguments("headless")
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
    
    member private this.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//table[contains(@class, 'data')]/tbody/tr[1]")).Displayed) |> ignore
        //this.PreparedPage driver
        this.ParserListTenders driver
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex) 
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : MoekRec) =
        try 
            let T = TenderMoek(set, t, 239, "ПАО «МОЭК»", "https://www.moek.ru/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//table[contains(@class, 'data')]/tbody/tr")
        for t in tenders do
            this.ParserTenders t
        () 
    member private this.ParserTenders (i : IWebElement) =
        let builder = TenderBuilder()
        let res = builder {
            let! purNum = i.findElementWithoutException ("./td[2]//a/span", sprintf "purNum not found, inner text - %s" i.Text)
            let! purName = i.findElementWithoutException
                                   ("./td[2]/p[2]", sprintf "purName not found %s" i.Text)
            let! hrefT = i.findWElementWithoutException
                                   ("./td[2]//a", sprintf "hrefT not found, text the element - %s" i.Text)
            let! href = hrefT.findAttributeWithoutException ("href", "href not found")
            let! orgName = i.findElementWithoutException
                                   ("./td[4]/p", sprintf "orgName not found %s" i.Text)
            let! cusName = i.findElementWithoutException
                                   ("./td[3]/p", sprintf "cusName not found %s" i.Text)
            let! endDateT = i.findElementWithoutException
                                   ("./td[1]/p/span", sprintf "endDateT not found %s" i.Text)
            let! dateEnd = endDateT.DateFromString("dd.MM.yyyy в H:mm", sprintf "endDateT not parse %s" endDateT)
            let datePub = DateTime.Now
            let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      OrgName = orgName
                      CusName = cusName
                      DatePub = datePub
                      DateEnd = dateEnd }
            listTenders.Add(ten)
            return ""}
        match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
        
        ()