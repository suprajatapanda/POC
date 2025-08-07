using TRS.IT.SI.BusinessFacadeLayer.Adapters;
using TRS.IT.SI.BusinessFacadeLayer.DAL;
using TRS.IT.SI.BusinessFacadeLayer.Model;

namespace TRS.IT.SI.BusinessFacadeLayer
{

    public class Sponsor
    {
        #region *** Private Members ***
        private ISponsorAdapter _SponsorAdapter;
        private string _SessionID;
        #endregion

        #region *** Constructor ***
        public Sponsor(string SessionID)
        {
            _SessionID = SessionID;
            switch (SponsorDC.GetPartnerID(SessionID))
            {
                case PartnerFlag.TAE:
                    {
                        _SponsorAdapter = new SponsorAdapter();
                        break;
                    }
                case PartnerFlag.Penco:
                    {
                        _SponsorAdapter = new SponsorAdapter_Penco(PartnerFlag.Penco);
                        break;
                    }
                case PartnerFlag.ISC:
                    {
                        _SponsorAdapter = new SponsorAdapter_Penco(PartnerFlag.ISC);
                        break;
                    }

                default:
                    {
                        _SponsorAdapter = new SponsorAdapter_Penco(PartnerFlag.TRS);
                        break;
                    }
            }

            var oSessionInfo = AudienceDC.GetSessionInfo(SessionID);
        }
        #endregion
        public static new string CreateSession(int InLoginID, string ContractID, string SubID, string sessionID = null)
        {
            return SponsorDC.CreateSession(InLoginID, ContractID, SubID, sessionID);
        }
        public ReportResponse GetReport(ReportInfo oReportInfo)
        {
            var oResponse = new ReportResponse();
            SessionInfo oSessionInfo;
            var bAvail = default(bool);
            string sFileName;
            PartnerFlag sPartnerID;
            string sRequest = "";

            oSessionInfo = AudienceDC.GetSessionInfo(_SessionID);
            sPartnerID = SponsorDC.GetPartnerID(_SessionID);
            sPartnerID = (PartnerFlag)Convert.ToInt32(sPartnerID == 0 ? (PartnerFlag)Enum.Parse(typeof(PartnerFlag), oReportInfo.PartnerID, true) : sPartnerID);
            if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.RequestATest)
            {
                oResponse.FileName = string.Format("{0}{1}_RequestTest_{2}.pdf", oReportInfo.ContractID, General.SubOut(oReportInfo.SubID), DateTime.Now.ToString("yyyyMMdd"));
                oResponse.Errors[0].Number = 0;
                oResponse.IsPending = true;
                oResponse.Request = TRSManagers.XMLManager.GetXML(oReportInfo);
                sPartnerID = PartnerFlag.Penco;
            }
            else
            {
                switch (sPartnerID)
                {
                    case PartnerFlag.ISC:
                        {
                            oResponse = _SponsorAdapter.GetReport(_SessionID, oReportInfo, ref bAvail, PartnerFlag.ISC);
                            break;
                        }
                    case PartnerFlag.Penco:
                        {
                            oResponse = _SponsorAdapter.GetReport(_SessionID, oReportInfo, ref bAvail, PartnerFlag.Penco);
                            break;
                        }
                    case PartnerFlag.DIA:
                        {
                            oResponse = _SponsorAdapter.GetReport(_SessionID, oReportInfo, ref bAvail, PartnerFlag.DIA);
                            break;
                        }
                }
            }

            sRequest = TRSManagers.XMLManager.GetXML(oReportInfo);

            if (oResponse.Errors[0].Number == 0)
            {
                if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.PlanDataCsvFile && oReportInfo.ReportDisplayType == ReportInfo.ReportDisplayTypeEnum.XLS)
                {
                    oReportInfo.ReportType = (int)ReportInfo.ReportTypeEnum.PlanDataXlsFile;
                    if (sPartnerID == PartnerFlag.Penco)
                    {
                        oResponse.IsPending = true;
                    }
                }
                sFileName = BusinessFacadeLayer.Util.GetReportPath((ReportInfo.ReportTypeEnum)oReportInfo.ReportType, sPartnerID) + oResponse.FileName;

                if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.RequestATest)
                {
                    string[] fn = oResponse.FileName.Split('_');
                    var contractSubId = fn[0].Substring(6, 1).Equals("0") ? fn[0].Remove(6, 1) : fn[0];
                    string newFileName = string.Format("{0}_{1}_{2}", contractSubId, fn[1], fn[2]);
                    string filePath = string.Format(@"YEFORMS.{0}\ASC Testing Results\{1}\{2}", DateTime.Today.ToString("yy"), contractSubId, newFileName);
                    oResponse.FileName = filePath;
                    sFileName = filePath;
                }

                if (sPartnerID == PartnerFlag.Penco)
                {

                    if (oResponse.IsPending)
                    {
                        ReportsDC.InsertReport(oSessionInfo.InLoginID, oReportInfo.ContractID, oReportInfo.SubID, (ReportInfo.ReportTypeEnum)oReportInfo.ReportType, ReportInfo.ReportStatusEnum.Pending, sFileName, oResponse.Request, sPartnerID, oReportInfo.UserID, oReportInfo.ApplicationName, oReportInfo.CustomReportName);
                    }
                    else
                    {
                        ReportsDC.InsertReport(oSessionInfo.InLoginID, oReportInfo.ContractID, oReportInfo.SubID, (ReportInfo.ReportTypeEnum)oReportInfo.ReportType, ReportInfo.ReportStatusEnum.Available, sFileName, oResponse.Request, sPartnerID, oReportInfo.UserID, oReportInfo.ApplicationName, oReportInfo.CustomReportName);
                    }
                }
                else
                {
                    ReportsDC.InsertReport(oSessionInfo.InLoginID, oReportInfo.ContractID, oReportInfo.SubID, (ReportInfo.ReportTypeEnum)oReportInfo.ReportType, ReportInfo.ReportStatusEnum.Pending, sFileName, oResponse.Request, sPartnerID, oReportInfo.UserID, oReportInfo.ApplicationName, oReportInfo.CustomReportName);
                }
                AudienceDC.UpdateTransactionStatus(_SessionID, sPartnerID, TransactionType.ReportRequest, TransactionStatus.Success, sRequest, oResponse.Errors[0].Description, oResponse.ConfirmationNumber);
            }
            else
            {
                oResponse.FileName = "";
                ReportsDC.InsertReport(oSessionInfo.InLoginID, oReportInfo.ContractID, oReportInfo.SubID, (ReportInfo.ReportTypeEnum)oReportInfo.ReportType, ReportInfo.ReportStatusEnum.Failed, oResponse.FileName, oResponse.Request, sPartnerID, oReportInfo.UserID, oReportInfo.ApplicationName, oReportInfo.CustomReportName);
                AudienceDC.UpdateTransactionStatus(_SessionID, sPartnerID, TransactionType.ReportRequest, TransactionStatus.Failed, sRequest, oResponse.Errors[0].Description);
            }
            return oResponse;

        }
    }
}