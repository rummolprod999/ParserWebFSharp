namespace ParserWeb

open OpenQA.Selenium
open OpenQA.Selenium.Chrome

[<AbstractClass>]
type Parser() = 
    abstract Parsing : unit -> unit
    
    member this.GetDefaultFromNull(e : IWebElement) = 
        match e with
        | null -> ""
        | _ -> e.Text.Trim()
    
    member this.Clicker (driver : ChromeDriver) (findPath : string) = 
        let mutable breakIt = true
        let count = ref 0
        while breakIt do
            try 
                driver.FindElement(By.XPath(findPath)).Click()
                breakIt <- false
            with
                | ex when ex.Message.Contains("element is not attached") || !count > 30 -> 
                    breakIt <- true
                    incr count
                | _ -> incr count
        ()
