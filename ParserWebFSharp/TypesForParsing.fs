namespace ParserWeb

open System
open System.Collections.Generic
open AngleSharp.Dom

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
    | Smart
    | RtsGen
    | Tj
    | Turk
    | Kg
    | Eten
    | CisLink
    | Petr
    | Mpkz
    | EstoreSpb
    | RosAgro
    | NeftReg
    | ForScience
    | VolgZmo
    | Rusal
    | Moek
    | Kamaz
    | Uni
    | Ksk
    | Gmt
    | Ymz
    | Unipro
    | Apps
    | RtsCorp
    | Sever
    | Medic
    | Bidzaar
    | Metodholding
    | Bhm
    | Domru
    | Samaragips
    | Goldenseed
    | Kaustik
    | Dme
    | Tele2
    | Osnova
    | Sibgenco
    | Vtbconnect
    | Rtci
    | Forumgd
    | Energybase
    | EtpRt
    | Comitazmo
    | Estp

type SlavNeft =
    | MEGION
    | YANOS
    | NGRE

type Exist =
    | Exist
    | NoExist

type RosSelRec =
    { Href: string
      PurNum: string
      PurName: string }

type NeftRec =
    { Href: string
      PurNum: string
      PurName: string
      OrgName: string
      DatePub: DateTime
      DateEnd: DateTime }

type SlavNeftRec =
    { HrefDoc: string
      HrefName: string
      PurNum: string
      PurName: string
      OrgName: string
      CusName: string
      DatePub: DateTime
      DateEnd: DateTime
      status: string
      typeT: SlavNeft }

type StroyTorgiRec =
    { Url: string
      PurNum: string
      PurName: string
      OrgName: string
      Status: string
      Price: string
      Currency: string }

type AeroRec =
    { Href: string
      PurNum: string
      PurName: string
      PwayName: string
      DatePub: DateTime
      DateEnd: DateTime
      status: string }

type AsgorRec =
    { Href: string
      PurNum: string
      PurName: string
      OrgName: string
      CusName: string
      DatePub: DateTime
      DateEnd: DateTime
      status: string
      PwayName: string
      Nmck: string
      NameLots: string Set }

type GosYakutRec =
    { Href: string
      PurNum: string
      PurName: string
      CusName: string
      CusUrl: string
      DatePub: DateTime
      DateEnd: DateTime
      PwayName: string
      Nmck: string }

type RosTendRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      Region: string
      Nmck: string
      DelivPlace: string
      Currency: string
      Page: string }

type RosTendRecNew =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      DateUpd: DateTime
      Region: string
      Nmck: string
      DelivPlace: string
      Currency: string
      Page: string }

type ChPtRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      Nmck: string
      Currency: string }

type TPlusRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      Nmck: string
      Status: string
      region: string
      Page: string
      Exist: Exist }

[<Struct>]
type DocSibServ =
    { name: string
      url: string }

type SibServRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      DocList: List<DocSibServ> }

type TGuruRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      Nmck: string
      OrgName: string
      RegionName: string }

type BidMartRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      Nmck: string
      Quant: string }

type ComitaRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      Nmck: string
      Currency: string
      Status: string }

type EshopRzdRec =
    { Href: string
      PurNum: string
      PurName: string
      CusName: string
      DatePub: DateTime
      DateEnd: DateTime
      Status: string
      Nmck: string
      Currency: string
      RegionName: string }

type YarRegionRec =
    { EmptyField: string }

type BtgRec =
    { Href: string
      PurNum: string
      PurName: string
      CusName: string
      DatePub: DateTime
      DateEnd: DateTime
      PwName: string
      OrgName: string }

type VendRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      PwName: string
      OrgInn: string }

type PikRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      Docs: List<string>
      Person: string
      OrgName: string }

type NorNicRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      CusName: string
      CusAddress: string
      PwName: string
      PersonEmail: string
      PersonTel: string
      OrgName: string }

type TendererRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      DateBidding: DateTime
      Nmck: string
      Currency: string
      OrgName: string
      RegionName: string }

type SamoletRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      DelivPlace: string }

type AribaRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      Nmck: string
      OrgName: string
      PwName: string
      Categories: string list
      DelivPlace: string }

type BeelineRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime }

type SmartRec =
    { Href: string
      PurNum: string
      PurName: string
      OrgName: string
      DatePub: DateTime
      DateEnd: DateTime
      status: string
      PwayName: string }
    
type SmartRecNew =
    { Id: int
      PurNum: string
      PurName: string
      Nmck: string
      OrgName: string
      OrgContactName: string
      OrgContactEmail: string
      OrgContactPhone: string
      DatePub: DateTime
      DateEnd: DateTime
      status: string
      PwayName: string }

type RtsGenRec =
    { Href: string
      PurNum: string
      PurName: string
      OrgName: string
      DatePub: DateTime
      DateEnd: DateTime
      Nmck: string
      RegionName: string
      status: string
      PwayName: string }

type TjRec =
    { status: string
      Href: string
      PurNum: string
      PurName: string
      OrgName: string
      DatePub: DateTime
      DateEnd: DateTime }

type KgRec =
    { PwName: string
      Href: string
      PurNum: string
      PurName: string
      OrgName: string
      Nmck: string
      DatePub: DateTime
      DateEnd: DateTime }

type EtenRec =
    { PwName: string
      Href: string
      PurNum: string
      PurName: string
      OrgName: string
      Status: string
      DatePub: DateTime}

type CisLinkRec =
    { Href: string
      DatePub: DateTime
      DateEnd: DateTime
      PurNum: string
      PurName: string
      OrgName: string}

