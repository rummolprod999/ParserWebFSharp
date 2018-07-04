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

type AeroRec =
    { Href : string
      PurNum : string
      PurName : string
      PwayName : string
      DatePub : DateTime
      DateEnd : DateTime
      status : string }
