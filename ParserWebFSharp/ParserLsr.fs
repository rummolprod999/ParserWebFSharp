namespace ParserWeb

open System
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System.Linq
open System.Collections.Generic
open System.Linq.Expressions
open System.Threading
open System.Web
open Tools
open TypeE
open System.Threading

type ParserLsr(stn : Settings.T) = 
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(300.)
    let listTenders = new List<TenderLsr>()
    let url = "https://zakupki.lsr.ru/Tenders"
    let options = ChromeOptions()
    
    do 
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("window-size=1920,1080")
    
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
        dr.SwitchTo().DefaultContent() |> ignore
        wait.Until(fun dr -> dr.FindElement(By.XPath("//li[@class = 'next']/a[@href = '#']")).Displayed) |> ignore
        wait.Until(fun dr -> dr.FindElement(By.XPath("//table[@id = 'grid_TenderGridViewModel']/tbody/tr/td[8]")).Displayed) |> ignore
        dr.SwitchTo().DefaultContent() |> ignore
        let tenders = dr.FindElementsByXPath("//table[@id = 'grid_TenderGridViewModel']/tbody/tr")
        for elem in tenders do
            try 
                this.ParserTender elem
            with ex -> Logging.Log.logger ex
        for i in 1..5 do
            dr.SwitchTo().DefaultContent() |> ignore
            wait.Until(fun dr -> dr.FindElement(By.XPath("//li[@class = 'next']/a[@href = '#']")).Displayed) |> ignore
            wait.Until(fun dr -> dr.FindElement(By.XPath("//table[@id = 'grid_TenderGridViewModel']/tbody/tr/td[8]")).Displayed) |> ignore
            Thread.Sleep(2000)
            dr.SwitchTo().DefaultContent() |> ignore
            let btn = dr.FindElement(By.XPath("//li[@class = 'next']/a[@href = '#']"))
            btn.Click()
            Thread.Sleep(2000)
            dr.SwitchTo().DefaultContent() |> ignore
            let tenders = dr.FindElementsByXPath("//table[@id = 'grid_TenderGridViewModel']/tbody/tr")
            dr.SwitchTo().DefaultContent() |> ignore
            for elem in tenders do
                try 
                    this.ParserTender elem
                with ex -> Logging.Log.logger ex
        for t in listTenders do
                try 
                    t.Parsing()
                with ex -> Logging.Log.logger (ex, t.etpUrl)
        ()
    
    member private this.ParserTender(el : IWebElement) = 
        let hrefT = el.FindElement(By.XPath("./td[2]/a"))
        
        let href = 
            match hrefT with
            | null -> raise <| System.NullReferenceException(sprintf "href not found in %s" url)
            | x -> x.GetAttribute("href")
        
        let hrefL = el.FindElement(By.XPath("./td[1]/a"))
        
        let hrefLot = 
            match hrefL with
            | null -> raise <| System.NullReferenceException(sprintf "href not found in %s" url)
            | x -> x.GetAttribute("href")
        
        let placingWay = this.GetDefaultFromNull <| el.FindElement(By.XPath("./td[8]"))
        let purNumT = el.FindElement(By.XPath("./td[1]/a"))
        
        let purNum = 
            match purNumT with
            | null -> raise <| System.NullReferenceException(sprintf "purNum not found in %s" url)
            | x -> x.Text.Trim()
        
        let purName = this.GetDefaultFromNull <| el.FindElement(By.XPath("./td[2]/a"))
        let fullNameOrg = this.GetDefaultFromNull <| el.FindElement(By.XPath("./td[6]"))
        listTenders.Add(TenderLsr(stn, href, purNum, purName, placingWay, fullNameOrg, hrefLot))
        ()
    
        
