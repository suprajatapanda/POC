using System.Runtime.InteropServices;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer.DAL;
using TRS.IT.SI.BusinessFacadeLayer.Model;
using TRS.IT.TAEMQCon;

namespace TRS.IT.SI.BusinessFacadeLayer.Adapters
{
    public class ParticipantAdapter : IParticipantAdapter
    {
        private bool _TransHistory = false;
        private const int C_GET_PARTICIPANT_INFO = 49;
        private const int C_UNLOCK_ACCOUNT = 56;
        private const int C_WthByMoneyType = 101;

        internal const string C_MQ_WthByMoneyType = "09300011";
        public PersonalProfile GetPersonalProfile(int InLoginID, string ContractID, string SubID)
        {
            throw new Exception("Not implemented");
        }
        public PersonalProfile GetPersonalProfile(string sessionID)
        {
            throw new Exception("Not implemented");
        }

        public ParticipantInfo GetParticipantInfo(string sessionID)
        {
            ParticipantInfo oParticipantObject;
            var oConverter = new Converter();
            string errorCode = string.Empty;
            string request = string.Empty;
            try
            {
                request = Converter.Format6000Message(sessionID, !_TransHistory);
                oParticipantObject = GetParticipantInfo(sessionID, request, _TransHistory, ref errorCode, true);
            }
            catch (IBM.WMQ.MQException mqEx)
            {
                Logger.LogMessage(mqEx.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oParticipantObject = new ParticipantInfo();
                oParticipantObject.Errors[0].Number = (int)ErrorCodes.MQException;
                oParticipantObject.Errors[0].Description = General.FormatErrorMsg(mqEx.Message, ErrorMessages.PartnerUnavailable, "ParticipantAdapter::GetParticipantInfo");
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oParticipantObject = new ParticipantInfo();
                oParticipantObject.Errors[0].Number = (int)ErrorCodes.Unknown;
                oParticipantObject.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error", "ParticipantAdapter::GetParticipantInfo");
            }
            if (oParticipantObject.Errors[0].Number == 0)
            {
                AudienceDC.UpdateTransactionStatus(sessionID, PartnerFlag.TAE, (TransactionType)C_GET_PARTICIPANT_INFO, TransactionStatus.Success, request, errorCode, null);
            }
            else
            {
                // transaction failed
                if (oParticipantObject.Errors[0].Description.Contains("Mapping not found"))
                {
                    errorCode = "4";
                }
                AudienceDC.UpdateTransactionStatus(sessionID, PartnerFlag.TAE, (TransactionType)C_GET_PARTICIPANT_INFO, TransactionStatus.Failed, request, errorCode, null);
            }
            return oParticipantObject;
        }
        public ParticipantInfo GetParticipantInfo(int InLoginID, string ContractID, string SubID)
        {
            var mqCon = new MQConnection();
            string response;
            string sRequest6007;

            // get member id and plan id
            string memberID = string.Empty;
            string planID = string.Empty;

            // *** Debug Only ***
            ParticipantInfo oParticipantObject;
            var oConverter = new Converter();

            try
            {
                ParticipantDC.GetPartner(InLoginID, ContractID, SubID, ref planID, ref memberID);
                response = mqCon.SubmitTransaction(Converter.Format6000Message(planID, memberID, "", SubID), C_GET_PARTICIPANT_INFO);
                if (response.Substring(0, 3) != "000")
                {
                    UnlockAccount(planID, memberID);
                    // re-send the request
                    response = mqCon.SubmitTransaction(Converter.Format6000Message(planID, memberID, "", SubID), C_GET_PARTICIPANT_INFO);
                }
                if (response.Substring(0, 3) == "000")
                {
                    oParticipantObject = oConverter.ConvertToParticipantInfo(InLoginID, ContractID, SubID, response);
                    sRequest6007 = Converter.Format6007Message(memberID, planID, C_MQ_WthByMoneyType, SubID);
                    response = mqCon.SubmitTransaction(sRequest6007, C_WthByMoneyType);
                    if (response.Substring(0, 3) == "000")
                    {
                        oConverter.WthAvailableAmtByMoneyType(oParticipantObject, response);
                    }
                }

                else
                {
                    oParticipantObject = new ParticipantInfo();
                    string argErrorCode = null;
                    oParticipantObject.Errors[0] = Converter.GetTAEErrorInfo(response, false, ErrorCode: ref argErrorCode);
                }
            }
            catch (IBM.WMQ.MQException mqEx)
            {
                Logger.LogMessage(mqEx.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oParticipantObject = new ParticipantInfo();
                oParticipantObject.Errors[0].Number = (int)ErrorCodes.MQException;
                oParticipantObject.Errors[0].Description = General.FormatErrorMsg(mqEx.Message, ErrorMessages.PartnerUnavailable, "ParticipantAdapter::GetParticipantInfo");
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oParticipantObject = new ParticipantInfo();
                oParticipantObject.Errors[0].Number = (int)ErrorCodes.Unknown;
                oParticipantObject.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error", "ParticipantAdapter::GetParticipantInfo");
            }

            return oParticipantObject;
        }
        private ParticipantInfo GetParticipantInfo(string sessionID, string request, bool transHistory, ref string errorCode, bool reSubmitOnError = true)
        {
            var mqCon = new MQConnection();
            string response;
            string sRequest6007 = Converter.Format6007Message(sessionID, C_MQ_WthByMoneyType);
            ParticipantInfo oParticipantObject;
            var oConverter = new Converter();
            SessionInfo oSessionInfo;
            oSessionInfo = AudienceDC.GetSessionInfo(sessionID);
            response = mqCon.SubmitTransaction(request, C_GET_PARTICIPANT_INFO);
            if (response.Substring(0, 3) == "000")
            {
                // ignore transaction history if not required
                oConverter.IgnoreTransHistory = !transHistory;
                oParticipantObject = oConverter.ConvertToParticipantInfo(oSessionInfo.InLoginID, oSessionInfo.ContractID, oSessionInfo.SubID, response);
                if (oParticipantObject.Errors[0].Number == (int)ErrorCodes.IncompleteResponse)
                {
                    errorCode = "RES_ERROR";
                }
                response = mqCon.SubmitTransaction(sRequest6007, C_WthByMoneyType);
                if (response.Substring(0, 3) == "000")
                {
                    oConverter.WthAvailableAmtByMoneyType(oParticipantObject, response);
                }
            }
            else
            {
                oParticipantObject = new ParticipantInfo();
                oParticipantObject.Errors[0] = Converter.GetTAEErrorInfo(response, false, ref errorCode);
            }
            if (oParticipantObject.Errors[0].Number == (int)ErrorCodes.IncompleteResponse && reSubmitOnError == true)
            {
                errorCode = string.Empty;
                oParticipantObject = GetParticipantInfo(sessionID, request, transHistory, ref errorCode, false); // recurrsion
            }
            if (oParticipantObject.Errors[0].Number == 0)
            {
                if (oParticipantObject.TransPending == "W")
                {
                    oParticipantObject.TransPendingName = "Withdrawal Request";
                }
                else
                {
                    switch (oParticipantObject.PeriodicIndicator)
                    {
                        case 3:
                            {
                                oParticipantObject.TransPendingName = "Periodic Rebalance";
                                oParticipantObject.TransPending = "R";
                                break;
                            }
                        case 4:
                            {
                                oParticipantObject.TransPendingName = "Periodic Fund Transfer";
                                oParticipantObject.TransPending = "T";
                                break;
                            }
                    }
                }
            }

            // End If
            return oParticipantObject;
        }
        public SIResponse UpdatePersonalProfile(string sessionID, PersonalProfile profile)
        {
            var oMQCon = new MQConnection();
            var oSIResponse = new SIResponse();
            string mqRequest = string.Empty;
            string mqParticipantAdditionalData;
            int trans_id;
            var dc = new ParticipantDC(sessionID);
            string ErrorCode = string.Empty;
            try
            {
                mqParticipantAdditionalData = oMQCon.SubmitTransaction(Converter.Format6007Message(sessionID, "004G"), (int)TransactionType.ViewParticipantProfile);
                if (mqParticipantAdditionalData.Substring(0, 3) == "000")
                {
                    // this is a right response
                    // strip first 9 bytes of the response
                    mqParticipantAdditionalData = mqParticipantAdditionalData.Substring(9);
                    // build mq request for the participant
                    mqRequest = Converter.FormatUpdateProfileMessage(sessionID, mqParticipantAdditionalData, profile);
                }
                else
                {
                    oSIResponse.Errors[0].Number = (int)ErrorCodes.Unknown;
                    oSIResponse.Errors[0].Description = General.FormatErrorMsg("Error getting participant data", "Partner Error", "ParticipantAdapter::UpdatePersonalProfile");
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oSIResponse.Errors[0].Number = (int)ErrorCodes.MappingError;
                oSIResponse.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Error", "ParticipantAdapter::UpdatePersonalProfile");
            }
            if (oSIResponse.Errors[0].Number == 0)
            {
                string argmqReturnResponse = null;
                oSIResponse = SubmitTransaction(mqRequest, "Error in transaction", "ParticipantAdapter::UpdatePersonalProfile", ref argmqReturnResponse, ref ErrorCode, (int)TransactionType.UpdateProfile);
            }

            // update confirmations

            if (oSIResponse.Errors[0].Number == 0)
            {
                trans_id = dc.UpdateTransactionStatus(TransactionType.UpdateProfile, TransactionStatus.Success, mqRequest, ErrorCode, (int)PartnerFlag.TAE, oSIResponse.ConfirmationNumber);
            }
            else if (oSIResponse.IsPending == true)
            {
                // transaction is pending - partner unavailable
                oSIResponse.Errors[0].Number = 0;
                oSIResponse.Errors[0].Description = "";
                trans_id = dc.UpdateTransactionStatus(TransactionType.UpdateProfile, TransactionStatus.Pending, mqRequest, ErrorCode, (int)PartnerFlag.TAE);
            }
            else
            {
                trans_id = dc.UpdateTransactionStatus(TransactionType.UpdateProfile, TransactionStatus.Failed, mqRequest, ErrorCode, (int)PartnerFlag.TAE);
            }
            oSIResponse.ConfirmationNumber = trans_id.ToString();
            oSIResponse.TransIDs = [trans_id];
            return oSIResponse;
        }

        private SIResponse SubmitTransaction(string mqRequest, string customError, string functionName, [Optional, DefaultParameterValue("")] ref string mqReturnResponse, [Optional] ref string ErrorCode, int transTypeID = 0)
        {
            var oSIResponse = new SIResponse();
            var oCon = new MQConnection();
            string mqResponse = string.Empty;
            try
            {
                mqResponse = oCon.SubmitTransaction(mqRequest, transTypeID);
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oSIResponse.Errors[0].Number = (int)ErrorCodes.Unknown;
                oSIResponse.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Partner Unavailable", "ParticipantAdapter::" + functionName);
                oSIResponse.IsPending = true;
            }
            // check if the response is valid or not
            if (oSIResponse.Errors[0].Number == 0)
            {
                oSIResponse = Converter.ParseMQResponse(customError, "ParticipantAdapter::" + functionName, mqResponse, ref ErrorCode);
                mqReturnResponse = mqResponse;
            }

            return oSIResponse;
        }

        internal static bool UnlockAccount(string planID, string memberID)
        {
            string mqRequest;
            string mqResponse;
            mqRequest = "9999" + DefaultSettings.BANKCODE() + DefaultSettings.CID() + planID + memberID.PadRight(10) + DefaultSettings.FPID(planID) + DefaultSettings.USERID() + "/";
            var oMQCon = new MQConnection();
            try
            {
                mqResponse = oMQCon.SubmitTransaction(mqRequest, C_UNLOCK_ACCOUNT);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                return false;
            }
        }

        public SIResponse RequestConfirmationLetter(int InLoginID, string ContractID, string SubID, ConfirmationLetterInfo oConfirmationInfo)
        {
            var oSIResponse = new SIResponse();
            var sbRequest = new System.Text.StringBuilder();
            int trans_id;
            string mqResponse = string.Empty;
            string errorCode = string.Empty;
            string memberID = string.Empty;
            string planID = string.Empty;
            var dc = new ParticipantDC(InLoginID, ContractID, SubID);
            try
            {

                ParticipantDC.GetPartner(InLoginID, ContractID, SubID, ref planID, ref memberID);

                sbRequest.Append(Converter.Format6007Message(memberID, planID, "09300007", SubID));
                sbRequest.Append(Converter.FormatConfirmationLetterMessage(oConfirmationInfo));
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oSIResponse.Errors[0].Number = (int)ErrorCodes.MappingError;
                oSIResponse.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Mapping Error", "ParticipantAdapter::RequestConfirmationLetter");
            }
            if (oSIResponse.Errors[0].Number == 0)
            {
                var mqCon = new MQConnection();

                try
                {
                    mqResponse = mqCon.SubmitTransaction(sbRequest.ToString(), (int)TransactionType.RequestConfirmationLetter);
                    if (Converter.IsAccountLocked(mqResponse))
                    {
                        UnlockAccount(planID, memberID);
                        // re-send the request
                        mqResponse = mqCon.SubmitTransaction(sbRequest.ToString(), (int)TransactionType.RequestConfirmationLetter);
                    }

                    if (mqResponse.Substring(0, 3) == "000")
                    {
                        oSIResponse.Errors[0].Number = 0;
                    }
                    else
                    {
                        oSIResponse.Errors[0].Number = Convert.ToInt32(mqResponse.Substring(0, 3));
                        errorCode = mqResponse.Substring(0, 3);
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    Logger.LogMessage(mqEx.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                    oSIResponse.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                    oSIResponse.Errors[0].Description = General.FormatErrorMsg(mqEx.Message, ErrorMessages.PartnerUnavailable, "ParticipantAdapter::RequestConfirmationLetter");
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                    oSIResponse.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                    oSIResponse.Errors[0].Description = General.FormatErrorMsg(ex.Message, ErrorMessages.PartnerUnavailable, "ParticipantAdapter::RequestConfirmationLetter");
                }

                // oSIResponse = SubmitTransaction(sbRequest.ToString, "Error", "ParticipantAdapter::RequestConfirmationLetter", mqResponse, errorCode, Model.TransactionType.RequestConfirmationLetter)
            }

            if (oSIResponse.Errors[0].Number == 0)
            {
                trans_id = dc.UpdateTransactionStatus(TransactionType.RequestConfirmationLetter, TransactionStatus.Success, sbRequest.ToString(), errorCode, (int)PartnerFlag.TAE, oSIResponse.ConfirmationNumber);
                oSIResponse.ConfirmationNumber = General.JoinConfirmationNumber(oSIResponse.ConfirmationNumber, trans_id);
                oSIResponse.TransIDs = [trans_id];
            }
            else if (oSIResponse.IsPending == true)
            {
                oSIResponse.Errors[0].Number = 0;
                oSIResponse.Errors[0].Description = "";
                trans_id = dc.UpdateTransactionStatus(TransactionType.RequestConfirmationLetter, TransactionStatus.Pending, sbRequest.ToString());
                oSIResponse.ConfirmationNumber = trans_id.ToString();
                oSIResponse.TransIDs = [trans_id];
            }
            else
            {
                trans_id = dc.UpdateTransactionStatus(TransactionType.RequestConfirmationLetter, TransactionStatus.Failed, sbRequest.ToString(), errorCode, (int)PartnerFlag.TAE, null);
                oSIResponse.ConfirmationNumber = trans_id.ToString();
            }

            return oSIResponse;

        }
        public SIResponse RequestConfirmationLetter(string sessionID, ConfirmationLetterInfo oConfirmationInfo)
        {
            var oSIResponse = new SIResponse();
            // Dim sbRequest As New System.Text.StringBuilder
            var dc = new ParticipantDC(sessionID);
            int trans_id;
            string mqResponse = string.Empty;
            string errorCode = string.Empty;
            var sbRequest = new System.Text.StringBuilder();
            try
            {
                sbRequest.Append(Converter.Format6007Message(sessionID, "09300007"));
                sbRequest.Append(Converter.FormatConfirmationLetterMessage(oConfirmationInfo));
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oSIResponse.Errors[0].Number = (int)ErrorCodes.MappingError;
                oSIResponse.Errors[0].Description = General.FormatErrorMsg(ex.Message, "Mapping Error", "ParticipantAdapter::RequestConfirmationLetter");
            }
            if (oSIResponse.Errors[0].Number == 0)
            {
                var mqCon = new MQConnection();

                try
                {
                    mqResponse = mqCon.SubmitTransaction(sbRequest.ToString(), (int)TransactionType.RequestConfirmationLetter);
                    if (mqResponse.Substring(0, 3) == "000")
                    {
                        oSIResponse.Errors[0].Number = 0;
                    }
                    else
                    {
                        oSIResponse.Errors[0].Number = Convert.ToInt32(mqResponse.Substring(0, 3));
                        errorCode = mqResponse.Substring(0, 3);
                    }
                }
                catch (IBM.WMQ.MQException mqEx)
                {
                    Logger.LogMessage(mqEx.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                    oSIResponse.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                    oSIResponse.Errors[0].Description = General.FormatErrorMsg(mqEx.Message, ErrorMessages.PartnerUnavailable, "ParticipantAdapter::RequestConfirmationLetter");
                }
                catch (Exception ex)
                {
                    Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                    oSIResponse.Errors[0].Number = (int)ErrorCodes.PartnerUnavailable;
                    oSIResponse.Errors[0].Description = General.FormatErrorMsg(ex.Message, ErrorMessages.PartnerUnavailable, "ParticipantAdapter::RequestConfirmationLetter");
                }
                // oSIResponse = SubmitTransaction(sbRequest.ToString, "Error", "ParticipantAdapter::RequestConfirmationLetter", mqResponse, errorCode, Model.TransactionType.RequestConfirmationLetter)
            }

            if (oSIResponse.Errors[0].Number == 0)
            {
                trans_id = dc.UpdateTransactionStatus(TransactionType.RequestPinLetter, TransactionStatus.Success, sbRequest.ToString(), errorCode, (int)PartnerFlag.TAE, oSIResponse.ConfirmationNumber);
                oSIResponse.ConfirmationNumber = General.JoinConfirmationNumber(oSIResponse.ConfirmationNumber, trans_id);
                oSIResponse.TransIDs = [trans_id];
            }
            else if (oSIResponse.IsPending == true)
            {
                oSIResponse.Errors[0].Number = 0;
                oSIResponse.Errors[0].Description = "";
                trans_id = dc.UpdateTransactionStatus(TransactionType.RequestConfirmationLetter, TransactionStatus.Pending, sbRequest.ToString());
                oSIResponse.ConfirmationNumber = trans_id.ToString();
                oSIResponse.TransIDs = [trans_id];
            }
            else
            {
                trans_id = dc.UpdateTransactionStatus(TransactionType.RequestConfirmationLetter, TransactionStatus.Failed, sbRequest.ToString(), errorCode, (int)PartnerFlag.TAE, null);
                oSIResponse.ConfirmationNumber = trans_id.ToString();
            }
            return oSIResponse;

        }
        public ParticipantWithdrawalsInfo GetDistributionInfo(string sessionID, string contractId, string subId, string ssn_no)
        {
            throw new Exception("Not implemented");
        }

    }
}