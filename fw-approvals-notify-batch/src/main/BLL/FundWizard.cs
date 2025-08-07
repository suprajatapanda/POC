using System.Xml.Linq;
using System.Xml.XPath;
using FwApprovalsNotificationBatch.DAL;
using TRS.IT.SOA.Model;
using TRS.IT.SI.BusinessFacadeLayer;
using MessageService = TRS.IT.SI.BusinessFacadeLayer.MessageService;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer.Model;

namespace FwApprovalsNotificationBatch.BLL
{
    public class FundWizard
    {
        TRS.IT.SI.BusinessFacadeLayer.FundWizard fundWizard;
        ContractDC _ContractDC;
        public FundWizard(TRS.IT.SI.BusinessFacadeLayer.FundWizard _fundWizard)
        {
            fundWizard = _fundWizard;
            _ContractDC = new ContractDC();

        }
        public int ExpirePendingByContract(int CaseNo,string _sConId,string _sSubId)
        {
            return _ContractDC.FwExpirePendingByContract(CaseNo, _sConId, _sSubId);
        }

        public SIResponse SendNotification(FundWizardInfo.fwNotification a_eNotificationType, int a_iNotifyType)
        {
            var oResponse = new SIResponse();
            var objMessageData = new MessageData();
            var objApprovedMessageData = new MessageData();
            var objMessageService = new MessageService();
            string sToEmail = "";
            string sToDesignatedEmail = "";
            string sReqUsername = "";
            string sLoginIdXML = "";
            string sToinLoginId = "";
            string sServicesForApproval = "";
            TRS.IT.SI.Services.wsMessage.webMessage oWebMessage = null;
            TRS.IT.SI.Services.wsMessage.MsgData oWebMessageData = null;
            var oDesignatedContacts = new List<PlanContactInfo>();


            objMessageData.MessageID = a_eNotificationType.GetHashCode();
            objMessageData.ContractID = fundWizard.ContractId;
            objMessageData.SubID = fundWizard.SubId;
            objMessageData.EImageOption = E_ImageOption.None;

            try
            {
                switch (a_eNotificationType)
                {
                    case FundWizardInfo.fwNotification.RequestForApproval:
                    case FundWizardInfo.fwNotification.RequestForApprovalReminder:
                    case FundWizardInfo.fwNotification.RequestForApprovalFinal:
                        {
                            string sExpireDate;
                            sToEmail = FWUtils.GetHdrData("plan_TnF_email", fundWizard.PdfHeader)[0];
                            sExpireDate = Convert.ToDateTime(FWUtils.GetHdrData("request_date", fundWizard.PdfHeader)[0]).AddDays(30d).ToLongDateString();
                            // DDEV-47749 Need to determine if this SendNotification request is triggered by NEW Fund wizard RemovePX/Add MA functionalities
                            if (fundWizard.isFWv2Impl)
                            {
                                objMessageData.MessageID = Convert.ToInt32((int)a_eNotificationType == 1970 ? 3510 : ((int)a_eNotificationType == 1990 ? 3520 : ((int)a_eNotificationType == 2000 ? 3530 : (int)a_eNotificationType)));
                                sServicesForApproval = fundWizard.isRemovePX ? "<li>Removal of <i>PortfolioXpress</i>&reg; from the Plan</li>" : "";
                                sServicesForApproval = sServicesForApproval + (fundWizard.isAddMA ? "<li>Addition of <i>Managed Advice</i>&reg; to the Plan</li>" : "");
                                sServicesForApproval = sServicesForApproval + (fundWizard.isInvestmentChangeRequested | fundWizard.isNewDefaultInvestmentChoiceRequested ? "<li>Changes to investment choices in the Plan</li>" : "");
                                objMessageData.EmailVariableContainer.Add("plan_name", fundWizard.ContractInfo.ContractName);
                                objMessageData.EmailVariableContainer.Add("contract_number", fundWizard.ContractId);
                                objMessageData.EmailVariableContainer.Add("sub_code", fundWizard.SubId);
                                objMessageData.EmailVariableContainer.Add("services_for_approval", sServicesForApproval);
                            }
                            // DDEV-47749 End
                            objMessageData.EmailVariableContainer.Add("email_to", sToEmail);
                            objMessageData.EmailVariableContainer.Add("expiration_date", sExpireDate);

                            // Message Center

                            foreach (string sIn in FWUtils.GetHdrData("plan_TnF_in_login_id", fundWizard.PdfHeader)[0].Split(';'))
                            {
                                if (!string.IsNullOrEmpty(sIn))
                                {
                                    sLoginIdXML += "<InLoginId>" + sIn + "</InLoginId>";
                                }
                            }


                            if (!string.IsNullOrEmpty(sLoginIdXML) & !fundWizard.isFWv2Impl)
                            {
                                oWebMessage = new TRS.IT.SI.Services.wsMessage.webMessage();
                                oWebMessageData = new TRS.IT.SI.Services.wsMessage.MsgData();

                                switch (a_eNotificationType)
                                {
                                    case FundWizardInfo.fwNotification.RequestForApproval:
                                        {
                                            oWebMessage.Subject = "Investment Choice Change Request for Approval";
                                            oWebMessageData.Body = "<br /><br />" + "You have received an investment choice change request for trustee or fiduciary approval.  Only one approval is required." + "  Please note, if your plan has multiple trustees and/or fiduciaries, the first party to approve the document will remove it from the queue and initiate processing of the change request page.<br />" + "<br />To view this request, go to the \"Add/Delete Investments\" page, which can be found under Plan Investments." + " This request will be available for review and approval for 30 days and will expire on " + sExpireDate + ". Please note that you will not be able to initiate any additional requests until the pending request is approved or denied." + "<br /><br /><br />Sincerely, </br>Transamerica Retirement Solutions";

                                            break;
                                        }
                                    case FundWizardInfo.fwNotification.RequestForApprovalReminder:
                                        {
                                            oWebMessage.Subject = "Reminder: Investment Choice Change Request for Approval";
                                            oWebMessageData.Body = "<div align=\"center\"><b>REMINDER</b></div>" + "You have received an investment choice change request for trustee or fiduciary approval.  Only one approval is required." + "  Please note, if your plan has multiple trustees and/or fiduciaries, the first party to approve the document will remove it from the queue and initiate processing of the change request page.<br />" + "<br />To view this request, go to the \"Add/Delete Investments\" page, which can be found under Plan Investments." + " This request will be available for review and approval for 30 days and will expire on " + sExpireDate + ". Please note that you will not be able to initiate any additional requests until the pending request is approved or denied." + "<br /><br /><br />Sincerely, </br>Transamerica Retirement Solutions";

                                            break;
                                        }
                                    case FundWizardInfo.fwNotification.RequestForApprovalFinal:
                                        {
                                            oWebMessage.Subject = "Final Reminder: Investment Choice Change Request for Approval";
                                            oWebMessageData.Body = "<div align=\"center\"><b>FINAL REMINDER</b></div>" + "You have received an investment choice change request for trustee or fiduciary approval.  Only one approval is required." + "  Please note, if your plan has multiple trustees and/or fiduciaries, the first party to approve the document will remove it from the queue and initiate processing of the change request page.<br />" + "<br />To view this request, go to the \"Add/Delete Investments\" page, which can be found under Plan Investments." + " This request will be available for review and approval for 30 days and will expire on " + sExpireDate + ". Please note that you will not be able to initiate any additional requests until the pending request is approved or denied." + "<br /><br /><br />Sincerely, </br>Transamerica Retirement Solutions";

                                            break;
                                        }
                                }
                            }

                            break;
                        }

                    case FundWizardInfo.fwNotification.ApprovalDenial:
                        {
                            int iTaskNo = Convert.ToInt32(fundWizard.CaseStatus == 4 ? FundWizardInfo.FwTaskTypeEnum.RequestDenied : FundWizardInfo.FwTaskTypeEnum.RequestApproved);
                            string sStatus = fundWizard.CaseStatus == 4 ? "denied" : "approved";
                            string sName = "";
                            string sDate = "";
                            var ds = fundWizard.GetTaskByTaskNo(iTaskNo);
                            var xEl = XElement.Load(new StringReader(Convert.ToString(ds.Tables[0].Rows[0]["task_data"])));
                            var xProfile = xEl.XPathSelectElement("//UserProfile");
                            sName = xProfile.Attribute("UserName").Value;
                            sDate = Convert.ToDateTime(xProfile.Attribute("CreateDt").Value).ToString("MM/dd/yyyy");
                            sToEmail = FWUtils.GetHdrData("request_email", fundWizard.PdfHeader)[0];
                            sToinLoginId = FWUtils.GetHdrData("request_in_login_id", fundWizard.PdfHeader)[0];
                            sReqUsername = FWUtils.GetHdrData("request_name", fundWizard.PdfHeader)[0];
                            // DDEV-47749 Need to determine if this SendNotification request is triggered by NEW Fund wizard RemovePX/Add MA functionalities

                            if (fundWizard.isFWv2Impl)
                            {
                                objMessageData.MessageID = Convert.ToInt32((int)a_eNotificationType == 2030 ? 3550 : (int)a_eNotificationType);
                                objMessageData.EmailVariableContainer.Add("plan_name", fundWizard.ContractInfo.PlanName);
                                objMessageData.EmailVariableContainer.Add("contract_number", fundWizard.ContractId);
                                objMessageData.EmailVariableContainer.Add("sub_code", fundWizard.SubId);
                                objMessageData.EmailVariableContainer.Add("decision", sStatus);
                                objMessageData.EmailVariableContainer.Add("trustee_or_fiduciary", sName);
                                objMessageData.EmailVariableContainer.Add("decision_date", sDate);
                                objMessageData.EmailVariableContainer.Add("to_inloginid", sToinLoginId);
                            }

                            objMessageData.EmailVariableContainer.Add("email_to", "");

                            if (sStatus.Equals("approved") && fundWizard.isAddMA | fundWizard.isRemovePX)
                            {
                                oDesignatedContacts = fundWizard.GetDesignatedContacts();
                                sToDesignatedEmail = sToEmail;
                                if (oDesignatedContacts.Count > 0)
                                {
                                    foreach (PlanContactInfo oContact in oDesignatedContacts)
                                    {
                                        if (!string.IsNullOrEmpty(oContact.Email))
                                        {
                                            sToDesignatedEmail = Convert.ToString(sToDesignatedEmail + (string.IsNullOrEmpty(sToDesignatedEmail) ? oContact.Email : ";" + oContact.Email));
                                        }
                                    }
                                    objApprovedMessageData.EmailVariableContainer.Add("plan_name", fundWizard.ContractInfo.PlanName);
                                    objApprovedMessageData.EmailVariableContainer.Add("contract_number", fundWizard.ContractId);
                                    objApprovedMessageData.EmailVariableContainer.Add("sub_code", fundWizard.SubId);
                                    objApprovedMessageData.EmailVariableContainer.Add("email_to", sToDesignatedEmail);
                                    objApprovedMessageData.MessageID = 3580;

                                    var serResponse = MessageService.MessageServiceSendEmail(objApprovedMessageData);
                                    if (serResponse.Errors[0].Number != 0)
                                    {
                                        oResponse.Errors[0].Description = serResponse.Errors[0].Description;
                                        oResponse.Errors[0].Number = -1;
                                    }

                                }
                            }

                            break;
                        }
                }

                var oNotSerResponse = MessageService.MessageServiceSendEmail(objMessageData);
                if (oNotSerResponse.Errors[0].Number != 0)
                {
                    oResponse.Errors[0].Description = oNotSerResponse.Errors[0].Description;
                    oResponse.Errors[0].Number = -1;
                }   

                switch (a_eNotificationType)
                {
                    case FundWizardInfo.fwNotification.RequestForApproval:
                    case FundWizardInfo.fwNotification.RequestForApprovalReminder:
                    case FundWizardInfo.fwNotification.RequestForApprovalFinal:
                        {
                            InsertNofiticationHistory(fundWizard.CaseNo, a_iNotifyType, sToEmail);
                            var xEmail = new XElement("Notification", new XAttribute("InloginId", FWUtils.GetHdrData("plan_TnF_in_login_id", fundWizard.PdfHeader)[0]), new XAttribute("ToEmail", sToEmail), new XElement("Error", oResponse.Errors[0].Description));
                            fundWizard.InsertTask((FundWizardInfo.FwTaskTypeEnum)(200 + a_iNotifyType), Convert.ToInt32(oResponse.Errors[0].Number == 0 ? 100 : -1), [xEmail]); // 200 is the offset value
                            break;
                        }

                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oResponse.Errors[0].Number = -1;
                oResponse.Errors[0].Description = ex.Message;
            }
            if (oResponse.Errors[0].Number != 0)
            {
                fundWizard.SendErrorNotification("", "Notification Error", oResponse.Errors[0].Description);
            }

            return oResponse;

        }

        public int InsertNofiticationHistory(int a_iCaseNo, int a_iNotifyType, string a_sTo)
        {
            return _ContractDC.FwInsertNotificationHistory(a_iCaseNo, a_iNotifyType, a_sTo);
        }
    }
}
