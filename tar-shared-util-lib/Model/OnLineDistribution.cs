using System.Xml.Serialization;

namespace TRS.IT.SI.BusinessFacadeLayer.Model
{

    [Serializable()]
    public class DistributionInfo
    {
        public enum DistributionStatusEnum
        {
            Pending = 0,
            Approved = 1,
            //Implies Sponsor has approved
            Complete = 2,
            //Implies have been updated in Partner records
            Waiting_30Days = 3,
            Rejected = 4,
            //Implies Sponsor has rejected the application
            Cancelled = 5,
            //User has cancelled his Online Distribution Request
            SponsorApproved = 6,
            //This request has been approved by sponsor and is now waiting TPA approval before it can be sent to Partner
            WaitingWMSIAccount = 7,
            //Waiting for WMSI to generate ML Account number
            PendingTPAApproval = 8
            //This request is pending for TPA Approval
        }

        public enum MailingOptionEnum
        {
            [XmlEnum("-1")]
            None = -1,
            [XmlEnum("0")]
            Overnight,
            [XmlEnum("1")]
            Normal,
            [XmlEnum("2")]
            OvernightPartial
        }
        public enum DenialReasonsEnum
        {
            [XmlEnum("-1")]
            Not_Reqd = -1,
            [XmlEnum("0")]
            PPT_NOTMET_RQMT = 0,
            [XmlEnum("1")]
            ExceedsMaxAmount_Inservice = 1,
            [XmlEnum("2")]
            ExceedsMaxAmount_Hardship = 2,
            [XmlEnum("3")]
            IncorrectVesting = 3,
            [XmlEnum("4")]
            IncorrectHardshipGuidelines = 4,
            [XmlEnum("5")]
            SupportDocsNotReceived = 5,
            [XmlEnum("6")]
            ExpiredRequest = 6,
            [XmlEnum("99")]
            Others = 99
        }
        public enum LoanOptionEnum
        {
            [XmlEnum("-1")]
            Not_Reqd = -1,
            [XmlEnum("0")]
            REPAY_LOAN = 0,
            [XmlEnum("1")]
            TAXABLE_DISTRIBUTION = 1,
            [XmlEnum("2")]
            LOAN_ACTIVE = 2
        }

        public enum RolloverToEnum
        {
            [XmlEnum("-1")]
            Not_Reqd = -1,
            [XmlEnum("0")]
            NEW_TA_IRA = 0,
            [XmlEnum("1")]
            Direct_RO_To_IRA = 1,
            [XmlEnum("2")]
            Direct_RO_To_401A = 2,
            [XmlEnum("3")]
            Direct_RO_To_401k = 3,
            [XmlEnum("4")]
            Direct_RO_To_403b = 4,
            [XmlEnum("5")]
            Governmental_457 = 5,
            [XmlEnum("6")]
            Particpant = 6,
            [XmlEnum("7")]
            Existing_IRA = 7,
            [XmlEnum("8")]
            Roth_IRA = 8,
            [XmlEnum("9")]
            Roth_401k = 9,
            [XmlEnum("10")]
            NEW_ML_IRA = 10
        }

        public enum TerminationReasonEnum
        {
            [XmlEnum("R")]
            Registration,
            [XmlEnum("T")]
            Termination,
            [XmlEnum("E")]
            EarlyRetirement,
            [XmlEnum("N")]
            NormalRetirement,
            [XmlEnum("L")]
            LateRetirement,
            [XmlEnum("P")]
            PermanentDisablity,
            [XmlEnum("D")]
            Death,
            [XmlEnum("C")]
            Discharge
        }

