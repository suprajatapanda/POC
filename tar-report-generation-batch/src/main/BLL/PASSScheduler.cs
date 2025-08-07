using System.Data;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace TarReportGenerationBatch.BLL
{
    public class PASSScheduler
    {
        TRS.IT.BendProcessor.BLL.PASSScheduler _pASSScheduler;
        private const string C_ApplicationName_FTP_TAG = "TAG_FTP_SCHEDULER";
        public PASSScheduler(TRS.IT.BendProcessor.BLL.PASSScheduler pASSScheduler)
        {
            _pASSScheduler = pASSScheduler;
        }
        public TaskStatus ProcessPASSScheduledRpt()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;

            const string C_Task = "ProcessPASSScheduledRpt";

            DataSet ds = new();

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday)  // Extra security, dont run on Sunday
                    {
                        _pASSScheduler.InitTaskStatus(oTaskReturn, C_Task);

                        ds = _pASSScheduler.GetScheduledReportsByAppName(C_ApplicationName_FTP_TAG, true); // IMP: Only TAG_FTP_SCHEDULER reports are excluded

                        oReturn = _pASSScheduler.RunScheduledReports(ds);

                        if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                        {
                            General.CopyResultError(oTaskReturn, oReturn);
                            oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                        }
                        else
                        {
                            oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                        }
                        oTaskReturn.rowsCount += oReturn.rowsCount;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                _pASSScheduler.InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
    }
}
