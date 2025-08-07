using SOAModel = TRS.IT.SOA.Model;

namespace TRS.IT.SI.BusinessFacadeLayer
{
    public class Tpa
    {
        private SOA.TpaSoa _SOA_TpaAdapter = new();
        public SOAModel.TPACompanyContactInformations GetContractTPAContacts(string Contract_id, string Sub_id)
        {
            return _SOA_TpaAdapter.GetContractTPAContacts(Contract_id, Sub_id);
        }

    }
}