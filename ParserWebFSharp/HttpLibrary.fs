namespace ParserWeb

open System
open System.IO
open System.Net
open System.Text
open System.Threading
open System.Threading.Tasks

module Download =
    type TimedWebClient() =
        inherit WebClient()
        override this.GetWebRequest(address : Uri) =
            let wr = base.GetWebRequest(address) :?> HttpWebRequest
            wr.Timeout <- 600000
            wr.UserAgent <- "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0"
            wr :> WebRequest
    
    type TimedWebClientCookies() =
        inherit WebClient()
        override this.GetWebRequest(address : Uri) =
            let wr = base.GetWebRequest(address) :?> HttpWebRequest
            wr.Timeout <- 600000
            wr.UserAgent <- "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0"
            wr.Headers.Add("Cookie", "auth_sess=alex19840606%40mail.ru+1234567; session_id=411084706")
            wr :> WebRequest
    
    type TimedWebClientCookiesTenderer() =
        inherit WebClient()
        override this.GetWebRequest(address : Uri) =
            let wr = base.GetWebRequest(address) :?> HttpWebRequest
            wr.Timeout <- 600000
            wr.UserAgent <- "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0"
            wr.Headers.Add("Cookie", (sprintf "auth_sess=%s" Settings.UserTenderer))
            wr :> WebRequest
    
    type TimedWebClientIrkutsk() =
        inherit WebClient()
        override this.GetWebRequest(address : Uri) =
            let wr = base.GetWebRequest(address) :?> HttpWebRequest
            wr.Timeout <- 600000
            wr.KeepAlive <- true
            wr.UserAgent <- "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:63.0) Gecko/20100101 Firefox/63.0"
            wr.Headers.Add("Referer", "https://tenders.irkutskoil.ru/")
            wr.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8")
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
                let task = Task.Run(fun () -> (new TimedWebClient()).DownloadString(url : string))
                if task.Wait(TimeSpan.FromSeconds(650.)) then 
                    s <- task.Result
                    continueLooping <- false
                else raise <| new TimeoutException()
            with _ -> 
                if !count >= 100 then 
                    Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                    continueLooping <- false
                else incr count
                Thread.Sleep(5000)
        s
    
    let DownloadStringIrkutsk url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true
        while continueLooping do
            try 
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task = Task.Run(fun () -> (new TimedWebClientIrkutsk()).DownloadString(url : string))
                if task.Wait(TimeSpan.FromSeconds(650.)) then 
                    s <- task.Result
                    continueLooping <- false
                else raise <| new TimeoutException()
            with _ -> 
                if !count >= 100 then 
                    Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                    continueLooping <- false
                else incr count
                Thread.Sleep(5000)
        s
    
    let DownloadString1251 url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true
        
        let getWebClient() =
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            let a = new TimedWebClient()
            a.Encoding <- Encoding.GetEncoding("windows-1251")
            a
        while continueLooping do
            try 
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task = Task.Run(fun () -> (getWebClient()).DownloadString(url : string))
                if task.Wait(TimeSpan.FromSeconds(650.)) then 
                    s <- task.Result
                    continueLooping <- false
                else raise <| new TimeoutException()
            with _ -> 
                if !count >= 100 then 
                    Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                    continueLooping <- false
                else incr count
                Thread.Sleep(5000)
        s
    
    let DownloadFileSimple (url : string) (patharch : string) : FileInfo =
        let mutable ret = null
        let downCount = ref 0
        let mutable cc = true
        while cc do
            try 
                let wc = new WebClient()
                wc.DownloadFile(url, patharch)
                ret <- new FileInfo(patharch)
                cc <- false
            with _ -> 
                let FileD = new FileInfo(patharch)
                if FileD.Exists then FileD.Delete()
                if !downCount = 0 then 
                    Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !downCount)
                    cc <- false
                else decr downCount
                Thread.Sleep(5000)
        ret
    
    let DownloadString1251Cookies url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true
        
        let getWebClient() =
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            let a = new TimedWebClientCookies()
            a.Encoding <- Encoding.GetEncoding("windows-1251")
            a
        while continueLooping do
            try 
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task = Task.Run(fun () -> (getWebClient()).DownloadString(url : string))
                if task.Wait(TimeSpan.FromSeconds(650.)) then 
                    s <- task.Result
                    continueLooping <- false
                else raise <| new TimeoutException()
            with _ -> 
                if !count >= 100 then 
                    Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                    continueLooping <- false
                else incr count
                Thread.Sleep(5000)
        s
    
    let DownloadString1251CookiesTenderer url =
        let mutable s = null
        let count = ref 0
        let mutable continueLooping = true
        
        let getWebClient() =
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance)
            let a = new TimedWebClientCookiesTenderer()
            a.Encoding <- Encoding.GetEncoding("windows-1251")
            a
        while continueLooping do
            try 
                //let t ():string = (new TimedWebClient()).DownloadString(url: Uri)
                let task = Task.Run(fun () -> (getWebClient()).DownloadString(url : string))
                if task.Wait(TimeSpan.FromSeconds(650.)) then 
                    s <- task.Result
                    continueLooping <- false
                else raise <| new TimeoutException()
            with _ -> 
                if !count >= 100 then 
                    Logging.Log.logger (sprintf "Не удалось скачать %s за %d попыток" url !count)
                    continueLooping <- false
                else incr count
                Thread.Sleep(5000)
        s
