using System.ServiceModel;
using TRS.IT.SI.Services.wsPSDSponsor;

namespace TRS.IT.BendProcessor.DriverSOA
{
    public class SponsorService : IDisposable
    {
        private SponsorBAImplPortTypeClient _ISCSponsorvc;
        private bool _disposed = false;

        public SponsorService()
        {
            InitializeClient();
        }

        private void InitializeClient()
        {
            string soapEndpoint = TrsAppSettings.AppSettings.GetValue("SponsorSrvWebServiceURL");
            var endpointAddress = new EndpointAddress(soapEndpoint);
            var basicHttpBinding = new BasicHttpBinding(
                endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
                    ? BasicHttpSecurityMode.Transport
                    : BasicHttpSecurityMode.None);
            if (endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }
            basicHttpBinding.MaxReceivedMessageSize = 10 * 1024 * 1024;
            basicHttpBinding.MaxBufferSize = 10 * 1024 * 1024;
            basicHttpBinding.MaxBufferPoolSize = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxDepth = 128;
            basicHttpBinding.ReaderQuotas.MaxStringContentLength = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxArrayLength = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxBytesPerRead = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxNameTableCharCount = 10 * 1024 * 1024;
            _ISCSponsorvc = new SponsorBAImplPortTypeClient(basicHttpBinding, endpointAddress);
            _ISCSponsorvc.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }

        public string GetPptWithBalanceCount(string inputContracts, DateTime asOfDate)
        {
            return _ISCSponsorvc.getPptWithBalanceCount(inputContracts, asOfDate.ToString("yyyy-MM-dd"));
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
                    if (_ISCSponsorvc?.State == CommunicationState.Opened)
                    {
                        _ISCSponsorvc.Close();
                    }
                }
                catch
                {
                    _ISCSponsorvc?.Abort();
                }
                _disposed = true;
            }
        }
    }
}