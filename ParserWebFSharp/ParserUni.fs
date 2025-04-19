namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open TypeE

type ParserUni(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<UniRec>()
    let options = ChromeOptions()
    
    let url =
            "https://unistream.ru/bank/about/tenders/"

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

        driver
            .Navigate()
            .GoToUrl(url)
        driver.SwitchTo().DefaultContent() |> ignore

        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//div[@class = 'content document-list__content']")
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

    member private this.ParserTendersList (driver: ChromeDriver) (t: UniRec) =
        try
            let T =
                TenderUni(set, t, 243, "АО КБ «Юнистрим»", "https://unistream.ru/")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//div[@class = 'content document-list__content']")

        for t in tenders do
            this.ParserTenders t

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let res =
            builder {
                let! purName =
                    i.findElementWithoutException (
                        ".//a",
                        sprintf "purName not found %s" i.Text
                    )

                let! hrefT =
                    i.findWElementWithoutException (
                        ".//a",
                        sprintf "hrefT not found, text the element - %s" i.Text
                    )

                let! href = hrefT.findAttributeWithoutException ("href", "href not found")

                let purNum = Tools.createMD5 href
                let datePub = DateTime.Now

                let! endDateT =
                    i.findElementWithoutException (
                        ".//div[@class = 'description document-list__description']",
                        sprintf "endDateT not found %s" i.Text
                    )

                let! endDateT = endDateT.Get1Optional "(?<=\s)(\d{2}.\d{2}.\d{4})"

                let! dateEnd =
                    endDateT.DateFromString("dd.MM.yyyy", sprintf "dateEnd not found %s %s " href endDateT)

                let tend =
                    { UniRec.Href = href
                      DateEnd = dateEnd
                      DatePub = datePub
                      PurNum = purNum
                      PurName = purName }

                listTenders.Add(tend)
                return ""
            }

        match res with
        | Success _ -> ()
        | Error e when e = "" -> ()
        | Error r -> Logging.Log.logger r

        ()