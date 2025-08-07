using System.Data;
using TRS.IT.SI.Services;
using TRS.IT.SOA.Model;

namespace TRS.IT.SI.BusinessFacadeLayer.SOA
{
    public class FundContractsSoa
    {
        private FundContractsService _wsFundContracts;

        public FundContractsSoa()
        {
            _wsFundContracts = new FundContractsService(TrsAppSettings.AppSettings.GetValue("FundContractsServiceURL"));
        }

        private Services.wsFundContractsService.FundContracts GetContracts(string contractId, string subId)
        {
            return _wsFundContracts.GetContracts(contractId, subId);
        }

        public DataSet GetContractsDataSet(string contractId, string subId)
        {
            DataSet ds = null;
            var oFundContracts = GetContracts(contractId, subId);
            string sFundContractsXml = TRSManagers.XMLManager.GetXML(oFundContracts);

            if (string.IsNullOrEmpty(sFundContractsXml))
            {
                return ds;
            }

            try
            {
                using (var stringReader = new StringReader(sFundContractsXml))
                {
                    ds = new DataSet();
                    ds.ReadXml(stringReader);
                }
            }
            catch
            {
                ds = null;
            }
            return ds;
        }
        public List<PlanContactInfo> GetDesignatedContacts(string contractId, string subId)
        {
            var oContacts = new List<PlanContactInfo>();
            var oFundContacts = new FundContractsSoa().GetContracts(contractId, subId);

            if (oFundContacts.Succeeded && !(oFundContacts.ContractCollection == null))
            {
                foreach (Services.wsFundContractsService.FundContractsData oC in oFundContacts.ContractCollection)
                {
                    if (oC.StatementID == 2001)
                    {
                        var oContact = new PlanContactInfo();
                        oContact.IndividualID = oC.IndividualID;
                        oContacts.Add(oContact);
                    }
                }
            }
            return oContacts;
        }
    }
}