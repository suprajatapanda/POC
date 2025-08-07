using TRS.IT.SI.Services;
using SOAModel = TRS.IT.SOA.Model;

namespace TRS.IT.BendProcessor.DriverSOA
{
    public class TPASvc
    {
        private TPAService _wsTPASvc;

        public TPASvc()
        {
            _wsTPASvc = new TPAService(TrsAppSettings.AppSettings.GetValue("TPAWebServiceURL"));
        }
        public SOAModel.TPACompanyContactInformations GetContractTPAContacts(string contractId, string subId)
        {
            var wsTPACompanyContactInfos = _wsTPASvc.GetContractTPAContacts(contractId, subId);
            return (SOAModel.TPACompanyContactInformations)TRSManagers.ConvertManager.CType(wsTPACompanyContactInfos, typeof(SOAModel.TPACompanyContactInformations));
        }
    }
}