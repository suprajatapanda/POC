using System.Data;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TRS.IT.BendProcessor.DAL
{
    public class GeneralDC
    {
        #region **** member variables ******

        private string _sConnectString;

        #endregion

        public GeneralDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public int GetMsgCtrAcctByExLoginId(string a_sExLoginId)
        {
            DataSet ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pMC_GetMsgCMSAcctById", [a_sExLoginId]);
            if (ds.Tables[0].Rows.Count > 0)
            {
                return (int)ds.Tables[0].Rows[0]["in_login_id"];
            }
            else
            {
                return 0;
            }
        }
        public string SubOut(string a_sSubId)
        {
            return TRSSqlHelper.ExecuteScalar(_sConnectString, "pSI_GetSubOut", [a_sSubId]).ToString();
        }

    }
}
