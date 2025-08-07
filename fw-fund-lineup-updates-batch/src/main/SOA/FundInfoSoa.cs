using TRS.IT.SI.BusinessFacadeLayer.Model;

namespace FWFundLineupUpdatesBatch.SOA
{
    public class FundInfoSoa
    {
        private TRS.IT.SI.Services.ContractService _wsContract;
        public FundInfoSoa()
        {
            _wsContract = new TRS.IT.SI.Services.ContractService(TRS.IT.TrsAppSettings.AppSettings.GetValue("ContractWebServiceURL"));
        }
        public string GetCustomPxFunds(string contractId, string subId)
        {
            return _wsContract.GetCustomPxFunds(contractId, subId);
        }
        public SIResponse SetCustomPxFunds(string contractId, string subId, string xmlCustomFunds)
        {
            var oResponse = _wsContract.SetCustomPxFunds(contractId, subId, xmlCustomFunds);

            var SIResponse = new SIResponse();
            SIResponse.Errors[0].Number = oResponse.Errors[0].Number;
            SIResponse.Errors[0].Description = oResponse.Errors[0].Description;
            return SIResponse;
        }
    }
}
