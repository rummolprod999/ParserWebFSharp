namespace ParserWeb

open System
open TypeE
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System.Linq

type ParserBidZaar(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://bidzaar.com/procedures/public"

    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<BidzaarRec>()
    let options = ChromeOptions()

    do
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
        options.AddArguments("remote-debugging-port=9222")
        options.AddArguments("window-size=1920,1080")

    override __.Parsing() =
        let driver =
            new ChromeDriver("/usr/local/bin", options)

        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                driver.Manage().Cookies.DeleteAllCookies()
                __.ParserSelen driver
            with
                | ex -> Logging.Log.logger ex
        finally
            driver.Quit()

        ()

    member private __.ParserSelen(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)

        driver
            .Navigate()
            .GoToUrl("https://bidzaar.com/auth/account/login")

        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//button[contains(., 'Войти')]")
                )
                .Displayed)
        |> ignore

        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        (*driver
            .FindElement(By.XPath("//button[contains(., ' Дата публикации ')]"))
            .Click()*)

        Thread.Sleep(3000)

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//div[contains(@class, 'ng-star-inserted')]/div[@class = 'item-content'][position() = 1]")
                )
                .Displayed)
        |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        __.Scroll driver wait

        for t in listTenders do
            try
                __.ParserTendersList driver t
            with
                | :? WebDriverException as e -> raise e
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: BidzaarRec) =
        try
            let T =
                TenderBidZaar(set, t, 269, "Bidzaar", "https://bidzaar.com/", driver)

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private __.Scroll(driver: ChromeDriver) (wait: WebDriverWait) =

        for i in 1..50 do
            try
                driver.SwitchTo().DefaultContent() |> ignore
                __.ParserListTenders(driver)
                driver.SwitchTo().DefaultContent() |> ignore
                let jse = driver :> IJavaScriptExecutor

                jse.ExecuteScript(
                    "document.getElementsByClassName('cdk-virtual-scroll-viewport scroll-container cdk-virtual-scroll-orientation-vertical')[0].scrollBy(0, 1000)",
                    ""
                )
                |> ignore
                driver.SwitchTo().DefaultContent() |> ignore
                wait.Until (fun driver -> driver.FindElement(By.XPath("//div[contains(@class, 'ng-star-inserted')]/div[@class = 'item-content'][position() = 1]")).Displayed)
                |> ignore
                driver.SwitchTo().DefaultContent() |> ignore
            with
                | ex -> Logging.Log.logger ex

        ()

    member private __.Auth(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.SwitchTo().DefaultContent() |> ignore

        driver
            .FindElement(By.XPath("//button[contains(., 'Войти')]"))
            .Click()

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//input[contains(@name, 'Email')]")
                )
                .Displayed)
        |> ignore

        driver
            .FindElement(By.XPath("//input[contains(@name, 'Email')]"))
            .SendKeys(Settings.UserBidZaar)

        driver
            .FindElement(By.XPath("//input[contains(@name, 'Password')]"))
            .SendKeys(Settings.PassBidZaar)

        Thread.Sleep(3000)

        driver
            .FindElement(By.XPath("//button[contains(.,'Войти')]"))
            .Click()

        Thread.Sleep(3000)
        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//div[contains(@class, 'ng-star-inserted')]/div[@class = 'item-content']")

        for t in tenders do
            this.ParserTenders t

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let res =
            builder {
                let! hrefT =
                    i.findWElementWithoutException (
                        ".//a[contains(@class, 'link')]",
                        sprintf "hrefT not found, text the element - %s" i.Text
                    )

                let! href = hrefT.findAttributeWithoutException ("href", "href not found")
                let! purNum = href.Get1("light/([a-z\d-]+)(?:/request)?", sprintf "purNum not found %s" i.Text)

                let! purName =
                    i.findElementWithoutException (
                        ".//div[@class = 'link-header']/div[@class = 'name']",
                        sprintf "purName not found %s" i.Text
                    )

                let pwName = ""

                let! cusName =
                    i.findElementWithoutException (
                        ".//bdz-cmp-name//div[@class = 'name ng-star-inserted']",
                        sprintf "cusName not found %s" i.Text
                    )

                let datePub = DateTime.Now

                let! endDateT =
                    i.findElementWithoutException (
                        ".//div[contains(@class, 'date ng-star-inserted')]",
                        sprintf "endDateT not found %s" i.Text
                    )

                let! dateEnd1 =
                    endDateT.Get1("(\d{2}\.\d{2}\.\d{4}.+\d{2}:\d{2})", sprintf "dateEnd1 not found %s" endDateT)

                let! dateEnd = dateEnd1.DateFromString("dd.MM.yyyy • HH:mm", sprintf "endDate not parse %s" dateEnd1)

                let ten =
                    { Href = href
                      PurName = purName
                      PurNum = purNum
                      CusName = cusName
                      PwName = pwName
                      DateEnd = dateEnd
                      DatePub = datePub }
                let res = listTenders.Where(fun t -> t.Href = href).ToList()
                if res.Count < 1 then 
                    listTenders.Add(ten)
                return ""
            }

        match res with
        | Success _ -> ()
        | Error e when e = "" -> ()
        | Error r -> Logging.Log.logger r

        ()
