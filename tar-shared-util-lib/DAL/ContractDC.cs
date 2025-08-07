using System.Data;
using TRS.SqlHelper;
using SOAModel = TRS.IT.SOA.Model;

namespace TRS.IT.SI.BusinessFacadeLayer.DAL
{

    public class ContractDC
    {

        #region *** Constructors ***
        public ContractDC(string a_sSessionId, string a_sConId, string a_sSubId)
        {
            _sSessionId = a_sSessionId;
            _sConId = a_sConId;
            _sSubId = a_sSubId;
        }


        #endregion

        #region *** Private members ***
        private string _sConnectString = General.ConnectionString;
        private string _sConId;
        private string _sSubId;
        private string _sSessionId;
        #endregion

        #region *** Fund wizard ***
        public DataSet FwGetFundSelection(string a_sConId, string a_sSubId, int a_iCaseNo)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetFundSelection", [a_sConId, a_sSubId, a_iCaseNo]);
        }
        public int FwInsertTask(int a_iCaseNo, int a_iTaskNo, int a_iStatus, string a_sXml)
        {
            return TRSSqlHelper.ExecuteNonQuery(_sConnectString, "fwp_InsertTask", [a_iCaseNo, a_iTaskNo, a_iStatus, a_sXml]);
        }
        public DataSet FwGetTaskByTaskNo(int a_iCaseNo, int a_iTaskNo)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetTaskByTaskNo", [a_iCaseNo, a_iTaskNo]);
        }
        public int FwUpdateComplete(int a_iCaseNo, string a_sConId, string a_sSubId)
        {
            return TRSSqlHelper.ExecuteNonQuery(_sConnectString, "fwp_UpdateFWComplete", [a_iCaseNo, a_sConId, a_sSubId]);
        }
        public DataSet FwGetDocsToImage(int a_iCaseNo)
        {
            return TRSSqlHelper.ExecuteDataset(General.ConnectionString, "fwp_GetTasksByCaseNo", [a_iCaseNo]);
        }
        public List<SOAModel.FundPendingChanges> FwGetPendingFundChangeByContractMigrated()
        {
            SOAModel.FundPendingChanges oFundPendingChange;
            var oFundPendingChanges = new List<SOAModel.FundPendingChanges>();
            var ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "fwp_GetPendingFundChangesByContract", [_sConId, _sSubId]);
            int I;
            var loopTo = ds.Tables[0].Rows.Count - 1;
            for (I = 0; I <= loopTo; I++)
            {
                oFundPendingChange = new SOAModel.FundPendingChanges();
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
        #endregion
    }
}