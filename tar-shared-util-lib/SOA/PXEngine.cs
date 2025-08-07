using System.Xml.Linq;
using TRS.IT.SI.Services;

namespace TRS.IT.SI.BusinessFacadeLayer.SOA
{
    public class PXEngine
    {
        private PXEngineService _pxSvc;

        public PXEngine()
        {
            _pxSvc = new PXEngineService(TrsAppSettings.AppSettings.GetValue("PXEngineSrvWebServiceURL"), TrsAppSettings.AppSettings.GetValue("wcfBinding"));                    
        }

        public string GetPxWithFunds(string contractId, string subId, int glidePath, string xmlFunds)
        {
            var portfolios = GetPXPortfolios(contractId, subId, glidePath, xmlFunds);
            return TRSManagers.XMLManager.GetXML(portfolios);
        }
        public Services.PXEngineSvc.PortfolioData GetPXPortfolios(string contractId, string subId, int glidePath, string xmlFunds = "", bool current = false)
        {
            if (glidePath < 1 || glidePath > 2)
            {
                throw new Exception("Invalid input value for GlidePath");
            }

            try
            {
                Services.PXEngineSvc.PortfolioData portfolios = null;
                if (xmlFunds == "")
                {
                    if (current)
                    {
                        portfolios = _pxSvc.GetPortfolioInfoCurrent(contractId, subId, (Services.PXEngineSvc.PXInputE_GLIDE_PATH)glidePath);
                    }
                    else
                    {
                        portfolios = _pxSvc.GetPortfolioInfo(contractId, subId, (Services.PXEngineSvc.PXInputE_GLIDE_PATH)glidePath);
                    }
                }
                else
                {
                    portfolios = _pxSvc.GetPortfolioInfoWithFunds(contractId, subId, (Services.PXEngineSvc.PXInputE_GLIDE_PATH)glidePath, xmlFunds);
                }

                return portfolios;
            }            
            catch (TimeoutException ex)
            {
                throw new Exception(ex.Message);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}