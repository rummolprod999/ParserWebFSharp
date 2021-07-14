namespace ParserWeb

open System
open System.IO

type Init(s: Settings.T, arg: Arguments) =

    do
        if String.IsNullOrEmpty(s.TempPathTenders) || String.IsNullOrEmpty(s.LogPathTenders) then
            printf "Не получится создать папки для парсинга"
            Environment.Exit(0)
        else
            match Directory.Exists(s.TempPathTenders) with
            | true ->
                let dirInfo = DirectoryInfo(s.TempPathTenders)
                dirInfo.Delete(true)
                Directory.CreateDirectory(s.TempPathTenders) |> ignore
            | false -> Directory.CreateDirectory(s.TempPathTenders) |> ignore
            match Directory.Exists(s.LogPathTenders) with
            | false -> Directory.CreateDirectory(s.LogPathTenders) |> ignore
            | true -> ()
        match arg with
        | x ->
            Logging.FileLog <-
                sprintf "%s%clog_parsing_%O_%s.log" s.LogPathTenders Path.DirectorySeparatorChar x
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
        | GosYakut -> this.ParsingGosYakut()
        | RosTend -> this.ParsingRosTendParall()
        | ChPt -> this.ParsingChPt()
        | Tplus -> this.ParsingTplus()
        | SibServ -> this.ParsingSibServ()
        | TGuru -> this.ParsingTGuru()
        | BidMart -> this.ParsingBidMart()
        | Comita -> this.ParsingComita()
        | EshopRzd -> this.ParsingEshopRzd()
        | YarRegion -> this.ParsingYarRegion()
        | Btg -> this.ParsingBtg()
        | Vend -> this.ParsingVend()
        | Pik -> this.ParsingPik()
        | NorNic -> this.ParsingNorNic()
        | Tenderer -> this.ParsingTenderer()
        | Samolet -> this.ParsingSamolet()
        | Ariba -> this.ParsingAriba()
        | Beeline -> this.ParsingBeeline()
        | Tsm -> this.ParsingTsm()
        | Smart -> this.ParsingSmart()
        | RtsGen -> this.ParsingRtsGen()
        | Tj -> this.ParsingTj()
        | Turk -> this.ParsingTurk()
        | Kg -> this.ParsingKg()
        | Eten -> this.ParsingEten()
        | CisLink -> this.ParsingCisLink()
        | Petr -> this.ParsingPetr()
        | Mpkz -> this.ParsingMpKz()
        | EstoreSpb -> this.ParsingEstoreSpb()
        | RosAgro -> this.ParsingRosAgro()
        | NeftReg -> this.ParsingNeftReg()
        | ForScience -> this.ParsingForScience()
        | VolgZmo -> this.ParsingVolgZmo()
        | Rusal -> this.ParsingRusal()
        | Moek -> this.ParsingMoek()
        | Kamaz -> this.ParsingKamaz()
        | Uni -> this.ParsingUni()
        | Ksk -> this.ParsingKsk()
        | Gmt -> this.ParsingGmt()
        | Ymz -> this.ParsingYmz()
        | Unipro -> this.ParsingUnipro()
        | Apps -> this.ParsingApps()
        | RtsCorp -> this.ParsingRtsCorp()
        | Sever -> this.ParsingSever()
        | Medic -> this.ParsingMedic()
        | Bidzaar -> this.ParsingBidzaar()
        | Metodholding -> this.ParsingMetodholding()
        | Bhm -> this.ParsingBhm()
        | Domru -> this.ParsingDomRu()
        | Samaragips -> this.ParsingSamaraGips()
        | Goldenseed -> this.ParsingGoldenSeed()
        | Kaustik -> this.ParsingKaustik()
        | Dme -> this.ParsingDme()
        | Tele2 -> this.ParsingTele2()
        | Osnova -> this.ParsingOsnova()
        | Sibgenco -> this.ParsingSibGenco()
        | Vtbconnect -> this.ParsingVtbConnect()
        | Rtci -> this.ParsingRtCi()
        | Forumgd -> this.ParsingForumGd()
        | Energybase -> this.ParsingEnergyBase()
        | EtpRt -> this.ParsingEtpRt()
        | Comitazmo -> this.ParsingComitaZmo()
        | Estp -> this.ParsingEstp()
        | Magnitstroy -> this.ParsingMagnitStroy()
        | Neftisa -> this.ParsingNeftisa()
        | Belorusneft -> this.ParsingBelorusNeft()
        | Ishim -> this.ParsingIshim()
        | Barnaultm -> this.ParsingBarnaulTm()
        | Tularegion -> this.ParsingTulaRegion()

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
        Logging.Log.logger
            (sprintf "Добавили Корпоративный интернет-магазин тендеров %d" !TenderRossel.tenderCountKim)
        Logging.Log.logger
            (sprintf "Обновили Корпоративный интернет-магазин тендеров %d" !TenderRossel.tenderUpCountKim)

    member private this.ParsingNeft() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserNeftNew(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderNeftNew.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderNeftNew.tenderUpCount)

    member private this.ParsingSlav() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserSlav(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger
            (sprintf "Добавили тендеров ОАО «Славнефть-Мегионнефтегаз» %d" !TenderSlav.tenderCountMegion)
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

    member private this.ParsingGosYakut() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserGosYakut(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderGosYakut.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderGosYakut.tenderUpCount)

    member private this.ParsingRosTendParall() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserRostendTask(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderRosTendNew.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderRosTendNew.tenderUpCount)

    member private this.ParsingChPt() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserChPt(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderChPt.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderChPt.tenderUpCount)

    member private this.ParsingTplus() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserTPlusParall(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderTplus.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderTplus.tenderUpCount)

    member private this.ParsingSibServ() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserSibServ(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderSibServ.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderSibServ.tenderUpCount)

    member private this.ParsingTGuru() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserTGuru(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderTGuru.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderTGuru.tenderUpCount)

    member private this.ParsingBidMart() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserBidMartNew(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderBidMartNew.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderBidMartNew.tenderUpCount)

    member private this.ParsingComita() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserComita(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderComita.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderComita.tenderUpCount)

    member private this.ParsingEshopRzd() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserEshopRzd(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderEshopRzd.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderEshopRzd.tenderUpCount)

    member private this.ParsingYarRegion() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserYarRegion(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderYarRegion.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderYarRegion.tenderUpCount)

    member private this.ParsingBtg() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserBtg(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderBtg.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderBtg.tenderUpCount)

    member private this.ParsingVend() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserVend(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderVend.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderVend.tenderUpCount)

    member private this.ParsingPik() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserPik(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderPik.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderPik.tenderUpCount)

    member private this.ParsingNorNic() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserNorNic(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderNorNic.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderNorNic.tenderUpCount)

    member private this.ParsingTenderer() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserTenderer(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderTenderer.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderTenderer.tenderUpCount)

    member private this.ParsingSamolet() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserSamolet(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderSamolet.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderSamolet.tenderUpCount)

    member private this.ParsingAriba() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserAriba(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderAriba.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderAriba.tenderUpCount)

    member private this.ParsingBeeline() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserBeeline(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderBeeline.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderBeeline.tenderUpCount)

    member private this.ParsingTsm() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserTsm(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderTsm.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderTsm.tenderUpCount)

    member private this.ParsingSmart() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserSmartNew(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderSmartNew.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderSmartNew.tenderUpCount)

    member private this.ParsingRtsGen() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserRtsGen(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderRtsGen.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderRtsGen.tenderUpCount)

    member private this.ParsingTj() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserTj(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderTj.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderTj.tenderUpCount)

    member private this.ParsingTurk() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserTurk(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderTurk.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderTurk.tenderUpCount)

    member private this.ParsingKg() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserKg(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderKg.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderKg.tenderUpCount)

    member private this.ParsingEten() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserEten(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderEten.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderEten.tenderUpCount)

    member private this.ParsingCisLink() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserCisLink(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderCisLink.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderCisLink.tenderUpCount)

    member private this.ParsingPetr() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserPetr(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderPetr.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderPetr.tenderUpCount)

    member private this.ParsingMpKz() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserMpkz(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderMpkz.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderMpkz.tenderUpCount)

    member private this.ParsingEstoreSpb() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserEstoreSpb(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderEstoreSpb.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderEstoreSpb.tenderUpCount)

    member private this.ParsingRosAgro() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserRosAgro(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderRosAgro.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderRosAgro.tenderUpCount)

    member private this.ParsingNeftReg() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserNeftReg(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderNeftReg.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderNeftReg.tenderUpCount)

    member private this.ParsingForScience() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserForScience(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderForScience.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderForScience.tenderUpCount)


    member private this.ParsingVolgZmo() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserVolgZmo(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderVolgZmo.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderVolgZmo.tenderUpCount)

    member private this.ParsingRusal() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserRusal(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderRusal.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderRusal.tenderUpCount)

    member private this.ParsingMoek() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserMoek(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderMoek.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderMoek.tenderUpCount)

    member private this.ParsingKamaz() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserKamaz(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderKamaz.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderKamaz.tenderUpCount)

    member private this.ParsingUni() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserUni(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderUni.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderUni.tenderUpCount)

    member private this.ParsingKsk() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserKsk(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderKsk.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderKsk.tenderUpCount)

    member private this.ParsingGmt() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserGmt(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderGmt.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderGmt.tenderUpCount)

    member private this.ParsingYmz() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserYmz(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderYmz.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderYmz.tenderUpCount)

    member private this.ParsingUnipro() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserUnipro(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderUnipro.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderUnipro.tenderUpCount)

    member private this.ParsingApps() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserApps(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderApps.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderApps.tenderUpCount)

    member private this.ParsingRtsCorp() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserRtsCorp(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров 223 %d" !TenderRtsCorp223.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров 223 %d" !TenderRtsCorp223.tenderUpCount)
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderRtsCorp.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderRtsCorp.tenderUpCount)

    member private this.ParsingSever() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserSeverStal(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderSeverStal.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderSeverStal.tenderUpCount)

    member private this.ParsingMedic() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserMedic(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderMedic.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderMedic.tenderUpCount)
    
    member private this.ParsingBidzaar() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserBidZaar(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderBidZaar.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderBidZaar.tenderUpCount)
    
    member private this.ParsingMetodholding() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserMetodholding(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderMetodholding.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderMetodholding.tenderUpCount)
   
    member private this.ParsingBhm() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserBhm(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderBhm.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderBhm.tenderUpCount)
    
    member private this.ParsingDomRu() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserDomRu(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderDomRu.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderDomRu.tenderUpCount)
    
    member private this.ParsingSamaraGips() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserSamaraGips(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderSamaraGips.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderSamaraGips.tenderUpCount)
    
    member private this.ParsingGoldenSeed() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserGoldenSeed(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderGoldenSeed.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderGoldenSeed.tenderUpCount)
    
    member private this.ParsingKaustik() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserKaustik(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderKaustik.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderKaustik.tenderUpCount)
    
    member private this.ParsingDme() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserDme(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderDme.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderDme.tenderUpCount)
    
    member private this.ParsingTele2() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserTele2(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderTele2.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderTele2.tenderUpCount)
    
    member private this.ParsingOsnova() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserOsnova(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderOsnova.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderOsnova.tenderUpCount)
        
    member private this.ParsingSibGenco() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserSibGenco(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderSibGenco.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderSibGenco.tenderUpCount)
        
    member private this.ParsingVtbConnect() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserVtbConnect(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderVtbConnect.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderVtbConnect.tenderUpCount)
    
    member private this.ParsingRtCi() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserRtCi(s))
        with ex -> Logging.Log.logger ex
        try
            this.GetParser(ParserRtCi2(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderRtCi.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderRtCi.tenderUpCount)
        
    member private this.ParsingForumGd() =
        Logging.Log.logger "Начало парсинга"
        try
            this.GetParser(ParserForumGd(s))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger "Конец парсинга"
        Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderForumGd.tenderCount)
        Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderForumGd.tenderUpCount)
    
    member private this.ParsingEnergyBase() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserEnergyBase(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderEnergyBase.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderEnergyBase.tenderUpCount)
    
    member private this.ParsingEtpRt() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserEtpRt(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderEtpRt.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderEtpRt.tenderUpCount)
    
    member private this.ParsingComitaZmo() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserComitaZmo(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderComitaZmo.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderComitaZmo.tenderUpCount)
    
    member private this.ParsingEstp() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserEstp(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderEstp.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderEstp.tenderUpCount)
    
    member private this.ParsingMagnitStroy() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserMagnitStroy(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderMagnitStroy.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderMagnitStroy.tenderUpCount)
    
    member private this.ParsingNeftisa() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserNeftisa(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderNeftisa.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderNeftisa.tenderUpCount)
    
    member private this.ParsingBelorusNeft() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserBelorusNeft(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderBelorusNeft.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderBelorusNeft.tenderUpCount)
    
    member private this.ParsingIshim() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserIshim(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderIshim.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderIshim.tenderUpCount)
    
    member private this.ParsingBarnaulTm() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserBarnaulTm(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderBarnaulTm.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderBarnaulTm.tenderUpCount)
    
    member private this.ParsingTulaRegion() =
            Logging.Log.logger "Начало парсинга"
            try
                this.GetParser(ParserTulaRegion(s))
            with ex -> Logging.Log.logger ex
            Logging.Log.logger "Конец парсинга"
            Logging.Log.logger (sprintf "Добавили тендеров %d" !TenderTulaRegion.tenderCount)
            Logging.Log.logger (sprintf "Обновили тендеров %d" !TenderTulaRegion.tenderUpCount)
    member private this.GetParser(p: Parser) = p.Parsing()
