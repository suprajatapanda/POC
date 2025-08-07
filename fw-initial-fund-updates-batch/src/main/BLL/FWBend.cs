using System.Collections;
using System.Data;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using TARSharedUtilLibUtil = TRS.IT.BendProcessor.Util;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
using TRS.IT.TrsAppSettings;
using TARSharedUtilLibModel = TRS.IT.BendProcessor.Model;
using FWInitialFundUpdatesBatch.DAL;
using TARSharedUtilLibBLL = TRS.IT.BendProcessor.BLL;

namespace FWInitialFundUpdatesBatch.BLL;
public class FWBend
{
    readonly TARSharedUtilLibBLL.FWBend fWBend;
    TARSharedUtilLibBFLBLL.FundWizard oFW;
    public FWBend(TARSharedUtilLibBLL.FWBend obj)
    {
        fWBend = obj;
    }
    FWBendDC _oFWDC = new FWBendDC();
    string _sFWBendFromEmail = AppSettings.GetValue("FWBendEmailAddr");
    Hashtable _htFailedPegInitial = new Hashtable();
    public TARSharedUtilLibModel.TaskStatus ProcessUpdatePegasysInitial()
    {
        TARSharedUtilLibModel.TaskStatus oTaskReturn = new TARSharedUtilLibModel.TaskStatus();
        TARSharedUtilLibModel.ResultReturn oReturn;
        string sPartnerId, sConId,sSubId;
        int iCaseNo;
        DataSet dsSigned;
        const string C_Task = "ProcessUpdatePegasysInitial";
        const string C_TaskName = TARSharedUtilLibModel.ConstN.C_TAG_P_O + C_Task + TARSharedUtilLibModel.ConstN.C_TAG_P_C;
        string sApprovalNotificationEmail = AppSettings.GetValue("ApprovalNotificationEmail");
        string sNotifyWhenApproved = AppSettings.GetValue("NotifyWhenApproved");
        int iNumOfTries = Convert.ToInt32(AppSettings.GetValue("PegNumOfTries"));
        string sQdia;
        bool bSkip = false;
        bool bSkip_PxISC_QDIA = false;
        try
        {
            oTaskReturn.retStatus = TARSharedUtilLibModel.TaskRetStatus.NotRun;
            if (AppSettings.GetValue(C_Task) == "1" && IsDaytime())
            {
                fWBend.InitTaskStatus(oTaskReturn, C_Task);
                dsSigned = _oFWDC.GetSignPending();
                foreach (DataRow dr in dsSigned.Tables[0].Rows)
                {
                    sPartnerId = dr["partner_id"].ToString(); 
                    sConId = dr["contract_id"].ToString();
                    sSubId = dr["sub_id"].ToString();
                    iCaseNo = (int)dr["case_no"];
                    bSkip_PxISC_QDIA = false;
                    bSkip = false;
                    //check for failed cases
                    if (_htFailedPegInitial.ContainsKey(iCaseNo))
                    {
                        if ((int)_htFailedPegInitial[iCaseNo] > iNumOfTries)
                            bSkip = true;

                    }
                    if (!bSkip)
                    {
                        oFW = new TARSharedUtilLibBFLBLL.FundWizard(Guid.NewGuid().ToString(), sConId, sSubId);
                        oFW.GetCaseNo(iCaseNo);
                        oReturn = PegasysInitialCall(oFW);

                        if (oReturn.returnStatus == TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
                        {
                            string[] sPX = new string[1];
                            sPX = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_portXpress_selected, oFW.PdfHeader);
                            if (sPX[0] != string.Empty && sPX[0] == "true" && sPartnerId.ToUpper() == TARSharedUtilLibModel.ConstN.C_PARTNER_ISC)
                            {
                                bSkip_PxISC_QDIA = true;
                            }

                            sQdia = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_qdia_answer, oFW.PdfHeader)[0];
                            if (sQdia == "Yes" && bSkip_PxISC_QDIA == false)
                            {
                                for (int i = 0; i < 6; i++)
                                {
                                    Thread.Sleep(5000);    
                                }
                                
                                if (oFW == null)
                                    throw new Exception("Lost oFW object");
                                TARSharedUtilLibModel.ResultReturn oRQdia = fWBend.QDIANotice(oFW);
                                if (oRQdia.returnStatus != TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
                                {
                                    TARSharedUtilLibUtil.General.CopyResultError(oTaskReturn, oRQdia);
                                    oTaskReturn.fatalErrCnt += 1;
                                    fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, oRQdia.Errors[0].errorDesc + C_TaskName);
                                }
                            }
                            if (sNotifyWhenApproved == "1")
                            {
                                TARSharedUtilLibUtil.Utils.SendMail(_sFWBendFromEmail, sApprovalNotificationEmail, "Fund Change Approved - eSign "
                                    + sConId + "-" + sSubId + " #" + iCaseNo.ToString(), "");
                            }

                        }
                        else
                        {
                            if (_htFailedPegInitial.ContainsKey(iCaseNo))
                            {   
                                _htFailedPegInitial[iCaseNo] = (int)_htFailedPegInitial[iCaseNo] + 1;
                            }
                            else
                            {
                                _htFailedPegInitial.Add(iCaseNo, 1);
                            }
                            oTaskReturn.retStatus = TARSharedUtilLibModel.TaskRetStatus.ToCompletionWithErr;
                            TARSharedUtilLibUtil.General.CopyResultError(oTaskReturn, oReturn);
                            oTaskReturn.fatalErrCnt += 1;
                            fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, "# tries: " + _htFailedPegInitial[iCaseNo].ToString() + oReturn.Errors[0].errorDesc + C_TaskName);
                        }
                        oTaskReturn.rowsCount += 1;
                    }                    
                }
            }
        }
        catch (Exception ex)
        {
            TARSharedUtilLibUtil.Utils.LogError(ex);
            fWBend.InitTaskError(oTaskReturn, ex, true);
        }
        return oTaskReturn;
    }
    private TARSharedUtilLibModel.ResultReturn PegasysInitialCall(TARSharedUtilLibBFLBLL.FundWizard a_oFW)
    {
        string sInfo = "";
        TARSharedUtilLibModel.ResultReturn oReturn = new TARSharedUtilLibModel.ResultReturn();
        BFLModel.SIResponse oResponse;
        DataSet dsTask;
        try
        {
            sInfo = "Contract: " + a_oFW.ContractId + " SubId: " + a_oFW.SubId + " CaseNo: " + a_oFW.CaseNo.ToString() + " ";
            dsTask = a_oFW.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.PegasysFundActivated.GetHashCode());
            if (dsTask.Tables[0].Rows.Count > 0 && (int)dsTask.Tables[0].Rows[0][""] == 100)
            {
                oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
            }
            else
            {
                if (a_oFW.Action != 8)
                {
                    oResponse = new FundWizard(a_oFW).UpdateFundLineup();
                    if (oResponse.Errors[0].Number == 0)
                    {
                        oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                    }
                    else
                    {
                        oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(oResponse.Errors[0].Number,
                            sInfo + oResponse.Errors[0].Description, TARSharedUtilLibModel.ErrorSeverityEnum.Failed));
                    }
                }
                else
                {
                    oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                }
            }
        }
        catch (Exception ex)
        {
            TARSharedUtilLibUtil.Utils.LogError(ex);
            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
            oReturn.isException = true;
            oReturn.confirmationNo = string.Empty;
            oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, sInfo + ex.Message, TARSharedUtilLibModel.ErrorSeverityEnum.ExceptionRaised));
        }
        return oReturn;
    }

    private bool IsDaytime()
    {        
        return DateTime.Now.TimeOfDay >= new TimeSpan(6, 0, 0) && DateTime.Now.TimeOfDay <= new TimeSpan(18, 0, 0);
    }
}
