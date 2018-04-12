namespace ParserWeb
open System
open System.Globalization

module TypeE = 
    type System.String with
        member this.DateFromString(pat : string) =
            try
                Some(DateTime.ParseExact(this, pat, CultureInfo.InvariantCulture))
            with ex -> None