using DailyAuditPackageISCBatch.BLL;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TRS.IT.TrsAppSettings;


namespace DailyAuditPackageISCBatch
{
    public class DailyAuditPackageISC
    {
        public void Run(string a_sScheduleName)
        {
            try
            {
                Logger.LogMessage("Checking if job has already run...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                {
                    Logger.LogMessage("Inserting schedule log for a given job & date...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);
                    Logger.LogMessage("Schedule log inserted for a given job & date...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                    Logger.LogMessage("Starting job execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                    AuditPackage processor = new();
                    var taskStatus = processor.ProcesseAuditPkgISC();
                    Logger.LogMessage("Job execution completed", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                    if (taskStatus.retStatus != TaskRetStatus.Succeeded)
                    {
                        string error = ParseError(taskStatus.errors);
                        AddErrorEventLog($"{a_sScheduleName}: {error}");
                        Logger.LogMessage("Sending error email...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        SendErrorEmail($"{a_sScheduleName} - {taskStatus.retStatus}", error, true);
                        Logger.LogMessage("Error email sent", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    else
                    {
                        Logger.LogMessage("Updating schedule log...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                        Logger.LogMessage("Schedule log updated", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }

                    //TRS.IT.BendProcessor.BLL.FWBend fWBend = new TRS.IT.BendProcessor.BLL.FWBend();
                    //Logger.LogMessage("Sending job completion email...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    //fWBend.SendTaskCompleteEmail(a_sScheduleName, taskStatus.retStatus.ToString(), a_sScheduleName);
                    //Logger.LogMessage("Job completion email sent", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
        }


        private static string ParseError(List<ErrorInfo> errors) =>
        string.Join(Environment.NewLine, errors.Select(e => e.errorDesc));


        private static void AddErrorEventLog(string message) =>
        Logger.LogMessage(message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);

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
