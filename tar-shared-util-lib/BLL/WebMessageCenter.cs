
using TRS.IT.SI.Services.wsMessage;

namespace TRS.IT.SI.BusinessFacadeLayer
{
    public class WebMessageCenter
    {
        public static long GetMsgCntrCMSAcctById(string ExLoginId)
        {
            return DAL.webMessageDC.GetMsgCntrCMSAcctById(ExLoginId);
        }
        public TWS_Response SendWebMessages(string InLogin_xml, webMessage oWebMsg, string ContractID, string SubID)
        {
            var _SOA_MessageService = new SOA.MessageSOA();
            return _SOA_MessageService.SendWebMessages(InLogin_xml, oWebMsg, ContractID, SubID);
        }
    }
}