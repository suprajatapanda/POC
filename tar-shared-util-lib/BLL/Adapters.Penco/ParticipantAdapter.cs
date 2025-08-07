using System.ServiceModel;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer.DAL;
using TRS.IT.SI.BusinessFacadeLayer.Model;

namespace TRS.IT.SI.BusinessFacadeLayer.Adapters
{
    public class ParticipantAdapter_Penco : IParticipantAdapter
    {
        public PersonalProfile GetPersonalProfile(string sessionID)
        {
            ParticipantInfo oParticipantInfo;
            SessionInfo oSessionInfo;
            oSessionInfo = AudienceDC.GetSessionInfo(sessionID);
            oParticipantInfo = GetParticipantInfoByMemberID(oSessionInfo.PartnerUserID, oSessionInfo.ContractID, oSessionInfo.SubID, false, sessionID);
            return oParticipantInfo.PersonalInfo;
        }

        public PersonalProfile GetPersonalProfile(int InLoginID, string ContractID, string SubID)
        {
            ParticipantInfo oParticipantInfo;
            string memberID = string.Empty;
            string planID = string.Empty;
            ParticipantDC.GetPartner(InLoginID, ContractID, SubID, ref planID, ref memberID);
            oParticipantInfo = GetParticipantInfoByMemberID(memberID, ContractID, SubID, false);
            return oParticipantInfo.PersonalInfo;
        }

        private string _partnerID;
        private string _partnerPlanID = "";
        private const int C_GET_PARTICIPANT_INFO = 49;
        private DEMOWSPPT.DemoParticipantServiceClient _demoPartnerService;
        private TRS.IT.SI.Services.ParticipantService _participantClient;
        private PencoPptSvc.PencoParticipantServiceClient _pptSvc = null;
        private Services.wsWithdrawals.WithdrawalsBAImplPortTypeClient _ISCWithdrawalSvc = null;
        public ParticipantAdapter_Penco(PartnerFlag Partner)
        {
            _partnerID = ((int)Partner).ToString();
            InitializeServices();
        }

        public ParticipantAdapter_Penco(PartnerFlag Partner, string PartnerPlanID)
        {
            _partnerID = ((int)Partner).ToString();
            _partnerPlanID = PartnerPlanID;
            InitializeServices();
        }
        private void InitializeServices()
        {
            switch (_partnerID ?? "")
            {
                case "4":
                    _demoPartnerService = new DEMOWSPPT.DemoParticipantServiceClient(DEMOWSPPT.DemoParticipantServiceClient.EndpointConfiguration.DemoParticipantServiceSoap);
                    break;
                case "2":
                default:
                    {
                        _partnerID = ((int)PartnerFlag.ISC).ToString();
                        _participantClient = new TRS.IT.SI.Services.ParticipantService(TrsAppSettings.AppSettings.GetValue("ISCParticipantWebServiceURL"));
                        InitializeWithdrawalsClient();
                        break;
                    }
            }
        }

