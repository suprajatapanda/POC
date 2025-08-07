using TRS.IT.SI.Services;
using TRS.IT.SI.Services.wsContract;
using SOAModel = TRS.IT.SOA.Model;

namespace TRS.IT.SI.BusinessFacadeLayer.SOA
{
    public class ContractSoa
    {
        private ContractService _wsConService;
        private string _sCallerId;

        public ContractSoa(string callerId)
        {
            _sCallerId = callerId;
            _wsConService = new ContractService(TrsAppSettings.AppSettings.GetValue("ContractWebServiceURL"));
        }

        public SOAModel.ContractInfo GetContractInformation(string contractId, string subId, SOAModel.AdditionalData additionalData)
        {
            var wsAdditionalData = (AdditionalData)TRSManagers.ConvertManager.CType(additionalData, typeof(AdditionalData));
            var wsContractInfo = _wsConService.GetContractInformation(contractId, subId, wsAdditionalData);
            return (SOAModel.ContractInfo)TRSManagers.ConvertManager.CType(wsContractInfo, typeof(SOAModel.ContractInfo));
        }

        public string SubmitTestingResults(string xmlTestingResults)
        {
            return _wsConService.SubmitTestingResults(xmlTestingResults);
        }
    }
}