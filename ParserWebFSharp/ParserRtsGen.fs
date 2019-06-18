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
        for t in listTenders do
            try
                __.ParserTendersList t
            with ex -> Logging.Log.logger (ex)
        ()

    member private __.ParserTendersList(t: RtsGenRec) =
        try
            let T = TenderRtsGen (set, t, 196, "«РТС-тендер» ЗАКУПКИ КОМПАНИЙ С ГОСУДАРСТВЕННЫМ УЧАСТИЕМ И КОММЕРЧЕСКИХ ОРГАНИЗАЦИЙ", "https://223.rts-tender.ru/")
            T.Parsing()
        with ex -> Logging.Log.logger (ex, t.Href)
        ()
