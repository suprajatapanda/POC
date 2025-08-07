using DailyeConfirmMainBatch.BLL;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace DailyeConfirmMainBatch
{
    public class DailyeConfirmMain
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            if (!ScheduleUtils.HasJobRun(a_sScheduleName))
            {
                ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);
                TRS.IT.BendProcessor.BLL.eStatement eStat = new TRS.IT.BendProcessor.BLL.eStatement();
                eStatement oSt = new(eStat);
                oTaskStatus = oSt.ProcesseConfirmMain();
                if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                {
                    Utils.AddErrorEventLog(a_sScheduleName + ": " + Utils.ParseError(oTaskStatus.errors));
                }
                else
                {
                    ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                }
            }
        }
    }
}

