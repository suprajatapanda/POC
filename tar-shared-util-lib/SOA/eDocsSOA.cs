using TRS.IT.SI.Services;

namespace TRS.IT.SI.BusinessFacadeLayer.SOA
{
    public class eDocsSOA
    {
        private TRSPlanProvService _wsTrsPlanProv;

        public eDocsSOA()
        {
            _wsTrsPlanProv = new TRSPlanProvService(TrsAppSettings.AppSettings.GetValue("EDocsWebServiceURL"));
        }

        public string DocGenFundRider(string contractId, string subId, string userId)
        {
            return _wsTrsPlanProv.DocGenFundRider(contractId, subId, userId);
        }

        public string DocGenQDIA(string contractId, string subId, string userId)
        {
            return _wsTrsPlanProv.DocGenQDIA(contractId, subId, userId);
        }
    }
}