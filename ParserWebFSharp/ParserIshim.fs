namespace ParserWeb

open System
open TypeE
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserIshim(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let url ="http://etp.ishim-agro.ru/?arrFilterLot_ff%5BARCHIVE_LOT%5D=Y&filter_for_submit=%D0%98%D1%81%D0%BA%D0%B0%D1%82%D1%8C+%D0%BB%D0%BE%D1%82%D1%8B&PAGEN_1=0&SORT_BY=DATE_START&SORT_ORDER=DESC"
    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<OsnovaRec>()
    let options = ChromeOptions()

    do 
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
        //options.AddArguments("window-size=1920,1080")
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
        driver.Navigate().GoToUrl("http://etp.ishim-agro.ru/tenders_detail_noadm.php")
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@name = 'USER_LOGIN']")).Displayed) |> ignore
        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("(//div[@class = 't_lot_info'])[10]")).Displayed) |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        __.ParserListTenders(driver)
        for t in listTenders do
            try 
                __.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : OsnovaRec) =
        try 
            let T = TenderIshim(set, t, 331, "«ISHIM-AGRO»", "http://etp.ishim-agro.ru/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private __.Auth(driver : ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//input[@name = 'USER_LOGIN']")).Displayed) |> ignore
        driver.FindElement(By.XPath("//input[@name = 'USER_LOGIN']")).SendKeys(Settings.UserIshim)
        driver.FindElement(By.XPath("//input[@name = 'USER_PASSWORD']")).SendKeys(Settings.PassIshim)
        driver.FindElement(By.XPath("//input[@name = 'Login']")).Click()
        Thread.Sleep(3000)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//div[@class = 't_lot_info']")
        for t in tenders do
            this.ParserTenders t
        ()
    
    member private this.ParserTenders (i : IWebElement) =
        let builder = TenderBuilder()
        let res = builder {
            let! purName = i.findElementWithoutException(".//a[@class = 't_lot_title']", sprintf "purName not found %s" i.Text)
            let! hrefT = i.findWElementWithoutException(".//a[@class = 't_lot_title']", sprintf "hrefT not found, text the element - %s" i.Text)
            let! href = hrefT.findAttributeWithoutException ("href", "href not found")
            let! purNum = i.findElementWithoutException(".//span[@class = 't_lot_id']", sprintf "purNum not found %s" i.Text)
            let purNum = purNum.Replace("№", "")
            let! pubDateT = i.findElementWithoutException(".//span[. = 'Начало:']/parent::span", sprintf "pubDateT not found %s" i.Text)
            let pubDateT = pubDateT.Replace("Начало:", "").RegexCutWhitespace().Trim()
            let! datePub = pubDateT.DateFromString("dd.MM.yyyy HH:mm:ss", sprintf "datePub not parse %s" pubDateT)
            let! endDateT = i.findElementWithoutException(".//span[. = 'Окончание:']/parent::span", sprintf "endDateT not found %s" i.Text)
            let endDateT = endDateT.Replace("Окончание:", "").RegexCutWhitespace().Trim()
            let dateEnd = endDateT.DateFromStringOrPubPlus2("dd.MM.yyyy HH:mm:ss", datePub)
            let ten =
                    { Href = href
                      PurName = purName.Trim()
                      PurNum = purNum
                      Status = ""
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