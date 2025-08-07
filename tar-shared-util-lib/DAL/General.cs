using System.Data;
using SIUtil;
using TRS.SqlHelper;

namespace TRS.IT.SI.BusinessFacadeLayer.DAL
{
    public class General
    {
        private static string m_sConnectString;
        public const string C_Conf_Seprator = "||";
        public static string FormatErrorMsg(string message, string customMessage, string classFunctionName)
        {
            return message + Environment.NewLine + customMessage + Environment.NewLine + classFunctionName;
        }
        public static string ConnectionString
        {
            get
            {
                m_sConnectString = TrsAppSettings.AppSettings.GetConnectionString("ConnectString");
                if (string.IsNullOrEmpty(m_sConnectString) || m_sConnectString.Length == 0)
                {
                    throw new ArgumentNullException("ConnectString", "Connection String not found.");
                }
                return m_sConnectString;
            }
        }
        public static string FormatNumberFlat(double Number, int intLen, int floatLen)
        {
            int strIntPart;
            var strFloatPart = default(int);
            int pointPosition;


            pointPosition = Number.ToString().IndexOf(".");
            if (pointPosition != -1)
            {
                strFloatPart = Convert.ToInt32(Number.ToString().Substring(pointPosition + 1));
                strIntPart = Convert.ToInt32(Number.ToString().Substring(0, pointPosition + 1));
            }
            else
            {
                strIntPart = (int)Math.Round(Number);
            }
            if (strIntPart.ToString().Length > intLen || strFloatPart.ToString().Length > floatLen)
            {
                throw new Exception("Number " + Number.ToString() + " Out Of Range 9(" + intLen.ToString() + ")." + new string('9', floatLen));
            }
            else
            {
                return strIntPart.ToString(new string('0', intLen)) + strFloatPart.ToString(new string('0', floatLen));
            }

        }
        public static string SubIn(string a_sSubID)
        {
            if (string.IsNullOrEmpty(a_sSubID))
            {
                return "";
            }
            return Convert.ToString(TRSSqlHelper.ExecuteScalar(ConnectionString, "pSI_GetSubIn", [a_sSubID]));
        }
        public static string SubOut(string a_sSubID)
        {
            if (string.IsNullOrEmpty(a_sSubID))
            {
                return "";
            }
            return Convert.ToString(TRSSqlHelper.ExecuteScalar(ConnectionString, "pSI_GetSubOut", [a_sSubID]));
        }

        public static string GetConfirmationNumber(string JoinedConfID)
        {
            // returns the confirmation number
            if (JoinedConfID == null)
            {
                return null;
            }
            else if (JoinedConfID.IndexOf(C_Conf_Seprator) == -1)
            {
                return JoinedConfID;
            }
            else
            {
                return JoinedConfID.Substring(0, JoinedConfID.IndexOf(C_Conf_Seprator));
            }
        }
        public static string JoinConfirmationNumber(string partnerConfID, int transID)
        {
            if (string.IsNullOrEmpty(partnerConfID))
            {
                return transID.ToString();
            }
            else
            {
                return partnerConfID + C_Conf_Seprator + transID.ToString();
            }
        }
        public static object ValidateDBNull(object o)
        {
            if (ReferenceEquals(DBNull.Value, o))
            {
                return null;
            }
            else
            {
                return Convert.ToString(o);
            }
        }
        public static void LogErrors(string PageName, string LoginID, string HostName, string ErrorMsg, string sessionXML)
        {
            TRSSqlHelper.ExecuteReader(ConnectionString, "pSI_InsertErrorLog", [PageName, LoginID, HostName, ErrorMsg, sessionXML]);
        }
        public static DataTable AddBusinessDays(DateTime dtStart, int iWorkingDays, bool bIgnoreHolidays = false)
        {
            return TRSSqlHelper.ExecuteDataset(ConnectionString, "pUtil_AddBusinessDays", [dtStart, iWorkingDays, bIgnoreHolidays]).Tables[0];
        }
    }
}