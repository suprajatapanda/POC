using System.Xml.Serialization;
namespace TRS.IT.SI.BusinessFacadeLayer.Model
{
    public class Enums
    {
        public enum E_ObjectType
        {
            [XmlEnum("0")]
            Unknown = 0,
            [XmlEnum("1")]
            ContractInfo = 1,
            [XmlEnum("2")]
            TPAInfo = 2,
            [XmlEnum("3")]
            ParticipantInfo = 3,
            [XmlEnum("4")]
            FMRS = 4,
            [XmlEnum("5")]
            WMS = 5,
            [XmlEnum("6")]
            PX = 6,
            [XmlEnum("7")]
            ACH = 7,
            [XmlEnum("8")]
            SponsorInfo = 8,
            [XmlEnum("9")]
            PartnerPlanFunds = 9,
            [XmlEnum("10")]
            PlanData = 10,
            [XmlEnum("11")]
            PlanInvoiceDetails = 11,
            [XmlEnum("12")]
            PlanHSAInfo = 12
        }
    }
    public enum E_AdminEmailType
    {
        ERORR = 0,
        SUCCESS = 1,
        SENT_ITEM = 3
    }
    public enum E_Status
    {
        PASS,
        FAIL,
        INFO
    }
    public enum E_Variable
    {
        ALLOCATION,
        ALLOCATION_DATE,
        CONFIRMATION_NUMBER,
        CURRENT_DATE,
        REMINDER,
        TOTAL_DEPOSIT_AMOUNT,
        DAYS,
        TPA_FIRM_NAME,
        TPA_TELEPHONE,
        TRANSACTION_DATE,
        RECEIPT_DATE,
        CONTRACT_NUMBER,
        SUB_ID,
        TRANSACTION_TYPE,
        EMAIL_DETAIL,
        LOAN_PAYMENT_AMOUNT,
        LOAN_PAYMENTS_COUNT,
        LOAN_PAYMENT_FREQUENCY,
        REASON_1,
        REASON_2,
        PARTICIPANT_NAME_MESSAGECENTER,
        PARTICIPANT_NAME,
        USER_NAME,
        LOAN_DOCUMENTS_TEXT,
        FREQUENCY,
        TPA_SUBMITTED_TEXT,
        PRIMARY_CONTACT,
        PASSWORD,
        PLAN_NAME_OR_COMPANY_NAME,
        EXPIRATION_DATE
    }
    public enum E_ReportColor
    {

        TASK_TRACKER = 0,
        NOTIFICATION_SERVICE = 1,
        ERROR = 2,
        INFORMATION = 3
    }
    public enum E_ImageOption
    {
        None = 0,
        NoImaging = 1,
        ImageMessageAndAttachments = 2,
        ImageMessageOnly = 3,
        ImageAttachmentsOnly = 4
    }
    public enum E_TableHeader
    {
        ItemNo,
        ContractID,
        SubID,
        DepositID,
        TransID,
        MessegeID,
        Notification,
        Status,
        Description,
        Error,
        TaskCode,
        SourceName
    }
    public enum E_MessageType
    {
        None = 0,
        Email = 1,
        Fax = 2,
        MessageCenter = 3
    }
    [Serializable()]
    public enum E_ADPACPtestingmethodType
    {
        [XmlEnum(Name = "1")]
        PRIORYEAR = 1,

        [XmlEnum(Name = "2")]
        CURRENTYEAR = 2

    }
    public enum E_DefaultFundType
    {
        [XmlEnum("0")]
        None = 0,
        [XmlEnum("1")]
        Regular = 1,
        [XmlEnum("2")]
        PortfolioXpress = 2,
        [XmlEnum("3")]
        Series = 3
    }
    public enum E_P3_SourceTypeC
    {
        [XmlEnum("102")]
        Roth = 102,

        [XmlEnum("116")]
        RothRollover = 116,

        [XmlEnum("117")]
        InPlanRothConversion = 117,

