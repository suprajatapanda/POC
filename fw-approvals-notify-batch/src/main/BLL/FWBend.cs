using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FwApprovalsNotificationBatch.DAL;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.DAL;
using TRS.IT.BendScheduler.Model;
using TRS.IT.SI.BusinessFacadeLayer;
using TRS.IT.TrsAppSettings;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;


namespace FwApprovalsNotificationBatch.BLL
{
    public class FWBend
    {
        TRS.IT.BendProcessor.BLL.FWBend fWBend;
        public FWBend(TRS.IT.BendProcessor.BLL.FWBend obj)
        {
            fWBend = obj;
        }

        #region **** private members ***

        FWBendDC _oFWDC = new();

        #endregion

        public TaskStatus ProcessSendReminderNotifications()
        
        {
            TaskStatus oTaskReturn = new TaskStatus();
            ResultReturn oReturn;
            string sConId;
            string sSubId;
            int iCaseNo;
            int iNotifyType;
            TRS.IT.SI.BusinessFacadeLayer.FundWizard fundWiz;
            FundWizard oFW;
            BFLModel.SIResponse oResponse;
            DataSet ds;
            const string C_Task = "ProcessSendReminderNotification";
            const string C_TaskName = ConstN.C_TAG_P_O + C_Task + ConstN.C_TAG_P_C;
            string sInfo = "";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    fWBend.InitTaskStatus(oTaskReturn, C_Task);

                    ds = _oFWDC.GetReminder();

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        sInfo = "";
                        sConId = Utils.CheckDBNullStr(dr["contract_id"]);
                        sSubId = Utils.CheckDBNullStr(dr["sub_id"]);
                        iCaseNo = Utils.CheckDBNullInt(dr["case_no"]);
                        iNotifyType = 0;
                        sInfo = "Contract: " + sConId + " SubId: " + sSubId + " CaseNo: " + iCaseNo.ToString() + " ";
                        switch ((int)dr["notify_type"])
                        {
                            case 1: //initial
                                if ((int)dr["day_num"] >= 5)
                                    iNotifyType = 5;
                                break;
                            case 5:
                                if ((int)dr["day_num"] >= 11)
                                    iNotifyType = 10;
                                break;
                            case 10:
                                if ((int)dr["day_num"] >= 16)
                                    iNotifyType = 15;
                                break;
                            case 15:
                                if ((int)dr["day_num"] >= 21)
                                    iNotifyType = 20;
                                break;
                            case 20:
                                if ((int)dr["day_num"] >= 26)
                                    iNotifyType = 25;
                                break;
                            case 25: // final Reminder
                                if ((int)dr["day_num"] >= 28)
                                    iNotifyType = 31;
                                break;
                            case 31: // expired
                                if ((int)dr["day_num"] >= 30)
                                {
                                    iNotifyType = 0;
                                    
                                    fundWiz = new TRS.IT.SI.BusinessFacadeLayer.FundWizard(Guid.NewGuid().ToString(), sConId, sSubId);
                                    oFW = new FundWizard(fundWiz);
                                    fundWiz.GetCaseNo(iCaseNo);
                                    oFW.ExpirePendingByContract(fundWiz.CaseNo,fundWiz.ContractId,fundWiz.SubId);
                                }
                                break;
                        }

                        if (iNotifyType != 0)
                        {
                            //oFW = new FundWizard(Guid.NewGuid().ToString(), sConId, sSubId);
                            fundWiz = new TRS.IT.SI.BusinessFacadeLayer.FundWizard(Guid.NewGuid().ToString(), sConId, sSubId);
                            oFW = new FundWizard(fundWiz);
                            fundWiz.GetCaseNo(iCaseNo);
                            oResponse = oFW.SendNotification(iNotifyType == 31 ? BFLModel.FundWizardInfo.fwNotification.RequestForApprovalFinal : BFLModel.FundWizardInfo.fwNotification.RequestForApprovalReminder, iNotifyType);
                            if (oResponse.Errors[0].Number != 0)
                            {
                                oTaskReturn.warningCnt += 1;
                                oTaskReturn.errors.Add(new ErrorInfo(oResponse.Errors[0].Number, sInfo + oResponse.Errors[0].Description, ErrorSeverityEnum.Warning));
                                fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, "", sInfo + oResponse.Errors[0].Description + C_TaskName);
                            }
                            oTaskReturn.rowsCount += 1;
                        }
                    }

                    if (oTaskReturn.errors.Count > 0)
                        oTaskReturn.retStatus = TaskRetStatus.Warning;
                    else
                        oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                fWBend.InitTaskError(oTaskReturn, ex, true);
            }

            return oTaskReturn;
        }
    }
}
