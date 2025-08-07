using System.Data;
using SIModel;
using BFL = TRS.IT.SI.BusinessFacadeLayer;

namespace SIPBO
{
    public class TRSCommon
    {
        public static string GetData(DataSet ds, string FieldName, int tblNumber = 0, int RowNumber = 0)
        {

            if (ds.Tables[tblNumber].Rows[RowNumber][FieldName] is DBNull)
            {
                return "";
            }
            else
            {
                return Convert.ToString(ds.Tables[tblNumber].Rows[RowNumber][FieldName]);
            }

        }
        public static string SubIn(string a_sSubID)
        {
            return BFL.Util.SubIn(a_sSubID);
        }
        public static string GetReportPath(BFL.Model.ReportInfo.ReportTypeEnum iReportType, string sPartnerID)
        {
            switch (sPartnerID ?? "")
            {
                case SIEnums.C_PartnerID_PENCO:
                    {
                        return GetReportPath(iReportType);
                    }
                case SIEnums.C_PartnerID_ISC:
                    {
                        return GetISCReportPath(iReportType);
                    }

                default:
                    {
                        return GetTAEReportPath(iReportType);
                    }
            }
        }
        public static string GetISCReportPath(BFL.Model.ReportInfo.ReportTypeEnum iReportType)
        {
            switch (iReportType)
            {
                case BFL.Model.ReportInfo.ReportTypeEnum.RequestATest:
                    {
                        return TRS.IT.TrsAppSettings.AppSettings.GetValue("Folder_ISCReports_RequestATest");
                    }

                default:
                    {
                        return "";
                    }
            }
        }
        public static string GetTAEReportPath(BFL.Model.ReportInfo.ReportTypeEnum iReportType)
        {
            switch (iReportType)
            {
                case BFL.Model.ReportInfo.ReportTypeEnum.RequestATest:
                    {
                        return TRS.IT.TrsAppSettings.AppSettings.GetValue("Folder_TAEReports_RequestATest");
                    }

                default:
                    {
                        return "";
                    }
            }

        }
        public static string GetReportPath(BFL.Model.ReportInfo.ReportTypeEnum iReportType)
        {
            switch (iReportType)
            {
                case BFL.Model.ReportInfo.ReportTypeEnum.ParticipantCensusData:
                    {
                        return "Retention2/MLCensusDownloadFile/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PlanDataCsvFile:
                case BFL.Model.ReportInfo.ReportTypeEnum.PlanDataXlsFile:
                    {
                        return "Retention2/MLTRXDataDownloadFile/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ParticipantStatement:
                case BFL.Model.ReportInfo.ReportTypeEnum.AccountStatement:
                    {
                        return "Retention2/TaPartStmt/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ContributionLimit:
                    {
                        return "Retention2/ContLmtRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ContributionDetails:
                    {
                        return "Retention2/YearContRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ContributionByMoneyType:
                    {
                        return "Retention2/ContByMoneyRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ContributionSummaryByFund:
                    {
                        return "Retention2/ContByFundRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ContributionRateChange:
                case BFL.Model.ReportInfo.ReportTypeEnum.ContributionRateChangeText:
                    {
                        return "Retention2/ContribChgRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicByVestedPercent:
                    {
                        return "Retention2/VestingRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicEligibility:
                    {
                        return "Retention2/EnrollmentRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicDesignatedAge:
                    {
                        return "Retention2/AgeRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicActiveInactive:
                    {
                        return "Retention2/EEStatusRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicIncompleteDataForActiveParticipants:
                    {
                        return "Retention2/IncDataRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.AccountBalanceAsOf:
                    {
                        return "Retention2/AccBalanceRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicEmployeeAddress:
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicInactiveParticipant:
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicParticpantDisplay:
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicParticipantCensusData:
                    {
                        return "Retention2/EmpAddrRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DistributionEmployeeDisbursement:
                    {
                        return "Retention2/DisbursRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DistributionDeminimusBalance:
                    {
                        return "Retention2/DeminimusRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.LoansBalance:
                case BFL.Model.ReportInfo.ReportTypeEnum.LoansIssued:
                    {
                        return "Retention2/LoanBalanceRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.LoansPaymentHistory:
                    {
                        return "Retention2/LoanPaymentRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.LoansPaidOff:
                    {
                        return "Retention2/LoansPaidRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PlanLevelForefeitureBalance:
                case BFL.Model.ReportInfo.ReportTypeEnum.PlanLevelProcessingHistory:
                case BFL.Model.ReportInfo.ReportTypeEnum.PlanLevelHeadCountByFund:
                    {
                        return "Retention2/ForfeitureRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DemographicParticipantBalanceByFund:
                    {
                        return "Retention2/BalByFndRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PlanLevelInvestmentSummary:
                case BFL.Model.ReportInfo.ReportTypeEnum.PlanLevelMultiLocationParticipants:
                case BFL.Model.ReportInfo.ReportTypeEnum.ParticipantInvestmentElections:
                case BFL.Model.ReportInfo.ReportTypeEnum.LoanDetail:
                    {
                        return "Retention2/ParticipantRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.InvestmentSummaryTPA:
                    {
                        return "Retention2/InvSummaryRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ParticipantIndicativeData:
                    {
                        return "Retention2/NBIIndDataDwnlds/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PayrollTemplate:
                    {
                        return "Retention2/PDITemplate/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.CensusFile:
                    {
                        return "Retention2/CensusDownloadFile/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.MidYearCensusDownload:
                    {
                        return "Retention2/CensusMidYearDownloadFile/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ContributionRateChange_2:
                    {
                        return "Retention2/TPAContribChgRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ContributionRate:
                    {
                        return "Retention2/TPAContribRateRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ParticipantEligibility:
                    {
                        return "Retention2/TPAEnrollmentRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ParticipantLoanBalance:
                    {
                        return "Retention2/TPALoanBalanceRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ParticipantLoanIssued:
                case var @case when @case == BFL.Model.ReportInfo.ReportTypeEnum.LoansIssued:
                    {
                        return "Retention2/TPALoanIssuedRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.MinRequiredDistribution:
                    {
                        return "Retention2/TPAMinReqDistRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.IndicativeDataDownload_NoVesting:
                    {
                        return "Retention2/TPAIndDataDwnlds/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.IndicativeDataDownload_Vesting:
                    {
                        return "Retention2/TPAIndDataVstDwnlds/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.DiscriminationDataDownload:
                    {
                        return "Retention2/TPADiscTestDwnlds/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.LoanDataDownload:
                    {
                        return "Retention2/TPALoanDataDwnlds/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ParticipantBasisDataDownload:
                    {
                        return "Retention2/TPAPartBasisDwnlds/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PlanAdminstration:
                    {
                        return "Retention2/AdminRpt1/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PASSAnnualNotice:
                    {
                        return "Retention2/PASSAnnualNotice/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PASSSummaryAnnualReport:
                    {
                        return "Retention2/PASSSummaryAnnualReport/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PASSSummaryPlanDescription:
                    {
                        return "Retention2/PASSSummaryPlanDescription/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PASSSummaryOfMaterialModifications:
                    {
                        return "Retention2/PASSSummaryOfMaterialModifications/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PASSForceOutDistribution:
                    {
                        return "Retention2/PASSForceOutDistribution/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PASSForceOutTermination:
                    {
                        return "Retention2/PASSForceOutTermination/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PASSEnrollment:
                    {
                        return "Retention2/PASSEnrollment/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.LoanRegister:
                    {
                        return "Retention2/LoanRegisterRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.BeneficiaryDetails:
                    {
                        return "Retention2/BeneficiaryRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PortfolioActiveParticipants:
                    {
                        return "Retention2/PXActivePptRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.PortfolioSubscription:
                    {
                        return "Retention2/PXSubscriptRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.ForfeitureReport:
                    {
                        return "Retention2/ForfSummaryRpts/";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.AUTOEnrollment:
                    {
                        return "Retention2/AutoEnrollmentRpts";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.RequestATest:
                    {
                        return "";
                    }
                case BFL.Model.ReportInfo.ReportTypeEnum.P360Report:
                    {
                        return "Retention1/PayrollVendorFeed1/";
                    }
            }
            return string.Empty;
        }
    }
}