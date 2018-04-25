namespace ParserWeb

open OpenQA.Selenium

[<AbstractClass>]
type Parser() = 
    abstract member Parsing : unit -> unit
    member this.GetDefaultFromNull(e : IWebElement) = 
            match e with
            | null -> ""
            | _ -> e.Text.Trim()
    
        