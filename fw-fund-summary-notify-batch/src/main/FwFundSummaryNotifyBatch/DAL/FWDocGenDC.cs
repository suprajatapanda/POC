using System.Data;
using TRS.IT.SI.BusinessFacadeLayer.DAL;
using TRS.SqlHelper;

namespace FWFundSummaryNotifyBatch.DAL
{
    public class FWDocGenDC
    {
        public static DataSet? GetSummaryFundChangesData(string partnerId, DateTime effectiveDate)
        {
            var connectionString = General.ConnectionString;

            // Execute stored procedure and return dataset
            return TRSSqlHelper.ExecuteDataset(
                     connectionString,
                     "fwp_GetFundChangesSummary",
                      new object[] { partnerId, effectiveDate }
                    );
        }

    }
}
