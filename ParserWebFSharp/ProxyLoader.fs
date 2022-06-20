namespace ParserWeb

open System.IO
open System.Collections.Generic
open System

type ProxyLoader() =

    static member val Loader =
        let reader =
            File.ReadLines(Settings.ProxyPath)

        let proxyList = List<ProxyB>()

        for p in reader do
            let arr = p.Split(":")

            proxyList.Add(
                { Ip = arr.[0]
                  Port = Int32.Parse(arr.[1])
                  User = arr.[2]
                  Pass = arr.[3] }
            )

        proxyList with get, set

    static member GetRandomProxy =
        let rand = Random()

        let index =
            rand.Next(0, ProxyLoader.Loader.Count)

        ProxyLoader.Loader.[index]
