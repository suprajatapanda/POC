using System.Data;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TarPlanAdminReportBatch.DAL
{
    public class TpaPlanAdminReportsDC
    {
        private string _sConnectString;

        public TpaPlanAdminReportsDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet GetAccountTypes()
        {
            DataSet ds = new();

            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pSI_GetAccountTypes", []);
            return ds;
        }

    }
}
