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
    let mutable two = true

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
        driver.Navigate().GoToUrl(spage)
        Thread.Sleep(5000)
        __.ParserListTenders driver
        for t in 1..pageC do
            try
                __.GetNextpage driver
                ()
            with ex -> Logging.Log.logger (ex)
        ()

    member __.GetNextpage(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
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

    member private __.ParserListTenders(driver: ChromeDriver) =
        __.Wait.Until (fun dr -> dr.FindElement(By.XPath("//table[@class = 'aqua_table']/tbody/tr[not(@valign)][10]")).Displayed) |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        for i in 1..10 do
               __.GetContentTender driver i
        ()

    member private __.GetContentTender (driver: ChromeDriver) (i: int) =
        let mutable wh = true
        let count = ref 0
        while wh do
               try
                   driver.SwitchTo().DefaultContent() |> ignore
                   let t = driver.FindElement(By.XPath(sprintf "//table[@class = 'aqua_table']/tbody/tr[not(@valign)][%d]" i))
                   __.ParserTenders t
                   wh <- false
               with ex -> incr count
                          if !count > 5 then
                              wh <- false
                              Logging.Log.logger (ex)
        ()
    member private this.ParserTenders(i: IWebElement) =
        let builder = new TenderBuilder()
        let result =
            builder {
                let! purNumT = i.findElementWithoutException (".//td[2]/strong", sprintf "purNumT not found, inner text - %s" i.Text)
                let! purNum = purNumT.Get1("(?<=№:)(\d+)", sprintf "purNum not found, inner text - %s" purNumT)
                let! hrefT = i.findWElementWithoutException
                                   (".//td[2]/a[1]", sprintf "hrefT not found, text the element - %s" i.Text)
                let! href = hrefT.findAttributeWithoutException ("href", "href not found")
                let! purName = i.findElementWithoutException
                                   (".//td[2]/a[1]", sprintf "purName not found %s" i.Text)
                let! orgName = i.findElementWithoutException
                                   (".//td[2]/strong[2]", sprintf "orgName not found %s" i.Text)
                let! status = i.findElementWithoutException
                                   (".//td[1]", sprintf "status not found %s" i.Text)
                let! pubDateT = i.findElementWithoutException
                                   (".//td[4]", sprintf "pubDateT not found %s" i.Text)
                let! datePub = pubDateT.DateFromString("yyyy-MM-dd HH:mm:ss", sprintf "datePub not parse %s" pubDateT)
                let! endDateT = i.findElementWithoutException
                                   (".//td[5]", sprintf "endDateT not found %s" i.Text)
                let! dateEnd = endDateT.DateFromString("yyyy-MM-dd HH:mm:ss", sprintf "endDateT not parse %s" endDateT)
                let ten =
                    { status = status
                      Href = href
                      PurNum = purNum
                      PurName = purName
                      OrgName = orgName
                      DatePub = datePub
                      DateEnd = dateEnd }
                listTenders.Add(ten)
                return "ok"
                }
        match result with
        | Success _ -> ()
        | Error e -> failwith e
