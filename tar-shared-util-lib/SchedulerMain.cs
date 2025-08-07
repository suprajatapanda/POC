using System.Text;
using SIUtil;
using TRS.IT.BendProcessor.BLL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace wsBendScheduler
{
    public partial class SchedulerMain
    {
        public SchedulerMain()
        {
            try
            {
                ScheduleCallBackInterval("TestingResult");
            }
            catch (Exception ex)
            {
                AddErrorEventLog("Exception: " + ex.Message);
            }
        }

        private string ParseError(List<ErrorInfo> a_oError)
        {
            StringBuilder strB = new();

            foreach (ErrorInfo oE in a_oError)
            {
                strB.Append(oE.errorDesc + "\n");
            }
            return strB.ToString();
        }
        private void ScheduleCallBackInterval(string a_sScheduleName)
        {
            TaskStatus oTaskStatus = null;
            try
            {
                if (AppSettings.GetValue("ScheduleCallBackInterval") == "1")
                {
                    AddInfoEventLog(a_sScheduleName + " " + DateTime.Now.ToString());
                }

                switch (a_sScheduleName)
                {
                    case "TestingResult":
                        TestingResults oTestResults = new();
                        oTaskStatus = oTestResults.ProcessTestingResultsMigrated();
                        if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                        {
                            AddErrorEventLog(a_sScheduleName + ": " + ParseError(oTaskStatus.errors));
                        }

                        oTaskStatus = oTestResults.ProcessFinalResultsMigrated();
                        if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                        {
                            AddErrorEventLog(a_sScheduleName + ": " + ParseError(oTaskStatus.errors));
                        }

                        break;

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                string sStatusError = string.Empty;
                if (oTaskStatus != null)
                {
                    ParseError(oTaskStatus.errors);
                }

                SendErrorEmail("ScheduleCallBackInterval: " + a_sScheduleName, sStatusError + " ex: " + ex.Message + "\n\n\n Stack Trace:\n" + ex.StackTrace, false);
            }
        }

        private void SendErrorEmail(string a_sSubject, string a_sBody, Boolean a_bToUserAsWell)
        {
            string sFr = AppSettings.GetValue("BendFromEmail");
            string sTo = AppSettings.GetValue("SystemErrorEmailNotification");
            if (a_bToUserAsWell)
            {
                sTo += ";" + AppSettings.GetValue("ProcessingErrorEmailNotification");
            }
            Utils.SendMail(sFr, sTo, a_sSubject, a_sBody);
        }

        private void AddInfoEventLog(string eventLog)
        {
            Logger.LogMessage(eventLog, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
        }

        private void AddErrorEventLog(string eventLog)
        {
            Logger.LogMessage(eventLog, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
        }
    }
}
