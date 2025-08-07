using System.Data;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace FWBamlDocsToWMSBatch.DAL
{
    public class FWBendDC
    {
        private string _sConnectString;
        public FWBendDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet GetBAMLFundChange(DateTime runDate)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetBAMLTaskDue", [runDate]);
        }
    }
}
