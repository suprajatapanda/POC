using System.ServiceModel;

namespace TRS.IT.SI.Services
{
    public class MDPService : IDisposable
    {
        private wsMDP.PricingUserInterfaceBAPortTypeClient _wsMDP;
        private bool _disposed = false;
        public MDPService(string soapEndpoint)
        {
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
            _wsMDP = new Services.wsMDP.PricingUserInterfaceBAPortTypeClient(basicHttpBinding, endpointAddress);
            _wsMDP.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public string GetLegacyCaseObject(string runId, int arg1)
        {
            return _wsMDP.getLegacyCaseObject(runId, arg1);
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
                    if (_wsMDP?.State == CommunicationState.Opened)
                    {
                        _wsMDP.Close();
                    }
                }
                catch
                {
                    _wsMDP?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
