using System.Text;
using IBM.WMQ;
using TRS.IT.TrsAppSettings;


namespace FWUpdateRKPartner.MessageQueue.MQConB
{
    public class TAEMQ
    {
        private string _sPutQueueName;

        private string _sGetQueueName;

        private string _sQueueManagerName;

        private string _sChannelName;

        private string _sTransportType;

        private string _sConnectionName;

        private string _sReplyToQueueName;

        private string _sReplyToQueueManagerName;

        private byte[] _MessageID;

        private byte[] _CorelID;

        private int _iWaitInterval;

        private int _iRetry = 1;

        private int _iPlanFamily;

		private MQQueueManager _oQueueManager;

		private MQQueue _oPutQueue;

		private MQQueue _oGetQueue;

		public bool IsMQConnected
		{
			get
			{
				if (_oQueueManager != null)
				{
					return _oQueueManager.IsConnected;
				}
				return false;
			}
		}

        public int RetryAttempts
        {
            get
            {
                return _iRetry;
            }
            set
            {
                _iRetry = value;
            }
        }

		public int PlanFamily => _iPlanFamily;

        public TAEMQ()
        {
            _sPutQueueName = AppSettings.GetValue("QueueName");
            _sGetQueueName = AppSettings.GetValue("GetQueueName");
            _sQueueManagerName = AppSettings.GetValue("QueueManagerName");
            _sChannelName = AppSettings.GetValue("ChannelName");
            _sTransportType = AppSettings.GetValue("TransportType");
            _sConnectionName = AppSettings.GetValue("ConnectionName");
            _sReplyToQueueName = AppSettings.GetValue("ReplyToQueueName");
            _sReplyToQueueManagerName = AppSettings.GetValue("ReplyToQueueManagerName");
            _iWaitInterval = Convert.ToInt32(AppSettings.GetValue("WaitInterval"));
        }

        public void MQConnect(string a_sPutQueueName, string a_sReplyToQueueName, int a_iPlanFamily)
        {
            if (a_iPlanFamily != _iPlanFamily)
            {
                MQDisconnect();
            }
            _sPutQueueName = a_sPutQueueName;
            _sReplyToQueueName = a_sReplyToQueueName;
            if (!IsMQConnected)
            {
                try
                {
					_oQueueManager = new MQQueueManager(_sQueueManagerName, _sChannelName, _sConnectionName);
					_oQueueManager.Connect();
					_oPutQueue = _oQueueManager.AccessQueue(_sPutQueueName, 8208);
					_oGetQueue = _oQueueManager.AccessQueue(_sGetQueueName, 8193);
					_iPlanFamily = a_iPlanFamily;
				}
				catch (MQException ex)
				{
					throw new Exception("P-RC: " + ex.ReasonCode + " R:" + ex.Reason + ex.Message.ToString());
				}
				catch
				{
					throw;
				}
			}
		}

		public void MQDisconnect()
		{
			if (_oQueueManager != null)
			{
				_oQueueManager.Disconnect();
				_oQueueManager = null;
			}
			if (_oGetQueue != null)
			{
				_oGetQueue.Close();
				_oGetQueue = null;
			}
			if (_oPutQueue != null)
			{
				_oPutQueue.Close();
				_oPutQueue = null;
			}
		}

		public string SubmitMQ(string a_sMsg)
		{
			string result = "";
			bool flag = false;
			int num = 0;
			if (!IsMQConnected)
			{
				throw new Exception("MQ not connected");
			}
			try
			{
				PutMQMsg(a_sMsg);
				result = GetMQMsg();
				flag = true;
			}
			catch
			{
				while (!flag && num < _iRetry)
				{
					num++;
					if (!IsMQConnected)
					{
						MQConnect(_sPutQueueName, _sReplyToQueueName, _iPlanFamily);
					}
					try
					{
						PutMQMsg(a_sMsg);
						result = GetMQMsg();
						flag = true;
					}
					catch
					{
						if (num >= _iRetry)
						{
							throw;
						}
					}
				}
			}
			return result;
		}

		private void PutMQMsg(string a_sMsg)
		{
			MQMessage mQMessage = new MQMessage();
			MQPutMessageOptions mQPutMessageOptions = new MQPutMessageOptions();
			try
			{
				mQMessage.ReplyToQueueName = _sReplyToQueueName;
				mQMessage.ReplyToQueueManagerName = _sReplyToQueueManagerName;
				mQMessage.Format = "MQSTR   ";
				mQMessage.WriteBytes(a_sMsg);
				_oPutQueue.Put(mQMessage, mQPutMessageOptions);
				_MessageID = mQMessage.MessageId;
				_CorelID = mQMessage.CorrelationId;
			}
			catch (MQException ex)
			{
				throw new Exception("P-RC: " + ex.ReasonCode + " R:" + ex.Reason + ex.Message.ToString());
			}
			catch
			{
				throw;
			}
		}

		private string GetMQMsg()
		{
			MQMessage mQMessage = new MQMessage();
			MQGetMessageOptions mQGetMessageOptions = new MQGetMessageOptions();
			string text = null;
			try
			{
				mQMessage.Format = "MQSTR   ";
				mQMessage.MessageId = _MessageID;
				mQMessage.CorrelationId = _CorelID;
				mQGetMessageOptions.Options = 1;
				mQGetMessageOptions.WaitInterval = _iWaitInterval;
				_oGetQueue.Get(mQMessage, mQGetMessageOptions);
				text = mQMessage.ReadString(mQMessage.MessageLength);
			}
			catch (MQException ex)
			{
				throw new Exception("P-RC: " + ex.ReasonCode + " R:" + ex.Reason + ex.Message.ToString());
			}
			catch
			{
				throw;
			}
			return CleanResponse(text);
		}

		private string CleanResponse(string a_sData)
		{
			StringBuilder stringBuilder = new StringBuilder(a_sData);
			_ = stringBuilder.Length;
			for (int i = 0; i < stringBuilder.Length; i++)
			{
				if (stringBuilder[i] == '\0')
				{
					stringBuilder.Remove(i, 1);
					stringBuilder.Insert(i, " ");
				}
			}
			return stringBuilder.ToString();
		}
	}
}
