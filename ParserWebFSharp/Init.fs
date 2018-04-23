namespace ParserWeb

open System
open System.IO

type Init(s : Settings.T, arg : Arguments) = 
    
    do 
        if String.IsNullOrEmpty(s.TempPathTenders) || String.IsNullOrEmpty(s.LogPathTenders) then 
            printf "Не получится создать папки для парсинга"
            Environment.Exit(0)
        else 
            match Directory.Exists(s.TempPathTenders) with
            | true -> 
                let dirInfo = new DirectoryInfo(s.TempPathTenders)
                dirInfo.Delete(true)
                Directory.CreateDirectory(s.TempPathTenders) |> ignore
            | false -> Directory.CreateDirectory(s.TempPathTenders) |> ignore
            match Directory.Exists(s.LogPathTenders) with
            | false -> Directory.CreateDirectory(s.LogPathTenders) |> ignore
            | true -> ()
        match arg with
        | IrkutskOil -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
        | Akd -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
    
    member public this.Parsing() = 
        match arg with
        | IrkutskOil -> this.ParsingIrkutsk()
        | Akd -> this.ParsingAkd()
    
    member private this.ParsingIrkutsk() = 
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserIrkutskOil(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderIrkutskOil.tenderCount)
    
    member private this.ParsingAkd() = 
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserAkd(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderAkd.tenderCount)
    
    member private this.GetParser( p : Parser) =
        p.Parsing()
        
