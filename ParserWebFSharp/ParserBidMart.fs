namespace ParserWeb

open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI
open System
open System.Threading
open TypeE

type ResultParserBidMart =
    | SuccessResult of string
    | BadArgument of string

type DefinedBuilder() =
    
    member this.Bind((x : ResultParserBidMart), (rest : string -> ResultParserBidMart)) =
        match x with
        | SuccessResult(x) -> rest x
        | BadArgument(b) -> BadArgument(b)
    
    member this.Return(r : 'T) = r

type ParserBidMart(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(120.)
    let url = "https://www.bidmart.by/"
    let options = ChromeOptions()
    
    let checker (x : string) (t : IWebElement) (b : string) =
        match t.findElementWithoutException (x) with
        | "" -> BadArgument(b)
        | r -> SuccessResult(r)
    
    do 
        options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
    
    override this.Parsing() =
        let driver = new ChromeDriver("/usr/local/bin", options)
        driver.Manage().Timeouts().PageLoad <- timeoutB
        //driver.Manage().Window.Maximize()
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
            (fun dr -> dr.FindElement(By.XPath("//table[@id = 'wlist' and @class = 'dynatable']/tbody")).Displayed) 
        |> ignore
        let tenders = driver.FindElementsByXPath("//table[@id = 'wlist' and @class = 'dynatable']/tbody/tr")
        for t in tenders do
            try 
                this.ParserTenders driver t
            with ex -> Logging.Log.logger ex
    
    member private this.ParserTenders (driver : ChromeDriver) (t : IWebElement) =
        let defined = DefinedBuilder()
        
        let res =
            defined { 
                let! purNum = checker "./td[2]" t "bad purNum"
                let purName = purNum
                let href = "https://www.bidmart.by/"
                let! nmckT = checker "./td[4]" t "bad nmckT"
                let nmck = nmckT.GetNmck()
                let! quantT = checker "./td[3]" t "bad quantity"
                let quant = quantT.GetNmck()
                let datePub = DateTime.Now
                let! dateEndT = checker "./td[6]" t "bad dateEndT"
                let dateEnd =
                    match dateEndT.DateFromString("dd.MM.yyyy") with
                    | Some d -> d
                    | None -> raise <| System.Exception(sprintf "can not parse dateEndT %s, %s" dateEndT url)
                
                let ten =
                    { Href = href
                      PurNum = purNum
                      PurName = purName
                      DatePub = datePub
                      DateEnd = dateEnd
                      Nmck = nmck
                      Quant = quant }
                
                let T = TenderBidMart(set, ten, 102, "ООО «Бидмартс»", "https://www.bidmart.by/")
                T.Parsing()
                return SuccessResult("ok")
            }
        match res with
        | SuccessResult r -> ()
        | BadArgument b -> Logging.Log.logger b
        ()
