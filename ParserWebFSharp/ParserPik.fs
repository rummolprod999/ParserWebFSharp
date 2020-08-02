namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserPik(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "https://tender.pik.ru/tenders"
    let listTenders = new List<PikRec>()
    let options = ChromeOptions()
    
    do 
        //options.AddArguments("headless")
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
        wait.Until(fun dr -> dr.FindElement(By.XPath("//button[@class = 'close-button ng-star-inserted']")).Displayed) |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        driver.FindElement(By.XPath("//button[@class = 'close-button ng-star-inserted']")).Click()
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until(fun dr -> dr.FindElement(By.XPath("//app-table-container/app-table-row")).Displayed) |> ignore
        this.ParserListTenders driver
        this.GetNextPage driver wait
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
    
    member private this.GetNextPage (driver : ChromeDriver) (wait : WebDriverWait) =
        for i in 1..1 do
            try 
                driver.SwitchTo().DefaultContent() |> ignore
                this.Clicker driver <| "//button[@class = 'next' and contains(., 'Вперед')]"
                Thread.Sleep(5000)
                driver.SwitchTo().DefaultContent() |> ignore
                wait.Until(fun dr -> dr.FindElement(By.XPath("//app-table-container/app-table-row")).Displayed) 
                |> ignore
                this.ParserListTenders driver
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : PikRec) =
        try 
            let T = TenderPik(set, t, 125, "ПАО «Группа Компаний ПИК»", "https://tender.pik.ru/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        let tenders = driver.FindElementsByXPath("//app-table-container/app-table-row[position() > 1]")
        for i = 1 to tenders.Count do
            try 
                this.ParserTenders driver (i + 1)
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTenders (driver : ChromeDriver) (i : int) =
        driver.SwitchTo().DefaultContent() |> ignore
        let t = driver.FindElement(By.XPath(sprintf "//app-table-container/app-table-row[position() ='%d']" i))
        let builder = new TenderBuilder()
        
        let result =
            builder { 
                let! purNameT = t.findElementWithoutException 
                                    ("./app-table-column[1]", sprintf "purNameT not found %s" t.Text)
                let! orgNameT = t.findElementWithoutException 
                                    ("./app-table-column[3]", sprintf "orgNameT not found %s" t.Text)
                let orgName = orgNameT.Replace("Заказчик", "").Trim()
                let! datePubTT = t.findElementWithoutException 
                                     ("./app-table-column[4]", sprintf "dates not found %s" t.Text)
                let! datePubT = datePubTT.Get1("(\d{2}\.\d{2}\.\d{4})", sprintf "datePubT not found %s" datePubTT)
                let! datePub = datePubT.DateFromString("dd.MM.yyyy", sprintf "datePub not parse %s" datePubT)
                let! dateEndTT = t.findElementWithoutException 
                                     ("./app-table-column[5]", sprintf "dates not found %s" t.Text)
                let! dateEndT = dateEndTT.Get1("(\d{2}\.\d{2}\.\d{4})", sprintf "dateEndT not found %s" dateEndTT)
                let! dateEnd = dateEndT.DateFromString("dd.MM.yyyy", sprintf "datePub not parse %s" dateEndT)
                t.Click()
                Thread.Sleep(5000)
                driver.SwitchTo().DefaultContent() |> ignore
                let pop = driver.FindElement(By.XPath("//app-popup-inner"))
                let personT = pop.findElementWithoutException (".//li[contains(b, 'Контактное лицо:')]")
                let person = personT.Replace("Контактное лицо:", "").Trim()
                let! purName = pop.findElementWithoutException (".//h1", sprintf "purNameTT not found %s" pop.Text)
                let href = driver.Url
                let! purNum = href.Get1("/tenders/(.+)", sprintf "purNum not found %s" href)
                let docs = pop.FindElements(By.XPath(".//li/a"))
                let Docs = new List<string>()
                docs |> Seq.iter (fun x -> Docs.Add(x.GetAttribute("href")))
                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub
                      DateEnd = dateEnd
                      Docs = Docs
                      Person = person
                      OrgName = orgName }
                listTenders.Add(ten)
                let close = pop.FindElement(By.XPath("./app-icon[contains(@class, 'close')]"))
                close.Click()
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
        ()
