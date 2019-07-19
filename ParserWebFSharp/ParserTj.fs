namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserTj(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 5 //TODO change
    let spage = "http://test.zakupki.gov.tj/reestr-zakazov-v-elektronnoy-forme/"
    let listTenders = new List<TjRec>()
    let options = ChromeOptions()
    let timeoutB = TimeSpan.FromSeconds(60.)
    let mutable wait = None

    do
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")

    override __.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        driver.Manage().Window.Maximize()
        __.Wait <- new WebDriverWait(driver, timeoutB)
        try
            try
                __.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        for t in listTenders do
            try
                __.ParserTendersList t
            with ex -> Logging.Log.logger (ex)
        ()
        
    member __.Wait with set (value) = wait <- Some(value)


    member __.Wait =
        match wait with
        | None -> failwith "Wait is None"
        | Some w -> w
    
    member private __.ParserSelen(driver: ChromeDriver) =
        __.ParserListTenders driver
        for t in 1..pageC do
            try
                __.GetNextpage driver
                ()
            with ex -> Logging.Log.logger (ex)
        ()
    
    member __.GetNextpage(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let mutable two = true
        let jse = driver :> IJavaScriptExecutor
        try
            if two then
                jse.ExecuteScript("document.querySelector('div.links button:nth-of-type(2)').click()", "") |> ignore
                two <- false
            else jse.ExecuteScript("document.querySelector('div.links button:nth-of-type(4)').click()", "") |> ignore
                
        with ex -> Logging.Log.logger ex
        Thread.Sleep(3000)
        __.ParserListTenders driver
    
    member private __.ParserTendersList(t: TjRec) =
        try
            let T = TenderTj (set, t, 203, "Агентство по государственным закупкам товаров, работ и услуг при Правительстве Республики Таджикистан", "http://test.zakupki.gov.tj/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    
    member private __.ParserListTenders(driver: ChromeDriver) = ()