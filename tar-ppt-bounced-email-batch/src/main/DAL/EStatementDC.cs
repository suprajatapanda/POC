using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TarPptBouncedEmailBatch.DAL
{
    public class EStatementDC
    {
        private string _sConnectString;

        public EStatementDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public int GetEmailBouncedCount(int a_iInLoginID, string a_sContractID, string a_sSubID, int a_iePrefID)
        {
            DataSet dsePref = new();
            DataRow dr;
            int iBouncedEmailCount = 0;
            bool b = false;
            dsePref = TRSSqlHelper.ExecuteDataset(_sConnectString, "dcP_GetEmailBounceCnt", [a_iInLoginID, a_sContractID, a_sSubID, a_iePrefID]);

            if (dsePref != null && dsePref.Tables.Count > 0 && dsePref.Tables[0].Rows.Count > 0)
            {
                dr = dsePref.Tables[0].Rows[0];
                if (dr["bounced_cnt"] != null)
                {
                    b = int.TryParse(dr["bounced_cnt"].ToString(), out iBouncedEmailCount);
                }
            }
            return iBouncedEmailCount;
        }

        public int IncreaseEmailBouncedCount(int a_iInLoginID, string a_sContractID, string a_sSubID, int a_iePrefID)
        {
            return TRSSqlHelper.ExecuteNonQuery(_sConnectString, "dcP_IncreaseEmailBounceCnt", [a_iInLoginID, a_sContractID, a_sSubID, a_iePrefID]);
        }

        public int UpdateForcedOptOutDate(int a_iInLoginID, string a_sContractID, string a_sSubID, int a_iePrefID)
        {
            return TRSSqlHelper.ExecuteNonQuery(_sConnectString, "dcP_UpdateForcedOptOutDate", [a_iInLoginID, a_sContractID, a_sSubID, a_iePrefID]);
        }

        public int InsertBouncedEmailHistory(int a_iInLoginID, string a_sContractID, string a_sSubID, string a_sEmail, string a_sReasonCode, string a_sReasonDesc, int a_iProcessStatus, int a_iNotificationType)
        {
            return TRSSqlHelper.ExecuteNonQuery(_sConnectString, "dcP_InsertBouncedEmailHistory", [a_iInLoginID, a_sContractID, a_sSubID, a_sEmail, a_sReasonCode, a_sReasonDesc, a_iProcessStatus, a_iNotificationType]);
        }
    }
}
