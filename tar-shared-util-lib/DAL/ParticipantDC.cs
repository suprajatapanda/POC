using Microsoft.Data.SqlClient;
using MimeKit;
using SIUtil;
using System.Data;
using System.Text;
using System.Xml;
using TRS.IT.SI.BusinessFacadeLayer.Model;
using TRS.SqlHelper;

namespace TRS.IT.SI.BusinessFacadeLayer.DAL
{
    public class ParticipantDC
    {

        #region *** Private members ***
        private string _SessionID;
        private int _InLoginID;
        private string _ContractID, _SubID;
        private bool _SessionFlag = true;
        #endregion

        #region *** Constructor ***
        public ParticipantDC(string SessionID)
        {
            // Return a Participant (Partner specific object)
            _SessionFlag = true;
            _SessionID = SessionID;
        }

        public ParticipantDC(int InLoginID, string ContractID, string SubID)
        {
            _SessionFlag = false;
            _InLoginID = InLoginID;
            _ContractID = ContractID;
            _SubID = SubID;
        }
        #endregion

        #region *** GetPartnerID (Shared) ***
        public static PartnerFlag GetPartnerID(string sessionID)
        {
            string strPlan = string.Empty;
            string strMember = string.Empty;
            // iPartner As PartnerFlag

            return GetPartner(sessionID, ref strPlan, ref strMember);
        }

        #endregion

        #region *** GetPartner (Shared) ***
        public static PartnerFlag GetPartner(string sessionID, ref string strPlanID, ref string strMemberID)
        {
            // retrieve command parameters from database
            SqlParameter[] @params;
            string strPartner;
            var iPartner = default(PartnerFlag);
            @params = SqlHelperParameterCache.GetSpParameterSet(General.ConnectionString, "pSI_GetPartner");

            // assign sessionid to parameter
            @params[0].Value = sessionID;

            SqlDataReader dr = null;

            try
            {
                // retrieve partner name from database
                dr = TRSSqlHelper.ExecuteReader(General.ConnectionString, CommandType.StoredProcedure, "pSI_GetPartner", @params);
                // return partner name
                if (dr.Read())
                {
                    iPartner = (PartnerFlag)Convert.ToInt32(dr["partner_id"]);

                    if (!ReferenceEquals(dr["plan_id"], DBNull.Value))
                    {
                        strPlanID = Convert.ToString(dr["plan_id"]) + "";
                    }
                    strMemberID = Convert.ToString(dr["ex_user_id"]);
                    strPartner = Convert.ToString(dr["short_name"]);
                }

                else
                {
                    strPlanID = string.Empty;
                    strMemberID = string.Empty;
                    strPartner = string.Empty;
                }
            }
            catch
            {
                iPartner = default;
            }
            if (!dr.IsClosed)
            {
                dr.Close();
            }

            return iPartner;
        }

        public static PartnerFlag GetPartner(int InLoginID, string ContractID, string SubID, ref string strPlanID, ref string strMemberID)
        {
            // retrieve command parameters from database
            SqlParameter[] @params;
            string strPartner;
            var iPartner = default(PartnerFlag);
            @params = SqlHelperParameterCache.GetSpParameterSet(General.ConnectionString, "pSI_GetPartnerByInLoginID");

            // assign sessionid to parameter
            @params[0].Value = InLoginID;
            @params[1].Value = ContractID;
            @params[2].Value = SubID;

            SqlDataReader dr = null;

            try
            {
                // retrieve partner name from database
                dr = TRSSqlHelper.ExecuteReader(General.ConnectionString, CommandType.StoredProcedure, "pSI_GetPartnerByInLoginID", @params);
                // return partner name
                if (dr.Read())
                {
                    iPartner = (PartnerFlag)Convert.ToInt32(dr["partner_id"]);
                    strMemberID = Convert.ToString(dr["ex_user_id"]);
                    strPartner = Convert.ToString(dr["short_name"]);

                    if (!ReferenceEquals(dr["plan_id"], DBNull.Value))
                    {
                        strPlanID = Convert.ToString(dr["plan_id"]);
                    }
                }
                else
                {
                    strPlanID = string.Empty;
                    strMemberID = string.Empty;
                    strPartner = string.Empty;
                }
            }
            catch
            {
                iPartner = default;
            }
            if (!dr.IsClosed)
            {
                dr.Close();
            }
            return iPartner;
        }

        #endregion

