namespace ParserWeb

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
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
    
    member private this.GetPurNum(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"№(.+) \(" gr1 -> Some(gr1)
        | _ -> None
    
    override this.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try 
            try 
                this.ParserSelen driver
                this.ParserSelenAtom driver
                this.ParserSelenRt driver
                this.ParserSelenVtb driver
                this.ParserSelenRosteh driver
                this.ParserSelenRushidro driver
                this.ParserSelenRosgeo driver
                this.ParserSelenRosseti driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed) 
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver 
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"
        this.Clicker driver "//input[@value = 'Найти']"
        for i in 1..3500 do
            //driver.Keyboard.SendKeys(OpenQA.Selenium.Keys.Down)
            let jse = driver :> IJavaScriptExecutor
            jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
        this.ParserListTenders driver
    
    member private this.ParserSelenAtom(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed) 
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver 
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"
        this.Clicker driver "//span[contains(@class, 'placeholder') and contains(., 'Торговые секции')]"
        this.Clicker driver "//label[. = 'ГК «Росатом»']"
        this.Clicker driver "//input[@value = 'Найти']"
        for i in 1..3500 do
            //driver.Keyboard.SendKeys(OpenQA.Selenium.Keys.Down)
            let jse = driver :> IJavaScriptExecutor
            jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
        this.ParserListTenders driver
    
    member private this.ParserSelenRt(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed) 
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver 
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"
        this.Clicker driver "//span[contains(@class, 'placeholder') and contains(., 'Торговые секции')]"
        this.Clicker driver "//label[. = 'ПАО «Ростелеком» и подведомственных организаций']"
        this.Clicker driver "//input[@value = 'Найти']"
        for i in 1..3500 do
            //driver.Keyboard.SendKeys(OpenQA.Selenium.Keys.Down)
            let jse = driver :> IJavaScriptExecutor
            jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
        this.ParserListTenders driver
    
    member private this.ParserSelenVtb(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed) 
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver 
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"
        this.Clicker driver "//span[contains(@class, 'placeholder') and contains(., 'Торговые секции')]"
        this.Clicker driver "//label[. = 'Группа ВТБ']"
        this.Clicker driver "//input[@value = 'Найти']"
        for i in 1..3500 do
            //driver.Keyboard.SendKeys(OpenQA.Selenium.Keys.Down)
            let jse = driver :> IJavaScriptExecutor
            jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
        this.ParserListTenders driver
    
    member private this.ParserSelenRosteh(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed) 
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver 
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"
        this.Clicker driver "//span[contains(@class, 'placeholder') and contains(., 'Торговые секции')]"
        this.Clicker driver "//label[. = 'ГК «Ростех»']"
        this.Clicker driver "//input[@value = 'Найти']"
        for i in 1..3500 do
            //driver.Keyboard.SendKeys(OpenQA.Selenium.Keys.Down)
            let jse = driver :> IJavaScriptExecutor
            jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
        this.ParserListTenders driver
    
    member private this.ParserSelenRushidro(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed) 
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver 
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"
        this.Clicker driver "//span[contains(@class, 'placeholder') and contains(., 'Торговые секции')]"
        this.Clicker driver "//label[. = 'Группа «РусГидро»']"
        this.Clicker driver "//input[@value = 'Найти']"
        for i in 1..3500 do
            //driver.Keyboard.SendKeys(OpenQA.Selenium.Keys.Down)
            let jse = driver :> IJavaScriptExecutor
            jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
        this.ParserListTenders driver
    
    member private this.ParserSelenRosseti(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed) 
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver 
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"
        this.Clicker driver "//span[contains(@class, 'placeholder') and contains(., 'Торговые секции')]"
        this.Clicker driver "//label[. = 'ПАО «Россети»']"
        this.Clicker driver "//input[@value = 'Найти']"
        for i in 1..3500 do
            //driver.Keyboard.SendKeys(OpenQA.Selenium.Keys.Down)
            let jse = driver :> IJavaScriptExecutor
            jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
        this.ParserListTenders driver
    
    member private this.ParserSelenRosgeo(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed) 
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver 
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"
        this.Clicker driver "//span[contains(@class, 'placeholder') and contains(., 'Торговые секции')]"
        this.Clicker driver "//label[. = 'Холдинг «Росгео»']"
        this.Clicker driver "//input[@value = 'Найти']"
        for i in 1..3500 do
            //driver.Keyboard.SendKeys(OpenQA.Selenium.Keys.Down)
            let jse = driver :> IJavaScriptExecutor
            jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
        this.ParserListTenders driver
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders = driver.FindElementsByXPath("//div[@id = 'table-div']/div[@class = 'w-search-item-b']")
        for t in tenders do
            try 
                this.ParserTenders driver t
            with ex -> Logging.Log.logger ex
    
    member private this.ParserTenders (driver : ChromeDriver) (t : IWebElement) =
        //driver.SwitchTo().DefaultContent() |> ignore
        let purNumT = t.FindElement(By.XPath(".//a[@class = 'g-link']"))
        
        let purNumM =
            match purNumT with
            | null -> raise <| System.NullReferenceException(sprintf "purNum not found in %s" url)
            | x -> x.Text.Trim()
        
        let purNum =
            match this.GetPurNum(purNumM) with
            | None -> raise <| System.NullReferenceException(sprintf "purNum not found in %s" purNumM)
            | Some pr -> pr.Trim()
        
        match purNum with
        | x when purNum.StartsWith("COM") -> this.ParserSelect driver t purNum 42
        | x when purNum.StartsWith("ATOM") -> this.ParserSelect driver t purNum 43
        | x when purNum.StartsWith("RT") -> this.ParserSelect driver t purNum 45
        | x when purNum.StartsWith("VTB") -> this.ParserSelect driver t purNum 46
        | x when purNum.StartsWith("OPK") -> this.ParserSelect driver t purNum 47
        | x when purNum.StartsWith("RH") -> this.ParserSelect driver t purNum 48
        | x when purNum.StartsWith("GEO") -> this.ParserSelect driver t purNum 49
        | x when purNum.StartsWith("ROSSETI") -> this.ParserSelect driver t purNum 50
        | _ -> ()
    
    member private this.ParserSelect (driver : ChromeDriver) (t : IWebElement) (purNum : string) (tFz : int) =
        let hrefT = t.FindElement(By.XPath(".//a[@class = 'g-link']"))
        
        let href =
            match hrefT with
            | null -> raise <| System.NullReferenceException(sprintf "href not found in %s" url)
            | x -> x.GetAttribute("href")
        let PurNameT = t.FindElement(By.XPath(".//div[@class = 'g-proc-name']/a"))
        let purName =
            match PurNameT with
            | null -> raise <| System.NullReferenceException(sprintf "purName not found in %s" url)
            | x -> x.Text.Trim()
        
        let ten =
            { Href = href
              PurNum = purNum
              PurName = purName }
        
        try 
            let T = TenderRossel(set, ten, tFz)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, href)
