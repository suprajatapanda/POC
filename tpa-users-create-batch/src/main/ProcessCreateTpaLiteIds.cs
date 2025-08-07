using System.Text;
using TRS.IT.TrsAppSettings;
using SIUtil;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendProcessor.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
using TRS.IT.BendScheduler.BLL;
using TpaUsersCreateBatch.BLL;


namespace TpaUsersCreateBatch
{
    public class ProcessCreateTpaLiteIds
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            string sError;
            if (!ScheduleUtils.HasJobRun(a_sScheduleName))
            {
                ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                Miscellaneous oM = new();
                oTaskStatus = oM.ProcessCreateTpaLiteIds();
                ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                {
                    sError = Utils.ParseError(oTaskStatus.errors);
                    Utils.SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                    Utils.AddErrorEventLog(a_sScheduleName + ": " + sError);
                }
                else
                {
                    Logger.LogInformation(a_sScheduleName + ": ProcessCreateTpaLiteIds completed successfully.");
                }
                oM.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
            }
            else
            {
                Utils.AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
            }
        }
    }
}