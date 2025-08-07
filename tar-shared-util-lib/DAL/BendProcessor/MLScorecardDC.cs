using System.Data;
using Microsoft.Data.SqlClient;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;
namespace TRS.IT.BendProcessor.DAL
{
    public class MLScorecardDC
    {
        private string _sConnectString;

        public MLScorecardDC()
        {
            _sConnectString = AppSettings.GetValue("web_main_DW_ConnectString");
        }
        public DataSet GetReportDateAndTimeAllainceKey()
        {
            DataSet ds = new();
            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pSI_GetReportDateAndTimeAllainceKey", []);
            return ds;
        }
        public DataSet GetAgreegateFileData(int iTimeAllianceKey, string strPrvMonthDate)
        {
            DataSet ds = new();
            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pMLDW_GetAgreegateFileData", [iTimeAllianceKey, strPrvMonthDate]);
            return ds;
        }
        public DataSet GetPlanDetailFileData(int iTimeAllianceKey)
        {
            DataSet ds = new();
            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pMLDW_GetPlanDetailFlatFile", [iTimeAllianceKey]);
            return ds;
        }

        public DataSet GetRolloverFileData(int iTimeAllianceKey)
        {
            DataSet ds = new();
            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pMLDW_GetRolloverFlatFile", [iTimeAllianceKey]);
            return ds;
        }
        public DataSet GetISCContracts(DateTime dtAsOfDate)
        {
            DataSet ds = new();
            string sWeb_mainConnectString = "";
            sWeb_mainConnectString = AppSettings.GetConnectionString("ConnectString");

            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pSI_GetISCContracts", [dtAsOfDate]);
            return ds;
        }
        public string GetPptWithBalanceCount(string sInputContracts, DateTime dtAsofDate) // sInputContracts = "'300019    00000', '300050    00000', '300069    00000'"
        {
            string sReturnXML = "";  // Format: "<Cases><Case><Id>300019    00000</Id><PptCountWbal>510</PptCountWbal></Case><Case><Id>300050    00000</Id><PptCountWbal>222</PptCountWbal></Case><Case><Id>300069    00000</Id><PptCountWbal>39</PptCountWbal></Case></Cases>"

            DriverSOA.SponsorService DriverSOASpo = new();
            sReturnXML = DriverSOASpo.GetPptWithBalanceCount(sInputContracts, dtAsofDate);
            return sReturnXML;
        }

        public int Insert_wn_part_hdr_Data(string contract_id, string sub_id, string partner_id, int total_employees, int ipptWbalCount, int num_elig_parts, int gt0_bal_parts)
        {
            int iRet = 0;
            string sWeb_mainConnectString = AppSettings.GetConnectionString("ConnectString");
            // as of now total_employees  and num_elig_parts are 0 and ipptWbalCount = gt0_bal_parts
            iRet = TRSSqlHelper.ExecuteNonQuery(sWeb_mainConnectString, "pMLDW_Insert_wn_part_hdr", [contract_id, sub_id, partner_id, total_employees, ipptWbalCount, num_elig_parts, gt0_bal_parts]);
            return iRet;
        }

        public int Delete_wn_part_hdr_Data(string partner_id)
        {
            int iRet = 0;
            string sWeb_mainConnectString = AppSettings.GetConnectionString("ConnectString");

            iRet = TRSSqlHelper.ExecuteNonQuery(sWeb_mainConnectString, "pMLDW_CleanUp_wn_part_hdr", [partner_id]);
            return iRet;
        }

        public DataSet GetBORAssets(int iAllianceID, DateTime dtRptPeriodBeginDay, DateTime dtRptPeriodEndDay)
        {
            DataSet ds = new();

            string spName = "ml_detRpt_BORAssets";

            using (SqlCommand cmd = new(spName, new SqlConnection(_sConnectString)))
            {
                cmd.CommandTimeout = 600;// use bigger time out for this sp
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter p0 = new("@a_iAllianceID", SqlDbType.Int);
                p0.Direction = ParameterDirection.Input;
                p0.Value = iAllianceID;
                cmd.Parameters.Add(p0);

                SqlParameter p1 = new("@a_dtFrom", SqlDbType.DateTime);
                p1.Direction = ParameterDirection.Input;
                p1.Value = dtRptPeriodBeginDay;
                cmd.Parameters.Add(p1);

                SqlParameter p2 = new("@a_dtTo", SqlDbType.DateTime);
                p2.Direction = ParameterDirection.Input;
                p2.Value = dtRptPeriodEndDay;
                cmd.Parameters.Add(p2);

                cmd.Connection.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    DataTable table = new();
                    table.Load(rd);
                    ds.Tables.Add(table);
                }
                cmd.Connection.Close();
            }

            return ds;
        }

        public DataSet GetBORTermProcessed(int iAllianceID, DateTime dtRptPeriodBeginDay, DateTime dtRptPeriodEndDay, DateTime dtRptYTDFr, DateTime dtInception)
        {
            DataSet ds = new();

            string spName = "ml_detRpt_GetBORTermProcessed";

            using (SqlCommand cmd = new(spName, new SqlConnection(_sConnectString)))
            {
                cmd.CommandTimeout = 600;// use bigger time out for this sp
                cmd.CommandType = CommandType.StoredProcedure;

                SqlParameter p0 = new("@a_iAllianceID", SqlDbType.Int);
                p0.Direction = ParameterDirection.Input;
                p0.Value = iAllianceID;
                cmd.Parameters.Add(p0);

                SqlParameter p1 = new("@a_dtFrom", SqlDbType.DateTime);
                p1.Direction = ParameterDirection.Input;
                p1.Value = dtRptPeriodBeginDay;
                cmd.Parameters.Add(p1);

                SqlParameter p2 = new("@a_dtTo", SqlDbType.DateTime);
                p2.Direction = ParameterDirection.Input;
                p2.Value = dtRptPeriodEndDay;
                cmd.Parameters.Add(p2);

                SqlParameter p3 = new("@a_ytdFr", SqlDbType.DateTime);
                p3.Direction = ParameterDirection.Input;
                p3.Value = dtRptYTDFr;
                cmd.Parameters.Add(p3);

                SqlParameter p4 = new("@a_incdt", SqlDbType.DateTime);
                p4.Direction = ParameterDirection.Input;
                p4.Value = dtInception;
                cmd.Parameters.Add(p4);

                cmd.Connection.Open();
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    DataTable table = new();
                    table.Load(rd);
                    ds.Tables.Add(table);
                }
                cmd.Connection.Close();
            }

            return ds;
        }


    }
}
