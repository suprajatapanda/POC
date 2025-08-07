using System.ServiceModel;

namespace TRS.IT.SI.BusinessFacadeLayer.Adapters.WSReport
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://tempuri.org/", ConfigurationName = "TRS.IT.SI.BusinessFacadeLayer.Adapters.WSReport.IPencoReportsService")]
    public interface IPencoReportsService
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IPencoReportsService/ReportRequest", ReplyAction = "http://tempuri.org/IPencoReportsService/ReportRequestResponse")]
        string ReportRequest(string ContractID, string SubID, string ReportInfo);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/IPencoReportsService/ReportRequest", ReplyAction = "http://tempuri.org/IPencoReportsService/ReportRequestResponse")]
        Task<string> ReportRequestAsync(string ContractID, string SubID, string ReportInfo);
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
    public interface IPencoReportsServiceChannel : TRS.IT.SI.BusinessFacadeLayer.Adapters.WSReport.IPencoReportsService, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.2.0-preview1.23462.5")]
    public partial class PencoReportsServiceClient : System.ServiceModel.ClientBase<TRS.IT.SI.BusinessFacadeLayer.Adapters.WSReport.IPencoReportsService>, TRS.IT.SI.BusinessFacadeLayer.Adapters.WSReport.IPencoReportsService
    {
        /// <summary>
        /// Implement this partial method to configure the service endpoint.
        /// </summary>
        /// <param name="serviceEndpoint">The endpoint to configure</param>
        /// <param name="clientCredentials">The client credentials</param>
        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);

        public PencoReportsServiceClient(EndpointConfiguration endpointConfiguration) :
                base(PencoReportsServiceClient.GetBindingForEndpoint(endpointConfiguration), PencoReportsServiceClient.GetEndpointAddress(endpointConfiguration))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }

        public PencoReportsServiceClient(EndpointConfiguration endpointConfiguration, string remoteAddress) :
                base(PencoReportsServiceClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }

        public PencoReportsServiceClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) :
                base(PencoReportsServiceClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }

        public PencoReportsServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
                base(binding, remoteAddress)
        {
        }

        public string ReportRequest(string ContractID, string SubID, string ReportInfo)
        {
            return base.Channel.ReportRequest(ContractID, SubID, ReportInfo);
        }

        public Task<string> ReportRequestAsync(string ContractID, string SubID, string ReportInfo)
        {
            return base.Channel.ReportRequestAsync(ContractID, SubID, ReportInfo);
        }

        public virtual Task OpenAsync()
        {
            return Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new Action<IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }

        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_IPencoReportsService))
            {
                BasicHttpBinding result = new BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;

                // Configure timeouts
                result.OpenTimeout = TimeSpan.FromMinutes(1);
                result.CloseTimeout = TimeSpan.FromMinutes(1);
                result.SendTimeout = TimeSpan.FromMinutes(10);
                result.ReceiveTimeout = TimeSpan.FromMinutes(10);

                // If the service requires HTTPS, uncomment the following line:
                // result.Security.Mode = BasicHttpSecurityMode.Transport;

                return result;
            }
            throw new InvalidOperationException(string.Format("Could not find endpoint with name '{0}'.", endpointConfiguration));
        }

        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_IPencoReportsService))
            {
                // Read URL from configuration or use default
                string urlSetting = null; // TrsAppSettings.AppSettings.GetValue("PencoReportsWebServiceURL");
                string url = !string.IsNullOrEmpty(urlSetting)
                    ? urlSetting
                    : "http://trscsctst1.us.aegon.com/PencoReportsService/PencoReportsService.svc";

                return new System.ServiceModel.EndpointAddress(url);
            }
            throw new InvalidOperationException(string.Format("Could not find endpoint with name '{0}'.", endpointConfiguration));
        }

        public enum EndpointConfiguration
        {
            BasicHttpBinding_IPencoReportsService,
        }
    }

}
