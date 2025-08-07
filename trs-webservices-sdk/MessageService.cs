using System.ServiceModel;
using TRS.IT.SI.Services.wsMessage;

namespace TRS.IT.SI.Services
{
    public class MessageService : IDisposable
    {
        private MessageServiceSoapClient _wsTRSMS;
        private bool _disposed = false;
        public MessageService(string soapEndpoint)
        {
            var (basicHttpBinding, endpointAddress) = InitializeBinding.InitializeHttpBindingClient(soapEndpoint);
            _wsTRSMS = new MessageServiceSoapClient(basicHttpBinding, endpointAddress);
            _wsTRSMS.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public TWS_Response SendWebMessages(string InLogin_xml, webMessage oWebMsg)
        {
            return SendWebMessages(InLogin_xml, oWebMsg, "", "");
        }
        public TWS_Response SendWebMessages(string loginXml, webMessage webMsg, string contractId, string subId)
        {
            return _wsTRSMS.SendWebMessages(loginXml, webMsg, contractId, subId);
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
                    if (_wsTRSMS?.State == CommunicationState.Opened)
                    {
                        _wsTRSMS.Close();
                    }
                }
                catch
                {
                    _wsTRSMS?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
