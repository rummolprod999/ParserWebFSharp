namespace ParserWeb

open AngleSharp.Dom
open AngleSharp.Parser.Html
open System
open System.Linq
open System.Threading
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open OpenQA.Selenium

type ParserLsr(stn : Settings.T) = 
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromMilliseconds(30000.)
    let url = "http://zakupki.lsrgroup.ru/search"
    let options = ChromeOptions()

    do 
        //options.AddArguments("headless")
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

    
    member private this.ParserSelen(dr:  ChromeDriver) =
        dr.Navigate().GoToUrl(url)
        let wait = WebDriverWait(dr, timeoutB)
        wait.Until(fun dr -> dr.FindElement(By.XPath("//a[@id = 'tenders_search_btn']")).Displayed) |> ignore
        let btn = dr.FindElement(By.XPath("//a[@id = 'tenders_search_btn']"))
        btn.Click()
        for i in 1..2 do
            wait.Until(fun dr -> dr.FindElement(By.XPath("//a[@id = 'tenders_search_btn']")).Displayed) |> ignore
            let btn = dr.FindElement(By.XPath("//a[@id = 'tenders_search_btn']"))
            btn.Click()
            Thread.Sleep(1000)
            
        let tenders = dr.FindElementsByXPath("//table[id = 'tenders_search_res']/tbody/tr[position() > 1]")
        for elem in tenders do 
            try 
                this.ParserTender elem
            with ex -> Logging.Log.logger ex
        ()
    
    member private this.ParserTender(el: IWebElement) =
        let hrefT = el.FindElement(By.XPath("//td[1]/a"))
        let hrefS = match hrefT with 
                    | null -> ""
                    | x -> x.GetAttribute("href")
        printfn "%s" hrefS
        ()