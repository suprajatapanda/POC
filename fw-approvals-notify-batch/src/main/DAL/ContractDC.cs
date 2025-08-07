using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace FwApprovalsNotificationBatch.DAL
{
    public class ContractDC
    {
        private string _sConnectString;

        public ContractDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public int FwExpirePendingByContract(int a_iCaseNo, string a_sConId, string a_sSubId)
        {
            return TRSSqlHelper.ExecuteNonQuery(_sConnectString, "fwp_UpdatePendingExpired", [a_iCaseNo, a_sConId, a_sSubId]);
        }
        public int FwInsertNotificationHistory(int a_iCaseNo, int a_iNotifyType, string a_sTo)
        {
            return TRSSqlHelper.ExecuteNonQuery(_sConnectString, "fwp_InsertNotification", [a_iCaseNo, a_iNotifyType, a_sTo]);
        }
    }
}
