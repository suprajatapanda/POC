using System.Xml.Linq;
using TRS.IT.TRSManagers;
using TARSharedUtilLibBFLSOA = TRS.IT.SI.BusinessFacadeLayer.SOA;
using TARSharedUtilLibSOA = TRS.IT.SI.Services;
namespace FWFundUpdatesToISCBatch.SOA
{
    public class PXEngine
    {
        TARSharedUtilLibBFLSOA.PXEngine _pxEngine;
        eDocsSOA _eDocsSOA;
        public PXEngine(TARSharedUtilLibBFLSOA.PXEngine obj) {
            _pxEngine = obj;
            _eDocsSOA = new eDocsSOA();
        }
        public string GeneratePxInvestmentMixXml(string customPx, TARSharedUtilLibSOA.PXEngineSvc.PortfolioData portfolios = null)
        {
            var pxDoc = XDocument.Parse(customPx);
            var contractId = pxDoc.Root.Element("contractId").Value;
            var subId = pxDoc.Root.Element("subId").Value;
            var fiduciaryName = pxDoc.Root.Element("fiduciaryName").Value;
            var agreementCode = pxDoc.Root.Element("agreementCode").Value;
            var pegasysDt = pxDoc.Root.Element("pegasysDt").Value;
            var glidePath = int.Parse(pxDoc.Root.Element("glidePath").Value);

            if (portfolios == null)
                portfolios = _pxEngine.GetPXPortfolios(contractId, subId, glidePath);

            var filePath = GetFileName(contractId, subId, glidePath);
            var fileContent = TRS.IT.TRSManagers.XMLManager.GetXML(portfolios);

            if (fiduciaryName == string.Empty)
                fiduciaryName = "Plan Sponsor";

            if (agreementCode == string.Empty)
            {
                var agreementCodeXml = _eDocsSOA.GetParticipantAgreementCode(contractId, subId);
                if (agreementCodeXml != string.Empty)
                {
                    var xD = XDocument.Parse(agreementCodeXml);
                    agreementCode = xD.Descendants("PXAgreementCode").FirstOrDefault().Value;
                }
            }

            var xDoc = XDocument.Parse(fileContent);
            xDoc.Root.AddFirst(new XElement("FiduciaryName", fiduciaryName));
            xDoc.Root.AddFirst(new XElement("AgreementCode", agreementCode));
            if (glidePath == 1)
                glidePath = 2;
            else if (glidePath == 2)
                glidePath = 1;

            xDoc.Root.AddFirst(new XElement("GlidePath", glidePath));
            xDoc.Root.AddFirst(new XElement("AccountNo", contractId.PadRight(10, ' ') + subId.PadLeft(5, '0')));
            pegasysDt = DateTime.Parse(pegasysDt).ToString("yyyy-MM-ddT00:00:00");
            xDoc.Descendants("StartDt").FirstOrDefault().SetValue(pegasysDt);
            SaveFile(xDoc.ToString(), filePath);
            return filePath;
        }
        private static string GetFileName(string contractId, string subId, int glidePath)
        {
            var fileFolder = TRS.IT.TrsAppSettings.AppSettings.GetValue("PxFilePath") ?? @"\\crasdiabattst03\LoadCscPortfolios\XML\";
            var fileName = String.Format("CSC_EM_Portfolios_{0}_{1}_{2}.xml", contractId, subId, glidePath);
            return fileFolder + fileName;
        }
        private static void SaveFile(string fileContent, string filePath)
        {
            string result = FileManager.WriteRemoteFile(filePath, fileContent, false);
            if (result != "0")
            {
                throw new Exception($"Failed to save file to {filePath}: {result}");
            }
        }
    }
}
