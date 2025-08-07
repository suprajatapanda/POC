using System.Data;
using TRS.IT.BendScheduler.Model;
using TRS.IT.TrsAppSettings;

namespace TRS.IT.BendScheduler.DAL
{
    public class SchedulerDC
    {
        string _sConnectString;

        public SchedulerDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }

        public BendSchedulerLog GetScheduleLog(DateTime a_dtRunDt, string a_sJobName)
        {
            BendSchedulerLog oScheduleLog = new();
            DataSet ds;
            SIUtil.Logger.LogMessage($"_sConnectString {_sConnectString}");
            using (ds = SqlHelper.TRSSqlHelper.ExecuteDataset(_sConnectString, "pBkP_GetScheduleLog", [a_dtRunDt, a_sJobName]))
            {
                if (ds.Tables[0].Rows.Count > 0)
                {

                    oScheduleLog.runDt = DALUtils.IsDBNullDt(ds.Tables[0].Rows[0]["run_dt"]);
                    oScheduleLog.jobName = DALUtils.IsDBNullStr(ds.Tables[0].Rows[0]["job_name"]);
                    oScheduleLog.jobType = DALUtils.IsDBNullInt(ds.Tables[0].Rows[0]["job_type"]);
                    oScheduleLog.partnerId = DALUtils.IsDBNullStr(ds.Tables[0].Rows[0]["partner_id"]);
                    oScheduleLog.startTime = DALUtils.IsDBNullDt(ds.Tables[0].Rows[0]["start_time"]);
                    oScheduleLog.endTime = DALUtils.IsDBNullDt(ds.Tables[0].Rows[0]["end_time"]);
                    oScheduleLog.status = DALUtils.IsDBNullInt(ds.Tables[0].Rows[0]["status"]);
                    oScheduleLog.resultCnt = DALUtils.IsDBNullInt(ds.Tables[0].Rows[0]["result_cnt"]);
                    oScheduleLog.retryCnt = DALUtils.IsDBNullInt(ds.Tables[0].Rows[0]["retry_cnt"]);
                    oScheduleLog.createDt = DALUtils.IsDBNullDt(ds.Tables[0].Rows[0]["create_dt"]);
                    oScheduleLog.lastRun = DALUtils.IsDBNullDt(ds.Tables[0].Rows[0]["last_run"]);
                    oScheduleLog.taskCompleted = DALUtils.IsDBNullStr(ds.Tables[0].Rows[0]["task_completed"]);
                }
            }
            return oScheduleLog;
        }

        public int InsertScheduleLog(DateTime a_dtRunDt, string a_sJobName, int a_iJobType, string a_sPartnerId,
            DateTime a_dtStartTime, string a_sTaskCompleted, int a_iIncreaseRetryCnt)
        {
            return SqlHelper.TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pBkP_InsertScheduleLog",
                [a_dtRunDt, a_sJobName, a_iJobType, a_sPartnerId, a_dtStartTime, a_sTaskCompleted, a_iIncreaseRetryCnt]);
        }
        public int UpdateScheduleLogComplete(DateTime a_dtRunDt, string a_sJobName, string a_sTaskCompleted, DateTime a_dtEndTime, int a_iResultCnt)
        {
            return SqlHelper.TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pBkP_UpdateScheduleLogComplete",
                [a_dtRunDt, a_sJobName, a_sTaskCompleted, a_dtEndTime, a_iResultCnt]);
        }


    }
}
