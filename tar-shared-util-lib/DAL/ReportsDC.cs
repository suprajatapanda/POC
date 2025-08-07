using SIUtil;
using TRS.SqlHelper;

namespace TRS.IT.SI.BusinessFacadeLayer.DAL
{

    public class ReportsDC
    {
        public static int InsertReport(int InLoginID, string ContractID, string SubID, Model.ReportInfo.ReportTypeEnum ReportType, Model.ReportInfo.ReportStatusEnum ReportStatus, string FileName, string reportRequest, Model.PartnerFlag sPartnerID, string sUserID = "", string sApplicationName = "", string sCustomReportName = "")
        {
            try
            {
                return Convert.ToInt32(TRSSqlHelper.ExecuteScalar(General.ConnectionString, "pSI_InsertReportRequestByInLoginID", [InLoginID, ContractID, SubID, (int)ReportType, (int)ReportStatus, FileName, reportRequest, false, sPartnerID, sUserID, sApplicationName, sCustomReportName]));
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                return 0;
            }
        }

        public static int GetNextReportNumber()
        {
            return Convert.ToInt32(TRSSqlHelper.ExecuteScalar(General.ConnectionString, "pSI_GetNextReportNumber"));
        }
    }
}