using System.Data;
using TRS.IT.TrsAppSettings;
using TRS.IT.BendProcessor.Util;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
using TRS.IT.BendProcessor.Model;
using FWUpdateRKPartner.DAL;
using FWUpdateRKPartner.Utils;
using FWUpdateRKPartner.MessageQueue;

namespace FWUpdateRKPartner.BLL
{
    public class FWBend
    {
        string _sBCCEmailNotification = AppSettings.GetValue("BCCEmailNotification");
        TRS.IT.BendProcessor.BLL.FWBend fWBend;
        public FWBend(TRS.IT.BendProcessor.BLL.FWBend obj)
        {
            fWBend = obj;
        }

        public TaskStatus ProcessUpdatePartner()
        {
            TaskStatus oTaskReturn = new TaskStatus();
            ResultReturn oReturn;
            int iAction;
            DateTime dtPegasysDt;
            string sPartnerId;
            string? sConId;
            string? sSubId;
            int iCaseNo;
            const string C_Task = "ProcessUpdatePartner";
            const string C_TaskName = ConstN.C_TAG_P_O + C_Task + ConstN.C_TAG_P_C;

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    fWBend.InitTaskStatus(oTaskReturn, C_Task);
                    foreach (DataRow dr in fWBend.PendingFundChanges.Tables[0].Rows)
                    {
                        iAction = (int)dr["change_type"];
                        sPartnerId = TRS.IT.BendProcessor.Util.Utils.CheckDBNullStr(dr["partner_id"]);
                        sConId = dr["contract_id"].ToString();
                        sSubId = dr["sub_id"].ToString();
                        iCaseNo = (int)dr["case_no"];

                        dtPegasysDt = Convert.ToDateTime("01/01/1990");
                        switch (sPartnerId.ToUpper())
                        {
                            case ConstN.C_PARTNER_TAE:
                            case ConstN.C_PARTNER_PENCO:
                            case ConstN.C_PARTNER_TRS:
                                dtPegasysDt = Convert.ToDateTime(TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetNextBusinessDay((DateTime)dr["pegasys_dt"], -1));
                                break;
                        }
                        if (dtPegasysDt.CompareTo(DateTime.Today) == 0)
                        {
                            switch (sPartnerId.ToUpper())
                            {
                                case ConstN.C_PARTNER_TAE:
                                    if (AppSettings.GetValue("FWMQUpdate") != "1")
                                        break;
                                    oReturn = UpdatePartnerTAE(sConId, sSubId, iCaseNo);
                                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded)
                                    {
                                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                        General.CopyResultError(oTaskReturn, oReturn);
                                        fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, oReturn.Errors[0].errorDesc + C_TaskName);
                                        oTaskReturn.fatalErrCnt += 1;
                                    }
                                    oTaskReturn.rowsCount += 1;
                                    break;
                                case ConstN.C_PARTNER_PENCO:
                                case ConstN.C_PARTNER_TRS:
                                    oReturn = SendNoticeToPartner(sConId, sSubId, iCaseNo, TRS.IT.BendProcessor.Util.Utils.CheckDBNullStr(dr["plan_name"]));
                                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded)
                                    {
                                        General.CopyResultError(oTaskReturn, oReturn);
                                        fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, oReturn.Errors[0].errorDesc + C_TaskName);
                                        oTaskReturn.fatalErrCnt += 1;
                                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                    }
                                    oTaskReturn.rowsCount += 1;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TRS.IT.BendProcessor.Util.Utils.LogError(ex);
                fWBend.InitTaskError(oTaskReturn, ex, true);
            }

