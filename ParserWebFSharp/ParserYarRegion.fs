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
    let url = "https://zakupki.yarregion.ru/purchasesoflowvolume-asp/"
    let listTenders = List<YarRegionRec>()
    let options = ChromeOptions()
    
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
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(8000)
        driver.SwitchTo().DefaultContent() |> ignore
        (*let num = driver.FindElements(By.TagName("iframe")).Count
        printfn "%d" num*)
        //driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//a//span[. = 'Таблица']")).Displayed) 
        |> ignore
        try
            driver.FindElement(By.XPath("//label[. = 'Завершен']/preceding-sibling::span/input")).Click()
            Thread.Sleep(1000)
            driver.FindElement(By.XPath("//label[. = 'Отменен']/preceding-sibling::span/input")).Click()
        with ex -> Logging.Log.logger (ex)
        driver.FindElement(By.XPath("//a//span[. = 'Таблица']")).Click()
        Thread.Sleep(10000)
        driver.SwitchTo().DefaultContent() |> ignore
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
            with
            | :? NoSuchElementException as ex -> Logging.Log.logger (ex, driver.Url)
                                                 driver.Close()
            | ex -> Logging.Log.logger (ex, driver.Url)
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
        let c = ref 0
        for t in tenders.Count-1 .. -1 .. 0 do
            try
                if true then
                    this.ParserTenders driver t
                incr c
            with ex -> Logging.Log.logger (ex)
        ()
    
    member private this.GetNextPage (driver : ChromeDriver) (wait : WebDriverWait) =
        for i in 1..3 do
            try 
                driver.SwitchTo().DefaultContent() |> ignore
                driver.SwitchTo().Frame(driver.FindElements(By.TagName("iframe")).[0]) |> ignore
                this.Clicker driver <| "//td[contains(@title, 'Следующая страница')]"
                Thread.Sleep(3000)
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
                let! x = driver.findElementWithoutException ( sprintf "(//span[@class = 'indicator-icon icon-circle-green'])[%d]" t, "red tender")
                
                let el = sprintf "document.querySelectorAll('a.report-link')[%d].click()" t
                driver.SwitchTo().Window(driver.WindowHandles.[0]) |> ignore
                driver.SwitchTo().DefaultContent() |> ignore
                let jse = driver :> IJavaScriptExecutor
                jse.ExecuteScript(el, "") |> ignore
                Thread.Sleep(100)
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
