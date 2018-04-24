namespace ParserWeb

open System
open System.Globalization
open System.Text.RegularExpressions
module TypeE = 
    type System.String with
        
        member this.DateFromString(pat : string) = 
            try 
                Some(DateTime.ParseExact(this, pat, CultureInfo.InvariantCulture))
            with ex -> None
        
        member this.DateFromStringRus(pat : string) = 
            try 
                Some(DateTime.ParseExact(this, pat, CultureInfo.CreateSpecificCulture("ru-RU")))
            with ex -> printfn "%O" this
                       printfn "%O" pat
                       None
        
        member this.RegexReplace() =
            Regex.Replace(this, @"\s+", " ")
        
        member this.RegexDeleteWhitespace() =
                    Regex.Replace(this, @"\s+", "")
                    
        member this.ReplaceDate() =
            if this.Contains("января") then  this.Replace(" января ", ".01.")
            elif this.Contains("февраля") then  this.Replace(" февраля ", ".02.")
            elif this.Contains("марта") then  this.Replace(" марта ", ".03.")
            elif this.Contains("апреля") then  this.Replace(" апреля ", ".04.")
            elif this.Contains("мая") then  this.Replace(" мая ", ".05.")
            elif this.Contains("июня") then  this.Replace(" июня ", ".06.")
            elif this.Contains("июля") then  this.Replace(" июля ", ".07.")
            elif this.Contains("августа") then  this.Replace(" августа ", ".08.")
            elif this.Contains("сентября") then  this.Replace(" сентября ", ".09.")
            elif this.Contains("октября") then  this.Replace(" октября ", ".10.")
            elif this.Contains("ноября") then  this.Replace(" ноября ", ".11.")
            elif this.Contains("декабря") then  this.Replace(" декабря ", ".12.")
            else this
