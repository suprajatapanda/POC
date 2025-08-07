using System.ServiceModel;
using TRS.IT.SI.Services.wsFmrs;

namespace TRS.IT.SI.Services
{
    public class FmrsService : IDisposable
    {
        private FMRSSoapClient _wsFmrs;
        private bool _disposed = false;
        public FmrsService(string soapEndpoint)
        {
            var (basicHttpBinding, endpointAddress) = InitializeBinding.InitializeHttpBindingClient(soapEndpoint);
            _wsFmrs = new FMRSSoapClient(basicHttpBinding, endpointAddress);
            _wsFmrs.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }

        public string GetFMRSFundsXml(string xml)
        {
            return _wsFmrs.GetFMRSFundsXml(xml, 0);
        }
        public string GetFMRSFundsXml(string xml, int fundType)
        {
            return _wsFmrs.GetFMRSFundsXml(xml, fundType);
        }
        public string UpdateFundLineup(string xml)
        {
            return _wsFmrs.UpdateFundLineup(xml);
        }
        public string GetFMRSFundsDataset(string inputXml, int fundType, string[] fundIds)
        {
            return _wsFmrs.GetFMRSFundsXml(inputXml, fundType);
        }
        public string GetFMRSFundCategory()
        {
            return _wsFmrs.GetFMRSFundCategory(Environment.UserName, DateTime.Now);
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
                    if (_wsFmrs?.State == CommunicationState.Opened)
                    {
                        _wsFmrs.Close();
                    }
                }
                catch
                {
                    _wsFmrs?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
