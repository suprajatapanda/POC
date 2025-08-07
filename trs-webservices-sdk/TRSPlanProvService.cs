using System.Data;
using System.ServiceModel;
using TRS.IT.SI.Services.wsTrsPlanProv;

namespace TRS.IT.SI.Services
{    
    public class TRSPlanProvService : IDisposable
    {
        private TRSPlanProvSoapClient _wsTrsPlanProv;
        private bool _disposed = false;
        public TRSPlanProvService(string soapEndpoint)
        {
            var (basicHttpBinding, endpointAddress) = InitializeBinding.InitializeHttpBindingClient(soapEndpoint);
            _wsTrsPlanProv = new TRSPlanProvSoapClient(basicHttpBinding, endpointAddress);
            _wsTrsPlanProv.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public string DocGenFundRider(string contractId, string subId, string userId)
        {
            return _wsTrsPlanProv.DocGenDocument(contractId, subId, "FundRider", userId);
        }
        public string DocGenQDIA(string contractId, string subId, string userId)
        {
            return _wsTrsPlanProv.DocGenDocument(contractId, subId, "QDIAFund", userId);
        }
        public DataSet ListForHardshipLift(string strStartDate, string strEndDate)
        {
            DataSet ds = new();
            string s = _wsTrsPlanProv.ListForHardshipLift(strStartDate, strEndDate, "Pass 150");
            StringReader oReader = new(s);
            ds.ReadXml(oReader);
            return ds;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    if (_wsTrsPlanProv?.State == CommunicationState.Opened)
                    {
                        _wsTrsPlanProv.Close();
                    }
                }
                catch
                {
                    _wsTrsPlanProv?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
