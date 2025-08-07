using System.Xml.Serialization;

namespace TRS.IT.SI.BusinessFacadeLayer.Model
{
    public class ReportInfo
    {
        public const int NO_RECORDS = 909;
        public enum SortOptionEnum
        {
            [XmlEnum("0")]
            SSN = 0,
            [XmlEnum("1")]
            LastName = 1,
            [XmlEnum("2")]
            DollarAmount = 2,
            [XmlEnum("3")]
            Age = 3,
            [XmlEnum("4")]
            NameWithinLocation = 4,
            [XmlEnum("5")]
            BalanceWithLocation = 5,
            [XmlEnum("1")]
            VestedValue = 1,
            [XmlEnum("2")]
            VestedValueMinusRollover = 2,
            [XmlEnum("3")]
            PortfolioAssets = 3,

        }
        public enum CompanyEnum
        {
            All = 0,
            NonHRA = 1,
            HRAOnly = 2
        }
        public enum SortOrderEnum
        {
            [XmlEnum("0")]
            None = 0,
            [XmlEnum("1")]
            Ascending = 1,
            [XmlEnum("2")]
            Descending = 2
        }
        public enum ReportStatusEnum
        {
            Failed = 0,
            Available = 1,
            Pending = 2,
            Deleted = 3,
            PSDError = 4,
            NoData = 5
        }
        public enum PlanReportOptionEnum
        {
            [XmlEnum("0")]
            None = 0,
            [XmlEnum("1")]
            ByProcessEndDate = 1,
            [XmlEnum("2")]
            ByPayrollEndingDate = 2,
            [XmlEnum("3")]
            ByEffectiveDate = 3,
            [XmlEnum("4")]
            ByTradeDate = 4,
            //Begin Modified By Eswar on 04/25/2005
            [XmlEnum("5")]
            ByPostDate = 5,
            [XmlEnum("6")]
            ByPayrollDate = 6,
            //end Modified By Eswar on 04/25/2005 
            [XmlEnum("7")]
            ByPrevPlanYear = 7,
            [XmlEnum("8")]
            ByCurrentPlanYear = 8
        }
        public enum ReportDisplayTypeEnum : int
        {
            [XmlEnum("0")]
            None = 0,
            [XmlEnum("1")]
            PDF = 1,
            [XmlEnum("2")]
            ReportingService = 2,
            [XmlEnum("3")]
            CSV = 3,
            [XmlEnum("4")]
            XLS = 4
        }
        public enum ReportTypeEnum : int
        {
            None = 0,
            AccountStatement = 1,
            ContributionLimit = 2,
            ContributionDetails = 3,
            ContributionRateChange = 4,
            ContributionRateChangeText = 5,
            ContributionByMoneyType = 6,
            ContributionSummaryByFund = 7,
            DemographicByVestedPercent = 8,
            DemographicEligibility = 9,
            DemographicDesignatedAge = 10,
            DemographicEmployeeAddress = 11,
            DemographicActiveInactive = 12,
            DemographicInactiveParticipant = 13,
            DemographicIncompleteDataForActiveParticipants = 14,
            DemographicParticpantDisplay = 15,
            DemographicParticipantCensusData = 16,
            DistributionEmployeeDisbursement = 17,
            DistributionDeminimusBalance = 18,
            LoansBalance = 19,
            LoansIssued = 20,
            LoansPaymentHistory = 21,
            LoansPaidOff = 22,
            PlanLevelInvestmentSummary = 23,
            PlanLevelForefeitureBalance = 24,
            PlanLevelProcessingHistory = 25,
            PlanLevelHeadCountByFund = 26,
            ParticipantStatement = 27,
            PlanDataCsvFile = 28,
            CensusFile = 29,
            ParticipantCensusData = 30,
            //Now Investment Elections & YTD Contributions
            AccountBalanceAsOf = 31,
            PlanLevelMultiLocationParticipants = 32,
            DemographicParticipantBalanceByFund = 33,
            ContributionDetailsCSV = 34,
            ParticipantInvestmentElections = 35,
            ParticipantIndicativeData = 36,
            LoanDetail = 37,
            BasicPlanInformation = 38,
            ConversionStatement = 39,
            PayrollTemplate = 40,
            Enrollment = 41,
            CensusData = 42,
            PlanData = 43,
            AccountStatement_SuppressVesting = 44,
            ContributionRateChange_2 = 45,
            ContributionRate = 46,
            ParticipantEligibility = 47,
            ParticipantLoanBalance = 48,
            ParticipantLoanIssued = 49,
            MinRequiredDistribution = 50,
            IndicativeDataDownload_NoVesting = 51,
            IndicativeDataDownload_Vesting = 52,
            DiscriminationDataDownload = 53,
            LoanDataDownload = 54,
            ParticipantBasisDataDownload = 55,
            MidYearCensusDownload = 56,
            ParticipantEligibilityCsv = 57,
            PlanAdminstration = 58,
            MEPActivity = 59,
            HardshipSuspension = 60,
            PlanDataXlsFile = 61,
            InvestmentSummaryTPA = 62,
            PASSAnnualNotice = 63,
            PASSSummaryAnnualReport = 64,
            PASSSummaryPlanDescription = 65,
            PASSSummaryOfMaterialModifications = 66,
            PASSForceOutDistribution = 67,
            PASSForceOutTermination = 68,
            PASSEnrollment = 69,
            LoanRegister = 71,
            EmployerReport = 72,
            RequestATest = 73,
            PortfolioActiveParticipants = 74,
            PortfolioSubscription = 75,
            BeneficiaryDetails = 76,
            ChangestoSafeHarborNonElectiveRate = 77,
            ForfeitureReport = 78,
            AUTOEnrollment = 79,
            PayrollTemplateMEP = 80,
            CensusFileMEP = 81,
            UnclaimedBenefits_Trinet = 83,
            UnclaimedBenefits = 84,
            P360Report = 85,
            ExpenseBudgetAccount = 86,
            ParticipantDataFile = 87,
            CSCPayrollTemplate = 88,
            ESReport = 89,
            OldRateChangeReport = 90,
            DefaultInvestment = 91,
            TPAFee = 92,
            TrinetLCRC = 110, //DDEV-47686
            LTPTReport = 114     // TTWDCLM-45050
        }
        public string PartnerUserID = "NBITEST";
        public int ReportType;
        public string StartDate;
        public string EndDate;
        public double DollarAmount;
        public SortOptionEnum SortBy;
        public SortOrderEnum SortOrder;
        public string PlanID;
        public string MemberID;
        public string LastName;
        public float AgeLimit;
        public int ParticipantType;
        public ReportStatusEnum Status;
        //applies to the contribution rate change text file report
        public bool FullFile;
        public PlanReportOptionEnum PlanReportOption;
        public string ContractID;
        public string SubID;
        public string PartnerID;
        public int ErrorCode;
        public string FTPLocation = "";
        public CompanyEnum Company = CompanyEnum.All;
        public ReportDisplayTypeEnum ReportDisplayType = ReportDisplayTypeEnum.PDF;
        public string LocationCode;
        public string UserID;
        public string ApplicationName;
        public string CustomReportName;
        public int ContributionParticipantType;
        public string hidesocial;
    }
    public class ReportResponse : SIResponse
    {
        public string FileName;

        public string Request;
        public ReportResponse()
        {
            ErrorInfo[] oErrors = [new ErrorInfo()];
            Errors = oErrors;
        }

    }

}