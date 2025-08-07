using System.Data;
using Microsoft.Data.SqlClient;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer.Model;
using TRS.SqlHelper;

namespace TRS.IT.SI.BusinessFacadeLayer.DAL
{
    public class SponsorDC
    {
        private string _ContractID, _SubID;

        public SponsorDC(string ContractID, string subID)
        {
            _ContractID = ContractID;
            _SubID = subID;
        }
        public string GetSponsorName()
        {
            return Convert.ToString(TRSSqlHelper.ExecuteScalar(General.ConnectionString, "paGetClientName", [_ContractID, _SubID]));
        }
        public DataSet GetPlanLevelData()
        {
            return TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_getContractInfo", [_ContractID, _SubID]);
        }
        public static PartnerFlag GetPartner(string sessionID, ref string strPlanID, ref string strPartnerUserID)
        {
            string strPartner;
            PartnerFlag iPartner;
            SqlDataReader dr = null;

            try
            {
                // retrieve partner name from database
                dr = TRSSqlHelper.ExecuteReader(General.ConnectionString, "pSI_GetSponsorPartnerInfoBySession", [sessionID]);
                // return partner name
                if (dr.Read())
                {
                    if (!ReferenceEquals(dr["partner_plan_id"], DBNull.Value))
                    {
                        strPlanID = Convert.ToString(dr["partner_plan_id"]);
                    }

                    strPartnerUserID = Convert.ToString(dr["partner_userid"]);
                    iPartner = (PartnerFlag)Convert.ToInt32(dr["partner_id"]);
                }
                else
                {
                    strPlanID = string.Empty;
                    strPartnerUserID = string.Empty;
                    strPartner = string.Empty;
                    iPartner = default;
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                iPartner = default;
            }
            finally
            {
                if (dr != null)
                {
                    dr.Close();
                }
            }
            return iPartner;
        }
        public static string CreateSession(int InLoginID, string contractID, string subID, string sessionID = null)
        {
            return Convert.ToString(TRSSqlHelper.ExecuteScalar(General.ConnectionString, "pSI_CreateSponsorSessionByInLoginID", [InLoginID, contractID, subID, sessionID]));
        }
        public static PartnerFlag GetPartnerID(string sessionID)
        {
            string argstrPlanID = null;
            string argstrPartnerUserID = null;
            return GetPartner(sessionID, ref argstrPlanID, ref argstrPartnerUserID);
        }
        public string GetReverseFeedFileFormatType()
        {
            return Convert.ToString(TRSSqlHelper.ExecuteScalar(General.ConnectionString, "pSI_ReverseFeedFileFormatType", [_ContractID, _SubID]));
        }

    }
}