        [XmlEnum("122")]
        QDRORoth = 122
    }

    [XmlType(Namespace = "http://TRS.SOAModel/")]
    public enum E_ContractStatus : int
    {
        [XmlEnum(Name = "0")]
        INCOMPLETE = 0,

        [XmlEnum(Name = "1")]
        ACTIVE = 1,

        [XmlEnum(Name = "2")]
        DISCONTINUED_PENDING = 2,

        [XmlEnum(Name = "20")]
        DISCONTINUED_INSTALLMENTONLY = 20,

        [XmlEnum(Name = "21")]
        PLAN_TERMINATION_PENDING = 21,

        [XmlEnum(Name = "3")]
        FROZEN_MATURED = 3,

        [XmlEnum(Name = "30")]
        FROZEN_WITHDRAWALS = 30,

        [XmlEnum(Name = "35")]
        ABANDONED = 35,

        [XmlEnum(Name = "4")]
        PENDING = 4,

        [XmlEnum(Name = "40")]
        PENDING_SIGNATURE = 40,

        [XmlEnum(Name = "41")]
        PENDING_SUBMISSION = 41,

        [XmlEnum(Name = "9")]
        DISCONTINUED_FULLY_PAID = 9,

        [XmlEnum(Name = "91")]
        DISCONTINUED_PAYING_ANNUITIES = 91,

        [XmlEnum(Name = "97")]
        OMNIBUS = 97,

        [XmlEnum(Name = "99")]
        CONVERSION = 99,

        [XmlEnum(Name = "999")]
        PLAN_TERMINATION_FULLY_PAID = 999,

        [XmlEnum(Name = "9990")]
        SERVICE_TYPE_CHANGE = 9990,

        [XmlEnum(Name = "9991")]
        SERVICE_TYPE_CHANGE_COMPLETE = 9991,

        [XmlEnum(Name = "9992")]
        PENDING_PLAN_DATA = 9992,

        [XmlEnum(Name = "9994")]
        PAYCHEX_TERMINATED = 9994,

        [XmlEnum(Name = "9995")]
        PAYCHEX_REFUSED_BUSINESS = 9995,

        [XmlEnum(Name = "9996")]
        PAYCHEX_PENDING = 9996,

        [XmlEnum(Name = "9997")]
        PAYCHEX_DISCONTINUED = 9997,

        [XmlEnum(Name = "9998")]
        NSP = 9998,

        [XmlEnum(Name = "9999")]
        NTO = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999,

