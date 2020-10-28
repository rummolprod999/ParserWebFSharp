namespace ParserWeb

open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open System.Threading
open TypeE

type ParserButb(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(120.)
    let url = "http://zakupki.butb.by/auctions/reestrauctions.html"
    let options = ChromeOptions()
    
    do 
        options.AddArguments("headless")
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
    
    member private this.ParserSelen(driver : ChromeDriver) =
        let wait = new WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//table[contains(@id, 'auctionList')]//a[contains(., 'Дата публикации')]")).Displayed) 
        |> ignore
        let jse = driver :> IJavaScriptExecutor
        try 
            jse.ExecuteScript
                ("var s = document.querySelector('table.iceDatTbl thead th:nth-child(4) a'); s.click();", "") |> ignore
        with ex -> Logging.Log.logger ex
        //this.Clicker driver "//table[contains(@id, 'auctionList')]//a[contains(., 'Дата публикации')]"
        Thread.Sleep(5000)
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//table[contains(@id, 'auctionList')]//a[contains(., 'Дата публикации')]")).Displayed) 
        |> ignore
        //this.Clicker driver "//table[contains(@id, 'auctionList')]//a[contains(., 'Дата публикации')]"
        try 
            jse.ExecuteScript
                ("var s = document.querySelector('table.iceDatTbl thead th:nth-child(4) a'); s.click();", "") |> ignore
        with ex -> Logging.Log.logger ex
        Thread.Sleep(5000)
        for i in 1..3 do
            match i with
            | m when m > 1 -> 
                for s in 1..20 do
                    try 
                        this.Parser2 driver s m
                    with ex -> Logging.Log.logger ex
            | _ -> 
                for s in 1..20 do
                    try 
                        this.ParserTender driver s
                    with ex -> Logging.Log.logger ex
    
    member private this.Parser2 (driver : ChromeDriver) (s : int) (m : int) =
        let mutable tmp = m
        driver.SwitchTo().DefaultContent() |> ignore
        while tmp <> 1 do
            let wait = new WebDriverWait(driver, timeoutB)
            wait.Until(fun dr -> dr.FindElement(By.XPath("//a[img[@title = 'Следующая страница']]")).Displayed) 
            |> ignore
            this.Clicker driver "//a[img[@title = 'Следующая страница']]"
            driver.SwitchTo().DefaultContent() |> ignore
            tmp <- tmp - 1
            Thread.Sleep(5000)
        this.ParserTender driver s
        ()
    
    member private this.ParserTender (driver : ChromeDriver) (s : int) =
        let wait = new WebDriverWait(driver, timeoutB)
        Thread.Sleep(5000)
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath(String.Format("//table[contains(@id, 'auctionList')]/tbody/tr[{0}]/td[2]/a", s))).Displayed) 
        |> ignore
        let purNumT =
            driver.FindElement
                (By.XPath(String.Format("//table[contains(@id, 'auctionList')]/tbody/tr[{0}]/td[1]/span[1]", s)))
        
        let purNum =
            match purNumT with
            | null -> raise <| System.NullReferenceException(sprintf "purNum not found in %s" url)
            | x -> x.Text.Trim()
        
        //Console.WriteLine(purNum)
        let status =
            this.GetDefaultFromNull 
            <| this.checkElement 
                   (driver, String.Format("//table[contains(@id, 'auctionList')]/tbody/tr[{0}]/td[11]/span[1]", s))
        let datePubT =
            this.GetDefaultFromNull 
            <| driver.FindElement
                   (By.XPath(String.Format("//table[contains(@id, 'auctionList')]/tbody/tr[{0}]/td[4]/span[1]", s)))
        
        let datePubS =
            match datePubT with
            | null -> ""
            | _ -> datePubT.Trim()
        
        let datePub =
            match datePubS.DateFromString("d.MM.yyyy") with
            | Some d -> d
            | None -> 
                match datePubS.DateFromString("d.MM.yyyy HH:mm") with
                | Some d -> d
                | None -> raise <| System.Exception(sprintf "cannot parse datePubS %s, %s" datePubS url)
        
        let endDateT =
            this.GetDefaultFromNull 
            <| driver.FindElement
                   (By.XPath(String.Format("//table[contains(@id, 'auctionList')]/tbody/tr[{0}]/td[8]/span[1]", s)))
        
        let endDateS =
            match endDateT with
            | null -> ""
            | _ -> endDateT.Trim()
        
        let endDate =
            match endDateS.DateFromString("d.MM.yyyy") with
            | Some d -> d
            | None -> 
                match endDateS.DateFromString("d.MM.yyyy HH:mm") with
                | Some d -> d
                | None -> raise <| System.Exception(sprintf "cannot parse endDateS %s, %s" endDateS url)
        
        let biddingDateT =
            this.GetDefaultFromNull 
            <| this.checkElement 
                   (driver, String.Format("//table[contains(@id, 'auctionList')]/tbody/tr[{0}]/td[10]/span[1]", s))
        
        let biddingDateS =
            match biddingDateT with
            | null -> ""
            | _ -> biddingDateT.Trim()
        
        let biddingDate =
            match biddingDateS.DateFromString("d.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> 
                match biddingDateS.DateFromString("d.MM.yyyy") with
                | Some d -> d
                | None -> DateTime.MinValue
        
        //Console.WriteLine(datePub)
        //Console.WriteLine(endDate)
        //Console.WriteLine(biddingDate)
        try 
            let T = TenderButb(stn, purNum, datePub, endDate, biddingDate, driver, wait, s, status)
            T.Parsing()
        with ex -> Logging.Log.logger (ex, url)
        ()