        public enum TerminationPaymentCycleEnum
        {
            [XmlEnum("M")]
            Monthly,
            [XmlEnum("Q")]
            Quarterly,
            [XmlEnum("T")]
            TriAnnually,
            [XmlEnum("S")]
            SemiAnnually,
            [XmlEnum("A")]
            Annually
        }
        public enum HardshipReasonsEnum
        {
            [XmlEnum("0")]
            Unknown = 0,
            [XmlEnum("1")]
            PurchaseOfPrincipalResidence = 1,
            [XmlEnum("2")]
            CollegePostsecondaryTuition = 2,
            [XmlEnum("3")]
            MedicalExpenses = 3,
            [XmlEnum("4")]
            ForeclosureEvictionPrincipalResidence = 4,
            [XmlEnum("5")]
            DamageToPrincipalResidence = 5,
            [XmlEnum("6")]
            FuneralBurialExpenses = 6,
            [XmlEnum("7")]
            Others = 7
        }
        public enum FormOfPaymentEnum
        {
            [XmlEnum("C")]
            Cash,
            [XmlEnum("S")]
            Stock,
            [XmlEnum("F")]
            Fund,
            [XmlEnum("B")]
            Rollover,
            [XmlEnum("I")]
            IRA,
            [XmlEnum("1")]
            DirectPaymentParticipant,
            [XmlEnum("2")]
            DirectPaymentAlternatePayee,
            [XmlEnum("3")]
            SeparateAccountAlternatePayee,
            [XmlEnum("4")]
            PartlyDirectPartlyRollover,
            [XmlEnum("5")]
            RetitleToSpouse,
            [XmlEnum("6")]
            RothIRARollover,
            [XmlEnum("7")]
            Roth401kRollover,
            [XmlEnum("8")]
            DirectPaymentBeneficiary,
            [XmlEnum("9")]
            SeparateAccountSpouse,
            [XmlEnum("10")]
            IRARollover,
            [XmlEnum("11")]
            OtherFIRollover
        }

        public enum PaymentMethodEnum
        {
            [XmlEnum("D")]
            Dollars,
            [XmlEnum("N")]
            NumberOfPayments,
            [XmlEnum("U")]
            Units
        }

        public enum MRDTypeEnum
        {
            [XmlEnum("1")]
            MinReqdOnly,
            [XmlEnum("2")]
            MinReqdPlusAmount
        }
        public enum AccountTypeEnum
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
        public enum HierarchyTypeTypeEnum
        {
            [XmlEnum("1")]
            ProrateAcrossAllSources,
            [XmlEnum("2")]
            WithdrawAccordingToHierarchy,
            [XmlEnum("3")]
            WithdrawFromOneFullyVestedSource
        }
        public enum RequestTypeEnum
        {
            [XmlEnum("-1")]
            Not_Reqd = -1,
            [XmlEnum("0")]
            Terminated = 0,
            [XmlEnum("1")]
            HardshipWithdrawal = 1,
            [XmlEnum("2")]
            InService59Withdrawal = 2,
            [XmlEnum("3")]
            New_TA_IRA = 3,
            [XmlEnum("4")]
            Existing_TA_IRA = 4,
            [XmlEnum("5")]
            Other_Financial_Inst = 5,
            [XmlEnum("6")]
            Lump_Sum = 6,
            [XmlEnum("7")]
            Loans = 7,
            [XmlEnum("8")]
            MinimumRequiredDistribution = 8,
            [XmlEnum("9")]
            TestingFailureRefund = 9,
            [XmlEnum("10")]
            QDRO = 10,
            [XmlEnum("11")]
            DeathBenefitClaim = 11,
            [XmlEnum("12")]
            Disability = 12,
            [XmlEnum("13")]
            InServiceAge62Withdrawal = 13,
            [XmlEnum("14")]
            New_ML_IRA = 14,
            [XmlEnum("15")]
            ChildBirthAdoptionWithdrawal = 15
        }

        public enum EmployeeTypeEnum
        {
            [XmlEnum("1")]
            ACTIVE = 1,
            [XmlEnum("2")]
            TERMINATED = 2
        }
        public enum FromAccountEnum
        {
            [XmlEnum("0")]
            Unknown = 0,
            [XmlEnum("1")]
            Traditional = 1,
            [XmlEnum("2")]
            Roth = 2,
            [XmlEnum("3")]
            All = 3 /* Added for ISC*/
        }

