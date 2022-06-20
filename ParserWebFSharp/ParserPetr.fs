namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserPetr(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "https://eshop-ptz.ru/purchases"
    let listTenders = List<EshopRzdRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")

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

    member private this.ParserSelen(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//div[contains(@class, 'panel-default') and contains(@class, 'panel-norad')]")
                )
                .Displayed)
        |> ignore

        this.ParserListTenders driver
        this.GetNextPage driver wait

        for t in listTenders do
            try
                this.ParserTendersList driver t
            with
                | ex -> Logging.Log.logger (ex)

    member private this.ParserTendersList (driver: ChromeDriver) (t: EshopRzdRec) =
        try
            let T =
                TenderPetr(
                    set,
                    t,
                    219,
                    "Электронный магазин Петрозаводского городского округа",
                    "https://eshop-ptz.ru/",
                    driver
                )

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.GetNextPage (driver: ChromeDriver) (wait: WebDriverWait) =
        for i in 2..5 do
            try
                driver.SwitchTo().DefaultContent() |> ignore

                this.Clicker driver
                <| sprintf "//ul[contains(@class, 'pagination-sm')]//li/a[. = '%d']" i

                Thread.Sleep(5000)

                wait.Until (fun dr ->
                    (dr.FindElement(
                        By.XPath("//div[contains(@class, 'panel-default') and contains(@class, 'panel-norad')]")
                    ))
                        .Displayed)
                |> ignore

                this.ParserListTenders driver
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//div[contains(@class, 'panel-default') and contains(@class, 'panel-norad')]")

        for t in tenders do
            try
                this.ParserTenders driver t
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserTenders (_: ChromeDriver) (t: IWebElement) =
        let builder = TenderBuilder()

        let result =
            builder {
                let! purName =
                    t.findElementWithoutException (".//a[contains(@class,'a-link')]/span[1]", "purName not found")

                let purName = purName.Trim('/').Trim()

                let hrefT =
                    t.FindElement(By.XPath(".//a[contains(@class,'a-link')]"))

                let href = hrefT.GetAttribute("href")

                let! purNumT =
                    t.findElementWithoutException (".//a[contains(@class,'a-link')]/span[2]", "purNumT not found")

                let purNum =
                    purNumT
                        .Replace("/", "")
                        .Replace("№", "")
                        .RegexDeleteWhitespace()

                let statusT =
                    t.FindElement(By.XPath(".//span[contains(@class,'glyphicon-calendar')]"))

                let status = statusT.GetAttribute("title")
                let! priceT = t.findElementWithoutExceptionOptional (".//b[contains(., '₽')]", "")

                let priceTT =
                    priceT
                        .Replace("&nbsp;", "")
                        .Replace(",", ".")
                        .Replace("₽", "")
                        .RegexDeleteWhitespace()

                let! price = priceTT.Get1Optional(@"^([\d\.]+)")
                let currency = "₽"

                let! nameCusT =
                    t.findElementWithoutExceptionOptional (
                        ".//div[contains(., 'Заказчик:') and @class = 'ng-binding']",
                        ""
                    )

                let nameCus =
                    nameCusT.Replace("Заказчик:", "").Trim()

                let! datePubT =
                    t.findElementWithoutException (
                        ".//small[@class = 'calendarSmall ng-binding']",
                        "datePubT not found"
                    )

                let! datePubR = datePubT.Get1(@"(\d{2}\.\d{2}\.\d{4})", sprintf "datePubR not found %s" datePubT)
                let! datePub = datePubR.DateFromString("dd.MM.yyyy", "datePub not found")
                let dateEnd = DateTime.MinValue

                let! regionT =
                    t.findElementWithoutExceptionOptional (
                        ".//span[contains(@class, 'glyphicon-map-marker')]/parent::div",
                        ""
                    )

                let region = regionT.RegexCutWhitespace()

                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      CusName = nameCus
                      DatePub = datePub
                      DateEnd = dateEnd
                      Status = status
                      Nmck = price
                      Currency = currency
                      RegionName = region }

                listTenders.Add(ten)
                return "ok"
            }

        match result with
        | Success _ -> ()
        | Error e -> Logging.Log.logger e

        ()
