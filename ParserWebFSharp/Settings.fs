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
    
    let getSettings (arg: Arguments) : T = 
        let mutable Database = ""
        let mutable TempPathTendersIrkutskOil = ""
        let mutable LogPathTendersIrkutskOil = ""
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
                elif (xnode :?> XmlNode).Name = "prefix" then Prefix <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "userdb" then UserDb <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "passdb" then PassDb <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "server" then Server <- (xnode :?> XmlNode).InnerText
                elif (xnode :?> XmlNode).Name = "port" then Port <- Int32.Parse((xnode :?> XmlNode).InnerText)
            let connectstring = 
                sprintf 
                    "Server=%s;port=%d;Database=%s;User Id=%s;password=%s;CharSet=utf8;Convert Zero Datetime=True;default command timeout=3600;Connection Timeout=3600" 
                    Server Port Database UserDb PassDb
            let TempPathTenders = match arg with
                                  | IrkutskOil -> TempPathTendersIrkutskOil
            let LogPathTenders = match arg with
                                  | IrkutskOil -> LogPathTendersIrkutskOil
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
            printf "Bad file settings, goodbye" ; Environment.Exit(1)
            { Database = Database
              TempPathTenders = ""
              LogPathTenders = ""
              Prefix = Prefix
              UserDb = UserDb
              PassDb = PassDb
              Server = Server
              Port = Port
              ConStr = "" }
