namespace ParserWeb

open System
open TypeE
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserBidMartNew(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://www.bidmart.by/b2c/sellers/tender"

    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<BidMartNewRec>()
    let options = ChromeOptions()

    let pageReloader (driver: ChromeDriver) (x: int) =
        for i in 1..x do
            let jse = driver :> IJavaScriptExecutor

            jse.ExecuteScript("document.querySelectorAll('app-section-list')[0].scrollBy(0, 500)", "")
            |> ignore

            Thread.Sleep(100)

    do
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")

    override __.Parsing() =
        let driver =
            new ChromeDriver("/usr/local/bin", options)

        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                __.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with
                | ex -> Logging.Log.logger ex
        finally
            driver.Quit()

        ()

    member private __.Auth(driver: ChromeDriver) =
        driver
            .FindElement(By.XPath("//input[@formcontrolname = 'login']"))
            .SendKeys(Settings.UserBidMart)

        driver
            .FindElement(By.XPath("//input[@type = 'password']"))
            .SendKeys(Settings.PassBidMart)

        Thread.Sleep(3000)

        driver
            .FindElement(By.XPath("//a[. = 'Войти']"))
            .Click()

        Thread.Sleep(3000)

        ()

    member private __.ParserSelen(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//input[@formcontrolname = 'login']")
                )
                .Displayed)
        |> ignore

        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        Thread.Sleep(3000)

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//tbody/tr[not(@class)]")
                )
                .Displayed)
        |> ignore

        __.GetListTenders(driver)
        __.ParserListTenders(driver)

        for t in listTenders do
            try
                __.ParserTendersList driver t
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: BidMartNewRec) =
        try
            let T =
                TenderBidMartNew(set, t, 102, "ООО «Бидмартс»", "https://www.bidmart.by/", driver)

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.GetListTenders(driver: ChromeDriver) =
        pageReloader driver 500
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//tbody/tr[not(@class)]")

        for t in tenders do
            this.ParserTenders t

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let res =
            builder {
                let! hrefT =
                    i.findWElementWithoutException (".//td/a", sprintf "hrefT not found, text the element - %s" i.Text)

                let! href = hrefT.findAttributeWithoutException ("href", "href not found")
                let! purNum = href.Get1("supplier/(\w+)$", sprintf "purNum not found %s" href)
                let! purName = i.findElementWithoutException (".//td/a", sprintf "purName not found %s" i.Text)

                let! delivPlace =
                    i.findElementWithoutException ("./td[6]/span", sprintf "delivPlace not found %s" i.Text)

                let! status =
                    i.findElementWithoutException (
                        "./td[7]//span[@class = 'additional']",
                        sprintf "status not found %s" i.Text
                    )

                let! cusName = i.findElementWithoutException ("./td[3]", sprintf "cusName not found %s" i.Text)
                let! endDateT = i.findElementWithoutException ("./td[5]", sprintf "endDateT not found %s" i.Text)

                let endDateT =
                    sprintf "%s.%s"
                    <| DateTime.Now.Year.ToString()
                    <| endDateT.ReplaceDateBidMart().RegexCutWhitespace()

                let! dateEnd = endDateT.DateFromString("yyyy.dd.MM HH:mm", sprintf "endDate not parse %s" endDateT)

                let! pubDateT = i.findElementWithoutException ("./td[4]", sprintf "endDateT not found %s" i.Text)

                let pubDateT =
                    sprintf "%s.%s"
                    <| DateTime.Now.Year.ToString()
                    <| pubDateT.ReplaceDateBidMart().RegexCutWhitespace()

                let! datePub = pubDateT.DateFromString("yyyy.dd.MM HH:mm", sprintf "datePub not parse %s" pubDateT)
                let! price = i.findElementWithoutException ("./td[8]/b", sprintf "price not found %s" i.Text)
                let! nmck = price.Get1("^([\s\d,]+)", sprintf "nmck not found %s" href)
                let nmck = nmck.GetPriceFromString()
                let! currency = i.findElementWithoutException ("./td[8]", sprintf "price not found %s" i.Text)
                let! currency = currency.Get1("([aA-Z]+)", sprintf "currency not found %s" href)
                let currency = currency.Trim()

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
