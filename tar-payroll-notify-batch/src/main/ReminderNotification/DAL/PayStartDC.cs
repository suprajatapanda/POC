using System.Data;
using Microsoft.Data.SqlClient;
using TRS.IT.BendProcessor.Model;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace ReminderNotificationBatch.DAL
{
    public class PayStartDC
    {
        private string _sConnectString;
        public PayStartDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet GetDailyJobRemindersInfo()
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "pPS_GetReminder");
        }
        public DataSet GetOptInNPassInfo(string a_sConId, string a_sSubId, bool rFlag = false)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "pPS_GetOptInNPassInfo", [a_sConId, a_sSubId, rFlag]);
        }
        public int InsertPayrollNotification(int row_id, int notify_type, string a_sNotificationTo)
        {
            return TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pPS_InsertPayStartNotification", [row_id, notify_type, a_sNotificationTo]);
        }
        public MessageTemplateContact GetClientSupportContact(string a_sConId, string a_sSubId)
        {
            MessageTemplateContact oContact = new();
            using (SqlDataReader rd = TRSSqlHelper.ExecuteReader(_sConnectString, "psi_getcontracthomeofficecontact", [a_sConId, a_sSubId]))
            {
                if (rd.Read())
                {
                    oContact.name = (rd["trscontactname"].ToString());
                    oContact.phone = (rd["trscontactphone"].ToString());
                }
                if ((rd != null) && (!rd.IsClosed))
                {
                    rd.Close();
                }
            }
            return oContact;
        }
    }
}
