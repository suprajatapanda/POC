using TRS.SqlHelper;

namespace FWSignedDocsToMsgcntrBatch.DAL
{
    public class ContractDC
    {
        private string _sConId;
        private string _sSubId;
        private string _sSessionId;
        public ContractDC(string a_sSessionId, string a_sConId, string a_sSubId)
        {
            _sSessionId = a_sSessionId;
            _sConId = a_sConId;
            _sSubId = a_sSubId;
        }
        private string _sConnectString = TRS.IT.SI.BusinessFacadeLayer.DAL.General.ConnectionString;
        public List<TRS.IT.SOA.Model.FundPendingChanges> FwGetPendingFundChangeByContract()
        {
            TRS.IT.SOA.Model.FundPendingChanges oFundPendingChange;
            var oFundPendingChanges = new List<TRS.IT.SOA.Model.FundPendingChanges>();
            var ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetPendingFundChangesByContract", [_sConId, _sSubId]);
            int I;
            var loopTo = ds.Tables[0].Rows.Count - 1;
            for (I = 0; I <= loopTo; I++)
            {
                oFundPendingChange = new TRS.IT.SOA.Model.FundPendingChanges();
                oFundPendingChange.CaseNo = Convert.ToString(ds.Tables[0].Rows[I]["case_no"]);
                oFundPendingChange.EffectiveDate = Convert.ToString(ds.Tables[0].Rows[I]["pegasys_dt"]);
                oFundPendingChange.Action = Convert.ToString(ds.Tables[0].Rows[I]["action"]);
                oFundPendingChange.FundID = Convert.ToString(ds.Tables[0].Rows[I]["fund_id"]);
                oFundPendingChange.FundName = Convert.ToString(ds.Tables[0].Rows[I]["fund_name"]);
                oFundPendingChange.ToFundID = Convert.ToString(ds.Tables[0].Rows[I]["to_fund_id"]);
                oFundPendingChange.ToFundName = Convert.ToString(ds.Tables[0].Rows[I]["to_fund_name"]);
                oFundPendingChanges.Add(oFundPendingChange);
            }
            return oFundPendingChanges;
        }
    }
}
