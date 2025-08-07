using System.Data;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace FWBamlDocsToWMSBatch.DAL
{
    public class ScheduleDC
    {
        private string _sConnectString;

        public ScheduleDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }

        public DataSet GetScheduleDRunDays(string sJob)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "pSI_ProcessScheduledJob", [sJob, DBNull.Value, DBNull.Value, DBNull.Value, DBNull.Value]);
        }
        public DataSet SetScheduleDStatus(string sJob, Int32 iSch_ID, int iStatus, string sType)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "pSI_ProcessScheduledJob", [sJob, iStatus, DBNull.Value, iSch_ID, sType]);
        }
    }
}
