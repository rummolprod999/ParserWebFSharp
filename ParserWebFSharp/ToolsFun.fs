namespace ParserWeb

open ICSharpCode.SharpZipLib.GZip
open ICSharpCode.SharpZipLib.Tar
open Newtonsoft.Json.Linq
open System
open System.Security.Cryptography
open System.Text
open System.IO
open System.Text.RegularExpressions

module Tools =
    open OpenQA.Selenium
    
    let (|RegexMatch2|_|) (pattern : string) (input : string) =
        let result = Regex.Match(input, pattern)
        if result.Success then 
            match (List.tail [ for g in result.Groups -> g.Value ]) with
            | fst :: snd :: [] -> Some(fst, snd)
            | _ -> None
        else None
    
    let (|RegexMatch1|_|) (pattern : string) (input : string) =
        let result = Regex.Match(input, pattern)
        if result.Success then 
            match (List.tail [ for g in result.Groups -> g.Value ]) with
            | fst :: [] -> Some(fst)
            | _ -> None
        else None
    
    let inline InlineFEWE (x : ^a) (s : string) =
        try 
            let res = (^a : (member FindElement : By -> IWebElement) (x, By.XPath(s)))
            match res with
            | null -> ""
            | r -> r.Text.Trim()
        with ex -> ""
    
    let createMD5 (s : string) : string =
        use md5Hash = MD5.Create()
        let data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(s))
        let sBuilder = new StringBuilder()
        for i in data do
            sBuilder.Append(i.ToString("x2")) |> ignore
        sBuilder.ToString()
    
    let GetRegionString(s : string) : string =
        let sLower = s.ToLower()
        match sLower with
        | s when s.Contains("отсуств") -> ""
        | s when s.Contains("белгор") -> "белгор"
        | s when s.Contains("брянск") -> "брянск"
        | s when s.Contains("владимир") -> "владимир"
        | s when s.Contains("воронеж") -> "воронеж"
        | s when s.Contains("иванов") -> "иванов"
        | s when s.Contains("калужск") -> "калужск"
        | s when s.Contains("костром") -> "костром"
        | s when s.Contains("курск") -> "курск"
        | s when s.Contains("липецк") -> "липецк"
        | s when s.Contains("москва") -> "москва"
        | s when s.Contains("московск") -> "московск"
        | s when s.Contains("орлов") -> "орлов"
        | s when s.Contains("рязан") -> "рязан"
        | s when s.Contains("смолен") -> "смолен"
        | s when s.Contains("тамбов") -> "тамбов"
        | s when s.Contains("твер") -> "твер"
        | s when s.Contains("тульс") -> "тульс"
        | s when s.Contains("яросл") -> "яросл"
        | s when s.Contains("архан") -> "архан"
        | s when s.Contains("вологод") -> "вологод"
        | s when s.Contains("калинин") -> "калинин"
        | s when s.Contains("карел") -> "карел"
        | s when s.Contains("коми") -> "коми"
        | s when s.Contains("ленинг") -> "ленинг"
        | s when s.Contains("мурм") -> "мурм"
        | s when s.Contains("ненец") -> "ненец"
        | s when s.Contains("новгор") -> "новгор"
        | s when s.Contains("псков") -> "псков"
        | s when s.Contains("санкт") -> "санкт"
        | s when s.Contains("адыг") -> "адыг"
        | s when s.Contains("астрахан") -> "астрахан"
        | s when s.Contains("волгог") -> "волгог"
        | s when s.Contains("калмык") -> "калмык"
        | s when s.Contains("краснод") -> "краснод"
        | s when s.Contains("ростов") -> "ростов"
        | s when s.Contains("дагест") -> "дагест"
        | s when s.Contains("ингуш") -> "ингуш"
        | s when s.Contains("кабардин") -> "кабардин"
        | s when s.Contains("карача") -> "карача"
        | s when s.Contains("осети") -> "осети"
        | s when s.Contains("ставроп") -> "ставроп"
        | s when s.Contains("чечен") -> "чечен"
        | s when s.Contains("башкор") -> "башкор"
        | s when s.Contains("киров") -> "киров"
        | s when s.Contains("марий") -> "марий"
        | s when s.Contains("мордов") -> "мордов"
        | s when s.Contains("нижегор") -> "нижегор"
        | s when s.Contains("оренбур") -> "оренбур"
        | s when s.Contains("пензен") -> "пензен"
        | s when s.Contains("пермс") -> "пермс"
        | s when s.Contains("самар") -> "самар"
        | s when s.Contains("сарат") -> "сарат"
        | s when s.Contains("татарс") -> "татарс"
        | s when s.Contains("удмурт") -> "удмурт"
        | s when s.Contains("ульян") -> "ульян"
        | s when s.Contains("чуваш") -> "чуваш"
        | s when s.Contains("курган") -> "курган"
        | s when s.Contains("свердлов") -> "свердлов"
        | s when s.Contains("тюмен") -> "тюмен"
        | s when s.Contains("ханты") -> "ханты"
        | s when s.Contains("челяб") -> "челяб"
        | s when s.Contains("ямало") -> "ямало"
        | s when s.Contains("алтайск") -> "алтайск"
        | s when s.Contains("алтай") -> "алтай"
        | s when s.Contains("бурят") -> "бурят"
        | s when s.Contains("забайк") -> "забайк"
        | s when s.Contains("иркут") -> "иркут"
        | s when s.Contains("кемеров") -> "кемеров"
        | s when s.Contains("краснояр") -> "краснояр"
        | s when s.Contains("новосиб") -> "новосиб"
        | s when s.Contains("томск") -> "томск"
        | s when s.Contains("омск") -> "омск"
        | s when s.Contains("тыва") -> "тыва"
        | s when s.Contains("хакас") -> "хакас"
        | s when s.Contains("амурск") -> "амурск"
        | s when s.Contains("еврей") -> "еврей"
        | s when s.Contains("камчат") -> "камчат"
        | s when s.Contains("магад") -> "магад"
        | s when s.Contains("примор") -> "примор"
        | s when s.Contains("сахалин") -> "сахалин"
        | s when s.Contains("якут") -> "саха"
        | s when s.Contains("саха") -> "саха"
        | s when s.Contains("хабар") -> "хабар"
        | s when s.Contains("чукот") -> "чукот"
        | s when s.Contains("крым") -> "крым"
        | s when s.Contains("севастоп") -> "севастоп"
        | s when s.Contains("байкон") -> "байкон"
        | _ -> ""
    
    let GetDateFromStringMonth(s : string) =
        match s with
        | s when s.Contains("января") -> s.Replace("января", "01")
        | s when s.Contains("февраля") -> s.Replace("февраля", "02")
        | s when s.Contains("марта") -> s.Replace("марта", "03")
        | s when s.Contains("апреля") -> s.Replace("апреля", "04")
        | s when s.Contains("мая") -> s.Replace("мая", "05")
        | s when s.Contains("июня") -> s.Replace("июня", "06")
        | s when s.Contains("июля") -> s.Replace("июля", "07")
        | s when s.Contains("августа") -> s.Replace("августа", "08")
        | s when s.Contains("сентября") -> s.Replace("сентября", "09")
        | s when s.Contains("октября") -> s.Replace("октября", "10")
        | s when s.Contains("ноября") -> s.Replace("ноября", "11")
        | s when s.Contains("декабря") -> s.Replace("декабря", "12")
        | _ -> s
    
    let UnzippedTargz (zipFileName : string) (targetDir : string) =
        let stream = File.OpenRead(zipFileName)
        let gzipStream = new GZipInputStream(stream)
        let tarArchive = TarArchive.CreateInputTarArchive(gzipStream)
        tarArchive.ExtractContents(targetDir)
        gzipStream.Close()
        stream.Close()
    
    let teststring (t : JToken) : string =
        match t with
        | null -> ""
        | _ -> ((string) t).Trim()
    
    let testint (t : JToken) : int =
        match t with
        | null -> 0
        | _ -> (int) t
    
    let testfloat (t : JToken) : float =
        match t with
        | null -> 0.
        | _ -> (float) t
    
    let testdate (t : string) : DateTime =
        match t with
        | null | "null" -> DateTime.MinValue
        | _ -> DateTime.Parse(((string) t).Trim('"'))
