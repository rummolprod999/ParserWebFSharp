namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserRtsCorp(stn: Settings.T) =
    inherit Parser()
    let set = stn
    let pageC = 3
    let listTenders223 = List<RtsCorpRec>()
    let listTendersCorp = List<RtsCorpRec>()
    let options = ChromeOptions()
    let timeoutB = TimeSpan.FromSeconds(60.)
    let mutable wait = None

    do
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("disable-dev-shm-usage")

    override __.Parsing() =
        let driver =
            new ChromeDriver("/usr/local/bin", options)

        driver.Manage().Timeouts().PageLoad <- timeoutB
        driver.Manage().Window.Maximize()
        __.Wait <- WebDriverWait(driver, timeoutB)

        try
            try
                __.ParserSelen driver
                driver.Manage().Cookies.DeleteAllCookies()
            with
                | ex -> Logging.Log.logger ex
        finally
            driver.Quit()

        for t in listTenders223 do
            try
                __.ParserTendersList223 t
            with
                | ex -> Logging.Log.logger (ex)

        for t in listTendersCorp do
            try
                __.ParserTendersListCorp t
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member __.Wait
        with set (value) = wait <- Some(value)

    member __.Wait =
        match wait with
        | None -> failwith "Wait is None"
        | Some w -> w

    member private __.ParserSelen(driver: ChromeDriver) =
        __.PreparePage driver
        __.ParserListTenders driver

        for t in 1..pageC do
            try
                __.GetNextpage driver t
                ()
            with
                | ex -> Logging.Log.logger (ex.Message)

        ()

    member __.GetNextpage (driver: ChromeDriver) (i: int) =
        driver.SwitchTo().DefaultContent() |> ignore
        let jse = driver :> IJavaScriptExecutor

        let a =
            sprintf "document.querySelector('a[data-pageindex = \"%d\"]').click()" i

        try
            jse.ExecuteScript(a, "") |> ignore
        with
            | ex -> Logging.Log.logger "element is not clickable"

        Thread.Sleep(3000)
        __.ParserListTenders driver

    member private __.ParserTendersList223(t: RtsCorpRec) =
        try
            let T =
                TenderRtsCorp223(set, t, 17, "РТС-тендер", "http://corp.rts-tender.ru")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private __.ParserListTenders(driver: ChromeDriver) =
        __.Wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//table[@class = 'purchase-card'][10]")
                )
                .Displayed)
        |> ignore

        driver.SwitchTo().DefaultContent() |> ignore

        let tenders =
            driver.FindElementsByXPath("//table[@class = 'purchase-card']")

        for t in tenders do
            try
                __.ParserTenders t
            with
                | ex -> Logging.Log.logger (ex)

        ()

    member private this.ParserTenders(i: IWebElement) =
        let builder = TenderBuilder()

        let result =
            builder {
                let! purNum =
                    i.findElementWithoutException (
                        "./tbody/tr[1]//li[contains(., 'Номер на площадке')]//p",
                        sprintf "purNum not found, inner text - %s" i.Text
                    )

                let ind = purNum.LastIndexOf("/")

                let purNum =
                    if ind <> -1 then
                        purNum.Substring(0, ind)
                    else
                        purNum

                let! hrefT =
                    i.findWElementWithoutException (
                        "./tbody/tr[2]/td//span[@class = 'spoiler']/a",
                        sprintf "hrefT not found, text the element - %s" i.Text
                    )

                let! href = hrefT.findAttributeWithoutException ("href", "href not found")

                let! purName =
                    i.findElementWithoutException (
                        ".//a[@class = 'purchase-card__link']/span",
                        sprintf "purName not found %s" i.Text
                    )

                let! orgName =
                    i.findElementWithoutException (
                        ".//p[. = 'Организатор']/following-sibling::p/a/span",
                        sprintf "orgName not found %s" i.Text
                    )

                let! cusName =
                    i.findElementWithoutException (
                        ".//p[. = 'Заказчик']/following-sibling::p/a/span",
                        sprintf "cusName not found %s" i.Text
                    )

                let! regionName =
                    i.findElementWithoutException (
                        ".//p[. = 'Регион']/following-sibling::p",
                        sprintf "regionName not found %s" i.Text
                    )

                let! nmckT =
                    i.findElementWithoutExceptionOptional (
                        ".//h5[contains(., 'Начальная максимальная цена')]/following-sibling::p/strong",
                        sprintf "nmckT not found %s" i.Text
                    )

                let nmck = nmckT.GetPriceFromString()

                let! contrGuaranteeT =
                    i.findElementWithoutExceptionOptional (
                        "./tbody/tr[1]//td[@class = 'column-aside']//div[contains(., 'Обеспечение контракта:')]//p//strong",
                        sprintf "contrGuaranteeT not found %s" i.Text
                    )

                let contrGuarantee =
                    contrGuaranteeT.GetPriceFromString()

                let! applGuaranteeT =
                    i.findElementWithoutExceptionOptional (
                        "./tbody/tr[1]//td[@class = 'column-aside']//div[contains(., 'Обеспечение заявки:')]//p//strong",
                        sprintf "applGuaranteeT not found %s" i.Text
                    )

                let applGuarantee =
                    applGuaranteeT.GetPriceFromString()

                let! status =
                    i.findElementWithoutException (
                        ".//h6[. = 'Статус на площадке']/following-sibling::p",
                        sprintf "status not found %s" i.Text
                    )

                let! currency =
                    i.findElementWithoutExceptionOptional (
                        ".//h5[contains(., 'Начальная максимальная цена')]/following-sibling::p/span",
                        sprintf "currency not found %s" i.Text
                    )

                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      OrgName = orgName
                      CusName = cusName
                      Nmck = nmck
                      RegionName = regionName
                      status = status
                      ContrGuarantee = contrGuarantee
                      ApplGuarantee = applGuarantee
                      Currency = currency }

                if href.Contains("223.rts-tender.ru") then
                    listTenders223.Add(ten)
                else
                    listTendersCorp.Add(ten)

                return "ok"
            }

        match result with
        | Success _ -> ()
        | Error e -> failwith e

    member private __.ParserTendersListCorp(t: RtsCorpRec) =
        try
            let T =
                TenderRtsCorp(set, t, 17, "РТС-тендер", "http://corp.rts-tender.ru")

            T.Parsing()
        with
            | ex -> Logging.Log.logger (ex, t.Href)

        ()

    member private __.PreparePage(driver: ChromeDriver) =
        driver
            .Navigate()
            .GoToUrl(
                "https://corp.rts-tender.ru/?fl=True&SearchForm.State=1&SearchForm.TenderRuleIds=4&SearchForm.CurrencyCode=undefined&FilterData.PageSize=100&FilterData.PageCount=1&FilterData.SortingField=DatePublished&FilterData.SortingDirection=Desc&&FilterData.PageIndex=1"
            )

        Thread.Sleep(3000)
        driver.SwitchTo().DefaultContent() |> ignore

        __.Wait.Until (fun dr ->
            dr
                .FindElement(
                    By.XPath("//table[@class = 'purchase-card'][10]")
                )
                .Displayed)
        |> ignore

        driver.SwitchTo().DefaultContent() |> ignore
