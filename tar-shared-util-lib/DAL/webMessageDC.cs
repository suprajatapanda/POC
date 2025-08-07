using System.Data;
using TRS.SqlHelper;

namespace TRS.IT.SI.BusinessFacadeLayer.DAL
{
    public class webMessageDC
    {
        private static string _sConnectString = General.ConnectionString;
        public static long GetMsgCntrCMSAcctById(string ExLoginId)
        {
            DataSet ds;
            long InLoginId;
            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pMC_GetMsgCMSAcctById", [ExLoginId]);
            if (ds.Tables[0].Rows.Count > 0)
            {
                InLoginId = Convert.ToInt64(ds.Tables[0].Rows[0]["in_login_id"]);
            }
            else
            {
                InLoginId = 0L;
            }
            return InLoginId;
        }
    }
}