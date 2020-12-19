namespace ParserWeb

open System
open TypeE
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserForumGd(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let url ="https://tender.forum-gd.ru/tender/list"
    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<ForumGdRec>()
    let options = ChromeOptions()

    do 
        options.AddArguments("headless")
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
        driver.Navigate().GoToUrl("https://tender.forum-gd.ru/tender/login/")
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@id = 'username']")).Displayed) |> ignore
        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//table[@class = 'table table-striped table-hover']/tbody/tr")).Displayed) |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        __.ParserListTenders(driver)
        for t in listTenders do
            try 
                __.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private __.Auth(driver : ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@id = 'username']")).Displayed) |> ignore
        driver.FindElement(By.XPath("//input[@id = 'username']")).SendKeys(Settings.UserForumGd)
        driver.FindElement(By.XPath("//input[@id = 'password']")).SendKeys(Settings.PassForumGd)
        driver.FindElement(By.XPath("//button[@type = 'submit']")).Click()
        Thread.Sleep(3000)
        ()
    member private this.ParserTendersList (driver : ChromeDriver) (t : ForumGdRec) =
        try 
            let T = TenderForumGd(set, t, 296, "АО «Форум-групп»", "https://tender.forum-gd.ru/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//table[@class = 'table table-striped table-hover']/tbody/tr")
        for t in tenders do
            this.ParserTenders t
        ()
    
    member private this.ParserTenders (i : IWebElement) =
        let builder = TenderBuilder()
        let res = builder {
            printfn "%O" i.Text
            return ""
        }
        match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
        
        ()