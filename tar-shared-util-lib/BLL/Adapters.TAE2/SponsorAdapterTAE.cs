using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer.DAL;
using TRS.IT.SI.BusinessFacadeLayer.Model;
using TRS.IT.TAEMQCon;

namespace TRS.IT.SI.BusinessFacadeLayer.Adapters
{

    public class SponsorAdapter : ISponsorAdapter
    {

        #region  Friend Constants 
        // These conatants are only used in TAE_Adapter assembly
        internal const string C_ParticipantMessage = "6000";
        internal const string C_6007Message = "6007";
        internal const string C_Suffix_OnlineDistribution = "006";
        internal const int C_Add_Participant = 35;

        #endregion

        #region  Private Constants 
        private const int C_REPORT_REQUEST = 37;
        #endregion

        #region *** GetReport ***
        public ReportResponse GetReport(string sessionID, ReportInfo oReportInfo, ref bool bAvail, PartnerFlag Partner)
        {
            var oResponse = new ReportResponse();
            var oSIResponse = new SIResponse();
            var oMQConn = new MQConnection();
            SessionInfo oSessionInfo;
            string mqRequest = "";
            string mqResponse;
            try
            {
                bAvail = false;
                oSessionInfo = AudienceDC.GetSessionInfo(sessionID);
                oReportInfo.LocationCode = oSessionInfo.LocationCode;
                oReportInfo.PartnerUserID = "INTERNE";
                if (!string.IsNullOrEmpty(oReportInfo.PartnerUserID))
                {
                    mqRequest = ReportConverter.FormatReportRequest(oReportInfo, ref oResponse);
                    mqResponse = oMQConn.SubmitTransaction(mqRequest, C_REPORT_REQUEST);
                    oSIResponse = ReportConverter.IsValidReportResponse(mqResponse);
                    oResponse.Errors[0] = oSIResponse.Errors[0];
                }
                else
                {
                    oResponse.Errors[0].Number = (int)ErrorCodes.InvalidLogin;
                    oResponse.Errors[0].Description = "User ID cannot be null or blank";
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oResponse.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                oResponse.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error", "GetReport");
            }
            return oResponse;
        }
        #endregion
    }
}