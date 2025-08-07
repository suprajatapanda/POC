using System.Data;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace FWFundDocsToWMSBatch.DAL
{
    public class FWBendDC
    {
        private string _sConnectString;

        public FWBendDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet GetPendingListNewCase()
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetFWPendingNewCase");
        }
        public DataSet GetPendingFundChangesForContract(string a_sContractId, string a_sSubId)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetPendingFundChangesByContract", new object[] { a_sContractId, a_sSubId });
        }
    }
}

