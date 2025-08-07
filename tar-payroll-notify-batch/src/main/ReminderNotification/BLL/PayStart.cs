using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using ReminderNotificationBatch.DAL;
using TRS.IT.BendProcessor.DriverSOA;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace ReminderNotificationBatch.BLL
{
    public class PayStart : BendProcessorBase
    {
        string _sGeneralEmailNotification = AppSettings.GetValue("GeneralEmailNotification");
        private const int C_MsgId_ReminderNotice_PlanSponsor = 1700;
        private const int C_NotifyType_ReminderNotice_PlanSponsor = 3;
        private const int C_NotifyType_FollowupNotice_SponsorConnect = 5;
        private PayStartDC _oPDC = new();
        public PayStart() : base("100", "PayStart", "TRS") { }
        public TaskStatus SendReminderNotification()
        {
            TaskStatus oTaskStatus = new();
            DataSet ds;
            MessageService oMS = new();
            ContractServ wsContract = new();
            ResultReturn oResult;
            DataSet dsOptions;
            string subIdFromFile = string.Empty;
            string s5DaysOverDue = AppSettings.GetValue("Payroll5DaysOverDue");

            try
            {
                ds = _oPDC.GetDailyJobRemindersInfo();

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string sContractID = dr["contract_id"].ToString();
                    string sSubID = dr["sub_id"].ToString();
                    int iRowNum = (int)dr["row_no"];
                    DateTime oEndDt = Convert.ToDateTime(dr["payroll_end_dt"].ToString());
                    string sUniqueIdentifier = dr["unique_identifier"].ToString();
                    if (sSubID == "000")
                    {
                        subIdFromFile = GetSubIdFromPayrollFileName(dr["generated_filename"].ToString());
                    }
                    else
                    {
                        subIdFromFile = sSubID;
                    }

                    switch ((int)dr["notify_type"])
                    {
                        case 1: //initial
                            if ((int)dr["day_num"] >= 3)
                            {
                                //send reminder email

                                dsOptions = _oPDC.GetOptInNPassInfo(dr["contract_id"].ToString(), subIdFromFile, false);
                                TRS.IT.SOA.Model.ContractInfo oConInfo = wsContract.GetContractInformation(sContractID, subIdFromFile);

                                oResult = SendSponsorNotification(dr["contract_id"].ToString(), subIdFromFile, dsOptions.Tables[0].Rows[0]["plan_name"].ToString(),
                                    ParseTemplateEmailString(dsOptions.Tables[0].Rows[0]["email"].ToString(), false), C_MsgId_ReminderNotice_PlanSponsor, false, "",
                                    oConInfo.ContractName, Convert.ToDouble(dr["total_amount"]), oEndDt.ToShortDateString(), sUniqueIdentifier);
                                if (oResult.returnStatus == ReturnStatusEnum.Succeeded)
                                {
                                    _oPDC.InsertPayrollNotification(iRowNum, C_NotifyType_ReminderNotice_PlanSponsor, dsOptions.Tables[0].Rows[0]["email"].ToString());
                                }
                                else
                                {
                                    oTaskStatus.errors.Add(new ErrorInfo(oResult.Errors[0].errorNum, oResult.Errors[0].errorDesc, ErrorSeverityEnum.Warning));
                                }
                                //throw new Exception("Unable to send second notice. Error: " + oResult.Errors[0].errorDesc);
                            }
                            break;
                        case 3: //second notice
                            if ((int)dr["day_num"] >= 5)
                            {
                                string sSponsorConnectEmail = AppSettings.GetValue("SponsorConnectEmail");
                                TRS.IT.SOA.Model.ContractInfo oConInfo = wsContract.GetContractInformation(sContractID, subIdFromFile);
                                //send follow-up to sponsorconnect
                                dsOptions = _oPDC.GetOptInNPassInfo(dr["contract_id"].ToString(), subIdFromFile, true);
                                oResult = SendNotificationSponsorConnect(dr["contract_id"].ToString(), subIdFromFile,
                                   dsOptions.Tables[0].Rows[0]["plan_name"].ToString(), ParseTemplateEmailString(dsOptions.Tables[0].Rows[0]["email"].ToString(), true),
                                   sSponsorConnectEmail, oConInfo.ContractName, Convert.ToDouble(dr["total_amount"]), oEndDt.ToShortDateString(), sUniqueIdentifier);

                                if (oResult.returnStatus == ReturnStatusEnum.Succeeded)
                                {
                                    _oPDC.InsertPayrollNotification(iRowNum, C_NotifyType_FollowupNotice_SponsorConnect, sSponsorConnectEmail);
                                }
                                else
                                {
                                    oTaskStatus.errors.Add(new ErrorInfo(oResult.Errors[0].errorNum, oResult.Errors[0].errorDesc, ErrorSeverityEnum.Warning));
                                }
                            }

                            break;
                        case 5: //currently no requirement to send after follow-up email was sent
                            if (s5DaysOverDue == "1")
                            {
                                Utils.SendMail(AppSettings.GetValue(ConstN.C_BPROCESSOR_EMAIL), _sGeneralEmailNotification, "5 days overdue " + dr["contract_id"].ToString(), "");
                            }

                            break;
                    }
                }
                if (oTaskStatus.errors.Count > 0)
                {
                    oTaskStatus.retStatus = TaskRetStatus.Warning;
                }
                else
                {
                    oTaskStatus.retStatus = TaskRetStatus.Succeeded;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oTaskStatus.errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
                oTaskStatus.retStatus = TaskRetStatus.Failed;
                SendErrorEmail(ex);
            }

            return oTaskStatus;
        }

        #region Private Methods
        private string GetSubIdFromPayrollFileName(string fileName)
        {
            Regex regex = new("(?<=-).*?(?=_)");
            Match m = regex.Match(fileName);
            string subIDFromFile = "000";
            if (m.Success)
            {
                subIDFromFile = m.Value;
                if (subIDFromFile.Length < 3)
                {
                    subIDFromFile = "000";
                }
            }
            return subIDFromFile;
        }

        private ResultReturn SendSponsorNotification(string a_sConId, string a_sSubId, string a_sPlanName, string a_sToEmail, int a_iMsgId,
           bool a_bIsMEP, string a_sClientSubId, string a_sCompanyName, double a_dTotalAmount, string a_sPayrolEndDate, string a_sUniqueIdentifier)
        {

            StringBuilder msgToLog = new("Method: SendSponsorNotification() Inputs are, ");
            msgToLog.Append("a_sConId: " + a_sConId
            + ", a_sSubId: " + a_sSubId
            + ", a_sPlanName: " + a_sPlanName
            + ", a_sToEmail: " + a_sToEmail
            + ", a_iMsgId: " + Convert.ToString(a_iMsgId)
            + ", a_bIsMEP: " + Convert.ToString(a_bIsMEP)
            + ", a_sClientSubId: " + a_sClientSubId
            + ", a_sCompanyName: " + a_sCompanyName
            + ", a_dTotalAmount: " + Convert.ToString(a_dTotalAmount)
            + ", a_sPayrolEndDate: " + a_sPayrolEndDate
            + ", a_sUniqueIdentifier: " + a_sUniqueIdentifier);
            MessageServiceKeyValue[] Keys;
            MessageService oMS = new();
            ResultReturn oResults;
            MessageTemplateContact oSupportContact;
            Utils.LogInfo(a_sUniqueIdentifier);
            oSupportContact = _oPDC.GetClientSupportContact(a_sConId, a_sSubId);

            Keys = new MessageServiceKeyValue[11];

            Keys[0] = new MessageServiceKeyValue();
            Keys[0].key = "contract_number";
            Keys[0].value = a_sConId;

            Keys[1] = new MessageServiceKeyValue();
            Keys[1].key = "sub_code";
            Keys[1].value = a_sSubId;

            //passin value
            Keys[2] = new MessageServiceKeyValue();
            Keys[2].key = "to_email";
            Keys[2].value = a_sToEmail;

            Keys[3] = new MessageServiceKeyValue();
            Keys[3].key = "contact_name";
            Keys[3].value = oSupportContact.name;
            msgToLog.Append(", contact_name: " + oSupportContact.name);

            Keys[4] = new MessageServiceKeyValue();
            Keys[4].key = "contact_number";
            Keys[4].value = oSupportContact.phone;
            msgToLog.Append(", contact_number: " + oSupportContact.phone);

            Keys[5] = new MessageServiceKeyValue();
            Keys[5].key = "plan_name";
            Keys[5].value = a_sPlanName;

            Keys[6] = new MessageServiceKeyValue();
            Keys[6].key = "company_name";
            Keys[6].value = a_sCompanyName;

            Keys[7] = new MessageServiceKeyValue();
            Keys[7].key = "payroll_date";
            Keys[7].value = a_sPayrolEndDate;

            Keys[8] = new MessageServiceKeyValue();
            Keys[8].key = "payroll_amount";
            Keys[8].value = a_dTotalAmount.ToString("C");

            Keys[9] = new MessageServiceKeyValue();
            Keys[9].key = "Unique_Identifier";

            if (!String.IsNullOrEmpty(a_sUniqueIdentifier))
            {
                Keys[9].value = "Unique-ID: " + a_sUniqueIdentifier;
            }
            else
            {
                Keys[9].value = a_sUniqueIdentifier;
            }

            Keys[10] = new MessageServiceKeyValue();
            Keys[10].key = "Unique_Identifier_Header";
            Keys[10].value = a_sUniqueIdentifier;
            Utils.LogInfo(msgToLog.ToString());
            oResults = oMS.SendPayrollNotification(a_sConId, a_sSubId, a_iMsgId, Keys);
            return oResults;
        }

        private string ParseTemplateEmailString(string a_sData, bool a_bReturnName)
        {
            if (a_sData == string.Empty)
            {
                return string.Empty;
            }

            StringBuilder strB = new();
            string[] Contacts = a_sData.Split(';');
            foreach (string contact in Contacts)
            {
                string[] sVal = contact.Split('|');
                if (sVal.Length > 0)
                {
                    if (a_bReturnName)
                    {
                        if (strB.Length > 1)
                        {
                            strB.Append(";" + sVal[1]);
                        }
                        else
                        {
                            strB.Append(sVal[1]);
                        }
                    }
                    else
                    {
                        if (strB.Length > 1)
                        {
                            strB.Append(";" + sVal[0]);
                        }
                        else
                        {
                            strB.Append(sVal[0]);
                        }
                    }
                }
            }
            return strB.ToString();
        }

        private ResultReturn SendNotificationSponsorConnect(string a_sConId, string a_sSubId, string a_sPlanName, string a_sContacts,
           string a_sSponsorConnectEmail, string a_sCompanyName, double a_dTotalAmount, string a_sPayrolEndDate, string a_sUniqueIdentifier)
        {
            string msgToLog = "SendNotificationSponsorConnect(), Inputs are a_sConId: " + a_sConId;
            msgToLog = msgToLog + ", a_sSubId: " + a_sSubId + ", a_sPlanName: " + a_sPlanName + ", a_sContacts: " + a_sContacts;
            msgToLog = msgToLog + ", a_sSponsorConnectEmail: " + a_sSponsorConnectEmail + ", a_sCompanyName: " + a_sCompanyName;
            msgToLog = msgToLog + ", a_dTotalAmount: " + Convert.ToString(a_dTotalAmount) + ", a_sPayrolEndDate: " + Convert.ToString(a_sPayrolEndDate);
            msgToLog = msgToLog + ", a_sUniqueIdentifier: " + a_sUniqueIdentifier;
            Utils.LogInfo(msgToLog);
            MessageServiceKeyValue[] Keys;
            MessageService oMS = new();
            ResultReturn oResults = null;
            const int C_MsgId_FollowupNotice_SponsorConnect = 1710;
            Keys = new MessageServiceKeyValue[10];

            Keys[0] = new MessageServiceKeyValue();
            Keys[0].key = "contract_number";
            Keys[0].value = a_sConId;

            Keys[1] = new MessageServiceKeyValue();
            Keys[1].key = "sub_code";
            Keys[1].value = a_sSubId;

            Keys[2] = new MessageServiceKeyValue();
            Keys[2].key = "Contact_Names";
            Keys[2].value = a_sContacts;

            Keys[3] = new MessageServiceKeyValue();
            Keys[3].key = "sponsor_connect";
            Keys[3].value = a_sSponsorConnectEmail; // AppSettings.GetValue("SponsorConnectEmail");

            Keys[4] = new MessageServiceKeyValue();
            Keys[4].key = "plan_name";
            Keys[4].value = a_sPlanName;

            Keys[5] = new MessageServiceKeyValue();
            Keys[5].key = "company_name";
            Keys[5].value = a_sCompanyName;

            Keys[6] = new MessageServiceKeyValue();
            Keys[6].key = "payroll_date";
            Keys[6].value = a_sPayrolEndDate;

            Keys[7] = new MessageServiceKeyValue();
            Keys[7].key = "payroll_amount";
            Keys[7].value = a_dTotalAmount.ToString("C");

            Keys[8] = new MessageServiceKeyValue();
            Keys[8].key = "Unique_Identifier";

            if (!String.IsNullOrEmpty(a_sUniqueIdentifier))
            {
                Keys[8].value = "Unique-ID: " + a_sUniqueIdentifier;
            }
            else
            {
                Keys[8].value = a_sUniqueIdentifier;
            }

            Keys[9] = new MessageServiceKeyValue();
            Keys[9].key = "Unique_Identifier_Header";
            Keys[9].value = a_sUniqueIdentifier;
            Utils.LogInfo("before oMS.SendPayrollNotification()");
            oResults = oMS.SendPayrollNotification(a_sConId, a_sSubId, C_MsgId_FollowupNotice_SponsorConnect, Keys);

            if (oResults == null)
            {
                ErrorInfo oError = new();
                oResults = new ResultReturn();
                oError.errorNum = 1;
                oError.errorDesc = "No result returned.";
                oResults.Errors.Add(oError);
                Utils.LogInfo("after oMS.SendPayrollNotification() returns error No result returned.");
            }
            Utils.LogInfo("after oMS.SendPayrollNotification() returns, no error.");
            return oResults;
        }
        
        #endregion
    }
}
