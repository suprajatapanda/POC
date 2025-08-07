using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using wsMS = TRS.IT.SI.Services.wsNotification;

namespace TRS.IT.BendProcessor.DriverSOA
{
    public class MessageService
    {
        wsMS.MessageService _wsMS;
        public MessageService()
        {
            _wsMS = new wsMS.MessageService(TrsAppSettings.AppSettings.GetValue("NotificationWebServiceURL"));
        }

        public ResultReturn SendPayrollNotification(string contractId, string subId, int templateId, MessageServiceKeyValue[] keys)
        {
            return SendMessage(contractId, subId, templateId, keys, "BendProcessor/PayrollNotification");
        }

        public ResultReturn SendMessage(string contractId, string subId, int templateId, MessageServiceKeyValue[] keys, string sourceName = "BendProcessor/PayrollNotification", wsMS.MessageAttachment[] attachments = null)
        {
            ResultReturn oReturn = new();

            try
            {
                var oMsg = CreateMessage(templateId, keys, sourceName, attachments);
                oMsg.Keys.ContractID = contractId;
                oMsg.Keys.SubID = subId;

                var oResponse = _wsMS.SendMessage(oMsg);
                ProcessResponse(oResponse, oReturn);
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.Errors.Add(new ErrorInfo(-1, "ex: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
                oReturn.returnStatus = ReturnStatusEnum.Failed;
            }

            return oReturn;
        }

        public ResultReturn SendMessage_NoContract(int templateId, MessageServiceKeyValue[] keys, string sourceName, wsMS.MessageAttachment[] attachments = null)
        {
            ResultReturn oReturn = new();

            try
            {
                var oMsg = CreateMessage(templateId, keys, sourceName, attachments);
                oMsg.Keys.Image = wsMS.E_ImageOption.NoImaging;

                var oResponse = _wsMS.SendMessage(oMsg);
                ProcessResponse(oResponse, oReturn);
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.Errors.Add(new ErrorInfo(-1, "ex: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
                oReturn.returnStatus = ReturnStatusEnum.Failed;
            }

            return oReturn;
        }

        private wsMS.Message CreateMessage(int templateId, MessageServiceKeyValue[] keys, string sourceName, wsMS.MessageAttachment[] attachments)
        {
            var oMsgData = new wsMS.KeyValue[keys.GetLength(0)];
            for (int i = 0; i < keys.GetLength(0); i++)
            {
                oMsgData[i] = new wsMS.KeyValue
                {
                    key = keys[i].key,
                    value = keys[i].value
                };
            }

            var oMsg = new wsMS.Message
            {
                Properties = new wsMS.MessageProperties
                {
                    MessageID = templateId,
                    SourceName = sourceName
                },
                Keys = new wsMS.MessageKeys(),
                Data = oMsgData
            };

            if (attachments != null)
            {
                oMsg.Attachments = attachments;
            }

            return oMsg;
        }

        private void ProcessResponse(wsMS.TWS_Response response, ResultReturn returnResult)
        {
            if (response.Errors[0].Number == 0)
            {
                returnResult.returnStatus = ReturnStatusEnum.Succeeded;
            }
            else
            {
                returnResult.returnStatus = ReturnStatusEnum.Failed;
                returnResult.Errors.Add(new ErrorInfo(response.Errors[0].Number, response.Errors[0].Description, ErrorSeverityEnum.Error));
            }
        }

        
    }
}