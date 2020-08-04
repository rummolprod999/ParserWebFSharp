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

type ParserMedic(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let url = "https://tender.medicina.ru/list.php"
    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = new List<MedicRec>()
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
    
    member private __.Auth(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.SwitchTo().DefaultContent() |> ignore
        driver.FindElement(By.XPath("//a[. = 'Войти']")).Click()
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@name = 'USER_LOGIN']")).Displayed) |> ignore
        driver.FindElement(By.XPath("//input[@name = 'USER_LOGIN']")).SendKeys(Settings.UserMedic)
        driver.FindElement(By.XPath("//input[@name = 'USER_PASSWORD']")).SendKeys(Settings.PassMedic)
        Thread.Sleep(3000)
        driver.FindElement(By.XPath("//input[@value = 'Войти']")).Click()
        Thread.Sleep(3000)
        
        ()
    member private __.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//a[. = 'Войти']")).Displayed) |> ignore
        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        driver.Navigate().GoToUrl("https://tender.medicina.ru/list.php?login=yes&SHOWALL_1=1")
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//table[@class = 'table table-striped table-hover table-condensed']/tbody/tr[position() > 1]")).Displayed) |> ignore
        Thread.Sleep(3000)
        __.ParserListTenders(driver)
        for t in listTenders do
            try 
                __.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    member private this.ParserTendersList (driver : ChromeDriver) (t : MedicRec) =
        try 
            let T = TenderMedic(set, t, 263, "АО «Медицина»", "https://tender.medicina.ru/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//table[@class = 'table table-striped table-hover table-condensed']/tbody/tr[position() > 1]")
        for t in tenders do
            this.ParserTenders t
        ()
    
    member private this.ParserTenders (i : IWebElement) =
        let builder = TenderBuilder()
        let res = builder {
            let! hrefT = i.findWElementWithoutException(".//a[contains(@class, 't_lot_title')]", sprintf "hrefT not found, text the element - %s" i.Text)
            let! href = hrefT.findAttributeWithoutException ("href", "href not found")
            let! purNum = i.findElementWithoutException(".//span[contains(@class, 't_lot_id')]", sprintf "purNum not found %s" i.Text)
            let purNum = purNum.Replace("Лот №:", "").Trim()
            let! purName = i.findElementWithoutException(".//a[contains(@class, 't_lot_title')]", sprintf "purName not found %s" i.Text)
            let! endDateT = i.findElementWithoutException("./td[5]/span/nobr", sprintf "endDateT not found %s" i.Text)
            let! dateEnd = endDateT.DateFromString("dd.MM.yyyy HH:mm:ss", sprintf "endDate not parse %s" endDateT)
            let! pubDateT = i.findElementWithoutException("./td[4]/span/nobr", sprintf "pubDateT not found %s" i.Text)
            let! datePub = pubDateT.DateFromString("dd.MM.yyyy HH:mm:ss", sprintf "datePub not parse %s" pubDateT)
            let! cusName = i.findElementWithoutExceptionOptional(".//span[b[. = 'Компания:']]", sprintf "cusName not found %s" i.Text)
            let cusName = cusName.Replace("Компания:", "").Trim()
            let! person = i.findElementWithoutExceptionOptional("./td[2]/div[@class = 't_lot_meta visible-xs']/span[contains(@class, 't_lot_responsible_fio')]", sprintf "person not found %s" i.Text)
            let person = person.Replace("Ответственный:", "").Trim()
            let! phone = i.findElementWithoutExceptionOptional("./td[2]/div[@class = 't_lot_meta visible-xs']/span[contains(@class, 't_lot_responsible_phone')]", sprintf "phone not found %s" i.Text)
            let phone = phone.Replace("Телефон:", "").Trim()
            let ten =
                    { Href = href
                      PurName = purName
                      PurNum = purNum
                      CusName = cusName
                      CusPerson = person
                      CusPhone = phone
                      DateEnd = dateEnd
                      DatePub = datePub }
            listTenders.Add(ten)
            return ""
        }
        match res with
                | Success _ -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
        
        ()