        public enum E_MoneyTypeID
        {
            [XmlEnum("0")]
            None,
            [XmlEnum("1")]
            EmployeePreTax,
            [XmlEnum("2")]
            EmployeeRoth,
            [XmlEnum("3")]
            VoluntaryAfterTax,
            [XmlEnum("4")]
            MandatoryAfterTax,
            [XmlEnum("5")]
            Rollover,
            [XmlEnum("6")]
            RolloverRoth,
            [XmlEnum("7")]
            EmployerMatch,
            [XmlEnum("8")]
            SafeHarborMatch,
            [XmlEnum("9")]
            ProfitSharing,
            [XmlEnum("10")]
            SafeHarborNonelective,
            [XmlEnum("11")]
            EmployerQNEC,
            [XmlEnum("12")]
            EmployerQMAC,
            [XmlEnum("13")]
            MoneyPurchase,
            [XmlEnum("14")]
            PrevailingWage,
            [XmlEnum("15")]
            FullyVestedMoneyPurchase
        }
        public enum E_RelationShipTypeID
        {
            [XmlEnum("0")]
            None,
            [XmlEnum("1")]
            Spouse,
            [XmlEnum("2")]
            Child,
            [XmlEnum("3")]
            Sister,
            [XmlEnum("4")]
            Brother,
            [XmlEnum("5")]
            Mother,
            [XmlEnum("6")]
            Father,
            [XmlEnum("7")]
            PersonalRepresentativeOfEstate,
            [XmlEnum("8")]
            GuardianOrConservatorOfMinorBeneficiaryEstate,
            [XmlEnum("9")]
            TrusteeOfTrustBeneficiary,
            [XmlEnum("10")]
            CustodianOfMinorBeneficiaryUnderUniformGiftTransfersToMinorsAct,
            [XmlEnum("11")]
            SuccessorUnderSmallEstateAffidavit,
            [XmlEnum("12")]
            Other
        }
        public enum E_FormOfRefundTypeID
        {
            [XmlEnum("1")]
            ADP_ACP,
            [XmlEnum("2")]
            ref_402g,
            [XmlEnum("3")]
            ref_415
        }
        public enum PaidBy
        {
            [XmlEnum("0")]
            None = 0,
            [XmlEnum("10")]
            Employer = 10,
            [XmlEnum("20")]
            Participant = 20,
            [XmlEnum("30")]
            AlternatePayee = 30
        }

        //class properties
        //Distribution ID in the database
        public string lOLDID;
        //Distribution Status
        public DistributionStatusEnum Status;
        //Distribution Type
        public RequestTypeEnum RequestType;
        //Distribution Request Date
        public string RequestDate;
        public string ProcessedDate;

        //Employee Type
        public EmployeeTypeEnum EmployeeType;
        //Description of Distribution Type
        public string sRequestTypeDesc;
        //Distribution Amount
        public double DistributionAmount;
        //Distribution Amount
        public double UpdatedDistributionAmount;
        //Distribution Amount
        public double AdditionalDistributionAmount;
        //AfterTax Principal
        public double AfterTaxPrincipal;
        //Roth Balance
        public double RothBalance;
        //Spouse Consent Required 
        public bool bSpouseConsentReqd;
        //Flag to indicate Federal Tax Withholding required, Default to True
        public bool FederalTaxWithheld;
        public TaxValuesInfo FederalTaxInfo = new();
        //Flag to indicate State Tax Withholding required, Default to True
        public bool StateTaxWithheld;

        public TaxValuesInfo StateTaxInfo = new();
        //Flag to indicate if Participant elected to waive 30 days notice waiting period
        public bool b30DaysNotice;
        //Mailing option selected for this distribution
        public MailingOptionEnum MailingOption = MailingOptionEnum.None;

        public bool OvernightMail = false;
        //Date user cancelled this distribution
        public string sCancellationDate;
        //Date user waived Pending 30 days waiting period
        public string sWaiverDate;
        //Employment Termination Date
        public string Employment_Termination_Dt;
        //Form of payment (Always C - Cash to be sent to TAE)
        public FormOfPaymentEnum Payment_Form;
        //Form of payment (Always C - Cash to be sent to TAE)
        public FormOfPaymentEnum AdditionalPayment_Form1;
        //Form of payment (Always C - Cash to be sent to TAE)
        public FormOfPaymentEnum AdditionalPayment_Form2;
        public int DistributionPercent;

        public bool bRepaymentOutstandingLoan;

