using System.Data;
using TpaUsersCreateBatch.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace TpaUsersCreateBatch.BLL
{
    public class Miscellaneous : BendProcessorBase
    {
        private MiscellaneousDC _oMiscellaneousDC;

        public Miscellaneous() : base("89", "Miscellaneous", "TRS") { _oMiscellaneousDC = new MiscellaneousDC(); }

        public TaskStatus ProcessCreateTpaLiteIds()
        {
            TaskStatus oTaskReturn = new();
            int iRecordCount = 0;
            const string C_Task = "ProcessCreateTpaLiteIds";
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    DataSet ds = _oMiscellaneousDC.CreateTpaLiteIds();

                    if (ds != null && ds.Tables.Count > 0)
                    {
                        iRecordCount = ds.Tables[0].Rows.Count;
                    }

                    SendTaskCompleteEmail("CreateTpaLiteIds Status - " + ReturnStatusEnum.Succeeded.ToString(), iRecordCount.ToString() + "  contacts created", oTaskReturn.taskName);

                    oTaskReturn.retStatus = TaskRetStatus.Succeeded;

                    oTaskReturn.rowsCount++;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }

    }
}
