// Learn more about F# at http://fsharp.org
namespace ParserWeb
open System
module Start = 
    [<EntryPoint>]
    let main argv =
        let arguments = "irkutskoil"
        if argv.Length = 0 then printf "Bad arguments, use %s" arguments; Environment.Exit(1)
        match argv.[0] with
        | "irkutskoil" -> 
            let settings = Settings.getSettings(IrkutskOil)
            let p = Init(settings, IrkutskOil)
            p.Parsing()
            ()
        | _ -> printf "Bad arguments, use %s" arguments; Environment.Exit(1)
        0 // return an integer exit code
