using System.Xml.Linq;

namespace FWFundUpdatesToISCBatch.BLL
{
    public class MDP
    {
        public static XDocument GetMdpLegacyCaseObject(int runId)
        {
            var runLogRequest = new TRS.IT.SI.Services.wsMDP.RunLogInput();
            string xmlOutput = new TRS.IT.SI.BusinessFacadeLayer.SOA.MDPSOA().GetLegacyCaseObject(runId);
            return XDocument.Parse(xmlOutput);
        }
    }
}