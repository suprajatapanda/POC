
using ProcessPptReqdNoticesISC;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TRS.IT.TrsAppSettings;


namespace RequiredNoticesProcBatch
{
    public class ProcessPptReqdNoticesISC
    {
        public void Run(string scheduleName)
        {
            Logger.LogMessage("Checking if ProcessPptReqdNoticesISC job has already run...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            if (!ScheduleUtils.HasJobRun(scheduleName))
            {
                Logger.LogMessage("Inserting schedule log for a given job & date...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                ScheduleUtils.InsertScheduleLog(DateTime.Today, scheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);
                Logger.LogMessage("Schedule log inserted for a given job & date...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                Logger.LogMessage("Starting ProcessPptReqdNoticesISC job execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                eStatement oSt7 = new();
                var oTaskStatus = oSt7.ProcessParticipantReqdNoticesISC();
                Logger.LogMessage("Job execution completed", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                {
                    string error = ParseError(oTaskStatus.errors);
                    AddErrorEventLog($"{scheduleName}: {error}");
                    Logger.LogMessage("Sending error email...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    SendErrorEmail($"{scheduleName} - {oTaskStatus.retStatus}", error, true);
                    Logger.LogMessage("Error email sent", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                }
                else
                {
                    Logger.LogMessage("Updating schedule log...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, scheduleName, "Done", DateTime.Now, 0);
                    AddInfoEventLog(scheduleName + ": Success");
                    Logger.LogMessage("Schedule log updated", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                }

                TRS.IT.BendProcessor.BLL.FWBend fWBend = new TRS.IT.BendProcessor.BLL.FWBend();
                Logger.LogMessage("Sending job completion email...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                fWBend.SendTaskCompleteEmail(scheduleName, oTaskStatus.retStatus.ToString(), scheduleName);
                Logger.LogMessage("Job completion email sent", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
        }

        public static string ParseError(List<ErrorInfo> errors) =>
            string.Join(Environment.NewLine, errors.Select(e => e.errorDesc));

        private static void AddErrorEventLog(string message) =>
        Logger.LogMessage(message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);

        private void AddInfoEventLog(string eventLog) =>
            Logger.LogMessage(eventLog, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

        private static void SendErrorEmail(string subject, string body, bool includeUser)
        {
            string from = AppSettings.GetValue("BendFromEmail");
            string to = AppSettings.GetValue("SystemErrorEmailNotification");

            if (includeUser)
            {
                string userEmail = AppSettings.GetValue("ProcessingErrorEmailNotification");
                to = $"{to};{userEmail}";
            }

            Utils.SendMail(from, to, subject, body);
        }
    }
}
