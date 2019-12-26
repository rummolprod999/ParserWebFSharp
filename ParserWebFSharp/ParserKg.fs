namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserKg(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(60.)
    let url = "http://zakupki.gov.kg/popp/view/order/list.xhtml"
    let listTenders = new List<KgRec>()
    let options = ChromeOptions()
    let pageC = 100
    
    do 
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
    
    override this.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        driver.Manage().Window.Maximize()
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
            dr.FindElement(By.XPath("//tr[@data-rk][10]")).Displayed) |> ignore
        //this.PreparedPage driver
        this.ParserListTenders driver
        for t in 1..pageC do
            try
                this.GetNextpage driver
                ()
            with ex -> Logging.Log.logger (ex)
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex) 
        ()
    
    member private this.PreparedPage(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let jse = driver :> IJavaScriptExecutor
        try
            //jse.ExecuteScript("document.querySelector('div.links button:nth-of-type(2)').click()", "") |> ignore
             this.Clicker driver <| "//select[@class = 'ui-paginator-rpp-options']"
             Thread.Sleep(3000)
             this.Clicker driver <| "//select[@class = 'ui-paginator-rpp-options']/option[5]"
        with ex -> Logging.Log.logger ex
        Thread.Sleep(3000)
        ()
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//tr[@data-rk]")
        for t in tenders do
            this.ParserTenders t
        () 
    member private this.ParserTendersList (driver : ChromeDriver) (t : KgRec) =
        try 
            let T = TenderKg(set, t, 208, "Министерство финансов Кыргызской Республики", "http://zakupki.gov.kg")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    member __.GetNextpage(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let jse = driver :> IJavaScriptExecutor
        try
             jse.ExecuteScript("document.querySelector('a[aria-label=\"Next Page\"]').click()", "") |> ignore
              
        with ex -> Logging.Log.logger ex
        Thread.Sleep(3000)
        __.ParserListTenders driver
    member private this.ParserTenders (i : IWebElement) =
        let builder = new TenderBuilder()
        let result =
            builder {
                let! purNum = i.findElementWithoutException (".//td[1]", sprintf "purNum not found, inner text - %s" i.Text)
                let purNum = purNum.Replace("№", "").Trim()
                let! orgName = i.findElementWithoutException
                                   (".//td[2]/a", sprintf "orgName not found %s" i.Text)
                let! purName = i.findElementWithoutException
                                   (".//td[4]/span[@class='nameTender']", sprintf "purName not found %s" i.Text)
                let! pwName = i.findElementWithoutException
                                   (".//td[5]", sprintf "purName not found %s" i.Text)
                let pwName = pwName.Replace("МЕТОД ЗАКУПОК", "").Trim()
                let nmck = Tools.InlineFEWE i ".//td[6]"
                let nmck = nmck.Replace("ПЛАНИРУЕМАЯ СУММА", "").Trim()
                let nmck = nmck.GetNmck()
                let! pubDateT = i.findElementWithoutException
                                   (".//td[7]", sprintf "pubDateT not found %s" i.Text)
                let pubDateT = pubDateT.Replace("ДАТА ОПУБЛИКОВАНИЯ", "").Trim()
                let! datePub = pubDateT.DateFromString("dd.MM.yyyy HH:mm", sprintf "datePub not parse %s" pubDateT)
                let! endDateT = i.findElementWithoutException
                                   (".//td[8]", sprintf "endDateT not found %s" i.Text)
                let endDateT = endDateT.Replace("СРОК ПОДАЧИ КОНКУРСНЫХ ЗАЯВОК", "").Trim()
                let! dateEnd = endDateT.DateFromString("dd.MM.yyyy HH:mm", sprintf "endDateT not parse %s" endDateT)
                let ten =
                    { KgRec.PwName = pwName
                      Href = "http://zakupki.gov.kg/popp/view/order/list.xhtml"
                      PurNum = purNum
                      PurName = purName
                      OrgName = orgName
                      Nmck = nmck
                      DatePub = datePub
                      DateEnd = dateEnd }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
        ()