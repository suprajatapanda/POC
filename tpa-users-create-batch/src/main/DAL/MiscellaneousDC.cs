using System.Data;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TpaUsersCreateBatch.DAL
{
    public class MiscellaneousDC
    {
        private string _sConnectString;
        const int NumberOfDays = -30;
        public MiscellaneousDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet CreateTpaLiteIds()
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "dlpp_ProcessCreateTpaLiteIds", [DateTime.Today.AddDays(NumberOfDays), "1"]);
        }
    }
}
