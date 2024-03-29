namespace ParserWeb

open System
open System.Globalization
open System.Text.RegularExpressions
open System.Web
open OpenQA.Selenium

module TypeE =
    open System.Collections.Generic

    type String with

        member this.DateFromString(pat: string) =
            try
                Some(DateTime.ParseExact(this, pat, CultureInfo.InvariantCulture))
            with
                | ex -> None

        member this.DateFromStringOrMin(pat: string) =
            try
                DateTime.ParseExact(this, pat, CultureInfo.InvariantCulture)
            with
                | ex -> DateTime.MinValue
        
        member this.DateFromStringOrCurr(pat: string) =
            try
                DateTime.ParseExact(this, pat, CultureInfo.InvariantCulture)
            with
                | ex -> DateTime.Today

        member this.DateFromStringOrPubPlus2(pat: string, datePub: DateTime) =
            try
                DateTime.ParseExact(this, pat, CultureInfo.InvariantCulture)
            with
                | ex -> datePub.AddDays(2.)

        member this.DateFromString(pat: string, exc: string) =
            match this.DateFromString(pat) with
            | None -> Error(exc)
            | Some d -> Success(d)

        member this.DateFromStringDoc(pat: string, exc: string) =
            match this.DateFromString(pat) with
            | None -> Err(exc)
            | Some d -> Succ(d)

        member this.DateFromStringDocMin(pat: string) =
            match this.DateFromString(pat) with
            | None -> Succ(DateTime.MinValue)
            | Some d -> Succ(d)

        member this.DateFromStringDocNow(pat: string) =
            match this.DateFromString(pat) with
            | None -> Succ(DateTime.Now)
            | Some d -> Succ(d)

        member this.Get1FromRegexp(regex: string) : string option =
            match this with
            | Tools.RegexMatch1 regex gr1 -> Some(gr1)
            | _ -> None

        member this.Get1FromRegexpDotAll(regex: string) : string option =
            match this with
            | Tools.RegexMatch1DotALL regex gr1 -> Some(gr1)
            | _ -> None

        member this.Get1FromRegexpOrDefaul(regex: string) : string =
            match this with
            | Tools.RegexMatch1 regex gr1 -> gr1
            | _ -> ""

        member this.Get1Optional(regex: string) =
            match this.Get1FromRegexp(regex) with
            | None -> Success("")
            | Some e -> Success(e.Trim())
        
        member this.Get1OptionalOrDefault(regex: string, def: string) =
            match this.Get1FromRegexp(regex) with
            | None -> Success(def)
            | Some e -> Success(e.Trim())

        member this.Get1OptionalDoc(regex: string) =
            match this.Get1FromRegexp(regex) with
            | None -> Succ("")
            | Some e -> Succ(e.Trim())

        member this.Get1Doc (regex: string) (exc: string) =
            match this.Get1FromRegexp(regex) with
            | None -> Err(exc)
            | Some e -> Succ(e.Trim())

        member this.Get1(regex: string, exc: string) =
            match this.Get1FromRegexp(regex) with
            | None -> Error(exc)
            | Some e -> Success(e.Trim())

        member this.Get2FromRegexp(regex: string) : (string * string) option =
            match this with
            | Tools.RegexMatch2 regex (gr1, gr2) -> Some(gr1, gr2)
            | _ -> None

        member this.Get4FromRegexp(regex: string) : (string * string * string * string) option =
            match this with
            | Tools.RegexMatch4 regex (gr1, gr2, gr3, gr4) -> Some(gr1, gr2, gr3, gr4)
            | _ -> None

        member this.Get2ListRegexp(regex: string) =
            match this.Get2FromRegexp(regex) with
            | Some (gr1, gr2) -> [ gr1; gr2 ]
            | _ -> [ ""; "" ]

        member this.Get4ListRegexp(regex: string) =
            match this.Get4FromRegexp(regex) with
            | Some (gr1, gr2, gr3, gr4) -> [ gr1; gr2; gr3; gr4 ]
            | _ -> [ ""; ""; ""; "" ]

        member this.Get2FromRegexpOptional(regex: string, exc: string) =
            match this with
            | Tools.RegexMatch2 regex (gr1, gr2) -> Success(gr1, gr2)
            | _ -> Error(exc)

        member this.GetPriceFromString(?template) : string =
            let templ = defaultArg template @"([\d, ]+)"

            match this.Get1FromRegexp templ with
            | Some x -> Regex.Replace(x.Replace(",", ".").Trim(), @"\s+", "")
            | None -> ""

        member this.GetPriceFromStringKz(?template) : string =
            let _ = defaultArg template @"([\d, ]+)"

            let th =
                this.Replace("&nbsp;", "").Replace("тг", "")

            match Some(th) with
            | Some x -> Regex.Replace(x.Replace(",", ".").Trim(), @"\s+", "")
            | None -> ""

        member this.DateFromStringRus(pat: string) =
            try
                Some(DateTime.ParseExact(this, pat, CultureInfo.CreateSpecificCulture("ru-RU")))
            with
                | ex ->
                    printfn "%O" this
                    printfn "%O" pat
                    None

        member this.RegexReplace() = Regex.Replace(this, @"\s+", " ")
        member this.RegexDeleteWhitespace() = Regex.Replace(this, @"\s+", "")
        member this.RegexCutWhitespace() = Regex.Replace(this, @"\s+", " ")
        member this.HtmlDecode() = HttpUtility.HtmlDecode(this)

        member this.GetNmck() =
            let tmp =
                this.Replace(",", ".").RegexDeleteWhitespace()

            tmp

        member this.ReplaceDate() =
            if this.Contains("января") then
                this.Replace(" января ", ".01.")
            elif this.Contains("февраля") then
                this.Replace(" февраля ", ".02.")
            elif this.Contains("марта") then
                this.Replace(" марта ", ".03.")
            elif this.Contains("апреля") then
                this.Replace(" апреля ", ".04.")
            elif this.Contains("мая") then
                this.Replace(" мая ", ".05.")
            elif this.Contains("июня") then
                this.Replace(" июня ", ".06.")
            elif this.Contains("июля") then
                this.Replace(" июля ", ".07.")
            elif this.Contains("августа") then
                this.Replace(" августа ", ".08.")
            elif this.Contains("сентября") then
                this.Replace(" сентября ", ".09.")
            elif this.Contains("октября") then
                this.Replace(" октября ", ".10.")
            elif this.Contains("ноября") then
                this.Replace(" ноября ", ".11.")
            elif this.Contains("декабря") then
                this.Replace(" декабря ", ".12.")
            else
                this

        member this.ReplaceDateBidMart() =
            if this.Contains("января") then
                this.Replace(" января", ".01")
            elif this.Contains("февраля") then
                this.Replace(" февраля", ".02")
            elif this.Contains("марта") then
                this.Replace(" марта", ".03")
            elif this.Contains("апреля") then
                this.Replace(" апреля", ".04")
            elif this.Contains("мая") then
                this.Replace(" мая", ".05")
            elif this.Contains("июня") then
                this.Replace(" июня", ".06")
            elif this.Contains("июля") then
                this.Replace(" июля", ".07")
            elif this.Contains("августа") then
                this.Replace(" августа", ".08")
            elif this.Contains("сентября") then
                this.Replace(" сентября", ".09")
            elif this.Contains("октября") then
                this.Replace(" октября", ".10")
            elif this.Contains("ноября") then
                this.Replace(" ноября", ".11")
            elif this.Contains("декабря") then
                this.Replace(" декабря", ".12")
            else
                this

        member this.ReplaceDateAriba() =
            if this.Contains("янв") then
                this.Replace(" янв ", ".01.")
            elif this.Contains("фев") then
                this.Replace(" фев ", ".02.")
            elif this.Contains("мар") then
                this.Replace(" мар ", ".03.")
            elif this.Contains("апр") then
                this.Replace(" апр ", ".04.")
            elif this.Contains("мая") then
                this.Replace(" мая ", ".05.")
            elif this.Contains("июн") then
                this.Replace(" июн ", ".06.")
            elif this.Contains("июл") then
                this.Replace(" июл ", ".07.")
            elif this.Contains("авг") then
                this.Replace(" авг ", ".08.")
            elif this.Contains("сен") then
                this.Replace(" сен ", ".09.")
            elif this.Contains("окт") then
                this.Replace(" окт ", ".10.")
            elif this.Contains("ноя") then
                this.Replace(" ноя ", ".11.")
            elif this.Contains("дек") then
                this.Replace(" дек ", ".12.")
            else
                this

        member this.ReplaceDateSib() =
            if this.Contains("янв") then
                this.Replace(" янв ", ".01.")
            elif this.Contains("фев") then
                this.Replace(" фев ", ".02.")
            elif this.Contains("мар") then
                this.Replace(" мар ", ".03.")
            elif this.Contains("апр") then
                this.Replace(" апр ", ".04.")
            elif this.Contains("май") then
                this.Replace(" май ", ".05.")
            elif this.Contains("июн") then
                this.Replace(" июн ", ".06.")
            elif this.Contains("июл") then
                this.Replace(" июл ", ".07.")
            elif this.Contains("авг") then
                this.Replace(" авг ", ".08.")
            elif this.Contains("сен") then
                this.Replace(" сен ", ".09.")
            elif this.Contains("окт") then
                this.Replace(" окт ", ".10.")
            elif this.Contains("ноя") then
                this.Replace(" ноя ", ".11.")
            elif this.Contains("дек") then
                this.Replace(" дек ", ".12.")
            else
                this

        member this.ReplaceDateAsgor() =
            if this.Contains("Января") then
                this.Replace("Января", "01")
            elif this.Contains("Февраля") then
                this.Replace("Февраля", "02")
            elif this.Contains("Марта") then
                this.Replace("Марта", "03")
            elif this.Contains("Апреля") then
                this.Replace("Апреля", "04")
            elif this.Contains("Мая") then
                this.Replace("Мая", "05")
            elif this.Contains("Июня") then
                this.Replace("Июня", "06")
            elif this.Contains("Июля") then
                this.Replace("Июля", "07")
            elif this.Contains("Августа") then
                this.Replace("Августа", "08")
            elif this.Contains("Сентября") then
                this.Replace("Сентября", "09")
            elif this.Contains("Октября") then
                this.Replace("Октября", "10")
            elif this.Contains("Ноября") then
                this.Replace("Ноября", "11")
            elif this.Contains("Декабря") then
                this.Replace("Декабря", "12")
            else
                this

    type AngleSharp.Dom.IParentNode with

        member this.GsnDoc(selector: string) =
            match this.QuerySelector(selector) with
            | null -> Succ("")
            | e -> Succ(e.TextContent.Trim())

        member this.GsnDocWithError (selector: string) (err: string) =
            match this.QuerySelector(selector) with
            | null -> Err(err)
            | e -> Succ(e.TextContent.Trim())

        member this.GsnAtrDoc (selector: string) (atr: string) =
            match this.QuerySelector(selector) with
            | null -> Succ("")
            | e ->
                match e.GetAttribute(atr) with
                | null -> Succ("")
                | at -> Succ(at.Trim())

        member this.GsnAtrDocWithError (selector: string) (atr: string) (err: string) =
            match this.QuerySelector(selector) with
            | null -> Err(err)
            | e ->
                match e.GetAttribute(atr) with
                | null -> Err(err)
                | at -> Succ(at.Trim())

        member this.GsnAtrSelfDocWithError (atr: string) (err: string) =
            let el = this :?> AngleSharp.Dom.IElement

            match el.GetAttribute(atr) with
            | null -> Err(err)
            | at -> Succ(at.Trim())

    type HtmlAgilityPack.HtmlNode with

        member this.Gsn(s: string) =
            match this.SelectSingleNode(s) with
            | null -> ""
            | e -> e.InnerText.Trim()

        member this.GsnDoc(xpath: string) =
            match this.SelectSingleNode(xpath) with
            | null -> Succ("")
            | e -> Succ(e.InnerText.Trim())

        member this.GsnDocWithError (xpath: string) (err: string) =
            match this.SelectSingleNode(xpath) with
            | null -> Err(err)
            | e -> Succ(e.InnerText.Trim())

        member this.GsnAtr (s: string) (atr: string) =
            match this.SelectSingleNode(s) with
            | null -> ""
            | e ->
                match e.Attributes.[atr] with
                | null -> ""
                | at -> at.Value.Trim()

        member this.GsnAtrDoc (xpath: string) (atr: string) =
            match this.SelectSingleNode(xpath) with
            | null -> Succ("")
            | e ->
                match e.Attributes.[atr] with
                | null -> Succ("")
                | at -> Succ(at.Value.Trim())

        member this.GsnAtrDocWithError (xpath: string) (atr: string) (err: string) =
            match this.SelectSingleNode(xpath) with
            | null -> Err(err)
            | e ->
                match e.Attributes.[atr] with
                | null -> Err(err)
                | at -> Succ(at.Value.Trim())

        member this.getAttrWithoutException(atr: string) =
            match this.Attributes.[atr] with
            | null -> ""
            | at -> at.Value.Trim()

    type HtmlAgilityPack.HtmlNodeNavigator with

        member this.Gsn(s: string) =
            match this.SelectSingleNode(s) with
            | null -> ""
            | e -> e.Value.Trim()

        member this.GsnAtr (s: string) (atr: string) =
            match this.SelectSingleNode(s) with
            | null -> ""
            | e ->
                match e.GetAttribute(atr, "") with
                | null -> ""
                | at -> at.Trim()

        member this.GsnDoc(xpath: string) =
            match this.SelectSingleNode(xpath) with
            | null -> Succ("")
            | e -> Succ(e.Value.Trim())

        member this.GsnDocWithError (xpath: string) (err: string) =
            match this.SelectSingleNode(xpath) with
            | null -> Err(err)
            | e -> Succ(e.Value.Trim())

        member this.GsnAtrDoc (xpath: string) (atr: string) =
            match this.SelectSingleNode(xpath) with
            | null -> Succ("")
            | e ->
                match e.GetAttribute(atr, "") with
                | null -> Succ("")
                | at -> Succ(at.Trim())

        member this.GsnAtrDocWithError (xpath: string) (atr: string) (err: string) =
            match this.SelectSingleNode(xpath) with
            | null -> Err(err)
            | e ->
                match e.GetAttribute(atr, "") with
                | null -> Err(err)
                | at -> Succ(at.Trim())

    type ISearchContext with

        member this.findElementWithoutException(xpath: string) =
            try
                let res = this.FindElement(By.XPath(xpath))

                match res with
                | null -> ""
                | r -> r.Text.Trim()
            with
                | ex -> ""

        member this.findElementsWithoutException(xpath: string) =
            try
                let res = this.FindElements(By.XPath(xpath))

                match res with
                | null ->
                    Collections.ObjectModel.ReadOnlyCollection<IWebElement>((List<IWebElement>()) :> IList<IWebElement>)
                | r -> r
            with
                | ex ->
                    Collections.ObjectModel.ReadOnlyCollection<IWebElement>((List<IWebElement>()) :> IList<IWebElement>)

        member this.findElementWithoutException(xpath: string, exc: string) =
            try
                let res = this.FindElement(By.XPath(xpath))

                match res with
                | null -> Error(exc)
                | r -> Success(r.Text.Trim())
            with
                | ex -> Error(exc)

        member this.findWElementWithoutException(xpath: string, exc: string) =
            try
                let res = this.FindElement(By.XPath(xpath))

                match res with
                | null -> Error(exc)
                | r -> Success(r)
            with
                | ex -> Error(exc)

        member this.findElementWithoutExceptionOptional(xpath: string, _: string) =
            try
                let res = this.FindElement(By.XPath(xpath))

                match res with
                | null -> Success("")
                | r -> Success(r.Text.Trim())
            with
                | ex -> Success("")

        member this.findWElementAttrWithoutException(xpath: string, attr: string, exc: string) =
            try
                let res = this.FindElement(By.XPath(xpath))

                match res with
                | null -> Error(exc)
                | r ->
                    try
                        let attr = r.GetAttribute(attr)
                        Success(attr)
                    with
                        | ex -> Error(exc)
            with
                | ex -> Error(exc)

        member this.findWElementAttrOrEmpty(xpath: string, attr: string) =
            try
                let res = this.FindElement(By.XPath(xpath))

                match res with
                | null -> ""
                | r ->
                    try
                        let attr = r.GetAttribute(attr)
                        attr
                    with
                        | ex -> ""
            with
                | ex -> ""

    type IWebElement with
        member this.findAttributeWithoutException(attr: string, exc: string) =
            try
                let attr = this.GetAttribute(attr)
                Success(attr)
            with
                | ex -> Error(exc)

    type IWebElement with
        member this.findAttributeOrEmpty(attr: string) =
            try
                let attr = this.GetAttribute(attr)
                Success(attr)
            with
                | ex -> Success("")

    type List<'T> with
        member x.RemoveAllFromList(ls: IEnumerable<_>) =
            for el in ls do
                x.Remove(el) |> ignore

    type HtmlAgilityPack.HtmlNode with
        member this.SelectNodesOrEmpty(xpath: string) =
            match this.SelectNodes(xpath) with
            | null -> HtmlAgilityPack.HtmlNodeCollection(null)
            | x -> x
