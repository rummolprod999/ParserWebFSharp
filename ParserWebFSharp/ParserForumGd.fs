namespace ParserWeb

open System
open TypeE
open System.Collections.Generic
open System.Threading
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserForumGd(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let url =
        "https://tender.forum-gd.ru/tender/list"

    let timeoutB = TimeSpan.FromSeconds(60.)
    let listTenders = List<ForumGdRec>()
    let options = ChromeOptions()

    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")
        options.AddArguments("window-size=1920,1080")

    override __.Parsing() =
        let driver =
            new ChromeDriver("/usr/local/bin", options)

        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
        try
            try
                __.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with
                | ex -> Logging.Log.logger ex
        finally
            driver.Quit()

        ()

    member private __.ParserSelen(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)

        driver
            .Navigate()
            .GoToUrl("https://tender.forum-gd.ru/tender/login/")

        Thread.Sleep(5000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//input[@id = 'username']")
                )
                .Displayed)
        |> ignore

        __.Auth(driver)
        driver.SwitchTo().DefaultContent() |> ignore
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//table[@class = 'table table-striped table-hover']/tbody/tr")
                )
                .Displayed)
        |> ignore

        driver.SwitchTo().DefaultContent() |> ignore
        __.ParserListTenders(driver)
        __.GetNextPage driver wait

        for t in listTenders do
            try
                __.ParserTendersList driver t
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.GetNextPage (driver: ChromeDriver) (wait: WebDriverWait) =
        for i in 1..5 do
            try
                driver.SwitchTo().DefaultContent() |> ignore
                let jse = driver :> IJavaScriptExecutor

                jse.ExecuteScript(
                    "var s = document.querySelector('a.blog-page-next'); s.click();",
                    ""
                )
                |> ignore
                Thread.Sleep(3000)
                driver.SwitchTo().DefaultContent() |> ignore

                wait.Until (fun dr ->
                    dr
                        .FindElement(
                            By.XPath("//table[@class = 'table table-striped table-hover']/tbody/tr")
                        )
                        .Displayed)
                |> ignore

                this.ParserListTenders driver
            with
                | ex -> Logging.Log.logger (ex)

        ()
        
    member private __.Auth(driver: ChromeDriver) =
        let wait = WebDriverWait(driver, timeoutB)
        driver.SwitchTo().DefaultContent() |> ignore

        wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//input[@id = 'username']")
                )
                .Displayed)
        |> ignore

        driver
            .FindElement(By.XPath("//input[@id = 'username']"))
            .SendKeys(Settings.UserForumGd)

        driver
            .FindElement(By.XPath("//input[@id = 'password']"))
            .SendKeys(Settings.PassForumGd)

        driver
            .FindElement(By.XPath("//button[@type = 'submit']"))
            .Click()

        Thread.Sleep(3000)
        ()

    member private this.ParserTendersList (driver: ChromeDriver) (t: ForumGdRec) =
        try
            let T =
                TenderForumGd(set, t, 296, "АО «Форум-групп»", "https://tender.forum-gd.ru/", driver)

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private this.ParserListTenders(driver: ChromeDriver) =
        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//table[@class = 'table table-striped table-hover']/tbody/tr")

        for t in tenders do
            this.ParserTenders t

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let res =
            builder {
                let! hrefT = i.findAttributeOrEmpty ("onclick")

                let href =
                    match hrefT with
                    | ""
                    | null -> url
                    | u ->
                        let urlNum =
                            u.Get1FromRegexpOrDefaul(@"tender/list/(\d+)/',")

                        match urlNum with
                        | "" -> url
                        | x -> sprintf "https://tender.forum-gd.ru/tender/list/%s/" x

                let! purName = i.findElementWithoutException ("./td[2]", sprintf "purName not found %s" i.Text)
                let! purNum = i.findElementWithoutException ("./td[1]", sprintf "purNum not found %s" i.Text)
                let! purName1 = i.findElementWithoutException ("./td[4]", sprintf "purName1 not found %s" i.Text)

                let purName =
                    sprintf "%s %s" purName purName1

                let! pwName = i.findElementWithoutException ("./td[3]", sprintf "pwName not found %s" i.Text)
                let! delivPlace = i.findElementWithoutException ("./td[6]", sprintf "delivPlace not found %s" i.Text)
                let! period = i.findElementWithoutException ("./td[8]", sprintf "period not found %s" i.Text)
                let! status = i.findElementWithoutException ("./td[11]", sprintf "status not found %s" i.Text)
                let! pubDateT = i.findElementWithoutException ("./td[7]", sprintf "pubDateT not found %s" i.Text)

                let! datePubT =
                    pubDateT.Get1Optional("^(\d{2}\.\d{2}\.\d{4})")

                let datePub = datePubT.DateFromStringOrCurr("dd.MM.yyyy")
                let! endDateT = i.findElementWithoutException ("./td[9]", sprintf "endDateT not found %s" i.Text)
                let! dateEndT = endDateT.Get1Optional("(\d{2}\.\d{2}\.\d{4})$")

                let dateEnd =
                    dateEndT.DateFromStringOrPubPlus2("dd.MM.yyyy", datePub)

                let ten =
                    { Href = href
                      PurName = purName.Trim()
                      PurNum = purNum
                      Status = status
                      PwName = pwName
                      Period = period
                      DelivPlace = delivPlace
                      DateEnd = dateEnd
                      DatePub = datePub }

                listTenders.Add(ten)
                return ""
            }

        match res with
        | Success _ -> ()
        | Error e when e = "" -> ()
        | Error r -> Logging.Log.logger r

        ()
