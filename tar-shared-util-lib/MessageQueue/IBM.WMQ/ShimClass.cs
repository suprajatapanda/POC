namespace IBM.WMQ
{
    public class MQException : Exception
    {
        public int ReasonCode { get; set; }
        public string Reason { get; set; }

        public MQException() : base() { }
        public MQException(string message) : base(message) { }
        public MQException(string message, Exception innerException) : base(message, innerException) { }

        public const int MQRC_CONNECTION_BROKEN = 2009;
        public const int MQRC_Q_MGR_NOT_AVAILABLE = 2059;
        public const int MQRC_NOT_AUTHORIZED = 2035;
        public const int MQRC_Q_FULL = 2053;
        public const int MQRC_NO_MSG_AVAILABLE = 2033;
    }

    public abstract class MQBaseObject
    {
        public int CompletionCode { get; set; }
        public int ReasonCode { get; set; }
    }

    public abstract class MQManagedObject : MQBaseObject
    {
        public bool IsOpen { get; protected set; }

        public virtual void Close()
        {
            IsOpen = false;
        }
    }

    public class MQQueueManager : MQManagedObject
    {
        private string _queueManagerName;
        private string _channelName;
        private string _connectionName;
        private bool _isConnected;

        public bool IsConnected => _isConnected;

        public MQQueueManager(string queueManagerName)
        {
            _queueManagerName = queueManagerName;
        }

        public MQQueueManager(string queueManagerName, string channelName, string connectionName)
        {
            _queueManagerName = queueManagerName;
            _channelName = channelName;
            _connectionName = connectionName;
        }

        public void Connect()
        {
            _isConnected = true;
            IsOpen = true;
        }

        public void Disconnect()
        {
            _isConnected = false;
            IsOpen = false;
        }

        public MQQueue AccessQueue(string queueName, int openOptions)
        {
            if (!_isConnected)
                throw new MQException("Queue manager not connected") { ReasonCode = 2059 };

            return new MQQueue(this, queueName, openOptions);
        }
    }

    public class MQQueue : MQManagedObject
    {
        private MQQueueManager _queueManager;
        private string _queueName;
        private int _openOptions;

        internal MQQueue(MQQueueManager queueManager, string queueName, int openOptions)
        {
            _queueManager = queueManager;
            _queueName = queueName;
            _openOptions = openOptions;
            IsOpen = true;
        }

        public void Put(MQMessage message, MQPutMessageOptions putMessageOptions)
        {
            if (!IsOpen)
                throw new MQException("Queue not open") { ReasonCode = 2018 };

            if (message.MessageId == null || message.MessageId.Length == 0)
            {
                message.MessageId = Guid.NewGuid().ToByteArray();
            }

            // Simulate correlation ID generation if not set
            if (message.CorrelationId == null || message.CorrelationId.Length == 0)
            {
                message.CorrelationId = new byte[24];
            }
        }

        public void Get(MQMessage message, MQGetMessageOptions getMessageOptions)
        {
            if (!IsOpen)
                throw new MQException("Queue not open") { ReasonCode = 2018 };
            if ((getMessageOptions.Options & MQC.MQGMO_WAIT) != 0)
            {
                System.Threading.Thread.Sleep(100);
            }
        }
    }

    // MQ Message
    public class MQMessage
    {
        private List<byte> _data = new List<byte>();
        private int _dataOffset = 0;

        public byte[] MessageId { get; set; } = new byte[24];
        public byte[] CorrelationId { get; set; } = new byte[24];
        public string ReplyToQueueName { get; set; }
        public string ReplyToQueueManagerName { get; set; }
        public string Format { get; set; }
        public int Expiry { get; set; }
        public int MessageLength => _data.Count;

        public void WriteBytes(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(text);
            _data.AddRange(bytes);
        }

        public void WriteBytes(byte[] data)
        {
            if (data != null)
                _data.AddRange(data);
        }

        public string ReadString(int length)
        {
            if (_dataOffset + length > _data.Count)
                length = _data.Count - _dataOffset;

            if (length <= 0)
                return string.Empty;

            byte[] bytes = _data.GetRange(_dataOffset, length).ToArray();
            _dataOffset += length;

            return System.Text.Encoding.UTF8.GetString(bytes);
        }
    }

    public class MQPutMessageOptions
    {
        public int Options { get; set; }

        public MQPutMessageOptions()
        {
            Options = MQC.MQPMO_NONE;
        }
    }

    public class MQGetMessageOptions
    {
        public int Options { get; set; }
        public int WaitInterval { get; set; }

        public MQGetMessageOptions()
        {
            Options = MQC.MQGMO_NONE;
            WaitInterval = 0;
        }
    }

    // MQ Constants
    public static class MQC
    {
        public const int MQOO_INPUT_AS_Q_DEF = 0x00000001;
        public const int MQOO_INPUT_SHARED = 0x00000002;
        public const int MQOO_INPUT_EXCLUSIVE = 0x00000004;
        public const int MQOO_BROWSE = 0x00000008;
        public const int MQOO_OUTPUT = 0x00000010;
        public const int MQOO_INQUIRE = 0x00000020;
        public const int MQOO_SET = 0x00000040;
        public const int MQOO_BIND_ON_OPEN = 0x00004000;
        public const int MQOO_BIND_NOT_FIXED = 0x00008000;
        public const int MQOO_FAIL_IF_QUIESCING = 0x00002000;

        public const int MQGMO_NONE = 0x00000000;
        public const int MQGMO_WAIT = 0x00000001;
        public const int MQGMO_NO_WAIT = 0x00000000;
        public const int MQGMO_BROWSE_FIRST = 0x00000010;
        public const int MQGMO_BROWSE_NEXT = 0x00000020;
        public const int MQGMO_MSG_UNDER_CURSOR = 0x00000100;
        public const int MQGMO_LOCK = 0x00000200;
        public const int MQGMO_UNLOCK = 0x00000400;
        public const int MQGMO_ACCEPT_TRUNCATED_MSG = 0x00000040;

        public const int MQPMO_NONE = 0x00000000;
        public const int MQPMO_SYNCPOINT = 0x00000002;
        public const int MQPMO_NO_SYNCPOINT = 0x00000004;
        public const int MQPMO_DEFAULT_CONTEXT = 0x00000020;
        public const int MQPMO_NEW_MSG_ID = 0x00000040;
        public const int MQPMO_NEW_CORREL_ID = 0x00000080;

        public const int MQCC_OK = 0;
        public const int MQCC_WARNING = 1;
        public const int MQCC_FAILED = 2;

        public const string MQFMT_STRING = "MQSTR   ";
        public const string MQFMT_NONE = "        ";
    }
}