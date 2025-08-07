using TRS.IT.SI.Services;
using SOAModel = TRS.IT.SOA.Model;

namespace TRS.IT.SI.BusinessFacadeLayer.SOA
{
    public class TpaSoa
    {
        private TPAService _wsTPA;

        public TpaSoa()
        {
            _wsTPA = new TPAService(TrsAppSettings.AppSettings.GetValue("TPASrvWebServiceURL"));
        }

        public SOAModel.TPACompanyContactInformations GetContractTPAContacts(string contractId, string subId)
        {
            var wsTPACompanyContactInfos = _wsTPA.GetContractTPAContacts(contractId, subId);
            return (SOAModel.TPACompanyContactInformations)TRSManagers.ConvertManager.CType(wsTPACompanyContactInfos, typeof(SOAModel.TPACompanyContactInformations));
        }
    }
}