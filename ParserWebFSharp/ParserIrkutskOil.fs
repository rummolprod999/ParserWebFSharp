namespace ParserWeb
type ParserIrkutskOil(stn : Settings.T) = 
    static member val tenderCount = ref 0
    member public this.Parsing() = 
        printfn "hello"
        ()