type MpKzRec =
    { Href: string
      DateEnd: DateTime
      PurNum: string
      PurName: string
      OrgName: string
      Nmck: string}

type EstoreSpbRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string
      OrgName: string
      Status: string
      Nmck: string}
    
type RosAgroRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string
      Nmck: string
      PwName: string}

type NeftRegRec =
    { Href: string
      PurNum: string
      PurName: string
      Nmck: string
      PwName: string
      Status: string}

type ForScienceRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string
      OrgName: string
      Nmck: string
      PwName: string}

type VolgZmoRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string
      CusName: string
      Nmck: string
      Status: string}

type RusalRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string}

type MoekRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string
      OrgName: string
      CusName: string}

type KamazRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      DateScoring: DateTime
      PurNum: string
      PurName: string
      OrgName: string
      OrgPhone: string
      OrgEmail: string
      OrgPerson: string
      Period: string}

type UniRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string}

type KskRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string
      Status: string
      PwName: string
      Nmck: string
      DocList: List<IElement>}

type GmtRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string
      PwName: string}

type YmzRec =
    { Href: string
      DateEnd: DateTime
      DatePub: DateTime
      PurNum: string
      PurName: string}
    
type AppsRec =
    { Href: string
      PurName: string
      PurNum: string}

type RtsCorpRec =
    { Href: string
      PurNum: string
      PurName: string
      OrgName: string
      CusName: string
      Nmck: string
      RegionName: string
      status: string
      ContrGuarantee: string
      ApplGuarantee: string
      Currency: string }

type SeverStalRec =
    { Href: string
      PurName: string
      PurNum: string
      AddInfo: string
      DateEnd: DateTime
      DatePub: DateTime}
    
type BidMartNewRec =
    { Href: string
      PurName: string
      PurNum: string
      Status: string
      DelivPlace: string
      CusName: string
      Nmck: string
      Currency: string
      DateEnd: DateTime
      DatePub: DateTime}
    
type MedicRec =
    { Href: string
      PurName: string
      PurNum: string
      CusName: string
      CusPerson: string
      CusPhone: string
      DateEnd: DateTime
      DatePub: DateTime}

type BidzaarRec =
    { Href: string
      PurName: string
      PurNum: string
      CusName: string
      PwName: string
      DateEnd: DateTime
      DatePub: DateTime}

type MetodholdingRec =
    { Href: string
      PurName: string
      PurNum: string
      CusName: string
      DateEnd: DateTime
      DatePub: DateTime
      Attr: string}

type BhmProductRec =
    {
        Name: string
        Okei: string
        Quantity: string
    }
     
type BhmRec =
    { Href: string
      PurName: string
      PurNum: string
      PersonName: string
      PersonPhone: string
      PersonEmail: string
      DateEnd: DateTime
      DatePub: DateTime
      Products: List<BhmProductRec>
      DocList: List<DocSibServ>}
type DomRuRec =
    { Href: string
      PurName: string
      mutable PurNum: string
      DateEnd: DateTime
      DatePub: DateTime
      PwName: String
      Nmck: String}

type SamaraGipsRec =
    { Href: string
      PurName: string
      PurNum: string
      Status: String
      DatePub: DateTime}

type SamaraGoldenSeedRec =
    { Href: string
      PurName: string
      PurNum: string
      Status: String
      Type: String
      CusName: String
      DatePub: DateTime
      DateEnd: DateTime}

type KaustikRec =
    { Href: string
      PurName: string
      PurNum: string
      AttachName: String
      AttachUrl: String
      NoticeVer: String
      DatePub: DateTime
      DateEnd: DateTime}
    
type DmeRec =
    { Href: string
      PurName: string
      PurNum: string
      CusName: String
      PwName: String
      PersonName: String
      PersonPhone: String
      PersonEmail: String
      DatePub: DateTime
      DateEnd: DateTime}

type Tele2Rec =
    { Href: string
      PurName: string
      PurNum: string
      DelivPlace: String
      DelivTerm: String
      DatePub: DateTime
      DateEnd: DateTime}

type OsnovaRec =
    { Href: string
      PurName: string
      PurNum: string
      DatePub: DateTime
      DateEnd: DateTime}

type SibGencoRec =
    { Href: string
      PurName: string
      PurNum: string
      PwName: string
      Status: string
      DatePub: DateTime
      DateEnd: DateTime}

type VtbConnectRec =
    { Href: string
      PurName: string
      PurNum: string
      Nmck: string
      Currency: string
      OrgName: string
      Status: string
      mutable DatePub: DateTime
      mutable DateEnd: DateTime}
 
type RtCiRec =
    { Href: string
      PurName: string
      PurNum: string
      PwName: string
      DatePub: DateTime
      DateEnd: DateTime}

type ForumGdRec =
    { Href: string
      PurName: string
      PurNum: string
      PwName: string
      Status: string
      Period: string
      DelivPlace: string
      DatePub: DateTime
      DateEnd: DateTime}

type EnergyBaseRec =
    { Href: string
      PurName: string
      PurNum: string
      PwName: string
      CusName: string
      Nmck: string
      Currency: string
      DatePub: DateTime
      DateScoring: DateTime
      DateEnd: DateTime}
    
type EtpRtRec =
    { Href: string
      PurNum: string}

type NeftNewRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      Status: string
      OrgName: string
      ContactPerson: string
      DocList: List<DocSibServ> }

type EstpRec =
    { Href: string
      PurNum: string
      PurName: string
      DatePub: DateTime
      DateEnd: DateTime
      Region: string
      OrgName: string
      Status: string
      Price: string
      Currency: string
      PlacingWay: string}
