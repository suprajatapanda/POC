using System.Collections;
using System.Data;
using System.Text;
using System.Xml.Linq;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.DriverSOA;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.SI.BusinessFacadeLayer.Model;
using BFL = TRS.IT.SI.BusinessFacadeLayer;
using ErrorInfo = TRS.IT.BendProcessor.Model.ErrorInfo;
using SOAModel = TRS.IT.SOA.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace TRS.IT.BendProcessor.BLL
{
    public class ConsolidatedNotifications : BendProcessorBase
    {
        private ConsolidatedNotificationsDC _oConsolidatedNotificationsDC;

        public ConsolidatedNotifications() : base("99", "ConsolidatedNotifications", "TRS") { _oConsolidatedNotificationsDC = new ConsolidatedNotificationsDC(); }

        private const int C_INPUTDETAILS_RESULT_ERROR = -1;
        private const int C_INPUTDETAILS_RESULT_COMPLETE = 100;
        private const int C_MSGQUEUE_RESULT_ERROR = -1;
        private const int C_MSGQUEUE_RESULT_COMPLETE = 100;

        #region ***ProcessNotificationInputDetails ****
        public TaskStatus ProcessNotificationInputDetailsMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;

            const string C_Task = "ProcessNotificationInputDetails";

            DataSet dsInputDetails = new();
            DataSet dsContacts = null;
            int row_no;
            int iDocType_id;
            Hashtable htDocType = new();
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (TrsAppSettings.AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    oTaskReturn.retStatus = TaskRetStatus.Succeeded;


                    dsInputDetails = _oConsolidatedNotificationsDC.GetALLInputDetails();

                    if (dsInputDetails != null && dsInputDetails.Tables.Count > 0)
                    {
                        foreach (DataRow dr in dsInputDetails.Tables[0].Rows)
                        {
                            row_no = 0; iDocType_id = 0;

                            row_no = Convert.ToInt32(dr["row_no"].ToString());
                            iDocType_id = Convert.ToInt32(dr["DocType_id"].ToString());

                            if (htDocType.ContainsKey(iDocType_id.ToString()))
                            {
                                dsContacts = (DataSet)htDocType[iDocType_id.ToString()];
                            }
                            else
                            {
                                dsContacts = _oConsolidatedNotificationsDC.GetMsgTemplatesAndContactDetails(iDocType_id);
                                htDocType.Add(iDocType_id.ToString(), dsContacts);
                            }


                            //if(iDocType_id >= 9999 && iDocType_id != 10000) // well 10000 needs to be Saved  at SaveToIndividualMessageQueue
                            if (IsCustomContactEnabled(iDocType_id))
                            {
                                oReturn = SaveToContractMessageQueue(dr, dsContacts); // one message per contract

                                if (iDocType_id == 36500 || iDocType_id == 36510 || iDocType_id == 36520) // custom contacts are enabled only for sponsor email so for TPA email we need to save in Individial Msg q too.
                                {
                                    oReturn = SaveToIndividualMessageQueue(dr, dsContacts);// one message per person/individual
                                }
                            }
                            else
                            {
                                oReturn = SaveToIndividualMessageQueue(dr, dsContacts); // one message per person/individual
                            }

                            if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                            {
                                General.CopyResultError(oTaskReturn, oReturn);
                                oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                            }
                            oTaskReturn.rowsCount += oReturn.rowsCount;

                        }
                    }
                    StringBuilder sbErr = new();
                    if (oTaskReturn.retStatus != TaskRetStatus.Succeeded || oTaskReturn.rowsCount > 0)
                    {
                        //send error
                        foreach (ErrorInfo oEr in oTaskReturn.errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("ProcessInputDetails Status - " + oTaskReturn.retStatus.ToString(), sbErr.ToString(), "Debug");
                    }

                    //------------------------------Send Notifications from Message queue----------------------------------------------
                    DataSet dsIndiv = new();
                    DataSet dsContr = new();
                    DataSet dsDocGrp = new();
                    ResultReturn oReturn3;
                    ResultReturn oReturn4;
                    ResultReturn oReturn5;

                    dsIndiv = _oConsolidatedNotificationsDC.GetConsolidatedMessageQueue();

                    oReturn3 = ProcessPendingIndividualMessageQueue(dsIndiv);

                    if (oReturn3.returnStatus != ReturnStatusEnum.Succeeded || oReturn3.Errors.Count > 0)
                    {
                        sbErr.Length = 0;
                        //send error
                        foreach (ErrorInfo oEr in oReturn3.Errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("ProcessPendingIndividualMessageQueue Status - " + oReturn3.returnStatus.ToString(), sbErr.ToString(), "Debug");

                        General.CopyResultError(oTaskReturn, oReturn3);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.rowsCount += oReturn3.rowsCount;


                    dsContr = _oConsolidatedNotificationsDC.GetConsolidatedContractMessageQueue();

                    oReturn4 = ProcessPendingContractMessageQueue(dsContr);

                    if (oReturn4.returnStatus != ReturnStatusEnum.Succeeded || oReturn4.Errors.Count > 0)
                    {
                        sbErr.Length = 0;
                        foreach (ErrorInfo oEr in oReturn4.Errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("ProcessPendingContractMessageQueue Status - " + oReturn4.returnStatus.ToString(), sbErr.ToString(), "Debug");

                        General.CopyResultError(oTaskReturn, oReturn4);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.rowsCount += oReturn4.rowsCount;


                    dsDocGrp = _oConsolidatedNotificationsDC.GetConsolidateDocGroupdMessageQueue();



                    oReturn5 = ProcessPendingDocGroupMessageQueue(dsDocGrp);

                    if (oReturn5.returnStatus != ReturnStatusEnum.Succeeded || oReturn5.Errors.Count > 0)
                    {
                        sbErr.Length = 0;
                        foreach (ErrorInfo oEr in oReturn5.Errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("ProcessPendingDocGroupMessageQueue Status - " + oReturn5.returnStatus.ToString(), sbErr.ToString(), "Debug");

                        General.CopyResultError(oTaskReturn, oReturn5);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.rowsCount += oReturn5.rowsCount;

                    //------------------------------End Send Notifications from Message queue----------------------------------------------
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        private string GetReqdNoticeFeedISC(string sCid, string sSId, string sSId_orig, string planName, string wmsDocFormat, string subNotificationType)
        {
            string feed = "5";
            string fileName = "";
            if (wmsDocFormat != "")
            {
                string[] values = wmsDocFormat.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                fileName = "," + values[1].Replace("name=", "").ToString().Replace("\"", "").ToString().Replace("tif", "pdf");
            }
            //feed += "," + sCid + "," + sSId + ",http://www.ta-retirement.com,(888) 637-8726,\"" + planName + "\"," + subNotificationType + fileName;
            feed += "," + sCid + "," + sSId + ",http://www.ta-retirement.com,(800) 401-8726,\"" + planName + "\"," + subNotificationType + fileName; //IT-88135: 800-401-8726 should display for both GAC and NAV (EM plans).
            feed += @",https://www.ta-retirement.com/Employee/Secure/pa_documentcenter.aspx?cid=" + sCid + "&sid=" + sSId_orig; //IT-84035
            return feed;
        }
        private Boolean CheckAndSubmitIMReqdNotice(string sCid, string sSId, string planName, string sInput_params, string PartnerID)
        {
            int iDocCode = 0;
            string feed;
            int subNotificationType = 0;
            string fileName = string.Empty;
            string wmsDocFormat = string.Empty;
            Boolean bSuccess = false;
            string sSId_orig = string.Empty;
            if (PartnerID == "1300" || PartnerID == "ISC")
            {
                try
                {
                    SOAModel.WsDocumentServiceDocumentEx oInputDoc;
                    oInputDoc = (SOAModel.WsDocumentServiceDocumentEx)TRSManagers.XMLManager.DeserializeXml(sInput_params, typeof(SOAModel.WsDocumentServiceDocumentEx));
                    if (oInputDoc != null)
                    {
                        string FileName = oInputDoc.DocFormat;

                        iDocCode = oInputDoc.DocTypeCode;
                        if (oInputDoc.TrackNumber.Trim() == "")
                        {
                            FileName = "";
                        }
                        sSId = BFL.Util.SubOut(sSId);
                        sSId_orig = oInputDoc.SubID;
                        if (sSId_orig != null && sSId_orig != "")
                        {
                            sSId_orig = BFL.Util.SubOut(sSId_orig);
                            sSId = sSId_orig;
                        }
                        if (PartnerID == "1300" || PartnerID == "ISC")
                        {
                            switch (iDocCode)
                            {
                                case 695: //SAR
                                    subNotificationType = 5;
                                    feed = GetReqdNoticeFeedISC(sCid, sSId, sSId_orig, planName, FileName, subNotificationType.ToString());
                                    _oConsolidatedNotificationsDC.InsertTmpDIAReqdNoticesFeedDaily(sCid, sSId, 5, subNotificationType, feed);
                                    bSuccess = true;
                                    break;
                                case 696: //SMM
                                    subNotificationType = 6;
                                    feed = GetReqdNoticeFeedISC(sCid, sSId, sSId_orig, planName, FileName, subNotificationType.ToString());
                                    _oConsolidatedNotificationsDC.InsertTmpDIAReqdNoticesFeedDaily(sCid, sSId, 5, subNotificationType, feed);
                                    bSuccess = true;
                                    break;
                                case 161: //SPD
                                    subNotificationType = 7;
                                    feed = GetReqdNoticeFeedISC(sCid, sSId, sSId_orig, planName, FileName, subNotificationType.ToString());
                                    _oConsolidatedNotificationsDC.InsertTmpDIAReqdNoticesFeedDaily(sCid, sSId, 5, subNotificationType, feed);
                                    bSuccess = true;
                                    break;
                                case 746:  //404
                                case 689:  //404
                                    subNotificationType = 8;
                                    feed = GetReqdNoticeFeedISC(sCid, sSId, sSId_orig, planName, FileName, subNotificationType.ToString());
                                    _oConsolidatedNotificationsDC.InsertTmpDIAReqdNoticesFeedDaily(sCid, sSId, 5, subNotificationType, feed);
                                    bSuccess = true;
                                    break;
                                case 653: //APN             
                                    subNotificationType = 4;
                                    feed = GetReqdNoticeFeedISC(sCid, sSId, sSId_orig, planName, FileName, "4");
                                    _oConsolidatedNotificationsDC.InsertTmpDIAReqdNoticesFeedDaily(sCid, sSId, 5, subNotificationType, feed);
                                    bSuccess = true;
                                    break;
                                default:
                                    bSuccess = true;
                                    break;
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    bSuccess = false;
                }
            }
            else
            {
                bSuccess = true;
            }

            return bSuccess;
        }
        private ResultReturn SaveToContractMessageQueue(DataRow drInPutDetails, DataSet dsContacts) //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            string sCid = "";
            string sSId = "";
            int row_no;
            int iDocType_id = 0;
            string sInput_params = string.Empty;
            string sData_To_Consolidate = string.Empty;
            string sMessage_Variables = string.Empty;
            string sPartnerUserId = string.Empty;
            string sLoginType = string.Empty;
            string PartnerID = string.Empty;
            string semail_id = string.Empty;
            int iMsg_Template_id;
            int iMsgCtr_Template_id;
            int iDelayDays;
            DateTime dtSendDate;
            DataSet ds = null;
            int iCustom_Recipient_Type = 0;
            string sSId_custom = "";
            bool bSuccess = false;
            bool bIsCustomContactEnabled = true;
            bool bSendEmail = true;
            string planName = string.Empty;
            SOAModel.ContractInfo oConInfo;
            string sep_mep_text = "";
            if (drInPutDetails != null)
            {
                row_no = Convert.ToInt32(drInPutDetails["row_no"].ToString());
                try
                {
                    if (drInPutDetails["contract_id"] != null)
                    {
                        sCid = drInPutDetails["contract_id"].ToString().Trim();
                    }

                    if (drInPutDetails["sub_id"] != null)
                    {
                        sSId = drInPutDetails["sub_id"].ToString().Trim();
                    }

                    if (drInPutDetails["DocType_id"] != null)
                    {
                        iDocType_id = Convert.ToInt32(drInPutDetails["DocType_id"].ToString());
                    }

                    if (drInPutDetails["Input_params"] != null)
                    {
                        sInput_params = drInPutDetails["Input_params"].ToString();
                    }

                    if (drInPutDetails["Data_To_Consolidate"] != null)
                    {
                        sData_To_Consolidate = drInPutDetails["Data_To_Consolidate"].ToString();
                    }

                    if (drInPutDetails["Message_Variables"] != null)
                    {
                        sMessage_Variables = drInPutDetails["Message_Variables"].ToString();
                    }

                    if (drInPutDetails["PartnerID"] != null)
                    {
                        PartnerID = drInPutDetails["PartnerID"].ToString();
                    }

                    if (drInPutDetails["plan_name"] != null)
                    {
                        planName = drInPutDetails["plan_name"].ToString();
                    }

                    bIsCustomContactEnabled = IsCustomContactEnabled(iDocType_id);

                    if (PartnerID == "1300" || PartnerID == "ISC")
                    {
                        bSuccess = CheckAndSubmitIMReqdNotice(sCid, sSId, planName, sInput_params, PartnerID);
                    }

                    oConInfo = GetContractInfoFromSRV(sCid, sSId);

                    if ((oConInfo != null) && oConInfo.FlagValues.isMEP == true)
                    {
                        sep_mep_text = @"<p></p>";
                    }
                    else
                    {
                        sep_mep_text = @"<p>If you have any questions regarding your reports or have trouble accessing them, please contact a Transamerica Retirement Solutions representative at 866-498-4557, 9 AM to 8 PM Eastern Time, Monday through Friday.</p>";
                    }

                    if (dsContacts != null && dsContacts.Tables.Count > 0)
                    {
                        foreach (DataRow drCon in dsContacts.Tables[0].Rows)
                        {
                            try
                            {
                                semail_id = ""; iMsg_Template_id = 0; iMsgCtr_Template_id = 0; bSuccess = false; bSendEmail = true;

                                sLoginType = drCon["LoginType"].ToString();
                                iDelayDays = Convert.ToInt32(drCon["delay_days"].ToString());
                                if (iDelayDays > 0)
                                {
                                    dtSendDate = DateTime.Today.AddDays(iDelayDays);
                                }
                                else
                                {
                                    dtSendDate = DateTime.Now.AddMinutes(-5).AddDays(iDelayDays);
                                }

                                if (drCon["Msg_Template_id"] != null)
                                {
                                    iMsg_Template_id = Convert.ToInt32(drCon["Msg_Template_id"].ToString());
                                }

                                if (drCon["MsgCtr_Template_id"] != null)
                                {
                                    iMsgCtr_Template_id = Convert.ToInt32(drCon["MsgCtr_Template_id"].ToString());
                                }

                                iCustom_Recipient_Type = 0; sSId_custom = "";
                                //if (iDocType_id == 9999 || iDocType_id == 10010) // dont touch any other doctypes for now
                                if (bIsCustomContactEnabled)
                                {
                                    if (sLoginType == "SPONSOR") // note: other login types are not implemented
                                    {
                                        //------------------------------------------------------------------------------------------------------------------------
                                        if ((iDocType_id == 36500 || iDocType_id == 36510 || iDocType_id == 36520) && sLoginType == "SPONSOR")
                                        {
                                            sMessage_Variables = AddMessageVariable(sMessage_Variables, "sep_mep_text", sep_mep_text);
                                        }

                                        ds = _oConsolidatedNotificationsDC.GetCustomContactDetails(sCid, sSId, iDocType_id, sLoginType);
                                        if (ds != null && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0]["Custom_Recipient_Type"] != null)
                                        {
                                            iCustom_Recipient_Type = Convert.ToInt32(ds.Tables[0].Rows[0]["Custom_Recipient_Type"].ToString());
                                            if (ds.Tables[0].Rows[0]["sub_id"] != null)
                                            {
                                                sSId_custom = ds.Tables[0].Rows[0]["sub_id"].ToString().Trim();
                                            }
                                        }

                                        if (sSId != "000" && sSId_custom == "000" && (iCustom_Recipient_Type == 0 || iCustom_Recipient_Type == 1 || iCustom_Recipient_Type == 3))  //We want to send a only one consolidated email for  000 level contacts so insert another row at "000" level for Custom_Recipient_Type 1 and 3 when sub_id != 000
                                        {
                                            _oConsolidatedNotificationsDC.InsertContractMessageQueue(sCid, "000", sLoginType, semail_id, sData_To_Consolidate,
                                                               sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, iDocType_id, sInput_params, PartnerID);

                                            if (iCustom_Recipient_Type == 3)// insert 2 records
                                            {
                                                _oConsolidatedNotificationsDC.InsertContractMessageQueue(sCid, sSId, sLoginType, semail_id, sData_To_Consolidate,
                                                               sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, iDocType_id, sInput_params, PartnerID);
                                            }

                                            bSuccess = true;
                                        }
                                        else
                                        {
                                            if (iCustom_Recipient_Type == 4 && sSId == "000")
                                            {
                                                // dont send email at 000 level when iCustom_Recipient_Type = 4 (Adopting Employer Contact)
                                            }
                                            else
                                            {
                                                _oConsolidatedNotificationsDC.InsertContractMessageQueue(sCid, sSId, sLoginType, semail_id, sData_To_Consolidate,
                                                                   sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, iDocType_id, sInput_params, PartnerID);
                                            }
                                            bSuccess = true;
                                        }
                                        //------------------------------------------------------------------------------------------------------------------------
                                    }
                                    else if (sLoginType.ToUpper() == "TPA")// for tpa email/msg
                                    {
                                        if ((oConInfo.ServiceType != "300") || (iDocType_id == 678 && GetKeyValue("TPAInvolveLoan", oConInfo.KeyValuePairs) != "1")) //TPA  involved in loan?
                                        {
                                            bSendEmail = false;
                                        }

                                        if (bSendEmail)
                                        {
                                            _oConsolidatedNotificationsDC.InsertContractMessageQueue(sCid, sSId, sLoginType, semail_id, sData_To_Consolidate,
                                                                                                                sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, iDocType_id, sInput_params, PartnerID);
                                        }

                                        bSuccess = true; // do not log the unnecessary error for other login types
                                    }
                                    else
                                    {
                                        bSuccess = true; // do not log the unnecessary error for other login types
                                    }
                                }
                                else
                                {
                                    _oConsolidatedNotificationsDC.InsertContractMessageQueue(sCid, sSId, sLoginType, semail_id, sData_To_Consolidate,
                                                            sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, iDocType_id, sInput_params, PartnerID);

                                    bSuccess = true;
                                }
                            }
                            catch (Exception exI)
                            {
                                Utils.LogError(exI);
                                oReturn.returnStatus = ReturnStatusEnum.Unknown;
                                oReturn.isException = true;
                                oReturn.confirmationNo = string.Empty;
                                oReturn.Errors.Add(new ErrorInfo(-1, "row_no = " + row_no.ToString() + " DocTypeId = " + iDocType_id.ToString() + " - " + " Error: " + exI.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
                            }
                        }
                        if (bSuccess == true)
                        {
                            _oConsolidatedNotificationsDC.UpdateInputDetailsStatus(row_no, C_INPUTDETAILS_RESULT_COMPLETE);
                        }
                        else
                        {
                            _oConsolidatedNotificationsDC.UpdateInputDetailsStatus(row_no, C_INPUTDETAILS_RESULT_ERROR);
                        }
                    }
                    else
                    {
                        throw new Exception(" DocTypeId = " + iDocType_id.ToString() + " : Contact details info NOT found in nt_ContactDetails table for contract_id-sub_id = " + sCid + "-" + sSId);
                    }

                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    _oConsolidatedNotificationsDC.UpdateInputDetailsStatus(row_no, C_INPUTDETAILS_RESULT_ERROR);

                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.isException = true;
                    oReturn.confirmationNo = string.Empty;
                    oReturn.Errors.Add(new ErrorInfo(-1, "row_no = " + row_no.ToString() + " - " + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
                }
            }

            return oReturn;
        }
        private ResultReturn SaveToIndividualMessageQueue(DataRow drInPutDetails, DataSet dsContacts) //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            GeneralDC oGenDC = new();

            SOAModel.ContractInfo oConInfo;
            //SOAModel.PlanContactInfo oCt = null;
            List<SOAModel.PlanContactInfo> oCt;
            List<SOAModel.TPAContactInformation> oTpaCt = null;
            SOAModel.TPACompanyContactInformations oTPACompanyInfos;

            string sCid = "";
            string sSId = "";
            string sSId_orig = "";
            int row_no = 0;
            int iDocType_id = 0;
            string sInput_params = string.Empty;
            string splan_name = string.Empty;
            string sMessage_Variables = string.Empty;
            string sPartnerUserId = string.Empty;
            string sLoginType = string.Empty;
            int individual_id;
            string semail_id = string.Empty;
            int iMsg_Template_id;
            int iMsgCtr_Template_id;
            DateTime dtSendDate;
            DateTime dtSendDate_OverRide;
            string PartnerID = string.Empty;

            int iDelayDays;
            bool bSuccess = false;
            bool bSendMail = true;
            bool bMultipleDocTypesGrouped = false;
            string sDocType_Description = string.Empty;
            string strTmpDay = string.Empty;
            string planName = string.Empty;
            int subNotificationType = 0;
            string feed = "";
            // sError = "";
            bSuccess = false;
            bool bSkip = false;
            if (drInPutDetails != null)
            {
                row_no = Convert.ToInt32(drInPutDetails["row_no"].ToString());
                try
                {
                    row_no = Convert.ToInt32(drInPutDetails["row_no"].ToString());
                    if (drInPutDetails["contract_id"] != null)
                    {
                        sCid = drInPutDetails["contract_id"].ToString();
                    }

                    if (drInPutDetails["sub_id"] != null)
                    {
                        sSId_orig = drInPutDetails["sub_id"].ToString();
                    }

                    sSId = sSId_orig;
                    if (drInPutDetails["DocType_id"] != null)
                    {
                        iDocType_id = Convert.ToInt32(drInPutDetails["DocType_id"].ToString());
                    }

                    if (drInPutDetails["Input_params"] != null)
                    {
                        sInput_params = drInPutDetails["Input_params"].ToString();
                    }

                    if (drInPutDetails["Message_Variables"] != null)
                    {
                        sMessage_Variables = drInPutDetails["Message_Variables"].ToString();
                    }

                    if (drInPutDetails["PartnerID"] != null)
                    {
                        PartnerID = drInPutDetails["PartnerID"].ToString();
                    }

                    if (drInPutDetails["plan_name"] != null)
                    {
                        planName = drInPutDetails["plan_name"].ToString();
                    }

                    if (PartnerID == "1300" || PartnerID == "ISC")
                    {
                        bSuccess = CheckAndSubmitIMReqdNotice(sCid, sSId, planName, sInput_params, PartnerID);
                    }

                    oConInfo = GetContractInfoFromSRV(sCid, sSId);

                    if ((oConInfo == null))
                    {
                        throw new Exception("row_no = " + row_no.ToString() + ": No contract info found for contract_id-sub_id = " + sCid + "-" + sSId);
                    }

                    splan_name = oConInfo.PlanName;

                    switch (iDocType_id)
                    {
                        case 695: //SAR
                        case 696: //SMM
                        case 161: //SPD
                                  //case 653: //Participant Notice for future

                            if (oConInfo.FlagValues.isMEP == true)
                            {
                                splan_name = "[" + sCid + "-" + sSId + "] : " + oConInfo.ContractName;
                            }
                            else
                            {
                                splan_name = oConInfo.PlanName;
                            }

                            //We have to send the emails to contacts at "000" level for SAR, SMMM and SPD (and for 653-Participant Notice in future)
                            if (sSId != "000")
                            {
                                sSId = "000";
                                oConInfo = GetContractInfoFromSRV(sCid, sSId);

                                if ((oConInfo == null))
                                {
                                    throw new Exception("row_no = " + row_no.ToString() + ": No contract info found for contract_id-sub_id = " + sCid + "-" + sSId + "<BR />");
                                }
                            }

                            break;

                        case 36500: // Late loan letter
                        case 36510:
                        case 36520:
                            if (oConInfo.FlagValues.isMEP == true)
                            {
                                //splan_name = "[" + sCid + "-" + sSId + "] " + oConInfo.ContractName ; 
                                splan_name = sCid + "-" + sSId + "</td><td> " + oConInfo.ContractName; //  (IT-87083)
                            }
                            else
                            {
                                //splan_name = "[" + sCid + "-" + sSId + "] " + oConInfo.PlanName; 
                                splan_name = sCid + "-" + sSId + "</td><td> " + oConInfo.PlanName; // (IT-87083)
                            }
                            break;

                        default:
                            splan_name = oConInfo.PlanName;
                            break;
                    }

                    if (dsContacts != null && dsContacts.Tables.Count > 0 && dsContacts.Tables[0].Rows.Count > 0)
                    {
                        //if (dsContacts.Tables[0].Rows.Count == 0)
                        //{
                        //    throw new Exception("Contact details are not set up for the supplied DocType_id: " + iDocType_id.ToString());
                        //}

                        foreach (DataRow drCon in dsContacts.Tables[0].Rows)
                        {
                            try
                            {
                                oCt = null; oTpaCt = null; individual_id = 0; semail_id = ""; iMsg_Template_id = 0; iMsgCtr_Template_id = 0; sMessage_Variables = ""; bMultipleDocTypesGrouped = false; sDocType_Description = ""; strTmpDay = string.Empty; bSkip = false;

                                bMultipleDocTypesGrouped = Convert.ToBoolean(drCon["MultipleDocTypesGrouped"]);
                                if (drCon["DocType_Description"] != null)
                                {
                                    sDocType_Description = drCon["DocType_Description"].ToString();
                                }

                                sLoginType = drCon["LoginType"].ToString();

                                iMsg_Template_id = Convert.ToInt32(drCon["Msg_Template_id"].ToString());
                                iMsgCtr_Template_id = Convert.ToInt32(drCon["MsgCtr_Template_id"].ToString());

                                if (IsCustomContactEnabled(iDocType_id) && sLoginType == "SPONSOR")
                                {
                                    bSkip = true;
                                    bSuccess = true;
                                }

                                if (bSkip == false)
                                {
                                    iDelayDays = Convert.ToInt32(drCon["delay_days"].ToString());
                                    if (iDelayDays > 0)
                                    {
                                        dtSendDate = DateTime.Today.AddDays(iDelayDays);
                                    }
                                    else
                                    {
                                        dtSendDate = DateTime.Now.AddMinutes(-5).AddDays(iDelayDays);
                                    }

                                    //If we want to send emails only on specific date of the month...
                                    strTmpDay = TrsAppSettings.AppSettings.GetValue("OverRideSendDate_" + iDocType_id.ToString() + "_" + iMsg_Template_id.ToString());
                                    if (strTmpDay != string.Empty && strTmpDay != null)
                                    {
                                        dtSendDate_OverRide = new DateTime(dtSendDate.Year, dtSendDate.Month, Convert.ToInt32(strTmpDay));

                                        if (dtSendDate.Subtract(dtSendDate_OverRide).Days > 0)
                                        {
                                            dtSendDate_OverRide = dtSendDate_OverRide.AddMonths(1);
                                        }

                                        dtSendDate = dtSendDate_OverRide;

                                        if (iDelayDays > 0)
                                        {
                                            dtSendDate = dtSendDate.AddDays(iDelayDays);
                                        }
                                    }


                                    if (sLoginType.ToUpper() == "TPA" && oConInfo.ServiceType == "300")
                                    {
                                        bSendMail = true;

                                        if (iDocType_id == 678) // Loan Pay off
                                        {
                                            if (GetKeyValue("TPAInvolveLoan", oConInfo.KeyValuePairs) != "1") //TPA Not involved in loan?
                                            {
                                                bSendMail = false; // no need to send TPA email
                                                bSuccess = true;
                                            }
                                        }

                                        if (bSendMail)
                                        {
                                            oTPACompanyInfos = GetTPAContractContactInfoFromSRV(sCid, sSId);
                                            oTpaCt = GetTPAContactsByContactType(oTPACompanyInfos, (E_TPACompanyContactType)Convert.ToInt32(drCon["contact_type"].ToString()));

                                            if ((iDocType_id == 678) && (oTpaCt == null || oTpaCt.Count == 0))
                                            {
                                                //1. try to send Sr. Plan Administrator  
                                                oTpaCt = GetTPAContactsByContactType(oTPACompanyInfos, E_TPACompanyContactType.TPASrPlanAdministrator);

                                                if (oTpaCt == null || oTpaCt.Count == 0) // if TPASrPlanAdministrator is not defined then send to TPA Owner
                                                {
                                                    oTpaCt = GetTPAContactsByContactType(oTPACompanyInfos, E_TPACompanyContactType.TPAOwner);
                                                }
                                            }


                                            if (oTpaCt != null && oTpaCt.Count > 0)
                                            {
                                                sMessage_Variables = GetArrayOfMessageServiceKeyValueForTPA(iDocType_id, sLoginType, oConInfo);

                                                foreach (SOAModel.TPAContactInformation oCtTpa in oTpaCt)
                                                {
                                                    individual_id = ((oCtTpa.Contact_id == null) ? 0 : Convert.ToInt32(oCtTpa.Contact_id)); // For TPAs use Contact_id as unique identifier
                                                    semail_id = ((oCtTpa.CommunicationInfo != null && oCtTpa.CommunicationInfo.EmailAddress != null) ? oCtTpa.CommunicationInfo.EmailAddress : "");

                                                    if ((individual_id != 0 && iMsgCtr_Template_id != 0) || (semail_id != "" && iMsg_Template_id != 0))
                                                    {
                                                        sMessage_Variables = AddMessageVariable(sMessage_Variables, "emailto_name", oCtTpa.FirstName + " " + oCtTpa.LastName);
                                                        _oConsolidatedNotificationsDC.InsertMessageQueue(row_no, individual_id, sLoginType, semail_id, splan_name, sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, bMultipleDocTypesGrouped, sDocType_Description);
                                                        bSuccess = true;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //???
                                            }
                                        }
                                    }
                                    else if (sLoginType.ToUpper() == "TPA_ASSIGNED" && oConInfo.ServiceType == "300")
                                    {
                                        oTpaCt = GetTpaAssignedContacts(oConInfo, (E_TPAContactType)Convert.ToInt32(drCon["contact_type"].ToString()), 0);
                                        if (oTpaCt != null && oTpaCt.Count > 0)
                                        {
                                            //sMessage_Variables = GetArrayOfMessageServiceKeyValueForTPA(iDocType_id, sLoginType, oConInfo);

                                            foreach (SOAModel.TPAContactInformation oCtTpa in oTpaCt)
                                            {
                                                individual_id = ((oCtTpa.Contact_id == null) ? 0 : Convert.ToInt32(oCtTpa.Contact_id)); // For TPAs use Contact_id as unique identifier
                                                semail_id = ((oCtTpa.CommunicationInfo != null && oCtTpa.CommunicationInfo.EmailAddress != null) ? oCtTpa.CommunicationInfo.EmailAddress : "");

                                                if ((individual_id != 0 && iMsgCtr_Template_id != 0) || (semail_id != "" && iMsg_Template_id != 0))
                                                {
                                                    sMessage_Variables = AddMessageVariable(sMessage_Variables, "emailto_name", oCtTpa.FirstName + " " + oCtTpa.LastName);
                                                    _oConsolidatedNotificationsDC.InsertMessageQueue(row_no, individual_id, sLoginType, semail_id, splan_name, sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, bMultipleDocTypesGrouped, sDocType_Description);
                                                    bSuccess = true;
                                                }
                                            }
                                        }
                                        else // if TPA_ASSIGNED contacts not found then send email to TPAOwner
                                        {
                                            oTPACompanyInfos = GetTPAContractContactInfoFromSRV(sCid, sSId);
                                            oTpaCt = GetTPAContactsByContactType(oTPACompanyInfos, E_TPACompanyContactType.TPAOwner);
                                            if (oTpaCt != null && oTpaCt.Count > 0)
                                            {
                                                //sMessage_Variables = GetArrayOfMessageServiceKeyValueForTPA(iDocType_id, sLoginType, oConInfo);

                                                foreach (SOAModel.TPAContactInformation oCtTpa in oTpaCt)
                                                {
                                                    individual_id = ((oCtTpa.Contact_id == null) ? 0 : Convert.ToInt32(oCtTpa.Contact_id)); // For TPAs use Contact_id as unique identifier
                                                    semail_id = ((oCtTpa.CommunicationInfo != null && oCtTpa.CommunicationInfo.EmailAddress != null) ? oCtTpa.CommunicationInfo.EmailAddress : "");

                                                    if ((individual_id != 0 && iMsgCtr_Template_id != 0) || (semail_id != "" && iMsg_Template_id != 0))
                                                    {
                                                        sMessage_Variables = AddMessageVariable(sMessage_Variables, "emailto_name", oCtTpa.FirstName + " " + oCtTpa.LastName);
                                                        _oConsolidatedNotificationsDC.InsertMessageQueue(row_no, individual_id, sLoginType, semail_id, splan_name, sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, bMultipleDocTypesGrouped, sDocType_Description);
                                                        bSuccess = true;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                //???
                                            }
                                        }
                                    }
                                    else if ((sLoginType.ToUpper() == "PRODUCER"))
                                    {
                                        oCt = GetContactsByContactType(oConInfo, (E_ContactType)Convert.ToInt32(drCon["contact_type"].ToString()));
                                        if (oCt != null && oCt.Count > 0)
                                        {
                                            sMessage_Variables = GetArrayOfMessageServiceKeyValueForFA(iDocType_id, sLoginType, oConInfo);

                                            foreach (SOAModel.PlanContactInfo oPlCt in oCt)
                                            {
                                                individual_id = ((oPlCt.WebInLoginID == null) ? 0 : Convert.ToInt32(oPlCt.WebInLoginID)); // For producers use WebInLoginID as unique identifier
                                                semail_id = ((oPlCt.Email == null) ? "" : oPlCt.Email);

                                                if ((individual_id != 0 && iMsgCtr_Template_id != 0) || (semail_id != "" && iMsg_Template_id != 0))
                                                {
                                                    sMessage_Variables = AddMessageVariable(sMessage_Variables, "emailto_name", oPlCt.FirstName + " " + oPlCt.LastName);
                                                    _oConsolidatedNotificationsDC.InsertMessageQueue(row_no, individual_id, sLoginType, semail_id, splan_name, sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, bMultipleDocTypesGrouped, sDocType_Description);
                                                    bSuccess = true;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            //???
                                        }
                                    }
                                    else if ((sLoginType.ToUpper() == "SPONSOR"))
                                    {
                                        // overwrite sMessage_Variables here and add message specific code here
                                        sMessage_Variables = GetArrayOfMessageServiceKeyValue(iDocType_id, sLoginType, oConInfo);

                                        if (iDocType_id == 663)
                                        {
                                            List<int> individualIDList = GetSelectedCommunicationTrustees(sCid, sSId);
                                            if (individualIDList.Count > 0)
                                            {
                                                oCt = GetContactsByIndivialID(oConInfo, individualIDList);
                                            }
                                            else
                                            {
                                                oCt = GetContactsByContactType(oConInfo, (E_ContactType)Convert.ToInt32(drCon["contact_type"].ToString()));
                                            }
                                        }
                                        else
                                        {
                                            oCt = GetContactsByContactType(oConInfo, (E_ContactType)Convert.ToInt32(drCon["contact_type"].ToString()));
                                        }

                                        if (oCt != null && oCt.Count > 0)
                                        {
                                            foreach (SOAModel.PlanContactInfo oPlCt in oCt)
                                            {
                                                individual_id = oPlCt.IndividualID;// For sponsors and others use IndividualID as unique identifier
                                                semail_id = ((oPlCt.Email == null) ? "" : oPlCt.Email);

                                                if ((individual_id != 0 && iMsgCtr_Template_id != 0) || (semail_id != "" && iMsg_Template_id != 0))
                                                {
                                                    sMessage_Variables = AddMessageVariable(sMessage_Variables, "emailto_name", oPlCt.FirstName + " " + oPlCt.LastName);
                                                    _oConsolidatedNotificationsDC.InsertMessageQueue(row_no, individual_id, sLoginType, semail_id, splan_name, sMessage_Variables, iMsg_Template_id, iMsgCtr_Template_id, dtSendDate, bMultipleDocTypesGrouped, sDocType_Description);
                                                    bSuccess = true;
                                                }

                                            }
                                        }
                                        else
                                        {
                                            //???
                                        }
                                    }
                                }
                            }// end try
                            catch (Exception exI)
                            {
                                Utils.LogError(exI);
                                oReturn.returnStatus = ReturnStatusEnum.Unknown;
                                oReturn.isException = true;
                                oReturn.confirmationNo = string.Empty;
                                oReturn.Errors.Add(new ErrorInfo(-1, "row_no = " + row_no.ToString() + " - " + " Error: " + exI.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.Warning));
                            }
                        }//end for each drCon
                        if (bSuccess == true)
                        {
                            _oConsolidatedNotificationsDC.UpdateInputDetailsStatus(row_no, C_INPUTDETAILS_RESULT_COMPLETE);


                            bool bRollUpMEP = false;
                            switch (iDocType_id)
                            {
                                case 696: //SMM
                                case 161: //SPD
                                case 746: //404(a) Annual Disclosure
                                          //case 653: //Participant Notice for future
                                case 695: // SAR -- now PENCO Sar is also imaged in WMS
                                    if (sSId_orig == "000" && iDocType_id == 696)
                                    {
                                        bRollUpMEP = true;
                                    }
                                    else
                                    {
                                        bRollUpMEP = false;
                                    }

                                    int ireturn;
                                    //if (PartnerID != "1300" && PartnerID != "ISC")
                                    if (PartnerID == "800" || PartnerID == "PENCO") // ISC cases are processed in CheckAndSubmitIMReqdNotice function
                                    {
                                        subNotificationType = 0;
                                        feed = GetReqdNoticeFeed(iDocType_id, PartnerID, ref subNotificationType);

                                        if (feed != string.Empty)
                                        {
                                            feed = feed.Trim();
                                            if (feed.Length > 0)
                                            {
                                                feed += ",";
                                            }
                                        }

                                        feed += @"https://www.ta-retirement.com/Employee/Secure/pa_documentcenter.aspx?cid=" + sCid + "&sid=" + sSId_orig; //IT-84035
                                        ireturn = _oConsolidatedNotificationsDC.InsertTmpDIAFeedDailyForAllPpt(sCid, sSId_orig, 4, bRollUpMEP, subNotificationType, feed);
                                    }

                                    break;
                            }
                        }
                        else
                        {
                            throw new Exception("DocTypeId = " + iDocType_id.ToString() + ": Contact info NOT found for any of the contacts in ContractInfo/TPACompanyInfo objects for contract_id-sub_id = " + sCid + "-" + sSId);
                        }
                    }
                    else
                    {
                        if (iDocType_id == 774)
                        {
                            _oConsolidatedNotificationsDC.UpdateInputDetailsStatus(row_no, C_INPUTDETAILS_RESULT_COMPLETE);
                        }
                        else
                        {
                            throw new Exception(" DocTypeId = " + iDocType_id.ToString() + " : Contact details info NOT found in nt_ContactDetails table for contract_id-sub_id = " + sCid + "-" + sSId);
                        }
                    }

                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    _oConsolidatedNotificationsDC.UpdateInputDetailsStatus(row_no, C_INPUTDETAILS_RESULT_ERROR);

                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.isException = true;
                    oReturn.confirmationNo = string.Empty;
                    oReturn.Errors.Add(new ErrorInfo(-1, "row_no = " + row_no.ToString() + " - " + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
                }
            }

            return oReturn;
        }
        private List<int> GetSelectedCommunicationTrustees(string cid, string sid)
        {
            string sClientId = string.Empty;
            BFL.FundWizard oFW = new(Guid.NewGuid().ToString(), cid, sid);
            List<int> IndividualIDs = new();
            DataSet dsFWComm = null;
            DataView dv = null;
            dsFWComm = oFW.GetCommunicationInfoByContract();

            if ((dsFWComm != null) && dsFWComm.Tables.Count > 1)
            {
                dv = dsFWComm.Tables[1].DefaultView;
                dv.RowFilter = "StatementID = '2001'";

                foreach (DataRowView dr in dv)
                {
                    if ((dr["IndividualID"] != null))
                    {
                        IndividualIDs.Add(Convert.ToInt32(dr["IndividualID"]));
                    }
                }
            }
            return IndividualIDs;
        }
        private string GetReqdNoticeFeed(int iDocTypeId_orig, string PartnerID, ref int subNotificationType) // same copy of this function exists in eSatement class
        {
            string feed = ""; // as of now feed is always blank for PENCO
            //string fileName = "";
            if (PartnerID.ToUpper() == "PENCO" || PartnerID.ToUpper() == "800")
            {
                feed = "";
                switch (iDocTypeId_orig)
                {
                    case 695: //SAR
                        subNotificationType = 5;
                        break;
                    case 696: //SMM
                        subNotificationType = 6;
                        break;
                    case 161: //SPD
                        subNotificationType = 7;
                        break;
                    case 746:  //404
                        //case 689:  //404 - not requd for PENCO
                        subNotificationType = 8;
                        break;
                    //case 653: //APN             // This document is taken care in BFL contractservice  for PENCO
                    //    subNotificationType = 4;
                    //    break;
                    default:
                        break;
                }
            }
            return feed;
        }
        private string GetArrayOfMessageServiceKeyValue(int iDocType_id, string sLoginType, SOAModel.ContractInfo oConInfo)// dont call this method for TPA and FA
        {
            XElement xElMsgVar = null;
            string sMessage_Variables = "";
            bool bIsPass = false;
            bool bIsMEP = false;
            string spass_txt = "<b> </b>";
            string snon_pass_txt = "<b> </b>";
            string snon_pass_txt1 = "<b> </b>";
            string sep_mep_text = "";

            //if (GetKeyValue("PassPlan", oConInfo.KeyValuePairs) == "1")
            if (GetKeyValue("PassCommunications", oConInfo.KeyValuePairs) == "1")
            {
                bIsPass = true;
            }

            if (oConInfo.FlagValues.isMEP)
            {
                bIsMEP = true;
            }

            if (sLoginType.ToUpper() == "SPONSOR") // dont call this method for TPA and FA
            {

                switch (iDocType_id)
                {
                    case 689:

                        if (bIsPass == true)
                        {
                            spass_txt = @"<p><b> As part of our PASS services, Transamerica will be distributing your Disclosure & Comparative Chart on your behalf to everyone who may direct investments in your plan in advance of the effective date. </b></p>
You can find Frequently Asked Questions on <a href=""http://www.TA-Retirement.com/default.aspx?"">www.TA-Retirement.com</a>,  (login > Participant Fee Disclosure).  Please call us if you have any questions at (866) 498-4557, 9 a.m. to 8 p.m. Eastern Time, Monday through Friday.";
                        }
                        else
                        {
                            snon_pass_txt = @"<p>You can find Frequently Asked Questions on <a href=""http://www.TA-Retirement.com/default.aspx?"">www.TA-Retirement.com</a>,  (login > Participant Fee Disclosure).  Please call us if you have any questions at (866) 498-4557, 9 a.m. to 8 p.m. Eastern Time, Monday through Friday.></p>";
                            snon_pass_txt1 = @"<p><b>Please ensure that they receive this information on or before August 30, 2012.</b> Here is sample text that you may want to use when you are distributing this disclosure:</p>
<blockquote><i>We are committed to providing you with information that can help you make the most of your retirement plan. That's why we want to call attention to a new disclosure regarding retirement plan information, fees, and investments.
<br/><br/>As of August 30, 2012, the U.S. Department of Labor (DOL) has enacted new, uniform guidelines for presenting information to employees regarding the investments and fees associated with their plan accounts.   You are receiving this new disclosure to comply with these new DOL requirements.
<br/><br/>The new, standardized format is designed to help you compare information and fees when deciding how to invest your account. The fees are not new; however, the new standardized format is intended to make it easier to compare fees. You can review and use this information along with other resources to make informed decisions about how to best save for your future retirement. <br/>
<br/>We hope you find this information to be helpful. Remember, your retirement plan offers an opportunity to save and invest for your future.</i>
<br/><br/></blockquote>";
                        }

                        xElMsgVar =
                           new XElement("ArrayOfMessageServiceKeyValue",
                               new XElement("MessageServiceKeyValue",
                                   new XElement("key", "pass_text"),
                                   new XElement("value", spass_txt)
                                   ),
                                   new XElement("MessageServiceKeyValue",
                                   new XElement("key", "non_pass_text"),
                                   new XElement("value", snon_pass_txt)
                                   ),
                                   new XElement("MessageServiceKeyValue",
                                   new XElement("key", "non_pass_text1"),
                                   new XElement("value", snon_pass_txt1)
                                   )
                                   );

                        //sMessage_Variables = xElMsgVar.ToString();


                        break;
                    case 746:

                        if (bIsPass == true)
                        {
                            spass_txt = @"<b>As part of our PASS Communication services, Transamerica will fulfill your annual delivery requirement by distributing your Participant Fee Disclosure on your behalf in your plan in conjunction with any other required notices in your plan.</b>";
                        }
                        else
                        {
                            spass_txt = @"<b>Important Reminder:</b> The U.S. Department of Labor (DOL) requires that you provide newly eligible plan participants a Participant Fee Disclosure before they can first select investment choices under an ERISA-qualified defined contribution plan (e.g., 401(k) plans). The DOL also requires that you provide all plan-eligible employees, former employees and beneficiaries an updated Participant Fee Disclosure annually, on a permanently established delivery date (with a 60-day grace period). When providing the disclosure, you should use the most recent version found in your online Document Center.<br /> <p>Plan participants who have registered to receive electronic notices from Transamerica will receive quarterly updates to the Participant Fee Disclosure in their participant Document Center. When the June 30th Participant Fee Disclosure is placed in the participant Document Center, the participant will receive an email notifying them that the document has been delivered and they should go to the Document Center to review it. This constitutes affirmative delivery to participants who have registered to receive electronic notices, relieving the plan sponsor of the responsibility of delivering the document to those participants. The Employee Address Report on TA-Retirement.com lists those participants who have elected to receive electronic notices.<p/>";
                        }

                        if (bIsMEP == true)
                        {
                            sep_mep_text = @"Please call TRSConnect if you have any questions at 800-875-8877, 9 a.m. to 8 p.m. Eastern Time, Monday through Friday.";
                        }
                        else
                        {
                            sep_mep_text = @"Please call SponsorConnect if you have any questions at 866-498-4557, 9 a.m. to 8 p.m. Eastern Time, Monday through Friday.";
                        }

                        xElMsgVar =
                           new XElement("ArrayOfMessageServiceKeyValue",
                               new XElement("MessageServiceKeyValue",
                                   new XElement("key", "pass_nopass_text"),
                                   new XElement("value", spass_txt)
                                   ),
                                   new XElement("MessageServiceKeyValue",
                                   new XElement("key", "sep_mep_text"),
                                   new XElement("value", sep_mep_text)
                                   )
                                   );

                        //sMessage_Variables = xElMsgVar.ToString();


                        break;
                    case 663:

                        if (bIsMEP == true)
                        {
                            xElMsgVar =
                            new XElement("ArrayOfMessageServiceKeyValue",
                                new XElement("MessageServiceKeyValue",
                                    new XElement("key", "contact_phone"),
                                    new XElement("value", "(800) 875-8877")
                                    ),
                                    new XElement("MessageServiceKeyValue",
                                    new XElement("key", "contact_email"),
                                    new XElement("value", "trsconnect@transamerica.com")
                                    )
                                    );
                        }
                        else
                        {
                            xElMsgVar =
                            new XElement("ArrayOfMessageServiceKeyValue",
                                new XElement("MessageServiceKeyValue",
                                    new XElement("key", "contact_phone"),
                                    new XElement("value", "(866) 498-4557")
                                    ),
                                    new XElement("MessageServiceKeyValue",
                                    new XElement("key", "contact_email"),
                                    new XElement("value", "sponsorconnect@transamerica.com")
                                    )
                                    );
                        }


                        //sMessage_Variables = xElMsgVar.ToString();
                        break;
                    case 161:
                        if (bIsMEP == true)
                        {
                            xElMsgVar =
                                new XElement("ArrayOfMessageServiceKeyValue",
                                    new XElement("MessageServiceKeyValue",
                                        new XElement("key", "doc_name"),
                                        new XElement("value", "SPD")
                                        ),
                                        new XElement("MessageServiceKeyValue",
                                        new XElement("key", "contact_phone"),
                                        new XElement("value", "(800) 875-8877")
                                        )
                                        );

                        }
                        else
                        {
                            xElMsgVar =
                               new XElement("ArrayOfMessageServiceKeyValue",
                                   new XElement("MessageServiceKeyValue",
                                       new XElement("key", "doc_name"),
                                       new XElement("value", "SPD")
                                       ),
                                       new XElement("MessageServiceKeyValue",
                                       new XElement("key", "contact_phone"),
                                       new XElement("value", "(866) 498-4557")
                                       )
                                       );
                        }

                        break;
                    case 695:
                        if (bIsMEP == true)
                        {
                            xElMsgVar =
                                new XElement("ArrayOfMessageServiceKeyValue",
                                    new XElement("MessageServiceKeyValue",
                                        new XElement("key", "doc_name"),
                                        new XElement("value", "SAR")
                                        ),
                                        new XElement("MessageServiceKeyValue",
                                        new XElement("key", "contact_phone"),
                                        new XElement("value", "(800) 875-8877")
                                        )
                                        );

                        }
                        else
                        {
                            xElMsgVar =
                               new XElement("ArrayOfMessageServiceKeyValue",
                                   new XElement("MessageServiceKeyValue",
                                       new XElement("key", "doc_name"),
                                       new XElement("value", "SAR")
                                       ),
                                       new XElement("MessageServiceKeyValue",
                                       new XElement("key", "contact_phone"),
                                       new XElement("value", "(866) 498-4557")
                                       )
                                       );
                        }

                        break;
                    case 696:
                        if (bIsMEP == true)
                        {
                            xElMsgVar =
                                new XElement("ArrayOfMessageServiceKeyValue",
                                    new XElement("MessageServiceKeyValue",
                                        new XElement("key", "doc_name"),
                                        new XElement("value", "SMM")
                                        ),
                                        new XElement("MessageServiceKeyValue",
                                        new XElement("key", "contact_phone"),
                                        new XElement("value", "(800) 875-8877")
                                        )
                                        );

                        }
                        else
                        {
                            xElMsgVar =
                               new XElement("ArrayOfMessageServiceKeyValue",
                                   new XElement("MessageServiceKeyValue",
                                       new XElement("key", "doc_name"),
                                       new XElement("value", "SMM")
                                       ),
                                       new XElement("MessageServiceKeyValue",
                                       new XElement("key", "contact_phone"),
                                       new XElement("value", "(866) 498-4557")
                                       )
                                       );
                        }
                        break;
                    case 36500:
                    case 36510:
                    case 36520:
                        if (bIsMEP == false)
                        {
                            sep_mep_text = @"<p>If you have any questions regarding your reports or have trouble accessing them, please contact a Transamerica Retirement Solutions representative at 866-498-4557, 9 AM to 8 PM Eastern Time, Monday through Friday.</p>";
                        }
                        else
                        {
                            sep_mep_text = @"<p></p>";
                        }

                        xElMsgVar =
                           new XElement("ArrayOfMessageServiceKeyValue",
                                   new XElement("MessageServiceKeyValue",
                                   new XElement("key", "sep_mep_text"),
                                   new XElement("value", sep_mep_text)
                                   )
                                   );
                        break;
                    default:
                        sMessage_Variables = "";
                        break;
                }
            }

            if (xElMsgVar != null)
            {
                sMessage_Variables = xElMsgVar.ToString();
            }
            return sMessage_Variables;
        }
        private string GetArrayOfMessageServiceKeyValueForFA(int iDocType_id, string sLoginType, SOAModel.ContractInfo oConInfo)// dont call this method for TPA and FA
        {
            XElement xElMsgVar = null;
            string sMessage_Variables = "";
            bool bIsPass = false;
            string spass_txt = "<b> </b>";

            if (sLoginType.ToUpper() == "PRODUCER")
            {
                if (GetKeyValue("PassCommunications", oConInfo.KeyValuePairs) == "1")
                {
                    bIsPass = true;
                }

                switch (iDocType_id)
                {
                    case 746:
                        if (bIsPass == true)
                        {
                            spass_txt = @"<b>As part of our PASS services, Transamerica will be distributing your Participant Fee Disclosure on your behalf to everyone who may direct investments in your plan in conjunction with any other required notices in your plan.</b>";
                        }
                        else
                        {
                            spass_txt = @"Newly Eligible Plan Participants:<br /><br />
The U.S. Department of Labor (&quot;DOL&quot;) requires that employees who are newly eligible to participate in your client&#39;s retirement plan receive this disclosure before they can first select investment choices under the plan. The document should be provided to any newly eligible plan participants before they enroll in the plan.  The purpose of the DOL regulation is to provide uniform guidelines and make sure that employees have the information they need to make informed decisions about the investment of their retirement plan accounts.
<br /><br />Annual Participant Fee Disclosure:<br /><br />The DOL also requires that all plan-eligible employees, former employees, and beneficiaries receive an Annual Participant Fee Disclosure within 12 months after the delivery of the prior year&#39;s disclosure. The document can be used to satisfy your Annual Disclosure requirements.";
                        }

                        xElMsgVar = new XElement("ArrayOfMessageServiceKeyValue",
                                        new XElement("MessageServiceKeyValue",
                                        new XElement("key", "pass_nopass_text"),
                                        new XElement("value", spass_txt)
                                    )
                                   );

                        break;
                    default:
                        sMessage_Variables = "";
                        break;
                }
            }

            if (xElMsgVar != null)
            {
                sMessage_Variables = xElMsgVar.ToString();
            }
            return sMessage_Variables;
        }
        private string GetArrayOfMessageServiceKeyValueForTPA(int iDocType_id, string sLoginType, SOAModel.ContractInfo oConInfo)
        {
            XElement xElMsgVar = null;
            string sMessage_Variables = "";
            bool bIsPass = false;
            string spass_txt = "<b> </b>";

            if (sLoginType.ToUpper() == "TPA")
            {
                if (GetKeyValue("PassCommunications", oConInfo.KeyValuePairs) == "1")
                {
                    bIsPass = true;
                }

                switch (iDocType_id)
                {
                    case 746:
                        if (bIsPass == true)
                        {
                            spass_txt = @"<b>As part of our PASS services, Transamerica will be distributing your Participant Fee Disclosure on your behalf to everyone who may direct investments in your plan in conjunction with any other required notices in your plan.</b>";
                        }
                        else
                        {
                            spass_txt = @"Newly Eligible Plan Participants:<br /><br />
The U.S. Department of Labor (&quot;DOL&quot;) requires that employees who are newly eligible to participate in your retirement plan receive this disclosure before they can first select investment choices under the plan. The document should be provided to any newly eligible plan participants before they enroll in the plan.  The purpose of the DOL regulation is to provide uniform guidelines and make sure that employees have the information they need to make informed decisions about the investment of their retirement plan accounts.
<br /><br />Annual Participant Fee Disclosure:<br /><br />The DOL also requires that all plan-eligible employees, former employees, and beneficiaries receive an Annual Participant Fee Disclosure within 12 months after the delivery of the prior year&#39;s disclosure. The document can be used to satisfy your Annual Disclosure requirements.";
                        }

                        xElMsgVar = new XElement("ArrayOfMessageServiceKeyValue",
                                        new XElement("MessageServiceKeyValue",
                                        new XElement("key", "pass_nopass_text"),
                                        new XElement("value", spass_txt)
                                    )
                                   );

                        break;
                    default:
                        sMessage_Variables = "";
                        break;
                }
            }

            if (xElMsgVar != null)
            {
                sMessage_Variables = xElMsgVar.ToString();
            }
            return sMessage_Variables;
        }
        #endregion

        #region ProcessMessageQueue
        public TaskStatus ProcessMessageQueueMigrated()
        {
            TaskStatus oTaskReturn = new();
            oTaskReturn.retStatus = TaskRetStatus.Succeeded;
            ResultReturn oReturn;
            ResultReturn oReturn1;
            ResultReturn oReturn2;

            const string C_Task = "ProcessMessageQueue";

            DataSet dsIndiv = new();
            DataSet dsContr = new();
            DataSet dsDocGrpContr = new();
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;

                if (TrsAppSettings.AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    dsIndiv = _oConsolidatedNotificationsDC.GetConsolidatedMessageQueue();

                    oReturn = ProcessPendingIndividualMessageQueue(dsIndiv);

                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.rowsCount += oReturn.rowsCount;


                    dsContr = _oConsolidatedNotificationsDC.GetConsolidatedContractMessageQueue();

                    oReturn1 = ProcessPendingContractMessageQueue(dsContr);

                    if (oReturn1.returnStatus != ReturnStatusEnum.Succeeded || oReturn1.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn1);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.rowsCount += oReturn1.rowsCount;

                    dsDocGrpContr = _oConsolidatedNotificationsDC.GetConsolidateDocGroupdMessageQueue();

                    oReturn2 = ProcessPendingDocGroupMessageQueue(dsDocGrpContr);

                    if (oReturn2.returnStatus != ReturnStatusEnum.Succeeded || oReturn2.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn2);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.rowsCount += oReturn2.rowsCount;


                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        private ResultReturn ProcessPendingContractMessageQueue(DataSet dsMsgQ) //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            string sCid = "";
            string sSId = ""; //string sSId_override = "";
            int row_counter = 0;
            string semail_id = "";
            int iMsg_Template_id = 0;
            int iMsgCtr_Template_id = 0;
            string sData_To_Consolidate = "";
            string sMessage_Variables = "";
            ResultReturn oRetEmail = null;
            bool bCustom_Recipients = false;
            int iCustom_Recipient_Type = 0;
            int iDocType_id = 0;
            string sLoginType = "";
            string sCustom_emails = ""; string sCustom_MsgcenterIds = "";
            bool bClientContactsReplaced = false;
            if (dsMsgQ != null && dsMsgQ.Tables.Count > 0)
            {
                foreach (DataRow dr in dsMsgQ.Tables[0].Rows)
                {
                    oRetEmail = null;

                    row_counter = Convert.ToInt32(dr["row_counter"].ToString());
                    sCustom_emails = ""; sCustom_MsgcenterIds = ""; bCustom_Recipients = false; iCustom_Recipient_Type = 0;
                    try
                    {
                        sCid = dr["contract_id"].ToString();
                        sSId = dr["sub_id"].ToString();
                        //sSId_override = sSId;
                        sLoginType = dr["LoginType"].ToString();
                        semail_id = dr["email_id"].ToString(); ;
                        iMsg_Template_id = Convert.ToInt32(dr["Msg_Template_id"].ToString());
                        iMsgCtr_Template_id = Convert.ToInt32(dr["MsgCtr_Template_id"].ToString());

                        iDocType_id = Convert.ToInt32(dr["DocType_id"].ToString());

                        sData_To_Consolidate = dr["Data_To_Consolidate"].ToString();
                        //sData_To_Consolidate = sData_To_Consolidate.Replace("$", "<BR />");
                        sMessage_Variables = dr["Message_Variables"].ToString();
                        sData_To_Consolidate = sData_To_Consolidate.Replace("</tr><BR /><tr>", "</tr><tr>");
                        sData_To_Consolidate = sData_To_Consolidate.Replace("</tr><br /><tr>", "</tr><tr>");
                        sData_To_Consolidate = sData_To_Consolidate.Replace("</TR><BR /><TR>", "</TR><TR>");
                        sData_To_Consolidate = sData_To_Consolidate.Replace("</TR><br /><TR>", "</TR><TR>");

                        if (iMsg_Template_id == 2990)
                        {
                            sData_To_Consolidate = FormatDataForMsgId2990(iMsg_Template_id, sData_To_Consolidate); //    IT-87084
                        }

                        //if (iDocType_id == 9999 || iDocType_id == 10010) // dont touch any other doctypes for now
                        if (IsCustomContactEnabled(iDocType_id))
                        {
                            iCustom_Recipient_Type = GetCustomContactInfo(sCid, sSId, iDocType_id, sLoginType, ref sCustom_emails, ref sCustom_MsgcenterIds, ref bClientContactsReplaced);
                            if (iCustom_Recipient_Type == 0)
                            {
                                bCustom_Recipients = false;
                            }
                            else
                            {
                                bCustom_Recipients = true;
                            }
                        }

                        if (iMsgCtr_Template_id != 0)
                        {
                            if (bCustom_Recipients == true)
                            {
                                oRetEmail = SendEmailNotification_Custom(iMsgCtr_Template_id, sCid, sSId, sData_To_Consolidate, sMessage_Variables, sCustom_emails, sCustom_MsgcenterIds, bClientContactsReplaced);
                            }
                            else
                            {
                                oRetEmail = SendEmailNotification(iMsgCtr_Template_id, sCid, sSId, sData_To_Consolidate, sMessage_Variables);
                            }
                            if (oRetEmail.returnStatus != ReturnStatusEnum.Succeeded)
                            {
                                oReturn.Errors.Add(new ErrorInfo(-1, "row_counter = " + row_counter.ToString() + " ContractId-SubId = " + sCid + "-" + sSId + "  iMsgCtr_Template_id = " + iMsgCtr_Template_id.ToString() + " - " + " SendEmailNotification Error: " + oRetEmail.Errors[0].errorDesc + "<BR />", ErrorSeverityEnum.Failed));
                            }
                        }


                        if (bCustom_Recipients == true)
                        {
                            oRetEmail = SendEmailNotification_Custom(iMsg_Template_id, sCid, sSId, sData_To_Consolidate, sMessage_Variables, sCustom_emails, sCustom_MsgcenterIds, bClientContactsReplaced);
                        }
                        else
                        {
                            oRetEmail = SendEmailNotification(iMsg_Template_id, sCid, sSId, sData_To_Consolidate, sMessage_Variables);
                        }

                        if (oRetEmail.returnStatus == ReturnStatusEnum.Succeeded)
                        {
                            _oConsolidatedNotificationsDC.UpdateConsolidatedContractMessageQueueStatus(row_counter, C_MSGQUEUE_RESULT_COMPLETE);
                        }
                        else
                        {
                            _oConsolidatedNotificationsDC.UpdateConsolidatedContractMessageQueueStatus(row_counter, C_MSGQUEUE_RESULT_ERROR);

                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "row_counter = " + row_counter.ToString() + " ContractId-SubId = " + sCid + "-" + sSId + "  iMsg_Template_id = " + iMsg_Template_id.ToString() + " - " + " SendEmailNotification Error: " + oRetEmail != null && oRetEmail.Errors.Count > 0 ? oRetEmail.Errors[0].errorDesc : string.Empty + "<BR />", ErrorSeverityEnum.Failed));
                        }

                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex);
                        _oConsolidatedNotificationsDC.UpdateConsolidatedContractMessageQueueStatus(row_counter, C_MSGQUEUE_RESULT_ERROR);

                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.isException = true;
                        oReturn.confirmationNo = string.Empty;
                        //oReturn.Errors.Add(new ErrorInfo(-1, "row_counter = " + row_counter.ToString() + " - " + " Error: " + ex.Message + System.Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
                        oReturn.Errors.Add(new ErrorInfo(-1, "row_counter = " + row_counter.ToString() + " ContractId-SubId = " + sCid + "-" + sSId + "  iMsg_Template_id = " + iMsg_Template_id.ToString() + " - " + " SendEmailNotification Error: " + ex.Message + "<BR />", ErrorSeverityEnum.ExceptionRaised));
                    }
                }


            }

            return oReturn;
        }
        private ResultReturn ProcessPendingIndividualMessageQueue(DataSet dsMsgQ) //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            int row_counter = 0;
            int individual_id = 0;
            string semail_id = "";
            int iMsg_Template_id = 0;
            int iMsgCtr_Template_id = 0;
            string sPlan_names = "";
            string sMessage_Variables = "";
            ResultReturn oRetEmail = null;

            if (dsMsgQ != null && dsMsgQ.Tables.Count > 0)
            {
                foreach (DataRow dr in dsMsgQ.Tables[0].Rows)
                {

                    oRetEmail = null;
                    row_counter = Convert.ToInt32(dr["row_counter"].ToString());

                    try
                    {
                        individual_id = Convert.ToInt32(dr["in_login_id"].ToString()); // in table, column name is in_login_id
                        semail_id = dr["email_id"].ToString(); ;
                        iMsg_Template_id = Convert.ToInt32(dr["Msg_Template_id"].ToString());
                        iMsgCtr_Template_id = Convert.ToInt32(dr["MsgCtr_Template_id"].ToString());
                        sPlan_names = dr["Data_To_Consolidate"].ToString();
                        sPlan_names = sPlan_names.Replace("$", "<BR />");
                        sMessage_Variables = dr["Message_Variables"].ToString();
                        if (semail_id != "")
                        {
                            oRetEmail = SendEmailNotification(iMsg_Template_id, semail_id, sPlan_names, sMessage_Variables);
                        }
                        if (oRetEmail != null && oRetEmail.returnStatus == ReturnStatusEnum.Succeeded)
                        {
                            _oConsolidatedNotificationsDC.UpdateConsolidatedMessageQueueStatus(row_counter, C_MSGQUEUE_RESULT_COMPLETE);
                        }
                        else
                        {
                            _oConsolidatedNotificationsDC.UpdateConsolidatedMessageQueueStatus(row_counter, C_MSGQUEUE_RESULT_ERROR);

                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            string errorDesc = oRetEmail != null && oRetEmail.Errors.Count > 0 && !string.IsNullOrEmpty(oRetEmail.Errors[0].errorDesc) ? oRetEmail.Errors[0].errorDesc : string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "row_counter = " + row_counter.ToString() + " individual_id = " + individual_id.ToString() + " iMsg_Template_id = " + iMsg_Template_id.ToString() + " - " + " SendEmailNotification Error: " + errorDesc + "<BR />", ErrorSeverityEnum.Failed));
                        }

                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex);
                        _oConsolidatedNotificationsDC.UpdateConsolidatedMessageQueueStatus(row_counter, C_MSGQUEUE_RESULT_ERROR);

                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.isException = true;
                        oReturn.confirmationNo = string.Empty;
                        oReturn.Errors.Add(new ErrorInfo(-1, "individual_id = " + individual_id.ToString() + " - " + " Error: " + ex.Message + "<BR />", ErrorSeverityEnum.ExceptionRaised));
                    }
                }


            }

            return oReturn;
        }
        private ResultReturn ProcessPendingDocGroupMessageQueue(DataSet dsMsgQ) //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            string sXMl_row_counter = "";
            int individual_id = 0;
            string semail_id = "";
            int iMsg_Template_id = 0;
            int iMsgCtr_Template_id = 0;
            string sPlan_names = "";
            string sMessage_Variables = "";

            ResultReturn oRetEmail = null;

            if (dsMsgQ != null && dsMsgQ.Tables.Count > 0)
            {
                foreach (DataRow dr in dsMsgQ.Tables[0].Rows)
                {

                    oRetEmail = null;

                    //row_counter = Convert.ToInt32(dr["row_counter"].ToString());

                    try
                    {
                        sXMl_row_counter = dr["row_counters"].ToString();
                        individual_id = Convert.ToInt32(dr["in_login_id"].ToString()); // in table, column name is in_login_id
                        semail_id = dr["email_id"].ToString();
                        iMsg_Template_id = Convert.ToInt32(dr["Msg_Template_id"].ToString());
                        iMsgCtr_Template_id = Convert.ToInt32(dr["MsgCtr_Template_id"].ToString());
                        sPlan_names = dr["Data_To_Consolidate"].ToString();
                        sPlan_names = sPlan_names.Replace("$", "<BR />");
                        sMessage_Variables = dr["Message_Variables"].ToString();
                        if (semail_id != "")
                        {
                            //oRetEmail = SendEmailNotification(iMsg_Template_id, semail_id, sPlan_names);
                            oRetEmail = SendEmailNotification(iMsg_Template_id, semail_id, sPlan_names, sMessage_Variables);
                        }

                        if (oRetEmail != null && oRetEmail.returnStatus == ReturnStatusEnum.Succeeded)
                        {
                            _oConsolidatedNotificationsDC.UpdateConsolidatedDocGroupdMessageQueueStatus(sXMl_row_counter, C_MSGQUEUE_RESULT_COMPLETE);
                        }
                        else
                        {
                            _oConsolidatedNotificationsDC.UpdateConsolidatedDocGroupdMessageQueueStatus(sXMl_row_counter, C_MSGQUEUE_RESULT_ERROR);

                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            string errorDesc = oRetEmail != null && oRetEmail.Errors.Count > 0 && !string.IsNullOrEmpty(oRetEmail.Errors[0].errorDesc) ? oRetEmail.Errors[0].errorDesc : string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "row_counter = " + sXMl_row_counter + " individual_id = " + individual_id.ToString() + " iMsg_Template_id = " + iMsg_Template_id.ToString() + " - " + " SendEmailNotification Error: " + errorDesc + "<BR />", ErrorSeverityEnum.Failed));
                        }

                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex);
                        if (sXMl_row_counter != "")
                        {
                            _oConsolidatedNotificationsDC.UpdateConsolidatedDocGroupdMessageQueueStatus(sXMl_row_counter, C_MSGQUEUE_RESULT_ERROR);
                        }

                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.isException = true;
                        oReturn.confirmationNo = string.Empty;
                        oReturn.Errors.Add(new ErrorInfo(-1, " individual_id = " + individual_id.ToString() + " - sXMl_row_counter = " + sXMl_row_counter + " - Error: " + ex.Message + "<BR />", ErrorSeverityEnum.ExceptionRaised));
                    }
                }


            }

            return oReturn;
        }
        #endregion

        public SOAModel.ContractInfo GetContractInfoFromSRV(string sContractID, string sSubID)
        {
            ContractServ DriverSOACon = new();

            SOAModel.ContractInfo oConInfo;
            oConInfo = DriverSOACon.GetContractInformation(sContractID, sSubID);
            return oConInfo;
        }
        public SOAModel.TPACompanyContactInformations GetTPAContractContactInfoFromSRV(string sContractID, string sSubID)
        {
            TPASvc DriverSOACon = new();

            SOAModel.TPACompanyContactInformations oTPACompanyInfos;
            oTPACompanyInfos = DriverSOACon.GetContractTPAContacts(sContractID, sSubID);
            return oTPACompanyInfos;
        }
        public List<SOAModel.PlanContactInfo> GetContactsByContactType(SOAModel.ContractInfo ContractInfo, E_ContactType eContactType)
        {
            bool bIgnore = false;
            List<SOAModel.PlanContactInfo> oCt = new();

            for (int iNum = 0; iNum < ContractInfo.PlanContacts.Count; iNum++)
            {
                bIgnore = false;
                if ((ContractInfo.PlanContacts[iNum].WebAccessType != null))
                {
                    if (ContractInfo.PlanContacts[iNum].WebAccessType == "8")
                    {
                        bIgnore = true;
                    }
                }

                if (ContractInfo.PlanContacts[iNum].IndividualID == -999)
                {
                    bIgnore = true;
                }

                if (bIgnore == false)
                {
                    for (int iCnt = 0; iCnt < ContractInfo.PlanContacts[iNum].Type.Count; iCnt++)
                    {

                        if (ContractInfo.PlanContacts[iNum].Type[iCnt] == eContactType)
                        {
                            oCt.Add(ContractInfo.PlanContacts[iNum]);
                        }

                    }
                }
            }
            return oCt;
        }
        private List<SOAModel.PlanContactInfo> GetContactsByIndivialID(SOAModel.ContractInfo ContractInfo, List<int> IndividualIDList)
        {
            bool bIgnore = false;
            List<SOAModel.PlanContactInfo> oCt = new();

            for (int iNum = 0; iNum < ContractInfo.PlanContacts.Count; iNum++)
            {
                bIgnore = false;
                if ((ContractInfo.PlanContacts[iNum].WebAccessType != null))
                {
                    if (ContractInfo.PlanContacts[iNum].WebAccessType == "8")
                    {
                        bIgnore = true;
                    }
                }

                if (ContractInfo.PlanContacts[iNum].IndividualID == -999)
                {
                    bIgnore = true;
                }

                if (bIgnore == false)
                {
                    if (IndividualIDList.Contains(ContractInfo.PlanContacts[iNum].IndividualID))
                    {
                        oCt.Add(ContractInfo.PlanContacts[iNum]);
                    }
                }
            }
            return oCt;
        }
        public List<SOAModel.TPAContactInformation> GetTPAContactsByContactType(SOAModel.TPACompanyContactInformations oTPACompanyInfos, E_TPACompanyContactType eContactType)
        {
            List<SOAModel.TPAContactInformation> oTpaContacts = new();
            if (oTPACompanyInfos.TPAContactInfo.Count > 0)
            {
                for (int iNum = 0; iNum < oTPACompanyInfos.TPAContactInfo.Count; iNum++)
                {
                    if (oTPACompanyInfos.TPAContactInfo[iNum].ContactType == eContactType)
                    {
                        oTpaContacts.Add(oTPACompanyInfos.TPAContactInfo[iNum]);
                    }
                }

            }
            return oTpaContacts;
        }
        public List<SOAModel.TPAContactInformation> GetTpaAssignedContacts(SOAModel.ContractInfo ContractInfo, E_TPAContactType eContactType, int iDocType = 0)
        {
            // There are only 2 tpa assigned contact type , Sr. plan Admin and tpa_cc. In db we will set up only Sr. plan Admin but this method always return both because we have to send email to tpa owner if both are not present
            // use eContactType and iDoctype values for future enhancement as of now they are not being used.

            List<SOAModel.TPAContactInformation> oCt = new();
            if (ContractInfo.TPAContacts.Count > 0)
            {
                for (int iNum = 0; iNum < ContractInfo.TPAContacts.Count; iNum++)
                {
                    //if (ContractInfo.TPAContacts[iNum].ContractContactType == eContactType) // return both
                    if (ContractInfo.TPAContacts[iNum].ContractContactType == E_TPAContactType.TPASrPlanAdministrator)
                    {
                        if (TRSManagers.ValidationManager.IsValidEmailAddress(ContractInfo.TPAContacts[iNum].CommunicationInfo.EmailAddress.ToString()))
                        {
                            oCt.Add(ContractInfo.TPAContacts[iNum]);
                        }
                    }
                    else if (ContractInfo.TPAContacts[iNum].ContractContactType == E_TPAContactType.TPACC_Communications)
                    {
                        if (TRSManagers.ValidationManager.IsValidEmailAddress(ContractInfo.TPAContacts[iNum].CommunicationInfo.EmailAddress.ToString()))
                        {
                            oCt.Add(ContractInfo.TPAContacts[iNum]);
                        }
                    }
                }
            }

            return oCt;
        }
        private string AddMessageVariable(string sMessage_Variables, string sKeyName, string sValue)
        {
            string sReturnMessage_Variables = sMessage_Variables;
            MessageServiceKeyValue[] Keys = null;
            MessageServiceKeyValue nKey = new();
            nKey.key = sKeyName;
            nKey.value = sValue;
            bool bFoundExisting = false;
            try
            {
                if (sMessage_Variables != "")
                {
                    Keys = (MessageServiceKeyValue[])TRSManagers.XMLManager.DeserializeXml(sMessage_Variables, typeof(MessageServiceKeyValue[]));
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
            }

            if (Keys != null && Keys.GetLength(0) > 0)
            {
                for (int i = 0; i < Keys.Length; i++)
                {
                    if (Keys[i].key == sKeyName) // if there is exisitng key then update the value.
                    {
                        bFoundExisting = true;
                        Keys[i].value = sValue;
                        break;
                    }
                }

                if (bFoundExisting == false) // add new only if there is no existing key
                {
                    Array.Resize(ref Keys, Keys.GetLength(0) + 1); // resize to add consolidated data key
                    Keys[Keys.GetLength(0) - 1] = nKey; //add key at last position.
                }

            }
            else // Just add new one
            {
                Keys = new MessageServiceKeyValue[1];
                Keys[0] = nKey;
            }

            sReturnMessage_Variables = TRSManagers.XMLManager.GetXML(Keys);
            return sReturnMessage_Variables;

        }
        private ResultReturn SendEmailNotification(int iMsg_Id, string sEmail, string sData_To_Consolidate, string sMessage_Variables)
        {
            MessageServiceKeyValue[] Keys = null;
            MessageService oMS = new();
            ResultReturn oResults = null;

            if (iMsg_Id == 0)
            {
                oResults = new ResultReturn();
                oResults.returnStatus = ReturnStatusEnum.Succeeded;
                //return oResults;
            }
            else
            {

                MessageServiceKeyValue nKey = new();
                nKey.key = "Consolidated_Data";
                nKey.value = sData_To_Consolidate;

                MessageServiceKeyValue nKey1 = new();
                nKey1.key = "to_email";
                nKey1.value = sEmail;


                try
                {
                    if (sMessage_Variables != "")
                    {
                        Keys = (MessageServiceKeyValue[])TRSManagers.XMLManager.DeserializeXml(sMessage_Variables, typeof(MessageServiceKeyValue[]));
                    }
                }
                catch (Exception ex)
                {

                    Utils.LogError(ex);
                }

                if (Keys != null && Keys.GetLength(0) > 0)
                {
                    Array.Resize(ref Keys, Keys.GetLength(0) + 2); // resize to add consolidated data key
                    Keys[Keys.GetLength(0) - 2] = nKey; //add key at second last position.
                    Keys[Keys.GetLength(0) - 1] = nKey1; //add key at last position.
                }
                else // send only consolidated data
                {
                    Keys = new MessageServiceKeyValue[2];
                    Keys[0] = nKey;
                    Keys[1] = nKey1;
                }

                oResults = oMS.SendMessage_NoContract(iMsg_Id, Keys, "TRS-Auto-Message-Service");

                if (oResults == null)
                {
                    ErrorInfo oError = new();
                    oResults = new ResultReturn();
                    oError.errorNum = 1;
                    oError.errorDesc = "No result returned by MessageService.";
                }
            }
            return oResults;
        }

        private ResultReturn SendEmailNotification(int iMsg_Id, string sContract_id, string sSub_Id, string sData_To_Consolidate, string sMessage_Variables)
        {
            ResultReturn oResults = null;
            MessageServiceKeyValue[] Keys = null;

            if (iMsg_Id == 0)
            {
                oResults = new ResultReturn();
                oResults.returnStatus = ReturnStatusEnum.Succeeded;
                //return oResults;
            }
            else
            {
                MessageServiceKeyValue nKey = new();
                nKey.key = "Consolidated_Data";
                nKey.value = sData_To_Consolidate;

                try
                {
                    if (sMessage_Variables != "")
                    {
                        Keys = (MessageServiceKeyValue[])TRSManagers.XMLManager.DeserializeXml(sMessage_Variables, typeof(MessageServiceKeyValue[]));
                    }
                }
                catch (Exception ex)
                {

                    Utils.LogError(ex);
                }

                if (Keys != null && Keys.GetLength(0) > 0)
                {
                    Array.Resize(ref Keys, Keys.GetLength(0) + 1); // resize to add consolidated data key
                    Keys[Keys.GetLength(0) - 1] = nKey; //add key at last position.
                }
                else // send only consolidated data
                {
                    Keys = new MessageServiceKeyValue[1];
                    Keys[0] = nKey;
                }


                MessageService oMS = new();
                oResults = oMS.SendMessage(sContract_id, sSub_Id, iMsg_Id, Keys, "TRS-Auto-Message-Service");

                if (oResults == null)
                {
                    ErrorInfo oError = new();
                    oResults = new ResultReturn();
                    oError.errorNum = 1;
                    oError.errorDesc = "No result returned by MessageService.";
                }

            }

            return oResults;
        }
        private ResultReturn SendEmailNotification_Custom(int iMsg_Id, string sContract_id, string sSub_Id, string sData_To_Consolidate, string sMessage_Variables, string sCustom_emails, string sCustom_MsgcenterIds, bool bClientContactsReplaced)
        {
            ResultReturn oResults = null;
            MessageServiceKeyValue[] Keys = null;
            string sKeyName_email = "to_email";
            string sKeyName_MsgCtr = "to_MessageCenter";

            if (iMsg_Id == 0)
            {
                oResults = new ResultReturn();
                oResults.returnStatus = ReturnStatusEnum.Succeeded;
                //return oResults;
            }
            else
            {
                if (sCustom_emails != null)
                {
                    sCustom_emails = sCustom_emails.Trim();
                }

                if (sCustom_MsgcenterIds != null)
                {
                    sCustom_MsgcenterIds = sCustom_MsgcenterIds.Trim();
                }

                MessageServiceKeyValue nKey = new();
                nKey.key = "Consolidated_Data";
                nKey.value = sData_To_Consolidate;

                try
                {
                    if (sMessage_Variables != "")
                    {
                        Keys = (MessageServiceKeyValue[])TRSManagers.XMLManager.DeserializeXml(sMessage_Variables, typeof(MessageServiceKeyValue[]));
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                }

                if (Keys != null && Keys.GetLength(0) > 0)
                {
                    Array.Resize(ref Keys, Keys.GetLength(0) + 1); // resize to add consolidated data key
                    Keys[Keys.GetLength(0) - 1] = nKey; //add key at last position.
                }
                else // send only consolidated data
                {
                    Keys = new MessageServiceKeyValue[1];
                    Keys[0] = nKey;
                }


                switch (iMsg_Id)
                {
                    case 550:
                    case 1460:
                    case 2990:
                    case 3050:
                    case 3060:
                        sKeyName_email = "primary_contact";
                        sKeyName_MsgCtr = "primary_contact_MessageCenter";
                        break;
                    default:
                        sKeyName_email = "to_email";
                        sKeyName_MsgCtr = "to_MessageCenter";
                        break;
                }

                if (!string.IsNullOrEmpty(sCustom_emails))
                {
                    MessageServiceKeyValue nKey_emails = new();
                    nKey_emails.key = sKeyName_email;
                    nKey_emails.value = sCustom_emails;

                    Array.Resize(ref Keys, Keys.GetLength(0) + 1); // resize to add custom emails data key
                    Keys[Keys.GetLength(0) - 1] = nKey_emails; //add key at last position.

                }

                if (!string.IsNullOrEmpty(sCustom_MsgcenterIds))
                {
                    MessageServiceKeyValue nKey_MsgCtr = new();
                    nKey_MsgCtr.key = sKeyName_MsgCtr;
                    nKey_MsgCtr.value = sCustom_MsgcenterIds;

                    Array.Resize(ref Keys, Keys.GetLength(0) + 1); // resize to add custom Message center Ids data key
                    Keys[Keys.GetLength(0) - 1] = nKey_MsgCtr; //add key at last position.
                }

                if (sSub_Id != "000" && ((string.IsNullOrEmpty(sCustom_emails) && string.IsNullOrEmpty(sCustom_MsgcenterIds)) || bClientContactsReplaced == true))
                {
                    switch (iMsg_Id)
                    {
                        case 550:
                        case 1460:
                        case 2990:
                        case 3050:
                        case 3060:
                            MessageServiceKeyValue nKey_reason = new();
                            nKey_reason.key = "reason";
                            nKey_reason.value = "<p>This message is being delivered to you because of one or more of the following reasons:</p><ul><li>There is no designation for both the Primary and Executive Contact for the Adopting Employer</li><li>Neither the Primary or Executive Contact for the Adopting Employer has an email address on file</li><li>Neither the Primary nor Executive Contact for the Adopting Employer has logged into www.TA-Retirement.com</li></ul> ";

                            Array.Resize(ref Keys, Keys.GetLength(0) + 1); // resize to add custom Message center Ids data key
                            Keys[Keys.GetLength(0) - 1] = nKey_reason; //add key at last position.

                            sSub_Id = "000"; // NOTE: override sub_id and try to send at 000 level (make sure message template is compatible i.e. message template is using defind veriable like primary_contact as recepient)
                            break;
                        default:
                            //should we  override sub_id for all?
                            //sSub_Id = "000"; // NOTE: override sub_id and try to send at 000 level (make sure message template is compatible i.e. message template is using defind veriable like primary_contact as recepient)                            
                            break;
                    }

                }

                MessageService oMS = new();
                oResults = oMS.SendMessage(sContract_id, sSub_Id, iMsg_Id, Keys, "TRS-Auto-Message-Service");

                if (oResults == null)
                {
                    ErrorInfo oError = new();
                    oResults = new ResultReturn();
                    oError.errorNum = 1;
                    oError.errorDesc = "No result returned by MessageService.";
                }

            }

            return oResults;
        }
        private string GetKeyValue(string sKey, List<SOAModel.KeyValue> oKeyValuePair)
        {
            string strValue = "";
            if ((oKeyValuePair != null))
            {
                var KeyVal = (from kv in oKeyValuePair
                              where kv.key.ToLower() == sKey.ToLower()
                              select kv.value).FirstOrDefault();

                if ((KeyVal != null))
                {
                    strValue = KeyVal.ToString();
                }

            }

            return strValue;
        }

        #region "*** Notification preferences (Custom contacts) ***"
        private int GetCustomContactInfo(string contract_id, string sub_id, int DocType_id, string sLoginType, ref string sCustom_emails, ref string sCustom_MsgcenterIds, ref bool bClientContactsReplaced)
        {
            bClientContactsReplaced = false;
            DataSet ds = null;
            DataTable dtInd = null;
            DataTable dtCust = null;
            int iCustom_Recipient_Type = 0;
            string sSId_custom = "";
            string semails_byIndivIds = ""; string sMsgcenterIds_byIndivIds = ""; string semails_byCtTypes = ""; string sMsgcenterIds_byCtTypes = "";
            ds = _oConsolidatedNotificationsDC.GetCustomContactDetails(contract_id, sub_id, DocType_id, sLoginType);

            if (ds != null && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Rows[0]["Custom_Recipient_Type"] != null)
            {
                iCustom_Recipient_Type = Convert.ToInt32(ds.Tables[0].Rows[0]["Custom_Recipient_Type"].ToString());
                if (ds.Tables[0].Rows[0]["sub_id"] != null)
                {
                    sSId_custom = ds.Tables[0].Rows[0]["sub_id"].ToString();
                }
            }

            // Note: other login types are not imlpemented. And in SaveToContractMessageQueue() method we save additional data row depending upon iCustom_Recipient_Type, so here we have to make sure we dont send overlapping contactdetails back to calling method
            if (iCustom_Recipient_Type > 0 && sLoginType.ToUpper() == "SPONSOR") // we will not call this function if iCustom_Recipient_Type <= 0 but  checking it here again just to be full proof
            {
                SOAModel.ContractInfo oConInfo;
                oConInfo = GetContractInfoFromSRV(contract_id, sub_id);
                if (ds.Tables.Count > 1 && (iCustom_Recipient_Type == 1 || iCustom_Recipient_Type == 3))// by individual or by both individual and contact type
                {   //should following apply to all doctypes?
                    dtInd = ds.Tables[1]; // 2nd table contains individual_ids

                    if (sub_id == "000" || sSId_custom == sub_id) //We want to send a only one consolidated email for  000 level contacts so ignore this for type 1 and 3 when sub_id != 000
                    {
                        bool b = GetCustomContactsIndividualIdDetails(oConInfo, dtInd, DocType_id, ref semails_byIndivIds, ref sMsgcenterIds_byIndivIds);
                    }
                }

                if (ds.Tables.Count > 2 && (iCustom_Recipient_Type == 2 || iCustom_Recipient_Type == 3 || iCustom_Recipient_Type == 4))//2 = By Contact Types of self,  3 = By BOTH Individual_ids at 000 level AND Adopting Employer Contact type (for MEP)  4 = By Adopting Employer Contact type only (for MEP)
                {
                    dtCust = ds.Tables[2]; // 3rd table contains contact types
                    if (iCustom_Recipient_Type == 2)
                    {
                        bool b1 = GetCustomContactsContactTypesDetails(oConInfo, dtCust, DocType_id, ref semails_byCtTypes, ref sMsgcenterIds_byCtTypes);
                    }
                    else // 3 or 4 
                    {
                        if (sub_id != "000") //We want to send a only one consolidated email for  000 level contacts so ignore this for type 3 and 4 OR in other words  iCustom_Recipient_Type = 3 is not a valid value for non 000 level in set up table.
                        {
                            bool b2 = GetCustomContactsContactTypesDetails(oConInfo, dtCust, DocType_id, ref semails_byCtTypes, ref sMsgcenterIds_byCtTypes);
                            if (iCustom_Recipient_Type == 3 && (semails_byCtTypes == "" || sMsgcenterIds_byCtTypes == ""))
                            {
                                // if none of the MEP custom contacts have email ids then send this message to custom contact at 000 level
                                SOAModel.ContractInfo oConInfo3;
                                oConInfo3 = GetContractInfoFromSRV(contract_id, "000");
                                semails_byCtTypes = ""; sMsgcenterIds_byCtTypes = "";
                                bool b3 = GetCustomContactsIndividualIdDetails(oConInfo3, dtInd, DocType_id, ref semails_byCtTypes, ref sMsgcenterIds_byCtTypes);
                                bClientContactsReplaced = true;
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(semails_byCtTypes))
            {
                if (!string.IsNullOrEmpty(semails_byIndivIds))
                {
                    sCustom_emails = semails_byIndivIds + ";" + semails_byCtTypes;
                }
                else
                {
                    sCustom_emails = semails_byCtTypes;
                }

            }
            else
            {
                sCustom_emails = semails_byIndivIds;
            }

            if (!string.IsNullOrEmpty(sMsgcenterIds_byCtTypes))
            {
                if (!string.IsNullOrEmpty(sMsgcenterIds_byIndivIds))
                {
                    sCustom_MsgcenterIds = sMsgcenterIds_byIndivIds + ";" + sMsgcenterIds_byCtTypes;
                }
                else
                {
                    sCustom_MsgcenterIds = sMsgcenterIds_byCtTypes;
                }

            }
            else
            {
                sCustom_MsgcenterIds = sMsgcenterIds_byIndivIds;
            }

            return iCustom_Recipient_Type;
        }
        private bool GetCustomContactsIndividualIdDetails(SOAModel.ContractInfo ContractInfo, DataTable dtCustContactsIndividualId, int iDocType_id, ref string semails_byIndivIds, ref string sMsgcenterIds_byIndivIds)
        {
            //dtCustContactsIndividualId is table with individual_id column
            bool bRet = false;
            const string NameDelimiter = "|";
            int iNum = 0;
            string sName = "";

            SOAModel.PlanContactInfo oPlanContact = default(SOAModel.PlanContactInfo);

            if ((ContractInfo != null) && (ContractInfo.PlanContacts != null) && ContractInfo.PlanContacts.Count > 0 && ((dtCustContactsIndividualId != null) && dtCustContactsIndividualId.Rows.Count > 0))
            {
                for (iNum = 0; iNum <= ContractInfo.PlanContacts.Count - 1; iNum++)
                {
                    oPlanContact = ContractInfo.PlanContacts[iNum];

                    // IndividualID, WebInLoginID and Email is must
                    if (oPlanContact.IndividualID != 0 &&
                        oPlanContact.WebInLoginID != null && oPlanContact.WebInLoginID.Trim() != "" && oPlanContact.WebInLoginID.Trim() != "0" &&
                        oPlanContact.Email != null && oPlanContact.Email.Trim() != "")
                    {
                        DataRow[] drS = dtCustContactsIndividualId.Select(" individual_id = " + oPlanContact.IndividualID);

                        if (drS != null && drS.Length > 0)
                        {
                            sName = "";

                            //if (CheckUserComPref(iDocType_id, Convert.ToInt32(oPlanContact.WebInLoginID))) // if user opted out from getting email from personal profile communication pref then do not send email
                            //{

                            if (semails_byIndivIds == "")
                            {
                                semails_byIndivIds = oPlanContact.Email.Trim();
                            }
                            else
                            {
                                if (semails_byIndivIds.IndexOf(oPlanContact.Email.Trim()) < 0)
                                {
                                    semails_byIndivIds = semails_byIndivIds + ";" + oPlanContact.Email.Trim();
                                }
                            }
                            //}
                            if (!string.IsNullOrEmpty(oPlanContact.MI))
                            {
                                sName = oPlanContact.FirstName.Trim() + " " + oPlanContact.MI.Trim() + " " + oPlanContact.LastName.Trim();
                            }
                            else
                            {
                                sName = oPlanContact.FirstName.Trim() + " " + oPlanContact.LastName.Trim();
                            }
                            sName = sName.Trim();
                            if (string.IsNullOrEmpty(sName))
                            {
                                sName = "Name not found";
                            }

                            if (sMsgcenterIds_byIndivIds == "")
                            {
                                sMsgcenterIds_byIndivIds = oPlanContact.WebInLoginID.ToString() + NameDelimiter + sName;
                            }
                            else
                            {
                                if (sMsgcenterIds_byIndivIds.IndexOf(oPlanContact.WebInLoginID.ToString() + NameDelimiter + sName) < 0)
                                {
                                    sMsgcenterIds_byIndivIds = sMsgcenterIds_byIndivIds + ";" + oPlanContact.WebInLoginID.ToString() + NameDelimiter + sName;
                                }
                            }

                            bRet = true;

                        }

                    }
                }
            }

            return bRet;
        }
        private bool GetCustomContactsContactTypesDetails(SOAModel.ContractInfo ContractInfo, DataTable dtCustContactsContactType, int iDocType_id, ref string semails_byCtTypes, ref string sMsgcenterIds_byCtTypes)
        {
            //dtCustContactsContactType is table with contact_type  column
            bool bRet = false;
            const string NameDelimiter = "|";

            string sName = "";
            List<SOAModel.PlanContactInfo> oCt;

            if ((ContractInfo != null) && (ContractInfo.PlanContacts != null) && ContractInfo.PlanContacts.Count > 0 && ((dtCustContactsContactType != null) && dtCustContactsContactType.Rows.Count > 0))
            {
                foreach (DataRow drCon in dtCustContactsContactType.Rows)
                {
                    oCt = GetContactsByContactType(ContractInfo, (E_ContactType)Convert.ToInt32(drCon["contact_type"].ToString()));
                    if (oCt != null && oCt.Count > 0)
                    {
                        foreach (SOAModel.PlanContactInfo oPlanContact in oCt)
                        {
                            if (oPlanContact.IndividualID != 0 &&
                                oPlanContact.WebInLoginID != null && oPlanContact.WebInLoginID.Trim() != "" && oPlanContact.WebInLoginID.Trim() != "0" &&
                                oPlanContact.Email != null && oPlanContact.Email.Trim() != "")
                            {
                                sName = "";
                                //if (CheckUserComPref(iDocType_id, Convert.ToInt32(oPlanContact.WebInLoginID))) // if user opted out from getting email from personal profile communication pref then do not send email
                                //{
                                if (semails_byCtTypes == "")
                                {
                                    semails_byCtTypes = oPlanContact.Email.Trim();
                                }
                                else
                                {
                                    if (semails_byCtTypes.IndexOf(oPlanContact.Email.Trim()) < 0)
                                    {
                                        semails_byCtTypes = semails_byCtTypes + ";" + oPlanContact.Email.Trim();
                                    }
                                }
                                //}

                                if (!string.IsNullOrEmpty(oPlanContact.MI))
                                {
                                    sName = oPlanContact.FirstName.Trim() + " " + oPlanContact.MI.Trim() + " " + oPlanContact.LastName.Trim();
                                }
                                else
                                {
                                    sName = oPlanContact.FirstName.Trim() + " " + oPlanContact.LastName.Trim();
                                }

                                sName = sName.Trim();
                                if (string.IsNullOrEmpty(sName))
                                {
                                    sName = "Name not found";
                                }

                                if (sMsgcenterIds_byCtTypes == "")
                                {
                                    sMsgcenterIds_byCtTypes = oPlanContact.WebInLoginID.ToString() + NameDelimiter + sName;
                                }
                                else
                                {
                                    if (sMsgcenterIds_byCtTypes.IndexOf(oPlanContact.WebInLoginID.ToString() + NameDelimiter + sName) < 0)
                                    {
                                        sMsgcenterIds_byCtTypes = sMsgcenterIds_byCtTypes + ";" + oPlanContact.WebInLoginID.ToString() + NameDelimiter + sName;
                                    }
                                }

                                bRet = true;
                            }


                        }
                    }
                    else
                    {
                        //???
                    }

                }
            }


            return bRet;

        }
        private bool IsCustomContactEnabled(int iDocTypeId)
        {
            bool bRet = false;

            string sCustomContactEnabled_docTyes = "9999, 10010, 36500, 36510, 36520, 678, 65300, 65310, 65330, 65350"; // can be moved to config file
            sCustomContactEnabled_docTyes = TrsAppSettings.AppSettings.GetValue("CustomContactEnabled_DocTypes");
            if (string.IsNullOrEmpty(sCustomContactEnabled_docTyes))
            {
                sCustomContactEnabled_docTyes = "9999, 10010, 36500, 36510, 36520, 678, 65300, 65310, 65330, 65350"; // existing
            }

            string[] sAryDocTypes = sCustomContactEnabled_docTyes.Split(',');
            if (sAryDocTypes != null && sAryDocTypes.Length > 0)
            {
                int[] iAryDocTypes = Array.ConvertAll(sAryDocTypes, int.Parse);

                if (iAryDocTypes != null && iAryDocTypes.Length > 0 && iAryDocTypes.Contains(iDocTypeId))
                {
                    bRet = true;
                }
            }

            return bRet;
        }
        private string FormatDataForMsgId2990(int iMessageTemplateId, string sDataToConsolidate) //IT-87084
        {
            sDataToConsolidate = sDataToConsolidate.Replace("&", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;");

            string sXML = "<root>" + sDataToConsolidate + "</root>";
            string sKey = "";
            string sOut = "";
            if (iMessageTemplateId == 2990)
            {
                try
                {
                    XElement xElTbl = XElement.Parse(sXML);
                    IEnumerable<XElement> xElRows = from row in xElTbl.Descendants("tr") select row;
                    Hashtable htDistContracts = new();
                    List<string> lstToSort = new();
                    foreach (XElement xEltr in xElRows)
                    {
                        sKey = "";
                        //sKey = xEltr.Elements("td").First().ToString() + xEltr.Elements("td").Skip(1).First().ToString();
                        sKey = xEltr.Elements("td").Skip(1).First().ToString() + xEltr.Elements("td").First().ToString();

                        if (htDistContracts.ContainsKey(sKey))
                        {
                            htDistContracts[sKey] = htDistContracts[sKey].ToString() + ", " + xEltr.Elements("td").Skip(2).First().Value;
                        }
                        else
                        {
                            lstToSort.Add(sKey);
                            htDistContracts.Add(sKey, xEltr.Elements("td").Skip(2).First().Value);
                        }
                    }
                    lstToSort.Sort();
                    foreach (string strKey in lstToSort)
                    {
                        if (htDistContracts[strKey] != null)
                        {
                            sOut += "<tr>" + strKey + "<td>" + (string)htDistContracts[strKey] + "</td></tr>";
                        }
                    }

                    sOut = sOut.Replace("&amp;", "&").Replace("&quot;", "\"").Replace("&apos;", "'");
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    sOut = sDataToConsolidate;
                }
            }
            else
            {
                sOut = sDataToConsolidate;
            }
            return sOut;

        }
        #endregion

    }
}