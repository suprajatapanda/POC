using System.Text.RegularExpressions;
using IBM.WMQ;
using TRS.IT.TAEMQCon.mqSvc;

namespace TRS.IT.TAEMQCon
{
	public class MQConnection
	{
		private sealed class ThreadData
		{
			public byte[] MessageID;

			public byte[] CorelID;

			public int TransTypeID;

			public string Message;

			public string Response;

			public string ResponseLocation;

			public MQLogDC oLogDC = new MQLogDC();

			public ConnectionSettings ConnectionSettings;
		}

		private static object _threadLock = new object();

		private static MQQueueManager _QueueManagerShared1;

		private static MQQueueManager _QueueManagerShared2;

		private static int _ErrorCount;

		private string _AlternateHandler = "";

		public MQConnection()
		{
		}
		private MQQueueManager GetQueueManagerShared(ThreadData data)
		{
			if (DateTime.Now.Millisecond % 2 == 0)
			{
				data.ResponseLocation = data.ResponseLocation + " - GetQueueManagerShared1 @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				return _QueueManagerShared1;
			}
			data.ResponseLocation = data.ResponseLocation + " - GetQueueManagerShared2 @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
			return _QueueManagerShared2;
		}
		private void QueueManagerShared(ThreadData data)
		{
			lock (_threadLock)
			{
				if (_QueueManagerShared1 == null || !_QueueManagerShared1.IsConnected || _ErrorCount > 5)
				{
					data.ResponseLocation = data.ResponseLocation + " - QueueManagerShared1 @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
					_QueueManagerShared1 = new MQQueueManager(data.ConnectionSettings.QueueManagerName, data.ConnectionSettings.ChannelName, data.ConnectionSettings.ConnectionName);
				}
				if (_QueueManagerShared2 == null || !_QueueManagerShared2.IsConnected || _ErrorCount > 5)
				{
					data.ResponseLocation = data.ResponseLocation + "- QueueManagerShared2 @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
					_QueueManagerShared2 = new MQQueueManager(data.ConnectionSettings.QueueManagerName, data.ConnectionSettings.ChannelName, data.ConnectionSettings.ConnectionName);
				}
			}
		}
		private void PutMessage(ThreadData data, string message)
		{
			data.ResponseLocation = data.ResponseLocation + " - AccessQueue @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
			MQQueue val = GetQueueManagerShared(data).AccessQueue(data.ConnectionSettings.QueueName, 8208);
			try
			{
				if (((MQManagedObject)val).IsOpen && ((MQBaseObject)val).CompletionCode != 2)
				{
					data.ResponseLocation = data.ResponseLocation + " - SetupQueue @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
					MQMessage val2 = new MQMessage();
					val2.ReplyToQueueName = data.ConnectionSettings.ReplyToQueueName;
					val2.ReplyToQueueManagerName = data.ConnectionSettings.ReplyToQueueManagerName;
					val2.Format = "MQSTR   ";
					val2.Expiry = 1200;
					data.ResponseLocation = data.ResponseLocation + " - WriteBytes @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
					val2.WriteBytes(message);
					MQPutMessageOptions val3 = new MQPutMessageOptions();
					data.ResponseLocation = data.ResponseLocation + " - Put @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
					val.Put(val2, val3);
					data.MessageID = val2.MessageId;
					data.CorelID = val2.CorrelationId;
					return;
				}
				throw new Exception("Access Queue failed");
			}
			finally
			{
				if (val != null || ((MQManagedObject)val).IsOpen)
				{
					((MQManagedObject)val).Close();
				}
			}
		}
		private string GetMessage(ThreadData data)
		{
			MQQueue val = null;
			MQMessage val2 = new MQMessage();
			val2.Format = "MQSTR   ";
			val2.MessageId = data.MessageID;
			val2.CorrelationId = data.CorelID;
			MQGetMessageOptions val3 = new MQGetMessageOptions();
			val3.Options = 1;
			val3.WaitInterval = data.ConnectionSettings.WaitInterval;
			string text;
			try
			{
				data.ResponseLocation = data.ResponseLocation + " - AccessQueue @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				val = GetQueueManagerShared(data).AccessQueue(data.ConnectionSettings.GetQueueName, 8193);
				data.ResponseLocation = data.ResponseLocation + " - Get @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				val.Get(val2, val3);
				text = ((val2.Format.CompareTo("MQSTR   ") != 0) ? "Non-text message" : CleanMe(val2.ReadString(val2.MessageLength)));
			}
			finally
			{
				if (val != null || ((MQManagedObject)val).IsOpen)
				{
					((MQManagedObject)val).Close();
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				text = "IsNull or Empty - AAM";
			}
			return text;
		}
		private void Thread_SubmitTransactionToMQ(object dataIncoming)
		{
			ThreadData threadData = (ThreadData)dataIncoming;
			threadData.oLogDC.LogRequest(threadData.Message, threadData.TransTypeID);
			try
			{
				threadData.ResponseLocation = threadData.ResponseLocation + "QueueManager @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				QueueManagerShared(threadData);
				threadData.ResponseLocation = threadData.ResponseLocation + "PutMessage @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				PutMessage(threadData, threadData.Message);
				threadData.ResponseLocation = threadData.ResponseLocation + "GetMessage @ " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
				threadData.Response = GetMessage(threadData);
				if (string.IsNullOrEmpty(threadData.Response))
				{
					threadData.Response = "*** Error: Response Empty ***";
				}
				_ErrorCount = 0;
			}
			catch (MQException ex)
			{
				MQException ex2 = ex;
				threadData.oLogDC.LogResponse(string.Format("Error {0} {1}: Reason - {2}; {3}; #{4}", threadData.ResponseLocation, "v1.4", ex2.Reason, ((Exception)(object)ex2).Message, _ErrorCount));
				CleanQueueManager(threadData, ex2);
				_ErrorCount++;
			}
			catch (Exception ex3)
			{
				threadData.oLogDC.LogResponse(string.Format("Error {0} {1}: {2}; #{3}", threadData.ResponseLocation, "v1.4", ex3.Message, _ErrorCount));
				_ErrorCount++;
			}
			finally
			{
				threadData.oLogDC.LogResponse(threadData.Response + "|| LD: " + threadData.ResponseLocation);
			}
		}
		private static void CleanQueueManager(ThreadData data, MQException mqe)
		{
			try
			{
				lock (_threadLock)
				{
					if (mqe.Reason != 2009.ToString())
					{
						return;
					}
					if (_QueueManagerShared1 != null)
					{
						if (_QueueManagerShared1.IsConnected)
						{
							_QueueManagerShared1.Disconnect();
						}
						_QueueManagerShared1 = null;
						_ErrorCount = 10;
					}
					if (_QueueManagerShared2 != null)
					{
						if (_QueueManagerShared2.IsConnected)
						{
							_QueueManagerShared2.Disconnect();
						}
						_QueueManagerShared2 = null;
						_ErrorCount = 10;
					}
				}
			}
			catch (Exception ex)
			{
				data.oLogDC.LogResponse($"Error cleaning up QueueManager - {ex.Message}");
			}
		}
		public string SubmitTransaction(string msg, int transTypeID)
		{
			ConnectionSettings connectionSettings = new ConnectionSettings();
			if (connectionSettings.UseWebservices)
			{
				return MQWebserviceCall(msg, transTypeID);
			}
			return MQCall(msg, transTypeID);
		}
		private string MQWebserviceCall(string msg, int transTypeID)
		{
			MQLogDC mQLogDC = new MQLogDC();
			MQService mQService = new MQService();
			return mQService.SubmitTransaction(msg, transTypeID, mQLogDC.GetHostName(), mQLogDC.GetClientIP(), mQLogDC.SessionID ?? string.Empty);
		}
		private string MQCall(string msg, int transTypeID)
		{
			ThreadData threadData = new ThreadData();
			if (string.IsNullOrEmpty(_AlternateHandler))
			{
				threadData.ConnectionSettings = new ConnectionSettings();
			}
			else
			{
				threadData.ConnectionSettings = new ConnectionSettings(_AlternateHandler);
			}
			threadData.ConnectionSettings.PlanID = int.Parse(msg.Substring(14, 4));
			threadData.ConnectionSettings.TransTypeID = transTypeID;
			threadData.TransTypeID = transTypeID;
			threadData.Message = msg;
			Thread thread = new Thread(Thread_SubmitTransactionToMQ);
			thread.Start(threadData);
			int num = threadData.ConnectionSettings.WaitInterval;
			if (threadData.ConnectionSettings.IsLongWaitTransType)
			{
				num *= 2;
			}
			if (!thread.Join(num + 5000))
			{
				_ErrorCount++;
				thread.Abort();
				if (string.IsNullOrEmpty(threadData.Response))
				{
					threadData.oLogDC.LogResponse(string.Format("Error, timeout occurred MQCall {0}", "v1.4"));
				}
				throw new Exception("Timeout occured in thread.");
			}
			return threadData.Response;
		}
		public string CleanMe(string str)
		{
			str = str.Replace("\0", " ");
			str = Regex.Replace(str, "[^\\u0000-\\u007F]", " ");
			return str;
		}
	}
}
