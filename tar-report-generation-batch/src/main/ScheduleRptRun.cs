using System.Text;
using SIUtil;
using TarReportGenerationBatch.BLL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace TarReportGenerationBatch
{
    public class ScheduleRptRun
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            string sError;
            TRS.IT.BendProcessor.BLL.PASSScheduler op1 = new();
            if (!ScheduleUtils.HasJobRun(a_sScheduleName))
            {
                ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                PASSScheduler oP = new(op1);
                oTaskStatus = oP.ProcessPASSScheduledRpt();
                ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                {
                    sError = Utils.ParseError(oTaskStatus.errors);
                    Utils.SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                    Utils.AddErrorEventLog(a_sScheduleName + ": " + sError);
                }
                else
                {
                    //ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                    AddInfoEventLog(a_sScheduleName + ": Success");
                }
                op1.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
            }
            else
            {
                Utils.AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
            }
        }

        private void AddInfoEventLog(string eventLog)
        {
            Logger.LogMessage(eventLog, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
        }
    }
}
