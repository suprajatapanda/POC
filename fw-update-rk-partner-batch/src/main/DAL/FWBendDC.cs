using System.Data;
using TRS.IT.TrsAppSettings;

namespace FWUpdateRKPartner.DAL
{
    public class FWBendDC
    {
        private string _sConnectString ;

        public FWBendDC() 
        {
            _sConnectString = AppSettings.GetValue("ConnectString");
        }

        public DataSet GetTmCode(string a_sPartnerId, int a_iFundId)
        {
            return TRS.SqlHelper.TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetTMCode", new object[] { a_sPartnerId, a_iFundId });
        }         

    }
}
