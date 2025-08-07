using TRS.IT.BendProcessor.Model;
using TRS.IT.TrsAppSettings;
namespace FWUpdateRKPartner.MessageQueue
{
    public class TaeMQ
    {
        private string _connectString;
        private string _tab = Convert.ToString("\t");
        private MQConB.TAEMQ _oMQ = new MQConB.TAEMQ();
        public TaeMQ()
        {
            _connectString = AppSettings.GetValue(ConstN.C_CONNECT_STRING);
        }

        #region ***** Public Funcs *****
        public ResultReturn UpdateFundChange(string a_sPlanId, string a_sEffectiveDt, string a_sPM, string a_sFundData)
        {
            ResultReturn oReturn = new ResultReturn();
            const string C_6007 = "6007SP0000????";
            const string C_MQUser = "INTERNET/09300016";

            try
            {
                if (a_sPM.Length > 30)
                    a_sPM = a_sPM.Substring(0, 30);
                else
                    a_sPM = a_sPM.PadRight(30, Convert.ToChar(" "));
                oReturn.request = C_6007 + a_sPlanId + "9999999999" + Utils.Utils.GetFID(a_sPlanId) + C_MQUser
                    + Convert.ToDateTime(a_sEffectiveDt).ToString("yyyyMMdd") + a_sPM + a_sFundData;
                oReturn.response = GetMQMsg(oReturn.request);
                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                if (!string.IsNullOrEmpty(oReturn.response) && oReturn.response.Length >= 3 && oReturn.response.Substring(0, 3) != null && oReturn.response.Substring(0, 3) != "000")
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, oReturn.response, ErrorSeverityEnum.Failed));
                }
            }
            catch (Exception ex)
            {
                TRS.IT.BendProcessor.Util.Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));

            }
            return oReturn;

        }
        public string GetTaePlanId(string a_sConId)
        {
            string sData = GetMQMsg("6011SP0001????    " + a_sConId);
            string sRet = string.Empty;
            if (sData.Substring(0, 3) == "000")
            {
                sRet = sData.Substring(10, 4);
            }
            return sRet;
        }
        #endregion
        private string GetMQMsg(string a_sMsg)
        {
            string sRet = string.Empty;
            int iIndex;
            string sPutQueue, sReplyQueue;
            int iPlanFamily, iPlanId;

            try
            {
                iIndex = a_sMsg.IndexOf("????");
                if (iIndex > 0)
                {
                    if (int.TryParse(a_sMsg.Substring(iIndex + 4, 4), out iPlanId) && (iPlanId > 8448))
                    {
                        //TANY
                        sPutQueue = AppSettings.GetValue("TANY_QueueName");
                        sReplyQueue = AppSettings.GetValue("TANY_ReplyToQueueName");
                        iPlanFamily = 2;
                    }
                    else
                    {
                        //TRAM
                        sPutQueue = AppSettings.GetValue("TRAM_QueueName");
                        sReplyQueue = AppSettings.GetValue("TRAM_ReplyToQueueName");
                        iPlanFamily = 1;
                    }
                }
                else
                {
                    //Default to TRAM
                    sPutQueue = AppSettings.GetValue("TRAM_QueueName");
                    sReplyQueue = AppSettings.GetValue("TRAM_ReplyToQueueName");
                    iPlanFamily = 1;

                }
                if (!_oMQ.IsMQConnected || _oMQ.PlanFamily != iPlanFamily)
                    _oMQ.MQConnect(sPutQueue, sReplyQueue, iPlanFamily);

                sRet = _oMQ.SubmitMQ(a_sMsg);
            }
            catch (Exception exp)
            {
                TRS.IT.BendProcessor.Util.Utils.LogError(exp);
                _oMQ.MQDisconnect();
                throw;
            }
            return sRet;
        }

    }
}
