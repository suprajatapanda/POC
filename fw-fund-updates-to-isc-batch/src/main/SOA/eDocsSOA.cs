using TRS.IT.SI.Services;
using TRS.IT.TrsAppSettings;

namespace FWFundUpdatesToISCBatch.SOA
{
    public class eDocsSOA
    {
        private eDocsService _oPlanClient;
        public eDocsSOA()
        {
            _oPlanClient = new eDocsService(AppSettings.GetValue("PlanServiceWebServiceURL"));
        }
        public string GetParticipantAgreementCode(string contractId, string subId)
        {
            return _oPlanClient.GetParticipantAgreementCode(contractId, subId);
        }
        public string GeteDocsFWPendingCases(string startDate, string endDate)
        {            
            return _oPlanClient.GeteDocsFWPendingCases(startDate, endDate);
        }
        public TRS.IT.SI.Services.TRSPlanService.ManagedAdvice GetManagedAdvice(string contractId, string subId, out string errorMessage)
        {
            return _oPlanClient.GetManagedAdvice(contractId, subId, out errorMessage);
        }
    }
}