        public bool bMaritalStatus;
        public RolloverInfo RolloverInfo;
        public PersonalProfile PersonalInfo;
        public TerminationInfo TerminationInfo;
        //Rollover Distribution Values
        //Public Rollover_To As RolloverToEnum        'Rollover to
        //Public sRollover_Account_Name As String     'Rollover Account Name
        //Public sRollover_Account_Code As String     'Rollover Account Code / Number
        //Public sCheck_Payable_Name As String        'Name under whose check would be made payable to 
        //Public sTrusteeName As String               'Name of the Trustee
        //Public sTrusteeAddress1 As String           'Address details of the Trustee
        //Public sTrusteeAddress2 As String
        //Public sTrusteeCity As String
        //Public sTrusteeState As String
        //Public sTrusteeZipCode As String
        //Public sTrusteeZipRouteCode As String


        //public AccountTypeEnum AccountType;
        //Distribution Type
        public RequestTypeEnum AdditionalRequestType = RequestTypeEnum.Not_Reqd;

        //Hardship Distribution related Values
        //Hardship Reason Type
        public HardshipReasonsEnum[] HardshipReasons;
        //1.	Purchase of a Principal Residence, 2.College/Post-Secondary Tuition, 3.	Medical Expenses, 4. Foreclosure/Eviction-Principal Residence
        public HardshipSupportedDocs[] cHardshipSupportedDocs;

        public string HardshipReasonOtherDesc;
        public FromAccountEnum FromAccount;
        //public WithdrawalHierarchyInfo WithdrawalHierarchyInfo;
        public QDROInfo QDROInfo;
        public MRDInfo MRDInfo;
        public DeathBenefitInfo DeathBenefitInfo;

        public TestingFailureRefundInfo TestingFailureRefundInfo;

        public NewVestingInfo[] NewVestingInfo;
        //Sponsor Related Values
        public double NewVestingPercent;

        public int HoursWorked;
        public string LastContributionDate;
        public double LastContributionAmt;
        public int sRevIndID;
        public string sRevContType;

        public string sRevName;
        //Not Captured bt default values to be sent to Partner
        public TerminationReasonEnum TerminationReason;
        //Current Date - Can be left blank
        public string TerminationPaymentDt;
        //Current Date - Can be left blank
        public string TerminationValuationDt;
        //Can be left blank
        public int TerminationAllowances;
        public double TerminationWithheldAmount;
        //Can be left blank
        public TerminationPaymentCycleEnum TerminationPaymentCycle;
        //Can be left blank
        public bool ForfeitNowPayLater;
        //Always D - Dollars
        public PaymentMethodEnum PaymentMethod;

        public bool RolloverIndicator;
        public int iIn_Login_ID;
        public string SSN;
        public string sContractID;
        public string sSubID;
        public string Division;
        public bool bTRSUpdate = false;
        public double DistributionFee;
        public PaidBy FeePaidBy = PaidBy.Participant;
        public double TPADistributionFee;
        public PaidBy TPAFeePaidBy = PaidBy.Participant;
        public string FeesPaidByStr;

        public string PartnerToken;
        public string TPAApproverName;
        public string TPAName;
        public string TPAApprovedDate;
        public string SponsorApprovedDate;
        public DenialReasonsEnum[] DenialReason;  //' change this same aas loan denials

        public string DenialReasonOther;
        public bool iSeriesContract = false;
        public string pptEmail;
        public string ConfirmationNumber;
        public string TransactionDate;
        public bool InserviceAs59 = false;
        public string TaxableState;

        public MailingAddressInfo MailingAddress;

        public string TPALoanFlagIndicator;
        public string TPAVestingFlagIndicator;
        public string TPADistributionFlagIndicator;

        //Description of Distribution Type
        public string ChildTIN;
        public string ChildDOB;
        public string CustomSubTypeCode;
        public double qbadAmountAvailable;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DisbursementInfo DisbursementInfo;

        [Serializable()]
        public class HardshipSupportedDocs
        {
            public string sHardshipType;
            public string sDocumentType;
            public string sDocSubmissionDate;
        }


        public UpdateDistributionInfo cUpdateDistInfo;

        public DistributionInfo OnlineDistributionSecondary;

        public SourceInfo[] SourceInfo;
        public AccountInfo[] AccountInfo;

        public double HardshipAmountAvailable;
        public double InServiceAmountAvailable;
        public double AccBal;

