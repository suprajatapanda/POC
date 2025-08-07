namespace TRS.IT.SI.BusinessFacadeLayer.SOA
{
    public class MessageSOA
    {
        private Services.MessageService _wsMessage;

        public MessageSOA()
        {
            _wsMessage = new Services.MessageService(TrsAppSettings.AppSettings.GetValue("MessageWebServiceURL"));
        }
        public Services.wsMessage.TWS_Response SendWebMessages(string loginXml, Services.wsMessage.webMessage webMsg, string contractId, string subId)
        {
            return _wsMessage.SendWebMessages(loginXml, webMsg, contractId, subId);
        }

    }
}