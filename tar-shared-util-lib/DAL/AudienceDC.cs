using System.Data;
using Microsoft.Data.SqlClient;
using SIUtil;
using TRS.SqlHelper;

namespace TRS.IT.SI.BusinessFacadeLayer.DAL
{

    public class SessionInfo
    {
        public int InLoginID;
        public string ContractID;
        public string SubID;
        public string PartnerUserID;
        public string PlanID;
        public Model.PartnerFlag PartnerID;
        public string HostID;
        public string LocationCode;
        public string PegasysUserId;
    }

    public class AudienceDC
    {
        private const string C_FINANCIAL = "Financial";
        public static SessionInfo GetSessionInfo(string SessionID)
        {
            SqlDataReader dr;
            SessionInfo oSessionInfo;

            dr = TRSSqlHelper.ExecuteReader(General.ConnectionString, "pSI_GetSessionInfo", [SessionID]);

            if (dr.Read())
            {
                oSessionInfo = new SessionInfo();
                oSessionInfo.InLoginID = Convert.ToInt32(dr["in_login_id"]);
                oSessionInfo.PartnerUserID = Convert.ToString(ReferenceEquals(dr["ex_user_id"], DBNull.Value) ? "" : dr["ex_user_id"]);
                oSessionInfo.HostID = Convert.ToString(ReferenceEquals(dr["host_id"], DBNull.Value) ? "" : dr["host_id"]);
                oSessionInfo.ContractID = Convert.ToString(ReferenceEquals(dr["contract_id"], DBNull.Value) ? "" : dr["contract_id"]);
                oSessionInfo.SubID = Convert.ToString(ReferenceEquals(dr["sub_id"], DBNull.Value) ? "" : dr["sub_id"]);
                oSessionInfo.PegasysUserId = Convert.ToString(ReferenceEquals(dr["pegasys_userid"], DBNull.Value) ? "" : dr["pegasys_userid"]);
                if (!ReferenceEquals(dr["plan_id"], DBNull.Value))
                {
                    oSessionInfo.PlanID = Convert.ToString(dr["plan_id"]);
                }
                oSessionInfo.LocationCode = Convert.ToString(ReferenceEquals(dr["location"], DBNull.Value) ? "" : dr["location"]);
                dr.Close();
            }
            else
            {
                dr.Close();
                throw new Exception("Invalid Session or unable to retirive session information for the user.");
            }
            return oSessionInfo;
        }
        public static string GetObjectDataByDate(string sDate, Model.Enums.E_ObjectType ObjectType, string ContractID, string SubID)
        {
            SqlParameter[] @params;
            string StrXML = "";
            string StoredProc;
            StoredProc = "pSI_GetObjectDataByDate";

            DataSet ds;

            @params = SqlHelperParameterCache.GetSpParameterSet(General.ConnectionString, StoredProc);
            @params[0].Value = sDate;
            @params[1].Value = Convert.ToInt32((int)ObjectType);
            @params[2].Value = ContractID;
            @params[3].Value = SubID;
            ds = TRSSqlHelper.ExecuteDataset(General.ConnectionString, CommandType.StoredProcedure, StoredProc, @params);
            if (ds.Tables[0].Rows.Count > 0)
            {
                StrXML = ds.Tables[0].Rows[0]["XML"].ToString();
            }
            else
            {
                StrXML = string.Empty;
            }

            return StrXML;


        }
        public static void SaveObjectDataByDate(string SessionID, Model.Enums.E_ObjectType ObjectType, string XML, string ContractID = "", string SubID = "")
        {
            try
            {
                if (!(SessionID == null) && SessionID.Length > 0)
                {
                    SqlParameter[] @params;
                    string StoredProc;
                    StoredProc = "pSI_SaveObjectDataByDate";

                    @params = SqlHelperParameterCache.GetSpParameterSet(General.ConnectionString, StoredProc);
                    @params[0].Value = SessionID;
                    @params[1].Value = Convert.ToInt32((int)ObjectType);
                    @params[2].Value = XML;
                    @params[3].Value = ContractID;
                    @params[4].Value = SubID;
                    TRSSqlHelper.ExecuteNonQuery(General.ConnectionString, CommandType.StoredProcedure, StoredProc, @params);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
            }


        }
        public static int UpdateTransactionStatus(string SessionID, Model.PartnerFlag partnerID, Model.TransactionType @type, Model.TransactionStatus status, string transactionData, string ErrorCode = null, string PartnerConfID = null)
        {
            int retVal = 0;
            SqlParameter[] @params;
            try
            {
                @params = SqlHelperParameterCache.GetSpParameterSet(General.ConnectionString, "pSI_SetTransactionStatus");
                @params[0].Value = SessionID;
                @params[1].Value = partnerID;
                @params[2].Value = type;
                @params[3].Value = status;
                @params[4].Value = transactionData;
                @params[6].Value = ErrorCode;
                @params[7].Value = PartnerConfID;
                TRSSqlHelper.ExecuteNonQuery(General.ConnectionString, CommandType.StoredProcedure, "pSI_SetTransactionStatus", @params);

                retVal = Convert.ToInt32(@params[5].Value);
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                var sbErr = new System.Text.StringBuilder(ex.Message);
                sbErr.AppendLine(ex.StackTrace);
                sbErr.AppendLine();
                sbErr.AppendLine("Parameters passed:");
                try
                {
                    sbErr.AppendFormat("SessionID: [{0}]", string.IsNullOrEmpty(SessionID) ? string.Empty : SessionID);
                    sbErr.AppendLine();
                    sbErr.AppendFormat("partnerID: [{0}]", partnerID);
                    sbErr.AppendLine();
                    sbErr.AppendFormat("type: [{0}]", type);
                    sbErr.AppendLine();
                    sbErr.AppendFormat("status: [{0}]", status);
                    sbErr.AppendLine();
                    sbErr.AppendFormat("transactionData: [{0}]", string.IsNullOrEmpty(transactionData) ? string.Empty : transactionData);
                    sbErr.AppendLine();
                    sbErr.AppendFormat("ErrorCode: [{0}]", string.IsNullOrEmpty(ErrorCode) ? string.Empty : ErrorCode);
                    sbErr.AppendLine();
                    sbErr.AppendFormat("PartnerConfID: [{0}]", string.IsNullOrEmpty(PartnerConfID) ? string.Empty : PartnerConfID);
                    sbErr.AppendLine();
                }

                catch (Exception diagEx)
                {
                    Logger.LogMessage(diagEx.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                    sbErr.AppendLine("Cannot determine rest of parameters being passed due to the exception: ");
                    sbErr.Append(diagEx.Message);
                }
                TRSManagers.MailManager.SendEmail("MAIL1.AEGONUSA.COM", "bfl@transamerica.com", "TRSWebDevelopment@AEGONUSA.com", "UpdateTransactionStatus Error", sbErr.ToString());
                retVal = 0;
            }
            return retVal;
        }
        public static bool IsFinancialTransacion(int transTypeID)
        {
            if ((GetTransactionType(transTypeID) ?? "") == C_FINANCIAL)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string GetTransactionType(int transTypeID)
        {
            return Convert.ToString(TRSSqlHelper.ExecuteScalar(General.ConnectionString, "pSI_GetTransactionType", [transTypeID]));
        }
        public static int GetTRSFundID(string partnerFundID, int partnerID, ref string FundName)
        {
            DataSet ds;
            var FundID = default(int);

            ds = TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_GetTRSFund", [partnerFundID, partnerID]);
            if (ds.Tables[0].Rows.Count > 0)
            {
                FundID = Convert.ToInt32(ds.Tables[0].Rows[0]["fund_id"]);
                FundName = Convert.ToString(ds.Tables[0].Rows[0]["fund_name"]);
            }
            return FundID;
            // Return SqlHelper.ExecuteScalar(General.ConnectionString, "pSI_GetTRSFundID", New Object() {partnerFundID, partnerID})
        }

    }
}