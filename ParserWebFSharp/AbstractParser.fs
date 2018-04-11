namespace ParserWeb

[<AbstractClass>]
type Parser = 
    val mutable etpName: string
    val mutable etpUrl: string
    new(n, u) = {etpName = n; etpUrl = u}
    member this.EtpName with get() = this.etpName
    member this.EtpUrl with get() = this.etpUrl
    abstract member Parsing : unit -> unit