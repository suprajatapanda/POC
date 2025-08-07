using System.ServiceModel;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer.DAL;
using TRS.IT.SI.BusinessFacadeLayer.Model;

namespace TRS.IT.SI.BusinessFacadeLayer.Adapters
{
    public class SponsorAdapter_Penco : ISponsorAdapter
    {
        private WSPPT.PencoParticipantServiceClient _pencoPptService;
		private DEMOWSPPT.DemoParticipantServiceClient _demoPptService;
        private Services.wsReports.ReportsBAImplPortTypeClient _reportsClient;
        private string _partnerID;
        private bool _disposed = false;
        private PartnerFlag _currentPartner;

        public SponsorAdapter_Penco(PartnerFlag Partner)
        {
            _partnerID = ((int)Partner).ToString();
            InitializeServices(Partner);
        }

        private void InitializeServices(PartnerFlag Partner)
        {
            switch (Partner)
            {
                case PartnerFlag.Penco:
                    _pencoPptService = new WSPPT.PencoParticipantServiceClient(WSPPT.PencoParticipantServiceClient.EndpointConfiguration.BasicHttpBinding_IPencoParticipantService);
                    break;
                case PartnerFlag.TRS:
                    _demoPptService = new DEMOWSPPT.DemoParticipantServiceClient(DEMOWSPPT.DemoParticipantServiceClient.EndpointConfiguration.DemoParticipantServiceSoap);
                    break;
                case PartnerFlag.ISC:                    
                        InitializeReportsClient();
                        break;                    
                default:                    
                    _pencoPptService = new WSPPT.PencoParticipantServiceClient(WSPPT.PencoParticipantServiceClient.EndpointConfiguration.BasicHttpBinding_IPencoParticipantService);
                    break;
                    
            }
        }
        private void InitializeReportsClient()
        {
            string soapEndpoint = TrsAppSettings.AppSettings.GetValue("ReportsSrvWebServiceURL");
            var endpointAddress = new EndpointAddress(soapEndpoint);
            var basicHttpBinding = new BasicHttpBinding(
                endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
                    ? BasicHttpSecurityMode.Transport
                    : BasicHttpSecurityMode.None);
            if (endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }
            _reportsClient = new Services.wsReports.ReportsBAImplPortTypeClient(basicHttpBinding, endpointAddress);
            _reportsClient.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }

        #region *** GetReport ***
        public ReportResponse GetReport(string sessionID, ReportInfo oReportInfo, ref bool bAvail, PartnerFlag Partner)
        {
            SessionInfo oSessionInfo;
            ReportResponse sResponse;
            string sRequest, Response;

            try
            {
                oSessionInfo = AudienceDC.GetSessionInfo(sessionID);
                switch (oReportInfo.ReportType)
                {
                    case (int)ReportInfo.ReportTypeEnum.AccountStatement:
                        switch (_currentPartner)
                        {
                            case PartnerFlag.TRS:
                                Response = _demoPptService.StatementRequest(oReportInfo.MemberID, oReportInfo.StartDate,oReportInfo.EndDate, "true", StatementInfo.StatementTypeEnum.Date_Range.ToString());
                                break;
                            case PartnerFlag.ISC:                                
                                //PROBLEM, NO DIAWSPPT service for StatementRequest
                                //Response = _diaPptService.StatementRequest(oReportInfo.MemberID, oReportInfo.StartDate,
                                // oReportInfo.EndDate, "true", StatementInfo.StatementTypeEnum.Date_Range);
                                throw new NotImplementedException("DIAWSPPT service does not support StatementRequest.");
                            default:
                                Response = _pencoPptService.StatementRequest(oReportInfo.MemberID, oReportInfo.StartDate,oReportInfo.EndDate, "true", StatementInfo.StatementTypeEnum.Date_Range.ToString());
                                break;
                        }

                        sResponse = (ReportResponse)TRSManagers.XMLManager.DeserializeXml(Response, typeof(ReportResponse));
                        bAvail = true;
                        break;

                    default:
                        WSReport.PencoReportsServiceClient _pencoReportService = null;
                        DEMOWSReport.DemoReportsServiceClient _demoReportService = null;

                        if (Partner == PartnerFlag.TRS)
                        {
                            _demoReportService = new DEMOWSReport.DemoReportsServiceClient(DEMOWSReport.DemoReportsServiceClient.EndpointConfiguration.DemoReportsServiceSoap);
                        }
                        else
                        {
                            _pencoReportService = new WSReport.PencoReportsServiceClient(WSReport.PencoReportsServiceClient.EndpointConfiguration.BasicHttpBinding_IPencoReportsService);
                        }
                        if (Partner == PartnerFlag.ISC)
                        {
                            oReportInfo.LocationCode = oSessionInfo.LocationCode;
                            if (oReportInfo.StartDate == null)
                            {
                                oReportInfo.StartDate = DateTime.Now.ToShortDateString();
                            }
                            switch (oReportInfo.ReportType)
                            {
                                case (int)ReportInfo.ReportTypeEnum.CensusFile:
                                case (int)ReportInfo.ReportTypeEnum.DiscriminationDataDownload:
                                        
                                        if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.CensusFile)
                                        {
                                            oReportInfo.FullFile = true;
                                        }

                                        if (oReportInfo.SubID == "000" && !ParticipantDC.IsMEPContract(oReportInfo.ContractID, oReportInfo.SubID))
                                        {
                                            oReportInfo.ReportType = (int)ReportInfo.ReportTypeEnum.CensusFileMEP;
                                        }
                                        else if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.DiscriminationDataDownload)
                                        {
                                            oReportInfo.ReportType = (int)ReportInfo.ReportTypeEnum.CensusFile;
                                        }

                                        break;
                                        
                                case (int)ReportInfo.ReportTypeEnum.ForfeitureReport:
                                case (int)ReportInfo.ReportTypeEnum.PlanAdminstration:                                        
                                        oReportInfo.ReportDisplayType = ReportInfo.ReportDisplayTypeEnum.PDF;
                                        break;
                                        
                                case (int)ReportInfo.ReportTypeEnum.ContributionRateChange_2:                                        
                                        if (oReportInfo.FullFile)
                                        {
                                            oReportInfo.ReportType = (int)ReportInfo.ReportTypeEnum.OldRateChangeReport;
                                        }
                                        break;
                            }
                                
                            if (oReportInfo.EndDate == null)
                            {
                                oReportInfo.EndDate = oReportInfo.StartDate;
                            }
                        }

                        string sTSubId = oReportInfo.SubID;
                        string subId;
                        oReportInfo.SubID = General.SubOut(oReportInfo.SubID);
                        subId = oReportInfo.SubID;
                        if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.TPAFee)
                        {
                            string sContractHold = oReportInfo.ContractID;
                            string[] sHold = oReportInfo.FTPLocation.Split('~');
                            oReportInfo.FTPLocation = sHold[0];
                            oReportInfo.ContractID = sHold[1];
                            sRequest = TRSManagers.XMLManager.GetXML(oReportInfo);
                            oReportInfo.ContractID = sContractHold;
                        }
                        else
                        {
                            sRequest = TRSManagers.XMLManager.GetXML(oReportInfo);
                        }

