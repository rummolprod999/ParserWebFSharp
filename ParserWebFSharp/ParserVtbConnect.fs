namespace ParserWeb

open System
open TypeE
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserVtbConnect(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://www.vtbconnect.ru/login?redirect=https://www.vtbconnect.ru/trades/vtb/"

    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<VtbConnectRec>()
    let options = ChromeOptions()

    do
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
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
                driver.Manage().Cookies.DeleteAllCookies()
            with
                | ex -> Logging.Log.logger ex
        finally
            driver.Quit()

        ()

    member private __.ParserSelen(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//input[@placeholder = 'Введите адрес эл. почты']")
                )
                .Displayed)
        |> ignore

        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//div[@class= 'auction_items']/div[@class = 'auction_item']")
                )
                .Displayed)
        |> ignore

        Thread.Sleep(3000)
        __.Scroll(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        __.ParserListTenders(driver)

        for t in listTenders do
            try
                __.ParserTendersList driver t
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: VtbConnectRec) =
        try
            let T =
                TenderVtbConnect(set, t, 290, "ВТБ Бизнес Коннект", "https://www.vtbconnect.ru/", driver)

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private __.Scroll(driver: ChromeDriver) =
        try
            driver
                .FindElement(By.XPath("//div[@class = 'catalog_items_filter']/select"))
                .Click()

            Thread.Sleep(1000)

            driver
                .FindElement(By.XPath("//div[@class = 'catalog_items_filter']/select/option[@value = '100']"))
                .Click()

            Thread.Sleep(15000)
        with
            | ex -> Logging.Log.logger ex

        ()

    member private __.Auth(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//input[@placeholder = 'Введите адрес эл. почты']")
                )
                .Displayed)
        |> ignore

        driver
            .FindElement(By.XPath("//input[@placeholder = 'Введите адрес эл. почты']"))
            .SendKeys(Settings.UserVtb)

        driver
            .FindElement(By.XPath("//input[@placeholder = 'Введите пароль']"))
            .SendKeys(Settings.PassVtb)

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//input[@placeholder = 'Введите адрес эл. почты']")
                )
                .Enabled)
        |> ignore

        driver
            .FindElement(By.XPath("//button[@type = 'submit']"))
            .Click()

        Thread.Sleep(3000)
        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//div[@class= 'auction_items']/div[@class = 'auction_item']")

        for t in tenders do
            this.ParserTenders t

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let res =
            builder {
                let! hrefT =
                    i.findWElementWithoutException (
                        ".//a[. = 'Подробнее']",
                        sprintf "hrefT not found, text the element - %s" i.Text
                    )

                let! href = hrefT.findAttributeWithoutException ("href", "href not found")

                let! purNum =
                    i.findElementWithoutException (
                        ".//p[@class = 'auction_item_number']",
                        sprintf "purNum not found %s" i.Text
                    )

                let purNum = purNum.Replace("№", "")
                let! pubDateT =
                    i.findElementWithoutException (
                        ".//li[contains(., 'Дата публикации')]",
                        sprintf "pubDateT not found %s num %s" i.Text purNum
                    )

                let pubDateT =
                    pubDateT
                        .Replace("Дата публикации", "")
                        .Replace(":", "")
                        .Trim()
                        .RegexDeleteWhitespace()

                let! datePub = pubDateT.DateFromString("dd.MM.yyyy", sprintf "datePub not parse %s" pubDateT)

                let! dateEndT =
                    i.findElementWithoutException (
                        ".//li[contains(., 'Дата окончания приёма предложений')]",
                        sprintf "dateEndT not found %s" i.Text
                    )

                let dateEndT =
                    dateEndT
                        .Replace("Дата окончания приёма предложений", "")
                        .Replace(":", "")
                        .Trim()

                let! dateEnd = dateEndT.DateFromString("dd.MM.yyyy", sprintf "endDate not parse %s" dateEndT)


                let! purName =
                    i.findElementWithoutException (
                        ".//li[contains(., 'Наименование')]",
                        sprintf "purName not found %s" i.Text
                    )

                let purName =
                    purName
                        .Replace("Наименование", "")
                        .Replace(":", "")
                        .Trim()

                let! orgName =
                    i.findElementWithoutException (
                        ".//li[contains(., 'Организатор')]",
                        sprintf "orgName not found %s" i.Text
                    )

                let orgName =
                    orgName
                        .Replace("Организатор", "")
                        .Replace(":", "")
                        .Trim()

                let! status =
                    i.findElementWithoutException (
                        ".//div[contains(@class, 'auction_item_status')]",
                        sprintf "status not found %s" i.Text
                    )

                let! nmckT =
                    i.findElementWithoutExceptionOptional (
                        ".//li[contains(., 'Начальная цена')]",
                        sprintf "nmckT not found %s" i.Text
                    )

                let nmckT =
                    nmckT
                        .Replace("Начальная цена", "")
                        .Replace(":", "")
                        .Trim()

                let nmck = nmckT.GetPriceFromString()
                let currency = "RUB"

                let ten =
                    { Href = href
                      PurName = purName
                      PurNum = purNum
                      OrgName = orgName
                      Nmck = nmck
                      Status = status
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
