using TRS.IT.SI.Services;

namespace TRS.IT.SI.BusinessFacadeLayer.SOA
{
    public class MDPSOA
    {
        private MDPService _wsMDP;

        public MDPSOA()
        {
            _wsMDP = new MDPService(TrsAppSettings.AppSettings.GetValue("MDPWebServiceURL"));
        }

        public string GetLegacyCaseObject(int runId)
        {
            return _wsMDP.GetLegacyCaseObject(runId.ToString(), 0);
        }
    }
}