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
    let listTenders = List<VtbConnectRec>()
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
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//div[@class= 'list-item-wrapper ng-star-inserted']/div[@class = 'item'][position() = 1]")).Displayed) |> ignore
        Thread.Sleep(3000)
        __.Scroll(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        __.ParserListTenders(driver)
        Thread.Sleep(500000)
        for t in listTenders do
            try 
                __.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()

    member private this.ParserTendersList (driver : ChromeDriver) (t : VtbConnectRec) =
        try 
            let T = TenderVtbConnect(set, t, 290, "ВТБ Бизнес Коннект", "https://www.vtbconnect.ru/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private __.Scroll(driver : ChromeDriver) =
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
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//div[@class= 'list-item-wrapper ng-star-inserted']/div[@class = 'item']")
        for t in tenders do
            this.ParserTenders t
        ()
    
    member private this.ParserTenders (i : IWebElement) =
        let builder = TenderBuilder()
        let res = builder {
            return ""
        }
        match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
        
        ()