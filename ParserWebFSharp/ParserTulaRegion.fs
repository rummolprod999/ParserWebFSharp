namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserTulaRegion(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(60.)

    let url =
        "https://zakupki.tularegion.ru/servisy/zapros-tsen-dlya-zakupok-malogo-obema-2020"

    let listTenders = List<YarRegionRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
    //options.AddArguments("disable-dev-shm-usage")

    override this.Parsing() =
        let driver =
            new ChromeDriver("/usr/local/bin", options)

        driver.Manage().Timeouts().PageLoad <- timeoutB
        driver.Manage().Window.Maximize()

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
        Thread.Sleep(8000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//a//span[. = 'Таблица']")
                )
                .Displayed)
        |> ignore

        try
            driver
                .FindElement(By.XPath("//label[. = 'Завершен']/preceding-sibling::span/input"))
                .Click()

            Thread.Sleep(1000)

            driver
                .FindElement(By.XPath("//label[. = 'Отменен']/preceding-sibling::span/input"))
                .Click()
        with
            | ex -> Logging.Log.logger (ex)

        driver
            .FindElement(By.XPath("//a//span[. = 'Таблица']"))
            .Click()

        Thread.Sleep(10000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//table[@class = 'x-grid-item']")
                )
                .Displayed)
        |> ignore

        this.ParserListTenders driver
        let handlers = driver.WindowHandles

        for t in handlers do
            try
                driver.SwitchTo().Window(t) |> ignore
                driver.Navigate().Refresh()
                driver.SwitchTo().Window(t) |> ignore
                this.ParserTendersList driver listTenders.[0]
                driver.Close()
            with
                | :? NoSuchElementException as ex ->
                    Logging.Log.logger (ex, driver.Url)
                    driver.Close()
                | ex -> Logging.Log.logger (ex, driver.Url)

        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: YarRegionRec) =
        try
            let T =
                TenderTulaRegion(
                    set,
                    t,
                    340,
                    "Электронный магазин закупок малого объема Тульской области",
                    "https://zakupki.tularegion.ru/",
                    driver
                )

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        let tenders =
            driver.FindElementsByXPath("//table[@class = 'x-grid-item']")

        let c = ref 0

        for t in tenders.Count - 1 .. -1 .. 0 do
            try
                if listTenders.Count <= 150 then
                    this.ParserTenders driver t

                incr c
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserTenders (driver: ChromeDriver) (t: int) =
        let builder = TenderBuilder()

        let result =
            builder {
                let! x =
                    driver.findElementWithoutException (
                        sprintf "(//span[@class = 'indicator-icon icon-circle-green'])[%d]" t,
                        "red tender"
                    )

                let el =
                    sprintf "document.querySelectorAll('a.report-link')[%d].click()" t

                driver.SwitchTo().Window(driver.WindowHandles.[0])
                |> ignore

                driver.SwitchTo().DefaultContent() |> ignore
                let jse = driver :> IJavaScriptExecutor
                jse.ExecuteScript(el, "") |> ignore
                Thread.Sleep(100)

                driver.SwitchTo().Window(driver.WindowHandles.[0])
                |> ignore

                let ten = { EmptyField = "" }
                listTenders.Add(ten)
                return "ok"
            }

        match result with
        | Success _ -> ()
        | Error e -> Logging.Log.logger e

        ()
