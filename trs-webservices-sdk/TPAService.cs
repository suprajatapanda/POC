using System.ServiceModel;
using TRS.IT.SI.Services.wsTPA;

namespace TRS.IT.SI.Services
{
    public class TPAService : IDisposable
    {
        private TPAServiceSoapClient _wsTPASvc;
        private bool _disposed = false;
        public TPAService(string soapEndpoint)
        {
            var (basicHttpBinding, endpointAddress) = InitializeBinding.InitializeHttpBindingClient(soapEndpoint);
            _wsTPASvc = new TPAServiceSoapClient(basicHttpBinding, endpointAddress);
            _wsTPASvc.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }

        public TPACompanyContactInformations GetContractTPAContacts(string contractId, string subId)
        {
            return _wsTPASvc.GetContractTPAContacts(contractId, subId);
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
                    if (_wsTPASvc?.State == CommunicationState.Opened)
                    {
                        _wsTPASvc.Close();
                    }
                }
                catch
                {
                    _wsTPASvc?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
