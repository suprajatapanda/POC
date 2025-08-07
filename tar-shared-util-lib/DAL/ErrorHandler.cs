using TRS.SqlHelper;

namespace TRS.IT.SI.BusinessFacadeLayer.DAL
{

    public class ErrorHandler
    {
        public static Model.ErrorInfo GetPartnerErrorInfo(Model.PartnerFlag partnerFlag, string partnerErrorCode)
        {
            Microsoft.Data.SqlClient.SqlDataReader dr;
            var oErrorInfo = new Model.ErrorInfo();

            dr = TRSSqlHelper.ExecuteReader(General.ConnectionString, "pSI_GetPartnerErrorInfo", [partnerFlag, partnerErrorCode]);
            if (dr.Read())
            {
                oErrorInfo.Number = Convert.ToInt32(dr["error_id"]);
                oErrorInfo.Description = Convert.ToString(dr["error_description"] + Environment.NewLine + "Error Code:" + partnerErrorCode);
            }
            else
            {
                oErrorInfo.Number = (int)Model.ErrorCodes.Unknown;
                oErrorInfo.Description = "Unknown Error!" + Environment.NewLine + "ErrorCode: " + partnerErrorCode;
            }
            dr.Close();
            return oErrorInfo;
        }
    }
}