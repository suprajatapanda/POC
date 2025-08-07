using System.Xml.Linq;
using System.Xml.Serialization;

namespace TRS.IT.SI.BusinessFacadeLayer.Model
{
    [Serializable()]
    public class FundWizardInfo
    {
        public enum FwTaskTypeEnum
        {
            [XmlEnum("0")]
            Unknown = 0,
            [XmlEnum("100")]
            FundChangeRequested = 100,
            [XmlEnum("101")]
            SponsorPptLetters = 101,
            [XmlEnum("102")]
            SetInkitFlag = 102,
            [XmlEnum("103")]
            MappingSpreadSheet = 103,
            [XmlEnum("104")]
            UpdatePegasys = 104,
            [XmlEnum("105")]
            FundRider = 105,
            [XmlEnum("106")]
            FundChangeRequestedModified = 106,
            [XmlEnum("107")]
            EffectiveDateEntered = 107,
            [XmlEnum("108")]
            PegasysFundActivated = 108,
            [XmlEnum("109")]
            PegasysFundDeactivated = 109,
            [XmlEnum("110")]
            PegasysValidated = 110,
            [XmlEnum("111")]
            SendNoticeToPartner = 111,
            [XmlEnum("112")]
            UpdatePartnerSystem = 112,
            [XmlEnum("113")]
            DocsImaged = 113,
            [XmlEnum("114")]
            SponsorPptLetterSentToMC = 114,
            [XmlEnum("115")]
            FundRiderSentToMC = 115,
            [XmlEnum("116")]
            StopProcessing = 116,
            [XmlEnum("117")]
            RequestApproved = 117,
            [XmlEnum("118")]
            RequestDenied = 118,
            [XmlEnum("119")]
            RequestCanceled = 119,
            [XmlEnum("120")]
            AnnualPptNotice = 120,
            [XmlEnum("121")]
            WebsiteUpdate = 121,
            [XmlEnum("122")]
            ProcessComplete = 122,
            [XmlEnum("123")]
            FileMannualUpload = 123,
            [XmlEnum("124")]
            AnnualPptNoticeSentToMC = 124,
            [XmlEnum("125")]
            AnnualPptNoticeSentToCCI = 125,
            [XmlEnum("126")]
            PXCustomSelectionUpdate = 126, //'used when we SetCustomPxFunds via eDocs 
            [XmlEnum("130")]
            PXAddendum = 130,
            [XmlEnum("131")]
            PXCustomChangeRequest = 131, //'used when change Custom Px Funds request via web 
            [XmlEnum("132")]
            PXChangeAuthorization = 132,
            [XmlEnum("133")]
            PXAddendumApproved = 133,
            [XmlEnum("134")]
            PXCustomChangeRequestApproved = 134,
            [XmlEnum("135")]
            PXChangeAuthorizationApproved = 135,
            [XmlEnum("136")]
            PXSetPortfolioXpress = 136, // used when we update Fiduciary type
            [XmlEnum("137")]
            SendCustomPXInvestmentMix = 137, //used when we provide the investment mix percentages from EM PX engine to P3
            [XmlEnum("138")]
            MAPXChangeRequested = 138, //Add MA + Remove PX
            [XmlEnum("139")]
            MAChangeRequested = 139, //Add MA
            [XmlEnum("140")]
            PXRemovalChangeRequested = 140,
            [XmlEnum("141")]
            PXRemovalChangeRequestApproved = 141,
            [XmlEnum("142")]
            PXRemovalSponsorLetter = 142,
            [XmlEnum("143")]
            PXRemovalParticipantLetter = 143,
            [XmlEnum("144")]
            PXRemovalSponsorLetterSentToMC = 144,
            [XmlEnum("145")]
            PXRemovalPptLetterSentToMC = 145,
            [XmlEnum("146")]
            PXRemovalSponsorPptLetterSentToMC = 146,
            [XmlEnum("146")]
            MAPXChangeAuthorization = 146,
            [XmlEnum("147")]
            PXRemovalSponsorPPTLetter = 147,
            [XmlEnum("148")]
            PXRemovalSetPegasysDate = 148,
            [XmlEnum("149")]
            MASetPegasysDate = 149,
            [XmlEnum("150")]
            MASetManagedAdvice = 150,
            [XmlEnum("151")]
            MAChangeApproved = 151,
            [XmlEnum("152")]
            MAChangeDeny = 152,
            [XmlEnum("154")]
            MARequestToAdd = 154,
            [XmlEnum("155")]
            MAParticipantNotice = 155,
            [XmlEnum("156")]
            MAAddendum = 156,
            [XmlEnum("157")]
            MATRASponsorAgreement = 157,
            [XmlEnum("158")]
            MAParticipantAgreement = 158,
            [XmlEnum("159")]
            MATRAFormADV = 159,
            [XmlEnum("160")]
            MASponsorConfirmation = 160,
            [XmlEnum("161")]
            MAChangeAuthorization = 161
        }
        public enum fwCaseStatusEnum
        {
            [XmlEnum("0")]
            CaseNew = 0,
            [XmlEnum("1")]
            Pending = 1,
            [XmlEnum("2")]
            PendingSignature = 2,
            [XmlEnum("3")]
            Approved = 3,
            [XmlEnum("4")]
            Denied = 4,
            [XmlEnum("20")]
            CancelByUser = 20,
            [XmlEnum("30")]
            Expired = 30,
            [XmlEnum("100")]
            FundChangeCompleted = 100,
            [XmlEnum("10")]
            MA_CaseNew = 10,
            [XmlEnum("11")]
            MA_Pending = 11,
            [XmlEnum("12")]
            MA_PendingSignature = 12,
            [XmlEnum("13")]
            MA_Approved = 13,
            [XmlEnum("14")]
            MA_Denied = 14,
            [XmlEnum("120")]
            MA_CancelByUser = 120,
            [XmlEnum("130")]
            MA_Expired = 130,
            [XmlEnum("200")]
            MACompleted = 200,
            [XmlEnum("40")]
            RemovePX_CaseNew = 40,
            [XmlEnum("41")]
            RemovePX_Pending = 41,
            [XmlEnum("42")]
            RemovePX_PendingSignature = 42,
            [XmlEnum("43")]
            RemovePX_Approved = 43,
            [XmlEnum("44")]
            RemovePX_Denied = 44,
            [XmlEnum("420")]
            RemovePX_CancelByUser = 420,
            [XmlEnum("430")]
            RemovePX_Expired = 430,
            [XmlEnum("400")]
            RemovePX_Completed = 400
        }
        public enum fwNotification
        {
            [XmlEnum("1970")]
            RequestForApproval = 1970,
            [XmlEnum("1990")]
            RequestForApprovalReminder = 1990,
            [XmlEnum("2000")]
            RequestForApprovalFinal = 2000,
            [XmlEnum("2030")]
            ApprovalDenial = 2030,
            [XmlEnum("2050")]
            AssignToReceiveNotification = 2050,
            [XmlEnum("2070")]
            DocumentDelivered = 2070,
            [XmlEnum("2090")]
            FundChangeCompleted = 2090,
            [XmlEnum("2110")]
            RequestExpired = 2110,
            [XmlEnum("2130")]
            AnnualNoticeError = 2130
        }
        public class FmrsUpdateReturn
        {
            public XElement xResult;
            public XElement xInputXml;
            public XElement xOutputXml;
        }
    }
    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class FWFundsInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FundID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PartnerFundID;

    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class AddFundsInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<FWFundsInfo> FundsInfo;
    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class DefaultFundInfo
    {

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DfltFundInfo FundsInfo;
    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class DfltFundInfo : FWFundsInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FundSeries;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Fiduciary;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string QDIA;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TransferDfltMny;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PptAgree;

    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class ManagedAdviceInstallInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string InDefaultStrategy;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string OutofDefaultStrategy;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PXDisablewithMA;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CMPDisablewithMA;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FreeLookDays;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AutoSubscribeMethod;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EmailAddress;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AdditionalEmailAddress;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FeeStartDate;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FeeTotalBasisPoints;

    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class Services
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DisablePX;
    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class ForfeitureFundInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public FWFundsInfo FundsInfo;
    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class TransferFundsInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<TransferFundInfo> FundsInfo;
    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class TransferFundInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FromFundID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ToFundID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FromPartnerFundID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ToPartnerFundID;
    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class ContractsFundsInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<AddDeleteFundsInfo> AddDeleteFundsInfo;

    }

    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class AddDeleteFundsInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ContractID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SubID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EffectiveDate;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Type;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ProjectManager;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AddFundsInfo AddFundsInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DefaultFundInfo DefaultFundInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ForfeitureFundInfo ForfeitureFundInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public TransferFundsInfo TransferFundsInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ManagedAdviceInstallInfo ManagedAdviceInstallInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Services Services;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public FeeConfigurations FeeInfo;

    }

    [XmlType(Namespace = "http://TRS.SOAModel/")]
    public class FeeConfigurations
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CACCommCalcTypeID { get; set; }
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TakeoverAssets { get; set; }
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AnnualFlow { get; set; }
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RolloverAmount { get; set; }
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Participants { get; set; }
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EligibleEmployees { get; set; }
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ProductID { get; set; }
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PAATypeID { get; set; }
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<Feature> Features { get; set; }
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<Band> LoiBand { get; set; }
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<CommCfg> CommCfgs { get; set; }
    }

    [XmlType(Namespace = "http://TRS.SOAModel/")]
    public class Feature
    {
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Id { get; set; }
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name { get; set; }
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DtlId { get; set; }
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Value { get; set; }
    }

    [XmlType(Namespace = "http://TRS.SOAModel/")]
    public class CommCfg
    {
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Id { get; set; }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name { get; set; }
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ComboId { get; set; }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PptRrId { get; set; }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PptRrName { get; set; }

        public List<CommType> CommTypes { get; set; }

    }

    [XmlType(Namespace = "http://TRS.SOAModel/")]
    public class CommType
    {
        public List<Band> CommScaleBands { get; set; }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Id { get; set; }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Name { get; set; }
    }

    [XmlType(Namespace = "http://TRS.SOAModel/")]
    public class Band
    {
        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Id { get; set; }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Start { get; set; }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string End { get; set; }

        [XmlAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Rate { get; set; }
    }
}