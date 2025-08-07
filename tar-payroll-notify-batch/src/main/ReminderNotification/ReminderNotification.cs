using ReminderNotificationBatch.BLL;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendScheduler.BLL;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace ReminderNotificationBatch
{
    public class ReminderNotification
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            if (!ScheduleUtils.HasJobRun(a_sScheduleName))
            {
                ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                PayStart oP = new();
                oTaskStatus = oP.SendReminderNotification();
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

        #region Private Methods
        private static void AddErrorEventLog(string message) =>
            Logger.LogMessage(message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);

        private static string ParseError(List<ErrorInfo> errors) =>
            string.Join(Environment.NewLine, errors.Select(e => e.errorDesc));
        #endregion
    }
}

