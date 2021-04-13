namespace ParserWeb

open System
open TypeE
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserMagnitStroy(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let url ="http://tender.magnitostroy.su/projects-jb/latest-projects-jb/project/listproject"
    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<MagnitStroyRec>()
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
        driver.Navigate().GoToUrl("http://tender.magnitostroy.su/")
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
            dr.FindElement(By.XPath("//form/div[@class = 'row-fluid']")).Displayed) |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        __.ParserListTenders(driver)
        __.GetNextPage driver wait
        for t in listTenders do
            try 
                __.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.GetNextPage (driver : ChromeDriver) (wait : WebDriverWait) =
        for i in 1..5 do
            try 
                driver.SwitchTo().DefaultContent() |> ignore
                this.Clicker driver <| "//a[@title='Вперёд']"
                Thread.Sleep(5000)
                driver.SwitchTo().DefaultContent() |> ignore
                wait.Until(fun dr -> dr.FindElement(By.XPath("//form/div[@class = 'row-fluid']")).Displayed) 
                |> ignore
                this.ParserListTenders driver
            with ex -> Logging.Log.logger (ex)
        ()
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//form/div[@class = 'row-fluid']")
        for t in tenders do
            this.ParserTenders t
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : MagnitStroyRec) =
        try 
            let T = TenderMagnitStroy(set, t, 309, "ООО \"ТРЕСТ МАГНИТОСТРОЙ\"", "http://tender.magnitostroy.su/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
        
    member private this.ParserTenders (i : IWebElement) =
        let builder = TenderBuilder()
        let res = builder {
            let! purName = i.findElementWithoutException(".//h3/a", sprintf "purName not found %s" i.Text)
            let! hrefT = i.findWElementWithoutException(".//h3/a", sprintf "hrefT not found, text the element - %s" i.Text)
            let! href = hrefT.findAttributeWithoutException ("href", "href not found")
            let! purNum = href.Get1 ("detailproject/(\d+)", sprintf "purNum not found %s" href )
            let! nmckT = i.findElementWithoutExceptionOptional(".//div[.='Начальная цена, Руб']/following-sibling::span", sprintf "nmckT not found %s" i.Text)
            let nmck = nmckT.RegexDeleteWhitespace()
            let! contactPerson = i.findElementWithoutExceptionOptional(".//strong[.='Разместил']/following-sibling::a", "")
            let! status = i.findElementWithoutExceptionOptional(".//div[i and contains(., 'Статус')]/span", "")
            let! dateEndT = i.findElementWithoutException(".//div[strong[. = 'Истекает:']]", sprintf "dateEndT not found %s" i.Text)
            let dateEndT = dateEndT.Replace("Истекает:", "").Replace(":", "").Trim()
            let! dateEnd = dateEndT.DateFromString("dd.MM.yyyy", sprintf "endDate not parse %s" dateEndT)
            let ten =
                    { Href = href
                      PurName = purName
                      PurNum = purNum
                      ContactPerson = contactPerson
                      Nmck = nmck
                      Status = status
                      DateEnd = dateEnd
                      DatePub = DateTime.Now }
            listTenders.Add(ten)
            return ""
        }
        match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
        
        ()
    member private __.Auth(driver : ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@id = 'username']")).Displayed) |> ignore
        driver.FindElement(By.XPath("//input[@id = 'username']")).SendKeys(Settings.UserMagnit)
        driver.FindElement(By.XPath("//input[@id = 'password']")).SendKeys(Settings.PassMagnit)
        driver.FindElement(By.XPath("//input[@type = 'submit']")).Click()
        Thread.Sleep(3000)