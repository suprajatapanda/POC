using System.Xml.Linq;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
using MO = TRS.IT.SI.BusinessFacadeLayer.Model;
using SIUtil;
using System.Data;

namespace FWFundLineupUpdatesBatch.BLL
{
    public class FundWizard
    {
        TARSharedUtilLibBFLBLL.FundWizard oFW;
        public FundWizard(TARSharedUtilLibBFLBLL.FundWizard obj)
        {
            oFW = obj;
        }
        public MO.SIResponse UpdateFundLineupBend(string a_sReason = "")
        {
            MO.FundWizardInfo.FmrsUpdateReturn oResult = null;
            var oResponse = new MO.SIResponse();
            var xElReason = new XElement("Reason", a_sReason);
            try
            {
                oResult = oFW.UpdateFMRS(2);
                if (oResult.xResult.Element("Wizard").Attribute("Error").Value == "0")
                {
                    if (oResult.xResult.Element("Validation").Attribute("Error").Value == "0" & oResult.xResult.Element("Pegasys").Attribute("Error").Value == "0")
                    {
                        oFW.InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.PegasysFundDeactivated, 100, [oResult.xInputXml, oResult.xOutputXml, oResult.xResult, xElReason]);
                        oFW._iCaseStatus = (int)MO.FundWizardInfo.fwCaseStatusEnum.FundChangeCompleted;
                        oResponse.Errors[0].Number = 0;

                        MO.SIResponse oResponseCustPX;
                        oResponseCustPX = SetCustomPxFunds();
                        if (oResponseCustPX.Errors[0].Number != 0)
                        {
                            SendErrorMessage(2840, "Error in SetCustomPxFunds() function which updates Custom portfoliioXpress lineup in eDocs and Partner.  Error Details:" + oResponseCustPX.Errors[0].Description); // new
                        }
                    }

                    else
                    {
                        string sErrorText = string.Empty;
                        if (oResult.xResult.Element("Validation").Attribute("Error").Value == "-1" & oResult.xResult.Element("Pegasys").Attribute("Error").Value == "-1")
                        {
                            sErrorText = "FMRS update failed, Validation and Pegasys update error.</br></br>" + oFW.GetPegasysMessage(oResult);
                        }
                        else if (oResult.xResult.Element("Validation").Attribute("Error").Value == "-1")
                        {
                            sErrorText = "FMRS validation failed.";
                        }
                        else if (oResult.xResult.Element("Pegasys").Attribute("Error").Value == "-1")
                        {
                            sErrorText = "FMRS Pegasys update failed. </br></br>" + oFW.GetPegasysMessage(oResult);
                        }
                        oFW.InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.PegasysFundDeactivated, -1, [oResult.xInputXml, oResult.xOutputXml, oResult.xResult, xElReason]);
                        oResponse.Errors[0].Number = -1;
                        oResponse.Errors[0].Description = sErrorText;
                        oFW.SendErrorEmail(oResult);
                    }
                }
                else
                {
                    oResponse.Errors[0].Number = -1;
                    oResponse.Errors[0].Description = oResult.xResult.Element("Wizard").Attribute("ErrorMsg").Value;
                    oFW.SendErrorEmail(oResult);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oResponse.Errors[0].Number = -1;
                oResponse.Errors[0].Description = ex.Message;
                if (oResult == null)
                {
                    oResult = new MO.FundWizardInfo.FmrsUpdateReturn();
                }
                oFW.SendErrorEmail(oResult);
            }

            return oResponse;
        }
        public MO.SIResponse SetCustomPxFunds()
        {
            var oFundInfo = new SOA.FundInfoSoa();
            var oResponse = new MO.SIResponse();
            int iStatus = 100;
            XElement xElInput = null;
            try
            {
                if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_portXpress_selected, oFW.PdfHeader)[0] == "true")
                {

                    xElInput = GetInput_SetCustomPxFunds();

                    if (xElInput.HasElements && xElInput.Elements("FundID").Count() > 0)
                    {

                        oResponse = oFundInfo.SetCustomPxFunds(oFW.ContractId, oFW.SubId, xElInput.ToString());

                        xElInput.Add(new XAttribute("eDocReturnStatus", oResponse.Errors[0].Number));
                        if (oResponse.Errors[0].Number != 0)
                        {
                            xElInput.Add(new XAttribute("eDocsReturnError", oResponse.Errors[0].Description));
                            iStatus = -1;
                        }
                        else
                        {
                            oFW.RefreshFMRS = true;
                        }
                        oFW.InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.PXCustomSelectionUpdate, iStatus, [xElInput]);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oResponse.Errors[0].Number = -1;
                oResponse.Errors[0].Description = "Exception in SetCustomPxFunds() " + ex.Message;
            }

