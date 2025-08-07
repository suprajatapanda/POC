using SIUtil;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendScheduler.BLL;
using FWUpdateRKPartner.BLL;

namespace FWUpdateRKPartner;
public class FWUpdatePartnerJobRunner
{
    public int Run(string jobName)
    {
        TaskStatus? oTaskStatus = null;
        string sError = string.Empty;

        try
        {
            Logger.LogMessage($"Starting job: {jobName}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

            if (!ScheduleUtils.HasJobRun(jobName))
            {
                ScheduleUtils.InsertScheduleLog(DateTime.Today, jobName, ScheduleType.DAILY, "TRS", string.Empty, true);
                TRS.IT.BendProcessor.BLL.FWBend fWBend = new TRS.IT.BendProcessor.BLL.FWBend();
                FWBend oP2 = new FWBend(fWBend);
                oTaskStatus = oP2.ProcessUpdatePartner();

                if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                {
                    sError = TRS.IT.BendProcessor.Util.Utils.ParseError(oTaskStatus.errors);
                    Console.Error.WriteLine($"{jobName}: {sError}");
                    TRS.IT.BendProcessor.Util.Utils.SendErrorEmail($"{jobName}-{oTaskStatus.retStatus}", sError, true);
                    return 1;
                }
                else
                {
                    ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, jobName, "Done", DateTime.Now, 0);
                }
                fWBend.SendTaskCompleteEmail(jobName, oTaskStatus.retStatus.ToString(), jobName);
            }
            else
            {
                Logger.LogMessage($"{jobName} has already run today.", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }

            Logger.LogMessage("Job completed successfully.", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            return 0;
        }
        catch (Exception ex)
        {
            Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            if (oTaskStatus != null)
            {
                sError = TRS.IT.BendProcessor.Util.Utils.ParseError(oTaskStatus.errors);
            }
            TRS.IT.BendProcessor.Util.Utils.SendErrorEmail($"ScheduleCallBackDaily: {jobName}", sError + " ex: " + ex.Message, false);
            return 2;
        }
    }
}