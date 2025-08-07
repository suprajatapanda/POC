using MimeKit;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer;
using TRS.IT.SI.BusinessFacadeLayer.Adapters;
using TRS.IT.SI.BusinessFacadeLayer.DAL;
using TRS.IT.SI.BusinessFacadeLayer.Model;
using TRS.IT.TrsAppSettings;

namespace TarPptBouncedEmailBatch.BLL
{
    public class Participant
    {
        private const int C_CLIENT_WEB = 1;
        private const int C_PARTICIPANT_ESTATEMENT_SIGNUP = 1930;

        private ParticipantDC _ParticipantDC;
        private IParticipantAdapter _ParticipantAdapter;
        private string _SessionID;
        private SessionInfo _SessionInfo;
        private int _ClientType;
        private int _InLoginID;
        private string _ContractID, _SubID;
        private PartnerFlag _partnerID;
        private string _partnerPlanID = "";
        private bool _SessionFlag = true;

        public Participant(string SessionID)
        {
            _SessionFlag = true;
            _SessionID = SessionID;
            _ParticipantDC = new ParticipantDC(SessionID);
            _SessionInfo = AudienceDC.GetSessionInfo(_SessionID);

            switch (ParticipantDC.GetPartnerID(SessionID))
            {
                case PartnerFlag.TAE:
                    {
                        _ParticipantAdapter = new ParticipantAdapter();
                        break;
                    }
                case PartnerFlag.Penco:
                    {
                        _ParticipantAdapter = new ParticipantAdapter_Penco(PartnerFlag.Penco);
                        break;
                    }
                case PartnerFlag.ISC:
                    {
                        _ParticipantAdapter = new ParticipantAdapter_Penco(PartnerFlag.ISC);
                        break;
                    }
                case PartnerFlag.DIA:
                    {
                        _partnerPlanID = _SessionInfo.PlanID;
                        _ParticipantAdapter = new ParticipantAdapter_Penco(PartnerFlag.DIA, _partnerPlanID);
                        break;
                    }

                default:
                    {
                        _ParticipantAdapter = new ParticipantAdapter_Penco(PartnerFlag.TRS);
                        break;
                    }
            }

            _ClientType = C_CLIENT_WEB;
            _partnerID = ParticipantDC.GetPartnerID(SessionID);
            _ContractID = _SessionInfo.ContractID;
            _SubID = _SessionInfo.SubID;
            _InLoginID = _SessionInfo.InLoginID;
        }
        public static string CreateSession(int InloginID, string ContractID, string SubID, int ClientTypeID, string HostID, string SessionID)
        {
            if (ClientTypeID != 6)
            {
                WebSessionManager.ResetSession();
            }
            return ParticipantDC.CreateSessionByInloginID(InloginID, ContractID, SubID, ClientTypeID, HostID, SessionID);
        }
        public ParticipantInfo GetParticipantInfoStayed(bool bPartner = false, bool bGetFundRestrictions = true, bool bFullPptInfo = true, short iOption = 0)
        {
            var oParticipant = default(ParticipantInfo);

            if (_SessionFlag == true | bPartner)
            {
                if (!WebSessionManager.IsPPTObjValid(_SessionID, _ClientType) | bPartner)
                {
                    oParticipant = GetPartnerData(bFullPptInfo);
                    if (iOption == 1)
                    {
                        oParticipant.PlanInfo.WithdrawalsInfo = _ParticipantAdapter.GetDistributionInfo(_SessionID, _ContractID, _SubID, oParticipant.PersonalInfo.SSN);
                    }
                    if (oParticipant.Errors[0].Number == 0)
                    {
                        if (oParticipant.PlanLoanInfo != null)
                        {
                            {
                                ref var withBlock = ref oParticipant.PlanLoanInfo;
                                if (withBlock.CurrentLoans >= withBlock.MaxLoans)
                                {
                                    if (!withBlock.RefinanceAllowed)
                                    {
                                        withBlock.LoanAmtAvail = 0d;
                                    }
                                }
                                else if (withBlock.LoanAmtAvail < withBlock.MinLoanAmt)
                                {
                                    withBlock.LoanAmtAvail = 0d;
                                }
                            }
                        }
                    }
                }
                else
                {
                    oParticipant = GetParticipantFromDB();
                }

                WebSessionManager.SetPPTObj(oParticipant, _ClientType);
            }

            else
            {
                oParticipant = GetPartnerData(bFullPptInfo);
            }

            return oParticipant;

        }
        private ParticipantInfo GetParticipantFromDB()
        {
            ParticipantInfo oParticipant;

            if (_ClientType == 6 || WebSessionManager.IsPPTObjDirty(_SessionID))
            {
                oParticipant = _ParticipantDC.GetParticipantInfoFromDB();

                if (oParticipant.Errors[0].Number == 0)
                {
                    oParticipant.IsCacheValid = true;

                    if (_ClientType != 6)
                        WebSessionManager.SetPPTObj(oParticipant);
                }
            }
            else
            {
                oParticipant = WebSessionManager.GetPPTObj();
            }

            return oParticipant;
        }
        public SIResponse UpdatePersonalProfile(PersonalProfile value, bool bSendConfirmEmail = false)
        {
            SIResponse oSIResponse;
            int ConfID;

            oSIResponse = _ParticipantAdapter.UpdatePersonalProfile(_SessionID, value);
            if (oSIResponse.Errors[0].Number == 0 & oSIResponse.IsPending == false)
            {
                // insert confirmation
                ConfID = ParticipantDC.InsertConfirmation(oSIResponse.TransIDs);
                oSIResponse.ConfirmationNumber = ConfID.ToString();
                if (bSendConfirmEmail)
                {
                    string sLine;

                    if (value.eStmtPreference == "Y" || value.eConfirmPreference == "Y" || value.ReqdNoticesPreference == "Y")
                    {
                        sLine = "Statements will be sent via Electronic Mail";
                    }
                    else
                    {
                        sLine = "Statements will be sent via US Mail";
                    }
                    SendNotification(C_PARTICIPANT_ESTATEMENT_SIGNUP, null, "", oSIResponse.ConfirmationNumber, sLine);
                }
                Profile argoProfile = value;
                _ParticipantDC.UpdatePersonalProfile(ref argoProfile);
                value = (PersonalProfile)argoProfile;
                oSIResponse.Errors = value.Errors;
            }

            if (oSIResponse.Errors[0].Number == 0)
            {
                ParticipantInfo oPPT;
                oPPT = WebSessionManager.GetPPTObj();
                if (!(oPPT == null))
                {
                    value.BirthDt = oPPT.PersonalInfo.BirthDt;
                    oPPT.PersonalInfo = value;
                    WebSessionManager.SetPPTObj(oPPT);
                }
            }
            return oSIResponse;
        }
        private ParticipantInfo GetPartnerData(bool bFullPptInfo = true)
        {
            var oParticipant = default(ParticipantInfo);
            var bIsCacheValid = default(bool);
            SIResponse oSIResponse = null;
            TRS.IT.SI.BusinessFacadeLayer.Model.ErrorInfo[] oPartnerErrors;
            string sError = "";

            if (_SessionFlag == true)
            {
                try
                {
                    if (bFullPptInfo)
                    {
                        oParticipant = _ParticipantAdapter.GetParticipantInfo(_SessionID);
                    }
                    else
                    {
                        oParticipant = new ParticipantInfo();
                        oParticipant.PersonalInfo = _ParticipantAdapter.GetPersonalProfile(_SessionID);
                        // If _partnerID = PartnerFlag.ISC OrElse _partnerID = PartnerFlag.DIA Then
                        oParticipant.PlanInfo = new PlanInfo();
                        oParticipant.PlanInfo.ContractID = _ContractID;
                        oParticipant.PlanInfo.SubID = _SubID;
                        // End If
                    }


                    if (oParticipant.Errors[0].Number == 0)
                    {
                        CheckDates(ref oParticipant);

                        // UPdate with local DB info
                        oParticipant.SessionID = _SessionID;
                        // If _partnerID = PartnerFlag.ISC OrElse _partnerID = PartnerFlag.DIA Then
                        _ParticipantDC.GetParticipantLocalData(ref oParticipant, isISC: true);
                        oParticipant.TotalEmployeeRothBalance = 0d;
                        oParticipant.TotalRolloverRothBalance = 0d;
                        if (!(oParticipant.SourceInfo == null))
                        {
                            E_P3_SourceTypeC currentSourceTypeC;

                            foreach (var oSourceInfo in oParticipant.SourceInfo)
                            {
                                if (string.IsNullOrEmpty(oSourceInfo.SourceTypeC))
                                {
                                    continue;
                                }

                                // Set and Validate currentSourceTypeC to oSourceInfo.SourceTypeC
                                if (!Enum.TryParse(oSourceInfo.SourceTypeC, out currentSourceTypeC) || !Enum.IsDefined(typeof(E_P3_SourceTypeC), currentSourceTypeC))
                                {
                                    continue;
                                }

                                if (currentSourceTypeC == E_P3_SourceTypeC.Roth || currentSourceTypeC == E_P3_SourceTypeC.InPlanRothConversion)
                                {
                                    oParticipant.TotalEmployeeRothBalance += oSourceInfo.VestingBalance;
                                    oParticipant.AfterTaxHardshipAmt += oSourceInfo.AvailableHardshipBalance;
                                    oParticipant.AfterTaxInservice59Amt = oSourceInfo.AvailableInserviceBalance;
                                }
                                else if (currentSourceTypeC == E_P3_SourceTypeC.RothRollover || currentSourceTypeC == E_P3_SourceTypeC.QDRORoth)
                                {
                                    oParticipant.TotalRolloverRothBalance += oSourceInfo.VestingBalance;
                                    oParticipant.AfterTaxHardshipAmt += oSourceInfo.AvailableHardshipBalance;
                                    oParticipant.AfterTaxInservice59Amt = oSourceInfo.AvailableInserviceBalance;
                                }
                            }
                        }

                        // temp fix 6/4/2008 roth problem 
                        if (oParticipant.AfterTaxCurrentBal == 0d)
                        {
                            oParticipant.AfterTaxCurrentBal = oParticipant.TotalEmployeeRothBalance + oParticipant.TotalRolloverRothBalance;
                        }
                        bIsCacheValid = true;
                        _ParticipantDC.SetCacheFlag(false);

                        if (!string.IsNullOrEmpty(oParticipant.TransPending) & oParticipant.TransPending != "0")
                        {
                            switch (oParticipant.TransPendingName.Substring(0, Math.Min(8, oParticipant.TransPendingName.Length)).ToUpper() ?? "")
                            {
                                case "PERIODIC":
                                    {
                                        // There is a periodic pending transaction to be executed in the next production run
                                        _ParticipantDC.UpdatePeriodicPendingInfo(oParticipant.TransPending);
                                        break;
                                    }
                                case "WITHDRAW":
                                case "ONLINE ":
                                    {
                                        // There is a withdrawal pending transaction to be executed in the next production run
                                        _ParticipantDC.UpdatePeriodicPendingInfo(oParticipant.TransPending);
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        _ParticipantDC.SetCacheFlag(true);
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                    if (!(oParticipant == null))
                    {
                        sError = "   " + oParticipant.Errors[0].Description;
                    }

                    oParticipant = new ParticipantInfo();
                    oParticipant.Errors[0] = new TRS.IT.SI.BusinessFacadeLayer.Model.ErrorInfo();
                    oParticipant.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                    oParticipant.Errors[0].Description = ex.Message + Environment.NewLine + "Partner Unavailable!" + Environment.NewLine + "Participant::GetPartnerData" + sError;

                }

                if (oParticipant.Errors[0].Number != 0)
                {
                    Util.SendPartnerUnavailableMail(_SessionID, _partnerID, oParticipant.Errors[0].Description, oParticipant.Errors[0].Number.ToString());
                    if (oParticipant.Errors[0].Number != (int)ErrorCodes.NoOnlineAccessAvailable && oParticipant.Errors[0].Number != (int)ErrorCodes.MappingError)
                    {
                        oParticipant.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                        oParticipant.Errors[0].Description = "Partner Unavailable" + "   " + oParticipant.Errors[0].Description;
                    }
                }

                // get participant info from cache
                // Dim iPartnerError As Integer

                if (oParticipant.Errors[0].Number == (int)ErrorCodes.PartnerUnavailable | oParticipant.Errors[0].Number == (int)ErrorCodes.TimeoutError | oParticipant.Errors[0].Number == (int)ErrorCodes.NoOnlineAccessAvailable)
                {
                    oPartnerErrors = oParticipant.Errors;
                    oParticipant = GetParticipantFromDB();
                    if (oParticipant.Errors[0].Number != 0)
                    {
                        // check the partner errors
                        if (oPartnerErrors[0].Number != 0)
                        {
                            // set the partner error information
                            // oParticipant.Errors = oPartnerErrors
                            if (oPartnerErrors[0].Number == (int)ErrorCodes.NoOnlineAccessAvailable)
                            {
                                oParticipant.Errors = oPartnerErrors;
                            }
                        }
                        else if (!(oSIResponse == null) && oSIResponse.Errors[0].Number != 0)
                        {
                            // set the participant error
                            oParticipant.Errors = oSIResponse.Errors;
                        }
                    }
                }
                oParticipant.IsCacheValid = bIsCacheValid;
            }
            else
            {
                oParticipant = GetPartnerDataByInLoginID(bFullPptInfo);
                oParticipant.IsCacheValid = true;
            }
            return oParticipant;
        }

        private ParticipantInfo GetPartnerDataByInLoginID(bool bFullPptInfo = true)
        {
            ParticipantInfo oParticipant = null;
            try
            {
                if (bFullPptInfo)
                {
                    oParticipant = _ParticipantAdapter.GetParticipantInfo(_InLoginID, _ContractID, _SubID);
                }
                else
                {
                    oParticipant = new ParticipantInfo();
                    oParticipant.PersonalInfo = _ParticipantAdapter.GetPersonalProfile(_InLoginID, _ContractID, _SubID);
                }

                if (oParticipant.Errors[0].Number == 0)
                {
                    // update participant info object to database
                    _ParticipantDC.UpdateParticipantInfo(oParticipant);
                }

                if (oParticipant.Errors[0].Number == 0 | oParticipant.Errors[0].Number == (int)ErrorCodes.PartnerUnavailable)
                {
                    // get participant info from cache
                    oParticipant = GetParticipantFromDB();
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                if (oParticipant == null)
                {
                    oParticipant = new ParticipantInfo();
                    oParticipant.Errors[0].Number = (int)ErrorCodes.MappingError;
                    oParticipant.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error in GetPartnerDataByInLoginID", "ParticipantDC::UpdateParticipantInfo");
                }
                else
                {
                    oParticipant.Errors[0].Description = ex.Message + "  " + oParticipant.Errors[0].Description;
                }
            }

            return oParticipant;

        }

        private void CheckDates(ref ParticipantInfo oParticipantInfo)
        {
            if (!(oParticipantInfo.LoanInfo == null) && oParticipantInfo.LoanInfo.Length > 0)
            {
                foreach (var oLoanInfo in oParticipantInfo.LoanInfo)
                {
                    oLoanInfo.FirstPmtDt = GetCheckedDate(oLoanInfo.FirstPmtDt, "LoanInfo.FirstPmtDt");
                    oLoanInfo.LoanDt = GetCheckedDate(oLoanInfo.LoanDt, "LoanInfo.LoanDt");
                    oLoanInfo.MaturityDt = GetCheckedDate(oLoanInfo.MaturityDt, "LoanInfo.MaturityDt");
                }
            }
        }

        private string GetCheckedDate(string strDate, string dateName)
        {
            DateTime checkDate = DateTime.Parse("1/1/1753");
            string checkDateString = "01/01/1753";
            if (DateTime.TryParse(strDate, out DateTime parsedDate) && parsedDate < checkDate)
            {
                // send invalid date e-mail
                SendInvalidDateEmail(strDate, dateName);
                strDate = checkDateString;
            }
            return strDate;
        }

        private void SendInvalidDateEmail(string strDate, string dateName)
        {
            string fromEmail, toEmail, subject, body;
            try
            {
                fromEmail = AppSettings.GetValue("FromEmail");
                toEmail = AppSettings.GetValue("ToEmail");
                if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(toEmail))
                {
                    return;
                }
                subject = "Automated e-mail from BFL: Invalid date " + dateName + " found for session:" + _SessionID;
                body = "Session ID:" + _SessionID + Environment.NewLine + "Field:" + dateName + Environment.NewLine + "Date:" + strDate;
                body += Environment.NewLine + "Please take necessary action.";
                body += Environment.NewLine + "This is anu automated e-mail. Please do not respond to this message.";
                Util.SendMail(fromEmail, toEmail, subject, body, false, false);
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                // do nothing
            }
        }
        public SIResponse RequestConfirmationLetter(ConfirmationLetterInfo oConfirmationInfo)
        {
            SIResponse oSIResponse;
            int ConfID;

            if (_SessionFlag)
            {
                oSIResponse = _ParticipantAdapter.RequestConfirmationLetter(_SessionID, oConfirmationInfo);
            }
            else
            {
                oSIResponse = _ParticipantAdapter.RequestConfirmationLetter(_InLoginID, _ContractID, _SubID, oConfirmationInfo);
            }

            if (oSIResponse.Errors[0].Number == 0)
            {
                ConfID = ParticipantDC.InsertConfirmation(oSIResponse.TransIDs);

                if (General.GetConfirmationNumber(oSIResponse.ConfirmationNumber) == null)
                {
                    oSIResponse.ConfirmationNumber = ConfID.ToString();
                }
                else
                {
                    oSIResponse.ConfirmationNumber = General.GetConfirmationNumber(oSIResponse.ConfirmationNumber);
                }
            }
            else
            {
                oSIResponse.ConfirmationNumber = "";
            }
            return oSIResponse;
        }
        public void SendNotification(int iMsgID, LoanInfo oLoanInfo = null, string sDenialReason = "", string sConfirmationNo = "", string sDeliveryText = "")
        {
            ParticipantInfo oPartInfo = null;
            if (_ClientType != 9)
            {
                oPartInfo = GetParticipantInfoStayed();
            }

            var oResponse = new TRS.IT.SI.Services.wsNotification.TWS_Response();
            var oMsgData = new MessageData();
            oMsgData.ContractID = _SessionInfo.ContractID;
            oMsgData.SubID = _SessionInfo.SubID;
            oMsgData.MessageID = iMsgID;

            TRS.IT.SOA.Model.ContractInfo oContract;
            var objcontract = new Contract(_SessionInfo.ContractID, _SessionInfo.SubID);
            var oData = new TRS.IT.SOA.Model.AdditionalData();
            oData.Basic_Provisions_Required = true;
            oData.All_Provisions_Required = true;
            oData.Contacts_Required = false;

            oContract = objcontract.GetContractInformation(_SessionInfo.ContractID, _SessionInfo.SubID, oData);

            string sIsPassLoans = string.Empty;
            bool bIsPassLoans = false;

            if (!(oContract == null))
            {
                sIsPassLoans = Util.GetKeyValue("PassLoans", oContract.KeyValuePairs);
                if (sIsPassLoans == "1")
                {
                    bIsPassLoans = true;
                }
            }

            switch (iMsgID)
            {
                case 520:
                case 970:
                    {
                        string sName = "";
                        if (_SessionInfo.PartnerID == PartnerFlag.ISC)
                        {
                            sName = ParticipantDC.GetParticipantName(_InLoginID.ToString(), _ContractID, _SubID);
                        }
                        else
                        {
                            sName = Convert.ToString(oPartInfo.PersonalInfo.FirstName + (string.IsNullOrEmpty(oPartInfo.PersonalInfo.MiddleInitial.Trim()) ? "" : " " + oPartInfo.PersonalInfo.MiddleInitial) + " " + oPartInfo.PersonalInfo.LastName);
                        }
                        oMsgData.EmailVariableContainer.Add(E_Variable.USER_NAME, sName);
                        break;
                    }

                case 550:
                case 5500:
                    {
                        string sLnDocText = string.Empty;
                        if (iMsgID == 5500)
                        {
                            sLnDocText = "The loan check will be sent to you within 2 business days of this notice. The participant can access the amortization schedule along with other loan documents in the Loans section of his/her account on www.TA-Retirement.com.";
                        }
                        else
                        {
                            sLnDocText = "The loan check will be sent to you within 2 business days of this notice. Your TPA will provide you with the amortization schedule and other loan documents for this participant.";
                        }
                        oMsgData.MessageID = 550;
                        oMsgData.EmailVariableContainer.Add(E_Variable.TRANSACTION_DATE, DateTime.Now.ToShortDateString());
                        oMsgData.EmailVariableContainer.Add(E_Variable.LOAN_PAYMENT_AMOUNT, oLoanInfo.PmtAmt.ToString("c"));
                        oMsgData.EmailVariableContainer.Add(E_Variable.LOAN_PAYMENTS_COUNT, oLoanInfo.NumberOfPayments);
                        oMsgData.EmailVariableContainer.Add(E_Variable.LOAN_PAYMENT_FREQUENCY, PaymentFrequency(oLoanInfo.PaymentFrequency));
                        oMsgData.EmailVariableContainer.Add(E_Variable.USER_NAME, oPartInfo.PersonalInfo.FirstName + " " + oPartInfo.PersonalInfo.LastName);
                        oMsgData.EmailVariableContainer.Add(E_Variable.LOAN_DOCUMENTS_TEXT, sLnDocText);
                        break;
                    }
                case 600:
                    {
                        oMsgData.SSN = oPartInfo.PersonalInfo.SSN;
                        oMsgData.EmailVariableContainer.Add("loan_amount", oLoanInfo.LoanAmt.ToString("c"));
                        oMsgData.EmailVariableContainer.Add(E_Variable.TRANSACTION_DATE, DateTime.Now.ToShortDateString());
                        oMsgData.EmailVariableContainer.Add(E_Variable.LOAN_PAYMENT_AMOUNT, oLoanInfo.PmtAmt.ToString("c"));
                        oMsgData.EmailVariableContainer.Add(E_Variable.LOAN_PAYMENTS_COUNT, oLoanInfo.NumberOfPayments);
                        oMsgData.EmailVariableContainer.Add(E_Variable.LOAN_PAYMENT_FREQUENCY, PaymentFrequency(oLoanInfo.PaymentFrequency));
                        oMsgData.EmailVariableContainer.Add(E_Variable.PARTICIPANT_NAME_MESSAGECENTER, _SessionInfo.InLoginID + "|" + oPartInfo.PersonalInfo.FirstName + " " + oPartInfo.PersonalInfo.LastName);
                        if (!string.IsNullOrEmpty(oPartInfo.PersonalInfo.Email) && TRS.IT.TRSManagers.ValidationManager.IsValidEmailAddress(oPartInfo.PersonalInfo.Email))
                        {
                            oMsgData.EmailVariableContainer.Add(E_Variable.PARTICIPANT_NAME, oPartInfo.PersonalInfo.Email);
                        }
                        else
                        {
                            oMsgData.EmailVariableContainer.Add(E_Variable.PARTICIPANT_NAME, "Subbaraju.Pakalapati@transamerica.com");
                        }

                        break;
                    }
                case 590:    // 12.5.3.0.4 - Loan Request Notice - Denied(participant)
                    {
                        oMsgData.SSN = oPartInfo.PersonalInfo.SSN;
                        oMsgData.EmailVariableContainer.Add(E_Variable.REASON_1, sDenialReason);

                        oMsgData.Override = false;
                        if (!string.IsNullOrEmpty(oPartInfo.PersonalInfo.Email) && TRS.IT.TRSManagers.ValidationManager.IsValidEmailAddress(oPartInfo.PersonalInfo.Email))
                        {
                            oMsgData.EmailVariableContainer.Add(E_Variable.PARTICIPANT_NAME, oPartInfo.PersonalInfo.Email);
                        }
                        else
                        {
                            oMsgData.EmailVariableContainer.Add(E_Variable.PARTICIPANT_NAME, "Subbaraju.Pakalapati@transamerica.com");
                        }
                        if (bIsPassLoans == true)
                        {
                            oMsgData.EmailVariableContainer.Add("contact_to_txt", "your Plan Administrator");
                        }
                        else
                        {
                            oMsgData.EmailVariableContainer.Add("contact_to_txt", "your Plan Administrator");
                        }

                        break;
                    }
                case 980: // authorized_signer_contract only for sub contract 000
                    {
                        oMsgData.SubID = "000";

                        string sPptName = "";
                        if (!(oPartInfo == null) && !(oPartInfo.PersonalInfo == null))
                        {
                            sPptName = Convert.ToString(oPartInfo.PersonalInfo.FirstName + (string.IsNullOrEmpty(oPartInfo.PersonalInfo.MiddleInitial.Trim()) ? "" : " " + oPartInfo.PersonalInfo.MiddleInitial) + " " + oPartInfo.PersonalInfo.LastName);
                        }
                        sPptName = sPptName.Trim();

                        if (string.IsNullOrEmpty(sPptName))
                        {
                            sPptName = ParticipantDC.GetParticipantName(_InLoginID.ToString(), _ContractID, _SubID);
                        }

                        oMsgData.EmailVariableContainer.Add(E_Variable.USER_NAME, sPptName); // IT -95606
                        break;
                    }

                case 1460:
                    {
                        oMsgData.EmailVariableContainer.Add(E_Variable.USER_NAME, oPartInfo.PersonalInfo.FirstName + " " + oPartInfo.PersonalInfo.LastName);

                        if (bIsPassLoans == true)
                        {
                            oMsgData.EmailVariableContainer.Add("pass_loanservices_txt", "Your PASS Administrator will process the loan and mail the proceeds of the loan directly to your participant.");
                        }
                        else
                        {
                            oMsgData.EmailVariableContainer.Add("pass_loanservices_txt", "");
                        }

                        break;
                    }
                case 1930:   // eStatement signup email notification
                    {
                        oMsgData.SSN = oPartInfo.PersonalInfo.SSN;
                        if (!string.IsNullOrEmpty(oPartInfo.PersonalInfo.Email) && TRS.IT.TRSManagers.ValidationManager.IsValidEmailAddress(oPartInfo.PersonalInfo.Email))
                        {
                            oMsgData.EmailVariableContainer.Add(E_Variable.PARTICIPANT_NAME, oPartInfo.PersonalInfo.Email);
                        }
                        else
                        {
                            oMsgData.EmailVariableContainer.Add(E_Variable.PARTICIPANT_NAME, "damanjit.singh@transamerica.com");
                        }
                        oMsgData.EmailVariableContainer.Add(E_Variable.USER_NAME, oPartInfo.PersonalInfo.FirstName + " " + oPartInfo.PersonalInfo.LastName);
                        oMsgData.EmailVariableContainer.Add("conf_number", sConfirmationNo);
                        oMsgData.EmailVariableContainer.Add("delivery_text", sDeliveryText);
                        oMsgData.EmailVariableContainer.Add("contact_number", "1-800-401-TRAN (8726)");
                        break;
                    }


            }
            oResponse = MessageService.MessageServiceSendEmail(oMsgData);
            if (oResponse.Errors[0].Number != 0)
            {
                var msg = new MimeMessage();

                msg.From.Add(new MailboxAddress("", AppSettings.GetValue("FromEmail")));
                msg.To.Add(new MailboxAddress("", "Subbaraju.Pakalapati@transamerica.com"));
                msg.Cc.Add(new MailboxAddress("", "Damanjit.Singh@transamerica.com"));
                msg.Subject = "ATTENTION REQUIRED – Distribution Notification Failed on " + DateTime.Now.ToShortDateString();

                var sbReportData = new System.Text.StringBuilder();
                string openTD = "<td>";
                string closeTD = "</td>";

                string sIntro = string.Empty;
                string sNotifSubject = string.Empty;
                if (iMsgID == 980)    // distribution notification. Send email 
                {
                    if (!(AppSettings.GetValue("BizUserEmailList") == null))
                    {
                        string sBizUserEmailList = AppSettings.GetValue("BizUserEmailList");
                        if (sBizUserEmailList.Length > 0)
                        {
                            Util.SplitEmailsToMAC(msg.To, sBizUserEmailList);
                        }
                    }
                    sIntro = "<tr><td>" + "Encountered an error when trying to send out Distribution Request Notification for the following contracts. <br/>" + "Please review these contracts and make sure Authorized Signer contact exists with a valid recipient email " + "addresses.<br/>" + "</td></tr>";
                }
                else
                {
                    sIntro = "<tr><td>" + "Encountered an error when trying to send out Loan Request Notification for the following contracts. <br/></td></tr>";
                }
                string sHead = "<tr style='background-color:#CCCCCC'><td>ID</td><td>Subject</td><td>Contract/Sub ID</td><td>Ppt InloginID</td><td>Errors</td><td>App Sending Message</td></tr>";
                sbReportData.Append("<table>");
                sbReportData.Append(sIntro);
                sbReportData.Append("</table><br><table border='1' cellpadding='2' cellspacing='2'>");
                sbReportData.Append(sHead);
                sbReportData.Append("<tr valign='top'>");
                sbReportData.Append(openTD + iMsgID + closeTD);
                sbReportData.Append(openTD + "Distribution Request" + closeTD);
                sbReportData.Append(openTD + _SessionInfo.ContractID + "-" + _SessionInfo.SubID + closeTD);
                sbReportData.Append(openTD + _SessionInfo.InLoginID + closeTD);
                sbReportData.Append(openTD + oResponse.Errors[0].Description + closeTD);
                sbReportData.Append(openTD + "TA-Retirement.com" + closeTD);
                sbReportData.Append("</tr>");
                sbReportData.Append("</table>");

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = sbReportData.ToString();
                msg.Body = bodyBuilder.ToMessageBody();

                TRS.IT.TRSManagers.MailManager.SendEmail(msg);
            }

        }
        private string PaymentFrequency(LoanPaymentFrequency frequency)
        {
            if (frequency.ToString().Trim().ToLower() == "every2weeks")
            {
                return "Every 2 Weeks";
            }
            else if (frequency.ToString().Trim().ToLower() == "twiceamonth")
            {
                return "Twice A Month";
            }
            else
            {
                return frequency.ToString();
            }
        }

    }
}
