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

    member this.ReturnFrom(x) = x

    member this.Delay(f) = f

    member this.Zero() = Success ""

    member this.Combine(a, b) =
        match a with
        | Success a' -> b()
        | Error e -> Error e

    member this.Run(f) = f()
