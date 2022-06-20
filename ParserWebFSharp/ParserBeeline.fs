namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserBeeline(stn: Settings.T) as __ =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)

    let url =
        "https://moskva.beeline.ru/business/partners/tenders/"

    let listTenders = List<BeelineRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")

    override __.Parsing() =
        let driver =
            new ChromeDriver("/usr/local/bin", options)

        driver.Manage().Timeouts().PageLoad <- timeoutB

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
                    By.XPath("//a[@class = 'ProductList_product_ghAg']")
                )
                .Displayed)
        |> ignore

        __.ParserListTenders driver

        for t in listTenders do
            try
                __.ParserTendersList t
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private __.ParserTendersList(t: BeelineRec) =
        try
            let T =
                TenderBeeline(set, t, 135, "ПАО «ВымпелКом»", "https://beeline.ru")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private __.ParserListTenders(driver: ChromeDriver) =
        let tenders =
            driver.FindElementsByXPath("//a[@class = 'ProductList_product_ghAg']")

        for t in tenders do
            __.ParserTenders t

        ()

    member private __.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let result =
            builder {
                let! href = i.findAttributeWithoutException ("href", "href not found")

                let! purName =
                    i.findElementWithoutException (
                        ".//div[@class = 'ProductList_description_2eFc']",
                        sprintf "purName not found %s" i.Text
                    )

                let purNum = Tools.createMD5 href

                let! datePubT =
                    i.findElementWithoutException (
                        ".//div[@class = 'ProductList_date_A5k7']",
                        sprintf "datePubT not found %s" i.Text
                    )

                let! datePub = datePubT.DateFromString("dd.MM.yyyy", sprintf "datePub not parse %s" datePubT)

                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub }

                listTenders.Add(ten)
                return "ok"
            }

        match result with
        | Success _ -> ()
        | Error e -> Logging.Log.logger e

        ()
