namespace ParserWeb

module DocumentBuilderNewton =

    type DocResult<'a> =
        | Success of 'a
        | Error of string

    type DocumentBuilder() =

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
            | Success _ -> b()
            | Error e -> Error e

        member this.Run(f) = f()
        
        member this.TryWith(body, handler) =
            try
                body()
            with e -> handler e