namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserSamolet(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "https://tender.samolet.ru/trades"
    let listTenders = List<SamoletRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
        options.AddArguments("ignore-certificate-errors")
        options.AddArguments("window-size=1920,1080")

    override this.Parsing() =
        let driver =
            new ChromeDriver("/usr/local/bin", options)

        driver.Manage().Timeouts().PageLoad <- timeoutB

        try
            try
                this.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with
                | ex -> Logging.Log.logger ex
        finally
            driver.Quit()

        ()
    
    member private __.Auth(driver: ChromeDriver) =
        driver
            .FindElement(By.XPath("//input[@formcontrolname = 'login']"))
            .SendKeys(Settings.UserSamolet)

        driver
            .FindElement(By.XPath("//input[@type = 'password']"))
            .SendKeys(Settings.PassSamolet)

        Thread.Sleep(3000)

        driver
            .FindElement(By.XPath("//button[contains(., 'ДАЛЕЕ')]"))
            .Click()

        Thread.Sleep(3000)

        ()

    member private this.ParserSelen(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//um-trade-search-card/um-card")
                )
                .Displayed)
        |> ignore

        this.ParserListTenders driver
        driver.Navigate().GoToUrl(url)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//button[contains(., 'Войти')]")
                )
                .Displayed)
        |> ignore
        driver.SwitchTo().DefaultContent() |> ignore

        driver
            .FindElement(By.XPath("//button[contains(., 'Войти')]"))
            .Click()
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//input[@formcontrolname = 'login']")
                )
                .Displayed)
        |> ignore

        this.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        Thread.Sleep(3000)
        for t in listTenders do
            try
                this.ParserTendersList driver t
            with
                | ex -> Logging.Log.logger (ex)

    member private this.ParserTendersList (driver: ChromeDriver) (t: SamoletRec) =
        try
            let T =
                TenderSamolet(set, t, 130, "АО «Группа компаний «Самолет»", "https://samoletgroup.ru/", driver)

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        let tenders =
            driver.FindElementsByXPath("//um-trade-search-card/um-card")

        for t in tenders do
            this.ParserTenders driver t

        ()

    member private this.ParserTenders (_: ChromeDriver) (i: IWebElement) =
        let builder = TenderBuilder()

        let result =
            builder {
                let! hrefT =
                    i.findWElementWithoutException (
                        ".//a[contains(@class, 'trade-title')]",
                        sprintf "hrefT not found %s" i.Text
                    )

                let! href = hrefT.findAttributeWithoutException ("href", "hrefT not found")

                let! purName =
                    i.findElementWithoutException (
                        ".//a[contains(@class, 'trade-title')]",
                        sprintf "purName not found %s" i.Text
                    )

                let! purNum =
                    i.findElementWithoutException (
                        ".//div[contains(@class, 'trade-number')]",
                        sprintf "purName not found %s" i.Text
                    )

                let delivPlace = ""
                let dateEnd = DateTime.Now
                let datePub = DateTime.Now

                let ten =
                    { SamoletRec.Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub
                      DateEnd = dateEnd
                      DelivPlace = delivPlace }

                listTenders.Add(ten)
                return "ok"
            }

        match result with
        | Success _ -> ()
        | Error e -> Logging.Log.logger e
