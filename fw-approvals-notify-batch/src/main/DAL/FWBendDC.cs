using System.Data;
using TRS.IT.TrsAppSettings;

namespace FwApprovalsNotificationBatch.DAL
{
    public class FWBendDC
    {
        private string _sConnectString;

        public FWBendDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }

        public DataSet GetReminder()
        {
            return TRS.SqlHelper.TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetReminder");
        }

    }
}
