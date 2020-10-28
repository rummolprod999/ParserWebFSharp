namespace ParserWeb

open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open System.Threading
open System.Collections.Generic

type ParserRossel(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "https://www.roseltorg.ru/search/com"
    let urlk = "https://www.roseltorg.ru/procedures/search"
    let listTenders = List<TenderRossel>()
    let options = ChromeOptions()
    let pageReloader (driver: ChromeDriver) (x: int) =
                for i in 1..x do
                    let jse = driver :> IJavaScriptExecutor
                    jse.ExecuteScript("window.scrollBy(0,250)", "") |> ignore
                    Thread.Sleep(100)
    do
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")

    member private this.GetPurNum(input: string): string option =
        match input with
        | Tools.RegexMatch1 @"№(.+) \(" gr1 -> Some(gr1)
        | _ -> None

    member private this.GetPurNumNew(input: string): string option =
        match input with
        | Tools.RegexMatch1 @"(.+)\s+\(" gr1 -> Some(gr1)
        | _ -> None

    override this.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        try
            try
                match Settings.RosselNum with
                | "1" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelen driver
                         with ex -> Logging.Log.logger ex
                | "2" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelenAtom driver
                         with ex -> Logging.Log.logger ex
                | "3" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelenRt driver
                         with ex -> Logging.Log.logger ex
                | "4" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelenVtb driver
                         with ex -> Logging.Log.logger ex
                | "5" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelenRosteh driver
                         with ex -> Logging.Log.logger ex
                | "6" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelenRushidro driver
                         with ex -> Logging.Log.logger ex
                | "7" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelenRushidro driver
                         with ex -> Logging.Log.logger ex
                | "8" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelenRosgeo driver
                         with ex -> Logging.Log.logger ex
                | "9" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelenRosseti driver
                         with ex -> Logging.Log.logger ex
                | "10" -> try
                            driver.Manage().Cookies.DeleteAllCookies()
                            this.ParserSelenKim driver
                          with ex -> Logging.Log.logger ex
                 | _ -> ()
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()
        
    member private this.ParsingSelen() =
        use driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParsingSelenKim() =
        use driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelenKim driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParsingSelenAtom() =
        use driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelenAtom driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParsingSelenRt() =
        use driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelenRt driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParsingSelenVtb() =
        use driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelenVtb driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
            
    member private this.ParsingSelenRosteh() =
        use driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelenRosteh driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParsingSelenRushidro() =
        use driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelenRushidro driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParsingSelenRosgeo() =
        use driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelenRosgeo driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParsingSelenRosseti() =
        use driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                this.ParserSelenRosseti driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    member private this.ParserSelen(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        this.Clicker driver "//span[span[contains(., 'Прием заявок')]]/following-sibling::span"
        this.Clicker driver "//li/span[. = 'Работа коммиссии']"
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver 5000
        this.ParserListTenders driver

    member private this.ParserSelenAtom(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        driver.FindElementByCssSelector("#form_procedures_search > div.search-form__advancedfilter > div:nth-child(1) > div > span > span.selection > span") .SendKeys(Keys.Space)
        driver.FindElementByCssSelector("body > span > span > span.select2-search.select2-search--dropdown > input") .SendKeys("ГК «Росатом»")
        Thread.Sleep(5000)
        driver.FindElementByXPath("//span[. = 'ГК «Росатом»']/parent::li").Click()
        Thread.Sleep(3000)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver 1000
        this.ParserListTenders driver

    member private this.ParserSelenRt(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        driver.FindElementByCssSelector("#form_procedures_search > div.search-form__advancedfilter > div:nth-child(1) > div > span > span.selection > span") .SendKeys(Keys.Space)
        driver.FindElementByCssSelector("body > span > span > span.select2-search.select2-search--dropdown > input") .SendKeys("ПАО «Ростелеком» и подведомственных организаций")
        Thread.Sleep(5000)
        driver.FindElementByXPath("//span[. = 'ПАО «Ростелеком» и подведомственных организаций']/parent::li").Click()
        Thread.Sleep(3000)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver 1000
        this.ParserListTenders driver

    member private this.ParserSelenVtb(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        driver.FindElementByCssSelector("#form_procedures_search > div.search-form__advancedfilter > div:nth-child(1) > div > span > span.selection > span") .SendKeys(Keys.Space)
        driver.FindElementByCssSelector("body > span > span > span.select2-search.select2-search--dropdown > input") .SendKeys("Группа ВТБ")
        Thread.Sleep(5000)
        driver.FindElementByXPath("//span[. = 'Группа ВТБ']/parent::li").Click()
        Thread.Sleep(3000)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver 1000
        this.ParserListTenders driver

    member private this.ParserSelenRosteh(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        driver.FindElementByCssSelector("#form_procedures_search > div.search-form__advancedfilter > div:nth-child(1) > div > span > span.selection > span") .SendKeys(Keys.Space)
        driver.FindElementByCssSelector("body > span > span > span.select2-search.select2-search--dropdown > input") .SendKeys("ГК «Ростех»")
        Thread.Sleep(5000)
        driver.FindElementByXPath("//span[. = 'ГК «Ростех»']/parent::li").Click()
        Thread.Sleep(3000)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver 1000
        this.ParserListTenders driver

    member private this.ParserSelenRushidro(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        driver.FindElementByCssSelector("#form_procedures_search > div.search-form__advancedfilter > div:nth-child(1) > div > span > span.selection > span") .SendKeys(Keys.Space)
        driver.FindElementByCssSelector("body > span > span > span.select2-search.select2-search--dropdown > input") .SendKeys("Группа «РусГидро»")
        Thread.Sleep(5000)
        driver.FindElementByXPath("//span[. = 'Группа «РусГидро»']/parent::li").Click()
        Thread.Sleep(3000)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver 100
        this.ParserListTenders driver

    member private this.ParserSelenRosseti(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        driver.FindElementByCssSelector("#form_procedures_search > div.search-form__advancedfilter > div:nth-child(1) > div > span > span.selection > span") .SendKeys(Keys.Space)
        driver.FindElementByCssSelector("body > span > span > span.select2-search.select2-search--dropdown > input") .SendKeys("ПАО «Россети»")
        Thread.Sleep(5000)
        driver.FindElementByXPath("//span[. = 'ПАО «Россети»']/parent::li").Click()
        Thread.Sleep(3000)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver 1000
        this.ParserListTenders driver

    member private this.ParserSelenRosgeo(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        driver.FindElementByCssSelector("#form_procedures_search > div.search-form__advancedfilter > div:nth-child(1) > div > span > span.selection > span") .SendKeys(Keys.Space)
        driver.FindElementByCssSelector("body > span > span > span.select2-search.select2-search--dropdown > input") .SendKeys("Холдинг «Росгео»")
        Thread.Sleep(5000)
        driver.FindElementByXPath("//span[. = 'Холдинг «Росгео»']/parent::li").Click()
        Thread.Sleep(3000)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver 1000
        this.ParserListTenders driver
        
    member private this.ParserSelenKim(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(urlk)
        Thread.Sleep(5000)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[contains(@class, 'btn-advanced-search')]")).Displayed)
        |> ignore
        this.Clicker driver "//a[contains(@class, 'btn-advanced-search')]"
        driver.FindElementByCssSelector("#form_procedures_search > div.search-form__advancedfilter > div:nth-child(1) > div > span > span.selection > span") .SendKeys(Keys.Space)
        driver.FindElementByCssSelector("body > span > span > span.select2-search.select2-search--dropdown > input") .SendKeys("Корпоративный интернет-магазин")
        Thread.Sleep(5000)
        driver.FindElementByXPath("//span[. = 'Корпоративный интернет-магазин']/parent::li").Click()
        Thread.Sleep(3000)
        this.Clicker driver "//button[contains( . , 'Найти')]"
        pageReloader driver 1000
        this.ParserListTenders driver

    member private this.ParserListTenders(driver: ChromeDriver) =
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

    member private this.ParserTenders (driver: ChromeDriver) (t: IWebElement) =
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
        | _ when purNum.StartsWith("COM") -> this.ParserSelect driver t purNum 42
        | _ when purNum.StartsWith("ATOM") -> this.ParserSelect driver t purNum 43
        | _ when purNum.StartsWith("RT") -> this.ParserSelect driver t purNum 45
        | _ when purNum.StartsWith("VTB") -> this.ParserSelect driver t purNum 46
        | _ when purNum.StartsWith("OPK") -> this.ParserSelect driver t purNum 47
        | _ when purNum.StartsWith("RH") -> this.ParserSelect driver t purNum 48
        | _ when purNum.StartsWith("GEO") -> this.ParserSelect driver t purNum 49
        | _ when purNum.StartsWith("ROSSETI") -> this.ParserSelect driver t purNum 50
        | _ when purNum.StartsWith("KIM") -> this.ParserSelect driver t purNum 260
        | _ -> ()

    member private this.ParserSelect (_: ChromeDriver) (t: IWebElement) (purNum: string) (tFz: int) =
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
            { RosSelRec.Href = href
              PurNum = purNum
              PurName = purName }

        let T = TenderRossel(set, ten, tFz)
        if  not <| listTenders.Exists(fun t -> t.PurNum = purNum) then
            listTenders.Add(T)

