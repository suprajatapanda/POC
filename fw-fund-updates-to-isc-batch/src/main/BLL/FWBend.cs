using System.Data;
using System.Text;
using System.Xml.Linq;
using TRS.IT.TrsAppSettings;
using TARSharedUtilLibUtil = TRS.IT.BendProcessor.Util;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using TARSharedUtilLibModel = TRS.IT.BendProcessor.Model;
using TARSharedUtilLibBLL = TRS.IT.BendProcessor.BLL;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
namespace FWFundUpdatesToISCBatch.BLL
{
    public class FWBend
    {
        readonly TARSharedUtilLibBLL.FWBend fWBend;
        TARSharedUtilLibBFLBLL.FundWizard oWF;
        public FWBend(TARSharedUtilLibBLL.FWBend obj) {
            fWBend = obj;
        }
        
        public TARSharedUtilLibModel.TaskStatus ProcessUpdatePartner_ISC()
        {
            TARSharedUtilLibModel.TaskStatus oTaskReturn = new TARSharedUtilLibModel.TaskStatus();
            TARSharedUtilLibModel.ResultReturn oReturn = new TARSharedUtilLibModel.ResultReturn();
            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
            TARSharedUtilLibModel.ResultReturn oRetISCFWRecord = new TARSharedUtilLibModel.ResultReturn();
            TARSharedUtilLibModel.ResultReturn oRetUpdatePartner = new TARSharedUtilLibModel.ResultReturn();
            TARSharedUtilLibModel.ResultReturn oRetPxCustXml = new TARSharedUtilLibModel.ResultReturn();
            int iAction;
            DateTime dtPegasysDt;
            DateTime dtTriggerDt;
            string sPartnerId = "";
            string sConId = "";
            string sSubId = "00000";
            string P3XML = "";
            int iCaseNo = 0;
            string sInfo = "";
            const string C_Task = "ProcessUpdatePartner_ISC";
            const string C_TaskName = TARSharedUtilLibModel.ConstN.C_TAG_P_O + C_Task + TARSharedUtilLibModel.ConstN.C_TAG_P_C;
            
            BFLModel.ContractsFundsInfo oContractFundsInfo = new BFLModel.ContractsFundsInfo();
            BFLModel.AddDeleteFundsInfo oAddDeleteFundsInfo;
            TRS.IT.BendProcessor.DAL.GeneralDC oGenDC = new TRS.IT.BendProcessor.DAL.GeneralDC();
            string sEmailStr = "";
            bool ConvertedCase = false;
            bool bPxISC = false;
            DataSet dsTask0;
            DataSet dsTask;
            try
            {
                oTaskReturn.retStatus = TARSharedUtilLibModel.TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    fWBend.InitTaskStatus(oTaskReturn, C_Task);
                    foreach (DataRow dr in fWBend.PendingFundChanges.Tables[1].Rows)
                    {
                        sEmailStr += "Have Pending fund changes.\n\r";
                        try
                        {
                            oRetISCFWRecord.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded; 
                            oRetUpdatePartner.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded; 
                            bPxISC = false;
                            dtPegasysDt = Convert.ToDateTime("01/01/1990"); 
                            dtTriggerDt = Convert.ToDateTime("01/01/1990");
                            iAction = (int)dr["change_type"];
                            sPartnerId = TARSharedUtilLibUtil.Utils.CheckDBNullStr(dr["partner_id"]);
                            sConId = dr["contract_id"].ToString();
                            sSubId = sSubId + oGenDC.SubOut(dr["sub_id"].ToString());
                            sSubId = sSubId.Substring(sSubId.Length - 5, 5);
                            iCaseNo = (int)dr["case_no"];
                            sInfo = "Contract: " + dr["contract_id"].ToString() + " SubId: " + dr["sub_id"].ToString() + " CaseNo: " + dr["case_no"].ToString() + " ";
                            sEmailStr += "Begin log for : " + sInfo + "\n\r";
                            switch (sPartnerId.ToUpper())
                            {
                                case TARSharedUtilLibModel.ConstN.C_PARTNER_ISC:
                                    dtPegasysDt = Convert.ToDateTime(dr["pegasys_dt"]);
                                    dtTriggerDt = Convert.ToDateTime(TARSharedUtilLibBFLBLL.FWUtils.GetNextBusinessDay((DateTime)dr["pegasys_dt"], -2));
                                    oWF = new TARSharedUtilLibBFLBLL.FundWizard(Guid.NewGuid().ToString(), sConId, dr["sub_id"].ToString());
                                    oWF.GetCaseNo(iCaseNo);

                                    if (iAction == (int)BFLModel.E_FW_ChangeType.REMOVE_PX || iAction == (int)BFLModel.E_FW_ChangeType.ADD_MA)
                                    {
                                        bPxISC = true;
                                    }
                                    else
                                    {
                                        string[] sPX = new string[1];
                                        sPX = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_portXpress_selected, oWF.PdfHeader);
                                        if (sPX[0] != string.Empty && sPX[0] == "true")
                                        {
                                            bPxISC = true;
                                        }
                                    }
                                    if ((DateTime.Today.CompareTo(dtTriggerDt.Date) == 0 || bPxISC == true))
                                    {
                                        oContractFundsInfo.AddDeleteFundsInfo ??= [];
                                        oRetISCFWRecord.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                                        dsTask0 = oWF.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.UpdatePegasys.GetHashCode());
                                        if (dsTask0.Tables[0].Rows.Count > 0 && Convert.ToInt32(dsTask0.Tables[0].Rows[0]["status"].ToString()) == 100)
                                        {
                                            dsTask = oWF.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.UpdatePartnerSystem.GetHashCode());
                                            if (dsTask.Tables[0].Rows.Count > 0 && Convert.ToInt32(dsTask.Tables[0].Rows[0]["status"].ToString()) == 100)
                                            {
                                                oRetISCFWRecord.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                                            }
                                            else
                                            {
                                                oAddDeleteFundsInfo = new BFLModel.AddDeleteFundsInfo();
                                                sEmailStr += "Going to get P3 XML" + "\n\r";

                                                oRetISCFWRecord = GetISCFWRecord(sConId, sSubId, iCaseNo, ref oAddDeleteFundsInfo, ConvertedCase, iAction, bPxISC);
                                                if (oRetISCFWRecord.returnStatus == TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
                                                {
                                                    sEmailStr += "Retrieved XML" + "\n\r";

                                                    oRetUpdatePartner = UpdatePartnerISC(sConId, sSubId, iCaseNo, TRS.IT.TRSManagers.XMLManager.GetXML(oAddDeleteFundsInfo));

                                                    if (oRetUpdatePartner.returnStatus == TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
                                                    {
                                                        if (oAddDeleteFundsInfo != null && oAddDeleteFundsInfo.ContractID != null)
                                                        {
                                                            oContractFundsInfo.AddDeleteFundsInfo.Add(oAddDeleteFundsInfo);
                                                            oTaskReturn.rowsCount += 1;
                                                        }
                                                        sEmailStr += "Calling  GeneratePXInvestmentMix" + "\n\r";
                                                        oRetPxCustXml = GeneratePXInvestmentMix(sConId, sSubId, iCaseNo, dtPegasysDt, oWF);

                                                        if (oRetPxCustXml.returnStatus != TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
                                                        {
                                                            sEmailStr += "Failed  GeneratePXInvestmentMix" + "\n\r";
                                                            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                                                            TARSharedUtilLibUtil.General.CopyResultError(oReturn, oRetPxCustXml);
                                                            fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, C_TaskName + " GeneratePXInvestmentMix failed : " + sInfo + TARSharedUtilLibUtil.General.ParseErrorText(oRetPxCustXml.Errors, "<br />"));
                                                        }
                                                    }
                                                    else
                                                    {
                                                        oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                                                        TARSharedUtilLibUtil.General.CopyResultError(oReturn, oRetUpdatePartner);
                                                        fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, C_TaskName + " Update the task value in db failed,  P3 XML will not be included for this case : " + sInfo + TARSharedUtilLibUtil.General.ParseErrorText(oRetUpdatePartner.Errors, "<br />"));
                                                    }
                                                }
                                                else
                                                {
                                                    sEmailStr += "No success return for ISC FW records for : " + sInfo + "\n\r";
                                                    oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                                                    TARSharedUtilLibUtil.General.CopyResultError(oReturn, oRetISCFWRecord);
                                                    fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, C_TaskName + " FAILED to get P3 XML for : " + sInfo + TARSharedUtilLibUtil.General.ParseErrorText(oRetISCFWRecord.Errors, "<br />"));
                                                }
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception exi)
                        {
                            TARSharedUtilLibUtil.Utils.LogError(exi);
                            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                            oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, sInfo + " - " + exi.Message, TARSharedUtilLibModel.ErrorSeverityEnum.ExceptionRaised));
                            fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, C_TaskName + " ExceptionRaised : " + sInfo + exi.Message);
                        }
                    }
                    try
                    {
                        String StartDate, EndDate;
                        sEmailStr += "Getting eDocs cases";
                        StartDate = DateTime.Now.AddDays(-1).ToString("MM/dd/yyyy") + " 7:00 pm";
                        EndDate = DateTime.Now.ToString("MM/dd/yyyy") + " 7:00 pm";
                        oContractFundsInfo = FWUtils.GeteDocsSubmittedFWCases(StartDate, EndDate, ref oContractFundsInfo);
                    }
                    catch (Exception exeDocs)
                    {
                        TARSharedUtilLibUtil.Utils.LogError(exeDocs);
                        oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, "ALL eDocs FW cases Failed" + " - " + exeDocs.Message, TARSharedUtilLibModel.ErrorSeverityEnum.ExceptionRaised));
                        fWBend.SendErrorEmailToUsers("ALL eDocs FW cases", "", 0, "", C_TaskName + " ALL eDocs FW cases Failed" + " - " + exeDocs.Message);
                    }

                    //Step 3: Create XML File and Drop at destination
                    try
                    {
                        if (oContractFundsInfo.AddDeleteFundsInfo != null && oContractFundsInfo.AddDeleteFundsInfo.Count >= 1)
                        {
                            P3XML = TRS.IT.TRSManagers.XMLManager.GetXML(oContractFundsInfo);
                            sEmailStr += "XML: " + P3XML + "\n\r";
                            string sFileName = AppSettings.GetValue("P3XMLDrop") + "FW_" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".xml";
                            string fmRet = TRS.IT.TRSManagers.FileManager.WriteRemoteFile(sFileName, P3XML.ToString(), false);
                            if (fmRet == "0")
                                sEmailStr += "P3 XML dropped successfully";
                            else
                            {
                                sEmailStr += "Error in dropping P3 XML : " + fmRet;
                                fWBend.SendErrorEmailToUsers("ALL ISC FW cases", "", 0, "", C_TaskName + " Critical Error: P3 XML file drop failed. Resolve ASAP. " + sEmailStr);
                            }
                        }
                        else
                            sEmailStr += "No P3 records for today";
                    }
                    catch (Exception exDrp)
                    {
                        TARSharedUtilLibUtil.Utils.LogError(exDrp);
                        oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, "ALL ISC FW cases Failed" + " - " + exDrp.Message, TARSharedUtilLibModel.ErrorSeverityEnum.ExceptionRaised));
                        fWBend.SendErrorEmailToUsers("ALL ISC FW cases", "", 0, "", C_TaskName + " Critical Error: P3 XML file drop failed. Resolve ASAP. " + exDrp.Message);
                    }

                }
                else
                {
                    sEmailStr += C_TaskName + " Task is not Active";
                }

                TARSharedUtilLibUtil.General.CopyResultError(oTaskReturn, oReturn);

                TARSharedUtilLibUtil.Utils.SendMail(AppSettings.GetValue(TARSharedUtilLibModel.ConstN.C_BPROCESSOR_EMAIL), AppSettings.GetValue("TRSWebDevelopment"), C_TaskName + " Logs", sEmailStr);

                if (oReturn.returnStatus != TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
                    oTaskReturn.retStatus = TARSharedUtilLibModel.TaskRetStatus.ToCompletionWithErr;

            }
            catch (Exception ex)
            {
                TARSharedUtilLibUtil.Utils.LogError(ex);
                sEmailStr += "Error : " + ex.Message;
                fWBend.InitTaskError(oTaskReturn, ex, true);
            }

            return oTaskReturn;
        }
        public TARSharedUtilLibModel.ResultReturn GetISCFWRecord(string a_sConId, string a_sSubId, int a_iCaseNo, ref BFLModel.AddDeleteFundsInfo oAddDeleteFundsInfo, Boolean ConvertedCase, int iAction, bool PXCase)
        {
            TARSharedUtilLibModel.ResultReturn oReturn = new TARSharedUtilLibModel.ResultReturn();
            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
            DataSet dsTask;
            string sInfo = "";
            BFLModel.AddFundsInfo oAddFundsInfo = new BFLModel.AddFundsInfo();
            BFLModel.TransferFundsInfo oTransferFundsInfo = new BFLModel.TransferFundsInfo();

            BFLModel.FWFundsInfo oFundInfo;
            BFLModel.TransferFundInfo oTransferFundInfo;
            bool PPT_PX;
            bool PPT_TDF;
            bool PPT_QDIA;

            string[] strTmp = new string[1];
            string[] strFTmp = new string[1];
            try
            {
                sInfo = "Contract: " + a_sConId + " SubId: " + a_sSubId + " CaseNo: " + a_iCaseNo.ToString() + " ";
                oWF = new TARSharedUtilLibBFLBLL.FundWizard(Guid.NewGuid().ToString(), a_sConId, a_sSubId);
                oWF.GetCaseNo(a_iCaseNo);
                oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                dsTask = oWF.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.UpdatePartnerSystem.GetHashCode());
                if (dsTask.Tables[0].Rows.Count > 0)
                {
                    oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                }
                else
                {
                    oAddDeleteFundsInfo.ContractID = a_sConId;
                    oAddDeleteFundsInfo.SubID = a_sSubId;
                    oAddDeleteFundsInfo.Type = "FW";
                    oAddDeleteFundsInfo.EffectiveDate = oWF.PegasysDate;
                    oAddDeleteFundsInfo.ProjectManager = oWF.PMName;

                    switch (iAction)
                    {
                        case (int)BFLModel.E_FW_ChangeType.REMOVE_PX:
                            BFLModel.Services Services = new BFLModel.Services();
                            strTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_removal, oWF.PdfHeader);
                            if (strTmp[0] != string.Empty && (String.Compare(strTmp[0].ToUpper(), "TRUE") == 0))
                                Services.DisablePX = "true";
                            oAddDeleteFundsInfo.Services = Services;
                            break;

                        case (int)BFLModel.E_FW_ChangeType.ADD_MA:
                            BFLModel.ManagedAdviceInstallInfo ManagedAdviceInstallInfo = new BFLModel.ManagedAdviceInstallInfo();
                            foreach (DataRow dr in oWF.ManagedAdvice.Rows)
                            {
                                if ((dr["ma_qdia"].ToString() != string.Empty) && (String.Compare(dr["ma_qdia"].ToString().ToUpper(), "TRUE") == 0))
                                    PPT_QDIA = true;
                                else
                                    PPT_QDIA = false;

                                if ((dr["ma_px"].ToString() != string.Empty) && (String.Compare(dr["ma_px"].ToString().ToUpper(), "TRUE") == 0))
                                    PPT_PX = true;
                                else
                                    PPT_PX = false;

                                if ((dr["ma_tdf"].ToString() != string.Empty) && (String.Compare(dr["ma_tdf"].ToString().ToUpper(), "TRUE") == 0))
                                    PPT_TDF = true;
                                else
                                    PPT_TDF = false;

                                if (PPT_QDIA)
                                    ManagedAdviceInstallInfo.InDefaultStrategy = "1";
                                else
                                    ManagedAdviceInstallInfo.InDefaultStrategy = "0";

                                switch (dr["ma_conversion_method"].ToString())
                                {
                                    case "VOLUNTARY":
                                        ManagedAdviceInstallInfo.OutofDefaultStrategy = "0";
                                        ManagedAdviceInstallInfo.FreeLookDays = "0";
                                        break;
                                    case "PARTICIPANTS":
                                    default:
                                        ManagedAdviceInstallInfo.FreeLookDays = "60";

                                        if ((PPT_PX && !PPT_QDIA && !PPT_TDF) || (PPT_PX && PPT_QDIA && !PPT_TDF))
                                            ManagedAdviceInstallInfo.OutofDefaultStrategy = "2";
                                        else if ((PPT_TDF && !PPT_QDIA && !PPT_PX) || (PPT_TDF && PPT_QDIA && !PPT_PX))
                                            ManagedAdviceInstallInfo.OutofDefaultStrategy = "4";
                                        else if (PPT_QDIA && !PPT_TDF && !PPT_PX)
                                            ManagedAdviceInstallInfo.OutofDefaultStrategy = "0";
                                        else if ((PPT_PX && PPT_TDF && !PPT_QDIA) || (PPT_PX && PPT_TDF && PPT_QDIA))
                                            ManagedAdviceInstallInfo.OutofDefaultStrategy = "3";
                                        break;
                                }

                                strTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_removal, oWF.PdfHeader);
                                if (strTmp[0] != string.Empty && (String.Compare(strTmp[0].ToUpper(), "TRUE") == 0))
                                    ManagedAdviceInstallInfo.PXDisablewithMA = "true";
                                else
                                    ManagedAdviceInstallInfo.PXDisablewithMA = "false";
                                ManagedAdviceInstallInfo.FeeStartDate = dr["ma_effective_date"].ToString();
                                ManagedAdviceInstallInfo.FeeTotalBasisPoints = (Convert.ToDouble(dr["ma_fee"]) * 100).ToString();

                                oAddDeleteFundsInfo.ManagedAdviceInstallInfo = ManagedAdviceInstallInfo;
                            }

                            break;
                    }
                    foreach (DataRow dr in oWF.NewFunds.Rows)
                    {
                        switch ((int)dr["action"])
                        {
                            case 1:
                                if (oAddFundsInfo.FundsInfo == null)
                                {
                                    oAddFundsInfo.FundsInfo = new List<BFLModel.FWFundsInfo>();
                                }

                                oFundInfo = new BFLModel.FWFundsInfo();
                                oFundInfo.FundID = dr["fund_id"].ToString();
                                if (ConvertedCase)
                                    oFundInfo.PartnerFundID = FWUtils.GetP3FundInfo(dr["fund_id"].ToString());
                                else
                                    oFundInfo.PartnerFundID = dr["partner_fund_id"].ToString();
                                oAddFundsInfo.FundsInfo.Add(oFundInfo);
                                break;
                            case 2:
                                if (oTransferFundsInfo.FundsInfo == null)
                                {
                                    oTransferFundsInfo.FundsInfo = new List<BFLModel.TransferFundInfo>();
                                }

                                oFundInfo = new BFLModel.FWFundsInfo();
                                oFundInfo.FundID = dr["fund_id"].ToString();
                                if (ConvertedCase)
                                    oFundInfo.PartnerFundID = FWUtils.GetP3FundInfo(dr["fund_id"].ToString());
                                else
                                    oFundInfo.PartnerFundID = dr["partner_fund_id"].ToString(); 

                                oTransferFundInfo = new BFLModel.TransferFundInfo();
                                oTransferFundInfo.FromFundID = dr["fund_id"].ToString();
                                oTransferFundInfo.FromPartnerFundID = oFundInfo.PartnerFundID;
                                oTransferFundInfo.ToFundID = dr["to_fund_id"].ToString();
                                if (ConvertedCase)
                                    oTransferFundInfo.ToPartnerFundID = FWUtils.GetP3FundInfo(dr["to_fund_id"].ToString());
                                else
                                    oTransferFundInfo.ToPartnerFundID = dr["to_partner_fund_id"].ToString();
                                oTransferFundsInfo.FundsInfo.Add(oTransferFundInfo);

                                break;
                        }
                    }
                    BFLModel.DefaultFundInfo oDefaultFundInfo = new BFLModel.DefaultFundInfo();
                    strTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_new, oWF.PdfHeader);
                    if (strTmp[0] != string.Empty)
                    {
                        BFLModel.DfltFundInfo oDfltFundInfo = new BFLModel.DfltFundInfo();
                        oDfltFundInfo.TransferDfltMny = "false";
                        string sQdia;
                        sQdia = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_qdia_answer, oWF.PdfHeader)[0];
                        if (sQdia == "Yes")
                            oDfltFundInfo.QDIA = "true";
                        else
                            oDfltFundInfo.QDIA = "false";

                        if (String.Compare(strTmp[1].ToUpper(), "PORTFOLIOXPRESS") == 0 && String.Compare(strTmp[0].ToUpper(), "-1") == 0)
                        {
                            oDfltFundInfo.FundID = "PX";
                            oDfltFundInfo.PartnerFundID = "PX";

                            string[] sFD = new string[1];
                            sFD = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_fiduciary_Name, oWF.PdfHeader);
                            if (sFD[0] != string.Empty)
                                oDfltFundInfo.Fiduciary = sFD[0];
                            else
                                oDfltFundInfo.Fiduciary = "Plan Sponsor";

                            oDfltFundInfo.PptAgree = FWUtils.GetParticipantAgreementCode(a_sConId, a_sSubId);
                        }
                        else if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_ManagedAdvice_Addition, oWF.PdfHeader)[0] == "true")
                        {
                            if (String.Compare(strTmp[1].ToUpper(), "MANAGEDADVICE") == 0 && String.Compare(strTmp[0].ToUpper(), "-2") == 0)
                            {
                                oDfltFundInfo.FundID = "MA";
                                oDfltFundInfo.PartnerFundID = "MA";
                                oDfltFundInfo.TransferDfltMny = "true";
                                oDfltFundInfo.QDIA = "true";
                            }
                            else
                            {
                                oDfltFundInfo.FundID = strTmp[0];
                                if (ConvertedCase)
                                    strTmp[0] = string.Empty;
                                else
                                    strTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_new_partner_id, oWF.PdfHeader);

                                if (strTmp[0] != string.Empty)
                                    oDfltFundInfo.PartnerFundID = strTmp[0];
                                else
                                    oDfltFundInfo.PartnerFundID = FWUtils.GetP3FundInfo(oDfltFundInfo.FundID);
                            }

                            oDfltFundInfo.PptAgree = "DCMA";
                        }
                        else
                        {
                            oDfltFundInfo.FundID = strTmp[0];
                            if (ConvertedCase)
                                strTmp[0] = string.Empty;
                            else
                                strTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_new_partner_id, oWF.PdfHeader);

                            if (strTmp[0] != string.Empty)
                                oDfltFundInfo.PartnerFundID = strTmp[0];
                            else
                                oDfltFundInfo.PartnerFundID = FWUtils.GetP3FundInfo(oDfltFundInfo.FundID);

                        }
                        string[] sTM = new string[1];
                        sTM = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_tmf_select, oWF.PdfHeader);
                        if (sTM[0] != string.Empty && sTM[0] == "Yes")
                            oDfltFundInfo.FundSeries = "true";
                        else
                            oDfltFundInfo.FundSeries = "false";
                        oDefaultFundInfo.FundsInfo = oDfltFundInfo;
                    }
                    oAddDeleteFundsInfo.DefaultFundInfo = oDefaultFundInfo;
                    BFLModel.ForfeitureFundInfo oForfeitureFundInfo = new BFLModel.ForfeitureFundInfo();
                    strFTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_forfeiture_fund, oWF.PdfHeader);
                    if (strFTmp[0] != string.Empty)
                    {
                        strTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_forfeiture_fund_new, oWF.PdfHeader);
                        if (strTmp[0] != string.Empty && strFTmp[0] != strTmp[0])
                        {
                            oFundInfo = new BFLModel.FWFundsInfo();
                            oFundInfo.FundID = strTmp[0];
                            if (ConvertedCase)
                                strTmp[0] = string.Empty;
                            else
                                strTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_forfeiture_fund_new_partner_id, oWF.PdfHeader);
                            if (strTmp[0] != string.Empty)
                                oFundInfo.PartnerFundID = strTmp[0];
                            else
                                oFundInfo.PartnerFundID = FWUtils.GetP3FundInfo(oFundInfo.FundID);
                            oForfeitureFundInfo.FundsInfo = oFundInfo;
                        }
                    }

                    oAddDeleteFundsInfo.ForfeitureFundInfo = oForfeitureFundInfo;
                    oAddDeleteFundsInfo.AddFundsInfo = oAddFundsInfo;
                    oAddDeleteFundsInfo.TransferFundsInfo = oTransferFundsInfo;
                }
            }
            catch (Exception ex)
            {
                TARSharedUtilLibUtil.Utils.LogError(ex);
                oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, sInfo + ex.Message, TARSharedUtilLibModel.ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }
        private TARSharedUtilLibModel.ResultReturn UpdatePartnerISC(string a_sConId, string a_sSubId, int a_iCaseNo, string RequestXML)
        {
            TARSharedUtilLibModel.ResultReturn oReturn = new TARSharedUtilLibModel.ResultReturn();
            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
            DataSet dsTask;
            string sInfo = "";
            oWF = new TARSharedUtilLibBFLBLL.FundWizard(Guid.NewGuid().ToString(), a_sConId, a_sSubId);
            try
            {
                sInfo = "Contract: " + a_sConId + " SubId: " + a_sSubId + " CaseNo: " + a_iCaseNo.ToString() + " ";
                oWF.GetCaseNo(a_iCaseNo);
                dsTask = oWF.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.UpdatePartnerSystem.GetHashCode());
                if (dsTask.Tables[0].Rows.Count > 0 && Convert.ToInt32(dsTask.Tables[0].Rows[0]["status"].ToString()) == 100)
                {
                    oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                }
                else
                {
                    oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                    oWF.InsertTaskUpdateISC(oReturn.returnStatus.ToString(), RequestXML);                            
                }
            }
            catch (Exception ex)
            {
                TARSharedUtilLibUtil.Utils.LogError(ex);
                oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, sInfo + ex.Message, TARSharedUtilLibModel.ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;

        }
        public TARSharedUtilLibModel.ResultReturn GeneratePXInvestmentMix(string a_sConId, string a_sSubId, int a_iCaseNo, DateTime dtPegasysDt, TARSharedUtilLibBFLBLL.FundWizard a_oFW)
        {
            TARSharedUtilLibModel.ResultReturn oReturn = new TARSharedUtilLibModel.ResultReturn();
            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
            string sError = String.Empty;
            StringBuilder sbError = new StringBuilder();
            bool bCustomPX = false;
            string sXMLInput = "";
            string sPXxmlOutput = "";
            string fiduciaryName, agreementCode, sglidePath;
            string sFilePath = "";
            string scustomPxInputXml = "";
            DataSet dsTask0;
            DataSet dsTask;
            try
            {
                sbError.AppendLine(" Trace: ");

                if ((TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_custom, a_oFW.PdfHeader)[0] == "true")) { bCustomPX = true; }

                if (bCustomPX != true || (a_oFW.PartnerId != "ISC"))
                {
                    oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                    return oReturn;
                }

                dsTask0 = a_oFW.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.UpdatePartnerSystem.GetHashCode());
                if (dsTask0.Tables[0].Rows.Count > 0 && Convert.ToInt32(dsTask0.Tables[0].Rows[0]["status"].ToString()) == 100)
                {
                    dsTask = a_oFW.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.SendCustomPXInvestmentMix.GetHashCode());
                    if (dsTask.Tables[0].Rows.Count > 0 && Convert.ToInt32(dsTask.Tables[0].Rows[0]["status"].ToString()) == 100)
                    {
                        oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                        return oReturn;
                    }
                    else
                    {
                        const int C_Status_0 = 0;
                        const int C_Status_100 = 100;

                        a_oFW.InsertTask(BFLModel.FundWizardInfo.FwTaskTypeEnum.SendCustomPXInvestmentMix, C_Status_0, [new XElement("Initiate")]);

                        TRS.IT.SI.Services.PXEngineSvc.PortfolioData oPortfolio = null;

                        fiduciaryName = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_fiduciary_Name, a_oFW.PdfHeader)[0];
                        agreementCode = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_AgreementCode, a_oFW.PdfHeader)[0];
                        sglidePath = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_glidepath, a_oFW.PdfHeader)[0];

                        XElement xEl = new XElement("CustomPx",
                                    new XElement("contractId", a_sConId),
                                    new XElement("subId", a_sSubId),
                                    new XElement("fiduciaryName", fiduciaryName),
                                    new XElement("agreementCode", agreementCode),
                                    new XElement("pegasysDt", dtPegasysDt.ToString()),
                                    new XElement("glidePath", sglidePath)
                                    );
                        scustomPxInputXml = xEl.ToString();
                        sbError.AppendLine(" scustomPxInputXml = " + scustomPxInputXml); 
                        sbError.AppendLine(" ");


                        sXMLInput = new FundWizard(a_oFW).GetInputXml_GetPxWithFunds(bCustomPX);
                        sbError.AppendLine(" sXMLInput for GetPxWithFunds= " + sXMLInput); sbError.AppendLine(" ");
                        var pxEngine = new TARSharedUtilLibBFLBLL.SOA.PXEngine();
                        sPXxmlOutput = pxEngine.GetPxWithFunds(a_sConId, a_sSubId, Convert.ToInt32(TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_glidepath, a_oFW.PdfHeader)[0]), sXMLInput);
                        sbError.AppendLine(" sPXxmlOutput from GetPxWithFunds= " + sPXxmlOutput); sbError.AppendLine(" ");

                        oPortfolio = (TRS.IT.SI.Services.PXEngineSvc.PortfolioData)TRS.IT.TRSManagers.XMLManager.DeserializeXml(sPXxmlOutput, typeof(TRS.IT.SI.Services.PXEngineSvc.PortfolioData));

                        sFilePath = new SOA.PXEngine(pxEngine).GeneratePxInvestmentMixXml(scustomPxInputXml, oPortfolio);
                        sbError.AppendLine(" sFilePath from GeneratePxInvestmentMixXml = " + sFilePath); sbError.AppendLine(" ");

                        if (!string.IsNullOrEmpty(sFilePath))
                        {
                            a_oFW.InsertTask(BFLModel.FundWizardInfo.FwTaskTypeEnum.SendCustomPXInvestmentMix, C_Status_100, [new XElement("FileName", sFilePath)]);
                            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                        }
                        else
                        {
                            sError = "GeneratePxInvestmentMixXml did not return file path";
                            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                            oReturn.confirmationNo = string.Empty;
                            oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, sError + sbError.ToString(), TARSharedUtilLibModel.ErrorSeverityEnum.Error));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TARSharedUtilLibUtil.Utils.LogError(ex);
                sError = ex.Message;
                oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, sError + "  " + sbError.ToString(), TARSharedUtilLibModel.ErrorSeverityEnum.ExceptionRaised));
            }

            try
            {
                if (oReturn.returnStatus != TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
                {
                    a_oFW.InsertTask(BFLModel.FundWizardInfo.FwTaskTypeEnum.SendCustomPXInvestmentMix, -1, [new XElement("Error", sError)]);
                }
            }
            catch (Exception ex)
            {
                TARSharedUtilLibUtil.Utils.LogError(ex);
                string stemp = ex.Message;
            }

            return oReturn;
        }
    }
}
