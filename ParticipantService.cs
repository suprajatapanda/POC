using System.ServiceModel;
using TRS.IT.SI.Services.DIAWSPPT;

namespace TRS.IT.SI.Services
{
    public class ParticipantService: IDisposable
    {
        private DIAWSPPT.ParticipantServiceClient _participantClient;
        private bool _disposed = false;
        public ParticipantService(string soapEndpoint)
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
            _participantClient = new TRS.IT.SI.Services.DIAWSPPT.ParticipantServiceClient(basicHttpBinding, endpointAddress);
            _participantClient.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public updateInvestmentElectionsResponse updateInvestmentElections(DIAWSPPT.updateInvestmentElections updateInvestmentElection1)
        {
            throw NotImplementedException();
        }
        public string UpdateInvestmentElections(string PartnerUserID, string sRequest)
        {
            throw NotImplementedException();
        }
        public string CancelRecurringTransfer(string PartnerUserID, string sRequest)
        {
            throw NotImplementedException();
        }
        public string UpdateCatchupContributions(string PartnerUserID, string sRequest)
        {
            throw NotImplementedException();
        }
        public string UpdateDeferralContributions(string PartnerUserID, string sRequest)
        {
            throw NotImplementedException();
        }
        public string CancelTransaction(string PartnerUserID, string sRequest)
        {
            throw NotImplementedException();
        }
        public string RequestPinLetter(string PartnerUserID)
        {
            throw NotImplementedException();
        }
        public string RebalanceFunds(string PartnerUserID, string other,string sRequest)
        {
            throw NotImplementedException();
        }
        public string UpdatePersonalProfile(string PartnerUserID, string sRequest)
        {
            throw NotImplementedException();
        }
        public updateParticipantInfoResponse updateParticipantInfo(updateParticipantInfo updateParticipantInfo)
        {
            throw NotImplementedException();
        }
        public string RequestLoan(string PartnerUserID, string sRequest)
        {
            throw NotImplementedException();
        }
        public updateHoursWorkedResponse updateHoursWorked(updateHoursWorked updateHoursWorked)
        {
            throw NotImplementedException();
        }
        public string GenerateConfirmationLetter(string memberID, string sRequest)
        {
            throw NotImplementedException();
        }

        public DIAWSPPT.getParticipantInfoResponse getParticipantInfo(getParticipantInfo obj)
        {
            throw NotImplementedException();
        }
        public string GetParticipantInfo(string memberID)
        {
            throw NotImplementedException();
        }
        private Exception NotImplementedException()
        {
            throw new NotImplementedException();
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
                    if (_participantClient?.State == CommunicationState.Opened)
                    {
                        _participantClient.Close();
                    }
                }
                catch
                {
                    _participantClient?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
