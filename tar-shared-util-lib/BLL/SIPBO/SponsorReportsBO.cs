using System.Data;
using TRS.SqlHelper;

namespace SIPBO
{
    public class SponsorReportsBO
    {
        public static DataSet GetAvailableReports(string contract_id, string sub_id, int report_type_id, string ApplicationName, int hours_sincecreation = 72)
        {
            var ds = new DataSet();
            ds = TRSSqlHelper.ExecuteDataset(TRS.IT.SI.BusinessFacadeLayer.DAL.General.ConnectionString, "pSI_GetReports", [contract_id, sub_id, report_type_id, ApplicationName, hours_sincecreation]);
            return ds;
        }

    }
}