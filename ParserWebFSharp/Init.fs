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
        | Lsr -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
        | Butb -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
        | RosSel -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
        | Neft -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
        | Slav -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
        | Aero -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
        | StroyTorgi -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
        | Asgor -> 
            Logging.FileLog <- sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar arg 
                               <| DateTime.Now.ToString("dd_MM_yyyy")
    
    member public this.Parsing() =
        match arg with
        | IrkutskOil -> this.ParsingIrkutsk()
        | Akd -> this.ParsingAkd()
        | Lsr -> this.ParsingLsr()
        | Butb -> this.ParsingButb()
        | RosSel -> this.ParsingRossel()
        | Neft -> this.ParsingNeft()
        | Slav -> this.ParsingSlav()
        | Aero -> this.ParsingAero()
        | StroyTorgi -> this.ParsingStroyTorgi()
        | Asgor -> this.ParsingAsgor()
    
    member private this.ParsingIrkutsk() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserIrkutskOil(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderIrkutskOil.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderIrkutskOil.tenderUpCount)
    
    member private this.ParsingAsgor() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserAsgor(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderAsgor.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderAsgor.tenderUpCount)
    
    member private this.ParsingAkd() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserAkd(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderAkd.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderAkd.tenderUpCount)
    
    member private this.ParsingLsr() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserLsr(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderLsr.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderLsr.tenderUpCount)
    
    member private this.ParsingButb() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserButb(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderButb.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderButb.tenderUpCount)
    
    member private this.ParsingRossel() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserRossel(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили коммерческих тендеров %d" !TenderRossel.tenderCount)
        Logging.Log.logger (sprintf "Обновили коммерческих тендеров %d" !TenderRossel.tenderUpCount)
        Logging.Log.logger (sprintf "Добавили ГК «Росатом» тендеров %d" !TenderRossel.tenderCountAtom)
        Logging.Log.logger (sprintf "Обновили ГК «Росатом» тендеров %d" !TenderRossel.tenderUpCountAtom)
        Logging.Log.logger 
            (sprintf "Добавили ПАО «Ростелеком» и подведомственных организаций тендеров %d" !TenderRossel.tenderCountRt)
        Logging.Log.logger 
            (sprintf "Обновили ПАО «Ростелеком» и подведомственных организаций тендеров %d" 
                 !TenderRossel.tenderUpCountRt)
        Logging.Log.logger (sprintf "Добавили Группа ВТБ тендеров %d" !TenderRossel.tenderCountVtb)
        Logging.Log.logger (sprintf "Обновили Группа ВТБ тендеров %d" !TenderRossel.tenderUpCountVtb)
        Logging.Log.logger (sprintf "Добавили ГК «Ростех» тендеров %d" !TenderRossel.tenderCountRosteh)
        Logging.Log.logger (sprintf "Обновили ГК «Ростех» тендеров %d" !TenderRossel.tenderUpCountRosteh)
        Logging.Log.logger (sprintf "Добавили Группа «РусГидро» тендеров %d" !TenderRossel.tenderCountRushidro)
        Logging.Log.logger (sprintf "Обновили Группа «РусГидро» тендеров %d" !TenderRossel.tenderUpCountRushidro)
        Logging.Log.logger (sprintf "Добавили Холдинг «Росгео» тендеров %d" !TenderRossel.tenderCountRosgeo)
        Logging.Log.logger (sprintf "Обновили Холдинг «Росгео» тендеров %d" !TenderRossel.tenderUpCountRosgeo)
        Logging.Log.logger (sprintf "Добавили ПАО «Россети» тендеров %d" !TenderRossel.tenderCountRosseti)
        Logging.Log.logger (sprintf "Обновили ПАО «Россети» тендеров %d" !TenderRossel.tenderUpCountRosseti)
    
    member private this.ParsingNeft() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserNeft(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderNeft.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderNeft.tenderUpCount)
    
    member private this.ParsingSlav() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserSlav(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров ОАО «Славнефть-Мегионнефтегаз» %d" !TenderSlav.tenderCountMegion)
        Logging.Log.logger 
            (sprintf "Обновили тендеров ОАО «Славнефть-Мегионнефтегаз» %d" !TenderSlav.tenderUpCountMegion)
        Logging.Log.logger (sprintf "Добавили тендеров ООО «Байкитская НГРЭ» %d" !TenderSlav.tenderCountNgre)
        Logging.Log.logger (sprintf "Обновили тендеров ООО «Байкитская НГРЭ» %d" !TenderSlav.tenderUpCountNgre)
        Logging.Log.logger (sprintf "Добавили тендеров ОАО «Славнефть-ЯНОС» %d" !TenderSlav.tenderCountYanos)
        Logging.Log.logger (sprintf "Обновили тендеров ОАО «Славнефть-ЯНОС» %d" !TenderSlav.tenderUpCountYanos)
    
    member private this.ParsingAero() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserAero(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderAero.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderAero.tenderUpCount)
    
    member private this.ParsingStroyTorgi() =
        Logging.Log.logger "Начало парсинга"
        try 
            this.GetParser(ParserStroyTorgi(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderStroyTorgi.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderStroyTorgi.tenderUpCount)
    
    member private this.GetParser(p : Parser) = p.Parsing()
