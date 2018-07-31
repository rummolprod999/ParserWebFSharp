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
