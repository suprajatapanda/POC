using System.Data;
using SIModel;
using SIUtil;
using BFL = TRS.IT.SI.BusinessFacadeLayer;

namespace SIPBO
{
    public class TRSReportHelper
    {
        public static bool IsReportAvailable(int ReportID)
        {

            try
            {
                if (!(TRS.IT.TrsAppSettings.AppSettings.GetValue("P3UnavailableReports") == null))
                {
                    string[] UnavailableReports = TRS.IT.TrsAppSettings.AppSettings.GetValue("P3UnavailableReports").ToString().Split(',');
                    foreach (string RptID in UnavailableReports)
                    {
                        if ((RptID.Trim() ?? "") == (ReportID.ToString().Trim() ?? ""))
                        {
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
            }

            return true;
        }
        public static string GetReport(int iInloginId, BFL.Model.ReportInfo rpt) // single report - returns complete report file path
        {
            string strReportFile = "";
            string strReportFolder = "";
            string sSessionID;
            BFL.Sponsor oSponsor = null;

            BFL.Model.ReportResponse oResponse;
            string rptPath;

            if ((rpt.PartnerID ?? "") == SIEnums.C_PartnerID_ISC)
            {
                if (IsReportAvailable(rpt.ReportType))
                {
                    sSessionID = BFL.Sponsor.CreateSession(iInloginId, rpt.ContractID, rpt.SubID);
                    oSponsor = new BFL.Sponsor(sSessionID);
                    oResponse = oSponsor.GetReport(rpt);
                }
                else
                {
                    oResponse = new BFL.Model.ReportResponse();
                    oResponse.Errors[0].Number = BFL.Model.ReportInfo.NO_RECORDS;
                }
            }
            else
            {
                sSessionID = BFL.Sponsor.CreateSession(iInloginId, rpt.ContractID, rpt.SubID);
                oSponsor = new BFL.Sponsor(sSessionID);
                oResponse = oSponsor.GetReport(rpt);
            }

            if (oResponse.Errors[0].Number == 0)
            {
                strReportFolder = TRSCommon.GetReportPath((BFL.Model.ReportInfo.ReportTypeEnum)rpt.ReportType, rpt.PartnerID);
                if ((rpt.PartnerID ?? "") == SIEnums.C_PartnerID_PENCO)
                {
                    rptPath = System.IO.Path.Combine(TRS.IT.TrsAppSettings.AppSettings.GetValue("Path_PencoReports"), strReportFolder);
                }
                else if ((rpt.PartnerID ?? "") == SIEnums.C_PartnerID_ISC)
                {
                    rptPath = System.IO.Path.Combine(TRS.IT.TrsAppSettings.AppSettings.GetValue("Path_ISCReports"), strReportFolder);
                }
                else
                {
                    rptPath = System.IO.Path.Combine(TRS.IT.TrsAppSettings.AppSettings.GetValue("Path_TAEReports"), strReportFolder);
                }
                strReportFile = System.IO.Path.Combine(rptPath, oResponse.FileName);
            }

            else
            {
                rpt.ErrorCode = oResponse.Errors[0].Number;
                strReportFile = "Error:" + oResponse.Errors[0].Description;
            }

            return strReportFile;

        }
        public static string GetAvailableReportFileName(string contract_id, string sub_id, int report_type_id, string ApplicationName, int hours_sincecreation = 72) // return full file path
        {
            DataSet ds;
            string sReportFileName = string.Empty; // return this
            string sReportPath = "";
            int iPartnerID = 0;
            DataRow dr;

            ds = SponsorReportsBO.GetAvailableReports(contract_id, sub_id, report_type_id, ApplicationName, hours_sincecreation);

            if (!(ds == null) && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {

                dr = ds.Tables[0].Rows[0]; // first record is latest report

                if (dr["status_id"].ToString() == BFL.Model.ReportInfo.ReportStatusEnum.Pending.ToString() ||
                   dr["status_id"].ToString() == BFL.Model.ReportInfo.ReportStatusEnum.PSDError.ToString() ||
                   dr["status_id"].ToString() == BFL.Model.ReportInfo.ReportStatusEnum.NoData.ToString())
                {
                    sReportFileName = string.Empty;
                }
                else
                {
                    sReportFileName = Convert.ToString(dr["report_file_name"]);
                    iPartnerID = Convert.ToInt32(dr["partnerId"]);

                    if (iPartnerID == 1)
                    {
                        // PartnerID = "TAE"
                        sReportPath = TRS.IT.TrsAppSettings.AppSettings.GetValue("Path_TAEReports");
                        sReportFileName = System.IO.Path.Combine(sReportPath, sReportFileName);
                    }

                    else if (iPartnerID == 2)
                    {
                    }
                    // PartnerID = "DIA"

                    else if (iPartnerID == 3)
                    {
                        // PartnerID = "PENCO"
                        sReportPath = TRS.IT.TrsAppSettings.AppSettings.GetValue("Path_PencoReports");
                        sReportFileName = System.IO.Path.Combine(sReportPath, sReportFileName);
                    }
                    else if (iPartnerID == 4)
                    {
                    }
                    // PartnerID = "TRS"
                    else if (iPartnerID == 5)
                    {
                        sReportPath = TRS.IT.TrsAppSettings.AppSettings.GetValue("Path_ISCReports");
                        sReportFileName = System.IO.Path.Combine(sReportPath, sReportFileName);
                    }

                }

            }

            return sReportFileName;
        }
        public static void GetContactNamesByType(TRS.IT.SOA.Model.ContractInfo ContractInfo, BFL.Model.E_ContactType eContactType, ref string sNames, ref string sEmailIds, ref string sInloginIds, ref string sIndividualIds)
        {
            // This function is called from CMS-PASS and Backend process
            sNames = "";
            sEmailIds = "";
            sInloginIds = "";
            sIndividualIds = "";
            int iNum;

            if (ContractInfo.PlanContacts.Count > 0)
            {
                int iCnt = 0;

                var loopTo = ContractInfo.PlanContacts.Count - 1;
                for (iNum = 0; iNum <= loopTo; iNum++)
                {
                    var loopTo1 = ContractInfo.PlanContacts[iNum].Type.Count - 1;
                    for (iCnt = 0; iCnt <= loopTo1; iCnt++)
                    {
                        if (!string.IsNullOrEmpty(ContractInfo.PlanContacts[iNum].WebInLoginID) && ContractInfo.PlanContacts[iNum].WebInLoginID != "0" && ContractInfo.PlanContacts[iNum].Type[iCnt] == eContactType)
                        {
                            if (!string.IsNullOrEmpty(sEmailIds)) // return email id even if WebInLoginID is not present
                            {
                                sEmailIds += ";";
                            }
                            sEmailIds += ContractInfo.PlanContacts[iNum].Email;

                            if (!string.IsNullOrEmpty(sNames))
                            {
                                sNames += ";";
                            }

                            if (!string.IsNullOrEmpty(sInloginIds))
                            {
                                sInloginIds += ";";
                            }

                            if (!string.IsNullOrEmpty(sIndividualIds))
                            {
                                sIndividualIds += ";";
                            }

                            sInloginIds += ContractInfo.PlanContacts[iNum].WebInLoginID;
                            sIndividualIds += ContractInfo.PlanContacts[iNum].IndividualID.ToString();

                            if (!string.IsNullOrEmpty(ContractInfo.PlanContacts[iNum].Email))
                            {
                                sNames += GetPlanContactName(ContractInfo.PlanContacts[iNum]);
                            }
                            else
                            {
                                sNames += GetPlanContactName(ContractInfo.PlanContacts[iNum]) + " (No email address on file)";
                            }

                        }
                    }
                }
            }

        }
        public static List<KeyValuePair<string, string>> GetContactEmailsByType(TRS.IT.SOA.Model.ContractInfo ContractInfo, BFL.Model.E_ContactType eContactType)
        {
            // This function is called from CMS-PASS and Backend process
            var emailCollection = new List<KeyValuePair<string, string>>();

            if (ContractInfo.PlanContacts.Count > 0)
            {
                int iCnt = 0;
                int iNum;
                var loopTo = ContractInfo.PlanContacts.Count - 1;
                for (iNum = 0; iNum <= loopTo; iNum++)
                {
                    var loopTo1 = ContractInfo.PlanContacts[iNum].Type.Count - 1;
                    for (iCnt = 0; iCnt <= loopTo1; iCnt++)
                    {
                        if (ContractInfo.PlanContacts[iNum].Type[iCnt] == eContactType && !string.IsNullOrEmpty(ContractInfo.PlanContacts[iNum].WebInLoginID) && ContractInfo.PlanContacts[iNum].WebInLoginID != "0" && !string.IsNullOrEmpty(ContractInfo.PlanContacts[iNum].Email))
                        {
                            emailCollection.Add(new KeyValuePair<string, string>(ContractInfo.PlanContacts[iNum].WebInLoginID, ContractInfo.PlanContacts[iNum].Email));
                        }
                    }
                }
            }
            return emailCollection;
        }
        public static List<KeyValuePair<string, string>> GetTpaContactEmails(TRS.IT.SOA.Model.ContractInfo ContractInfo, BFL.Model.E_TPACompanyContactType eContactType)
        {
            // This function is called from CMS-PASS and Backend process
            var emailCollection = new List<KeyValuePair<string, string>>();

            int i = 0;
            var oBFLTPA = new BFL.Tpa();
            var TPACompanyContactInfo = oBFLTPA.GetContractTPAContacts(ContractInfo.ContractID, ContractInfo.SubID);

            var loopTo = TPACompanyContactInfo.TPAContactInfo.Count - 1;
            for (i = 0; i <= loopTo; i++)
            {
                if (TPACompanyContactInfo.TPAContactInfo[i].ContactType == eContactType && !string.IsNullOrEmpty(TPACompanyContactInfo.TPAContactInfo[i].Web_InLoginId) && TPACompanyContactInfo.TPAContactInfo[i].Web_InLoginId != "0" && !string.IsNullOrEmpty(TPACompanyContactInfo.TPAContactInfo[i].CommunicationInfo.EmailAddress))
                {
                    emailCollection.Add(new KeyValuePair<string, string>(TPACompanyContactInfo.TPAContactInfo[i].Web_InLoginId, TPACompanyContactInfo.TPAContactInfo[i].CommunicationInfo.EmailAddress));
                }
            } // return all
            return emailCollection;
        }
        public static List<KeyValuePair<string, string>> GetAssignedTpaContactEmails(TRS.IT.SOA.Model.ContractInfo ContractInfo, BFL.Model.E_TPAContactType eContactType)
        {
            // This function is called from CMS-PASS and Backend process
            var emailCollection = new List<KeyValuePair<string, string>>();
            if (ContractInfo.TPAContacts.Count > 0)
            {
                foreach (TRS.IT.SOA.Model.TPAContactInformation contact in ContractInfo.TPAContacts)
                {
                    if (contact.ContractContactType == eContactType && !string.IsNullOrEmpty(contact.Web_InLoginId) && contact.Web_InLoginId != "0" && !string.IsNullOrEmpty(contact.CommunicationInfo.EmailAddress))
                    {
                        emailCollection.Add(new KeyValuePair<string, string>(contact.Web_InLoginId, contact.CommunicationInfo.EmailAddress));
                    }
                } // return all
            }
            return emailCollection;
        }
        public static void GetTpaMsgCenterContactName(TRS.IT.SOA.Model.ContractInfo ContractInfo, ref string sNames, ref string sNamesAndTitle, ref string sEmailIds, ref string sInloginIds)
        {
            // This function is called from CMS-PASS and Backend process
            sNames = "";
            sNamesAndTitle = "";
            sInloginIds = "";
            int iNum;

            if (!(ContractInfo.TPAContacts == null) && ContractInfo.TPAContacts.Count > 0)
            {
                var loopTo = ContractInfo.TPAContacts.Count - 1;
                for (iNum = 0; iNum <= loopTo; iNum++)
                {
                    if (ContractInfo.TPAContacts[iNum].ContractContactType == TRS.IT.SI.BusinessFacadeLayer.Model.E_TPAContactType.TPASrPlanAdministrator)
                    {
                        if (!(ContractInfo.TPAContacts[iNum].CommunicationInfo == null) && !string.IsNullOrEmpty(ContractInfo.TPAContacts[iNum].CommunicationInfo.EmailAddress) && !string.IsNullOrEmpty(ContractInfo.TPAContacts[iNum].Web_InLoginId) && ContractInfo.TPAContacts[iNum].Web_InLoginId != "0")
                        {
                            if (!string.IsNullOrEmpty(sEmailIds)) // return email id even if WebInLoginID is not present
                            {
                                sEmailIds += ";";
                            }
                            sEmailIds += ContractInfo.TPAContacts[iNum].CommunicationInfo.EmailAddress;
                            if (!string.IsNullOrEmpty(sNames))
                            {
                                sNames += ";";
                            }
                            if (!string.IsNullOrEmpty(sNamesAndTitle))
                            {
                                sNamesAndTitle += ";";
                            }
                            if (!string.IsNullOrEmpty(sInloginIds))
                            {
                                sInloginIds += ";";
                            }
                            sInloginIds += ContractInfo.TPAContacts[iNum].Web_InLoginId;
                            sNames += GetTPAName(ContractInfo.TPAContacts[iNum]);
                            if (!(ContractInfo.TPAContacts[iNum].CommunicationInfo == null) && !string.IsNullOrEmpty(ContractInfo.TPAContacts[iNum].CommunicationInfo.EmailAddress))
                            {
                                sNamesAndTitle += GetTPAName(ContractInfo.TPAContacts[iNum]) + " - TPA Administrator ";
                            }
                            else
                            {
                                sNamesAndTitle += GetTPAName(ContractInfo.TPAContacts[iNum]) + " - TPA Administrator (No email address on file)";
                            }
                        }


                    }
                } // return all
            }

            var oBFLTPA = new BFL.Tpa();
            TRS.IT.SOA.Model.TPACompanyContactInformations oTPACompanyInfos;
            oTPACompanyInfos = oBFLTPA.GetContractTPAContacts(ContractInfo.ContractID, ContractInfo.SubID);
            oTPACompanyInfos.TPAContactInfo.Sort((x, y) => x.Web_InLoginId.CompareTo(y.Web_InLoginId));
            if (string.IsNullOrEmpty(sEmailIds))
            {
                // No TPA Sr Plan Administrator assigned to contract found , return TPA Sr Plan Administrator  
                if (!(oTPACompanyInfos == null) && !(oTPACompanyInfos.TPAContactInfo == null) && oTPACompanyInfos.TPAContactInfo.Count > 0)
                {
                    var loopTo1 = oTPACompanyInfos.TPAContactInfo.Count - 1;
                    for (iNum = 0; iNum <= loopTo1; iNum++)
                    {
                        if (oTPACompanyInfos.TPAContactInfo[iNum].ContactType == TRS.IT.SI.BusinessFacadeLayer.Model.E_TPACompanyContactType.TPASrPlanAdministrator)
                        {
                            // return email id even if WebInLoginID is not present
                            if (!(oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo == null) && !string.IsNullOrEmpty(oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo.EmailAddress) && !string.IsNullOrEmpty(oTPACompanyInfos.TPAContactInfo[iNum].Web_InLoginId) && oTPACompanyInfos.TPAContactInfo[iNum].Web_InLoginId != "0")
                            {
                                sEmailIds = oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo.EmailAddress;
                                sNames = GetTPAName(oTPACompanyInfos.TPAContactInfo[iNum]);
                                sInloginIds = oTPACompanyInfos.TPAContactInfo[iNum].Web_InLoginId;
                                if (!(oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo == null) && !string.IsNullOrEmpty(oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo.EmailAddress))
                                {
                                    sNamesAndTitle = GetTPAName(oTPACompanyInfos.TPAContactInfo[iNum]) + " - TPA Sr Plan Administrator";
                                }
                                else
                                {
                                    sNamesAndTitle = GetTPAName(oTPACompanyInfos.TPAContactInfo[iNum]) + " - TPA Sr Plan Administrator (No email address on file)";
                                }
                                break; // return only one
                            }

                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(sEmailIds))
            {
                // No TPA Sr Plan Administrator found, return the TPA Owner (only one)
                if (!(oTPACompanyInfos == null) && !(oTPACompanyInfos.TPAContactInfo == null) && oTPACompanyInfos.TPAContactInfo.Count > 0)
                {
                    var loopTo2 = oTPACompanyInfos.TPAContactInfo.Count - 1;
                    for (iNum = 0; iNum <= loopTo2; iNum++)
                    {
                        if (oTPACompanyInfos.TPAContactInfo[iNum].ContactType == TRS.IT.SI.BusinessFacadeLayer.Model.E_TPACompanyContactType.TPAOwner)
                        {
                            // return email id even if WebInLoginID is not present
                            if (!(oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo == null) && !string.IsNullOrEmpty(oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo.EmailAddress) && !string.IsNullOrEmpty(oTPACompanyInfos.TPAContactInfo[iNum].Web_InLoginId) && oTPACompanyInfos.TPAContactInfo[iNum].Web_InLoginId != "0")
                            {
                                sEmailIds = oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo.EmailAddress;
                                sNames = GetTPAName(oTPACompanyInfos.TPAContactInfo[iNum]);
                                sInloginIds = oTPACompanyInfos.TPAContactInfo[iNum].Web_InLoginId;
                                if (!(oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo == null) && !string.IsNullOrEmpty(oTPACompanyInfos.TPAContactInfo[iNum].CommunicationInfo.EmailAddress))
                                {
                                    sNamesAndTitle = GetTPAName(oTPACompanyInfos.TPAContactInfo[iNum]) + " - TPA Owner";
                                }
                                else
                                {
                                    sNamesAndTitle = GetTPAName(oTPACompanyInfos.TPAContactInfo[iNum]) + " - TPA Owner (No email address on file)";
                                }
                                break; // return only one
                            }
                        }
                    }
                }
            }

        }
        private static string GetPlanContactName(TRS.IT.SOA.Model.PlanContactInfo oContact)
        {
            string sName = "";
            if (oContact.FirstName == null)
            {
                oContact.FirstName = "";
            }

            if (oContact.LastName == null)
            {
                oContact.LastName = "";
            }

            sName = oContact.FirstName.Trim() + " " + oContact.LastName.Trim();

            sName = sName.Trim();
            if (string.IsNullOrEmpty(sName))
            {
                sName = "Name not found";
            }

            return sName;
        }
        private static string GetTPAName(TRS.IT.SOA.Model.TPAContactInformation oContact)
        {
            string sName = "";
            if (oContact.FirstName == null)
            {
                oContact.FirstName = "";
            }

            if (oContact.LastName == null)
            {
                oContact.LastName = "";
            }

            sName = oContact.FirstName.Trim() + " " + oContact.LastName.Trim();

            sName = sName.Trim();
            if (string.IsNullOrEmpty(sName))
            {
                sName = "Name not found";
            }

            return sName;
        }
    }
}