namespace TRS.IT.BendScheduler.Model
{
    public class BendSchedulerLog
    {
        public DateTime runDt;
        public string jobName;
        public int jobType;
        public string partnerId;
        public DateTime startTime;
        public DateTime endTime;
        public int status;
        public string taskCompleted;
        public int resultCnt;
        public int retryCnt;
        public DateTime lastRun;
        public DateTime createDt;
    }
}
