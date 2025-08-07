using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
using DailyVendorNotificationBatch.BLL;


namespace DailyVendorNotificationBatch
{
    public class DailyVendorNotification
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            try
            {
                Logger.LogMessage("Checking if job has already run...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                
                if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                {
                    ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                    PayStart oP2 = new PayStart();
                    oTaskStatus = oP2.SendVendorNotification();
                    if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                    {
                        AddErrorEventLog(a_sScheduleName + ": " + ParseError(oTaskStatus.errors));
                    }
                    else
                    {
                        ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                    }
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
