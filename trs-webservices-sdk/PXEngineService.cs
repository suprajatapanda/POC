using System.ServiceModel;
using System.ServiceModel.Channels;
using TRS.IT.SI.Services.PXEngineSvc;

namespace TRS.IT.SI.Services
{
    public class PXEngineService : IDisposable
    {
        private PXEngineClient _pxSvc;
        private bool _disposed = false;
        
        public PXEngineService(string soapEndpoint,string wcfBinding)
        {
            (Binding basicBinding, EndpointAddress endpointAddress) bindingConfig;
            bindingConfig = !string.IsNullOrEmpty(wcfBinding) && wcfBinding.ToLower() == "basichttpbinding_ipxengine" 
                    ? InitializeBinding.InitializeHttpBindingClient(soapEndpoint)
                    : InitializeBinding.InitializeTcpClient(soapEndpoint);
            _pxSvc = new PXEngineClient(bindingConfig.basicBinding, bindingConfig.endpointAddress);
            _pxSvc.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public PortfolioData GetPortfolioInfoCurrent(string contractID, string subID, PXInputE_GLIDE_PATH glidePath)
        {
            try
            {
                return _pxSvc.GetPortfolioInfoCurrent(contractID, subID, glidePath);
            }
            catch (FaultException<BusinessRuleFault> ex)
            {
                throw new Exception(ex.Detail.Message);
            }
            catch (FaultException<FundFault> ex)
            {
                throw new Exception(ex.Detail.Message);
            }
            catch (FaultException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (CommunicationException ex)
            {
                throw new Exception(ex.Message);
            }            
        }
        public PortfolioData GetPortfolioInfo(string contractID, string subID, PXInputE_GLIDE_PATH glidePath)
        {
            try
            {
                return _pxSvc.GetPortfolioInfo(contractID, subID, glidePath);
            }
            catch (FaultException<BusinessRuleFault> ex)
            {
                throw new Exception(ex.Detail.Message);
            }
            catch (FaultException<FundFault> ex)
            {
                throw new Exception(ex.Detail.Message);
            }
            catch (FaultException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (CommunicationException ex)
            {
                throw new Exception(ex.Message);
            }            
        }
        public PortfolioData GetPortfolioInfoWithFunds(string contractID, string subID, PXInputE_GLIDE_PATH glidePath,string xmlFunds)
        {
            try
            {
                return _pxSvc.GetPortfolioInfoWithFunds(contractID, subID, glidePath, xmlFunds);
            }
            catch (FaultException<BusinessRuleFault> ex)
            {
                throw new Exception(ex.Detail.Message);
            }
            catch (FaultException<FundFault> ex)
            {
                throw new Exception(ex.Detail.Message);
            }
            catch (FaultException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (CommunicationException ex)
            {
                throw new Exception(ex.Message);
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
                    if (_pxSvc != null)
                    {
                        if (_pxSvc.State == CommunicationState.Faulted)
                        {
                            _pxSvc.Abort();
                        }
                        else if (_pxSvc.State != CommunicationState.Closed)
                        {
                            _pxSvc.Close();
                        }
                    }
                }
                catch
                {
                    _pxSvc?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
