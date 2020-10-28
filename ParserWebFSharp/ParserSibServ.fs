namespace ParserWeb

open System
open System.Collections.Generic
open System.Threading
open TypeE
open OpenQA.Selenium
open OpenQA.Selenium.Chrome
open OpenQA.Selenium.Support.UI

type ParserSibServ(stn : Settings.T) =
    inherit Parser()
    let set = stn
    let timeoutB = TimeSpan.FromSeconds(120.)
    let url = "https://tp.sibserv.com/tenders.php"
    let options = ChromeOptions()
    
    do 
        options.AddArguments("headless")
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
        let wait = WebDriverWait(driver, timeoutB)
        driver.Navigate().GoToUrl(url)
        Thread.Sleep(5000)
        wait.Until
            (fun dr -> 
            dr.FindElement(By.XPath("//div[@class = 'defc']/div[@class = 'line']/following-sibling::p[1]")).Displayed) 
        |> ignore
        driver.SwitchTo().DefaultContent() |> ignore
        let tenders = driver.FindElementsByXPath("//div[@class = 'defc']/div[@class = 'line']")
        for t in tenders do
            try 
                this.ParserTenders driver t
            with ex -> Logging.Log.logger (ex)
    
    member private this.ParserTenders (_ : ChromeDriver) (t : IWebElement) =
        let purNum =
            match t.findElementWithoutException ("./following-sibling::p[1]") with
            | "" -> raise <| NullReferenceException(sprintf "purNum not found in %s" url)
            | x -> x.Replace("Лот №", "")
        
        let purName =
            match t.findElementWithoutException ("./following-sibling::p[2]") with
            | "" -> raise <| NullReferenceException(sprintf "purNume not found in %s" url)
            | x -> x
        
        let href =
            match t.FindElement(By.XPath("./following-sibling::p/a[contains(., 'подробнее')]")) with
            | null -> raise <| NullReferenceException(sprintf "href not found in %s" url)
            | x -> x.GetAttribute("href")
        
        let requesttext = "Запрос на участие в лоте"
        
        let hrefrequest =
            match t.FindElement(By.XPath("./following-sibling::p/a[contains(., 'Запрос')]")) with
            | null -> raise <| NullReferenceException(sprintf "href not found in %s" url)
            | x -> x.GetAttribute("href")
        
        let listDoc = List<DocSibServ>()
        listDoc.Add({ name = requesttext
                      url = hrefrequest })
        let dateT = this.GetDefaultFromNull <| t.FindElement(By.XPath("./following-sibling::p[4]"))
        
        let datePubT =
            match dateT.RegexCutWhitespace().Get1FromRegexp @"Дата публикации лота:\s(\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2})" with
            | Some x -> x.Trim()
            | None -> raise <| NullReferenceException(sprintf "datePubT not found in %s %s" url dateT)
        
        let datePub =
            match datePubT.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> raise <| Exception(sprintf "cannot parse datePubT %s, %s" datePubT url)
        
        let dateEndT =
            match dateT.RegexCutWhitespace()
                       .Get1FromRegexp @"Дата окончания приема заявок:\s(\d{2}\.\d{2}\.\d{4}\s\d{2}:\d{2})" with
            | Some x -> x.Trim()
            | None -> raise <| NullReferenceException(sprintf "dateEndT not found in %s %s" url dateT)
        
        let dateEnd =
            match dateEndT.DateFromString("dd.MM.yyyy HH:mm") with
            | Some d -> d
            | None -> raise <| Exception(sprintf "cannot parse dateEndT %s, %s" dateEndT url)
        
        let ten =
            { Href = href
              PurNum = purNum
              PurName = purName
              DatePub = datePub
              DateEnd = dateEnd
              DocList = listDoc }
        
        try 
            let T = TenderSibServ(set, ten, 99, "АО «Сибирская Сервисная Компания»", "https://tp.sibserv.com")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, href)
        ()
