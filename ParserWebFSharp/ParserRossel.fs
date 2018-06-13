namespace ParserWeb

open OpenQA.Selenium
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open System.Linq
open System.Threading
open TypeE

type ParserRossel(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(120.)
    let url = "https://www.roseltorg.ru/search/com"
    let options = ChromeOptions()
    
    do 
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
    
    override this.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try 
            try 
                this.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> 
                    dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed) |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"
        this.Clicker driver "//input[@value = 'Найти']"
        for i in 1..10 do
            //driver.Keyboard.SendKeys(OpenQA.Selenium.Keys.Down)
            let jse = driver :> IJavaScriptExecutor
            jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
            ()
        ()
