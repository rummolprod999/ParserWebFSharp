namespace ParserWeb

open System
open System.Threading
open AngleSharp.Dom
open AngleSharp.Parser.Html
open OpenQA.Selenium.Chrome
open ParserWeb.Download

type ParserIrkutskOil(stn: Settings.T) =
    inherit Parser()
    let options = ChromeOptions()
    do
        //options.AddArguments("headless")
        options.AddArguments("disable-gpu")
        options.AddArguments("no-sandbox")
        options.AddArguments("user-agent=Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/132.0.0.0 Safari/537.36")
    let _ = stn
   

    let url =
        "https://lkk.irkutskoil.ru/active-tenders/list"
   

    override this.Parsing() =
        let driver =
            new ChromeDriver("/usr/local/bin", options)

        driver.Manage().Timeouts().PageLoad <- TimeSpan.FromSeconds(30.)
        //driver.Manage().Window.Maximize()
        try
            try
                driver.Navigate().GoToUrl(url)
                Thread.Sleep(5000)
                let mutable cc = "";
                for cookiesAllCookie in driver.Manage().Cookies.AllCookies do
                    cc <- cc + cookiesAllCookie.Name + "=" + cookiesAllCookie.Value + ";"
                TimedWebClientIrkutsk.cIrk.Value <- cc;
                driver.Manage().Cookies.DeleteAllCookies()
            with
                | ex -> Logging.Log.logger ex
        finally
            driver.Quit()
        let Page =
            Download.DownloadStringIrkutsk url

        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get start page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents.QuerySelectorAll("div.tender.row")

            for t in tens do
                try
                    this.ParsingTender t
                with
                    | ex -> Logging.Log.logger ex

            ()

    member private this.ParsingTender(t: IElement) =
        let urlT =
            match t.QuerySelector("div a") with
            | null -> ""
            | ur -> ur.GetAttribute("href").Trim()

        match urlT with
        | "" -> Logging.Log.logger ("cannot find href on page ", url)
        | url ->
            try
                let urlN = "https://lkk.irkutskoil.ru" + url
                let T = TenderIrkutskOil(stn, urlN)
                T.Parsing()
            with
                | ex -> Logging.Log.logger (ex, url)

        ()
