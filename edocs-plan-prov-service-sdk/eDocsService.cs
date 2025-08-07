using System.ServiceModel;
using System.ServiceModel.Security;
using System.Text;
using System.Xml;
using TRS.IT.TrsAppSettings;

namespace TRS.IT.SI.Services
{
    public class eDocsService: IDisposable
    {
        private TRSPlanService.PlanServiceClient _oPlanClient;
        private bool _disposed = false;
        public eDocsService(string soapEndpoint)
        {
            var endpointAddress = new EndpointAddress(soapEndpoint);
            _oPlanClient = new TRSPlanService.PlanServiceClient(InitializePlanServiceClient(soapEndpoint), endpointAddress);
            _oPlanClient.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        private WSHttpBinding InitializePlanServiceClient(string soapEndpoint)
        {
            return new WSHttpBinding
            {
                CloseTimeout = TimeSpan.FromMinutes(1),
                OpenTimeout = TimeSpan.FromMinutes(1),
                ReceiveTimeout = TimeSpan.FromMinutes(10),
                SendTimeout = TimeSpan.FromMinutes(10),
                BypassProxyOnLocal = false,
                TransactionFlow = false,
                MaxBufferPoolSize = 2147483647,
                MaxReceivedMessageSize = 2147483647,
                AllowCookies = false,
                MessageEncoding = WSMessageEncoding.Text,
                TextEncoding = Encoding.UTF8,
                UseDefaultWebProxy = true,
                ReaderQuotas = new XmlDictionaryReaderQuotas
                {
                    MaxDepth = 32,
                    MaxStringContentLength = 2147483647,
                    MaxArrayLength = 2147483647,
                    MaxBytesPerRead = 2147483647,
                    MaxNameTableCharCount = 2147483647
                },
                ReliableSession = new OptionalReliableSession
                {
                    Ordered = true,
                    InactivityTimeout = TimeSpan.FromMinutes(10),
                    Enabled = false
                },
                Security = new WSHttpSecurity
                {
                    Mode = soapEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? SecurityMode.Transport : SecurityMode.None,
                    Transport = new HttpTransportSecurity
                    {
                        ClientCredentialType = HttpClientCredentialType.None,
                        ProxyCredentialType = HttpProxyCredentialType.None
                    },
                    Message = new NonDualMessageSecurityOverHttp
                    {
                        ClientCredentialType = MessageCredentialType.Certificate,
                        AlgorithmSuite = SecurityAlgorithmSuite.Default
                    }
                }
            };
            
        }
        public string GeteDocsFWPendingCases(string startDate, string endDate)
        {
            var oContract = new TRSPlanService.ContractInfo
            {
                StartDate = Convert.ToDateTime(startDate),
                EndDate = Convert.ToDateTime(endDate),
                isSalesOffice = "False",
                LoggedInUser = AppSettings.GetValue("CyberArkUserName")
            };
            return _oPlanClient.GetNewlySubmittedContracts(oContract);
        }

        public string GetParticipantAgreementCode(string contractId, string subId)
        {
            try
            {
                var oContract = new TRSPlanService.ContractInfo
                {
                    ContractID = contractId,
                    SubID = subId,
                    isSalesOffice = "False"
                };
                return _oPlanClient.GetParticipantAgreementCode(oContract);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public TRSPlanService.ManagedAdvice GetManagedAdvice(string contractId, string subId, out string errorMessage)
        {
            errorMessage = "";
            var oContract = new TRSPlanService.ContractInfo
            {
                ContractID = contractId,
                SubID = subId,
                isSalesOffice = "False"
            };

            try
            {
                return _oPlanClient.GetManagedAdvice(oContract);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return new TRSPlanService.ManagedAdvice();
            }
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
                    if (_oPlanClient?.State == CommunicationState.Opened)
                    {
                        _oPlanClient.Close();
                    }
                }
                catch
                {
                    _oPlanClient?.Abort();
                }

                _disposed = true;
            }
        }
    }
}
