using System.Data;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TRS.IT.BendProcessor.DAL
{
    public class FWBendDC
    {
        private string _sConnectString;

        public FWBendDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet GetPendingList(DateTime a_dtRunDt)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetFWPending", [a_dtRunDt]);
        }
    }
}
