using System.Data;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace TRS.IT.SI.BusinessFacadeLayer
{
    public class Util
    {
        public static void SendMail(string FromEmail, string ToEmail, string Subject, string EmailBody, bool HTMLFormat = false, bool BccEmail = true, string CcEmail = "")
        {
            var oMail = new MimeMessage();
            oMail.From.Add(new MailboxAddress("", FromEmail));
            oMail.Subject = Subject;
            var bodyBuilder = new BodyBuilder();
            if (HTMLFormat)
            {
                bodyBuilder.HtmlBody = EmailBody;
            }
            else
            {
                bodyBuilder.TextBody = EmailBody;
            }
            oMail.Body = bodyBuilder.ToMessageBody();
            SplitEmailsToMAC(oMail.To, ToEmail);
            if (!string.IsNullOrEmpty(CcEmail))
            {
                SplitEmailsToMAC(oMail.Cc, CcEmail);
            }
            if (BccEmail)
            {
                SplitEmailsToMAC(oMail.Bcc, TrsAppSettings.AppSettings.GetValue("TrsWebMails"));
            }
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.ServerCertificateValidationCallback = (s, c, h, e) => {
                    return true;
                };
                smtpClient.Connect(TrsAppSettings.AppSettings.GetValue("SMTPServer"), 587, SecureSocketOptions.StartTls);
                smtpClient.Send(oMail);
                smtpClient.Disconnect(true);
            }
        }


        public static void SplitEmailsToMAC(InternetAddressList addressList, string strArray)
        {
            if (!string.IsNullOrEmpty(strArray))
            {
                foreach (string s in strArray.Split(';'))
                {
                    if (!string.IsNullOrEmpty(s.Trim()))
                    {
                        addressList.Add(new MailboxAddress("", s.Trim()));
                    }
                }
            }
        }
        public static void SendPartnerUnavailableMail(string SessionID, Model.PartnerFlag partnerID, string errorString, string errorNumber)
        {
            SendMail(TrsAppSettings.AppSettings.GetValue("FromEmail"), TrsAppSettings.AppSettings.GetValue("ToEmail"), "Partner:" + partnerID.ToString() + " is down or timeout - " + errorNumber, "SessionID:" + SessionID + Environment.NewLine + "Error:" + errorString, false);
            DAL.General.LogErrors(@"BusinessFacadeLayer\Participant.vb", "N/A", "BusinessFacadeLayer", errorString, "N/A");
        }
        public static string GetReportPath(Model.ReportInfo.ReportTypeEnum iReportType, Model.PartnerFlag _partnerFlag)
        {
            if (_partnerFlag == Model.PartnerFlag.Penco)
            {
                return GetReportPath(iReportType);
            }
            else
            {
                return GetTAEReportPath(iReportType);
            }
        }
        public static string GetTAEReportPath(Model.ReportInfo.ReportTypeEnum iReportType)
        {
            switch (iReportType)
            {
                case Model.ReportInfo.ReportTypeEnum.RequestATest:
                    {
                        return TrsAppSettings.AppSettings.GetValue("Folder_TAEReports_RequestATest");
                    }

                default:
                    {
                        return "";
                    }
            }
        }
        public static string GetReportPath(Model.ReportInfo.ReportTypeEnum iReportType)
        {
            string strTemp;
            strTemp = "";
            switch (iReportType)
            {
                case Model.ReportInfo.ReportTypeEnum.ParticipantCensusData:
                    {
                        strTemp = "MLCensusDownloadFile/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PlanDataCsvFile:
                case Model.ReportInfo.ReportTypeEnum.PlanDataXlsFile:
                    {
                        strTemp = "MLTRXDataDownloadFile/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ParticipantStatement:
                case Model.ReportInfo.ReportTypeEnum.AccountStatement:
                    {
                        strTemp = "TaPartStmt/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ContributionLimit:
                    {
                        strTemp = "ContLmtRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ContributionDetails:
                    {
                        strTemp = "YearContRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ContributionByMoneyType:
                    {
                        strTemp = "ContByMoneyRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ContributionSummaryByFund:
                    {
                        strTemp = "ContByFundRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ContributionRateChange:
                case Model.ReportInfo.ReportTypeEnum.ContributionRateChangeText:
                    {
                        strTemp = "ContribChgRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DemographicByVestedPercent:
                    {
                        strTemp = "VestingRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DemographicEligibility:
                    {
                        strTemp = "EnrollmentRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DemographicDesignatedAge:
                    {
                        strTemp = "AgeRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DemographicActiveInactive:
                    {
                        strTemp = "EEStatusRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DemographicIncompleteDataForActiveParticipants:
                    {
                        strTemp = "IncDataRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.AccountBalanceAsOf:
                    {
                        strTemp = "AccBalanceRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DemographicEmployeeAddress:
                case Model.ReportInfo.ReportTypeEnum.DemographicInactiveParticipant:
                case Model.ReportInfo.ReportTypeEnum.DemographicParticpantDisplay:
                case Model.ReportInfo.ReportTypeEnum.DemographicParticipantCensusData:
                    {
                        strTemp = "EmpAddrRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DistributionEmployeeDisbursement:
                    {
                        strTemp = "DisbursRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DistributionDeminimusBalance:
                    {
                        strTemp = "DeminimusRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.LoansBalance:
                    {
                        strTemp = "LoanBalanceRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.LoansPaymentHistory:
                    {
                        strTemp = "LoanPaymentRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.LoansPaidOff:
                    {
                        strTemp = "LoansPaidRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PlanLevelForefeitureBalance:
                case Model.ReportInfo.ReportTypeEnum.PlanLevelProcessingHistory:
                case Model.ReportInfo.ReportTypeEnum.PlanLevelHeadCountByFund:
                    {
                        strTemp = "ForfeitureRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DemographicParticipantBalanceByFund:
                    {
                        strTemp = "BalByFndRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PlanLevelInvestmentSummary:
                case Model.ReportInfo.ReportTypeEnum.PlanLevelMultiLocationParticipants:
                case Model.ReportInfo.ReportTypeEnum.ParticipantInvestmentElections:
                case Model.ReportInfo.ReportTypeEnum.LoanDetail:
                    {
                        strTemp = "ParticipantRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.InvestmentSummaryTPA:
                    {
                        strTemp = "InvSummaryRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ParticipantIndicativeData:
                    {
                        strTemp = "NBIIndDataDwnlds/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PayrollTemplate:
                    {
                        strTemp = "PDITemplate/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.CensusFile:
                    {
                        strTemp = "CensusDownloadFile/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ContributionRateChange_2:
                    {
                        strTemp = "TPAContribChgRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ContributionRate:
                    {
                        strTemp = "TPAContribRateRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ParticipantEligibility:
                    {
                        strTemp = "TPAEnrollmentRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ParticipantLoanBalance:
                    {
                        strTemp = "TPALoanBalanceRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ParticipantLoanIssued:
                case Model.ReportInfo.ReportTypeEnum.LoansIssued:
                    {
                        strTemp = "TPALoanIssuedRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.MinRequiredDistribution:
                    {
                        strTemp = "TPAMinReqDistRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.IndicativeDataDownload_NoVesting:
                    {
                        strTemp = "TPAIndDataDwnlds/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.IndicativeDataDownload_Vesting:
                    {
                        strTemp = "TPAIndDataVstDwnlds/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.DiscriminationDataDownload:
                    {
                        strTemp = "TPADiscTestDwnlds/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.LoanDataDownload:
                    {
                        strTemp = "TPALoanDataDwnlds/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ParticipantBasisDataDownload:
                    {
                        strTemp = "TPAPartBasisDwnlds/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.MidYearCensusDownload:
                    {
                        strTemp = "CensusMidYearDownloadFile/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PlanAdminstration:
                    {
                        strTemp = "AdminRpt1/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PASSAnnualNotice:
                    {
                        strTemp = "PASSAnnualNotice/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PASSSummaryAnnualReport:
                    {
                        strTemp = "PASSSummaryAnnualReport/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PASSSummaryPlanDescription:
                    {
                        strTemp = "PASSSummaryPlanDescription/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PASSSummaryOfMaterialModifications:
                    {
                        strTemp = "PASSSummaryOfMaterialModifications/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PASSForceOutDistribution:
                    {
                        strTemp = "PASSForceOutDistribution/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PASSForceOutTermination:
                    {
                        strTemp = "PASSForceOutTermination/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PASSEnrollment:
                    {
                        strTemp = "PASSEnrollment/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.LoanRegister:
                    {
                        strTemp = "LoanRegisterRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.BeneficiaryDetails:
                    {
                        strTemp = "BeneficiaryRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PortfolioActiveParticipants:
                    {
                        strTemp = "PXActivePptRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.PortfolioSubscription:
                    {
                        strTemp = "PXSubscriptRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.ForfeitureReport:
                    {
                        strTemp = "ForfSummaryRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.AUTOEnrollment:
                    {
                        strTemp = "AutoEnrollmentRpts/";
                        break;
                    }
                case Model.ReportInfo.ReportTypeEnum.RequestATest:
                    {
                        return "";
                    }
                case Model.ReportInfo.ReportTypeEnum.P360Report:
                    {
                        strTemp = "PayrollVendorFeed1/";
                        break;
                    }
            }
            return "Retention2/" + strTemp;
        }
        public static string GetKeyValue(string sKey, List<IT.SOA.Model.KeyValue> oKeyValuePair)
        {
            string strValue = "";
            if (!(oKeyValuePair == null))
            {
                object KeyVal = (from kv in oKeyValuePair
                                 where kv.key.ToLower() == sKey.ToLower()
                                 select kv.value).FirstOrDefault();

                if (!(KeyVal == null))
                {
                    strValue = KeyVal.ToString();
                }

            }

            return strValue;
        }
        public static string SubIn(string a_sSubID)
        {
            return DAL.General.SubIn(a_sSubID);
        }
        public static string SubOut(string a_sSubID)
        {
            return DAL.General.SubOut(a_sSubID);
        }
    }
}