using wsNotificaton = TRS.IT.SI.Services.wsNotification;

namespace TRS.IT.SI.BusinessFacadeLayer
{
    public class MessageService
    {
        public static wsNotificaton.TWS_Response MessageServiceSendEmail(Model.MessageData objMessageData)
        {
            objMessageData.ContractID = objMessageData.ContractID.Trim();
            objMessageData.SubID = BusinessFacadeLayer.Util.SubIn(objMessageData.SubID);
            return new MsgSrv_Adapter.MessageSrvAdapter().SendMessage(objMessageData);
        }
    }
}