            return oTaskReturn;
        }
        private ResultReturn UpdatePartnerTAE(string? a_sConId, string? a_sSubId, int a_iCaseNo)
        {
            TaeMQ oMQ = new TaeMQ();
            ResultReturn oReturn = new ResultReturn();
            ResultReturn oMQReturn;
            DataSet dsTask;
            string sMsg;
            string? sAA = string.Empty;
            string sDD = string.Empty;
            string sTOTI = string.Empty;
            string sDF = string.Empty;
            string sFF = string.Empty;
            string sPlanId = string.Empty;
            string[] strTmp = new string[1];
            string sInfo = "";
            TRS.IT.SI.BusinessFacadeLayer.FundWizard oWF = new TRS.IT.SI.BusinessFacadeLayer.FundWizard(Guid.NewGuid().ToString(), a_sConId, a_sSubId);
            try
            {
                sInfo = "Contract: " + a_sConId + " SubId: " + a_sSubId + " CaseNo: " + a_iCaseNo.ToString() + " ";
                oWF.GetCaseNo(a_iCaseNo);
                dsTask = oWF.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.UpdatePartnerSystem.GetHashCode());
                if (dsTask.Tables[0].Rows.Count > 0)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    foreach (DataRow dr in oWF.NewFunds.Rows)
                    {
                        switch ((int)dr["action"])
                        {
                            case 1:
                                sAA += FWUtils.TAE2ByteFundId(dr["partner_fund_id"].ToString());
                                break;
                            case 2:
                                sDD += FWUtils.TAE2ByteFundId(dr["partner_fund_id"].ToString());
                                sTOTI += FWUtils.TAE2ByteFundId(dr["partner_fund_id"].ToString())
                                    + FWUtils.TAE2ByteFundId(dr["to_partner_fund_id"].ToString());
                                break;
                        }

                    }
                    strTmp = TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData("partner_plan_id", oWF.PdfHeader);
                    if (strTmp[0] != string.Empty)
                        sPlanId = strTmp[0];
                    else
                    {
                        sPlanId = oMQ.GetTaePlanId(a_sConId);
                        if (sPlanId == string.Empty)
                            throw new Exception("Missing partner_plan_id");
                    }
                    strTmp = TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_default_fund_new + "_partner_id", oWF.PdfHeader);
                    if (strTmp[0] != string.Empty)
                    {
                        string[] sTM = new string[1];
                        sTM = TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_default_fund_tmf_select, oWF.PdfHeader);
                        if (sTM[0] != string.Empty && sTM[0] == "Yes")
                        {
                            string[] sTMFundId = new string[1];
                            DataSet ds;

                            sTMFundId = TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_default_fund_new, oWF.PdfHeader);
                            ds = new FWBendDC().GetTmCode("TAE", Convert.ToInt32(sTMFundId[0]));
                            if ((ds.Tables[0].Rows.Count > 0))
                            {
                                sDF = "TM" + TRS.IT.BendProcessor.Util.Utils.CheckDBNullStr(ds.Tables[0].Rows[0]["tm"]);
                            }
                            else
                            {
                                throw new Exception("Missing TM Default");
                            }
                        }
                        else
                            sDF = strTmp[0];
                    }
                    else
                        sDF = "00";
                    strTmp = TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_forfeiture_fund_new + "_partner_id", oWF.PdfHeader);
                    if (strTmp[0] != string.Empty)
                        sFF = strTmp[0];
                    else
                        sFF = "00";

                    sMsg = sAA + "#" + sDD + "#" + sTOTI + "#" + sDF + "#" + sFF + "@@";
                    // TDB add a business day
                    oMQReturn = oMQ.UpdateFundChange(sPlanId, oWF.PegasysDate, oWF.PMName, sMsg);
                    if (oMQReturn.returnStatus == ReturnStatusEnum.Succeeded)
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;

                    }
                    else
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(oMQReturn.Errors[0].errorNum, sInfo + oMQReturn.Errors[0].errorDesc, ErrorSeverityEnum.Failed));
                    }
                    new FundWizard(oWF).InsertTaskUpdatePartnerSystem(oReturn.returnStatus.ToString(), oMQReturn.request, oMQReturn.response);
                }
            }
            catch (Exception ex)
            {
                TRS.IT.BendProcessor.Util.Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, sInfo + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }


        private ResultReturn SendNoticeToPartner(string? a_sConId, string? a_sSubId, int a_iCaseNo, string a_sPlanName)
        {
            ResultReturn oMappingReturn;
            ResultReturn oReturn = new ResultReturn();
            string sFrom;
            DataSet dsTask;
            TRS.IT.SI.BusinessFacadeLayer.FundWizard oWF = new TRS.IT.SI.BusinessFacadeLayer.FundWizard(Guid.NewGuid().ToString(), a_sConId, a_sSubId);
            string sInfo = "";

            try
            {
                sInfo = "Contract: " + a_sConId + " SubId: " + a_sSubId + " CaseNo: " + a_iCaseNo.ToString() + " ";
                oWF.GetCaseNo(a_iCaseNo);
                dsTask = oWF.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.SendNoticeToPartner.GetHashCode());
                if (dsTask.Tables[0].Rows.Count > 0)
                {
                    // return as succeeded for now
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    oMappingReturn = GenerateFundMappingSpreadsheet(oWF);
                    if (oMappingReturn.returnStatus == ReturnStatusEnum.Succeeded)
                    {
                        string sSubject = "";
                        string sBody = "";
                        string sMailBox = string.Empty;
                        sFrom = AppSettings.GetValue("FWBendEmailAddr");

                        FWUtils.GetPartnerEmail(oWF.PartnerId, a_sPlanName, oWF.ContractId, oWF.PMName,
                            Convert.ToDateTime(TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetNextBusinessDay(Convert.ToDateTime(oWF.PegasysDate), 1)), ref sSubject, ref sBody);
                        switch (oWF.PartnerId)
                        {
                            case ConstN.C_PARTNER_PENCO:
                                sMailBox = AppSettings.GetValue("FWCSCEmailAddr");
                                break;
                        }
                        TRS.IT.BendProcessor.Util.Utils.SendMail(sFrom, sMailBox, sSubject, sBody, new string[] { oMappingReturn.confirmationNo }, _sBCCEmailNotification);
                        var taskNumber = new FundWizard(oWF).InsertTaskSendNoticeToPartner(sMailBox, sSubject, sBody);

                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                    }
                    else
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(-1, sInfo + "Error Generating Fund Mapping Spreadsheet. (Error) " + oMappingReturn.Errors[0].errorDesc, ErrorSeverityEnum.Error));
                    }
                }
            }
            catch (Exception ex)
            {
                TRS.IT.BendProcessor.Util.Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, sInfo + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        private ResultReturn GenerateFundMappingSpreadsheet(TRS.IT.SI.BusinessFacadeLayer.FundWizard a_oFW)
        {
            ResultReturn oReturn = new ResultReturn();
            TRS.IT.SI.BusinessFacadeLayer.FWDocGen oFWDocGen = new TRS.IT.SI.BusinessFacadeLayer.FWDocGen(a_oFW);
            string sError = "";
            try
            {
                SetFWDocGenPaths(oFWDocGen);
                oReturn.confirmationNo = oFWDocGen.CreateFundMappingSpreadsheet(ref sError);
                if (!string.IsNullOrEmpty(sError))
                    throw new Exception(sError);
                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            }
            catch (Exception ex)
            {
                TRS.IT.BendProcessor.Util.Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;

        }
        private void SetFWDocGenPaths(TRS.IT.SI.BusinessFacadeLayer.FWDocGen a_oFWDocGen)
        {
            a_oFWDocGen.LicenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Aspose.Total.lic");
            a_oFWDocGen.OutputPath = AppSettings.GetValue("FWDocGenOutputPath");
            a_oFWDocGen.TemplatePath = AppSettings.GetValue("FWDocGenTemplatePath");
            a_oFWDocGen.LocalPath = AppSettings.GetValue("FWDocGenLocalPath");
        }


    }
}
