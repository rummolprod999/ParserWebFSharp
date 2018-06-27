namespace ParserWeb
open System

type Arguments = 
    | IrkutskOil
    | Akd
    | Lsr
    | Butb
    | RosSel
    | Neft

type RosSelRec = {Href: string; PurNum: string}
type NeftRec = {Href: string; PurNum: string; PurName: string; OrgName: string; DatePub: DateTime; DateEnd: DateTime}