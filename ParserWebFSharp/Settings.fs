namespace ParserWeb

open System
open System.IO
open System.Reflection
open System.Xml

module Settings =
    let PathProgram: string =
        let path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
        if path <> null then path.Substring(5)
        else ""

    let mutable internal RosselNum = ""
    let mutable internal UserTenderer = ""
    let mutable internal UserCisLink = ""
    let mutable internal PassCisLink = ""
    let mutable internal UserBidMart = ""
    let mutable internal PassBidMart = ""
    let mutable internal UserMedic = ""
    let mutable internal PassMedic = ""
    let mutable internal UserBidZaar = ""
    let mutable internal PassBidZaar = ""
    let mutable internal UserVtb = ""
    let mutable internal PassVtb = ""
    type T =
        { Database: string
          TempPathTenders: string
          LogPathTenders: string
          Prefix: string
          UserDb: string
          PassDb: string
          Server: string
          Port: int
          ConStr: string }

    let getSettings (arg: Arguments): T =
        let mutable Database = ""
        let mutable TempPathTendersIrkutskOil = ""
        let mutable LogPathTendersIrkutskOil = ""
        let mutable TempPathTendersAkd = ""
        let mutable LogPathTendersAkd = ""
        let mutable TempPathTendersLsr = ""
        let mutable LogPathTendersLsr = ""
        let mutable TempPathTendersButb = ""
        let mutable LogPathTendersButb = ""
        let mutable TempPathTendersRossel = ""
        let mutable LogPathTendersRossel = ""
        let mutable TempPathTendersNeft = ""
        let mutable LogPathTendersNeft = ""
        let mutable TempPathTendersSlav = ""
        let mutable LogPathTendersSlav = ""
        let mutable TempPathTendersAero = ""
        let mutable LogPathTendersAero = ""
        let mutable TempPathTendersStroyTorgi = ""
        let mutable LogPathTendersStroyTorgi = ""
        let mutable TempPathTendersAsgor = ""
        let mutable LogPathTendersAsgor = ""
        let mutable TempPathTendersGosYakut = ""
        let mutable LogPathTendersGosYakut = ""
        let mutable TempPathTendersRosTend = ""
        let mutable LogPathTendersRosTend = ""
        let mutable TempPathTendersChPt = ""
        let mutable LogPathTendersChPt = ""
        let mutable TempPathTendersTplus = ""
        let mutable LogPathTendersTplus = ""
        let mutable TempPathTendersSibServ = ""
        let mutable LogPathTendersSibServ = ""
        let mutable TempPathTendersTGuru = ""
        let mutable LogPathTendersTGuru = ""
        let mutable TempPathTendersBidMart = ""
        let mutable LogPathTendersBidMart = ""
        let mutable TempPathTendersComita = ""
        let mutable LogPathTendersComita = ""
        let mutable TempPathTendersEshopRzd = ""
        let mutable LogPathTendersEshopRzd = ""
        let mutable TempPathTendersYarRegion = ""
        let mutable LogPathTendersYarRegion = ""
        let mutable TempPathTendersBtg = ""
        let mutable LogPathTendersBtg = ""
        let mutable TempPathTendersVend = ""
        let mutable LogPathTendersVend = ""
        let mutable TempPathTendersPik = ""
        let mutable LogPathTendersPik = ""
        let mutable TempPathTendersNorNic = ""
        let mutable LogPathTendersNorNic = ""
        let mutable TempPathTendersTenderer = ""
        let mutable LogPathTendersTenderer = ""
        let mutable TempPathTendersSamolet = ""
        let mutable LogPathTendersSamolet = ""
        let mutable TempPathTendersAriba = ""
        let mutable LogPathTendersAriba = ""
        let mutable TempPathTendersBeeline = ""
        let mutable LogPathTendersBeeline = ""
        let mutable TempPathTendersTsm = ""
        let mutable LogPathTendersTsm = ""
        let mutable TempPathTendersSmart = ""
        let mutable LogPathTendersSmart = ""
        let mutable TempPathTendersRtsGen = ""
        let mutable LogPathTendersRtsGen = ""
        let mutable TempPathTendersTj = ""
        let mutable LogPathTendersTj = ""
        let mutable TempPathTendersTurk = ""
        let mutable LogPathTendersTurk = ""
        let mutable TempPathTendersKg = ""
        let mutable LogPathTendersKg = ""
        let mutable TempPathTendersEten = ""
        let mutable LogPathTendersEten = ""
        let mutable TempPathTendersCisLink = ""
        let mutable LogPathTendersCisLink = ""
        let mutable TempPathTendersPetr = ""
        let mutable LogPathTendersPetr = ""
        let mutable TempPathTendersMpkz = ""
        let mutable LogPathTendersMpkz = ""
        let mutable TempPathTendersEstoreSpb = ""
        let mutable LogPathTendersEstoreSpb = ""
        let mutable TempPathTendersRosAgro = ""
        let mutable LogPathTendersRosAgro = ""
        let mutable TempPathTendersNeftReg = ""
        let mutable LogPathTendersNeftReg = ""
        let mutable TempPathTendersForScience = ""
        let mutable LogPathTendersForScience = ""
        let mutable TempPathTendersVolgZmo = ""
        let mutable LogPathTendersVolgZmo = ""
        let mutable TempPathTendersRusal = ""
        let mutable LogPathTendersRusal = ""
        let mutable TempPathTendersMoek = ""
        let mutable LogPathTendersMoek = ""
        let mutable TempPathTendersKamaz = ""
        let mutable LogPathTendersKamaz = ""
        let mutable TempPathTendersUni = ""
        let mutable LogPathTendersUni = ""
        let mutable TempPathTendersKsk = ""
        let mutable LogPathTendersKsk = ""
        let mutable TempPathTendersGmt = ""
        let mutable LogPathTendersGmt = ""
        let mutable TempPathTendersYmz = ""
        let mutable LogPathTendersYmz = ""
        let mutable TempPathTendersUnipro = ""
        let mutable LogPathTendersUnipro = ""
        let mutable TempPathTendersApps = ""
        let mutable LogPathTendersApps = ""
        let mutable TempPathTendersRtsCorp = ""
        let mutable LogPathTendersRtsCorp = ""
        let mutable TempPathTendersSeverStal = ""
        let mutable LogPathTendersSeverStal = ""
        let mutable TempPathTendersMedic = ""
        let mutable LogPathTendersMedic = ""
        let mutable TempPathTendersBidzaar = ""
        let mutable LogPathTendersBidzaar = ""
        let mutable TempPathTendersMetodholding = ""
        let mutable LogPathTendersMetodholding  = ""
        let mutable TempPathTendersBhm = ""
        let mutable LogPathTendersBhm  = ""
        let mutable TempPathTendersDomru = ""
        let mutable LogPathTendersDomru  = ""
        let mutable TempPathTendersSamaraGips = ""
        let mutable LogPathTendersSamaraGips  = ""
        let mutable TempPathTendersGoldenSeed = ""
        let mutable LogPathTendersGoldenSeed  = ""
        let mutable TempPathTendersKaustik = ""
        let mutable LogPathTendersKaustik  = ""
        let mutable TempPathTendersDme = ""
        let mutable LogPathTendersDme  = ""
        let mutable TempPathTendersTele2 = ""
        let mutable LogPathTendersTele2  = ""
        let mutable TempPathTendersOsnova = ""
        let mutable LogPathTendersOsnova  = ""
        let mutable TempPathTendersSibGenco = ""
        let mutable LogPathTendersSibGenco  = ""
        let mutable TempPathTendersVtbConnect = ""
        let mutable LogPathTendersVtbConnect  = ""
        let mutable Prefix = ""
        let mutable UserDb = ""
        let mutable PassDb = ""
        let mutable Server = ""
        let mutable Port = 3306
        let xDoc = XmlDocument()
        xDoc.Load(sprintf "%s%csetting_tenders.xml" PathProgram Path.DirectorySeparatorChar)
        let xRoot = xDoc.DocumentElement
        if xRoot <> null then
            for xnode in xRoot do
                if (xnode :?> XmlNode).Name = "database" then Database <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_irkutskoil" then
                    TempPathTendersIrkutskOil <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                     (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_irkutskoil" then
                    LogPathTendersIrkutskOil <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                    (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_akd" then
                    TempPathTendersAkd <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_akd" then
                    LogPathTendersAkd <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                             (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_lsrgroup" then
                    TempPathTendersLsr <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_lsrgroup" then
                    LogPathTendersLsr <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                             (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_butb" then
                    TempPathTendersButb <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                               (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_butb" then
                    LogPathTendersButb <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_rossel" then
                    TempPathTendersRossel <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_rossel" then
                    LogPathTendersRossel <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_neft" then
                    TempPathTendersNeft <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                               (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_neft" then
                    LogPathTendersNeft <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_slav" then
                    TempPathTendersSlav <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                               (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_slav" then
                    LogPathTendersSlav <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_aero" then
                    TempPathTendersAero <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                               (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_aero" then
                    LogPathTendersAero <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_stroytorgi" then
                    TempPathTendersStroyTorgi <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                     (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_stroytorgi" then
                    LogPathTendersStroyTorgi <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                    (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_asgor" then
                    TempPathTendersAsgor <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_asgor" then
                    LogPathTendersAsgor <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                               (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_gosyakut" then
                    TempPathTendersGosYakut <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                   (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_gosyakut" then
                    LogPathTendersGosYakut <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_rostend" then
                    TempPathTendersRosTend <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_rostend" then
                    LogPathTendersRosTend <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_chpt" then
                    TempPathTendersChPt <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                               (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_chpt" then
                    LogPathTendersChPt <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_tplus" then
                    TempPathTendersTplus <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_tplus" then
                    LogPathTendersTplus <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                               (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_sibserv" then
                    TempPathTendersSibServ <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_sibserv" then
                    LogPathTendersSibServ <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_tguru" then
                    TempPathTendersTGuru <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_tguru" then
                    LogPathTendersTGuru <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                               (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_bidmart" then
                    TempPathTendersBidMart <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_bidmart" then
                    LogPathTendersBidMart <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_comita" then
                    TempPathTendersComita <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_comita" then
                    LogPathTendersComita <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_eshoprzd" then
                    TempPathTendersEshopRzd <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                   (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_eshoprzd" then
                    LogPathTendersEshopRzd <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_yarregion" then
                    TempPathTendersYarRegion <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                    (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_yarregion" then
                    LogPathTendersYarRegion <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                   (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_btg" then
                    TempPathTendersBtg <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_btg" then
                    LogPathTendersBtg <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                             (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_vend" then
                    TempPathTendersVend <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                               (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_vend" then
                    LogPathTendersVend <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_pik" then
                    TempPathTendersPik <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                              (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_pik" then
                    LogPathTendersPik <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                             (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_nornic" then
                    TempPathTendersNorNic <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_nornic" then
                    LogPathTendersNorNic <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_tenderer" then
                    TempPathTendersTenderer <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                   (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_tenderer" then
                    LogPathTendersTenderer <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_samolet" then
                    TempPathTendersSamolet <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_samolet" then
                    LogPathTendersSamolet <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_ariba" then
                    TempPathTendersAriba <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_ariba" then
                    LogPathTendersAriba <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_beeline" then
                    TempPathTendersBeeline <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_beeline" then
                    LogPathTendersBeeline <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_tsm" then
                    TempPathTendersTsm <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_tsm" then
                    LogPathTendersTsm <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_smart" then
                    TempPathTendersSmart <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_smart" then
                    LogPathTendersSmart <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_rtsgen" then
                    TempPathTendersRtsGen <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_rtsgen" then
                    LogPathTendersRtsGen <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_tj" then
                    TempPathTendersTj <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_tj" then
                    LogPathTendersTj <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_turk" then
                    TempPathTendersTurk <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_turk" then
                    LogPathTendersTurk <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_kg" then
                                    TempPathTendersKg <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_kg" then
                                    LogPathTendersKg <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_eten" then
                                    TempPathTendersEten <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_eten" then
                                    LogPathTendersEten <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_cislink" then
                                    TempPathTendersCisLink <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_cislink" then
                                    LogPathTendersCisLink <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_petr" then
                                    TempPathTendersPetr <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_petr" then
                                    LogPathTendersPetr <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_mpkz" then
                                    TempPathTendersMpkz <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_mpkz" then
                                    LogPathTendersMpkz <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_estorespb" then
                                    TempPathTendersEstoreSpb <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_estorespb" then
                                    LogPathTendersEstoreSpb <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_rosagro" then
                                    TempPathTendersRosAgro <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_rosagro" then
                                    LogPathTendersRosAgro <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_neftreg" then
                                    TempPathTendersNeftReg <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_neftreg" then
                                    LogPathTendersNeftReg <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_forscience" then
                                    TempPathTendersForScience <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_forscience" then
                                    LogPathTendersForScience <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_volgzmo" then
                                    TempPathTendersVolgZmo <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_volgzmo" then
                                    LogPathTendersVolgZmo <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_rusal" then
                                    TempPathTendersRusal <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_rusal" then
                                    LogPathTendersRusal <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_moek" then
                                    TempPathTendersMoek <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_moek" then
                                    LogPathTendersMoek <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_kamaz" then
                                    TempPathTendersKamaz <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_kamaz" then
                                    LogPathTendersKamaz <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_unistream" then
                                    TempPathTendersUni <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_unistream" then
                                    LogPathTendersUni <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_ksk" then
                                    TempPathTendersKsk <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_ksk" then
                                    LogPathTendersKsk <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_gmt" then
                                    TempPathTendersGmt <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_gmt" then
                                    LogPathTendersGmt <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_ymz" then
                                    TempPathTendersYmz <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_ymz" then
                                    LogPathTendersYmz <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_unipro" then
                                    TempPathTendersUnipro <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_unipro" then
                                    LogPathTendersUnipro <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_apps" then
                                    TempPathTendersApps <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_apps" then
                                    LogPathTendersApps <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_rts_corp" then
                                    TempPathTendersRtsCorp <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_rts_corp" then
                                    LogPathTendersRtsCorp <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_severstal" then
                                    TempPathTendersSeverStal <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_severstal" then
                                    LogPathTendersSeverStal <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_medic" then
                                    TempPathTendersMedic <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_medic" then
                                    LogPathTendersMedic <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_bidzaar" then
                                    TempPathTendersBidzaar <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_bidzaar" then
                                    LogPathTendersBidzaar <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_metodholding" then
                                    TempPathTendersMetodholding <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_metodholding" then
                                    LogPathTendersMetodholding <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_bhm" then
                                    TempPathTendersBhm <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_bhm" then
                                    LogPathTendersBhm <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_domru" then
                                    TempPathTendersDomru <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_domru" then
                                    LogPathTendersDomru <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_samaragips" then
                                    TempPathTendersSamaraGips <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_samaragips" then
                                    LogPathTendersSamaraGips <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_goldenseed" then
                                    TempPathTendersGoldenSeed <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_goldenseed" then
                                    LogPathTendersGoldenSeed <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_kaustik" then
                                    TempPathTendersKaustik <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_kaustik" then
                                    LogPathTendersKaustik <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_dme" then
                                    TempPathTendersDme <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_dme" then
                                    LogPathTendersDme <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_tele2" then
                                    TempPathTendersTele2 <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_tele2" then
                                    LogPathTendersTele2 <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_osnova" then
                                    TempPathTendersOsnova <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_osnova" then
                                    LogPathTendersOsnova <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_sibgenco" then
                                    TempPathTendersSibGenco <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_sibgenco" then
                                    LogPathTendersSibGenco <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "tempdir_tenders_vtbconnect" then
                                    TempPathTendersVtbConnect <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                  (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "logdir_tenders_vtbconnect" then
                                    LogPathTendersVtbConnect <- sprintf "%s%c%s" PathProgram Path.DirectorySeparatorChar
                                                                 (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "prefix" then Prefix <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "userdb" then UserDb <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "passdb" then PassDb <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "server" then Server <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "usertenderer" then UserTenderer <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "usercislink" then UserCisLink <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "passcislink" then PassCisLink <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "userbidmart" then UserBidMart <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "passbidmart" then PassBidMart <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "usermedic" then UserMedic <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "passmedic" then PassMedic <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "userbidzaar" then UserBidZaar <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "passbidzaar" then PassBidZaar <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "uservtb" then UserVtb <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "passvtb" then PassVtb <- (xnode :?> XmlNode).InnerText
                else if (xnode :?> XmlNode).Name = "port" then Port <- Int32.Parse((xnode :?> XmlNode).InnerText)
            let connectstring =
                sprintf
                    "Server=%s;port=%d;Database=%s;User Id=%s;password=%s;CharSet=utf8;Convert Zero Datetime=True;default command timeout=3600;Connection Timeout=3600;SslMode=none"
                    Server Port Database UserDb PassDb

            let TempPathTenders =
                match arg with
                | IrkutskOil -> TempPathTendersIrkutskOil
                | Akd -> TempPathTendersAkd
                | Lsr -> TempPathTendersLsr
                | Butb -> TempPathTendersButb
                | RosSel -> TempPathTendersRossel
                | Neft -> TempPathTendersNeft
                | Slav -> TempPathTendersSlav
                | Aero -> TempPathTendersAero
                | StroyTorgi -> TempPathTendersStroyTorgi
                | Asgor -> TempPathTendersAsgor
                | GosYakut -> TempPathTendersGosYakut
                | RosTend -> TempPathTendersRosTend
                | ChPt -> TempPathTendersChPt
                | Tplus -> TempPathTendersTplus
                | SibServ -> TempPathTendersSibServ
                | TGuru -> TempPathTendersTGuru
                | BidMart -> TempPathTendersBidMart
                | Comita -> TempPathTendersComita
                | EshopRzd -> TempPathTendersEshopRzd
                | YarRegion -> TempPathTendersYarRegion
                | Btg -> TempPathTendersBtg
                | Vend -> TempPathTendersVend
                | Pik -> TempPathTendersPik
                | NorNic -> TempPathTendersNorNic
                | Tenderer -> TempPathTendersTenderer
                | Samolet -> TempPathTendersSamolet
                | Ariba -> TempPathTendersAriba
                | Beeline -> TempPathTendersBeeline
                | Tsm -> TempPathTendersTsm
                | Smart -> TempPathTendersSmart
                | RtsGen -> TempPathTendersRtsGen
                | Tj -> TempPathTendersTj
                | Turk -> TempPathTendersTurk
                | Kg -> TempPathTendersKg
                | Eten -> TempPathTendersEten
                | CisLink -> TempPathTendersCisLink
                | Petr -> TempPathTendersPetr
                | Mpkz -> TempPathTendersMpkz
                | EstoreSpb -> TempPathTendersEstoreSpb
                | RosAgro -> TempPathTendersRosAgro
                | NeftReg -> TempPathTendersNeftReg
                | ForScience -> TempPathTendersForScience
                | VolgZmo -> TempPathTendersVolgZmo
                | Rusal -> TempPathTendersRusal
                | Moek -> TempPathTendersMoek
                | Kamaz -> TempPathTendersKamaz
                | Uni -> TempPathTendersUni
                | Ksk -> TempPathTendersKsk
                | Gmt -> TempPathTendersGmt
                | Ymz -> TempPathTendersYmz
                | Unipro -> TempPathTendersUnipro
                | Apps -> TempPathTendersApps
                | RtsCorp -> TempPathTendersRtsCorp
                | Sever -> TempPathTendersSeverStal
                | Medic -> TempPathTendersMedic
                | Bidzaar -> TempPathTendersBidzaar
                | Metodholding -> TempPathTendersMetodholding
                | Bhm -> TempPathTendersBhm
                | Domru -> TempPathTendersDomru
                | Samaragips -> TempPathTendersSamaraGips
                | Goldenseed -> TempPathTendersGoldenSeed
                | Kaustik -> TempPathTendersKaustik
                | Dme -> TempPathTendersDme
                | Tele2 -> TempPathTendersTele2
                | Osnova -> TempPathTendersOsnova
                | Sibgenco -> TempPathTendersSibGenco
                | Vtbconnect -> TempPathTendersVtbConnect

            let LogPathTenders =
                match arg with
                | IrkutskOil -> LogPathTendersIrkutskOil
                | Akd -> LogPathTendersAkd
                | Lsr -> LogPathTendersLsr
                | Butb -> LogPathTendersButb
                | RosSel -> LogPathTendersRossel
                | Neft -> LogPathTendersNeft
                | Slav -> LogPathTendersSlav
                | Aero -> LogPathTendersAero
                | StroyTorgi -> LogPathTendersStroyTorgi
                | Asgor -> LogPathTendersAsgor
                | GosYakut -> LogPathTendersGosYakut
                | RosTend -> LogPathTendersRosTend
                | ChPt -> LogPathTendersChPt
                | Tplus -> LogPathTendersTplus
                | SibServ -> LogPathTendersSibServ
                | TGuru -> LogPathTendersTGuru
                | BidMart -> LogPathTendersBidMart
                | Comita -> LogPathTendersComita
                | EshopRzd -> LogPathTendersEshopRzd
                | YarRegion -> LogPathTendersYarRegion
                | Btg -> LogPathTendersBtg
                | Vend -> LogPathTendersVend
                | Pik -> LogPathTendersPik
                | NorNic -> LogPathTendersNorNic
                | Tenderer -> LogPathTendersTenderer
                | Samolet -> LogPathTendersSamolet
                | Ariba -> LogPathTendersAriba
                | Beeline -> LogPathTendersBeeline
                | Tsm -> LogPathTendersTsm
                | Smart -> LogPathTendersSmart
                | RtsGen -> LogPathTendersRtsGen
                | Tj -> LogPathTendersTj
                | Turk -> LogPathTendersTurk
                | Kg -> LogPathTendersKg
                | Eten -> LogPathTendersEten
                | CisLink -> LogPathTendersCisLink
                | Petr -> LogPathTendersPetr
                | Mpkz -> LogPathTendersMpkz
                | EstoreSpb -> LogPathTendersEstoreSpb
                | RosAgro -> LogPathTendersRosAgro
                | NeftReg -> LogPathTendersNeftReg
                | ForScience -> LogPathTendersForScience
                | VolgZmo -> LogPathTendersVolgZmo
                | Rusal -> LogPathTendersRusal
                | Moek -> LogPathTendersMoek
                | Kamaz -> LogPathTendersKamaz
                | Uni -> LogPathTendersUni
                | Ksk -> LogPathTendersKsk
                | Gmt -> LogPathTendersGmt
                | Ymz -> LogPathTendersYmz
                | Unipro -> LogPathTendersUnipro
                | Apps -> LogPathTendersApps
                | RtsCorp -> LogPathTendersRtsCorp
                | Sever -> LogPathTendersSeverStal
                | Medic -> LogPathTendersMedic
                | Bidzaar -> LogPathTendersBidzaar
                | Metodholding -> LogPathTendersMetodholding
                | Bhm -> LogPathTendersBhm
                | Domru -> LogPathTendersDomru
                | Samaragips -> LogPathTendersSamaraGips
                | Goldenseed -> LogPathTendersGoldenSeed
                | Kaustik -> LogPathTendersKaustik
                | Dme -> LogPathTendersDme
                | Tele2 -> LogPathTendersTele2
                | Osnova -> LogPathTendersOsnova
                | Sibgenco -> LogPathTendersSibGenco
                | Vtbconnect -> LogPathTendersVtbConnect

            { Database = Database
              TempPathTenders = TempPathTenders
              LogPathTenders = LogPathTenders
              Prefix = Prefix
              UserDb = UserDb
              PassDb = PassDb
              Server = Server
              Port = Port
              ConStr = connectstring }
        else
            printf "Bad file settings, goodbye"
            Environment.Exit(1)
            { Database = Database
              TempPathTenders = ""
              LogPathTenders = ""
              Prefix = Prefix
              UserDb = UserDb
              PassDb = PassDb
              Server = Server
              Port = Port
              ConStr = "" }
