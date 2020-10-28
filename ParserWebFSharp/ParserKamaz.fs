namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserKamaz(stn: Settings.T) =
    inherit Parser()
    let _ = stn
    let timeoutB = TimeSpan.FromSeconds(60.)
    let url = "https://web-1c.kamaz.ru/pur/ru_RU/"
    let _ = List<KamazRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")

    override this.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()
    member private this.ParserSelen(driver: ChromeDriver) =
        let _ = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore