using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SIUtil;
using TarPlanAdminReportBatch.BLL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace TarPlanAdminReportBatch
{
    public class TpaPlanAdminReportsISC
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            try
            {
                if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                {
                    ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);
                    TRS.IT.BendProcessor.BLL.FWBend fWBend = new TRS.IT.BendProcessor.BLL.FWBend();
                    TpaPlanAdminReports otp = new TpaPlanAdminReports(fWBend);
                    oTaskStatus = otp.ProcessTpaPlanAdminReportsISC();
                    if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                    {
                        Utils.SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), Utils.ParseError(oTaskStatus.errors), true);
                        Utils.AddErrorEventLog(a_sScheduleName + ": " + Utils.ParseError(oTaskStatus.errors));
                    }
                    else
                    {
                        ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                        Logger.LogInformation(a_sScheduleName + ": Success");
                    }
                    fWBend.SendTaskCompleteEmail(a_sScheduleName, General.ParseTaskInfo(oTaskStatus), a_sScheduleName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                string sStatusError = string.Empty;
                if (oTaskStatus != null) Utils.ParseError(oTaskStatus.errors);

                Utils.SendErrorEmail("ScheduleCallBackDaily: " + a_sScheduleName, sStatusError + " ex: " + ex.Message, false);
            }
        }
    }
}
