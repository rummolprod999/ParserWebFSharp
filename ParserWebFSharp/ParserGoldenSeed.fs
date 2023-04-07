namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq

type ParserGoldenSeed(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://www.goldenseed.ru/tenders/"

    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<SamaraGoldenSeedRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
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
                    By.XPath("(//div[ul and div and starts-with(@class, 'sc')])[2]")
                )
                .Displayed)
        |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        __.ParserListTenders(driver)

        for t in listTenders do
            try
                __.ParserTendersList driver t
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: SamaraGoldenSeedRec) =
        try
            let T =
                TenderGoldenSeed(set, t, 283, "ГК «Юг Руси»", "https://www.goldenseed.ru/")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//div[ul and div and starts-with(@class, 'sc')]")
        let tensN = tenders.Skip(1).Reverse()
        for t in tensN do
            this.ParserTenders t

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let res =
            builder {
                let href = url
                let! purName =
                    i.findElementWithoutException (
                        "./div[1]",
                        sprintf "purName not found %s" i.Text
                    )

                let purNum = Tools.createMD5 (purName)
                let! dates =
                    i.findElementWithoutException (
                        "./ul/ul[1]",
                        sprintf "dates not found %s num %s" <| i.Text.RegexCutWhitespace() <| purNum
                    )

                let! (pubd, endd) =
                    dates.Get2FromRegexpOptional("(\d{4}\-\d{2}\-\d{2}).+(\d{4}\-\d{2}\-\d{2})", "date and time not found")

                let! datePub = pubd.DateFromString("yyyy-MM-dd", sprintf "datePub not parse %s" pubd)

                let! dateEnd = endd.DateFromString("yyyy-MM-dd", sprintf "dateEnd not parse %s" endd)

                let orgName = "УМТС ООО «МЭЗ Юг Руси»"
                let! status =
                    i.findElementWithoutException (
                        "./ul/ul[2]",
                        sprintf "status not found %s" i.Text
                    )

                let tend =
                    { Href = url
                      PurName = purName
                      PurNum = purNum
                      CusName = orgName
                      Type = ""
                      DateEnd = dateEnd
                      DatePub = datePub
                      Status = status }

                listTenders.Add(tend)
                return ""
            }

        match res with
        | Success _ -> ()
        | Error e when e = "" -> ()
        | Error r -> Logging.Log.logger r

        ()
