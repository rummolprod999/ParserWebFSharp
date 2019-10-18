namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserNorNic(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let listTenders = new List<NorNicRec>()
    let urls =
        [ "https://www.nornickel.ru/suppliers/tenders/central/"; "https://www.nornickel.ru/suppliers/tenders/local/" ]
    let options = ChromeOptions()
    
    do 
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("ignore-certificate-errors")
        options.AcceptInsecureCertificates <- new Nullable<_>(true)
    
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
        for p in urls do
            try 
                this.ParserUrl p driver
                listTenders.Clear()
            with ex -> Logging.Log.logger (ex)
    
    member private this.ParserUrl (url : string) (driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        let hundr = driver.FindElement(By.XPath("//span[@class = 'select2-selection__rendered']")).Click()
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        //let hundr = driver.FindElement(By.XPath("//li[. = '100']")).Click()
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until(fun dr -> dr.FindElement(By.XPath("//div[@class = 'data-table-scroll']/table/tbody/tr")).Displayed) 
        |> ignore
        this.ParserListTenders driver
        (*for i in 1..5 do
            try 
                this.GetNextPage driver wait
                ()
            with ex -> Logging.Log.logger (ex)*)
        for t in listTenders do
            try 
                this.ParserTendersList t
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTendersList(t : NorNicRec) =
        try 
            let T = TenderNorNic(set, t, 127, "ПАО \"ГМК \"Норильский никель\"", "https://www.nornickel.ru/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
    
    member private this.GetNextPage (driver : ChromeDriver) (wait : WebDriverWait) =
        driver.SwitchTo().DefaultContent() |> ignore
        try 
            let pag =
                driver.FindElement
                    (By.XPath
                         ("//a[contains(@class, 'paginate_button') and contains(@class, 'next') and not(contains(@class, 'disabled'))]"))
            match pag with
            | null -> ()
            | x -> 
                x.Click()
                driver.SwitchTo().DefaultContent() |> ignore
                wait.Until
                    (fun dr -> dr.FindElement(By.XPath("//div[@class = 'data-table-scroll']/table/tbody/tr")).Displayed) 
                |> ignore
                this.ParserListTenders driver
        with ex -> ()
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders = driver.FindElementsByXPath("//div[@class = 'data-table-scroll']/table/tbody/tr")
        for i in tenders do
            try 
                this.ParserTenders i
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserTenders(el : IWebElement) =
        let builder = new TenderBuilder()
        
        let result =
            builder { 
                let! purName = el.findElementWithoutException (".//span/a", sprintf "purName not found %s" el.Text)
                let! href = el.FindElement(By.XPath(".//span/a"))
                              .findAttributeWithoutException("href", sprintf "href not found %s" purName)
                let pwName = Tools.InlineFEWE el "./td[position() = 2]"
                let purNum = Tools.createMD5 purName
                let datePub = DateTime.Now
                let! dateEndT = el.findElementWithoutException 
                                    ("./td[position() = 3]/nobr", sprintf "dates not found %s" purName)
                let! dateEnd = dateEndT.DateFromString("dd.MM.yyyy", sprintf "dateEnd not parse %s" dateEndT)
                let cusName = Tools.InlineFEWE el "./td[position() = 5]"
                let cusAddress = Tools.InlineFEWE el "./td[position() = 6]"
                let orgName = Tools.InlineFEWE el "./td[position() = 7]"
                let personEmail = Tools.InlineFEWE el "./td[position() = 8]/a"
                let personTel = Tools.InlineFEWE el "./td[position() = 8]/span"
                
                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub
                      DateEnd = dateEnd
                      CusName = cusName
                      CusAddress = cusAddress
                      PwName = pwName
                      PersonEmail = personEmail
                      PersonTel = personTel
                      OrgName = orgName }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
