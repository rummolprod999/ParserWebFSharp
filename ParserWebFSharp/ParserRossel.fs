namespace ParserWeb

open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open System.Threading
open System.Collections.Generic

type ParserRossel(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "https://www.roseltorg.ru/search/com"
    let urlk = "https://www.roseltorg.ru/procedures/search"
    let listTenders = new List<TenderRossel>()
    let options = ChromeOptions()
    let pageReloader (driver : ChromeDriver) =
                for i in 1..5000 do
                    let jse = driver :> IJavaScriptExecutor
                    jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
                    Thread.Sleep(100)
    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")

    member private this.GetPurNum(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"№(.+) \(" gr1 -> Some(gr1)
        | _ -> None

    member private this.GetPurNumNew(input : string) : string option =
        match input with
        | Tools.RegexMatch1 @"(.+)\s+\(" gr1 -> Some(gr1)
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
        this.Clicker driver "//span[span[contains(., 'Прием заявок')]]/following-sibling::span"
        this.Clicker driver "//li/span[. = 'Работа коммиссии']"
        (*this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"*)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver
        this.ParserListTenders driver

    member private this.ParserSelenAtom(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//span[span[contains(., 'Прием заявок')]]/following-sibling::span"
        this.Clicker driver "//li/span[. = 'Работа коммиссии']"
        //this.Clicker driver "//span[contains(., 'Коммерческие закупки и закупки по 223-ФЗ')]/following-sibling::label/i"
        //this.Clicker driver "//label[contains(., 'Коммерческие закупки и закупки по 223-ФЗ')]"
        this.Clicker driver "//label[. = 'Торговые секции']/following-sibling::div//span[. = 'Select Here']/following-sibling::label"
        this.Clicker driver "//label[. = 'ГК «Росатом»']/parent::li"
        (*this.Clicker driver "//a[contains(., 'Очистить критерии поиска')]"
        this.Clicker driver
            "//span[contains(@class, 'c-inp-select-g-procedure-status-select') and span[contains(@class, 'c-inp-select-opener')]]"
        this.Clicker driver "//span[contains(@class, 'c-inp-option') and @data-index = '0']"*)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver
        this.ParserListTenders driver

    member private this.ParserSelenRt(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//span[span[contains(., 'Прием заявок')]]/following-sibling::span"
        this.Clicker driver "//li/span[. = 'Работа коммиссии']"
        this.Clicker driver "//label[. = 'Торговые секции']/following-sibling::div//span[. = 'Select Here']/following-sibling::label"
        this.Clicker driver "//label[. = 'ПАО «Ростелеком» и подведомственных организаций']/parent::li"
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver
        this.ParserListTenders driver

    member private this.ParserSelenVtb(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//span[span[contains(., 'Прием заявок')]]/following-sibling::span"
        this.Clicker driver "//li/span[. = 'Работа коммиссии']"
        this.Clicker driver "//label[. = 'Торговые секции']/following-sibling::div//span[. = 'Select Here']/following-sibling::label"
        this.Clicker driver "//label[. = 'Группа ВТБ']/parent::li"
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver
        this.ParserListTenders driver

    member private this.ParserSelenRosteh(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//span[span[contains(., 'Прием заявок')]]/following-sibling::span"
        this.Clicker driver "//li/span[. = 'Работа коммиссии']"
        this.Clicker driver "//label[. = 'Торговые секции']/following-sibling::div//span[. = 'Select Here']/following-sibling::label"
        this.Clicker driver "//label[. = 'ГК «Ростех»']/parent::li"
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver
        this.ParserListTenders driver

    member private this.ParserSelenRushidro(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//span[span[contains(., 'Прием заявок')]]/following-sibling::span"
        this.Clicker driver "//li/span[. = 'Работа коммиссии']"
        this.Clicker driver "//label[. = 'Торговые секции']/following-sibling::div//span[. = 'Select Here']/following-sibling::label"
        this.Clicker driver "//label[. = 'Группа «РусГидро»']/parent::li"
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver
        this.ParserListTenders driver

    member private this.ParserSelenRosseti(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//span[span[contains(., 'Прием заявок')]]/following-sibling::span"
        this.Clicker driver "//li/span[. = 'Работа коммиссии']"
        this.Clicker driver "//label[. = 'Торговые секции']/following-sibling::div//span[. = 'Select Here']/following-sibling::label"
        this.Clicker driver "//label[. = 'ПАО «Россети»']/parent::li"
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver
        this.ParserListTenders driver

    member private this.ParserSelenRosgeo(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//span[span[contains(., 'Прием заявок')]]/following-sibling::span"
        this.Clicker driver "//li/span[. = 'Работа коммиссии']"
        this.Clicker driver "//label[. = 'Торговые секции']/following-sibling::div//span[. = 'Select Here']/following-sibling::label"
        this.Clicker driver "//label[. = 'Холдинг «Росгео»']/parent::li"
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver
        this.ParserListTenders driver

    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders = driver.FindElementsByXPath("//div[@id = 'auction_search_results']/div[@class = 'search-results__item']")
        for t in tenders do
            try
                this.ParserTenders driver t
            with ex -> Logging.Log.logger ex
        for ten in listTenders do
           try
                ten.Parsing()
            with ex -> Logging.Log.logger ex
        listTenders.Clear()

    member private this.ParserTenders (driver : ChromeDriver) (t : IWebElement) =
        //driver.SwitchTo().DefaultContent() |> ignore
        let purNumT = t.FindElement(By.XPath(".//a[@class = 'search-results__link']"))

        let purNumM =
            match purNumT with
            | null -> raise <| System.NullReferenceException(sprintf "purNum not found in %s" url)
            | x -> x.Text.Trim()

        let purNum =
            match this.GetPurNumNew(purNumM) with
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
        let hrefT = t.FindElement(By.XPath(".//a[@class = 'search-results__link']"))

        let href =
            match hrefT with
            | null -> raise <| System.NullReferenceException(sprintf "href not found in %s" url)
            | x -> x.GetAttribute("href")
        let PurNameT = t.FindElement(By.XPath(".//div[@class = 'search-results__subject']/a"))
        let purName =
            match PurNameT with
            | null -> raise <| System.NullReferenceException(sprintf "purName not found in %s" url)
            | x -> x.Text.Trim()

        let ten =
            { Href = href
              PurNum = purNum
              PurName = purName }

        let T = TenderRossel(set, ten, tFz)
        listTenders.Add(T)

