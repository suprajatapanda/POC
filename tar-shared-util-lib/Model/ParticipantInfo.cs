using System.Xml.Serialization;

namespace TRS.IT.SI.BusinessFacadeLayer.Model
{
    #region "*** IParticipantAdapter ***"
    public interface IParticipantAdapter
    {
        ParticipantWithdrawalsInfo GetDistributionInfo(string sessionID, string contractId, string subId, string ssn);
        SIResponse UpdatePersonalProfile(string sessionID, PersonalProfile profile);
        ParticipantInfo GetParticipantInfo(string sessionID);
        ParticipantInfo GetParticipantInfo(int InLoginID, string ContractID, string SubID);
        PersonalProfile GetPersonalProfile(string sessionID);
        PersonalProfile GetPersonalProfile(int InLoginID, string ContractID, string SubID);
        SIResponse RequestConfirmationLetter(string sessionID, ConfirmationLetterInfo oConfirmationInfo);
        SIResponse RequestConfirmationLetter(int InLoginID, string ContractID, string SubID, ConfirmationLetterInfo oConfirmationInfo);
    }
    #endregion

    #region "***Enums***"
    public enum E_ContribType
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        Traditional401k = 1,
        [XmlEnum("2")]
        Roth401k = 2
    }
    public enum RestrictionTypeEnum
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        TransferRestrictions_Warning = 1,
        [XmlEnum("2")]
        TransferRestrictions_NotAllowed = 2,
        [XmlEnum("3")]
        TransferRestrictions_Frozen = 3
    }
    public enum ElectionsChangeTypeEnum
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        DuringOEProcess = 1,
        [XmlEnum("2")]
        DefaultFund = 2,
        [XmlEnum("3")]
        SelectedElections = 3
    }
    public enum Audience_Type_Enum
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        Employer = 1,
        [XmlEnum("2")]
        Employee = 2,
        [XmlEnum("3")]
        Producer = 3,
        [XmlEnum("4")]
        TPA = 4
    }
    public enum TransferMethod
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        Percent = 1,
        [XmlEnum("2")]
        Dollar = 2
    }
    public enum TransferFrequency
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        OneTime = 1,
        [XmlEnum("2")]
        Monthly = 2,
        [XmlEnum("3")]
        Quarterly = 3,
        [XmlEnum("4")]
        SemiAnnually = 4,
        [XmlEnum("5")]
        Annually = 5
    }
    public enum CatchUpPayType
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        PayPeriodDollar = 1,
        [XmlEnum("2")]
        PayPeriodPercent = 2,
        [XmlEnum("3")]
        OnetimeDollar = 3,
        [XmlEnum("4")]
        NoContribution = 4,
        [XmlEnum("5")]
        NotNow = 5
    }
    public enum LoanType
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        General = 1,
        [XmlEnum("2")]
        Home = 2,
        [XmlEnum("3")]
        Hardship = 3
    }
    public enum PlanLoanType
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        Paperless = 1,
        [XmlEnum("2")]
        PaperlessResidential = 2,
        [XmlEnum("3")]
        NonPaperless = 3,
        [XmlEnum("4")]
        ConvertedLoan = 4
    }
    public enum LetterTypeEnum
    {
        [XmlEnum("1")]
        WebsiteRegistraion = 1,
        [XmlEnum("2")]
        PasswordChange = 2,
        [XmlEnum("3")]
        TempPasswordNotice = 3,
        [XmlEnum("4")]
        UsernameChange = 4,
        [XmlEnum("5")]
        PasswordAssociation = 5,
        [XmlEnum("6")]
        AddressChange = 6,
        [XmlEnum("7")]
        PINChange = 7,
        [XmlEnum("8")]
        PINAssociation = 8,
        [XmlEnum("10")]
        NoticeOfUndeliverableEmail = 10
    }
    public enum LoanStatus
    {
        [XmlEnum("0")]
        Deleted = 0,
        [XmlEnum("1")]
        Active = 1,
        [XmlEnum("2")]
        Closed = 2,
        [XmlEnum("3")]
        Renegotiated = 3,
        [XmlEnum("4")]
        Defaulted = 4,
        [XmlEnum("5")]
        Pending = 5,
        [XmlEnum("6")]
        Deemed = 6
    }
    public enum LoanPaymentFrequency
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        Quarterly = 1,
        [XmlEnum("2")]
        Monthly = 2,
        [XmlEnum("3")]
        TwiceAMonth = 3,
        [XmlEnum("4")]
        Every2Weeks = 4,
        [XmlEnum("5")]
        Weekly = 5
    }
    public enum DenialReasonsEnum
    {
        [XmlEnum("0")]
        ExceedsMaximumLoanAmountAvailable = 0,
        [XmlEnum("1")]
        ParticipantIneligibleForLoan = 1,
        [XmlEnum("2")]
        LoanDoesNotSatisfyHardshipGuidelines = 2,
        [XmlEnum("3")]
        ExceedsMaximumNumberOfLoansAvailable = 3,
        [XmlEnum("4")]
        InterestRateIsInvalid = 4,
        [XmlEnum("5")]
        LoanFeeIsInvalid = 5,
        [XmlEnum("99")]
        Other = 99
    }
    public enum ISCLoanStatus
    {
        [XmlEnum("0")]
        Approved = 0,
        [XmlEnum("2")]
        Pending = 2,
        [XmlEnum("9")]
        Denied = 9
    }
    public enum TransactionActivityCode
    {
        [XmlEnum("1")]
        Contribution = 1,

        [XmlEnum("2")]
        Loan = 2,

        [XmlEnum("3")]
        Transfer = 3,

        [XmlEnum("4")]
        Distribution = 4,

        [XmlEnum("5")]
        Expenses = 5,

        [XmlEnum("6")]
        Unknown = 6,

        [XmlEnum("7")]
        Dividend = 7,

        [XmlEnum("8")]
        TransferOut = 8,

        [XmlEnum("9")]
        LoanRepayment = 9,

        [XmlEnum("10")]
        LoanAndLoanRepaymentHistory = 10,

        [XmlEnum("11")]
        NegativeUnitValueAdj = 11,

        [XmlEnum("12")]
        PositiveUnitValueAdj = 12,

        [XmlEnum("13")]
        RedemptionFee = 13,

        [XmlEnum("14")]
        ContributionAdjustment = 14,

        [XmlEnum("15")]
        DividendReinvestment = 15,

        [XmlEnum("16")]
        PlanServiceFee = 16,

        [XmlEnum("17")]
        PlanServiceCredit = 17,

        [XmlEnum("18")]
        CapitalGains_ShortTerm = 18,

        [XmlEnum("19")]
        CapitalGains_LongTerm = 19,

        [XmlEnum("20")]
        FiduciaryServices = 20
    }
    public enum PayrollFrequency
    {
        [XmlEnum("0")]
        Unknown = 0,

        [XmlEnum("1")]
        None = 1,

        [XmlEnum("2")]
        Annually = 2,

        [XmlEnum("3")]
        Weekly = 3,

        [XmlEnum("4")]
        BiWeekly = 4,

        [XmlEnum("5")]
        SemiMonthly = 5,

        [XmlEnum("6")]
        Monthly = 6,

        [XmlEnum("7")]
        Quarterly = 7,

        [XmlEnum("8")]
        SemiAnnually = 8
    }
    public enum FundType
    {
        [XmlEnum("0")]
        Standard = 0,
        [XmlEnum("1")]
        GICFund = 1,
        [XmlEnum("2")]
        PS58InsuranceFund = 2,
        [XmlEnum("3")]
        SavingsBond = 3,
        [XmlEnum("4")]
        EmployerStockNoPurchases = 4,
        [XmlEnum("5")]
        EmployerStockPurchases = 5,
        [XmlEnum("6")]
        MutualFund = 6,
        [XmlEnum("7")]
        OutsideInvestment = 7,
        [XmlEnum("8")]
        LoanFund = 8,
        [XmlEnum("9")]
        SDBAFund = 9
    }
    public enum GenderType
    {
        [XmlEnum("N")]
        NotOnFile = 0,
        [XmlEnum("F")]
        Female = 1,
        [XmlEnum("M")]
        Male = 2
    }
    public enum E_DisbursementType
    {
        [XmlEnum("1")]
        Check = 1,
        [XmlEnum("2")]
        ACH = 2
    }
    public enum E_AccountType
    {
        [XmlEnum("C")]
        Checking,
        [XmlEnum("S")]
        Savings,
        [XmlEnum("F")]
        Fund,
        [XmlEnum("B")]
        Rollover,
        [XmlEnum("I")]
        IRA
    }
    public enum EnrollmentStatus
    {
        [XmlEnum("I")]
        InEligible = 0,
        [XmlEnum("E")]
        Eligible = 1,
        [XmlEnum("C")]
        Complete = 2
    }
    public enum ProfileStatus
    {
        [XmlEnum("0")]
        Unknown = 0,
        [XmlEnum("1")]
        Normal = 1,
        [XmlEnum("2")]
        Registered = 2,
        [XmlEnum("3")]
        Registration_Reqd = 3,
        [XmlEnum("4")]
        New_Plan_Added = 4,
        [XmlEnum("5")]
        Pin_Change_Reqd = 5,
        [XmlEnum("6")]
        Registration_Reqd_MultiPlan = 6
    }

    #endregion

    #region "*** Classes ***"
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class Profile
    {
        public Profile()
        {
            ErrorInfo[] errInfo = [new ErrorInfo()];
            Errors = errInfo;
        }

        //<remarks/>
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified), XmlArrayItem("Error", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]

        public ErrorInfo[] Errors;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Title;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FirstName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string MiddleInitial;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string Suffix;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AddressLine1;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AddressLine2;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AddressLine3;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string City;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string State;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ZipCode1;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ZipCode2;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Country;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Telephone;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string MemberID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Password;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string NewPassword;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PasswordHelpQuestion;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PasswordHelpAnswer;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SSN;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Email;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string BirthDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EmploymentDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TerminationDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string EmployerName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string WorkTelephone;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string WorkTelephoneExt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PersonalNumber;
        //ADDITIONAL DATA REQUIRED FOR THE UPDATE TRANSACTION FROM SPONSOR SITE
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string VestingDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool HCECurrentYear;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool HCEPriorYear;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string Status;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LocationCode;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double AnnualSalary;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double CurrentSalary;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double PriorYearSalary;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double Pre87NonTax;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double TotalTax;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string[] SortOptions;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string EntryDt;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int VestingCounter;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //M-Male, F-Female
        public GenderType Gender = GenderType.NotOnFile;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public PayrollFrequency PayrollFrequency = PayrollFrequency.Unknown;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool NRACode;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool SignatureCard;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool EligibilityCode;

        //ADDITIONAL DATA REQUIRED FOR Distribution Transaction FROM SPONSOR SITE
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastContributionDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double ContributionAmount_Elective;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double ContributionAmount_Matching;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double ContributionAmount_Other;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double VestingPercent;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PlanYearEnd;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool USAddress = true;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public ProfileStatus ProfileStatus;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string GroupCode;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string RehireDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int YearsofService;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int HoursWorkedYTD;

        //<remarks/> ' Possible values : "",  "Y" , "N"
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string eStmtPreference;

        //<remarks/> ' Possible values : "",  "Y" , "N"
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string eConfirmPreference;

        //<remarks/> ' Possible values : "",  "Y" , "N"
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ReqdNoticesPreference;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SelfRegistrationDate;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int WebInLoginID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string WebUserID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Boolean UpdateHoursWorked;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Boolean UpdateYOS;

    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class PersonalProfile : Profile
    {

        public PersonalProfile()
        {
            ErrorInfo[] errInfo = [new ErrorInfo()];
            Errors = errInfo;
        }

    }

    public class StatementInfo
    {

        public enum StatementTypeEnum
        {
            [XmlEnum("1")]
            Date_Range = 1,
            [XmlEnum("2")]
            Last_Copy = 2,
            [XmlEnum("3")]
            NewStatement = 3
        }

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string SessionID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //report file name
        public string Identifier;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool IsCurrentPeriod;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string FromDate;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string ToDate;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool IsOnline;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        // unique report number required for each request
        public string UniqueStamp;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //Type of Statement
        public StatementTypeEnum Statement_Type;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int FundID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int ActivityCode;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int MoneyType;
    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schSIResponse")]
    public class SIResponse
    {
        public SIResponse()
        {
            ErrorInfo[] errInfo = [new ErrorInfo()];
            Errors = errInfo;
        }
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SessionID;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ConfirmationNumber;

        //<remarks/>
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified), XmlArrayItem("Error", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public ErrorInfo[] Errors;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool IsPending;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //transaction ids associtated with each confirmation
        public int[] TransIDs;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AdditionalData;
    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class ErrorInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string Description;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int Number;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Type;
    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class AddressInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AddressLine1;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AddressLine2;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AddressLine3;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string City;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string State;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ZipCode1;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ZipCode2;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Country;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool USAddress = true;

    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class PortfolioData
    {

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Boolean PXFlag;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RetirementYear;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RiskFactor;

    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class ParticipantInfo : SIResponse
    {
        public ParticipantInfo()
        {
            ErrorInfo[] errInfo = [new ErrorInfo()];
            Errors = errInfo;
        }

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool IsCacheValid = false;
        //<System.Xml.Serialization.XmlElementAttribute(Form:=System.Xml.Schema.XmlSchemaForm.Unqualified)> _
        //Public SessionID As String

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public PersonalProfile PersonalInfo;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string TransPending;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int TransPendingID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string TransPendingName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int PeriodicIndicator;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public TransferFrequency PeriodicFrequency;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double HardshipAmt;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double AccBal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double VstBal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double VstPct;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //outstanding loan balance
        public double LoanOutBal;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastProductionDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastContrDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double LastContrAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PlanEntryDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastTfrDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double LastTfrAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastInvChangeDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastInvChangeConfID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double PersonalReturnRate;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MinVstAccBal;
        //<remarks/>
        //<System.Xml.Serialization.XmlElementAttribute(Form:=System.Xml.Schema.XmlSchemaForm.Unqualified)> _
        //Public MinVstAccBalPct As Double

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int InvNewCounter;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int InvNewIncrement;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int InvNewNumber;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string InvNewPeriod;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public PayrollFrequency PayrollFrequency;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public CatchupInfo CatchupInfo;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public PlanInfo PlanInfo;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public PlanLoanInfo PlanLoanInfo;
        //<remarks/>
        [XmlElement("VestingInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public VestingInfo[] VestingInfo;
        //<remarks/>
        [XmlElement("LoanInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public LoanInfo[] LoanInfo;
        //<remarks/>
        [XmlElement("AccountInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public AccountInfo[] AccountInfo;
        //<remarks/>
        [XmlElement("DeferralInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public DeferralInfo DeferralInfo;
        //<remarks/>
        [XmlElement("FundInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public FundInfo[] FundInfo;
        //<remarks/>
        [XmlElement("SDBAFundInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public SDBAFundInfo SDBAFundInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SourceInfo[] SourceInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SourceGroupInfo[] SourceGroupInfo;

        //<remarks/>
        [XmlElement("TransactionHistoryInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public TransactionHistoryInfo[] TransactionHistoryInfo;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool ShowTransHistory;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double Pre87NonTax;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double Post86NonTax;
        //'<remarks/>
        //<System.Xml.Serialization.XmlArrayAttribute(Form:=System.Xml.Schema.XmlSchemaForm.Unqualified), _
        // System.Xml.Serialization.XmlArrayItemAttribute("Error", Form:=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable:=False)> _
        //Public Errors() As ErrorInfo

        //<remarks/>
        [XmlAttribute()]

        public string Action;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string ExcludedMenuList;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string ExcludedPageList;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public EnrollmentInfo EnrollmentInfo;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastStatementGeneratedDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastStatementMailedDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //oldest by sell date for the participant. 10/19/2004 - Added
        public string OldestBuySellDt;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LastQtrRebalanceDate;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string NextSchQtrTxnDate;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double InserviceAmt;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double InServiceAllowedPreTax;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MaxAmt59Withdrawal;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MaxAmt62Withdrawal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string ExecutionDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AfterTaxYrOfFirstContrib;
        //<remarks/>

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double AfterTaxCurrentBal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double AfterTaxBasis;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public ParticipantVestingInfo ParticipantVestingInfo;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LastVestingPercentUpdatedDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double TotalEmployeeRothBalance;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double TotalRolloverRothBalance;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double AfterTaxHardshipAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double AfterTaxInserviceAmt;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double InServiceAllowedRoth;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double AfterTaxInservice59Amt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double AfterTaxInservice62Amt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MaxAmtAvailTraditional;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int PendingTransactionCount;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int PendingDistributionCount;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PortfolioData PortfolioData;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public E_DefaultFundType DefaultFundType;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool CMPFlag;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool TMAFlag;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool ElectionsBySource;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public RequestorInfo RequestorInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool ES_Active;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool Plan_ES_Active;

        //TTWDCMM-2830
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double QbadAmountAvailable;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public QbadDistributionFees QbadDistributionFees;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public PriorQbadWithdrawals[] PriorQbadWithdrawals;
    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class RequestorInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string WebInLoginID;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string WebUserName;

    }
    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class EnrollmentInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public EnrollmentStatus EnrollmentStatus;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //indicates whether pin change is required or not
        public bool PinChangeReq;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //indicates whether investment elections change is needed or not
        public bool InvElectChangeReq;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //indicates whether contribution change needed or not
        public bool ContrChangeReq;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string EnrollmentDt;

    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class PlanInfo
    {

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string ContractID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string EligAge;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string EligMonths;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string EligEntryDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PlanID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PlanName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string SubID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool HardshipWithdrawals = true;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool Age59Withdrawals = true;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool OnlineEnrollment = true;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool AutoEnrollment = true;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool CatchupContributions = true;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string OleEndDate;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string ServiceType;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool REA_Exempt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public FundInfo DefaultFund;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double DefaultContrRate;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PlanType;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //plan type name for displaying on the web
        public string PlanTypeName;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool HardshipForElective = true;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int BVAFundID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double BVAMaxAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool SuppressVesting;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool AdviceSolution;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string TACode;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool OnlineDistribution;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool SuppressHardshipAvail;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool Contribution_PCT_Allowed;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool Contribution_AMT_Allowed;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool Term_Dist_Fee_Allowed;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public Audience_Type_Enum Term_Dist_Fee_PaidBy;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double Term_Dist_Fee;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool Contributions_Allowed;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool ROTH_Allowed;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PlanStatus;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public ElectionsChangeTypeEnum ElectionsChangeType;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool SDBAPlan;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DistributionRestriction;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Boolean OnlineForms;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int LoginStatus;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ParticipantWithdrawalsInfo WithdrawalsInfo;
        public PlanInfo()
        {
        }
    }


    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class FundInfo
    {

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string AssetID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string AssetName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string FundID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string FundName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string FundSequenceNumber;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double ExistingPct;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double ContrDirection;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool TransfersInAllowed;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool TransfersOutAllowed;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string TransPending;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //only used for parsing
        public string PartnerFundID;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public FundType FundType;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string P3FundType;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double TransferValue;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double FundBalance;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double Units;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double UnitPrice;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double VstVal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MinContribution;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MaxContribution;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool DisplayOnly;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public RestrictionTypeEnum RestrictionType;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string rolloverIdList;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string StartDate;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CloseDate;
    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class PlanLoanInfo
    {

        public PlanLoanInfo()
        {
            ErrorInfo[] errInfo = [new ErrorInfo()];
            Errors = errInfo;
        }

        //<remarks/>
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified), XmlArrayItem("Error", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]

        public ErrorInfo[] Errors;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int CurrentLoans;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double LoanIntRate;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MaxLoanAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MinLoanAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MinLoanPmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double LoanAmtAvail;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int MaxLoans;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int MaxGeneralLoanTerm;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int MaxResLoanTerm;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double LoanFee;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LoanFeePaidBy;
        //**********FIX FOR BUG: Munaf Kotawdekar (9/11/2004)
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double LoanSetupFee;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LoanSetupPaymt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double LoanMaintFee;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LoanMaintPaymt;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double TPALoanSetupFee;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string TPALoanSetupPaymt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double TPALoanMaintFee;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string TPALoanMaintPaymt;
        //************END FIX FOR BUG: Munaf Kotawdekar (9/11/2004)

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool PaperlessLoans;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool IsLoanAllowed;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int MaxLoansPerPeriod;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int FrequencyForMaxLoansPerPeriod;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //for purpose of applying loan period
        public string LoanPeriod;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool HardshipLoans;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //Number of loans taken this period
        public int LoanCounter;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool SuppressLoanAvail;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool RefinanceAllowed;
    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class DeferralInfo
    {
        public DeferralInfo()
        {
            ErrorInfo[] errInfo = [new ErrorInfo()];
            Errors = errInfo;
        }

        //<remarks/>
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified), XmlArrayItem("Error", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]

        public ErrorInfo[] Errors;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MinDefPct;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double MaxDefPct;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double CurDefPct;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double NewDefVal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string DefType;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //max def. changes allowed per period
        public int MaxDefChanges;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //actual number of deferral changes
        public int ActualDefChanges;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //period to apply the max deferral changes
        public string DefChangePeriod;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //To indicate this deferral change is in % or $
        public TransferMethod Method = TransferMethod.Unknown;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double DefValAT;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool AutoOptout;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public bool IsAutoIncrease;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public AutomaticDeferralInfo AutomaticDeferralInfo;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string ContributionRateLastChangeDate;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool NoConfLetter;
    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class AutoIncreasedInfo
    {

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public E_ContribType Type;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double minValue;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double maxValue;
    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class AutomaticDeferralInfo
    {

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public AutoIncreasedInfo[] AutoIncreasedInfo;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int AutomaticIncreaseEffectiveMonth;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int AutomaticIncreaseEffectiveYear;
    }
    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class AccountInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PartnerFundID;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PartnerAccountID;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FundID;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FundName;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AccID;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AccName;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VstPct;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VstVal;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Units;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double MarketVal;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public FundType FundType;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VestingInserviceAmt;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double InserviceAllowedVestedAmt;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VestingHardshipAmt;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VestingInservice62Amt;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VestingInservice59Amt;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VestingPriorPlanYearEndBalance;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SourceTypeC;

    }
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class DisbursementInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public E_DisbursementType DisbursementType;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AccountNumber;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RoutingNumber;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public E_AccountType AccountType;
    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class DenialReasonInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<DenialReasonsEnum> DenialReasonsEnum;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DenialReasonOther;
    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class LoanInfo
    {

        public ErrorInfo[] Errors;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LoanNumber;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LoanDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double CurBal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double LoanAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double RequestedLoanAmount;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double LoanIntRate;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double PmtAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string FirstPmtDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string MaturityDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double PayOffAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double YTDIntPaid;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double TotalIntPaid;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int RemPayments;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public LoanPaymentFrequency PaymentFrequency;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int LoanPeriodInMonths;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int NumberOfPayments;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public LoanType LoanType;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public LoanStatus Status;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PayoffQuoteGoodThru;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public PlanLoanType PlanLoanType;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string Notes1;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string Notes2;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string NewLoanFee;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int PartnerFlag;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public FeesPaidInfo TPALoanSetupFeeInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public FeesPaidInfo TPALoanMaintenanceFeeInfo;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public FeesPaidInfo TRSLoanSetupFeeInfo;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public FeesPaidInfo TRSLoanMaintenanceFeeInfo;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string TPAFirmName;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string LoanXMLContent;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LoanConfID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool TPAInvolved = false;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DistributionInfo.MailingOptionEnum MailingOption = DistributionInfo.MailingOptionEnum.None;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DisbursementInfo DisbursementInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public MailingAddressInfo MailingAddressInfo;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int TotalNumberOfPymt;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int TotalNumberOfPymtCalenderYear;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PartnerConfirmation;

        // -----------------Added for P3 JIRA # IT-64738---------------------------
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ContractID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SubID;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SSN;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TPAApproverName;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DenialReasonInfo DenialReason;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ISCLoanStatus ISCLoanStatus;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ProcessedDate;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LoanDocGenDate;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Division;

        // -------------End Added for P3  JIRA # IT-64738---------------------------
    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class TransactionHistoryInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string TxnDate;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int FundID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string FundName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double Amount;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double Shares;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public TransactionActivityCode ActivityCode;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string ActivityName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string FundSequenceNumber;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int MoneyTypeID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double UnitPrice;
    }
    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class ParticipantVestingInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public List<VestingPercentInfo> VestingInfo;
    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class VestingPercentInfo
    {

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string AccID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PartnerAccID;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string AccName;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VstPct;
    }

    //<remarks/>
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class VestingInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string VstYears;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double EmpContrPct;
    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class CatchupInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double CatchupContrPerPeriod;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double CatchupContrPerYear;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double CatchupContrMax;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double CatchupContrOneTime;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string OneTimeCatchupDate;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double NewCatchupVal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CatchUpPayType CatchupType = CatchUpPayType.PayPeriodDollar;
    }


    [Serializable(), XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class SDBAFundInfo
    {
        public SDBAFundInfo()
        {
            ErrorInfo[] errInfo = [new ErrorInfo()];
            Errors = errInfo;
        }

        //<remarks/>
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified), XmlArrayItem("Error", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]

        public ErrorInfo[] Errors;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string SDBAEnrollmentDt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public int SDBAFlag;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double SDBABal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double SDBACash;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string SDBAFundSequence;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double SDBAInitTransferAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double SDBASubTransferAmt;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double SDBAMinBal;
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double SDBAMaxPct;
    }

    [Serializable()]
    public class ConfirmationLetterInfo
    {

        [XmlArray("Letters"), XmlArrayItem(ElementName = "Letter")]

        public LetterInfo[] Letters;
        public class LetterInfo
        {
            [XmlElement()]

            public int LetterType;
            [XmlElement()]
            //Web Username - used when Website Registration, Username Change Confirmation letter request
            public string Username;

            [XmlElement()]
            //Confirmation Number - used when Password Change Confirmation, Username Change letter request
            public string ConfirmationNumber;

            [XmlElement()]
            //Temporary Password - used when Temp Password Confirmation letter request
            public string TemporaryPwd;

            [XmlElement()]
            //AddressLine1 - used when Address Change Confirmation letter request
            public string OldAddressLine1;

            [XmlElement()]
            //AddressLine2 - used when Address Change Confirmation letter request
            public string OldAddressLine2;

            [XmlElement()]
            //AddressLine3 - used when Address Change Confirmation letter request
            public string OldAddressLine3;

            [XmlElement()]
            //AddressLine4 - used when Address Change Confirmation letter request
            public string OldAddressLine4;

            [XmlElement()]
            //Daytime Phone number - used when Address Change Confirmation letter request
            public string DayPhoneNumber = "";

            [XmlElement()]
            //Evening Phone Number - used when Address Change Confirmation letter request
            public string EveningPhoneNumber = "";

            [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
            //For Back end process
            public string[] UndeliverableDocuments;

            [XmlArray("Contract")]

            public ContractInfo[] Contract;

            public class ContractInfo
            {
                [XmlElement()]

                public string ContractID;
                [XmlElement()]

                public string PlanName;
                [XmlElement()]
                public string CompanyName;
            }
        }

    }

    [Serializable()]
    public class FeesPaidInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public double FeeAmt;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]

        public string PaidBy;
    }
    #endregion

    [Serializable()]
    public class distributionFeesAmount
    {
        public double tpaFeeAmount;
        public double trsFeeAmount;
        public int tpaFeePaidByCode;
        public int trsFeePaidByCode;
    }
    [Serializable]
    public class hardshipReason
    {
        public int hardshipReasonCode;
        public string hardshipReasonValue;
    }
    public class taxWithholding
    {
        public double federalTaxWithholding;
        public bool fedTaxWithholdingOptOutAllowed;
        public string federalTaxWithholdingMessage;
        public string stateTaxLabel;
        public string stateTaxWithholdingMessage;
        public double stateTaxWithholding;
        public int stateTaxWithholdingType;
        public bool shouldWithholdStateTax;
        public bool isZeroStateTaxAllowed;
        public bool isNeverAllowStateTaxes;
        public bool withholdPercentOfFedTax;
    }
    public class ParticipantWithdrawalsInfo
    {
        public bool isTerminationDistributionAllowed;
        public bool isInServiceDistributionAllowed;
        public bool isHardshipDistributionAllowed;
        public bool isInServiceAllowedOnPlan;
        public bool isHardshipAllowedOnPlan;
        public bool isSpousalConsentRequired;
        public double spousalConsentRequiredAmt;
        public distributionFeesAmount hardshipDistributionFeesAmount;
        public distributionFeesAmount inServiceDistributionFeesAmount;
        public distributionFeesAmount terminationDistributionFeesAmount;
        public string yearOfFirstRothContribution;
        public string nonTaxableRoth;
        public string taxableRoth;
        public List<hardshipReason> hardshipReasons;
        public bool isOverNightAllowed;
        public double overNightFee;
        public taxWithholding hardshipTaxWithholding;
        public taxWithholding inServiceTaxWithholding;
        public taxWithholding terminationTaxWithholding;
        public List<string> items;
        public bool isMedallionSignatureRequired;
        public double priorWithdrawalsTotal;
        public double maxOnlineWithdrawalLimit;
        public string onlineWithdrawalLimitCode;
        public List<OutstandingLoan> outstandingLoans { get; set; }
    }

    public class OutstandingLoan
    {
        public string loanNo { get; set; }
        public double loanRepayAmt { get; set; }
        public double loanPayoffAmt { get; set; }
        public string loanOriginationDate { get; set; }
        public string quoteRequestedDate { get; set; }
        public string goodThroughDate { get; set; }
        public double loanPrincipal { get; set; }
        public double interestRate { get; set; }
    }
}