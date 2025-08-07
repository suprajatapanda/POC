using System;
using TRS.IT.TrsAppSettings;

namespace TRS.IT.TAEMQCon
{
	internal class ConnectionSettings
	{
		private readonly string _BackupString = "";

		private int _PlanID;

		private int _TransTypeID;

		public int TransTypeID
		{
			set
			{
				_TransTypeID = value;
			}
		}

		public int PlanID
		{
			set
			{
				_PlanID = value;
			}
		}

		public string QueueName
		{
			get
			{
				if (IsLongWaitTransType)
				{
					return AppSettings.GetValue("QueueName" + _BackupString);
				}
				if (_PlanID > 8448)
				{
					return AppSettings.GetValue("TANY_QueueName" + _BackupString);
				}
				return AppSettings.GetValue("TRAM_QueueName" + _BackupString);
			}
		}

		public string GetQueueName => AppSettings.GetValue("GetQueueName" + _BackupString);

		public string QueueManagerName => AppSettings.GetValue("QueueManagerName" + _BackupString);

		public string ChannelName => AppSettings.GetValue("ChannelName" + _BackupString);

		public string ConnectionName => AppSettings.GetValue("ConnectionName" + _BackupString);

		public string ReplyToQueueName
		{
			get
			{
				if (IsLongWaitTransType)
				{
					return AppSettings.GetValue("ReplyToQueueName" + _BackupString);
				}
				if (_PlanID > 8448)
				{
					return AppSettings.GetValue("TANY_ReplyToQueueName" + _BackupString);
				}
				return AppSettings.GetValue("TRAM_ReplyToQueueName" + _BackupString);
			}
		}

		public string ReplyToQueueManagerName => AppSettings.GetValue("ReplyToQueueManagerName" + _BackupString);

		public int WaitInterval => int.Parse(AppSettings.GetValue("WaitInterval" + _BackupString));

		public string LongWaitTransTypes => AppSettings.GetValue("LongWaitTransTypes" + _BackupString);

		public bool IsLongWaitTransType
		{
			get
			{
				bool result = false;
				try
				{
					string longWaitTransTypes = LongWaitTransTypes;
					if (!string.IsNullOrEmpty(longWaitTransTypes))
					{
						string[] array = longWaitTransTypes.Split(';');
						if (array.Length > 0 && Array.IndexOf(array, _TransTypeID.ToString()) != -1)
						{
							result = true;
						}
					}
				}
				catch (Exception)
				{
					result = false;
				}
				return result;
			}
		}

		public bool UseWebservices
		{
			get
			{
				try
				{
					return bool.Parse(AppSettings.GetValue("UseWebservices" + _BackupString));
				}
				catch (Exception)
				{
					return false;
				}
			}
		}

		public ConnectionSettings()
		{
		}

		public ConnectionSettings(string AlternateHandler)
		{
			if (AlternateHandler != "")
			{
				_BackupString = "_" + AlternateHandler;
			}
		}
	}
}
