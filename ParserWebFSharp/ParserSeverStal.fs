namespace ParserWeb

open System
open ParserWeb
open TypeE
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserSeverStal(stn: Settings.T) =
    inherit Parser()

    let set = stn

    let url =
        "https://suppliers.severstal.com/procurement/?order=PROPERTY_DEADLINE_DATE&type=asc&pageSize=30#actual"

    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<SeverStalRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
    //options.AddArguments("window-size=1920,1080")
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

    member private __.ParserSelen(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//h2[contains(., 'Текущие')]")
                )
                .Displayed)
        |> ignore

        driver.SwitchTo().DefaultContent() |> ignore
        __.ParserListTenders(driver)
        __.GetNextPage driver wait

        for t in listTenders do
            try
                __.ParserTendersList driver t
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.GetNextPage (driver: ChromeDriver) (wait: WebDriverWait) =
        for i in 1..10 do
            try
                driver.SwitchTo().DefaultContent() |> ignore
                //this.Clicker driver <| "//span[contains(@class, 'slick-next')]"
                let jse = driver :> IJavaScriptExecutor

                try
                    jse.ExecuteScript("var s = document.querySelector('span.arrow.next'); s.click();", "")
                    |> ignore
                with
                    | ex -> Logging.Log.logger ex
                driver.SwitchTo().DefaultContent() |> ignore
                Thread.Sleep(3000)
                driver.SwitchTo().DefaultContent() |> ignore

                wait.Until (fun dr ->
                    dr
                        .FindElement(
                            By.XPath("//table[@class = 'actual-item-table']")
                        )
                        .Displayed)
                |> ignore

                this.ParserListTenders driver
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//a[@class = 'actual-item']")

        for t in tenders do
            this.ParserTenders t

        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: SeverStalRec) =
        try
            let T =
                TenderSeverStal(set, t, 262, "ПАО «Северсталь»", "https://www.severstal.com/")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let res =
            builder {
                let! purName = i.findElementWithoutException (".//td[contains(@class, 'actual-item-table-name')]", sprintf "purName not found %s" i.Text)

                let! href = i.findAttributeWithoutException ("href", "href not found")

                let! purNum =
                    i.findElementWithoutException (
                        ".//td[contains(@class, 'actual-item-table-number')]",
                        sprintf "purNum not found, text the element - %s" i.Text
                    )

                let! dateEndT =
                    i.findElementWithoutException (
                        ".//td[contains(@class, 'actual-item-table-deadline')]",
                        sprintf "dateEndT not found %s" i.Text
                    )

                let dateEndT =
                    dateEndT.RegexCutWhitespace().Trim()

                let dateEndT =
                    dateEndT.Get1FromRegexpOrDefaul(@"(\d{2}\.\d{2}\.\d{4}\s+\d{2}:\d{2}:\d{2})")

                let dateEnd =
                    dateEndT.DateFromStringOrMin("dd.MM.yyyy HH:mm:ss")

                let! addInfo =
                    i.findElementWithoutExceptionOptional (
                        ".//td[contains(@class, 'actual-item-table-city')]",
                        ""
                    )

                let tend =
                    { Href = href
                      PurName = purName
                      PurNum = purNum
                      AddInfo = addInfo
                      DateEnd = dateEnd
                      DatePub = DateTime.Now }

                listTenders.Add(tend)
                return ""
            }

        match res with
        | Success _ -> ()
        | Error e when e = "" -> ()
        | Error r -> Logging.Log.logger r
        ()
