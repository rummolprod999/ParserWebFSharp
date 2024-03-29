namespace ParserWeb

open AngleSharp.Dom
open OpenQA.Selenium
open OpenQA.Selenium.Chrome

[<AbstractClass>]
type Parser() =
    abstract Parsing: unit -> unit

    member this.GetDefaultFromNull(e: IWebElement) =
        match e with
        | null -> ""
        | _ -> e.Text.Trim()

    member this.GetDefaultFromNullAngle (e: Html.IHtmlDocument) (a: string) =
        let u = e.QuerySelector(a)

        match u with
        | null -> ""
        | x -> x.TextContent.Trim()

    member this.Clicker (driver: ChromeDriver) (findPath: string) =
        let mutable breakIt = true
        let count = ref 0

        while breakIt do
            try
                driver.FindElement(By.XPath(findPath)).Click()
                driver.SwitchTo().DefaultContent() |> ignore
                breakIt <- false
            with
                | ex when
                    ex.Message.Contains("element is not attached")
                    || !count > 30
                    ->
                    breakIt <- false
                    incr count
                | _ -> incr count

        ()

    member this.checkElement(driver: ChromeDriver, f: string) : IWebElement =
        let res =
            try
                driver.FindElement(By.XPath(f))
            with
                | ex -> null

        res
