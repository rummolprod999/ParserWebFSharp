namespace ParserWeb

type TenderResult<'a> =
    | Success of 'a
    | Error of string

type TenderBuilder() =
    
    member this.Bind(m, f) =
        match m with
        | Error e -> Error e
        | Success a -> f a
    
    member this.Return(x) = Success x
