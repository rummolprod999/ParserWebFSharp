namespace ParserWeb

open System
open System.Collections.Generic
open System.Linq
open System.Text.RegularExpressions
open System.Threading
open System.Threading.Tasks
open TypeE

type ParserRosTendXml(stn : Settings.T) =
    inherit Parser()
    
    override this.Parsing() = ()