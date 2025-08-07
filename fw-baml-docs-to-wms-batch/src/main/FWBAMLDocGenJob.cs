using System.Text;
using FWBamlDocsToWMSBatch.BLL;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace FWBamlDocsToWmsBatch
{
    public class FWBAMLDocGenJob
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            string sError;

            try
            {
                if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                {
                    ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                    BAMLFundDocGen oP = new BAMLFundDocGen();
                    oTaskStatus = oP.Start();
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
                    oP.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
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
