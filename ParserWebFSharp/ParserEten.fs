namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserEten(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(60.)
    let url = "http://etender.gov.md/proceduri"
    let listTenders = new List<EtenRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
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
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[@id = 'langRu']")).Displayed) |> ignore
        this.Prepared(driver, wait)
        this.ParserListTenders driver
        for t in listTenders do
            try
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)

    member __.Prepared(driver: ChromeDriver, wait: WebDriverWait) =
            driver.SwitchTo().DefaultContent() |> ignore
            driver.FindElement(By.XPath("//a[@id = 'langRu']")).Click()
            wait.Until(fun dr ->
            dr.FindElement(By.XPath("//table[@id = 'list']/tbody/tr[50]")).Displayed) |> ignore
            driver.SwitchTo().DefaultContent() |> ignore
            ()
    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//table[@id = 'list']/tbody/tr")
        for t in tenders do
            this.ParserTenders t
        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: EtenRec) =
        try
            let T = TenderEten(set, t, 210, "Регистр Государственных закупок", "http://etender.gov.md/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = new TenderBuilder()
        let result =
            builder {
                let! purNum = i.findElementWithoutException (".//td[2]", sprintf "purNum not found, inner text - %s" i.Text)
                let! hrefT = i.findWElementWithoutException
                                   (".//td[3]/a", sprintf "hrefT not found, text the element - %s" i.Text)
                let! href = hrefT.findAttributeWithoutException ("href", "href not found")
                let! orgName = i.findElementWithoutException
                                   (".//td[7]", sprintf "orgName not found %s" i.Text)
                let! pwName = i.findElementWithoutException
                                   (".//td[5]", sprintf "purName not found %s" i.Text)
                let! status = i.findElementWithoutException
                                   (".//td[8]", sprintf "status not found %s" i.Text)
                let! purName = i.findElementWithoutException
                                   (".//td[9]", sprintf "purName not found %s" i.Text)
                let! pubDateT = i.findElementWithoutException
                                   (".//td[6]", sprintf "pubDateT not found %s" i.Text)
                let! datePub = pubDateT.DateFromString("dd.MM.yyyy", sprintf "datePub not parse %s" pubDateT)
                let ten =
                    { PwName = pwName
                      Href = href
                      PurNum = purNum
                      PurName = purName
                      OrgName = orgName
                      Status = status
                      DatePub = datePub }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success _ -> ()
        | Error e -> Logging.Log.logger e
        ()
