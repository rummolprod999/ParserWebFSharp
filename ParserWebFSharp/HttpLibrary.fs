namespace ParserWeb

open System
open System.Collections.Generic
open System.IO
open System.Net
open System.Text
open System.Threading
open System.Threading.Tasks
open System.Net.Http
open RandomUserAgent

module Download =
    type TimedWebClient() =
        inherit WebClient()

        override this.GetWebRequest(address: Uri) =
            let wr =
                ``base``.GetWebRequest(address) :?> HttpWebRequest

            wr.Timeout <- 60000
            wr.UserAgent <- "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0"
            wr :> WebRequest

    type TimedWebClientBot() =
        inherit WebClient()

        override this.GetWebRequest(address: Uri) =
            let wr =
                ``base``.GetWebRequest(address) :?> HttpWebRequest

            wr.Timeout <- 60000

            wr.UserAgent <-
                "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots) Gecko/20100101 Firefox/55.0"

            wr.AutomaticDecompression <-
                DecompressionMethods.GZip
                ||| DecompressionMethods.Deflate
                ||| DecompressionMethods.None

            wr :> WebRequest

    type TimedWebClientRtsGen() =
        inherit WebClient()

        override this.GetWebRequest(address: Uri) =
            let wr =
                ``base``.GetWebRequest(address) :?> HttpWebRequest

            wr.Timeout <- 60000

            wr.UserAgent <-
                "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots) Gecko/20100101 Firefox/55.0"
            let cookie = "ASP.NET_SessionId=" + Settings.RtsSessionId + "; " + "223_SecurityTokenKey=" + Settings.RtsSecToken + "; " + ".223=" + Settings.Rts223
            wr.Headers.Add("Cookie", cookie)

            wr.AutomaticDecompression <-
                DecompressionMethods.GZip
                ||| DecompressionMethods.Deflate
                ||| DecompressionMethods.None

            wr :> WebRequest
    
    type TimedWebClientRtsGenDoc(token: String) =
        inherit WebClient()

        override this.GetWebRequest(address: Uri) =
            let wr =
                ``base``.GetWebRequest(address) :?> HttpWebRequest

            wr.Timeout <- 60000

            wr.UserAgent <-
                "Mozilla/5.0 (compatible; YandexBot/3.0; +http://yandex.com/bots) Gecko/20100101 Firefox/55.0"
            let cookie = "ASP.NET_SessionId=" + Settings.RtsSessionId + "; " + "223_SecurityTokenKey=" + Settings.RtsSecToken + "; " + ".223=" + Settings.Rts223
            wr.Headers.Add("Cookie", cookie)
            wr.Headers.Add("X-JwtToken-TradeDocumentsForGrid", token)

            wr.AutomaticDecompression <-
                DecompressionMethods.GZip
                ||| DecompressionMethods.Deflate
                ||| DecompressionMethods.None

            wr :> WebRequest

    type TimedWebClientCookies() =
        inherit WebClient()

        override this.GetWebRequest(address: Uri) =
            let wr =
                ``base``.GetWebRequest(address) :?> HttpWebRequest

            wr.Timeout <- 60000
            wr.UserAgent <- "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0"
            wr.Headers.Add("Cookie", "auth_sess=ent.it%40yandex.ru+landa541531; session_id=512185358")
            wr :> WebRequest

    type TimedWebClientCookiesTenderer() =
        inherit WebClient()

        override this.GetWebRequest(address: Uri) =
            let wr =
                ``base``.GetWebRequest(address) :?> HttpWebRequest

            wr.Timeout <- 600000
            wr.UserAgent <- "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0"
            wr.Headers.Add("Cookie", (sprintf "auth_sess=%s" Settings.UserTenderer))
            wr :> WebRequest

    type TimedWebClientIrkutsk() =
        inherit WebClient()

        override this.GetWebRequest(address: Uri) =
            let wr =
                ``base``.GetWebRequest(address) :?> HttpWebRequest

            wr.Timeout <- 60000
            wr.KeepAlive <- true

            wr.UserAgent <-
                "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.56 Safari/537.36"

            wr.Headers.Add("Referer", "https://tenders.irkutskoil.ru/")

            wr.Headers.Add(
                "Accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9"
            )

            wr.Headers.Add("DNT", "1")
            //let r = wr.GetResponse()
            //printfn "%d" r.ContentLength
            //wr.Referer <- "https://tenders.irkutskoil.ru/"
            wr :> WebRequest

    let DownloadString url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run(fun () -> (new TimedWebClient()).DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(60.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | _ ->
                    if !count >= 3 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        s

    let DownloadStringRts url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run (fun () ->
                        (new TimedWebClientRtsGen())
                            .DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(30.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | e ->
                    if !count >= 1 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток %s" url !count e.Message)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(3000)

        s
    
    let DownloadStringRtsDoc url token =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run (fun () ->
                        (new TimedWebClientRtsGenDoc(token))
                            .DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(30.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | e ->
                    if !count >= 1 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток %s" url !count e.Message)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(3000)

        s
        
    let DownloadStringBot url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run (fun () ->
                        (new TimedWebClientBot())
                            .DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(60.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | _ ->
                    if !count >= 3 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        s

    let DownloadStringIrkutsk url =
        ServicePointManager.ServerCertificateValidationCallback <- fun sender certificate chain sslPolicyErrors -> true
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run (fun () ->
                        (new TimedWebClientIrkutsk())
                            .DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(30.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | ex ->
                    Logging.Log.logger ex

                    if !count >= 3 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        s

    let DownloadString1251 url =
        ServicePointManager.ServerCertificateValidationCallback <- fun sender certificate chain sslPolicyErrors -> true
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        let getWebClient () =
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            let a = new TimedWebClient()
            a.Encoding <- Encoding.GetEncoding("windows-1251")
            a

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run(fun () -> (getWebClient ()).DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(30.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | _ ->
                    if !count >= 3 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        s

    let DownloadString1251Bot url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        let getWebClient () =
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            let a = new TimedWebClientBot()
            a.Encoding <- Encoding.GetEncoding("windows-1251")
            a

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run(fun () -> (getWebClient ()).DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(30.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | _ ->
                    if !count >= 5 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        s

    let DownloadStringUtf8Bot url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        let getWebClient () =
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            let a = new TimedWebClientBot()
            a

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run(fun () -> (getWebClient ()).DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(30.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | _ ->
                    if !count >= 5 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        s

    let DownloadFileSimple (url: string) (patharch: string) : FileInfo =
        let mutable ret = null
        let downCount = ref 0
        let mutable cc = true

        while cc do
            try
                let wc = new WebClient()
                wc.DownloadFile(url, patharch)
                ret <- FileInfo(patharch)
                cc <- false
            with
                | _ ->
                    let FileD = FileInfo(patharch)
                    if FileD.Exists then FileD.Delete()

                    if !downCount = 0 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !downCount)
                        cc <- false
                    else
                        decr downCount

                    Thread.Sleep(5000)

        ret

    let DownloadString1251Cookies url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        let getWebClient () =
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            let a = new TimedWebClientCookies()
            a.Encoding <- Encoding.GetEncoding("windows-1251")
            a

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run(fun () -> (getWebClient ()).DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(30.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | _ ->
                    if !count >= 3 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        s

    let DownloadString1251CookiesTenderer url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true

        let getWebClient () =
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            let a = new TimedWebClientCookiesTenderer()
            a.Encoding <- Encoding.GetEncoding("windows-1251")
            a

        while continueLooping do
            try
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task =
                    Task.Run(fun () -> (getWebClient ()).DownloadString(url: string))

                if task.Wait(TimeSpan.FromSeconds(650.)) then
                    s <- task.Result
                    continueLooping <- false
                else
                    raise <| TimeoutException()
            with
                | _ ->
                    if !count >= 3 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        continueLooping <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        s

    let DownloadSmartTender (page: int) : string =
        let url =
            "https://smarttender.biz/ProZorroTenders/GetTenders/"

        let json =
            "{'searchParam':{'TradeSegment':1,'TenderMode':1,'Page':"
            + (page.ToString())
            + ",'ClassificationGroupId':null,'Sorting':2,'AssignedManagerIds':[],'OrganizerIds':[],'TenderStatuses':[],'GroupedBiddingTypeCodes':[],'BiddingTypeCodes':[],'AddressSearchTypes':[1],'RegionInfos':[],'CategoryIds':[],'AwardStatusCodes':[],'MainProcurementCategoryIds':[],'RationaleIds':[],'PaymentTermTypeIds':[],'MyFilterId':null}}"

        let mutable ret = null
        let count = ref 0
        let mutable cc = true

        while cc do
            try
                use client = new HttpClient()

                let response =
                    client
                        .PostAsync(
                            url,
                            new StringContent(json, Encoding.UTF8, "application/json")
                        )
                        .Result

                ret <- response.Content.ReadAsStringAsync().Result
                cc <- false
            with
                | ex ->
                    Logging.Log.logger (ex)

                    if !count >= 3 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        cc <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        ret

    let DownloadPost (dict: Dictionary<string, string>, url: string) : string =
        let content =
            new FormUrlEncodedContent(dict) :> HttpContent

        let mutable ret = null
        let count = ref 0
        let mutable cc = true

        while cc do
            try
                use client = new HttpClient()

                let response: Task<HttpResponseMessage> =
                    client.PostAsync(url, content)

                ret <- response.Result.Content.ReadAsStringAsync().Result
                cc <- false
            with
                | ex ->
                    Logging.Log.logger (ex)

                    if !count >= 3 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        cc <- false
                    else
                        incr count

                    Thread.Sleep(5000)

        ret

    let DownloadUseProxy (useProxy: bool, url: string) : string =
        let mutable ret = null
        let count = ref 0
        let mutable cc = true

        while cc do
            try
                let httpClientHandler =
                    new HttpClientHandler()

                httpClientHandler.AllowAutoRedirect <- true

                if useProxy then
                    let prixyEntity = ProxyLoader.GetRandomProxy
                    let proxy = WebProxy()
                    proxy.Address <- Uri(sprintf "http://%s:%d" prixyEntity.Ip prixyEntity.Port)
                    proxy.BypassProxyOnLocal <- false
                    proxy.UseDefaultCredentials <- false
                    proxy.Credentials <- NetworkCredential(prixyEntity.User, prixyEntity.Pass)
                    httpClientHandler.Proxy <- proxy

                use client =
                    new HttpClient(httpClientHandler)

                client.DefaultRequestHeaders.Add("User-Agent", RandomUa.RandomUserAgent)

                let response: Task<HttpResponseMessage> =
                    client.GetAsync(url)

                ret <- response.Result.Content.ReadAsStringAsync().Result
                cc <- false
            with
                | ex ->
                    //Logging.Log.logger(ex)
                    if !count >= 3 then
                        Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                        cc <- false
                    else
                        incr count

        ret
