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
    let timeoutB = TimeSpan.FromSeconds(60.)
    let url = "http://zakupki.yarregion.ru/purchasesoflowvolume-asp/"
    let listTenders = List<YarRegionRec>()
    let options = ChromeOptions()
    
    do 
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
    
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
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(15000)
        driver.SwitchTo().DefaultContent() |> ignore
        (*let num = driver.FindElements(By.TagName("iframe")).Count
        printfn "%d" num*)
        //driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//a//span[. = 'Таблица']")).Displayed) 
        |> ignore
        driver.FindElement(By.XPath("//a//span[. = 'Таблица']")).Click()
        Thread.Sleep(15000)
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//table[@class = 'x-grid-item']")).Displayed) 
        |> ignore
        this.ParserListTenders driver
        //this.GetNextPage driver wait
        let handlers = driver.WindowHandles
        for t in handlers do
            try
                driver.SwitchTo().Window(t) |> ignore
                this.ParserTendersList driver listTenders.[0]
                driver.Close()
            with ex -> Logging.Log.logger (ex)
        printfn ""
        ()
    
    member private this.ParserTendersList (driver : ChromeDriver) (t : YarRegionRec) =
        try 
            let T =
                TenderYarRegion
                    (set, t, 114, "Электронный магазин закупок малого объема Ярославской области", 
                     "http://zakupki.yarregion.ru/", driver)
            T.Parsing()
        with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.ParserListTenders(driver : ChromeDriver) =
        //driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
        let tenders = driver.FindElementsByXPath("//table[@class = 'x-grid-item']")
        for t in 0..tenders.Count-1 do
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
    
    member private this.ParserTenders (driver : ChromeDriver) (t : int) =
        let builder = TenderBuilder()
        
        let result =
            builder { 
                let el = sprintf "document.querySelectorAll('a.report-link')[%d].click()" t
                driver.SwitchTo().Window(driver.WindowHandles.[0]) |> ignore
                driver.SwitchTo().DefaultContent() |> ignore
                let jse = driver :> IJavaScriptExecutor
                jse.ExecuteScript(el, "") |> ignore
                Thread.Sleep(1000)
                driver.SwitchTo().Window(driver.WindowHandles.[0]) |> ignore
                let ten =
                    { EmptyField = "" }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success _ -> ()
        | Error e -> Logging.Log.logger e
        ()
