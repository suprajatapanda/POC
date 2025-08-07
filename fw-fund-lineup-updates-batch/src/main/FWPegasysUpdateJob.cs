using System.Text;
using FWFundLineupUpdatesBatch.BLL;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace FWFundLineupUpdatesBatch
{
    public  class FWPegasysUpdateJob
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            TRS.IT.BendProcessor.BLL.FWBend fWBend = new();
            string sError;
            try
            {
                if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                {
                    ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);                    
                    FWBend oP = new FWBend(fWBend);
                    oTaskStatus = oP.ProcessUpdatePegasys();
                    if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                    {
                        sError = Utils.ParseError(oTaskStatus.errors);
                        Utils.AddErrorEventLog(a_sScheduleName + ": " + sError);
                        Utils.SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                    }
                    else
                    {
                        ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                    }
                    fWBend.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                string sStatusError = string.Empty;
                if (oTaskStatus != null) Utils.ParseError(oTaskStatus.errors);
                fWBend.SendErrorEmail(ex);
            }
        }
    }
}
