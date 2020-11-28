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
    let url ="https://www.vtbconnect.ru/login?redirect=https://www.vtbconnect.ru/trades/vtb/"
    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<BidzaarRec>()
    let options = ChromeOptions()

    do 
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
        options.AddArguments("window-size=1920,1080")
    override __.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try 
            try 
                __.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()
    
    member private __.ParserSelen(driver : ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@placeholder = 'Введите имя пользователя']")).Displayed) |> ignore
        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        Thread.Sleep(500000)
        ()
    
    member private __.Auth(driver : ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@placeholder = 'Введите имя пользователя']")).Displayed) |> ignore
        driver.FindElement(By.XPath("//input[@placeholder = 'Введите имя пользователя']")).SendKeys(Settings.UserVtb)
        driver.FindElement(By.XPath("//input[@placeholder = 'Введите пароль']")).SendKeys(Settings.PassVtb)
        Thread.Sleep(3000)
        driver.FindElement(By.XPath("//button[@type = 'submit']")).Click()
        Thread.Sleep(3000)
        ()