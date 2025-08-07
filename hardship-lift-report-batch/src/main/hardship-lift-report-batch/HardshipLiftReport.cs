using HardshipLiftReport.BLL;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace HardshipLiftReport
{
    class HardshipLiftRpt
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            try
            {
                PASS oP = new();
                oTaskStatus = oP.ProcessSendHardShipLiftRpt();
                if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                {
                    Utils.AddErrorEventLog(a_sScheduleName + ": " + Utils.ParseError(oTaskStatus.errors));
                }

                oP.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);

            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                string sStatusError = string.Empty;
                if (oTaskStatus != null)
                {
                    Utils.ParseError(oTaskStatus.errors);
                }

                Utils.SendErrorEmail("ScheduleCallBackMonthly: " + a_sScheduleName, sStatusError + " ex: " + ex.Message, false);
            }
        }
    }
}