                        oReportInfo.SubID = sTSubId;

                        sRequest = sRequest.Replace(">false<", ">0<");
                        sRequest = sRequest.Replace(">FALSE<", ">0<");
                        sRequest = sRequest.Replace(">true<", ">1<");
                        sRequest = sRequest.Replace(">TRUE<", ">1<");
                        switch (Partner)
                        {
                            case PartnerFlag.ISC:
                                    
                                if (oReportInfo.ReportType == 85)
                                {
                                    string reverseFeedFileFormat;
                                    var _SponsorDC = new SponsorDC(oReportInfo.ContractID, oReportInfo.SubID);
                                    reverseFeedFileFormat = _SponsorDC.GetReverseFeedFileFormatType();
                                    if (reverseFeedFileFormat is null)
                                    {
                                        reverseFeedFileFormat = "Standard";
                                    }
                                    sRequest = sRequest.Replace("</ReportInfo>", "<ReverseFeedFileFormat>" + reverseFeedFileFormat + "</ReverseFeedFileFormat></ReportInfo>");
                                }
		                        Response = _reportsClient.getPlanReportAsync(sRequest);
                                sResponse = (ReportResponse)TRSManagers.XMLManager.DeserializeXml(Response, typeof(ReportResponse));
                                break;
                                    
                            case PartnerFlag.TRS:
		                        Response = _demoReportService.ReportRequest(oSessionInfo.ContractID, oSessionInfo.SubID, sRequest);
		                        break;
                                    

                            default:
                                    
                                if (SponsorDC.GetPartnerID(sessionID) != PartnerFlag.ISC)
                                {
                                    subId = "000";
                                }
                                Response = _pencoReportService.ReportRequest(oReportInfo.ContractID, subId, sRequest);
                                break;
                                    
                        }
                        sResponse = (ReportResponse)TRSManagers.XMLManager.DeserializeXml(Response, typeof(ReportResponse));
                        if (oReportInfo.ReportType == (int)ReportInfo.ReportTypeEnum.P360Report)
                        {
                            sResponse.IsPending = true;
                        }

                        sResponse.Request = sRequest;

                        bAvail = true;
                        break;
                        
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                bAvail = false;
                sResponse = new ReportResponse();
                sResponse.Errors[0].Number = 100;
                sResponse.Errors[0].Description = "Partner Unavaliable" + ex.Message;
            }
            finally
            {
                CleanupServices();
            }

            return sResponse;
        }
        #endregion

        private void CleanupServices()
        {
            try
            {
                if (_pencoPptService != null)
                {
                    if (_pencoPptService.State == CommunicationState.Faulted)
                    {
                        _pencoPptService.Abort();
                    }
                    else if (_pencoPptService.State != CommunicationState.Closed)
                    {
                        _pencoPptService.Close();
                    }
                }

                if (_demoPptService != null)
                {
                    if (_demoPptService.State == CommunicationState.Faulted)
                    {
                        _demoPptService.Abort();
                    }
                    else if (_demoPptService.State != CommunicationState.Closed)
                    {
                        _demoPptService.Close();
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}