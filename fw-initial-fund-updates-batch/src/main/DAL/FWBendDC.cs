using System.Data;
using TRS.IT.TrsAppSettings;

namespace FWInitialFundUpdatesBatch.DAL
{
    public class FWBendDC
    {
        private string _sConnectString ;

        public FWBendDC() 
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet GetSignPending()
        {
            return TRS.SqlHelper.TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetPendingSigned");
        }
       
    }
}
