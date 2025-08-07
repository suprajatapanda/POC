using System.Data;
using System.Data.SqlTypes;
using System.Xml;
using Microsoft.Data.SqlClient;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TRS.IT.BendProcessor.DAL
{
    public class PASSSchedulerDC
    {
        private string _sConnectString;

        public PASSSchedulerDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }
        public DataSet GetALLScheduledReports()
        {
            DataSet ds = new();

            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pBkP_GetALLScheduledReports", []);
            return ds;

        }

        public DataSet GetPayrollReverseFeedContracts()
        {
            DataSet ds = new();

            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pSI_GetReverseFeedContracts", []);
            return ds;

        }

        public DataSet GetALLScheduledReports(int schedule_status, DateTime dtDeliveryDate) //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
        {
            DataSet ds = new();
            //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report unavailable ; 20 = Error sending notification; 100 = success;
            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pBkP_GetALLScheduledReports", [dtDeliveryDate, schedule_status]);
            return ds;
        }

        public void GetPartnerUserIdAndInLoginId(string contractId, string subId, ref string sPartnerUserId, ref int iInlogInId)
        {
            DataSet ds = null;
            ds = GetSponsorContractsData(contractId, subId);

            if ((ds != null) && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    if (dr["login_Type"] != null && dr["login_Type"].ToString() != "")
                    {
                        sPartnerUserId = Utils.CheckDBNullStr(dr["partner_userid"]);
                        iInlogInId = Utils.CheckDBNullInt(dr["in_login_id"]);
                    }
                    //If sPartnerUserId <> String.Empty Then

                    //    Exit For
                    //End If
                }
            }


        }

        public int UpdateScheduledReportSetNextDates(int schedule_id, int schedule_run_result)// updates next dates only when  schedule_run_result = 100 = success;
        {
            //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
            int iRet;
            //try
            //{
            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "ppsd_EditScheduledReportSetNextDates", [schedule_id, schedule_run_result]);
            //}
            //catch (Exception ex)
            //{
            //    // what to do? 

            //}            

            return iRet;
        }

        public DataSet GetSponsorContractsData(string a_sConId, string a_sSubId)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "ppsd_GetSponsorContractsData", [a_sConId, a_sSubId]);
        }

        public DataSet GetScheduledReportData_customized(int schedule_id)
        {
            return TRSSqlHelper.ExecuteDataset(_sConnectString, "ppsd_GetScheduleDeliveryType_Customized", [schedule_id]);
        }

        public DataSet GetTpaPlanAdminReportsSchedulesDelta()
        {
            DataSet ds = new();
            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pBk_GetTpaPlanAdminReportsSchedulesDelta", []);
            return ds;
        }


        public int InsertBulkTPAPlanAdminScheduledReportData(string ContractsToInsertSchedules_xml)
        {
            /*
             sample ContractsToInsertSchedules_xml = '<Contracts>  <Contract Id="300204" />  <Contract Id="300205" />  <Contract Id="300207" /></Contracts>'
            */

            int iRet = 0;
            if (string.IsNullOrEmpty(ContractsToInsertSchedules_xml))
            {
                return iRet;
            }

            //iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pSI_InsertBulkTPAPlanAdminScheduledReportData", new object[] { ContractsToInsertSchedules_xml, "BackendProc" });


            string spName = "pSI_InsertBulkTPAPlanAdminScheduledReportData";

            using (SqlCommand cmd = new(spName, new SqlConnection(_sConnectString)))
            {
                cmd.CommandTimeout = 600;// use bigger time out for this sp
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Connection.Open();

                StringReader stringReader = new(ContractsToInsertSchedules_xml);
                XmlTextReader reader = new(stringReader);
                SqlXml sqlXml = new(reader);

                SqlParameter p0 = new("@ContractInfo", SqlDbType.Xml);
                p0.Direction = ParameterDirection.Input;
                p0.Value = sqlXml;
                cmd.Parameters.Add(p0);

                SqlParameter p1 = new("@createdBy", SqlDbType.VarChar, 100);
                p1.Direction = ParameterDirection.Input;
                p1.Value = "BackendProc";
                cmd.Parameters.Add(p1);

                iRet = cmd.ExecuteNonQuery();
            }


            return iRet;
        }

        public int DeleteBulkTPAPlanAdminScheduledReportData(string SchedulesToDelete_xml)
        {
            /*
             sample @SchedulesToDelete_xml = '<Schedules>  <Schedule Id="1" />  <Schedule Id="2" /> <Schedule Id="3" /></Schedules>'
            */
            int iRet = 0;
            if (string.IsNullOrEmpty(SchedulesToDelete_xml))
            {
                return iRet;
            }

            iRet = TRSSqlHelper.ExecuteNonQuery(_sConnectString, "pSI_DeleteBulkTPAPlanAdminScheduledReportData", [SchedulesToDelete_xml, 0]);
            return iRet;
        }

        public DataSet GetMEPSubIds(string sContractId)
        {
            DataSet ds = new();

            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pBk_GetMEPSubIds", [sContractId]);
            return ds;
        }

        //DDEV-47686
        public object InsertReport(int InLoginID, string ContractID, string SubID, int ReportType, int ReportStatus, string FileName, string reportRequest, int sPartnerID, bool isSuperUser = false, string sUserID = "", string sApplicationName = "", string sCustomReportName = "")
        {
            try
            {
                return TRSSqlHelper.ExecuteScalar(_sConnectString, "pSI_InsertReportRequestByInLoginID", [InLoginID, ContractID, SubID, ReportType, ReportStatus, FileName, reportRequest, isSuperUser, sPartnerID, sUserID, sApplicationName, sCustomReportName]);
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                return 0;
            }
        }
        //DDEV-47686

        public DataSet GetFIBIScheduledReports() //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
        {
            DataSet ds = new();
            //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report unavailable ; 20 = Error sending notification; 100 = success;
            ds = TRSSqlHelper.ExecuteDataset(_sConnectString, "pBkP_GetFIBIScheduledReports", []);
            return ds;
        }

    }
}
