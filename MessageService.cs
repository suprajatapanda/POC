using System.ServiceModel;
namespace TRS.IT.SI.Services.wsNotification
{
    public class MessageService : IDisposable
    {
        private bool _disposed = false;
        private MessageServiceSoapClient _wsMS;
        public MessageService(string soapEndpoint)
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
            _wsMS = new MessageServiceSoapClient(basicHttpBinding, endpointAddress);
            _wsMS.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public TWS_Response SendMessage(Message oMsg)
        {
            return _wsMS.SendMessage(oMsg);
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
                    if (_wsMS?.State == CommunicationState.Opened)
                    {
                        _wsMS.Close();
                    }
                }
                catch
                {
                    _wsMS?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