        [XmlEnum(Name = "100000")]
        UNKNOWN = 100000
    };
    public enum E_PhoneType : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "10")]
        PrimaryBusiness = 10,

        [XmlEnum(Name = "100")]
        Business = 100,

        [XmlEnum(Name = "200")]
        FAX = 200,

        [XmlEnum(Name = "300")]
        Cellular = 300,

        [XmlEnum(Name = "400")]
        Home = 400,

        [XmlEnum(Name = "20")]
        PrimaryFax = 20,

        [XmlEnum(Name = "9999")]
        Other = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    };
    public enum E_EntityTypes : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "10")]
        Corporation = 10,

        [XmlEnum(Name = "100")]
        Foundation = 100,

        [XmlEnum(Name = "110")]
        HealthWarfare = 110,

        [XmlEnum(Name = "120")]
        Union = 120,

        [XmlEnum(Name = "130")]
        FinancialPool = 130,

        [XmlEnum(Name = "140")]
        ProfessionalServiceOrganization = 140,

        [XmlEnum(Name = "20")]
        Partnership = 20,

        [XmlEnum(Name = "200")]
        S_Corporation = 200,

        [XmlEnum(Name = "30")]
        CharitableOrg = 30,

        [XmlEnum(Name = "300")]
        LLC = 300,

        [XmlEnum(Name = "310")]
        LLP = 310,

        [XmlEnum(Name = "320")]
        NongovernmentalTaxExemptOrganization = 320,

        [XmlEnum(Name = "330")]
        TaxExemptOrganization = 330,

        [XmlEnum(Name = "340")]
        Tribal = 340,

        [XmlEnum(Name = "40")]
        Governmental = 40,

        [XmlEnum(Name = "50")]
        ChurchOrganization = 50,

        [XmlEnum(Name = "51")]
        ChurchERISACovered = 51,

        [XmlEnum(Name = "53")]
        ChurchNotERISACovered = 53,

        [XmlEnum(Name = "60")]
        Educational = 60,

        [XmlEnum(Name = "70")]
        NonProfitOrganization = 70,

        [XmlEnum(Name = "80")]
        SoleProprietorship = 80,

        [XmlEnum(Name = "90")]
        Hospital = 90,

        [XmlEnum(Name = "9999")]
        OTHER = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    };
    public enum E_AddressType : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "1")]
        Home = 1,

        [XmlEnum(Name = "2")]
        Office = 2,

        [XmlEnum(Name = "9999")]
        Other = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    };
    public enum E_Partners : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "200")]
        TAE = 200,

        [XmlEnum(Name = "1200")]
        DIA = 1200,

        [XmlEnum(Name = "800")]
        PENCO = 800,

        [XmlEnum(Name = "2000")]
        PAYCHEX = 2000,

        [XmlEnum(Name = "4000")]
        REHOR = 4000,

        [XmlEnum(Name = "4020")]
        CAPEN = 4020,

        [XmlEnum(Name = "4010")]
        FUBEN = 4010,

        [XmlEnum(Name = "400")]
        SEBS = 400,

        [XmlEnum(Name = "1300")]
        ISC = 1300,

        [XmlEnum(Name = "-1")]
        FUNDS = -1,

        [XmlEnum(Name = "9999")]
        TRS = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    };
    public enum E_ContractType : int
    {
        None = 0,
        New = 1,
        Takeover = 2,
        Other = 9999,
        NOTSET = 99999
    };
    public enum E_ContractInstallationStatus : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "40")]
        Under_Installation = 40,

        [XmlEnum(Name = "160")]
        Complete = 160,

        [XmlEnum(Name = "9999")]
        Other = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    };
    public enum E_ER_MC_SH_PLUSType : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "1")]
        PercentEC = 1,

        [XmlEnum(Name = "2")]
        MinCompensation = 2,

        [XmlEnum(Name = "3")]
        MaxCompensation = 3,

        [XmlEnum(Name = "9999")]
        Other = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    }
    public enum E_TPACompanyContactType : int
    {

        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "36")]
        TPAProducers = 36,

        [XmlEnum(Name = "37")]
        TPACompany = 37,

        [XmlEnum(Name = "54")]
        TPAOwner = 54,

        [XmlEnum(Name = "55")]
        TPAPrimaryContact = 55,

        [XmlEnum(Name = "56")]
        TPASrPlanAdministrator = 56,

        [XmlEnum(Name = "57")]
        TPAJrPlanAdministrator = 57,

        [XmlEnum(Name = "58")]
        TPAAdministrativeAssistant = 58,

        [XmlEnum(Name = "59")]
        TPACompanyAdminAssistant = 59,

        [XmlEnum(Name = "60")]
        TPACompanyJrPlanAdmin = 60,

        [XmlEnum(Name = "61")]
        TPACompanyOwner = 61,

        [XmlEnum(Name = "62")]
        TPACompanySrPlanAdmin = 62,

        [XmlEnum(Name = "63")]
        TPACompanyPrimaryContact = 63,

        [XmlEnum(Name = "150")]
        TPALoanContact = 150,

        [XmlEnum(Name = "152")]
        TPADistributionContact = 152,

        [XmlEnum(Name = "70")]
        TPAProposalContact = 70,

        [XmlEnum(Name = "72")]
        TPACommunicationContact = 72,

        [XmlEnum(Name = "102")]
        TPANewBusinessContact = 102,

        [XmlEnum(Name = "103")]
        TPAFirmSalesRep = 103,

        [XmlEnum(Name = "104")]
        TPAOperationManager = 104,

        [XmlEnum(Name = "105")]
        TPAOperationPersonnel = 105,

        [XmlEnum(Name = "106")]
        TPAMarketingPersonnel = 106,

        [XmlEnum(Name = "107")]
        TPAMarketingManager = 107,

        [XmlEnum(Name = "166")]
        TPAFeeContact = 166,

        [XmlEnum(Name = "134")]
        TPAAuthorizedSigner = 134,

        [XmlEnum(Name = "888")]
        TPAAccounting = 888,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    }
    public enum E_TPAContactType : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "800")]
        TPASrPlanAdministrator = 800,

        [XmlEnum(Name = "805")]
        TPACC_Communications = 805,

        [XmlEnum(Name = "9999")]
        Other = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    }
    public enum E_MatchType : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "1500")]
        SafeHarbor = 1500,

        [XmlEnum(Name = "1100")]
        Percentage = 1100,

        [XmlEnum(Name = "1300")]
        Elective = 1300,

        [XmlEnum(Name = "1200")]
        Tiered = 1200,

        [XmlEnum(Name = "1600")]
        Addendum = 1600,

        [XmlEnum(Name = "1400")]
        Catchup_contributions = 1400,

        [XmlEnum(Name = "9999")]
        Other = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    }
    public enum E_SHMatchType : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "10")]
        NonElective3Percent = 10,

        [XmlEnum(Name = "20")]
        Basic = 20,

        [XmlEnum(Name = "30")]
        Enhanced = 30,

        [XmlEnum(Name = "9999")]
        Other = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    }
    public enum E_FeePaidBy : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "10")]
        Employer = 10,

        [XmlEnum(Name = "20")]
        Participant = 20,

        [XmlEnum(Name = "30")]
        AlternatePayee = 30,

        [XmlEnum(Name = "9999")]
        Other = 9999,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    }
    public enum E_LoanFreqType : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "1")]
        PerMonth = 1,

        [XmlEnum(Name = "2")]
        Every3Months = 2,

        [XmlEnum(Name = "3")]
        PerCalendarYear = 3,

        [XmlEnum(Name = "4")]
        PerPlanYear = 4,

        [XmlEnum(Name = "9999")]
        Other = 9999,

        [XmlEnum(Name = "9998")]
        NA = 9998,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    }
    public enum E_ContactType : int
    {
        [XmlEnum(Name = "0")]
        Unknown = 0,

        [XmlEnum(Name = "23")]
        AccountStatementRecipients = 23,

        [XmlEnum(Name = "28")]
        DepositConfirmees = 28,

        [XmlEnum(Name = "29")]
        DepositConfirmeesCopy = 29,

        [XmlEnum(Name = "100")]
        PrimaryContact = 100,

        [XmlEnum(Name = "105")]
        LocationContact = 105,

        [XmlEnum(Name = "110")]
        BondUnderwriter = 110,

        [XmlEnum(Name = "120")]
        Employer = 120,

        [XmlEnum(Name = "125")]
        ClientCompanyExecutiveContact = 125,

        [XmlEnum(Name = "130")]
        ClientCompanyPrimaryContact = 130,

        [XmlEnum(Name = "135")]
        ParticipatingEmployerTrustee = 135,

        [XmlEnum(Name = "140")]
        AgentforLegalProcess = 140,

        [XmlEnum(Name = "150")]
        Fiduciary = 150,

        [XmlEnum(Name = "151")]
        Delegated316 = 151,

        [XmlEnum(Name = "160")]
        Disclosure = 160,

        [XmlEnum(Name = "180")]
        Consultant = 180,

        [XmlEnum(Name = "20")]
        PlanAdministrator = 20,

        [XmlEnum(Name = "200")]
        Broker = 200,

        [XmlEnum(Name = "210")]
        InvestmentManager = 210,

        [XmlEnum(Name = "220")]
        PortfolioManager = 220,

        [XmlEnum(Name = "30")]
        EnrollmentContact = 30,

        [XmlEnum(Name = "300")]
        PriorPlanAdministrator = 300,

        [XmlEnum(Name = "310")]
        PriorPlanRecordkeeper = 310,

        [XmlEnum(Name = "320")]
        PriorInvestmentProvider = 320,

        [XmlEnum(Name = "40")]
        Trustee = 40,

        [XmlEnum(Name = "410")]
        CorporateTrustee = 410,

        [XmlEnum(Name = "420")]
        CustodianofStock = 420,

        [XmlEnum(Name = "430")]
        ExternalTrustee = 430,

        [XmlEnum(Name = "440")]
        AuthorizedSigners = 440,

        [XmlEnum(Name = "450")]
        FiscalAgent = 450,

        [XmlEnum(Name = "470")]
        Issuer = 470,

        [XmlEnum(Name = "550")]
        BankOneICA = 550,

        [XmlEnum(Name = "560")]
        TAEastLargePlan = 560,

        [XmlEnum(Name = "561")]
        TAEastSinglePoint = 561,

        [XmlEnum(Name = "60")]
        PersonnelPayrollMgr = 60,

        [XmlEnum(Name = "600")]
        ExecutiveContact = 600,

        [XmlEnum(Name = "700")]
        Recordkeeper = 700,

        [XmlEnum(Name = "710")]
        TrusteeofnonTRSFunds = 710,

        [XmlEnum(Name = "720")]
        PassiveTrustee = 720,

        [XmlEnum(Name = "75")]
        ChiefFinancialOfficer = 75,

        [XmlEnum(Name = "80")]
        Controller = 80,

        [XmlEnum(Name = "800")]
        TPAContact = 800,

        [XmlEnum(Name = "810")]
        PartnerContact = 810,

        [XmlEnum(Name = "820")]
        PayrollValidator = 820,

        [XmlEnum(Name = "830")]
        SponsorEnewsRcpt = 830,

        [XmlEnum(Name = "840")]
        Auditor = 840,

        [XmlEnum(Name = "850")]
        PlanSponsorsAttorney = 850,

        [XmlEnum(Name = "860")]
        PlanSponsorsAccountant = 860,

        [XmlEnum(Name = "870")]
        Delegated338 = 870,
        //[XmlEnum(Name = "2")]
        //PlanFiduciary = 2,

        //[XmlEnum(Name = "5")]
        //PayrollOnly = 5,

        //[XmlEnum(Name = "6")]
        //LegalServiceAgent = 6,
        [XmlEnum(Name = "999")]
        Others = 999,

        [XmlEnum(Name = "1360")]
        PassManager = 1360,

        [XmlEnum(Name = "1500")]
        ClientRelationshipAssociate = 1500,

        [XmlEnum(Name = "3060")]
        AccountExecutive = 3060,

        [XmlEnum(Name = "4000")]
        TPAConnect = 4000,

        [XmlEnum(Name = "8888")]
        InvoiceRecipient = 8888,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    }
    [XmlType(Namespace = "http://TRS.SOAModel/")]
    public enum E_MAMethodType : int
    {
        [XmlEnum(Name = "0")]
        NONE = 0,

        [XmlEnum(Name = "1")]
        FULL_RE_ENROLL = 1,

        [XmlEnum(Name = "2")]
        PARTICIPANTS = 2,

        [XmlEnum(Name = "3")]
        VOLUNTARY = 3,

        [XmlEnum(Name = "99999")]
        NOTSET = 99999
    };
    public enum E_FW_ChangeType : int
    {
        [XmlEnum(Name = "0")]
        None = 0,

        [XmlEnum(Name = "7")]
        REMOVE_PX = 7,

        [XmlEnum(Name = "8")]
        ADD_MA = 8

    }
}