using TRS.IT.BendScheduler.DAL;
using TRS.IT.BendScheduler.Model;

namespace TRS.IT.BendScheduler.BLL
{
    public class ScheduleUtils
    {
        private ScheduleUtils() { }
        public static bool HasJobRun(string a_sJobName)
        {
            SchedulerDC oDC = new();
            BendSchedulerLog oScheduleLog;
            oScheduleLog = oDC.GetScheduleLog(DateTime.Today, a_sJobName);
            if (oScheduleLog.status == 100 | (oScheduleLog.lastRun.CompareTo(DateTime.Today) == 0 & oScheduleLog.lastRun.AddMinutes(5) > DateTime.Now))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static void InsertScheduleLog(DateTime a_dtRunDt, string a_sJobName, ScheduleType a_eType,
            string a_sPartnerId, string a_sTaskComplete, bool a_bIncreaseRetryCnt)
        {
            SchedulerDC oDC = new();
            int iRetry = 0;
            if (a_bIncreaseRetryCnt)
            {
                iRetry = 1;
            }

            oDC.InsertScheduleLog(a_dtRunDt, a_sJobName, a_eType.GetHashCode(), a_sPartnerId, DateTime.Now, a_sTaskComplete, iRetry);
        }

        public static void UpdateScheduleLogComplete(DateTime a_dtRunDt, string a_sJobName, string a_sTaskComplete, DateTime a_dtEndTime, int a_iResultCnt)
        {
            SchedulerDC oDC = new();
            oDC.UpdateScheduleLogComplete(a_dtRunDt, a_sJobName, a_sTaskComplete, a_dtEndTime, a_iResultCnt);
        }
    }
}
