using System.Data;
using TRS.SqlHelper;

namespace FWFundUpdatesToISCBatch.DAL
{
    class FWDocGenDC
    {
        public static DataSet GetP3FundsData(string FundID)
        {
            string sConnectionString = "";

            sConnectionString = TRS.IT.SI.BusinessFacadeLayer.DAL.General.ConnectionString;
            return TRSSqlHelper.ExecuteDataset(sConnectionString, "pp3_GetP3FundInfo", [Convert.ToInt32(FundID)]);
        }
    }
}
