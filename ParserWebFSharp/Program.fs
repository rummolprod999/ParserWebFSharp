﻿namespace ParserWeb

open System

module Start =
    [<EntryPoint>]
    let main argv =
        let arguments =
            "irkutskoil, akd, lsr, butb, rossel, neft, slav, aero, stroytorgi, asgor, gosyakut, rostend, chpt, tplus, sibserv, tguru, bidmart, comita, eshoprzd, yarregion, btg, vend, pik, nornic, tenderer, samolet, ariba, beeline, tsm, smart, rtsgen, tj, turk, kg, eten, cislink, petr, mpkz, estorespb, rosagro, neftreg, forscience, volgzmo, rusal, moek, kamaz, uni, ksk, gmt, ymz, unipro, apps, rtscorp, sever, medic, bidzaar, metodholding, bhm, domru, samaragips, goldenseed, kaustik, dme, tele2, osnova, sibgenco, vtbconnect, rtci, forumgd, energybase, etprt, comitazmo, estp, magnitstroy, neftisa, belorusneft, ishim, barnaultm, tularegion, sngb, sevzakaz, dfsamara, yanao"

        if argv.Length = 0 then
            printf "Bad arguments, use %s" arguments
            Environment.Exit(1)

        if argv.Length = 2 then
            Settings.RosselNum <- argv.[1]
            ()

        match argv.[0] with
        | "irkutskoil" ->
            let settings =
                Settings.getSettings (IrkutskOil)

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
            let settings =
                Settings.getSettings (StroyTorgi)

            let p = Init(settings, StroyTorgi)
            p.Parsing()
        | "asgor" ->
            let settings = Settings.getSettings (Asgor)
            let p = Init(settings, Asgor)
            p.Parsing()
        | "gosyakut" ->
            let settings =
                Settings.getSettings (GosYakut)

            let p = Init(settings, GosYakut)
            p.Parsing()
        | "rostend" ->
            let settings =
                Settings.getSettings (RosTend)

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
            let settings =
                Settings.getSettings (SibServ)

            let p = Init(settings, SibServ)
            p.Parsing()
        | "tguru" ->
            let settings = Settings.getSettings (TGuru)
            let p = Init(settings, TGuru)
            p.Parsing()
        | "bidmart" ->
            let settings =
                Settings.getSettings (BidMart)

            let p = Init(settings, BidMart)
            p.Parsing()
        | "comita" ->
            let settings = Settings.getSettings (Comita)
            let p = Init(settings, Comita)
            p.Parsing()
        | "eshoprzd" ->
            let settings =
                Settings.getSettings (EshopRzd)

            let p = Init(settings, EshopRzd)
            p.Parsing()
        | "yarregion" ->
            let settings =
                Settings.getSettings (YarRegion)

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
            let settings =
                Settings.getSettings (Tenderer)

            let p = Init(settings, Tenderer)
            p.Parsing()
        | "samolet" ->
            let settings =
                Settings.getSettings (Samolet)

            let p = Init(settings, Samolet)
            p.Parsing()
        | "ariba" ->
            let settings = Settings.getSettings (Ariba)
            let p = Init(settings, Ariba)
            p.Parsing()
        | "beeline" ->
            let settings =
                Settings.getSettings (Beeline)

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
            let settings =
                Settings.getSettings (CisLink)

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
            let settings =
                Settings.getSettings (EstoreSpb)

            let p = Init(settings, EstoreSpb)
            p.Parsing()
        | "rosagro" ->
            let settings =
                Settings.getSettings (RosAgro)

            let p = Init(settings, RosAgro)
            p.Parsing()
        | "neftreg" ->
            let settings =
                Settings.getSettings (NeftReg)

            let p = Init(settings, NeftReg)
            p.Parsing()
        | "forscience" ->
            let settings =
                Settings.getSettings (ForScience)

            let p = Init(settings, ForScience)
            p.Parsing()
        | "volgzmo" ->
            let settings =
                Settings.getSettings (VolgZmo)

            let p = Init(settings, VolgZmo)
            p.Parsing()
        | "rusal" ->
            let settings = Settings.getSettings (Rusal)
            let p = Init(settings, Rusal)
            p.Parsing()
        | "moek" ->
            let settings = Settings.getSettings (Moek)
            let p = Init(settings, Moek)
            p.Parsing()
        | "kamaz" ->
            let settings = Settings.getSettings (Kamaz)
            let p = Init(settings, Kamaz)
            p.Parsing()
        | "uni" ->
            let settings = Settings.getSettings (Uni)
            let p = Init(settings, Uni)
            p.Parsing()
        | "ksk" ->
            let settings = Settings.getSettings (Ksk)
            let p = Init(settings, Ksk)
            p.Parsing()
        | "gmt" ->
            let settings = Settings.getSettings (Gmt)
            let p = Init(settings, Gmt)
            p.Parsing()
        | "ymz" ->
            let settings = Settings.getSettings (Ymz)
            let p = Init(settings, Ymz)
            p.Parsing()
        | "unipro" ->
            let settings = Settings.getSettings (Unipro)
            let p = Init(settings, Unipro)
            p.Parsing()
        | "apps" ->
            let settings = Settings.getSettings (Apps)
            let p = Init(settings, Apps)
            p.Parsing()
        | "rtscorp" ->
            let settings =
                Settings.getSettings (RtsCorp)

            let p = Init(settings, RtsCorp)
            p.Parsing()
        | "sever" ->
            let settings = Settings.getSettings (Sever)
            let p = Init(settings, Sever)
            p.Parsing()
        | "medic" ->
            let settings = Settings.getSettings (Medic)
            let p = Init(settings, Medic)
            p.Parsing()
        | "bidzaar" ->
            let settings =
                Settings.getSettings (Bidzaar)

            let p = Init(settings, Bidzaar)
            p.Parsing()
        | "metodholding" ->
            let settings =
                Settings.getSettings (Metodholding)

            let p = Init(settings, Metodholding)
            p.Parsing()
        | "bhm" ->
            let settings = Settings.getSettings (Bhm)
            let p = Init(settings, Bhm)
            p.Parsing()
        | "domru" ->
            let settings = Settings.getSettings (Domru)
            let p = Init(settings, Domru)
            p.Parsing()
        | "samaragips" ->
            let settings =
                Settings.getSettings (Samaragips)

            let p = Init(settings, Samaragips)
            p.Parsing()
        | "goldenseed" ->
            let settings =
                Settings.getSettings (Goldenseed)

            let p = Init(settings, Goldenseed)
            p.Parsing()
        | "kaustik" ->
            let settings =
                Settings.getSettings (Kaustik)

            let p = Init(settings, Kaustik)
            p.Parsing()
        | "dme" ->
            let settings = Settings.getSettings (Dme)
            let p = Init(settings, Dme)
            p.Parsing()
        | "tele2" ->
            let settings = Settings.getSettings (Tele2)
            let p = Init(settings, Tele2)
            p.Parsing()
        | "osnova" ->
            let settings = Settings.getSettings (Osnova)
            let p = Init(settings, Osnova)
            p.Parsing()
        | "sibgenco" ->
            let settings =
                Settings.getSettings (Sibgenco)

            let p = Init(settings, Sibgenco)
            p.Parsing()
        | "vtbconnect" ->
            let settings =
                Settings.getSettings (Vtbconnect)

            let p = Init(settings, Vtbconnect)
            p.Parsing()
        | "rtci" ->
            let settings = Settings.getSettings (Rtci)
            let p = Init(settings, Rtci)
            p.Parsing()
        | "forumgd" ->
            let settings =
                Settings.getSettings (Forumgd)

            let p = Init(settings, Forumgd)
            p.Parsing()
        | "energybase" ->
            let settings =
                Settings.getSettings (Energybase)

            let p = Init(settings, Energybase)
            p.Parsing()
        | "etprt" ->
            let settings = Settings.getSettings (EtpRt)
            let p = Init(settings, EtpRt)
            p.Parsing()
        | "comitazmo" ->
            let settings =
                Settings.getSettings (Comitazmo)

            let p = Init(settings, Comitazmo)
            p.Parsing()
        | "estp" ->
            let settings = Settings.getSettings (Estp)
            let p = Init(settings, Estp)
            p.Parsing()
        | "magnitstroy" ->
            let settings =
                Settings.getSettings (Magnitstroy)

            let p = Init(settings, Magnitstroy)
            p.Parsing()
        | "neftisa" ->
            let settings =
                Settings.getSettings (Neftisa)

            let p = Init(settings, Neftisa)
            p.Parsing()
        | "belorusneft" ->
            let settings =
                Settings.getSettings (Belorusneft)

            let p = Init(settings, Belorusneft)
            p.Parsing()
        | "ishim" ->
            let settings = Settings.getSettings (Ishim)
            let p = Init(settings, Ishim)
            p.Parsing()
        | "barnaultm" ->
            let settings =
                Settings.getSettings (Barnaultm)

            let p = Init(settings, Barnaultm)
            p.Parsing()
        | "tularegion" ->
            let settings =
                Settings.getSettings (Tularegion)

            let p = Init(settings, Tularegion)
            p.Parsing()
        | "sngb" ->
            let settings = Settings.getSettings (Sngb)
            let p = Init(settings, Sngb)
            p.Parsing()
        | "sevzakaz" ->
            let settings = Settings.getSettings (Sevzakaz)
            let p = Init(settings, Sevzakaz)
            p.Parsing()
        | "dfsamara" ->
            let settings = Settings.getSettings (DfSamara)
            let p = Init(settings, DfSamara)
            p.Parsing()
        | "yanao" ->
            let settings = Settings.getSettings (Yanao)
            let p = Init(settings, Yanao)
            p.Parsing()
        | _ ->
            printf "Bad arguments, use %s" arguments
            Environment.Exit(1)

        0 // return an integer exit code
