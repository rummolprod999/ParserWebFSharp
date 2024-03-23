namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserCisLink(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(60.)

    let url =
        "https://ident.cislinketp.com/Account/Login"

    let listTenders = List<CisLinkRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")

    override this.Parsing() =
        let driver =
            new ChromeDriver("/usr/local/bin", options)

        driver.Manage().Timeouts().PageLoad <- timeoutB
        driver.Manage().Window.Maximize()

        try
            try
                this.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with
                | ex -> Logging.Log.logger ex
        finally
            driver.Quit()

        ()

    member private this.ParserSelen(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//input[@id = 'Username']")
                )
                .Displayed)
        |> ignore

        this.Auth driver
        driver.SwitchTo().DefaultContent() |> ignore

        driver
            .Navigate()
            .GoToUrl("https://cislinketp.com/schedule")

        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//mat-table//mat-row")
                )
                .Displayed)
        |> ignore

        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        this.ParserListTenders driver

        for t in listTenders do
            try
                this.ParserTendersList driver t
            with
                | ex -> Logging.Log.logger (ex)

    member private this.Auth(driver: ChromeDriver) =
        driver
            .FindElement(By.XPath("//input[@id = 'Username']"))
            .SendKeys(Settings.UserCisLink)

        driver
            .FindElement(By.XPath("//input[@id = 'Password']"))
            .SendKeys(Settings.PassCisLink)

        driver
            .FindElement(By.XPath("//button[@value = 'login']"))
            .Click()

        Thread.Sleep(3000)
        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: CisLinkRec) =
        try
            let T =
                TenderCisLink(set, t, 217, "CISLINK", "http://auction.cislink.com/", driver)

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//mat-table//mat-row")

        for t in tenders do
            this.ParserTenders t

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let result =
            builder {
                let! orgName =
                    i.findElementWithoutException (
                        "./mat-cell[1]",
                        sprintf "orgName not found %s" i.Text
                    )

                let! purName = i.findElementWithoutException ("./mat-cell[2]", sprintf "purName not found %s" i.Text)
                let purNum = Tools.createMD5 purName

                let mutable href =
                    i.findWElementAttrOrEmpty ("./mat-cell[2]//a", "href")

                if href = "" then
                    href <- "https://cislinketp.com/schedule"

                let! pubDateT = i.findElementWithoutException ("./mat-cell[5]", sprintf "pubDateT not found %s" i.Text)
                let! datePub = pubDateT.DateFromString("dd.MM.yyyy HH:mm:ss", sprintf "datePub not parse %s" pubDateT)

                let ten =
                    { CisLinkRec.Href = href
                      CisLinkRec.DatePub = datePub
                      CisLinkRec.DateEnd = datePub
                      CisLinkRec.PurNum = purNum
                      CisLinkRec.PurName = purName
                      CisLinkRec.OrgName = orgName }

                listTenders.Add(ten)
                return "ok"
            }

        match result with
        | Success _ -> ()
        | Error e -> Logging.Log.logger e

        ()
