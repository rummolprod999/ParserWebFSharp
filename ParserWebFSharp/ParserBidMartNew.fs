namespace ParserWeb

open System
open System.Linq.Expressions
open TypeE
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System.Linq
open System.Collections.Generic
open System.Linq.Expressions
open System.Threading
open System.Web
open Tools
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserBidMartNew(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let url = "https://www.bidmart.by/b2c/sellers/tender"
    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = new List<MoekRec>()
    let options = ChromeOptions()
    do 
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
    override __.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        driver.Manage().Window.Maximize()
        try 
            try 
                __.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()
    
    member private __.Auth(driver : ChromeDriver) =
        driver.FindElement(By.XPath("//input[@type = 'email']")).SendKeys(Settings.UserBidMart)
        driver.FindElement(By.XPath("//input[@type = 'password']")).SendKeys(Settings.PassBidMart)
        Thread.Sleep(3000)
        driver.FindElement(By.XPath("//button[. = 'Войти']")).Click()
        Thread.Sleep(3000)
        
        ()
    member private __.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@type = 'email']")).Displayed) |> ignore
        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        Thread.Sleep(5000000)
        ()