// Learn more about F# at http://fsharp.org
namespace ParserWeb

open System

module Start =
    [<EntryPoint>]
    let main argv =
        let arguments =
            "irkutskoil, akd, lsr, butb, rossel, neft, slav, aero, stroytorgi, asgor, gosyakut, rostend, chpt, tplus, sibserv, tguru, bidmart, comita, eshoprzd"
        if argv.Length = 0 then 
            printf "Bad arguments, use %s" arguments
            Environment.Exit(1)
        match argv.[0] with
        | "irkutskoil" -> 
            let settings = Settings.getSettings (IrkutskOil)
            let p = Init(settings, IrkutskOil)
            p.Parsing()
        | "akd" -> 
            let settings = Settings.getSettings (Akd)
            let p = Init(settings, Akd)
            p.Parsing()
        | "lsr" -> 
            let settings = Settings.getSettings (Lsr)
            let p = Init(settings, Lsr)
            p.Parsing()
        | "butb" -> 
            let settings = Settings.getSettings (Butb)
            let p = Init(settings, Butb)
            p.Parsing()
        | "rossel" -> 
            let settings = Settings.getSettings (RosSel)
            let p = Init(settings, RosSel)
            p.Parsing()
        | "neft" -> 
            let settings = Settings.getSettings (Neft)
            let p = Init(settings, Neft)
            p.Parsing()
        | "slav" -> 
            let settings = Settings.getSettings (Slav)
            let p = Init(settings, Slav)
            p.Parsing()
        | "aero" -> 
            let settings = Settings.getSettings (Aero)
            let p = Init(settings, Aero)
            p.Parsing()
        | "stroytorgi" -> 
            let settings = Settings.getSettings (StroyTorgi)
            let p = Init(settings, StroyTorgi)
            p.Parsing()
        | "asgor" -> 
            let settings = Settings.getSettings (Asgor)
            let p = Init(settings, Asgor)
            p.Parsing()
        | "gosyakut" -> 
            let settings = Settings.getSettings (GosYakut)
            let p = Init(settings, GosYakut)
            p.Parsing()
        | "rostend" -> 
            let settings = Settings.getSettings (RosTend)
            let p = Init(settings, RosTend)
            p.Parsing()
        | "chpt" -> 
            let settings = Settings.getSettings (ChPt)
            let p = Init(settings, ChPt)
            p.Parsing()
        | "tplus" -> 
            let settings = Settings.getSettings (Tplus)
            let p = Init(settings, Tplus)
            p.Parsing()
        | "sibserv" -> 
            let settings = Settings.getSettings (SibServ)
            let p = Init(settings, SibServ)
            p.Parsing()
        | "tguru" -> 
            let settings = Settings.getSettings (TGuru)
            let p = Init(settings, TGuru)
            p.Parsing()
        | "bidmart" -> 
            let settings = Settings.getSettings (BidMart)
            let p = Init(settings, BidMart)
            p.Parsing()
        | "comita" -> 
            let settings = Settings.getSettings (Comita)
            let p = Init(settings, Comita)
            p.Parsing()
        | "eshoprzd" -> 
            let settings = Settings.getSettings (EshopRzd)
            let p = Init(settings, EshopRzd)
            p.Parsing()
        | _ -> 
            printf "Bad arguments, use %s" arguments
            Environment.Exit(1)
        0 // return an integer exit code
