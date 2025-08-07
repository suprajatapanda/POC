using TRS.SqlHelper;

namespace FWInitialFundUpdatesBatch.DAL
{
    public class ContractDC
    {
        public int FwUpdatePending(int a_iCaseNo, string a_sConId, string a_sSubId, int a_iNewCase)
        {
            return TRSSqlHelper.ExecuteNonQuery(TRS.IT.SI.BusinessFacadeLayer.DAL.General.ConnectionString, "fwp_UpdateFWPending", [a_iCaseNo, a_sConId, a_sSubId, a_iNewCase]);
        }
    }
}