        public bool bTPAEntered;
        public List<string> OnlineConditions;
        public double OverNightFee;

    }

    [Serializable()]
    public class MailingAddressInfo
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool USAddress = true;

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
    }

    [Serializable()]
    public class NewVestingInfo
    {
        public string MoneyTypeID;
        public double VestingValue;
    }

    [Serializable()]
    public class TerminationInfo
    {
        public string NewTerminationDate;
        public DistributionInfo.LoanOptionEnum LoanOption = DistributionInfo.LoanOptionEnum.Not_Reqd;
        public RolloverInfo RolloverInfo;
    }
    [Serializable()]
    public class QDROInfo
    {
        public string AltPayeeName;
        public string AltPayeeSSN;
        public string AltPayeeDOB;
        public string AltPayeeAddress1;
        public string AltPayeeAddress2;
        public string AltPayeeCity;
        public string AltPayeeState;
        public string AltPayeeZip;
        public bool USAddress = true;
        public string AltPayeeCountry;
    }

    [Serializable()]
    public class MRDInfo
    {
        public DistributionInfo.MRDTypeEnum Type;
        public double MRDAmount;
        public double MRDAmount2;
        public double MRDAmount3;
    }

    [Serializable()]
    public class DeathBenefitInfo
    {
        public string DateOfDeath;
        public string State;
        public DistributionInfo.E_RelationShipTypeID BeneficiaryRelationship;
        public string OtherBeneficiaryRelationship;
        public BeneficiaryInfo BeneficiaryInfo;
    }
    [Serializable()]
    public class BeneficiaryInfo
    {
        public PersonalInfo PersonalInfo;
        public bool W8BENFormReceived;
        public bool TaxIDIncludedOnForm;
        public string TaxIDCountry;
    }

    [Serializable()]
    public class TestingFailureRefundInfo
    {
        public DistributionInfo.E_FormOfRefundTypeID FormOfRefund;
        public List<AccountTypeInfo> AccountTypeInfo;
        public Request402gInfo Request402gInfo;
        public string TaxedIn;

        public string Code_1099;
    }
    [Serializable()]
    public class AccountTypeInfo
    {
        //Money Type ID
        public int TypeID;
        public string TypeName;
        public double AmountRequested;
    }

    [Serializable()]
    public class Request402gInfo
    {
        public double ExcessSalaryDeferral;
        public double Earnings;
        public string ExcessDeferral_1099Code;
        public string Earnings_1099Code;
    }

    [Serializable()]
    public class PersonalInfo
    {
        public bool USCitizen;
        public string FirstName;
        public string LastName;
        public string Address1;
        public string Address2;
        public string City;
        public string State;
        public string Zipcode1;
        public string Zipcode2;
        public string Country;
        public string TaxIDNumber;
        public string TrusteeName;
    }

    [Serializable()]
    public class RolloverInfo
    {
        //Rollover to
        public DistributionInfo.RolloverToEnum Rollover_To = DistributionInfo.RolloverToEnum.Not_Reqd;
        //Rollover Account Name
        public string Rollover_Account_Name;
        //Rollover Account Code / Number
        public string Rollover_Account_Code;
        //Name under whose check would be made payable to 
        public string Check_Payable_Name;
        //Name of the Trustee
        public string TrusteeName;
        //Address details of the Trustee
        public string TrusteeAddress1;
        public string TrusteeAddress2;
        public string TrusteeCity;
        public string TrusteeState;
        public string TrusteeZipCode;
        public string TrusteeZipRouteCode;
        public bool bOvernightMail;
        public double OvernightFee;
        public string XTraditionalRollover;
    }

    [Serializable()]
    public class TaxValuesInfo
    {
        public double DollarsWithheld;
        public double PercentWithheld;
        public double DollarsWithheld2;
        public double PercentWithheld2;
    }

    [Serializable()]
    public class UpdateDistributionInfo
    {
        public string lOldID;
        public DistributionInfo.DistributionStatusEnum iStatus;
        public bool bConfirmPptMaritalStatus;
        public bool bSpouseConsentRcvd;
        public bool bParticpant_DocRcvd;
        //If iStatus = Cancelled, this will be Cancellation Date, if iStatus=0, this will be Waived30Days notice date
        public string dDate;
    }
}