namespace ParserWeb

open System
open System.Collections.Generic
open System.Linq.Expressions
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserCisLink(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(60.)
    let url = "http://auction.cislink.com/account/login"
    let listTenders = new List<CisLinkRec>()
    let options = ChromeOptions()

    do
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")

    override this.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()
    member private this.ParserSelen(driver: ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until(fun dr -> dr.FindElement(By.XPath("//input[@class = 'input-block-level auth_login']")).Displayed) |> ignore
        this.Auth driver
        driver.SwitchTo().DefaultContent() |> ignore
        driver.Navigate().GoToUrl("http://auction.cislink.com/auction/schedule")
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until(fun dr -> dr.FindElement(By.XPath("//tbody[@data-bind = 'foreach: Auctions']")).Displayed) |> ignore
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        this.ParserListTenders driver
    
    member private this.Auth(driver: ChromeDriver) =
        driver.FindElement(By.XPath("//input[@class = 'input-block-level auth_login']")).SendKeys(Settings.UserCisLink)
        driver.FindElement(By.XPath("//input[@class = 'input-block-level auth_pass']")).SendKeys(Settings.PassCisLink)
        driver.FindElement(By.XPath("//input[@id = 'login-button']")).Click()
        Thread.Sleep(3000)
        ()
    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//tbody[@data-bind = 'foreach: Auctions']/tr")
        for t in tenders do
            this.ParserTenders t
        ()
    
    member private this.ParserTenders(i: IWebElement) =
        let builder = new TenderBuilder()
        let result =
            builder {
                let! orgName = i.findElementWithoutException
                                   (".//td[@data-bind = 'text: OrganizierName']", sprintf "orgName not found %s" i.Text)
                let! purName = i.findElementWithoutException
                                   (".//td[2]", sprintf "purName not found %s" i.Text)
                let purNum = Tools.createMD5 purName
                let mutable href = i.findWElementAttrOrEmpty(".//td[2]/a", "href")
                if href = "" then href <- "http://auction.cislink.com/auction/schedule"
                let! pubDateT = i.findElementWithoutException
                                   (".//td[3]", sprintf "pubDateT not found %s" i.Text)
                let! datePub = pubDateT.DateFromString("dd.MM.yyyy HH:mm", sprintf "datePub not parse %s" pubDateT)
                let ten =
                    { CisLinkRec.Href = href
                      CisLinkRec.DatePub = datePub
                      CisLinkRec.DateEnd = datePub 
                      CisLinkRec.PurNum = purNum
                      CisLinkRec.PurName = purName
                      CisLinkRec.OrgName = orgName}
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
        ()