using System.Data;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace DailyVendorNotificationBatch.DAL
{
    public class PayStartDC
    {
        private string _sConnectString;

        public PayStartDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        
        public DataSet GetDailyJobActivitySummary(string sVendor, DateTime dtStartDate, DateTime dtEndDate)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "pPS_GetDailyJobActivitySummary", [sVendor, dtStartDate.ToString(), dtEndDate.ToString()]);
        }
        public DataSet GetPayStartVenders()
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "pPs_GetPaystartVendors");
        }


    }
}
