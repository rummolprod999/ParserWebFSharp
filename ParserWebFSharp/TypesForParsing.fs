namespace ParserWeb

open System

type Arguments =
    | IrkutskOil
    | Akd
    | Lsr
    | Butb
    | RosSel
    | Neft
    | Slav
    | Aero
    | StroyTorgi
    | Asgor

type SlavNeft =
    | MEGION
    | YANOS
    | NGRE

type RosSelRec =
    { Href : string
      PurNum : string }

type NeftRec =
    { Href : string
      PurNum : string
      PurName : string
      OrgName : string
      DatePub : DateTime
      DateEnd : DateTime }

type SlavNeftRec =
    { HrefDoc : string
      HrefName : string
      PurNum : string
      PurName : string
      OrgName : string
      CusName : string
      DatePub : DateTime
      DateEnd : DateTime
      status : string
      typeT : SlavNeft }

type StroyTorgiRec =
    { Url : string
      PurNum : string
      PurName : string
      OrgName : string
      Status : string
      Price : string
      Currency : string }

type AeroRec =
    { Href : string
      PurNum : string
      PurName : string
      PwayName : string
      DatePub : DateTime
      DateEnd : DateTime
      status : string }

type AsgorRec =
    { Href : string
      PurNum : string
      PurName : string
      OrgName : string
      CusName : string
      DatePub : DateTime
      DateEnd : DateTime
      status : string
      PwayName : string
      Nmck : string
      NameLots : string Set}
