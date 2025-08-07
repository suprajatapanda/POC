using System.ServiceModel;
using TRS.IT.SI.Services.wsGeneral;

namespace TRS.IT.SI.Services
{
    public class GeneralService : IDisposable
    {
        private GeneralSoapClient _wsGeneral;
        private bool _disposed = false;
        public GeneralService(string soapEndpoint)
        {
            var (basicHttpBinding, endpointAddress) = InitializeBinding.InitializeHttpBindingClient(soapEndpoint);
            _wsGeneral = new GeneralSoapClient(basicHttpBinding, endpointAddress);
            _wsGeneral.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }

        public string GetLastBusinessDay()
        {
            return _wsGeneral.GetLastBusinessDay();
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
                    if (_wsGeneral?.State == CommunicationState.Opened)
                    {
                        _wsGeneral.Close();
                    }
                }
                catch
                {
                    _wsGeneral?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
