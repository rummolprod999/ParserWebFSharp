namespace ParserWeb

open System
open System.Collections.Generic

type TZ = | PST | PDT

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
    | GosYakut
    | RosTend
    | ChPt
    | Tplus
    | SibServ
    | TGuru
    | BidMart
    | Comita
    | EshopRzd
    | YarRegion
    | Btg
    | Vend
    | Pik
    | NorNic
    | Tenderer
    | Samolet
    | Ariba
    | Beeline
    | Tsm

type SlavNeft =
    | MEGION
    | YANOS
    | NGRE

type Exist =
    | Exist
    | NoExist

type RosSelRec =
    { Href : string
      PurNum : string
      PurName : string }

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
      NameLots : string Set }

type GosYakutRec =
    { Href : string
      PurNum : string
      PurName : string
      CusName : string
      CusUrl : string
      DatePub : DateTime
      DateEnd : DateTime
      PwayName : string
      Nmck : string }

type RosTendRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      Region : string
      Nmck : string
      DelivPlace : string
      Currency : string
      Page : string }

type ChPtRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      Nmck : string
      Currency : string }

type TPlusRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      Nmck : string
      Status : string
      region : string
      Page : string
      Exist : Exist }

[<Struct>]
type DocSibServ =
    { name : string
      url : string }

type SibServRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      DocList : List<DocSibServ> }

type TGuruRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      Nmck : string
      OrgName : string
      RegionName : string }

type BidMartRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      Nmck : string
      Quant : string }

type ComitaRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      Nmck : string
      Currency : string
      Status : string }

type EshopRzdRec =
    { Href : string
      PurNum : string
      PurName : string
      CusName : string
      DatePub : DateTime
      DateEnd : DateTime
      Status : string
      Nmck : string
      Currency : string
      RegionName : string }

type YarRegionRec =
    { Href : string
      PurNum : string
      PurName : string
      CusName : string
      DatePub : DateTime
      mutable DateEnd : DateTime
      Status : string
      Nmck : string }

type BtgRec =
    { Href : string
      PurNum : string
      PurName : string
      CusName : string
      DatePub : DateTime
      DateEnd : DateTime
      PwName : string
      OrgName : string }

type VendRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      PwName : string
      OrgInn : string }

type PikRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      Docs : List<string>
      Person : string
      OrgName : string }

type NorNicRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      CusName : string
      CusAddress : string
      PwName : string
      PersonEmail : string
      PersonTel : string
      OrgName : string }

type TendererRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      DateBidding : DateTime
      Nmck : string
      Currency : string
      OrgName : string
      RegionName : string }

type SamoletRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      DelivPlace : string }

type AribaRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime
      DateEnd : DateTime
      Nmck : string
      OrgName : string
      PwName : string
      Categories : string list
      DelivPlace : string }

type BeelineRec =
    { Href : string
      PurNum : string
      PurName : string
      DatePub : DateTime }
