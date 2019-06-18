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
        options.AddArguments("headless")
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
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore
        //__.Clicker driver "//td[@id = 'next_t_BaseMainContent_MainContent_jqgTrade_toppager']/span"
        let jse = driver :> IJavaScriptExecutor
        try
            jse.ExecuteScript
                ("document.querySelector('#next_t_BaseMainContent_MainContent_jqgTrade_toppager').click()", "") |> ignore
        with ex -> Logging.Log.logger ex
        __.ParserListTenders driver

    member private __.PreparePage(driver: ChromeDriver) =
        driver.Navigate().GoToUrl(spage)
        Thread.Sleep(3000)
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
        for i in 1..100 do
               __.GetContentTender driver i
        ()

    member private __.GetContentTender (driver: ChromeDriver) (i: int) =
        let mutable wh = true
        let count = ref 0
        while wh do
               try
                   driver.SwitchTo().DefaultContent() |> ignore
                   let t = driver.FindElement (By.XPath(sprintf "//table[@class = 'ui-jqgrid-btable']/tbody/tr[@role = 'row'][%d]" i))
                   __.ParserTenders t
                   wh <- false
               with ex -> incr count
                          if !count > 10 then
                              wh <- false
                              Logging.Log.logger (ex)
        ()
    member private this.ParserTenders(i: IWebElement) =
        let builder = new TenderBuilder()
        let result =
            builder {
                let! purNum = i.findElementWithoutException (".//td[9]", sprintf "purNum not found, inner text - %s" i.Text)
                let! hrefT = i.findWElementWithoutException 
                                   (".//td[9]/a", sprintf "hrefT not found %s" i.Text)
                let! href = hrefT.findAttributeWithoutException ("href", "href not found")
                let! purName = i.findElementWithoutException 
                                   (".//td[10]", sprintf "purName not found %s" i.Text)
                let! orgName = i.findElementWithoutException 
                                   (".//td[7]", sprintf "orgName not found %s" i.Text)
                let! regionName = i.findElementWithoutException 
                                   (".//td[8]", sprintf "regionName not found %s" i.Text)
                let! nmckT = i.findElementWithoutException 
                                   (".//td[11]", sprintf "nmckT not found %s" i.Text)
                let nmck = nmckT.GetPriceFromString()
                let! pwName = i.findElementWithoutException 
                                   (".//td[15]", sprintf "pwName not found %s" i.Text)
                let! status = i.findElementWithoutException 
                                   (".//td[16]", sprintf "status not found %s" i.Text)
                let! pubDateT = i.findElementWithoutException 
                                   (".//td[4]", sprintf "pubDateT not found %s" i.Text)
                let pubDateS = match pubDateT.Get1FromRegexp """(\d{2}\.\d{2}\.\d{4}.+\d{2}:\d{2})""" with
                               | Some p -> p
                               | None -> ""
                let! datePub = pubDateS.DateFromString("d.MM.yyyy HH:mm", sprintf "datePub not parse %s" pubDateS)
                let! endDateT = i.findElementWithoutException 
                                   (".//td[13]", sprintf "endDateT not found %s" i.Text)
                let endDateS = match endDateT.Get1FromRegexp """(\d{2}\.\d{2}\.\d{4}.+\d{2}:\d{2})""" with
                               | Some p -> p
                               | None -> ""
                let endDate = match endDateS.DateFromString("d.MM.yyyy HH:mm") with
                              | None -> datePub
                              | Some d -> d
                printfn "date pub - %O" datePub
                printfn "date end - %O" endDate
                printfn "\n\n%s" ""
                return "ok"
            }

        match result with
        | Success _ -> ()
        | Error e -> failwith e
