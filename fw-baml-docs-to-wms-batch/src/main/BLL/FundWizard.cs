using System.Xml.Linq;
using System.Xml.Serialization;
using TRS.IT.SI.BusinessFacadeLayer.SOA;
using TRS.IT.SOA.Model.PreSales.FundLineupData;

namespace FWBamlDocsToWMSBatch.BLL
{
    public class FundWizard
    {
        public static FMRS GetFMRSFundsByContract(string contractID, string subID, DateTime asOfDate, string applicationId)
        {
            if (applicationId is null || applicationId.Equals(""))
            {
                applicationId = 1025.ToString();
            }

            var oFundInfo = new FundInfoSoa();
            var serviceRequest = new XElement("FMRS", new XAttribute("Type", "FundLineup"), new XAttribute("AsOfDate", asOfDate.ToString("yyyy-MM-ddThh:mm:ssZ")), new XElement("Contract", new XAttribute("ContractID", contractID), new XAttribute("SubID", subID)), new XElement("User", new XAttribute("UsrName", @"US\\sptlatrssoa")), new XElement("Application", new XAttribute("ApplicationID", applicationId)));
            string serviceResult = oFundInfo.GetFmrxXml(serviceRequest.ToString());

            var ser = new XmlSerializer(typeof(FMRS));
            var reader = new StringReader(serviceResult);
            return (FMRS)ser.Deserialize(reader);
        }
    }
}