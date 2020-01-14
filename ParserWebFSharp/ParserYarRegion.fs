namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserYarRegion(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "http://zakupki.yarregion.ru/purchasesoflowvolume-asp/"
    let listTenders = new List<YarRegionRec>()
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
        (*let num = driver.FindElements(By.TagName("iframe")).Count
        printfn "%d" num*)
        driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//table[contains(@class, 'dataview')]//tr[contains(@class, 'rows')]")).Displayed) 
        |> ignore
        this.ParserListTenders driver
        this.GetNextPage driver wait
        for t in listTenders do
            try 
                this.ParserTendersList driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : YarRegionRec) =
        try 
            let T =
                TenderYarRegion
                    (set, t, 114, "Электронный магазин закупок малого объема Ярославской области", 
                     "http://zakupki.yarregion.ru/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        //driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
        let tenders = driver.FindElementsByXPath("//table[contains(@class, 'dataview')]//tr[contains(@class, 'rows')]")
        for t in tenders do
            try 
                this.ParserTenders driver t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.GetNextPage (driver : ChromeDriver) (wait : WebDriverWait) =
        for i in 1..3 do
            try 
                driver.SwitchTo().DefaultContent() |> ignore
                driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
                this.Clicker driver <| "//td[contains(@title, 'Следующая страница')]"
                Thread.Sleep(5000)
                driver.SwitchTo().DefaultContent() |> ignore
                driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
                wait.Until
                    (fun dr -> 
                    dr.FindElement(By.XPath("//table[contains(@class, 'dataview')]//tr[contains(@class, 'rows')]")).Displayed) 
                |> ignore
                this.ParserListTenders driver
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTenders (driver : ChromeDriver) (t : IWebElement) =
        let builder = new TenderBuilder()
        
        let result =
            builder { 
                let! purNameT = t.findElementWithoutException (".//td[1]", "purName not found")
                let purName = purNameT.RegexCutWhitespace()
                let! hrefT = t.findAttributeWithoutException ("onclick", "hrefT not found")
                let! hrefV = hrefT.Get1Optional(@"(/framelk.+TenderID=\d+)")
                let href = String.Format("{0}{1}", "http://zakupki.yarregion.ru", hrefV)
                let! purNum = hrefV.Get1Optional(@"(?:/framelk.+TenderID=(\d+))")
                let! status = t.findElementWithoutExceptionOptional (".//td[5]", "")
                let! nameCus = t.findElementWithoutExceptionOptional (".//td[2]", "")
                let! datePubT = t.findElementWithoutException (".//td[3]", "datePubT not found")
                let! datePub = datePubT.DateFromString("dd.MM.yyyy", "datePub not found")
                let dateEnd =
                    match status with
                    | x when x.Contains("Завершен") -> DateTime.Now
                    | _ -> DateTime.MinValue
                let! priceT = t.findElementWithoutExceptionOptional (".//td[4]", "")
                let price = priceT.Replace("&nbsp;", "").Replace(",", ".").RegexDeleteWhitespace()
                
                let ten =
                    { YarRegionRec.Href = href
                      PurNum = purNum
                      PurName = purName
                      CusName = nameCus
                      DatePub = datePub
                      DateEnd = dateEnd
                      Status = status
                      Nmck = price }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
        ()