            return oResponse;
        }
        private void SendErrorMessage(int a_iMsgId, string a_sBody, string a_sTo = "", string a_sSubject = "")
        {
            string sError = "";
            try
            {
                var objMessageData = new MO.MessageData();
                var objMessageService = new TARSharedUtilLibBFLBLL.MessageService(); // ?

                objMessageData.MessageID = a_iMsgId;
                objMessageData.ContractID = oFW.ContractId;
                objMessageData.SubID = oFW.SubId;
                objMessageData.EImageOption = MO.E_ImageOption.None;

                objMessageData.EmailVariableContainer.Add("details", a_sBody);

                objMessageData.EmailVariableContainer.Add("case_number", oFW.CaseNo);

                if (!string.IsNullOrEmpty(a_sTo))
                {
                    objMessageData.EmailVariableContainer.Add("email_to", a_sTo);
                }

                if (!string.IsNullOrEmpty(a_sSubject))
                {
                    objMessageData.Subject = a_sSubject;
                }

                var oNotSerResponse = TARSharedUtilLibBFLBLL.MessageService.MessageServiceSendEmail(objMessageData);

                if (oNotSerResponse.Errors[0].Number != 0)
                {
                    sError = oNotSerResponse.Errors[0].Description;
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                sError = ex.Message;
            }

            if (!string.IsNullOrEmpty(sError))
            {
                oFW.SendErrorNotification("", "Error in FundWizard.SendErrorMessage - MessageId = " + a_iMsgId + " : ContractID = " + oFW.ContractId + " - " + oFW.SubId, sError);
            }
        }
        private XElement GetInput_SetCustomPxFunds()
        {
            var xElOut = new XElement("CustomFunds");
            string sCustom = string.Empty;
            XElement xElCustom;
            bool bCopyAddedFunds = false;

            if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_portXpress_selected, oFW.PdfHeader)[0] == "true")
            {
                sCustom = GetCustomPxFunds();
                if (!string.IsNullOrEmpty(sCustom))
                {
                    xElCustom = XElement.Load(new StringReader(sCustom));

                    if (!(oFW.NewFundsCustomPX == null) && oFW.NewFundsCustomPX.Rows.Count > 0)
                    {
                        if (xElCustom.Elements("FundID").Count() > 0)
                        {
                            foreach (XElement xElem in xElCustom.Elements("FundID"))
                            {
                                var drC = oFW.NewFundsCustomPX.Rows.Find(Convert.ToInt32(xElem.Value));
                                if (drC == null || Convert.ToBoolean(drC["action"]) && Convert.ToInt32(drC["action"]) == 2)
                                {
                                    xElOut.Add(new XElement("FundID", xElem.Value));
                                }
                            }
                            bCopyAddedFunds = true;
                        }
                        else
                        {
                            bCopyAddedFunds = true;
                        }
                    }
                }
                else
                {
                    bCopyAddedFunds = true;
                }
            }
            if (bCopyAddedFunds == true && !(oFW.NewFundsCustomPX == null) && oFW.NewFundsCustomPX.Rows.Count > 0)
            {
                foreach (DataRow drN in oFW.NewFundsCustomPX.Rows)
                {
                    if (Convert.ToBoolean(drN["action"]) && Convert.ToInt32(drN["action"]) == 1)
                    {
                        xElOut.Add(new XElement("FundID", drN["fund_id"]));
                    }
                }
            }

            return xElOut;
        }
        public string GetCustomPxFunds()
        {
            var oFundInfo = new SOA.FundInfoSoa();
            return oFundInfo.GetCustomPxFunds(oFW.ContractId, oFW.SubId);
        }
    }
}
