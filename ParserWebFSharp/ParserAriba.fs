namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserAriba(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(30.)
    let url = "https://service.ariba.com/Discovery.aw/125007041/aw?awh=r&awssk=iV7OWrxR#b0"
    let listTenders = new List<AribaRec>()
    let options = ChromeOptions()
    let (+%%) (a : DateTime) (b : float) : DateTime = a.AddHours(b)

    do
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")

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
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr ->
            dr.FindElement(By.XPath("//a[span[contains(., 'Я ПРОДАЮ')]]")).Displayed)
        |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        this.Clicker driver """//a[span[contains(., 'Я ПРОДАЮ')]]"""
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr ->
            dr.FindElement(By.XPath("//a[span[contains(., 'Предложения')]]")).Displayed)
        |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        this.Clicker driver """//a[span[contains(., 'Предложения')]]"""
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        wait.Until
            (fun dr ->
            dr.FindElement(By.XPath("//div[@class = 'adse-sortBy']")).Displayed)
        |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        this.Clicker driver """//div[@class = 'adse-sortBy']/select"""
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        this.Clicker driver """//div[@class = 'adse-sortBy']/select/option[contains(., 'Дата - Самая последняя')]"""
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        this.TryGetListTenders driver
        for t in 1..10 do
            try
                this.GetNextpage driver
            with ex -> Logging.Log.logger (ex)
        for t in listTenders do
            try
                this.ParserTendersList t
            with ex -> Logging.Log.logger (ex)
        ()

    member private this.ParserTendersList(t : AribaRec) =
        try
            let T = TenderAriba(set, t, 143, "SAP Ariba", "https://service.ariba.com/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
    member this.GetNextpage(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        this.Clicker driver """//div/a[. = '›']"""
        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore
        this.TryGetListTenders driver
        ()
    member private this.ParserListTenders(driver : ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders =
            driver.FindElementsByXPath("//td[contains(@class, 'ADTableBodyWhite')]")
        for t in tenders do
            this.ParserTenders driver t

        ()

    member this.TryGetListTenders(driver : ChromeDriver) =
        let mutable breakIt = true
        let count = ref 0
        while breakIt do
            try
                driver.SwitchTo().DefaultContent() |> ignore
                this.ParserListTenders driver
                breakIt <- false
            with
                | ex when !count > 3 ->
                    breakIt <- false
                | e -> Logging.Log.logger (e)
                       incr count
    member private this.ParserTenders (driver : ChromeDriver) (i : IWebElement) =
        let builder = new TenderBuilder()

        let result =
            builder {
                let pwName = Tools.InlineFEWE i ".//span[@class = 'adap-ADIL-linkClass']"
                let! purName = i.findElementWithoutException (".//a[@class = 'QuoteSearchResultTitle']", sprintf "purName not found %s" i.Text)
                let purNum = Tools.createMD5 purName
                let OrgName = Tools.InlineFEWE i ".//td[@class = 'adse-ADQSRR-detail']/span[1]"
                let delivPlace = Tools.InlineFEWE i ".//td[contains(., 'Адреса доставки или предоставления услуг')]/following-sibling::td"
                let Categories = Tools.InlineFEWE i ".//td[contains(., 'Категории товаров и услуг')]/following-sibling::td"
                let catList = Categories.Split(",") |> Seq.map (fun x -> x.Trim()) |> Seq.toList
                let catList = purName :: catList
                let priceT = Tools.InlineFEWE i ".//td[contains(@class, 'adse-ADQSRR-title')]/b"
                let price = match priceT.Get1FromRegexp """\$([\d ,\.]+)$""" with
                            | Some p -> p
                            | None -> ""
                let price = price.RegexDeleteWhitespace()
                let! datePubTT = i.findElementWithoutException (".//span[contains(., 'Опубликовано')]/following-sibling::span[1]",
                                      sprintf "datePubTT not found %s" i.Text)
                let datePubT = datePubTT.ReplaceDateAriba().RegexDeleteWhitespace()
                let! datePub = datePubT.DateFromString("d.MM.yyyy", sprintf "datePub not parse %s" datePubT)
                let! dateEndTT = i.findElementWithoutException (".//span[contains(., 'Закрывается')]/following-sibling::span[1]",
                                      sprintf "dateEndTT not found %s" i.Text)
                let dateEndT = dateEndTT.Replace("PST", "").ReplaceDateAriba().Trim()
                let! dateEndP = dateEndT.DateFromString("d.MM.yyyy H:mm", sprintf "dateEndP not parse %s" dateEndT)
                let dateEnd = dateEndP +%% 11.
                let ten =
                    { Href = url
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub
                      DateEnd = dateEnd
                      Nmck = price
                      OrgName = OrgName
                      PwName = pwName
                      Categories = catList
                      DelivPlace = delivPlace }
                listTenders.Add(ten)
                return "ok"
            }
        match result with
        | Success r -> ()
        | Error e -> Logging.Log.logger e
        ()
