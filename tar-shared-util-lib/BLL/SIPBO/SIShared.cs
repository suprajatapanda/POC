using TRS.IT.BendProcessor.Util;
using TRS.SqlHelper;

namespace SIPBO
{
    public class SIShared
    {
        public static string GetPlanName(string a_sConID, string a_sSubID)
        {
            Microsoft.Data.SqlClient.SqlDataReader dr;
            string sPlanName;
            dr = TRSSqlHelper.ExecuteReader(TRS.IT.SI.BusinessFacadeLayer.DAL.General.ConnectionString, "pSI_getPlanNamebyCID", [a_sConID, a_sSubID]);
            if (dr.Read())
            {
                sPlanName = Utils.CheckDBNull(dr["plan_name"]);
            }
            else
            {
                sPlanName = "";
            }
            dr.Close();
            return sPlanName;
        }
    }
}