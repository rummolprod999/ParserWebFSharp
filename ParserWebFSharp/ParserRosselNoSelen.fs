namespace ParserWeb

open System.Threading
open AngleSharp.Dom
open AngleSharp.Parser.Html
open System
open System.Collections.Generic

type ParserRosselNoSelen(stn: Settings.T) =
    inherit Parser()
    let set = stn

    let listTenders = List<TenderRossel>()

    member private this.GetPurNum(input: string) : string option =
        match input with
        | Tools.RegexMatch1 @"№(.+) \(" gr1 -> Some(gr1)
        | _ -> None

    member private this.GetPurNumNew(input: string) : string option =
        match input with
        | Tools.RegexMatch1 @"(.+)\s+\(" gr1 -> Some(gr1)
        | _ -> None

    override this.Parsing() =
        try
            match Settings.RosselNum with
            | "1" ->
                try

                    this.ParserSelen
                with
                    | ex -> Logging.Log.logger ex
            | "2" ->
                try

                    this.ParserSelenAtom
                with
                    | ex -> Logging.Log.logger ex
            | "3" ->
                try

                    this.ParserSelenRt
                with
                    | ex -> Logging.Log.logger ex
            | "4" ->
                try

                    this.ParserSelenVtb
                with
                    | ex -> Logging.Log.logger ex
            | "5" ->
                try

                    this.ParserSelenRosteh
                with
                    | ex -> Logging.Log.logger ex
            | "7" ->
                try

                    this.ParserSelenRushidro
                with
                    | ex -> Logging.Log.logger ex
            | "8" ->
                try

                    this.ParserSelenRosgeo
                with
                    | ex -> Logging.Log.logger ex
            | "9" ->
                try

                    this.ParserSelenRosseti
                with
                    | ex -> Logging.Log.logger ex
            | "10" ->
                try

                    this.ParserSelenKim
                with
                    | ex -> Logging.Log.logger ex
            | "11" ->
                try

                    this.ParserSelenBussines
                with
                    | ex -> Logging.Log.logger ex
            | "12" ->
                try

                    this.ParserSelenAll
                with
                    | ex -> Logging.Log.logger ex
            | _ -> ()
        with
            | ex -> Logging.Log.logger ex

        for ten in listTenders do
            try
                Thread.Sleep(5000)
                ten.Parsing()
            with
                | ex -> Logging.Log.logger ex

        listTenders.Clear()
        ()

    member private this.ParserSelen =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=2&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url
            ()



    member private this.ParserSelenBussines =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=24&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()

    member private this.ParserSelenAll =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=all&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()

    member private this.ParserSelenAtom =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=3&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()

    member private this.ParserSelenRt =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=6&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()

    member private this.ParserSelenVtb =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=8&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()

    member private this.ParserSelenRosteh =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=11&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()

    member private this.ParserSelenRushidro =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=14&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()

    member private this.ParserSelenRosseti =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=18&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()


    member private this.ParserSelenRosgeo =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=12&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()

    member private this.ParserSelenKim =
        for i in 0..30 do
            let url =
                "https://www.roseltorg.ru/procedures/search?sale=0&source%5B%5D=21&status%5B%5D=0&status%5B%5D=1&currency=all&page="
                + i.ToString()
                + "&from="
                + ((i * 10)).ToString()

            this.ParserListTenders url

        ()

    member private this.ParserListTenders(url: String) =

        let Page = Download.DownloadString url
        //Thread.Sleep(5000)
        match Page with
        | null
        | "" -> Logging.Log.logger ("Dont get start page", url)
        | s ->
            let parser = HtmlParser()
            let documents = parser.Parse(s)

            let tens =
                documents.QuerySelectorAll("#auction_search_results > div.search-results__item")

            for t in tens do
                try
                    this.ParsingTender t url
                with
                    | ex -> Logging.Log.logger ex

        ()

    member private this.ParsingTender (t: IElement) (url: String) =
        //driver.SwitchTo().DefaultContent() |> ignore
        let purNumT =
            t.QuerySelector("a.search-results__link")

        let purNumM =
            match purNumT with
            | null ->
                raise
                <| NullReferenceException(sprintf "purNum not found in %s" url)
            | x -> x.TextContent.Trim()

        let purNum =
            match this.GetPurNumNew(purNumM) with
            | None ->
                raise
                <| NullReferenceException(sprintf "purNum not found in %s" purNumM)
            | Some pr -> pr.Trim()

        match purNum with
        | _ when purNum.StartsWith("COM") -> this.ParserSelect t purNum 42
        | _ when purNum.StartsWith("ATOM") -> this.ParserSelect t purNum 43
        | _ when purNum.StartsWith("RT") -> this.ParserSelect t purNum 45
        | _ when purNum.StartsWith("VTB") -> this.ParserSelect t purNum 46
        | _ when purNum.StartsWith("OPK") -> this.ParserSelect t purNum 47
        | _ when purNum.StartsWith("RH") -> this.ParserSelect t purNum 48
        | _ when purNum.StartsWith("GEO") -> this.ParserSelect t purNum 49
        | _ when purNum.StartsWith("ROSSETI") -> this.ParserSelect t purNum 50
        | _ when purNum.StartsWith("KIM") -> this.ParserSelect t purNum 260
        | _ when purNum.StartsWith("B") -> this.ParserSelect t purNum 348
        | _ -> this.ParserSelect t purNum 347

    member private this.ParserSelect (t: IElement) (purNum: string) (tFz: int) =
        let hrefT =
            t.QuerySelector("a.search-results__link")

        let href =
            match hrefT with
            | null ->
                raise
                <| NullReferenceException(sprintf "href not found in %s" purNum)
            | x -> "https://www.roseltorg.ru" + x.GetAttribute("href")

        let PurNameT =
            t.QuerySelector("div.search-results__subject > a")

        let purName =
            match PurNameT with
            | null ->
                raise
                <| NullReferenceException(sprintf "purName not found in %s" purNum)
            | x -> x.TextContent.Trim()

        let ten =
            { RosSelRec.Href = href
              PurNum = purNum
              PurName = purName }

        let T = TenderRossel(set, ten, tFz)

        if not
           <| listTenders.Exists(fun t -> t.PurNum = purNum) then
            listTenders.Add(T)
            //Logging.Log.logger (sprintf "purNum is %s" purNum)
