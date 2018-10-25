namespace ParserWeb

open System
open System.Collections.Generic
open System.Linq
open System.Threading
open System.Text.RegularExpressions
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserBtg(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "http://www.btg.by/tenders/"
    let listTenders = new List<BtgRec>()
    let options = ChromeOptions()
    
    do 
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
    
    override this.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        try 
            try 
                this.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()
    
    member private this.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//div[contains(@class, 'dataTables_paginate')]/following-sibling::table/tbody")).Displayed) 
        |> ignore
        this.ParserListTenders driver
        this.GetNextPage driver wait
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : BtgRec) =
        try 
            let T = TenderBtg(set, t, 115, "ОАО «Газпром трансгаз Беларусь»", "http://www.btg.by")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.GetNextPage (driver : ChromeDriver) (wait : WebDriverWait) =
        for i in 2..2 do
            try 
                driver.SwitchTo().DefaultContent() |> ignore
                this.Clicker driver <| sprintf "//span[@class = 'paginate_button' and . = '%d']" i
                Thread.Sleep(5000)
                driver.SwitchTo().DefaultContent() |> ignore
                wait.Until
                    (fun dr -> dr.FindElement(By.XPath("//div[contains(@class, 'dataTables_paginate')]/following-sibling::table/tbody")).Displayed) |> ignore
                this.ParserListTenders driver
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        //driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
        let tenders =
            driver.FindElementsByXPath
                ("//div[contains(@class, 'dataTables_paginate')]/following-sibling::table/tbody/tr")
        for t in tenders do
            try 
                this.ParserTenders driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTenders (driver : ChromeDriver) (t : IWebElement) =
        let builder = new TenderBuilder()
        
        let result =
            builder { 
                let! purName = t.findElementWithoutException (".//td[2]/p[2]", "purName not found")
                let! purNum = t.findElementWithoutException (".//td[2]/p[1]/a/span", "purNum not found")
                let hrefT = t.FindElement(By.XPath(".//td[2]/p[1]/a"))
                let! href = hrefT.findAttributeWithoutException ("href", "href not found")
                let! orgName = t.findElementWithoutExceptionOptional (".//td[4]/p", "")
                let! nameCus = t.findElementWithoutExceptionOptional (".//td[3]/p", "")
                let datePub = DateTime.Now
                let! dateEndT = t.findElementWithoutException (".//td[1]/p/span[@class = 'tender_date']", "dateEndT not found")
                let! (dat, tm) = dateEndT.Get2FromRegexpOptional("(\d{2}\.\d{2}\.\d{4}).+(\d{2}:\d{2})", "date and time not found")
                let dateEndV = sprintf "%s %s" dat tm
                let! dateEnd = dateEndV.DateFromString("dd.MM.yyyy HH:mm", sprintf "dateEnd not parse %s" dateEndV)
                let! pwName = t.findElementWithoutExceptionOptional (".//td[2]/p[3]/span[@class = 'tender_type']", "")
                
                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      CusName = nameCus
                      DatePub = datePub
                      DateEnd = dateEnd
                      PwName = pwName
                      OrgName = orgName }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
        ()
