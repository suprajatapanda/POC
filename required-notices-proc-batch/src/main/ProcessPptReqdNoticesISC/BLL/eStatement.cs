using System.Data;
using System.Text;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace ProcessPptReqdNoticesISC
{
    public class eStatement
    {
        private static readonly TRS.IT.BendProcessor.BLL.eStatement _estatement = new();
        private const int C_ParticipantReqdNoticesNotificationType_ISC = 5;
        private DataSet _dsPptOptin;

        public eStatement() { }

        public TaskStatus ProcessParticipantReqdNoticesISC()
        {
            TaskStatus oTaskReturn = new();
            const string C_Task = "ProcessParticipantReqdNoticesISC";
            var oTaskStatus = new TaskStatus { retStatus = TaskRetStatus.NotRun };
            var strB = new StringBuilder();
            ResultReturn oReturn = new();

            try
            {
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    _estatement.InitTaskStatus(oTaskReturn, C_Task);

                    oTaskStatus = _estatement.ProcessNotifyDIA(C_ParticipantReqdNoticesNotificationType_ISC); // create and upload dia notification file containing list of participants name and email id etc.
                    strB.Append(General.ParseTaskInfo(oTaskStatus));

                    if (oTaskStatus.retStatus == TaskRetStatus.Failed || oTaskStatus.retStatus == TaskRetStatus.FailedAborted)
                    {
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                        _estatement.SendTaskCompleteEmail("ParticipantReqdNoticesISC Status (ProcessNotifyDIA Failed)- " + oTaskStatus.retStatus.ToString(), strB.ToString(), oTaskStatus.taskName);
                    }
                    else
                    {
                        oTaskStatus = _estatement.ProcessClearDailyDiaFeed(C_ParticipantReqdNoticesNotificationType_ISC);
                        strB.Append(General.ParseTaskInfo(oTaskStatus));
                        _estatement.SendTaskCompleteEmail("ParticipantReqdNoticesISC Status (ProcessClearDailyDiaFeed)- " + oTaskStatus.retStatus.ToString(), strB.ToString(), oTaskStatus.taskName);
                    }

                    oTaskReturn.rowsCount += oReturn.rowsCount;
                    strB.Append(General.ParseTaskInfo(oTaskStatus));


                    oTaskReturn.endTime = DateTime.Now;
                    _estatement.SendTaskCompleteEmail("ParticipantReqdNoticesISC Task Completed", strB.ToString(), "ParticipantReqdNotices Backend Processing");

                    //Clear out cache dataset
                    if (_dsPptOptin != null)
                    {
                        _dsPptOptin.Clear();
                        _dsPptOptin = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                _estatement.InitTaskError(oTaskReturn, ex, true);
            }

            oTaskReturn.endTime = DateTime.Now;

            return oTaskReturn;
        }

    }
}
