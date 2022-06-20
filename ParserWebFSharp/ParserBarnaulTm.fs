namespace ParserWeb

open System
open TypeE
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserBarnaulTm(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "http://www.barnaultransmash.ru/tenders"

    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<RosSelRec>()
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
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//a[@class = 'js-feed-post-link']")
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

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//a[@class = 'js-feed-post-link']")

        for t in tenders do
            this.ParserTenders t

        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: RosSelRec) =
        try
            let T =
                TenderBarnaulTm(set, t, 333, "«Барнаултрансмаш»", "http://www.barnaultransmash.ru/", driver)

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let res =
            builder {
                let! href = i.findAttributeWithoutException ("href", "href not found")
                let! purName = i.findElementWithoutException (".", sprintf "purName not found %s" i.Text)
                let purNum = Tools.createMD5 (href)

                let ten =
                    { RosSelRec.Href = href
                      PurName = purName
                      PurNum = purNum }

                listTenders.Add(ten)
                return ""
            }

        match res with
        | Success _ -> ()
        | Error e when e = "" -> ()
        | Error r -> Logging.Log.logger r

        ()