        private void InitializeWithdrawalsClient()
        {
            string soapEndpoint = TrsAppSettings.AppSettings.GetValue("WithdrawalsSrvWebServiceURL");
            var endpointAddress = new EndpointAddress(soapEndpoint);
            var basicHttpBinding = new BasicHttpBinding(
                endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
                    ? BasicHttpSecurityMode.Transport
                    : BasicHttpSecurityMode.None);
            if (endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }
            basicHttpBinding.MaxReceivedMessageSize = 10 * 1024 * 1024;
            basicHttpBinding.MaxBufferSize = 10 * 1024 * 1024;
            basicHttpBinding.MaxBufferPoolSize = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxDepth = 128;
            basicHttpBinding.ReaderQuotas.MaxStringContentLength = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxArrayLength = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxBytesPerRead = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxNameTableCharCount = 10 * 1024 * 1024;

            _ISCWithdrawalSvc = new Services.wsWithdrawals.WithdrawalsBAImplPortTypeClient(basicHttpBinding, endpointAddress);
            _ISCWithdrawalSvc.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public ParticipantInfo GetParticipantInfo(int InLoginID, string ContractID, string SubID)
        {
            string memberID = string.Empty;
            string planID = string.Empty;

            // Get the planID and memberID
            ParticipantDC.GetPartner(InLoginID, ContractID, SubID, ref planID, ref memberID);

            // Get the ParticipantInfo
            return GetParticipantInfoByMemberID(memberID, ContractID, SubID);
        }
        public ParticipantInfo GetParticipantInfo(string sessionID)
        {
            ParticipantInfo oParticipantInfo;
            SessionInfo oSessionInfo;
            oSessionInfo = AudienceDC.GetSessionInfo(sessionID);
            oParticipantInfo = GetParticipantInfoByMemberID(oSessionInfo.PartnerUserID, oSessionInfo.ContractID, oSessionInfo.SubID, true, sessionID);

            return oParticipantInfo;
        }
        public ParticipantInfo GetParticipantInfoByMemberID(string memberID, string ContractID = "", string SubID = "", bool FullPptInfo = true, string sessionID = "")
        {
            string response;
            ParticipantInfo oParticipantInfo;
            string sError = "In GetParticipantInfoByMemberID";
            string responseLog = string.Empty;

            try
            {
                if (Convert.ToDouble(_partnerID) == (double)PartnerFlag.ISC)
                {
                    var pInfo = new Services.DIAWSPPT.getParticipantInfo();
                    var p3Respons = new Services.DIAWSPPT.getParticipantInfoResponse();
                    pInfo.SSN = TRSManagers.FormatManager.FormatSSN(memberID);
                    if (string.IsNullOrEmpty(_partnerPlanID))
                    {
                        pInfo.Account_Number = (ContractID + new string(' ', 10)).Substring(0, 10) + (new string('0', 5) + General.SubOut(SubID)).Substring((new string('0', 5) + General.SubOut(SubID)).Length - 5);    // "990739    00000"
                    }
                    else
                    {
                        pInfo.Account_Number = _partnerPlanID;
                    }
                    pInfo.Get_Full_ParticipantInfo = FullPptInfo;
                    sError += Environment.NewLine + "Going to call P3 webservice for GetPptInfo with : SSN = " + pInfo.SSN + ", Account Number: " + pInfo.Account_Number;
                    p3Respons = _participantClient.getParticipantInfo(pInfo);
                    // Do mappings here for ISC
                    response = DoP3ResponseMapping(p3Respons);
                    responseLog = memberID + ": " + response;
                }
                else
                {
                    if (UseWcfService() == false)
                    {
                        response = _demoPartnerService.GetParticipantInfo(memberID);
                        responseLog = memberID;
                    }
                    else
                    {
                        try
                        {
                            LoadBalance();
                            responseLog = TRSWEBGlobal.LastURL + ": " + memberID;
                            response = _pptSvc.GetParticipantInfo(memberID);

                            // If the control came here then reset the unavailable count
                            ResetURLsUnavailableCount();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                            MarkCSCURLUnavailable();
                            oParticipantInfo = new ParticipantInfo();
                            oParticipantInfo.Errors[0].Number = (int)ErrorCodes.TimeoutError;
                            oParticipantInfo.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error", "ParticipantAdapter::GetParticipantInfo from wcf service: " + TRSWEBGlobal.LastURL);

                            AudienceDC.UpdateTransactionStatus(sessionID, (PartnerFlag)Convert.ToInt32(_partnerID), (TransactionType)C_GET_PARTICIPANT_INFO, TransactionStatus.Failed, responseLog, oParticipantInfo.Errors[0].Number.ToString(), null);
                            return oParticipantInfo;
                        }
                        finally
                        {
                            ClosePPTSvc();
                        }
                    }
                    response = response.Replace("<PayrollFrequency>U</PayrollFrequency>", "<PayrollFrequency>0</PayrollFrequency>").Trim();
                    response = response.Replace("<PayrollFrequency>N</PayrollFrequency>", "<PayrollFrequency>1</PayrollFrequency>").Trim();
                    response = response.Replace("<PayrollFrequency>A</PayrollFrequency>", "<PayrollFrequency>2</PayrollFrequency>").Trim();
                    response = response.Replace("<PayrollFrequency>W</PayrollFrequency>", "<PayrollFrequency>3</PayrollFrequency>").Trim();
                    response = response.Replace("<PayrollFrequency>B</PayrollFrequency>", "<PayrollFrequency>4</PayrollFrequency>").Trim();
                    response = response.Replace("<PayrollFrequency>H</PayrollFrequency>", "<PayrollFrequency>5</PayrollFrequency>").Trim();
                    response = response.Replace("<PayrollFrequency>M</PayrollFrequency>", "<PayrollFrequency>6</PayrollFrequency>").Trim();
                    response = response.Replace("<PayrollFrequency>Q</PayrollFrequency>", "<PayrollFrequency>7</PayrollFrequency>").Trim();
                    response = response.Replace("<PayrollFrequency>S</PayrollFrequency>", "<PayrollFrequency>8</PayrollFrequency>").Trim();


                    if (Convert.ToDouble(_partnerID) == (double)PartnerFlag.Penco) // IT- 93048 ******** updating payroll codes for CSC 
                    {
                        response = response.Replace("<PayrollFrequency>4</PayrollFrequency>", "<PayrollFrequency>6</PayrollFrequency>").Trim();
                        response = response.Replace("<PayrollFrequency>3</PayrollFrequency>", "<PayrollFrequency>5</PayrollFrequency>").Trim();
                        response = response.Replace("<PayrollFrequency>2</PayrollFrequency>", "<PayrollFrequency>4</PayrollFrequency>").Trim();
                        response = response.Replace("<PayrollFrequency>1</PayrollFrequency>", "<PayrollFrequency>3</PayrollFrequency>").Trim();
                    }

                }
                oParticipantInfo = refactorGetMemberID(response, ContractID, SubID, FullPptInfo, _partnerID);

                // Mapping Inservice amounts new tags to old ones
                if (Convert.ToDouble(_partnerID) == (double)PartnerFlag.Penco)
                {
                    if (oParticipantInfo.InServiceAllowedPreTax != null && oParticipantInfo.InServiceAllowedPreTax > 0d)
                    {
                        oParticipantInfo.InserviceAmt = oParticipantInfo.InServiceAllowedPreTax;
                    }

                    if (oParticipantInfo.InServiceAllowedRoth != null && oParticipantInfo.InServiceAllowedRoth > 0d)
                    {
                        oParticipantInfo.AfterTaxInserviceAmt = oParticipantInfo.InServiceAllowedRoth;
                    }

                    if (oParticipantInfo.AccountInfo != null)
                    {
                        short index = 0;
                        foreach (var accountInfo in oParticipantInfo.AccountInfo)
                        {
                            if (accountInfo.InserviceAllowedVestedAmt != null && accountInfo.InserviceAllowedVestedAmt > 0d)
                            {
                                oParticipantInfo.AccountInfo[index].VestingInserviceAmt = accountInfo.InserviceAllowedVestedAmt;
                            }
                            index = (short)(index + 1);
                        }
                    }
                }

                sError += Environment.NewLine + "Going to Return this method without any exception";
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oParticipantInfo = new ParticipantInfo();
                oParticipantInfo.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                oParticipantInfo.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error", "ParticipantAdapter::GetParticipantInfo");
                sError += Environment.NewLine + "Exception : " + ex.Message;
                Util.SendMail("bfl@transamerica.com", "Subbaraju.Pakalapati@transamerica.com;Damanjit.Singh@transamerica.com", "GetPPtINFO Exception", sError);
            }

            if (string.IsNullOrEmpty(sessionID))
            {
                sessionID = "00000000-0000-0000-0000-000000000001";
            }

            if (oParticipantInfo.Errors[0].Number == 0)
            {
                AudienceDC.UpdateTransactionStatus(sessionID, (PartnerFlag)Convert.ToInt32(_partnerID), (TransactionType)C_GET_PARTICIPANT_INFO, TransactionStatus.Success, responseLog, oParticipantInfo.Errors[0].Number.ToString(), null);
            }
            else
            {
                AudienceDC.UpdateTransactionStatus(sessionID, (PartnerFlag)Convert.ToInt32(_partnerID), (TransactionType)C_GET_PARTICIPANT_INFO, TransactionStatus.Failed, responseLog, oParticipantInfo.Errors[0].Number.ToString(), null);
            }


            return oParticipantInfo;
        }
        public string DoP3ResponseMapping(Services.DIAWSPPT.getParticipantInfoResponse p3Respons)
        {
            string response;
            string YrsofService = Convert.ToInt32(p3Respons.@return.PersonalInfo[0].YearsofService).ToString();
            p3Respons.@return.PersonalInfo[0].YearsofService = YrsofService;
            string HoursYTD = Convert.ToInt32(p3Respons.@return.PersonalInfo[0].HoursWorkedYTD).ToString();
            p3Respons.@return.PersonalInfo[0].HoursWorkedYTD = HoursYTD;
            response = TRSManagers.XMLManager.GetXML(p3Respons);
            response = response.Replace("getParticipantInfoResponse", "ParticipantInfo").Trim();
            response = response.Replace("FundType", "P3FundType").Trim();
            response = response.Replace("<return>", "").Trim();
            response = response.Replace("</return>", "").Trim();
            response = response.Replace("<PayrollFrequency>  </PayrollFrequency>", "<PayrollFrequency>0</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>1 </PayrollFrequency>", "<PayrollFrequency>1</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>52</PayrollFrequency>", "<PayrollFrequency>3</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>24</PayrollFrequency>", "<PayrollFrequency>5</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>26</PayrollFrequency>", "<PayrollFrequency>4</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>12</PayrollFrequency>", "<PayrollFrequency>6</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>04</PayrollFrequency>", "<PayrollFrequency>7</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>3 </PayrollFrequency>", "<PayrollFrequency>3</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>5 </PayrollFrequency>", "<PayrollFrequency>5</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>4 </PayrollFrequency>", "<PayrollFrequency>4</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>6 </PayrollFrequency>", "<PayrollFrequency>6</PayrollFrequency>").Trim();
            response = response.Replace("<PayrollFrequency>7 </PayrollFrequency>", "<PayrollFrequency>7</PayrollFrequency>").Trim();
            response = response.Replace("HardshipAmountAvailable", "HardshipAmt").Trim();
            response = response.Replace("InserviceAmountAvailable", "InserviceAmt").Trim();
            response = response.Replace("<MaxLoanAmt>", "<PlanLoanInfo><MaxLoanAmt>").Trim();
            response = response.Replace("</MaxLoanAmt>", "</MaxLoanAmt></PlanLoanInfo>").Trim();
            response = response.Replace("IsParticipantInEnhancedSecurity", "ES_Active").Trim();
            response = response.Replace("IsEnhancedSecurityEnabledOnPlan", "Plan_ES_Active").Trim();
            response = response.Replace("ES_Active>T", "ES_Active>1").Trim();
            response = response.Replace("Plan_ES_Active>T", "Plan_ES_Active>1").Trim();
            response = response.Replace("ES_Active>F", "ES_Active>0").Trim();
            response = response.Replace("Plan_ES_Active>F", "Plan_ES_Active>1").Trim();
            return response;
        }
        public static ParticipantInfo refactorGetMemberID(string response, string ContractID, string SubID, bool FullPptInfo, string PartnerID)
        {
            ParticipantInfo oParticipantInfo;
            bool bReturn, bReturn2 = default;
            string sErrorMsg = string.Empty;

            try
            {
                oParticipantInfo = (ParticipantInfo)TRSManagers.XMLManager.DeserializeXml(response, typeof(ParticipantInfo));
                if (oParticipantInfo.Errors[0].Number == 0)
                {
                    if (!oParticipantInfo.PersonalInfo.USAddress)
                    {
                        oParticipantInfo.PersonalInfo.State = "ZZ";
                    }
                    oParticipantInfo.PersonalInfo.Telephone = TRSManagers.FormatManager.ParsePhoneNumber(oParticipantInfo.PersonalInfo.Telephone);
                    oParticipantInfo.PersonalInfo.WorkTelephone = TRSManagers.FormatManager.ParsePhoneNumber(oParticipantInfo.PersonalInfo.WorkTelephone);

                    oParticipantInfo.PersonalInfo.RehireDt = oParticipantInfo.PersonalInfo.RehireDt == "01/01/1900" ? "" : oParticipantInfo.PersonalInfo.RehireDt;

                    if (oParticipantInfo.PlanInfo != null && oParticipantInfo.PlanInfo.DefaultFund != null)
                    {
                        oParticipantInfo.PlanInfo.DefaultFund.FundID = oParticipantInfo.PlanInfo.DefaultFund.PartnerFundID;
                    }
                    if (oParticipantInfo.AccountInfo != null)
                    {
                        bReturn = FundMappingsDC.UpdatePartnerAccountMappings(PartnerFlag.Penco, ref oParticipantInfo.AccountInfo, ref sErrorMsg);
                    }
                    else
                    {
                        bReturn = true;
                    }

                    if (Convert.ToDouble(PartnerID) == (double)PartnerFlag.ISC)
                    {

                        if (oParticipantInfo.PlanInfo == null)
                        {
                            oParticipantInfo.PlanInfo = new PlanInfo();
                        }

                        oParticipantInfo.PlanInfo.ContractID = ContractID;
                        oParticipantInfo.PlanInfo.SubID = SubID;

                        if (FullPptInfo && oParticipantInfo.SourceInfo != null)
                        {
                            int iCnt;
                            oParticipantInfo.AccBal = 0d;
                            oParticipantInfo.InserviceAmt = 0d;
                            oParticipantInfo.HardshipAmt = 0d;
                            var oAccountInfo = new AccountInfo[oParticipantInfo.SourceInfo.Length];
                            for (iCnt = 0; iCnt < oParticipantInfo.SourceInfo.Length; iCnt++)
                            {
                                oAccountInfo[iCnt] = new AccountInfo();
                                oAccountInfo[iCnt].AccID = oParticipantInfo.SourceInfo[iCnt].SourceID;
                                oAccountInfo[iCnt].AccName = oParticipantInfo.SourceInfo[iCnt].SourceName;
                                oAccountInfo[iCnt].FundID = "0";
                                oAccountInfo[iCnt].MarketVal = oParticipantInfo.SourceInfo[iCnt].TotalBalance;
                                oAccountInfo[iCnt].VstPct = oParticipantInfo.SourceInfo[iCnt].VestingPercent;
                                oAccountInfo[iCnt].VstVal = oParticipantInfo.SourceInfo[iCnt].VestingBalance;
                                oAccountInfo[iCnt].VestingInserviceAmt = oParticipantInfo.SourceInfo[iCnt].AvailableInserviceBalance;
                                oAccountInfo[iCnt].VestingInservice59Amt = oParticipantInfo.SourceInfo[iCnt].AvailableInserviceBalance;
                                oAccountInfo[iCnt].VestingHardshipAmt = oParticipantInfo.SourceInfo[iCnt].AvailableHardshipBalance;
                                oAccountInfo[iCnt].SourceTypeC = oParticipantInfo.SourceInfo[iCnt].SourceTypeC;
                                oParticipantInfo.AccBal += oParticipantInfo.SourceInfo[iCnt].VestingBalance;
                                oParticipantInfo.InserviceAmt += oParticipantInfo.SourceInfo[iCnt].AvailableInserviceBalance;
                                oParticipantInfo.HardshipAmt += oParticipantInfo.SourceInfo[iCnt].AvailableHardshipBalance;
                            }
                            oParticipantInfo.AccountInfo = oAccountInfo;
                            oParticipantInfo.VstBal = oParticipantInfo.AccBal;

                        }


                        if (FullPptInfo && oParticipantInfo.SourceGroupInfo != null)
                        {
                            int iCnt, iCnt2;
                            for (iCnt = 0; iCnt < oParticipantInfo.SourceGroupInfo.Length; iCnt++)
                            {
                                if (oParticipantInfo.SourceGroupInfo[iCnt].FundInfo != null)
                                {
                                    for (iCnt2 = 0; iCnt2 < oParticipantInfo.SourceGroupInfo[iCnt].FundInfo.Length; iCnt2++)
                                    {
                                        if (DateTime.TryParse(oParticipantInfo.SourceGroupInfo[iCnt].FundInfo[iCnt2].StartDate, out DateTime startDate) && startDate > DateTime.Now)
                                        {
                                            oParticipantInfo.SourceGroupInfo[iCnt].FundInfo[iCnt2].MaxContribution = 0d;
                                        }
                                        if (DateTime.TryParse(oParticipantInfo.SourceGroupInfo[iCnt].FundInfo[iCnt2].CloseDate, out DateTime closeDate) && closeDate <= DateTime.Now)
                                        {
                                            if (oParticipantInfo.SourceGroupInfo[iCnt].FundInfo[iCnt2].ContrDirection > 0d)
                                            {
                                                oParticipantInfo.SourceGroupInfo[iCnt].FundInfo[iCnt2].DisplayOnly = true;
                                            }
                                            else
                                            {
                                                oParticipantInfo.SourceGroupInfo[iCnt].FundInfo[iCnt2].MaxContribution = 0d;
                                            }

                                        }
                                        switch (oParticipantInfo.SourceGroupInfo[iCnt].FundInfo[iCnt2].AssetName)
                                        {
                                            case "Q1QL":
                                            case "Q1QM":
                                            case "R2BA":
                                                {
                                                    oParticipantInfo.SourceGroupInfo[iCnt].FundInfo[iCnt2].MaxContribution = 0d;
                                                    break;
                                                }
                                        }
                                    }

                                    bReturn2 = FundMappingsDC.UpdatePartnerFundMappings((PartnerFlag)Convert.ToInt32(PartnerID), ref oParticipantInfo.SourceGroupInfo[iCnt].FundInfo, ref sErrorMsg, oParticipantInfo.PlanInfo.ContractID, oParticipantInfo.PlanInfo.SubID);

                                }
                            }
                            if (oParticipantInfo.SourceGroupInfo[0].FundInfo != null)
                            {
                                oParticipantInfo.FundInfo = oParticipantInfo.SourceGroupInfo[0].FundInfo;
                            }
                        }
                        else
                        {
                            bReturn2 = true;
                        }

                        if (FullPptInfo && oParticipantInfo.PriorQbadWithdrawals != null)
                        {
                            int iCnt;
                            var oPriorQbadWithdrawals = new PriorQbadWithdrawals[oParticipantInfo.PriorQbadWithdrawals.Length];
                            for (iCnt = 0; iCnt < oParticipantInfo.PriorQbadWithdrawals.Length; iCnt++)
                            {
                                oPriorQbadWithdrawals[iCnt] = new PriorQbadWithdrawals();
                                oPriorQbadWithdrawals[iCnt].ChildTIN = oParticipantInfo.PriorQbadWithdrawals[iCnt].ChildTIN;
                                oPriorQbadWithdrawals[iCnt].AmountRequested = oParticipantInfo.PriorQbadWithdrawals[iCnt].AmountRequested;
                            }
                            oParticipantInfo.PriorQbadWithdrawals = oPriorQbadWithdrawals;
                        }

                        if (FullPptInfo && oParticipantInfo.QbadDistributionFees != null)
                        {
                            var oQbadDistributionFees = new QbadDistributionFees();
                            oQbadDistributionFees.TpaFeeAmount = oParticipantInfo.QbadDistributionFees.TpaFeeAmount;
                            oQbadDistributionFees.TpaFeePaidByCode = oParticipantInfo.QbadDistributionFees.TpaFeePaidByCode;
                            oQbadDistributionFees.TrsFeeAmount = oParticipantInfo.QbadDistributionFees.TrsFeeAmount;
                            oQbadDistributionFees.TrsFeePaidByCode = oParticipantInfo.QbadDistributionFees.TrsFeePaidByCode;
                            oParticipantInfo.QbadDistributionFees = oQbadDistributionFees;
                        }
                    }

                    else if (oParticipantInfo.FundInfo != null)
                    {
                        bReturn2 = FundMappingsDC.UpdatePartnerFundMappings(PartnerFlag.Penco, ref oParticipantInfo.FundInfo, ref sErrorMsg, oParticipantInfo.PlanInfo.ContractID, oParticipantInfo.PlanInfo.SubID);
                    }
                    else
                    {
                        bReturn2 = true;
                    }

                    if (bReturn == false || bReturn2 == false)
                    {
                        oParticipantInfo.Errors[0].Number = (int)ErrorCodes.IncompleteData;
                        oParticipantInfo.Errors[0].Description = General.FormatErrorMsg(sErrorMsg, "Error", "ParticipantAdapter::GetParticipantInfo");
                    }
                }

                else if (oParticipantInfo.Errors[0].Number != (int)ErrorCodes.NoOnlineAccessAvailable)
                {
                    oParticipantInfo = new ParticipantInfo();
                    oParticipantInfo.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                    oParticipantInfo.Errors[0].Description = "Partner is unavailable";

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oParticipantInfo = new ParticipantInfo();
                oParticipantInfo.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                oParticipantInfo.Errors[0].Description = "refactor : Partner is unavailable";
            }
            return oParticipantInfo;
        }

        public SIResponse UpdatePersonalProfile(string sessionID, PersonalProfile profile)
        {
            SessionInfo oSessionInfo;
            string sRequest;
            SIResponse oSIResponse;

            // Get Session Info
            oSessionInfo = AudienceDC.GetSessionInfo(sessionID);
            if (!profile.USAddress)
            {
                profile.State = "";
                profile.AddressLine2 = "";
            }

            // Get the XML for profile
            sRequest = TRSManagers.XMLManager.GetXML(profile);

            sRequest = sRequest.Replace("<PayrollFrequency>0</PayrollFrequency>", "<PayrollFrequency></PayrollFrequency>").Trim();
            sRequest = sRequest.Replace("<PayrollFrequency>1</PayrollFrequency>", "<PayrollFrequency></PayrollFrequency>").Trim();

            if (Convert.ToDouble(_partnerID) == (double)PartnerFlag.Penco) // IT- 93048 ******** updating payroll codes for CSC 
            {
                sRequest = sRequest.Replace("<PayrollFrequency>3</PayrollFrequency>", "<PayrollFrequency>1</PayrollFrequency>").Trim();
                sRequest = sRequest.Replace("<PayrollFrequency>4</PayrollFrequency>", "<PayrollFrequency>2</PayrollFrequency>").Trim();
                sRequest = sRequest.Replace("<PayrollFrequency>5</PayrollFrequency>", "<PayrollFrequency>3</PayrollFrequency>").Trim();
                sRequest = sRequest.Replace("<PayrollFrequency>6</PayrollFrequency>", "<PayrollFrequency>4</PayrollFrequency>").Trim();
            }

            if (profile.SignatureCard)
            {
                oSIResponse = GenericSubmit(sessionID, profile.SSN, TransactionType.P3HoursWorked, profile.HoursWorkedYTD.ToString());
            }
            else
            {
                oSIResponse = GenericSubmit(sessionID, sRequest, TransactionType.UpdateProfile);
            }
            return oSIResponse;
        }

        public SIResponse RequestConfirmationLetter(string sessionID, ConfirmationLetterInfo oConfirmationInfo)
        {
            string sRequest;
            SIResponse oSIResponse;

            // Get the XML for profile
            sRequest = TRSManagers.XMLManager.GetXML(oConfirmationInfo);
            oSIResponse = GenericSubmit(sessionID, sRequest, TransactionType.RequestConfirmationLetter);

            return oSIResponse;
        }

        public SIResponse RequestConfirmationLetter(int InLoginID, string ContractID, string SubID, ConfirmationLetterInfo oConfirmationInfo)
        {
            string sRequest, sResponse;
            SIResponse oSIResponse;
            var oTransInfo = new TransactionInformation();

            // Get the XML for profile
            sRequest = TRSManagers.XMLManager.GetXML(oConfirmationInfo);
            // Get PartnerUSER ID
            string memberID = string.Empty;
            string planID = string.Empty;

            try
            {
                // Get the planID and memberID
                ParticipantDC.GetPartner(InLoginID, ContractID, SubID, ref planID, ref memberID);
                oTransInfo.status = TransactionStatus.Pending;

                if (_demoPartnerService != null)
                {
                    //PROBLEM, NO DIAWSPPT service for StatementRequest
                    //sResponse = _demoPartnerService.GenerateConfirmationLetter(memberID, sRequest);
                    throw new NotImplementedException("Demo service does not support GenerateConfirmationLetter");
                }
                else if (_participantClient != null)
                {
                    //PROBLEM, NO DIAWSPPT service for StatementRequest
                    //sResponse = _diaPartnerService.GenerateConfirmationLetter(memberID, sRequest);
                    throw new NotImplementedException("DIAWSPPT service does not support GenerateConfirmationLetter");
                }
                else
                {
                    throw new Exception("No partner service initialized");
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oSIResponse = new SIResponse();
                if (oTransInfo.status == TransactionStatus.Pending)
                {
                    oSIResponse.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                }
                else
                {
                    oSIResponse.Errors[0].Number = (int)ErrorCodes.Unknown;
                }

                oSIResponse.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error", "ParticipantAdapter::Type - " + ((int)TransactionType.RequestConfirmationLetter).ToString());
            }

            // Insert audit record
            oTransInfo.ErrorCode = oSIResponse.Errors[0].Number.ToString();
            oTransInfo.Type = TransactionType.RequestConfirmationLetter;
            oTransInfo.PartnerConfID = oSIResponse.ConfirmationNumber;
            oTransInfo.PartnerID = PartnerFlag.Penco;
            oTransInfo.SessionID = "00000000-0000-0000-0000-000000000002";
            oTransInfo.transactionData = sRequest;

            // Store the transaction information
            SubmitTransaction(oTransInfo, oSIResponse);
            return oSIResponse;
        }

        public void SubmitTransaction(TransactionInformation transInfo, SIResponse oSIResponse)
        {
            int transID;
            transInfo.IsFinancial = AudienceDC.IsFinancialTransacion((int)transInfo.Type);

            if (Convert.ToDouble(transInfo.ErrorCode) == 0d && transInfo.IsFinancial)
            {
                // successful transaction
                transInfo.status = TransactionStatus.PendingFinancial;
            }
            else if (Convert.ToDouble(transInfo.ErrorCode) == 0d)
            {
                transInfo.status = TransactionStatus.Success;
            }
            else if (Convert.ToDouble(transInfo.ErrorCode) != 0d && transInfo.IsFinancial == false)
            {
                transInfo.status = TransactionStatus.Failed;
            }
            else
            {
                transInfo.status = TransactionStatus.Failed;
            }

            transID = AudienceDC.UpdateTransactionStatus(transInfo.SessionID, transInfo.PartnerID, transInfo.Type, transInfo.status, transInfo.transactionData, transInfo.ErrorCode, transInfo.PartnerConfID);

            if (Convert.ToDouble(transInfo.ErrorCode) == 0d)
            {
                oSIResponse.TransIDs = [transID];
            }
        }

        private SIResponse GenericSubmit(string sessionID, string sRequest, TransactionType @type, string other = null, string sReviewSessionID = "")
        {
            SessionInfo oSessionInfo;
            SIResponse oSIResponse;
            string sResponse = string.Empty;
            var oTransInfo = new TransactionInformation();

            try
            {
                // Get Session Info
                oSessionInfo = AudienceDC.GetSessionInfo(sessionID);

                // Determine the web service to call and then execute
                switch (type)
                {
                    case TransactionType.P3HoursWorked:
                        {
                            oTransInfo.status = TransactionStatus.Pending;
                            var pInfo = new Services.DIAWSPPT.updateHoursWorked();
                            var p3Response = new Services.DIAWSPPT.updateHoursWorkedResponse();


                            pInfo.ParticipantInfo = new Services.DIAWSPPT.updateHoursWorkedParticipantInfo();
                            pInfo.ParticipantInfo.PersonalInfo = new Services.DIAWSPPT.updateHoursWorkedParticipantInfoPersonalInfo[2];
                            pInfo.ParticipantInfo.PersonalInfo[0] = new Services.DIAWSPPT.updateHoursWorkedParticipantInfoPersonalInfo();
                            pInfo.ParticipantInfo.PersonalInfo[0].HoursWorkedYTD = other;
                            pInfo.ParticipantInfo.PersonalInfo[0].SSN = sRequest;

                            pInfo.Account_Number = (oSessionInfo.ContractID + new string(' ', 10)).Substring(0, 10) +
                                                   (new string('0', 5) + General.SubOut(oSessionInfo.SubID)).Substring((new string('0', 5) + General.SubOut(oSessionInfo.SubID)).Length - 5);

                            // Requestor Name
                            pInfo.Session_ID = sessionID;      // oSessionInfo. 

                            sRequest = "arg0 :  " + TRSManagers.XMLManager.GetXML(pInfo.ParticipantInfo) + ", arg1 : " + pInfo.Account_Number + "; arg2 : " + pInfo.Session_ID;

                            p3Response = _participantClient.updateHoursWorked(pInfo);
                            oSIResponse = new SIResponse();
                            if (!p3Response.@return)
                            {
                                oSIResponse.Errors[0].Number = (int)ErrorCodes.Unknown;
                                oSIResponse.Errors[0].Description = "Unable to update Participant Hours worked on P3";
                            }
                            oSIResponse.AdditionalData = TRSManagers.XMLManager.GetXML(pInfo.ParticipantInfo);
                            sResponse = TRSManagers.XMLManager.GetXML(oSIResponse);
                            break;
                        }
                    case TransactionType.UpdateProfile:
                        {
                            oTransInfo.status = TransactionStatus.Pending;
                            oSIResponse = new SIResponse();

                            if (Convert.ToDouble(_partnerID) == (double)PartnerFlag.ISC)
                            {

                                var pInfo = new Services.DIAWSPPT.updateParticipantInfo();
                                var p3Response = new Services.DIAWSPPT.updateParticipantInfoResponse();

                                // Participant Info ibject
                                var oPptInfo = new ParticipantInfo();
                                sRequest = sRequest.Replace("<PayrollFrequency></PayrollFrequency>", "<PayrollFrequency>1</PayrollFrequency>").Trim();

                                oPptInfo.PersonalInfo = (PersonalProfile)TRSManagers.XMLManager.DeserializeXml(sRequest, typeof(PersonalProfile));
                                pInfo.ParticipantInfo = new Services.DIAWSPPT.updateParticipantInfoParticipantInfo();
                                pInfo.ParticipantInfo.PersonalInfo = new Services.DIAWSPPT.updateParticipantInfoParticipantInfoPersonalInfo[2];
                                pInfo.ParticipantInfo.PersonalInfo[0] = new Services.DIAWSPPT.updateParticipantInfoParticipantInfoPersonalInfo();
                                pInfo.ParticipantInfo.PersonalInfo[0].Email = oPptInfo.PersonalInfo.Email;
                                pInfo.ParticipantInfo.PersonalInfo[0].SSN = oPptInfo.PersonalInfo.SSN;
                                pInfo.ParticipantInfo.PersonalInfo[0].Title = oPptInfo.PersonalInfo.Title;
                                pInfo.ParticipantInfo.PersonalInfo[0].Suffix = oPptInfo.PersonalInfo.Suffix;
                                pInfo.ParticipantInfo.PersonalInfo[0].FirstName = oPptInfo.PersonalInfo.FirstName;
                                pInfo.ParticipantInfo.PersonalInfo[0].MiddleInitial = oPptInfo.PersonalInfo.MiddleInitial;
                                pInfo.ParticipantInfo.PersonalInfo[0].LastName = oPptInfo.PersonalInfo.LastName;
                                pInfo.ParticipantInfo.PersonalInfo[0].GroupCode = oPptInfo.PersonalInfo.GroupCode;
                                pInfo.ParticipantInfo.PersonalInfo[0].LocationCode = oPptInfo.PersonalInfo.LocationCode;
                                pInfo.ParticipantInfo.PersonalInfo[0].USAddress = oPptInfo.PersonalInfo.USAddress ? "1" : "0";
                                if (!oPptInfo.PersonalInfo.USAddress)
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].Country = oPptInfo.PersonalInfo.Country;
                                }
                                pInfo.ParticipantInfo.PersonalInfo[0].AddressLine1 = oPptInfo.PersonalInfo.AddressLine1;
                                pInfo.ParticipantInfo.PersonalInfo[0].AddressLine2 = oPptInfo.PersonalInfo.AddressLine2;
                                pInfo.ParticipantInfo.PersonalInfo[0].City = oPptInfo.PersonalInfo.City;
                                pInfo.ParticipantInfo.PersonalInfo[0].State = oPptInfo.PersonalInfo.State;
                                pInfo.ParticipantInfo.PersonalInfo[0].ZipCode1 = oPptInfo.PersonalInfo.ZipCode1;
                                pInfo.ParticipantInfo.PersonalInfo[0].ZipCode2 = oPptInfo.PersonalInfo.ZipCode2;
                                pInfo.ParticipantInfo.PersonalInfo[0].WorkTelephone = oPptInfo.PersonalInfo.WorkTelephone;
                                pInfo.ParticipantInfo.PersonalInfo[0].WorkTelephoneExt = oPptInfo.PersonalInfo.WorkTelephoneExt;
                                pInfo.ParticipantInfo.PersonalInfo[0].Telephone = oPptInfo.PersonalInfo.Telephone;

                                if (DateTime.TryParse(oPptInfo.PersonalInfo.BirthDt, out DateTime birthDate) &&
                                    !string.IsNullOrEmpty(birthDate.ToShortDateString()) &&
                                    birthDate.ToShortDateString() != "01/01/0001" &&
                                    birthDate.ToShortDateString() != "1/1/0001")
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].BirthDt = oPptInfo.PersonalInfo.BirthDt;
                                }
                                else
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].BirthDt = "";
                                }

                                if (DateTime.TryParse(oPptInfo.PersonalInfo.EmploymentDt, out DateTime employmentDate) &&
                                    !string.IsNullOrEmpty(employmentDate.ToShortDateString()) &&
                                    employmentDate.ToShortDateString() != "1/1/0001" &&
                                    employmentDate.ToShortDateString() != "01/01/0001")
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].EmploymentDt = oPptInfo.PersonalInfo.EmploymentDt;
                                }
                                else
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].EmploymentDt = "";
                                }

                                if (DateTime.TryParse(oPptInfo.PersonalInfo.RehireDt, out DateTime rehireDate) &&
                                    !string.IsNullOrEmpty(rehireDate.ToShortDateString()) &&
                                    rehireDate.ToShortDateString() != "1/1/0001" &&
                                    rehireDate.ToShortDateString() != "01/01/0001")
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].RehireDt = oPptInfo.PersonalInfo.RehireDt;
                                }
                                else
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].RehireDt = "N/A";
                                }

                                if (DateTime.TryParse(oPptInfo.PersonalInfo.VestingDt, out DateTime vestingDate) &&
                                    !string.IsNullOrEmpty(vestingDate.ToShortDateString()) &&
                                    vestingDate.ToShortDateString() != "1/1/0001" &&
                                    vestingDate.ToShortDateString() != "01/01/0001")
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].VestingDt = oPptInfo.PersonalInfo.VestingDt;
                                }
                                else
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].VestingDt = "";
                                }

                                if (DateTime.TryParse(oPptInfo.PersonalInfo.TerminationDt, out DateTime terminationDate) &&
                                    !string.IsNullOrEmpty(terminationDate.ToShortDateString()) &&
                                    terminationDate.ToShortDateString() != "1/1/0001" &&
                                    terminationDate.ToShortDateString() != "01/01/0001")
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].TerminationDt = oPptInfo.PersonalInfo.TerminationDt;
                                }
                                else
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].TerminationDt = "N/A";
                                }
                                if (oPptInfo.PersonalInfo.UpdateYOS)
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].YearsofService = oPptInfo.PersonalInfo.YearsofService.ToString();
                                }
                                pInfo.ParticipantInfo.PersonalInfo[0].PayrollFrequency = ((int)oPptInfo.PersonalInfo.PayrollFrequency).ToString();
                                switch (oPptInfo.PersonalInfo.PayrollFrequency)
                                {
                                    case PayrollFrequency.Weekly:
                                        {
                                            pInfo.ParticipantInfo.PersonalInfo[0].PayrollFrequency = "52";
                                            break;
                                        }
                                    case PayrollFrequency.BiWeekly:
                                        {
                                            pInfo.ParticipantInfo.PersonalInfo[0].PayrollFrequency = "26";
                                            break;
                                        }
                                    case PayrollFrequency.SemiMonthly:
                                        {
                                            pInfo.ParticipantInfo.PersonalInfo[0].PayrollFrequency = "24";
                                            break;
                                        }
                                    case PayrollFrequency.Monthly:
                                        {
                                            pInfo.ParticipantInfo.PersonalInfo[0].PayrollFrequency = "12";
                                            break;
                                        }
                                    case PayrollFrequency.Quarterly:
                                        {
                                            pInfo.ParticipantInfo.PersonalInfo[0].PayrollFrequency = "04";
                                            break;
                                        }

                                    default:
                                        {
                                            pInfo.ParticipantInfo.PersonalInfo[0].PayrollFrequency = "  ";
                                            break;
                                        }
                                }

                                if (oPptInfo.PersonalInfo.UpdateHoursWorked)
                                {
                                    pInfo.ParticipantInfo.PersonalInfo[0].HoursWorkedYTD = oPptInfo.PersonalInfo.HoursWorkedYTD.ToString();
                                }

                                pInfo.Account_Number = (oSessionInfo.ContractID + new string(' ', 10)).Substring(0, 10) +
                                                       (new string('0', 5) + General.SubOut(oSessionInfo.SubID)).Substring((new string('0', 5) + General.SubOut(oSessionInfo.SubID)).Length - 5);

                                // Requestor Name
                                pInfo.Session_ID = sessionID;      // oSessionInfo. 

                                sRequest = "arg0 :  " + TRSManagers.XMLManager.GetXML(pInfo.ParticipantInfo) + ", arg1 : " + pInfo.Account_Number + "; arg2 : " + pInfo.Session_ID;

                                p3Response = _participantClient.updateParticipantInfo(pInfo);
                                oSIResponse = new SIResponse();
                                if (!p3Response.@return)
                                {
                                    oSIResponse.Errors[0].Number = (int)ErrorCodes.Unknown;
                                    oSIResponse.Errors[0].Description = "Unable to update Participant profile on P3";
                                }
                                oSIResponse.AdditionalData = TRSManagers.XMLManager.GetXML(pInfo.ParticipantInfo);
                                sResponse = TRSManagers.XMLManager.GetXML(oSIResponse);
                            }
                            // End If
                            else
                            {
                                if (_demoPartnerService != null)
                                {
                                    sResponse = _demoPartnerService.UpdatePersonalProfile(oSessionInfo.PartnerUserID, sRequest);
                                }
                                else if (_participantClient != null)
                                {
                                    //PROBLEM, NO DIAWSPPT service for UpdatePersonalProfile
                                    //sResponse = _diaPartnerService.UpdatePersonalProfile(oSessionInfo.PartnerUserID, sRequest);
                                    throw new NotImplementedException("DIAWSPPT service does not support UpdatePersonalProfile");
                                }
                            }

                            break;
                        }
                    case TransactionType.RequestConfirmationLetter:
                        {
                            oTransInfo.status = TransactionStatus.Pending;
                            if (_demoPartnerService != null)
                            {
                                //PROBLEM, NO DIAWSPPT service for GenerateConfirmationLetter
                                //sResponse = _demoPartnerService.GenerateConfirmationLetter(oSessionInfo.PartnerUserID, sRequest);
                                throw new NotImplementedException("Demo service does not support GenerateConfirmationLetter");
                            }
                            else if (_participantClient != null)
                            {
                                //PROBLEM, NO DIAWSPPT service for GenerateConfirmationLetter
                                //sResponse = _diaPartnerService.GenerateConfirmationLetter(oSessionInfo.PartnerUserID, sRequest);
                                throw new NotImplementedException("DIAWSPPT service does not support GenerateConfirmationLetter");
                            }
                            break;
                        }

                }

                // De-serialize the response
                oSIResponse = (SIResponse)TRSManagers.XMLManager.DeserializeXml(sResponse, typeof(SIResponse));
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oSIResponse = new SIResponse();
                if (oTransInfo.status == TransactionStatus.Pending)
                {
                    oSIResponse.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                }
                else
                {
                    oSIResponse.Errors[0].Number = (int)ErrorCodes.Unknown;
                }
                oTransInfo.status = TransactionStatus.Failed;
                oSIResponse.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error", "ParticipantAdapter::Type - " + ((int)type).ToString());
                sResponse += oSIResponse.Errors[0].Description;

            }

            // Insert audit record
            oTransInfo.ErrorCode = oSIResponse.Errors[0].Number.ToString();
            oTransInfo.Type = type;
            oTransInfo.PartnerConfID = oSIResponse.ConfirmationNumber;
            oTransInfo.PartnerID = (PartnerFlag)Convert.ToInt32(_partnerID);
            oTransInfo.SessionID = sessionID;
            if (!string.IsNullOrEmpty(sReviewSessionID))
            {
                oTransInfo.SessionID = sReviewSessionID;
            }

            oTransInfo.transactionData = sRequest + sResponse;

            // Store the transaction information
            SubmitTransaction(oTransInfo, oSIResponse);

            return oSIResponse;
        }

        private void LoadBalance()
        {

            string svcEndpoint = "WS_CscPptSvcEndpoint";
            string svcEndpointAlt = "WS_CscPptSvcEndpointAlt";
            GetSvcEndpoints(ref svcEndpoint, ref svcEndpointAlt);

            try
            {
                TRSWEBGlobal.ResetURLAvailable = "true";

                if (string.IsNullOrEmpty(TRSWEBGlobal.url1))
                {
                    TRSWEBGlobal.url1 = svcEndpoint;
                    TRSWEBGlobal.url1Avail = true;
                }

                if (string.IsNullOrEmpty(TRSWEBGlobal.url2))
                {
                    TRSWEBGlobal.url2 = svcEndpointAlt;
                    TRSWEBGlobal.url2Avail = true;
                }

                if (string.IsNullOrEmpty(TRSWEBGlobal.LastURL) || (TRSWEBGlobal.LastURL ?? "") == (TRSWEBGlobal.url2 ?? ""))
                {
                    if (TRSWEBGlobal.url1Avail)
                    {
                        TRSWEBGlobal.LastURL = TRSWEBGlobal.url1;
                    }
                    else if (TRSWEBGlobal.url2Avail)
                    {
                        TRSWEBGlobal.LastURL = TRSWEBGlobal.url2;
                    }
                    else
                    {
                        TRSWEBGlobal.LastURL = svcEndpoint;
                    }
                }
                else if (TRSWEBGlobal.url2Avail)
                {
                    TRSWEBGlobal.LastURL = TRSWEBGlobal.url2;
                }
                else if (TRSWEBGlobal.url1Avail)
                {
                    TRSWEBGlobal.LastURL = TRSWEBGlobal.url1;
                }
                else
                {
                    TRSWEBGlobal.LastURL = svcEndpoint;
                }
                svcEndpoint = TRSWEBGlobal.LastURL;
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
            }

            _pptSvc = new PencoPptSvc.PencoParticipantServiceClient(PencoPptSvc.PencoParticipantServiceClient.EndpointConfiguration.WSHttpBinding_IPencoParticipantService);


        }
        private void ClosePPTSvc()
        {
            if (!(_pptSvc == null))
            {
                if (_pptSvc.State == System.ServiceModel.CommunicationState.Faulted)
                {
                    _pptSvc.Abort();
                }
                else if (_pptSvc.State != System.ServiceModel.CommunicationState.Closed)
                {
                    _pptSvc.Close();
                }
            }
        }
        private void ResetURLsUnavailableCount()
        {
            if ((TRSWEBGlobal.LastURL ?? "") == (TRSWEBGlobal.url1 ?? ""))
            {
                TRSWEBGlobal.url1UnavailCount = 0;
            }
            else if ((TRSWEBGlobal.LastURL ?? "") == (TRSWEBGlobal.url2 ?? ""))
            {
                TRSWEBGlobal.url2UnavailCount = 0;
            }
        }

        private void MarkCSCURLUnavailable()
        {
            if ((TRSWEBGlobal.LastURL ?? "") == (TRSWEBGlobal.url1 ?? ""))
            {
                TRSWEBGlobal.url1UnavailCount = TRSWEBGlobal.url1UnavailCount + 1;
                switch (TRSWEBGlobal.url1UnavailCount)
                {
                    case 1:
                        {
                            TRSWEBGlobal.URL1UnavailableStart = DateTime.Now.ToString();
                            break;
                        }
                    case var @case when @case >= 3:
                        {
                            TRSWEBGlobal.url1Avail = false;
                            SendUrlUnavailableAlert(TRSWEBGlobal.url1);
                            break;
                        }
                }
            }
            else if ((TRSWEBGlobal.LastURL ?? "") == (TRSWEBGlobal.url2 ?? ""))
            {
                TRSWEBGlobal.url2UnavailCount = TRSWEBGlobal.url2UnavailCount + 1;
                switch (TRSWEBGlobal.url2UnavailCount)
                {
                    case 1:
                        {
                            TRSWEBGlobal.URL2UnavailableStart = DateTime.Now.ToString();
                            break;
                        }
                    case var case1 when case1 >= 3:
                        {
                            TRSWEBGlobal.url2Avail = false;
                            SendUrlUnavailableAlert(TRSWEBGlobal.url2);
                            break;
                        }
                }
            }

        }

        private void SendUrlUnavailableAlert(string url)
        {
            Util.SendMail("CSCSvcUnavailable_" + Environment.MachineName + "@transamerica.com", "Subbaraju.Pakalapati@transamerica.com;Damanjit.Singh@transamerica.com;Alvin.McBride@transamerica.com", "CSC Web Service Unavailable", "CSC WCF Service on " + url + " had 3 consecutive errors and is marked unavailable for next 5 minutes. Please check.");
        }

        private void GetSvcEndpoints(ref string svcEndpoint, ref string svcEndpointAlt)
        {

            if (!(TrsAppSettings.AppSettings.GetValue("CscEndPoint") == null))
            {
                svcEndpoint = TrsAppSettings.AppSettings.GetValue("CscEndPoint").ToString();
            }
            if (!(TrsAppSettings.AppSettings.GetValue("CscEndPointAlt") == null))
            {
                svcEndpointAlt = TrsAppSettings.AppSettings.GetValue("CscEndPointAlt").ToString();
            }

        }
        private bool UseWcfService()
        {
            string svcEndpoint = string.Empty;
            string svcEndpointAlt = string.Empty;

            GetSvcEndpoints(ref svcEndpoint, ref svcEndpointAlt);

            if (string.IsNullOrEmpty(svcEndpoint) && string.IsNullOrEmpty(svcEndpointAlt))
            {
                return false;
            }
            return true;
        }

        public ParticipantWithdrawalsInfo GetDistributionInfo(string sessionID, string contractId, string subId, string ssn_no)
        {
            ParticipantWithdrawalsInfo oPptDistInfo = null;
            if (ssn_no.Length == 9)
            {
                ssn_no = string.Format("{0}-{1}-{2}", ssn_no.Substring(0, 3), ssn_no.Substring(3, 2), ssn_no.Substring(5));
                // ssn_no = String.Format("{0:000-00-0000}", CType(ssn_no, Integer))
            }

            string sResponse = _ISCWithdrawalSvc.getPptDistributionInfo(contractId, General.SubOut(subId), ssn_no);
            SIResponse oSIResponse = (SIResponse)TRSManagers.XMLManager.DeserializeXml(sResponse, typeof(SIResponse));

            if (oSIResponse.Errors[0].Number == 0)
            {
                string distInfo = oSIResponse.AdditionalData;
                distInfo = distInfo.Replace("com.divinvest.businessobject.P3DataSentToInitiateTPAdistribution", "ParticipantWithdrawalsInfo");
                distInfo = distInfo.Replace("com.divinvest.businessobject.HardshipReason", "hardshipReason");

                // Setting default values in absence of a value
                distInfo = distInfo.Replace("<federalTaxWithholding></federalTaxWithholding>", "<federalTaxWithholding>0</federalTaxWithholding>");
                distInfo = distInfo.Replace("<stateTaxWithholdingType></stateTaxWithholdingType>", "<stateTaxWithholdingType>0</stateTaxWithholdingType>");
                distInfo = distInfo.Replace("<maxOnlineWithdrawalLimit></maxOnlineWithdrawalLimit>", "<maxOnlineWithdrawalLimit>0</maxOnlineWithdrawalLimit>");
                distInfo = distInfo.Replace("<priorWithdrawalsTotal></priorWithdrawalsTotal>", "<priorWithdrawalsTotal>0</priorWithdrawalsTotal>");
                distInfo = distInfo.Replace("<overNightFee></overNightFee>", "<overNightFee>0</overNightFee>");
                distInfo = distInfo.Replace("<stateTaxWithholding></stateTaxWithholding>", "<stateTaxWithholding>0</stateTaxWithholding>");

                // Handling Outstanding Loans tags
                distInfo = distInfo.Replace("outstandingLoansList", "outstandingLoans");
                distInfo = distInfo.Replace("com.divinvest.businessobject.OutstandingLoans", "OutstandingLoan");

                oPptDistInfo = (ParticipantWithdrawalsInfo)TRSManagers.XMLManager.DeserializeXml(distInfo, typeof(ParticipantWithdrawalsInfo));
                AudienceDC.UpdateTransactionStatus(sessionID, (PartnerFlag)Convert.ToInt32(_partnerID), TransactionType.P3GetDistributionInfo, TransactionStatus.Success, ssn_no + ": " + distInfo, oSIResponse.Errors[0].Number.ToString(), null);
            }
            else
            {
                AudienceDC.UpdateTransactionStatus(sessionID, (PartnerFlag)Convert.ToInt32(_partnerID), TransactionType.P3GetDistributionInfo, TransactionStatus.Failed, ssn_no + ": " + sResponse, oSIResponse.Errors[0].Number.ToString(), null);
            }
            return oPptDistInfo;
        }
    }
}