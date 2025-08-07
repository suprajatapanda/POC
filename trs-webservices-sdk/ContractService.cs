using System.ServiceModel;
using TRS.IT.SI.Services.wsContract;

namespace TRS.IT.SI.Services
{
    public class ContractService : IDisposable
    {
        private ContractServiceSoapClient _wsContract;
        private bool _disposed = false;
        public ContractService(string soapEndpoint)
        {
            var (basicHttpBinding, endpointAddress) = InitializeBinding.InitializeHttpBindingClient(soapEndpoint);
            _wsContract = new ContractServiceSoapClient(basicHttpBinding, endpointAddress);
            _wsContract.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public string GetCustomPxFunds(string contractId, string subId)
        {
            return _wsContract.GetCustomPxFunds(contractId, subId);
        }

        public TWS_Response SetCustomPxFunds(string contractId, string subId, string xmlCustomFunds)
        {
            return _wsContract.SetCustomPxFunds(contractId, subId, xmlCustomFunds);
        }
        public ContractInfo GetContractInformation(string contractId, string subId, AdditionalData additionalData)
        {
            return _wsContract.GetContractInformation(contractId, subId, additionalData);
        }

        public string SubmitTestingResults(string xmlTestingResults)
        {
            return _wsContract.SubmitTestingResults(xmlTestingResults);
        }

        public ContractInfo GetContractInformation(string contractId, string subId)
        {
            var adData = new AdditionalData
            {
                Basic_Provisions_Required = true,
                All_Provisions_Required = true,
                Contacts_Required = true
            };
            return _wsContract.GetContractInformation(contractId, subId, adData);
        }

        public string NotifyToConsolidateMessages(string xmlWsDocumentServiceDocumentEx, string dataToConsolidate, string xmlMessageVariablesEx)
        {
            return _wsContract.NotifyToConsolidateMessages(xmlWsDocumentServiceDocumentEx, dataToConsolidate, xmlMessageVariablesEx);
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
                    if (_wsContract?.State == CommunicationState.Opened)
                    {
                        _wsContract.Close();
                    }
                }
                catch
                {
                    _wsContract?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
