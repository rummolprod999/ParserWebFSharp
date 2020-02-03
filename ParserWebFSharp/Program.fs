// Learn more about F# at http://fsharp.org
namespace ParserWeb

open System

module Start =
    [<EntryPoint>]
    let main argv =
        let arguments =
            "irkutskoil, akd, lsr, butb, rossel, neft, slav, aero, stroytorgi, asgor, gosyakut, rostend, chpt, tplus, sibserv, tguru, bidmart, comita, eshoprzd, yarregion, btg, vend, pik, nornic, tenderer, samolet, ariba, beeline, tsm, smart, rtsgen, tj, turk, kg, eten, cislink, petr, mpkz, estorespb, rosagro, neftreg, forscience, volgzmo, rusal"
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
        | "yarregion" ->
            let settings = Settings.getSettings (YarRegion)
            let p = Init(settings, YarRegion)
            p.Parsing()
        | "btg" ->
            let settings = Settings.getSettings (Btg)
            let p = Init(settings, Btg)
            p.Parsing()
        | "vend" ->
            let settings = Settings.getSettings (Vend)
            let p = Init(settings, Vend)
            p.Parsing()
        | "pik" ->
            let settings = Settings.getSettings (Pik)
            let p = Init(settings, Pik)
            p.Parsing()
        | "nornic" ->
            let settings = Settings.getSettings (NorNic)
            let p = Init(settings, NorNic)
            p.Parsing()
        | "tenderer" ->
            let settings = Settings.getSettings (Tenderer)
            let p = Init(settings, Tenderer)
            p.Parsing()
        | "samolet" ->
            let settings = Settings.getSettings (Samolet)
            let p = Init(settings, Samolet)
            p.Parsing()
        | "ariba" ->
            let settings = Settings.getSettings (Ariba)
            let p = Init(settings, Ariba)
            p.Parsing()
        | "beeline" ->
            let settings = Settings.getSettings (Beeline)
            let p = Init(settings, Beeline)
            p.Parsing()
        | "tsm" ->
            let settings = Settings.getSettings (Tsm)
            let p = Init(settings, Tsm)
            p.Parsing()
        | "smart" ->
            let settings = Settings.getSettings (Smart)
            let p = Init(settings, Smart)
            p.Parsing()
        | "rtsgen" ->
            let settings = Settings.getSettings (RtsGen)
            let p = Init(settings, RtsGen)
            p.Parsing()
        | "tj" ->
            let settings = Settings.getSettings (Tj)
            let p = Init(settings, Tj)
            p.Parsing()
        | "turk" ->
            let settings = Settings.getSettings (Turk)
            let p = Init(settings, Turk)
            p.Parsing()
        | "kg" ->
            let settings = Settings.getSettings (Kg)
            let p = Init(settings, Kg)
            p.Parsing()
        | "eten" ->
            let settings = Settings.getSettings (Eten)
            let p = Init(settings, Eten)
            p.Parsing()
        | "cislink" ->
            let settings = Settings.getSettings (CisLink)
            let p = Init(settings, CisLink)
            p.Parsing()
        | "petr" ->
            let settings = Settings.getSettings (Petr)
            let p = Init(settings, Petr)
            p.Parsing()
        | "mpkz" ->
            let settings = Settings.getSettings (Mpkz)
            let p = Init(settings, Mpkz)
            p.Parsing()
        | "estorespb" ->
            let settings = Settings.getSettings (EstoreSpb)
            let p = Init(settings, EstoreSpb)
            p.Parsing()
        | "rosagro" ->
            let settings = Settings.getSettings (RosAgro)
            let p = Init(settings, RosAgro)
            p.Parsing()
        | "neftreg" ->
            let settings = Settings.getSettings (NeftReg)
            let p = Init(settings, NeftReg)
            p.Parsing()
        | "forscience" ->
            let settings = Settings.getSettings (ForScience)
            let p = Init(settings, ForScience)
            p.Parsing()
        | "volgzmo" ->
            let settings = Settings.getSettings (VolgZmo)
            let p = Init(settings, VolgZmo)
            p.Parsing()
        | "rusal" ->
            let settings = Settings.getSettings (Rusal)
            let p = Init(settings, Rusal)
            p.Parsing()
        | _ ->
            printf "Bad arguments, use %s" arguments
            Environment.Exit(1)
        0 // return an integer exit code
