namespace ParserWeb

open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open System.Threading

type ParserLsr(stn : Settings.T) = 
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(300.)
    let url = "http://zakupki.lsrgroup.ru/search"
    let options = ChromeOptions()
    
    do 
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
    
    override this.Parsing() = 
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- TimeSpan.FromSeconds(120.)
        //driver.Manage().Timeouts().ImplicitWait <- TimeSpan.FromSeconds(120.)
        try 
            try 
                this.ParserSelen driver
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
    
    member private this.ParserSelen(dr : ChromeDriver) = 
        dr.Navigate().GoToUrl(url)
        let wait = WebDriverWait(dr, timeoutB)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[@id = 'tenders_search_btn']")).Displayed) |> ignore
        let btn = dr.FindElement(By.XPath("//a[@id = 'tenders_search_btn']"))
        Thread.Sleep(2000)
        btn.Click()
        for i in 1..20 do
            //wait.Until(fun dr -> dr.FindElement(By.XPath("//a[@id = 'tenders_search_btn']")).Displayed) |> ignore
            Thread.Sleep(2000)
            let btn = dr.FindElement(By.XPath("//a[@id = 'tenders_search_btn']"))
            btn.Click()
            Thread.Sleep(2000)
        let tenders = dr.FindElementsByXPath("//table[@id = 'tenders_search_res']/tbody/tr[position() > 1]")
        for elem in tenders do
            try 
                this.ParserTender elem
            with ex -> Logging.Log.logger ex
        ()
    
    member private this.ParserTender(el : IWebElement) = 
        let hrefT = el.FindElement(By.XPath(".//td[1]/a"))
        
        let href = 
            match hrefT with
            | null -> raise <| System.NullReferenceException(sprintf "href not found in %s" url)
            | x -> x.GetAttribute("href")
        
        let placingWay = this.GetDefaultFromNull <| el.FindElement(By.XPath(".//td[1]/a/b"))
        let purNumT = el.FindElement(By.XPath(".//td[2]//span[contains(concat(' ', @class, ' '), ' number ')]"))
        
        let purNum = 
            match purNumT with
            | null -> raise <| System.NullReferenceException(sprintf "purNum not found in %s" url)
            | x -> x.Text.Replace("№", "").Trim()
        
        let purName = this.GetDefaultFromNull <| el.FindElement(By.XPath(".//td[2]//span[@class = 'name']"))
        let fullNameOrg = this.GetDefaultFromNull <| el.FindElement(By.XPath(".//td[3]//span[@class = 'day']"))
        try 
            let T = TenderLsr(stn, href, purNum, purName, placingWay, fullNameOrg)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, href)
        ()
