using System.ServiceModel;

namespace TRS.IT.SI.Services
{
    public static class InitializeBinding
    {
        public static (BasicHttpBinding, EndpointAddress) InitializeHttpBindingClient(string soapEndpoint)
        {
            var endpointAddress = new EndpointAddress(soapEndpoint);
            var basicHttpBinding = new BasicHttpBinding(
                endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
                    ? BasicHttpSecurityMode.Transport
                    : BasicHttpSecurityMode.None);
            if (endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                basicHttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
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
            return (basicHttpBinding, endpointAddress);
        }
        public static (NetTcpBinding, EndpointAddress) InitializeTcpClient(string soapEndpoint)
        {
            var endpointAddress = new EndpointAddress(soapEndpoint);
            var netTcpBinding = new NetTcpBinding();
            netTcpBinding.CloseTimeout = TimeSpan.FromMinutes(1);
            netTcpBinding.OpenTimeout = TimeSpan.FromMinutes(5);
            netTcpBinding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            netTcpBinding.SendTimeout = TimeSpan.FromMinutes(1);

            netTcpBinding.TransferMode = TransferMode.Buffered;
            netTcpBinding.MaxBufferPoolSize = 524288;
            netTcpBinding.MaxBufferSize = 185536;
            netTcpBinding.MaxReceivedMessageSize = 185536;

            netTcpBinding.ReliableSession.Ordered = true;
            netTcpBinding.ReliableSession.InactivityTimeout = TimeSpan.FromMinutes(10);
            netTcpBinding.ReliableSession.Enabled = false;

            netTcpBinding.Security.Mode = SecurityMode.Transport;
            netTcpBinding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            netTcpBinding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            netTcpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Windows;
            return (netTcpBinding, endpointAddress);
        }
    }
}
