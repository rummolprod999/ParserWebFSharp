namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserRtsGen(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 2 //TODO change it
    let spage = "https://223.rts-tender.ru/supplier/auction/Trade/Search.aspx"
    let listTenders = new List<RtsGenRec>()
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
        __.Wait <- new WebDriverWait(driver, timeoutB)
        try
            try
                __.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        ()

    member __.Wait with set (value) = wait <- Some(value)


    member __.Wait =
        match wait with
        | None -> failwith "Wait is None"
        | Some w -> w

    member private __.ParserSelen(driver: ChromeDriver) =
        __.PreparePage driver
        __.ParserListTenders driver
        for t in 1..pageC do
            try
                __.GetNextpage driver
            with ex -> Logging.Log.logger (ex)
        for t in listTenders do
            try
                __.ParserTendersList t
            with ex -> Logging.Log.logger (ex)
        ()

    member __.GetNextpage(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        __.Clicker driver "//td[@id = 'next_t_BaseMainContent_MainContent_jqgTrade_toppager']"
        __.ParserListTenders driver

    member private __.PreparePage(driver: ChromeDriver) =
        driver.Navigate().GoToUrl(spage)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        __.Wait.Until (fun dr -> dr.FindElement(By.XPath("//table[@class = 'ui-jqgrid-btable']/tbody/tr[@role = 'row'][10]")).Displayed) |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        __.Clicker driver "//select[@class = 'ui-pg-selbox' and @role = 'listbox']"
        __.Wait.Until (fun dr -> dr.FindElement(By.XPath("//select[@class = 'ui-pg-selbox' and @role = 'listbox']/option[@value = '100']")).Displayed) |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        __.Clicker driver "//select[@class = 'ui-pg-selbox' and @role = 'listbox']/option[@value = '100']"
        ()

    member private __.ParserTendersList(t: RtsGenRec) =
        try
            let T = TenderRtsGen (set, t, 196, "«РТС-тендер» ЗАКУПКИ КОМПАНИЙ С ГОСУДАРСТВЕННЫМ УЧАСТИЕМ И КОММЕРЧЕСКИХ ОРГАНИЗАЦИЙ", "https://223.rts-tender.ru/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()

    member private __.ParserListTenders(driver: ChromeDriver) =
        __.Wait.Until (fun dr -> dr.FindElement(By.XPath("//table[@class = 'ui-jqgrid-btable']/tbody/tr[@role = 'row'][100]")).Displayed) |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//table[@class = 'ui-jqgrid-btable']/tbody/tr[@role = 'row']")
        for t in tenders do
            __.ParserTenders driver t
        ()

    member private this.ParserTenders (driver: ChromeDriver) (i: IWebElement) =
        printfn "%s" i.Text
