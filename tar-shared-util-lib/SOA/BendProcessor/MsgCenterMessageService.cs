using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;

namespace TRS.IT.BendProcessor.DriverSOA
{
    public class MsgCenterMessageService
    {
        private SI.Services.MessageService _wsTRSMS;

        public MsgCenterMessageService()
        {
            _wsTRSMS = new SI.Services.MessageService(TrsAppSettings.AppSettings.GetValue("MessageWebServiceURL"));
        }

        public ResultReturn SendMessageCenterMessage(string loginXml, SI.Services.wsMessage.webMessage mcMsg)
        {
            ResultReturn oReturn = new();
            try
            {
                var oResponse = _wsTRSMS.SendWebMessages(loginXml, mcMsg);
                if (oResponse.Errors[0].Number == 0)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    oReturn.Errors.Add(new ErrorInfo(oResponse.Errors[0].Number, oResponse.Errors[0].Description, ErrorSeverityEnum.Error));
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
                oReturn.returnStatus = ReturnStatusEnum.Failed;
            }
            return oReturn;
        }
    }
}