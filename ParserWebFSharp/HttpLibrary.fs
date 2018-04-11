namespace ParserWeb

open System
open System.Collections.Generic
open System.Data
open System.Globalization
open System.IO
open System.Linq
open System.Net
open System.Text
open System.Text.RegularExpressions
open System.Threading
open System.Threading.Tasks

module Download = 
    type TimedWebClient() = 
        inherit WebClient()
        override this.GetWebRequest(address : Uri) = 
            let wr = base.GetWebRequest(address) :?> HttpWebRequest
            wr.Timeout <- 600000
            wr.UserAgent <- "Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:55.0) Gecko/20100101 Firefox/55.0";
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