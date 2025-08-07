using System.ServiceModel;
using TRS.IT.SI.Services.wsWithdrawals;

namespace TRS.IT.BendProcessor.DriverSOA
{
    public class WithdrawalsService : IDisposable
    {
        private WithdrawalsBAImplPortTypeClient _ISCWithdrawalSvc;
        private bool _disposed = false;

        public WithdrawalsService()
        {
            InitializeClient();
        }

        private void InitializeClient()
        {
            string soapEndpoint = TrsAppSettings.AppSettings.GetValue("WithdrawalsSrvWebServiceURL");
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
            _ISCWithdrawalSvc = new WithdrawalsBAImplPortTypeClient(basicHttpBinding, endpointAddress);
            _ISCWithdrawalSvc.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }

        public string GetMissingAddressData(DateTime startDate, DateTime endDate)
        {
            return _ISCWithdrawalSvc.getMissingAddressData(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
        }

        public string getLoanDefaultQtrlyReport(DateTime startDate, DateTime endDate)
        {
            return _ISCWithdrawalSvc.getLoanDefaultQtrlyReport(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
        }

        public string getLoanPaidOff(DateTime startDate, DateTime endDate)
        {
            return _ISCWithdrawalSvc.getLoanPaidOff(startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));
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
                    if (_ISCWithdrawalSvc?.State == CommunicationState.Opened)
                    {
                        _ISCWithdrawalSvc.Close();
                    }
                }
                catch
                {
                    _ISCWithdrawalSvc?.Abort();
                }
                _disposed = true;
            }
        }
    }
}