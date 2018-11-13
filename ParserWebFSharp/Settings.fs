namespace ParserWeb

open System
open System.IO
open System.Reflection
open System.Xml

module Settings =
    let PathProgram : string =
        let path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase)
        if path <> null then path.Substring(5)
        else ""
    
    type T =
        { Database : string
          TempPathTenders : string
          LogPathTenders : string
          Prefix : string
          UserDb : string
          PassDb : string
          Server : string
          Port : int
          ConStr : string }
    
    let getSettings (arg : Arguments) : T =
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
        let mutable Prefix = ""
        let mutable UserDb = ""
        let mutable PassDb = ""
        let mutable Server = ""
        let mutable Port = 3306
        let xDoc = new XmlDocument()
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
                elif (xnode :?> XmlNode).Name = "prefix" then Prefix <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "userdb" then UserDb <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "passdb" then PassDb <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "server" then Server <- (xnode :?> XmlNode).InnerText
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
