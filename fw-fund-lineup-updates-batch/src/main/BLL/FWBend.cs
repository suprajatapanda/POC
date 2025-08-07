using System.Data;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
using TRS.IT.TrsAppSettings;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
using SIUtil;

namespace FWFundLineupUpdatesBatch.BLL
{
    public  class FWBend
    {
        TRS.IT.BendProcessor.BLL.FWBend fWBend;
        public FWBend(TRS.IT.BendProcessor.BLL.FWBend obj) 
        {
            fWBend = obj;
        }

        public TaskStatus ProcessUpdatePegasys()
        {
            TaskStatus oTaskReturn = new TaskStatus();
            ResultReturn oReturn;
            int iAction;
            DateTime dtPegasysDt;
            string sPartnerId;
            const string C_Task = "ProcessUpdatePegasys";
            const string C_TaskName = ConstN.C_TAG_P_O + C_Task + ConstN.C_TAG_P_C;
            bool bCallPegasysUpdate = false;

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    fWBend.InitTaskStatus(oTaskReturn, C_Task);
                    foreach (DataRow dr in fWBend.PendingFundChanges.Tables[0].Rows)
                    {
                        bCallPegasysUpdate = false;
                        iAction = (int)dr["change_type"];
                        dtPegasysDt = (DateTime)dr["pegasys_dt"];
                        sPartnerId = dr["partner_id"].ToString();
                        if (dtPegasysDt.CompareTo(DateTime.Today) == 0)
                        {
                            TARSharedUtilLibBFLBLL.FundWizard oWF = new TARSharedUtilLibBFLBLL.FundWizard(Guid.NewGuid().ToString(), dr["contract_id"].ToString(), dr["sub_id"].ToString());
                            oWF.GetCaseNo((int)dr["case_no"]);

                            if (oWF.NewFundsCustomPX != null && oWF.NewFundsCustomPX.Rows.Count > 0)
                            {
                                bCallPegasysUpdate = true;
                            }

                        }
                        if (((iAction > 1 && iAction < 4) || bCallPegasysUpdate == true) && dtPegasysDt.CompareTo(DateTime.Today) == 0) //must be today's date
                        {
                            oReturn = UpdatePegasys(dr["contract_id"].ToString(), dr["sub_id"].ToString(), (int)dr["case_no"]);
                            if (oReturn.returnStatus != ReturnStatusEnum.Succeeded)
                            {
                                oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                General.CopyResultError(oTaskReturn, oReturn);
                                oTaskReturn.fatalErrCnt += 1;
                                fWBend.SendErrorEmailToUsers(dr["contract_id"].ToString(), dr["sub_id"].ToString(), (int)dr["case_no"], sPartnerId, oReturn.Errors[0].errorDesc + C_TaskName);
                            }
                            oTaskReturn.rowsCount += 1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                fWBend.InitTaskError(oTaskReturn, ex, true);
            }
            return oTaskReturn;
        }

        private ResultReturn UpdatePegasys(string a_sConId, string a_sSubId, int a_iCaseNo)
        {
            ResultReturn oReturn = new ResultReturn();
            BFLModel.SIResponse oResponse;
            DataSet dsTask;
            TARSharedUtilLibBFLBLL.FundWizard oWF = new TARSharedUtilLibBFLBLL.FundWizard(Guid.NewGuid().ToString(), a_sConId, a_sSubId);
            string sInfo = "";
            try
            {
                sInfo = "Contract: " + a_sConId + " SubId: " + a_sSubId + " CaseNo: " + a_iCaseNo.ToString() + " ";
                oWF.GetCaseNo(a_iCaseNo);
                dsTask = oWF.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.PegasysFundDeactivated.GetHashCode());
                if (dsTask.Tables[0].Rows.Count > 0)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    oResponse = new FundWizard(oWF).UpdateFundLineupBend(string.Empty);
                    if (oResponse.Errors[0].Number == 0)
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                    }
                    else
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(oResponse.Errors[0].Number, sInfo + oResponse.Errors[0].Description, ErrorSeverityEnum.Failed));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, sInfo + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }
    }
}