        #region *** UpdateParticipantInfo ***
        public SIResponse UpdateParticipantInfo(ParticipantInfo oParticipant)
        {
            int iCacheFlag = (int)CacheFlag.All;
            string strParticipantInfoXML;
            var req = new SIResponse();
            string SpName = string.Empty;
            string sLine = string.Empty;


            if (string.IsNullOrEmpty(oParticipant.PersonalInfo.TerminationDt))
            {
                oParticipant.PersonalInfo.TerminationDt = null;
            }

            strParticipantInfoXML = TRSManagers.XMLManager.GetXML(oParticipant);
            // replace "true" with 1 and "false" with 0
            strParticipantInfoXML = strParticipantInfoXML.Replace(">false<", ">0<");
            strParticipantInfoXML = strParticipantInfoXML.Replace(">true<", ">1<");
            strParticipantInfoXML = strParticipantInfoXML.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");
            try
            {

                if (_SessionFlag == true)
                {
                    SpName = "pSI_InsertParticipantCache";
                    sLine = _SessionID;
                    TRSSqlHelper.ExecuteNonQuery(General.ConnectionString, "pSI_InsertParticipantCache", [_SessionID, strParticipantInfoXML, iCacheFlag]);
                }
                else
                {
                    sLine = _InLoginID.ToString() + "/" + _ContractID + "/" + _SubID;
                    SpName = "pSI_InsertParticipantCacheByInLoginID";
                    TRSSqlHelper.ExecuteNonQuery(General.ConnectionString, "pSI_InsertParticipantCacheByInLoginID", [_InLoginID, _ContractID, _SubID, strParticipantInfoXML, iCacheFlag]);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                req.Errors[0].Number = (int)ErrorCodes.Unknown;
                req.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error in " + SpName + " SP!: " + sLine, "ParticipantDC::UpdateParticipantInfo");
            }
            return req;
        }
        #endregion

        #region *** GetParticipantInfo ***
        public ParticipantInfo GetParticipantInfoFromDB()
        {
            ParticipantInfo p;
            var s = new System.Xml.Serialization.XmlSerializer(typeof(ParticipantInfo));
            XmlReader xmlr = null;
            var mCon = new SqlConnection(General.ConnectionString);
            try
            {
                mCon.Open();
                xmlr = TRSSqlHelper.ExecuteXmlReader(mCon, "pSI_GetParticipantCacheByInLoginID2", [_InLoginID, _ContractID, _SubID, _SessionID]);
                p = (ParticipantInfo)s.Deserialize(xmlr);

                if (!(_SessionID == null) && !string.IsNullOrEmpty(_SessionID))
                {
                    p.SessionID = _SessionID;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                p = new ParticipantInfo();
                p.Errors[0].Number = (int)ErrorCodes.NoParticipantCache;
                p.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error in GetParticipantInfo SP!", "ParticipantDC::GetParticipantInfo");
            }
            finally
            {
                if (xmlr != null)
                {
                    xmlr.Close();
                }

                mCon.Dispose();
            }

            // Return the object
            if (p.PersonalInfo != null)
            {
                if (p.PersonalInfo.State == "ZZ")
                {
                    p.PersonalInfo.USAddress = false;
                }
            }
            return p;
        }

        public void GetParticipantLocalData(ref ParticipantInfo oParticipantInfo, bool isISC = false)
        {
            try
            {
                var ds = new DataSet();
                DataRow dr;
                string sLine;

                ds = TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_GetParticipantLocalData", [oParticipantInfo.SessionID]);

                if (ds.Tables[0].Rows.Count > 0)
                {
                    dr = ds.Tables[0].Rows[0];

                    if (oParticipantInfo.TransPending == "0" | string.IsNullOrEmpty(oParticipantInfo.TransPending))
                    {
                        sLine = Convert.ToString(General.ValidateDBNull(dr["TRANS_PENDING"]));
                        if (sLine == "D")
                        {
                            oParticipantInfo.TransPending = Convert.ToString(General.ValidateDBNull(dr["TRANS_PENDING"]));
                            oParticipantInfo.TransPendingID = Convert.ToInt32(General.ValidateDBNull(dr["TransPendingID"]));
                            oParticipantInfo.TransPendingName = Convert.ToString(General.ValidateDBNull(dr["TransPendingName"]));
                        }
                    }
                    oParticipantInfo.LastStatementGeneratedDt = Convert.ToString(General.ValidateDBNull(dr["LastStatementGeneratedDt"]));
                    oParticipantInfo.LastStatementMailedDt = Convert.ToString(General.ValidateDBNull(dr["LastStatementMailedDt"]));
                    oParticipantInfo.PersonalInfo.EmployerName = Convert.ToString(General.ValidateDBNull(dr["EmployerName"]));
                    if (!isISC)
                    {
                        oParticipantInfo.PersonalInfo.Email = Convert.ToString(General.ValidateDBNull(dr["Email"]));
                    }
                    if (oParticipantInfo.PlanInfo is null)
                    {
                        oParticipantInfo.PlanInfo = new PlanInfo();
                    }
                    oParticipantInfo.PlanInfo.OnlineEnrollment = Convert.ToBoolean(General.ValidateDBNull(dr["OnlineEnrollment"]));
                    oParticipantInfo.PlanInfo.AutoEnrollment = Convert.ToBoolean(General.ValidateDBNull(dr["AutoEnrollment"]));
                    oParticipantInfo.PlanInfo.CatchupContributions = Convert.ToBoolean(General.ValidateDBNull(dr["CatchupContributions"]));
                    oParticipantInfo.PlanInfo.OleEndDate = Convert.ToString(General.ValidateDBNull(dr["OleEndDate"]));
                    oParticipantInfo.PlanInfo.DefaultContrRate = Convert.ToDouble(General.ValidateDBNull(dr["DefaultContrRate"]));
                    oParticipantInfo.PlanInfo.PlanType = Convert.ToString(General.ValidateDBNull(dr["PlanType"]));
                    oParticipantInfo.PlanInfo.PlanName = Convert.ToString(General.ValidateDBNull(dr["PlanTypeName"]));
                    oParticipantInfo.PlanInfo.PlanTypeName = Convert.ToString(General.ValidateDBNull(dr["PlanTypeName"]));
                    oParticipantInfo.PlanInfo.HardshipForElective = Convert.ToBoolean(General.ValidateDBNull(dr["HardshipForElective"]));
                    oParticipantInfo.PlanInfo.ServiceType = Convert.ToString(General.ValidateDBNull(dr["ServiceType"]));
                    oParticipantInfo.PlanInfo.REA_Exempt = Convert.ToBoolean(General.ValidateDBNull(dr["REA_Exempt"]));
                    oParticipantInfo.PlanInfo.AdviceSolution = Convert.ToBoolean(General.ValidateDBNull(dr["AdviceSolution"]));
                    oParticipantInfo.PlanInfo.HardshipWithdrawals = Convert.ToBoolean(General.ValidateDBNull(dr["HardshipWithdrawals"]));
                    oParticipantInfo.PlanInfo.Age59Withdrawals = Convert.ToBoolean(General.ValidateDBNull(dr["Age59Withdrawals"]));
                    oParticipantInfo.PlanInfo.TACode = Convert.ToString(General.ValidateDBNull(dr["TACode"]));
                    oParticipantInfo.PlanInfo.OnlineDistribution = Convert.ToBoolean(General.ValidateDBNull(dr["OnlineDistribution"]));
                    if (oParticipantInfo.PlanLoanInfo is null)
                    {
                        oParticipantInfo.PlanLoanInfo = new PlanLoanInfo();
                    }
                    oParticipantInfo.PlanLoanInfo.LoanSetupPaymt = Convert.ToString(General.ValidateDBNull(dr["LoanSetupPaymt"]));
                    oParticipantInfo.PlanLoanInfo.PaperlessLoans = Convert.ToBoolean(General.ValidateDBNull(dr["PaperlessLoans"]));
                    oParticipantInfo.PlanLoanInfo.IsLoanAllowed = Convert.ToBoolean(General.ValidateDBNull(dr["IsLoanAllowed"]));
                    oParticipantInfo.PlanLoanInfo.LoanMaintPaymt = Convert.ToString(General.ValidateDBNull(dr["LoanMaintPaymt"]));
                    oParticipantInfo.PlanLoanInfo.SuppressLoanAvail = Convert.ToBoolean(General.ValidateDBNull(dr["SuppressLoanAvail"]));
                    oParticipantInfo.PlanLoanInfo.HardshipLoans = Convert.ToBoolean(General.ValidateDBNull(dr["HardshipLoans"]));
                    if (oParticipantInfo.EnrollmentInfo is null)
                    {
                        oParticipantInfo.EnrollmentInfo = new EnrollmentInfo();
                    }
                    oParticipantInfo.EnrollmentInfo.PinChangeReq = Convert.ToBoolean(General.ValidateDBNull(dr["PinChangeReq"]));
                    oParticipantInfo.EnrollmentInfo.EnrollmentDt = Convert.ToString(General.ValidateDBNull(dr["EnrollmentDt"]));
                    oParticipantInfo.PlanInfo.SDBAPlan = Convert.ToBoolean(General.ValidateDBNull(dr["SDBAPlan"]));
                    oParticipantInfo.PendingTransactionCount = GetPendingTransactionCount(oParticipantInfo.SessionID);
                    oParticipantInfo.PendingDistributionCount = GetPptDistributionDetailsCount(_InLoginID);

                    PartnerFlag partnerID;
                    partnerID = GetPartnerID(_SessionID);
                    if (oParticipantInfo.PlanInfo.DefaultFund == null)
                    {
                        oParticipantInfo.PlanInfo.DefaultFund = new FundInfo();
                    }
                    if (oParticipantInfo.PlanInfo.DefaultFund.FundID == "0" | string.IsNullOrEmpty(oParticipantInfo.PlanInfo.DefaultFund.FundID))
                    {
                        oParticipantInfo.PlanInfo.DefaultFund.FundID = Convert.ToString(General.ValidateDBNull(dr["DefaultFundID"]));
                        oParticipantInfo.PlanInfo.DefaultFund.FundName = Convert.ToString(General.ValidateDBNull(dr["DefaultFundName"]));
                    }

                    oParticipantInfo.PlanInfo.Contribution_PCT_Allowed = Convert.ToBoolean(General.ValidateDBNull(dr["Contribution_PCT_Allowed"]));
                    oParticipantInfo.PlanInfo.Contribution_AMT_Allowed = Convert.ToBoolean(General.ValidateDBNull(dr["Contribution_AMT_Allowed"]));
                    oParticipantInfo.PlanLoanInfo.RefinanceAllowed = Convert.ToBoolean(General.ValidateDBNull(dr["RefinanceAllowed"]));
                    oParticipantInfo.PlanInfo.PlanName = Convert.ToString(General.ValidateDBNull(dr["PlanName"]));
                    oParticipantInfo.PlanInfo.Contributions_Allowed = Convert.ToBoolean(General.ValidateDBNull(dr["Contributions_Allowed"]));
                    oParticipantInfo.PlanInfo.ROTH_Allowed = Convert.ToBoolean(General.ValidateDBNull(dr["ROTH_Allowed"]));
                    oParticipantInfo.ShowTransHistory = Convert.ToBoolean(General.ValidateDBNull(dr["ShowTranHistory"]));
                    oParticipantInfo.PlanInfo.SuppressVesting = Convert.ToBoolean(General.ValidateDBNull(dr["SuppressVesting"]));

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oParticipantInfo.Errors[0].Number = (int)ErrorCodes.Unknown;
                oParticipantInfo.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error in GetParticipantLocalData SP!", "ParticipantDC::GetParticipantLocalData") + "   " + oParticipantInfo.Errors[0].Description;
            }
        }

        private int GetPendingTransactionCount(string sSessionID)
        {
            try
            {
                return GetPendingTransaction(sSessionID).Tables[0].Rows.Count;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                SendErrorMail(ex);
                return 0;
            }
        }

        public static DataSet GetPendingTransaction(string sSessionID)
        {
            return TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_GetPendingTransactions", [sSessionID]);
        }
        private int GetPptDistributionDetailsCount(int in_Login_ID)
        {
            try
            {
                return GetPptDistributionDetails(in_Login_ID).Tables[0].Rows.Count;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                SendErrorMail(ex);
                return 0;
            }
        }

        public static DataSet GetPptDistributionDetails(int in_Login_ID)
        {
            return TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_getPendingDistributionsByIn_LoginID", [in_Login_ID]);
        }
        #endregion

        #region SendErrorEmail
        public void SendErrorMail(Exception ex)
        {
            var oMail = new MimeMessage();
            oMail.From.Add(new MailboxAddress("", TrsAppSettings.AppSettings.GetValue("FromEmail")));

            foreach (string s in TrsAppSettings.AppSettings.GetValue("ToEmail").Split(';'))
            {
                if (!string.IsNullOrEmpty(s.Trim()))
                {
                    oMail.To.Add(new MailboxAddress("", s.Trim()));
                }
            }

            oMail.Subject = "Error in GetParticipantLocalData";

            var strStatus = new StringBuilder();
            strStatus.AppendLine("SessionID: " + _SessionID);
            strStatus.AppendLine("ex: " + ex.Message);

            string sInner = string.Empty;
            if (!(ex.InnerException == null))
            {
                strStatus.AppendLine("InnerException: " + ex.InnerException.Message + Environment.NewLine);
            }

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = strStatus.ToString();
            oMail.Body = bodyBuilder.ToMessageBody();

            TRSManagers.MailManager.SendEmail(oMail);
        }

        #endregion

        #region *** GetPlanInfo ***
        public static DataSet GetPlanInfo(string MemberID, string PlanID)
        {
            SqlConnection Cn = null;
            var ds = new DataSet();
            XmlReader xmlr = null;

            try
            {
                Cn = new SqlConnection(General.ConnectionString);

                Cn.Open();
                xmlr = TRSSqlHelper.ExecuteXmlReader(Cn, "pSI_GetPlanInfo", [MemberID, PlanID]);

                ds.ReadXml(xmlr);
            }
            catch
            {
                ds = null;
            }
            finally
            {
                Cn.Dispose();

                if (xmlr != null)
                {
                    xmlr.Close();
                }
            }

            return ds;
        }
        #endregion

        #region *** CreateSession ***
        public static string CreateSessionByInloginID(int InloginID, string ContractID, string SubID, int ClientTypeID, string HostID, string SessionID)
        {
            SqlParameter[] @params;
            string retValue;

            if (string.IsNullOrEmpty(SessionID))
            {
                SessionID = null;
            }

            @params = SqlHelperParameterCache.GetSpParameterSet(General.ConnectionString, "pSI_CreateParticipantSessionByInloginID");
            @params[0].Value = InloginID;
            @params[1].Value = ContractID;
            @params[2].Value = SubID;
            @params[3].Value = ClientTypeID;
            @params[4].Value = HostID;
            if (string.IsNullOrEmpty(SessionID))
            {
                @params[5].Value = DBNull.Value;
            }
            else
            {
                @params[5].Value = SessionID;
            }
            TRSSqlHelper.ExecuteNonQuery(General.ConnectionString, CommandType.StoredProcedure, "pSI_CreateParticipantSessionByInloginID", @params);
            retValue = Convert.ToString(@params[5].Value);
            return retValue;
        }
        #endregion

        #region *** Private Functions ***
        private void UpdateParticipantInfo(ParticipantInfo oParticipantInfo, int intCacheFlag)
        {
            string strParticipantInfoXML;
            var req = new SIResponse();

            strParticipantInfoXML = TRSManagers.XMLManager.GetXML(oParticipantInfo);
            strParticipantInfoXML = strParticipantInfoXML.Replace(">false<", ">0<");
            strParticipantInfoXML = strParticipantInfoXML.Replace(">true<", ">1<");
            strParticipantInfoXML = strParticipantInfoXML.Replace(" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"", "");

            try
            {
                TRSSqlHelper.ExecuteNonQuery(General.ConnectionString, "pSI_UpdateParticipantCache", [_SessionID, strParticipantInfoXML, intCacheFlag]);
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oParticipantInfo.Errors[0].Number = (int)ErrorCodes.Unknown;
                oParticipantInfo.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error in pSI_UpdateParticipantCache SP!", "ParticipantDC::UpdateParticipantInfo") + "   " + oParticipantInfo.Errors[0].Description;
            }
        }
        public bool ValidateCache(int intDataTypeBit)
        {
            // retrieve command parameters from database
            SqlParameter[] @params;
            @params = SqlHelperParameterCache.GetSpParameterSet(General.ConnectionString, "pSI_GetCacheInfo");
            // asssign session id to parameter
            @params[0].Value = _SessionID;

            // retrieve cache flag from database
            TRSSqlHelper.ExecuteNonQuery(General.ConnectionString, CommandType.StoredProcedure, "pSI_GetCacheInfo", @params);
            int intCacheFlag;
            bool blnIsCacheValid;

            intCacheFlag = Convert.ToInt32(@params[1].Value);
            blnIsCacheValid = Convert.ToBoolean(@params[2].Value);
            intCacheFlag = intCacheFlag & intDataTypeBit;
            @params = null;
            if (blnIsCacheValid == true & intCacheFlag != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public int UpdateTransactionStatus(TransactionType @type, TransactionStatus status, string transactionData, string ErrorCode = null, int partnerID = 0, string PartnerConfID = null, string sReviewSessionID = "")
        {
            // Dim params() As SqlParameter
            try
            {
                if (partnerID == 0)
                {
                    partnerID = (int)GetPartnerID(_SessionID);
                }
                if (string.IsNullOrEmpty(sReviewSessionID))
                {
                    return AudienceDC.UpdateTransactionStatus(_SessionID, (PartnerFlag)partnerID, type, status, transactionData, ErrorCode, PartnerConfID);
                }
                else
                {
                    return AudienceDC.UpdateTransactionStatus(sReviewSessionID, (PartnerFlag)partnerID, type, status, transactionData, ErrorCode, PartnerConfID);
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                return 0;
            }
        }
        public static int InsertConfirmation(int[] trans_ids)
        {
            var transXML = new System.Text.StringBuilder();

            transXML.Append("<trans_ids>");

            foreach (int x in trans_ids)
            {
                transXML.Append("<trans_id>");
                transXML.Append(x);
                transXML.Append("</trans_id>");
            }

            transXML.Append("</trans_ids>");

            try
            {
                return Convert.ToInt32(TRSSqlHelper.ExecuteScalar(General.ConnectionString, "pSI_InsertConfirmation", [transXML.ToString()]));
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
            }

            return default;
        }
        public bool SetCacheFlag(bool cacheFlag)
        {
            TRSSqlHelper.ExecuteScalar(General.ConnectionString, "pSI_SetCacheFlag", [_SessionID, cacheFlag]);
            return default;
        }

        #endregion

        public static string GetParticipantName(string inLoginId, string ContractID, string SubID)
        {
            SqlParameter[] @params;
            string sName = "";

            @params = SqlHelperParameterCache.GetSpParameterSet(General.ConnectionString, "pSI_GetEmailAddress_InLoginID");
            @params[0].Value = inLoginId;
            @params[1].Value = ContractID;
            @params[2].Value = SubID;

            TRSSqlHelper.ExecuteNonQuery(General.ConnectionString, CommandType.StoredProcedure, "pSI_GetEmailAddress_InLoginID", @params);
            if (!(@params[3] == null))
            {
                sName = Convert.ToString(@params[3].Value);
            }

            return sName;
        }

        #region *** GetEnrollmentDate ***
        public static string GetEnrollmentDate(string memberID, string contractID, string subID)
        {
            return Convert.ToString(TRSSqlHelper.ExecuteScalar(General.ConnectionString, "pSI_GetPptEnrollmentDate", [memberID, contractID, subID]));
        }
        #endregion

        #region *** UpdatePersonalProfile ***
        public void UpdatePersonalProfile(ref Profile oProfile)
        {
            var oParticipantInfo = new ParticipantInfo();
            oParticipantInfo = GetParticipantInfoFromDB();
            if (oParticipantInfo.Errors[0].Number == 0)
            {
                if (!oProfile.USAddress)
                {
                    oProfile.State = "ZZ";
                }

                oParticipantInfo.PersonalInfo = (PersonalProfile)oProfile;
                UpdateParticipantInfo(oParticipantInfo, (int)CacheFlag.Address);
            }
            oProfile.Errors = oParticipantInfo.Errors;
        }
        #endregion

        #region *** IsMEPContract ****
        public static bool IsMEPContract(string contractID, string subID)
        {
            SqlDataReader dr;
            bool bFlag;
            dr = TRSSqlHelper.ExecuteReader(General.ConnectionString, "pSI_IsMEPContract", [contractID, subID]);
            if (dr.Read())
            {
                bFlag = Convert.ToBoolean(dr[0]);
            }
            else
            {
                bFlag = false;
            }
            dr.Close();
            return bFlag;

        }
        #endregion
        public SIResponse UpdatePeriodicPendingInfo(string sTransPending)
        {

            var req = new SIResponse();

            try
            {
                TRSSqlHelper.ExecuteNonQuery(General.ConnectionString, "pSI_UpdatePeriodicPendingInfo", [_SessionID, sTransPending]);
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                req.Errors[0].Number = (int)ErrorCodes.Unknown;
                req.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error in pSI_UpdatePeriodicPendingInfo SP!", "ParticipantDC::UpdatePeriodicPendingInfo");
            }
            return req;
        }

    }
}