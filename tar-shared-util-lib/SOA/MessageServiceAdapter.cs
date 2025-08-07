using System.Collections;
using Notification = TRS.IT.SI.Services.wsNotification;

namespace TRS.IT.SI.BusinessFacadeLayer.MsgSrv_Adapter
{
    public class MessageSrvAdapter
    {
        Notification.MessageService _objMessageSrv;
        public MessageSrvAdapter()
        {
            _objMessageSrv = new Notification.MessageService(TrsAppSettings.AppSettings.GetValue("NotificationWebServiceURL"));
        }

        public Notification.TWS_Response SendMessage(Model.MessageData objMessageInfo)
        {
            var objMessage = new Notification.Message
            {
                Properties = new Notification.MessageProperties(),
                Keys = new Notification.MessageKeys()
            };

            objMessage.Properties.MessageID = objMessageInfo.MessageID % 2 == 0 ? objMessageInfo.MessageID : objMessageInfo.MessageID - 5;
            objMessage.Properties.Type = (Notification.E_MessageType)(int)objMessageInfo.Service_MessageType;
            objMessage.Properties.FromDisplayName = objMessageInfo.FromDisplayName;
            objMessage.Properties.SourceName = objMessageInfo.SourceName;
            objMessage.Properties.Subject = objMessageInfo.Subject;

            if (objMessageInfo.Override)
            {
                objMessage.Properties.From = objMessageInfo.From;
                objMessage.Properties.To = objMessageInfo.GetTo;
                objMessage.Properties.Bcc = objMessageInfo.GetBCC;
                objMessage.Properties.Cc = objMessageInfo.GetCC;
            }

            objMessage.Properties.CustomData = objMessageInfo.DepositID.ToString();
            objMessage.Keys.ContractID = objMessageInfo.ContractID;
            objMessage.Keys.SubID = objMessageInfo.SubID;
            objMessage.Keys.Image = (Notification.E_ImageOption)(int)objMessageInfo.EImageOption;
            objMessage.Properties.Bcc = objMessageInfo.GetBCC;

            if (objMessageInfo.SSN != string.Empty)
            {
                objMessage.Keys.Ssn = objMessageInfo.SSN;
            }

            if (objMessageInfo != null)
            {
                objMessage.Data = new Notification.KeyValue[objMessageInfo.EmailVariableContainer.Count];
                int index = 0;
                foreach (DictionaryEntry ev in objMessageInfo.EmailVariableContainer)
                {
                    objMessage.Data[index] = new Notification.KeyValue();
                    if (ev.Key.ToString().ToLower().IndexOf("messagecenter") > 0)
                    {
                        objMessage.Data[index].key = ev.Key.ToString().ToLower().Replace("messagecenter", "MessageCenter");
                    }
                    else
                    {
                        objMessage.Data[index].key = ev.Key.ToString().ToLower();
                    }
                    objMessage.Data[index].value = ev.Value != null ? ev.Value.ToString() : " ";
                    index++;
                }
            }

            return _objMessageSrv.SendMessage(objMessage);
        }
    }
}