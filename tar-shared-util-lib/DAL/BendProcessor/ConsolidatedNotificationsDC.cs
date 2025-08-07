using System.Data;
using Microsoft.Data.SqlClient;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TRS.IT.BendProcessor.DAL
{
    public class ConsolidatedNotificationsDC
    {
        private string _sConnectString;
        public ConsolidatedNotificationsDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet GetALLInputDetails()
        {
            DataSet ds = new();

            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pnt_GetInputDetails", []);
            return ds;
        }
        public DataSet GetMsgTemplatesAndContactDetails(int DocType_id)
        {
            DataSet ds = new();

            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pnt_GetMsgTemplatesAndContactDetails", [DocType_id]);
            return ds;

        }
        public int InsertMessageQueue(int row_no_InputDetails, int individual_id, string LoginType, string email_id, string Data_To_Consolidate, string sMessage_Variables, int Msg_Template_id, int MsgCtr_Template_id, DateTime Send_dt, bool bMultipleDocTypesGrouped, string sDocType_Description)
        {
            //process_status: -1 = Error ; 0 = Pending; 100 = complete/ successfully copied to nt_MessageQueue table;
            int iRet = 0;

            if (bMultipleDocTypesGrouped)
            {
                iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pnt_InsertDocGroupMessageQueue", [row_no_InputDetails, individual_id, email_id, sDocType_Description, Data_To_Consolidate, sMessage_Variables, Msg_Template_id, MsgCtr_Template_id, Send_dt]);
            }
            else
            {
                iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pnt_InsertMessageQueue", [row_no_InputDetails, individual_id, LoginType, email_id, Data_To_Consolidate, sMessage_Variables, Msg_Template_id, MsgCtr_Template_id, Send_dt]);
            }

            return iRet;
        }
        public int InsertContractMessageQueue(string contract_id, string sub_id, string LoginType, string email_id, string Data_To_Consolidate,
            string Message_Variables, int Msg_Template_id, int MsgCtr_Template_id, DateTime Send_dt, int iDocType_id, string sInput_params, string PartnerID)
        {
            //process_status: -1 = Error ; 0 = Pending; 100 = complete/ successfully copied to nt_MessageQueue table;
            int iRet;

            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pnt_InsertContractMessageQueue", [contract_id, sub_id, LoginType, email_id, Data_To_Consolidate, Message_Variables, Msg_Template_id, MsgCtr_Template_id, Send_dt, iDocType_id, sInput_params, PartnerID]);

            return iRet;
        }
        public int UpdateInputDetailsStatus(int row_id, int process_status)
        {
            //process_status: -1 = Error ; 0 = Pending; 100 = complete/ successfully copied to nt_InputDetails table;
            int iRet;

            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pnt_UpdateInputDetailsStatus", [row_id, process_status]);

            return iRet;
        }
        public DataSet GetConsolidatedMessageQueue()
        {
            DataSet ds = new();

            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pnt_GetConsolidatedMessageQueue", []);
            return ds;
        }
        public DataSet GetConsolidatedContractMessageQueue()
        {
            DataSet ds = new();

            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pnt_GetConsolidatedContractMessageQueue", []);
            return ds;
        }
        public DataSet GetConsolidateDocGroupdMessageQueue()
        {
            DataSet ds = new();

            ////ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pnt_GetConsolidateDocGroupdMessageQueue", new object[] { });

            string spName = "pnt_GetConsolidateDocGroupdMessageQueue";

            using (SqlCommand cmd = new(spName, new SqlConnection(_sConnectString)))
            {
                cmd.CommandTimeout = 600;// use bigger time out for this sp
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection.Open();
                //DataTable table = new DataTable();
                //table.Load(cmd.ExecuteReader());
                //ds.Tables.Add(table);
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    DataTable table = new();
                    table.Load(rd);
                    ds.Tables.Add(table);
                }
                cmd.Connection.Close();
            }

            return ds;
        }

        public int UpdateConsolidatedMessageQueueStatus(int row_counter, int process_status)
        {
            //process_status: -1 = Error ; 0 = Pending; 100 = complete/ successfully copied to nt_MessageQueue table;
            int iRet;

            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pnt_UpdateConsolidatedMessageQueueStatus", [row_counter, process_status]);

            return iRet;
        }

        public int UpdateConsolidatedContractMessageQueueStatus(int row_counter, int process_status)
        {
            //process_status: -1 = Error ; 0 = Pending; 100 = complete/ successfully copied to nt_MessageQueue table;
            int iRet;

            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pnt_UpdateConsolidatedContractMessageQueueStatus", [row_counter, process_status]);

            return iRet;
        }

        public int UpdateConsolidatedDocGroupdMessageQueueStatus(string sXMl_row_counter, int process_status)
        {
            //process_status: -1 = Error ; 0 = Pending; 100 = complete/ successfully copied to nt_MessageQueue table;
            int iRet;

            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pnt_UpdateConsolidatedDocGroupdMessageQueueStatus", [sXMl_row_counter, process_status]);

            return iRet;
        }
        public int InsertTmpDIAFeedDailyForAllPpt(string contract_id, string sub_id, int notification_type, bool bRollUpMEP, int sub_notification_type, string feed)
        {
            int iRet;

            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "dcP_InsertTmpDIAFeedDailyForAllPpt", [contract_id, sub_id, notification_type, Convert.ToByte(bRollUpMEP), sub_notification_type, feed]);

            return iRet;
        }
        public int InsertTmpDIAReqdNoticesFeedDaily(string contract_id, string sub_id, int notification_type, int subnotification_type, string feed)
        {
            int iRet;

            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "dcP_InsertReqdNoticesDIAFeedDaily", [contract_id, sub_id, notification_type, subnotification_type, feed]);

            return iRet;
        }
        public DataSet GetCustomContactDetails(string contract_id, string sub_id, int DocType_id, string sLoginType)
        {
            //Note: When input DocType_id = 678 then return the data for 9999 , custom contacts should be same for both (avoid backfilling of setup data since 678 was added later)

            // Please note that if supplied sub_id is not 000 then this sp try to return the data for 000 level. Use it if applicable, otherwise treat as no data returned.
            DataSet dsReturn = null;
            string sConnectionString = _sConnectString;

            //'TEMP TEST ONLY change
            //sConnectionString = "Server=LADBTRSDEV04\TRSSQL02A;DATABASE=web_main;trusted_connection=yes;Connection Lifetime=3000;Max Pool Size=200"
            if (DocType_id == 36510 || DocType_id == 36520)
            {
                DocType_id = 36500; // settings for 36510 and 36520 are same as 36500 so we just justa save them for 36500 from ta-retirement.com
            }

            if (DocType_id == 678)
            {
                DocType_id = 9999; // settings for 36510 and 36520 are same as 36500 so we just justa save them for 36500 from ta-retirement.com
            }

            dsReturn = TRSSqlHelper.ExecuteDataset(sConnectionString, "pnt_GetCustomContactDetails", [contract_id, sub_id, DocType_id, sLoginType]);

            return dsReturn;
        }
    }
}
