namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserVend(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)

    let url =
        "http://vendorportal.ru/Market/OfferInvitationOpen"

    let listTenders = List<VendRec>()
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
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath(
                        "//div[contains(@class, 'pagination-container')]/following-sibling::table[@class = 'tableList']//tr[position() > 1]"
                    )
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

        ()

    member private this.ParserTendersList (_: ChromeDriver) (t: VendRec) =
        try
            let T =
                TenderVend(set, t, 116, "Портал поставщиков Южного Урала", "http://vendorportal.ru/")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.GetNextPage (driver: ChromeDriver) (wait: WebDriverWait) =
        for i in 1..10 do
            try
                driver.SwitchTo().DefaultContent() |> ignore

                this.Clicker driver
                <| "//li[@class = 'PagedList-skipToNext']/a"

                Thread.Sleep(5000)
                driver.SwitchTo().DefaultContent() |> ignore

                wait.Until (fun dr ->
                    dr
                        .FindElement(
                            By.XPath(
                                "//div[contains(@class, 'pagination-container')]/following-sibling::table[@class = 'tableList']//tr[position() > 1]"
                            )
                        )
                        .Displayed)
                |> ignore

                this.ParserListTenders driver
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        //driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
        let mutable statement = true
        let mutable count = 3

        while statement && count > 0 do
            driver.SwitchTo().DefaultContent() |> ignore

            let tenders =
                driver.FindElementsByXPath(
                    "//div[contains(@class, 'pagination-container')]/following-sibling::table[@class = 'tableList']//tr[position() > 1]"
                )

            try
                for t in tenders do
                    this.ParserTenders driver t

                statement <- false
            with
                | :? StaleElementReferenceException ->
                    count <- count - 1

                    if count = 0 then
                        Logging.Log.logger ("reload tenders page fail")
                | ex ->
                    Logging.Log.logger (ex)
                    statement <- false

            statement <- false

        ()

    member private this.ParserTenders (_: ChromeDriver) (t: IWebElement) =
        let builder = TenderBuilder()

        let result =
            builder {
                let! purName = t.findElementWithoutException ("./td[4]/a", sprintf "purName not found %s" t.Text)

                let hrefT =
                    t.FindElement(By.XPath("./td[4]/a"))

                let! href = hrefT.findAttributeWithoutException ("href", "href not found")
                let! purNumT = t.findElementWithoutException (".//td[2]", "purNumT not found")

                let purNum =
                    purNumT.Replace("№", "").RegexDeleteWhitespace()

                let orgNameFull =
                    t.findElementWithoutException (
                        "./td[5]"
                    )

                let innCus =
                    orgNameFull.Get1FromRegexpOrDefaul(@"/.*(\d{10})")

                let! pwName =
                    t.findElementWithoutExceptionOptional (
                        ".//td[3]",
                        ""
                    )

                let! dates =
                    t.findElementWithoutException (
                        ".//td[6]",
                        sprintf "dates not found %s" t.Text
                    )

                let! datePubT =
                    dates.Get1(
                        "с\s*(\d{2}\.\d{2}\.\d{4}\s\d{1,2}:\d{2})\sпо",
                        sprintf "datePubT not found %s" dates
                    )

                let! datePub = datePubT.DateFromString("dd.MM.yyyy H:mm", sprintf "datePub not parse %s" datePubT)

                let! dateEndT =
                    dates.Get1("по\s*(\d{2}\.\d{2}\.\d{4}\s\d{1,2}:\d{2})", sprintf "dateEndT not found %s" dates)

                let! dateEnd = dateEndT.DateFromString("dd.MM.yyyy H:mm", sprintf "dateEnd not parse %s" dateEndT)

                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub.AddHours(-2.)
                      DateEnd = dateEnd.AddHours(-2.)
                      PwName = pwName
                      OrgInn = innCus }

                listTenders.Add(ten)
                return "ok"
            }

        match result with
        | Success _ -> ()
        | Error e -> Logging.Log.logger e

        ()
