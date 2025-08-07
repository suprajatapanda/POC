using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TRS.IT.TAEMQCon
{
    internal class MQLogDC
    {
        private long _RequestID;
        private static string _HostName;

        private string ConnectionString => AppSettings.GetValue("ConnectString");

        internal string SessionID
        {
            get
            {
                return null;
            }
        }

        internal long LogRequest(string request, int transTypeID)
        {
            if (AppSettings.GetValue("SuppressMQLog") == null || AppSettings.GetValue("SuppressMQLog") != "True")
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(ConnectionString, "[dbo].pSI_InsertMQRequest");
                spParameterSet[0].Value = GetHostName();
                spParameterSet[1].Value = GetClientIP();
                spParameterSet[2].Value = request;
                spParameterSet[3].Value = SessionID;
                if (transTypeID != 0)
                {
                    spParameterSet[4].Value = transTypeID;
                }
                _RequestID = Convert.ToInt32(TRSSqlHelper.ExecuteScalar(ConnectionString, CommandType.StoredProcedure, "[dbo].pSI_InsertMQRequest", spParameterSet));
            }
            else
            {
                _RequestID = 0L;
            }
            return _RequestID;
        }

        internal void LogResponse(string response)
        {
            if (AppSettings.GetValue("SuppressMQLog") == null || AppSettings.GetValue("SuppressMQLog") != "True")
            {
                SqlParameter[] spParameterSet = SqlHelperParameterCache.GetSpParameterSet(ConnectionString, "[dbo].pSI_InsertMQResponse");
                spParameterSet[0].Value = _RequestID;
                spParameterSet[1].Value = string.Format("{1} || LT: {0} ", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), response);
                TRSSqlHelper.ExecuteNonQuery(ConnectionString, CommandType.StoredProcedure, "[dbo].pSI_InsertMQResponse", spParameterSet);
            }
        }

        internal string GetHostName()
        {
            if (string.IsNullOrEmpty(_HostName))
            {
                _HostName = Dns.GetHostName();
            }
            return _HostName;
        }

        internal string GetClientIP()
        {
            // Always return default IP for console application
            return "0.0.0.0";
        }
    }
}
