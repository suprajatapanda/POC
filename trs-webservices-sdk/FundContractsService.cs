using System.ServiceModel;
using TRS.IT.SI.Services.wsFundContractsService;

namespace TRS.IT.SI.Services
{
    public class FundContractsService : IDisposable
    {
        private FundContractsServiceSoapClient _wsFundContracts;
        private bool _disposed = false;
        public FundContractsService(string soapEndpoint)
        {
            var (basicHttpBinding, endpointAddress) = InitializeBinding.InitializeHttpBindingClient(soapEndpoint);
            _wsFundContracts = new FundContractsServiceSoapClient(basicHttpBinding, endpointAddress);
            _wsFundContracts.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        
        public FundContracts GetContracts(string contractId, string subId)
        {
            return _wsFundContracts.GetContracts(contractId, subId);
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
                    if (_wsFundContracts?.State == CommunicationState.Opened)
                    {
                        _wsFundContracts.Close();
                    }
                }
                catch
                {
                    _wsFundContracts?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
