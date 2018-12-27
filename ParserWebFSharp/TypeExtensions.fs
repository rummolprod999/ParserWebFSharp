namespace ParserWeb

open System
open System.Globalization
open System.Text.RegularExpressions
open OpenQA.Selenium

module TypeE =
    open System.Collections.Generic
    
    type System.String with
        
        member this.DateFromString(pat : string) =
            try 
                Some(DateTime.ParseExact(this, pat, CultureInfo.InvariantCulture))
            with ex -> None
        
        member this.DateFromString(pat : string, exc : string) =
            match this.DateFromString(pat) with
            | None -> Error(exc)
            | Some d -> Success(d)
        
        member this.Get1FromRegexp(regex : string) : string option =
            match this with
            | Tools.RegexMatch1 regex gr1 -> Some(gr1)
            | _ -> None
        
        member this.Get1FromRegexpOrDefaul(regex : string) : string =
            match this with
            | Tools.RegexMatch1 regex gr1 -> gr1
            | _ -> ""
        
        member this.Get1Optional(regex : string) =
            match this.Get1FromRegexp(regex) with
            | None -> Success("")
            | Some e -> Success(e.Trim())
        
        member this.Get1(regex : string, exc : string) =
            match this.Get1FromRegexp(regex) with
            | None -> Error(exc)
            | Some e -> Success(e.Trim())
        
        member this.Get2FromRegexp(regex : string) : (string * string) option =
            match this with
            | Tools.RegexMatch2 regex (gr1, gr2) -> Some(gr1, gr2)
            | _ -> None
        
        member this.Get2FromRegexpOptional(regex : string, exc : string) =
            match this with
            | Tools.RegexMatch2 regex (gr1, gr2) -> Success(gr1, gr2)
            | _ -> Error(exc)
        
        member this.GetPriceFromString(?template) : string =
            let templ = defaultArg template @"([\d, ]+)"
            match this.Get1FromRegexp templ with
            | Some x -> Regex.Replace(x.Replace(",", ".").Trim(), @"\s+", "")
            | None -> ""
        
        member this.DateFromStringRus(pat : string) =
            try 
                Some(DateTime.ParseExact(this, pat, CultureInfo.CreateSpecificCulture("ru-RU")))
            with ex -> 
                printfn "%O" this
                printfn "%O" pat
                None
        
        member this.RegexReplace() = Regex.Replace(this, @"\s+", " ")
        member this.RegexDeleteWhitespace() = Regex.Replace(this, @"\s+", "")
        member this.RegexCutWhitespace() = Regex.Replace(this, @"\s+", " ")
        
        member this.GetNmck() =
            let tmp = this.Replace(",", ".").RegexDeleteWhitespace()
            tmp
        
        member this.ReplaceDate() =
            if this.Contains("января") then this.Replace(" января ", ".01.")
            elif this.Contains("февраля") then this.Replace(" февраля ", ".02.")
            elif this.Contains("марта") then this.Replace(" марта ", ".03.")
            elif this.Contains("апреля") then this.Replace(" апреля ", ".04.")
            elif this.Contains("мая") then this.Replace(" мая ", ".05.")
            elif this.Contains("июня") then this.Replace(" июня ", ".06.")
            elif this.Contains("июля") then this.Replace(" июля ", ".07.")
            elif this.Contains("августа") then this.Replace(" августа ", ".08.")
            elif this.Contains("сентября") then this.Replace(" сентября ", ".09.")
            elif this.Contains("октября") then this.Replace(" октября ", ".10.")
            elif this.Contains("ноября") then this.Replace(" ноября ", ".11.")
            elif this.Contains("декабря") then this.Replace(" декабря ", ".12.")
            else this
        
        member this.ReplaceDateAsgor() =
            if this.Contains("Января") then this.Replace("Января", "01")
            elif this.Contains("Февраля") then this.Replace("Февраля", "02")
            elif this.Contains("Марта") then this.Replace("Марта", "03")
            elif this.Contains("Апреля") then this.Replace("Апреля", "04")
            elif this.Contains("Мая") then this.Replace("Мая", "05")
            elif this.Contains("Июня") then this.Replace("Июня", "06")
            elif this.Contains("Июля") then this.Replace("Июля", "07")
            elif this.Contains("Августа") then this.Replace("Августа", "08")
            elif this.Contains("Сентября") then this.Replace("Сентября", "09")
            elif this.Contains("Октября") then this.Replace("Октября", "10")
            elif this.Contains("Ноября") then this.Replace("Ноября", "11")
            elif this.Contains("Декабря") then this.Replace("Декабря", "12")
            else this
    
    type HtmlAgilityPack.HtmlNode with
        
        member this.Gsn(s : string) =
            match this.SelectSingleNode(s) with
            | null -> ""
            | e -> e.InnerText.Trim()
        
        member this.GsnAtr (s : string) (atr : string) =
            match this.SelectSingleNode(s) with
            | null -> ""
            | e -> 
                match e.Attributes.[atr] with
                | null -> ""
                | at -> at.Value.Trim()
        
        member this.getAttrWithoutException (atr : string) =
            match this.Attributes.[atr] with
            | null -> ""
            | at -> at.Value.Trim()
    
    type HtmlAgilityPack.HtmlNodeNavigator with
        
        member this.Gsn(s : string) =
            match this.SelectSingleNode(s) with
            | null -> ""
            | e -> e.Value.Trim()
        
        member this.GsnAtr (s : string) (atr : string) =
            match this.SelectSingleNode(s) with
            | null -> ""
            | e -> 
                match e.GetAttribute(atr, "") with
                | null -> ""
                | at -> at.Trim()
    
    type ISearchContext with
        
        member this.findElementWithoutException (xpath : string) =
            try 
                let res = this.FindElement(By.XPath(xpath))
                match res with
                | null -> ""
                | r -> r.Text.Trim()
            with ex -> ""
        
        member this.findElementsWithoutException (xpath : string) =
            try 
                let res = this.FindElements(By.XPath(xpath))
                match res with
                | null -> 
                    new Collections.ObjectModel.ReadOnlyCollection<IWebElement>((new List<IWebElement>()) :> IList<IWebElement>)
                | r -> r
            with ex -> 
                new Collections.ObjectModel.ReadOnlyCollection<IWebElement>((new List<IWebElement>()) :> IList<IWebElement>)
        
        member this.findElementWithoutException (xpath : string, exc : string) =
            try 
                let res = this.FindElement(By.XPath(xpath))
                match res with
                | null -> Error(exc)
                | r -> Success(r.Text.Trim())
            with ex -> Error(exc)
        
        member this.findElementWithoutExceptionOptional (xpath : string, exc : string) =
            try 
                let res = this.FindElement(By.XPath(xpath))
                match res with
                | null -> Success("")
                | r -> Success(r.Text.Trim())
            with ex -> Success("")
    
    type IWebElement with
        member this.findAttributeWithoutException (attr : string, exc : string) =
            try 
                let attr = this.GetAttribute(attr)
                Success(attr)
            with ex -> Error(exc)
