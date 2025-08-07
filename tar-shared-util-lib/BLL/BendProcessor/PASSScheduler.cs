using System.Collections;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SIPBO;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.DriverSOA;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using BFL = TRS.IT.SI.BusinessFacadeLayer;
using SOAModel = TRS.IT.SOA.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace TRS.IT.BendProcessor.BLL
{
    public class PASSScheduler : BendProcessorBase
    {
        private PASSSchedulerDC _oPASSSchedulerDC;

        public PASSScheduler() : base("99", "PASSScheduler", "TRS") { _oPASSSchedulerDC = new PASSSchedulerDC(); }

        private const int C_SCHEDULE_RUN_RESULT_ERROR = 0;
        private const int C_SCHEDULE_RUN_RESULT_SUCCESS = 100;
        private const int C_SCHEDULE_RUN_RESULT_REPORTPENDING = 10;
        private const string C_ApplicationName = "CMS_PASS_SCHEDULER";
        private const string C_ApplicationName_NonPASS = "CMS_NONPASS_SCHEDULER";
        private const string C_ApplicationName_FTP = "FTP_SCHEDULER";
        private const string C_ApplicationName_TPAADMIN = "TPAADMIN_SCHEDULER";
        private const string C_ApplicationName_FTP_TAG = "TAG_FTP_SCHEDULER";
        private const string C_ApplicationName_FTP_TRINET = "TRINET_FTP_SCHEDULER";
        private const string C_ApplicationName_FTP_932058 = "932058_FTP_SCHEDULER";
        private const string C_ApplicationName_FTP_CLCRC = "TRINET_CLCRC_REPORT_SCHEDU"; //DDEV-47686
        private const string C_ApplicationName_FTP_TRINET_SCHEDU = "TRINET_REPORT_SCHEDU";
        private const string C_ApplicationName_FTP_FIBI = "FIBI_FTP_SCHEDULER";

        public TaskStatus ProcessTAGScheduledRptMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;

            const string C_Task = "ProcessTAGScheduledRpt";

            DataSet ds = new();

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    ds = GetScheduledReportsByAppName(C_ApplicationName_FTP_TAG, false);  // IMP: ONLY TAG_FTP_SCHEDULER reports are INCLUDED
                    oReturn = RunScheduledReports(ds);

                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    else
                    {
                        oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        public TaskStatus ProcessSundayScheduledRptMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;

            const string C_Task = "ProcessPASSScheduledRpt";

            DataSet ds = new();

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    {
                        InitTaskStatus(oTaskReturn, C_Task);

                        ds = GetScheduledReportsByAppName(C_ApplicationName_FTP_TAG, true);

                        oReturn = RunScheduledReports(ds);

                        if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                        {
                            General.CopyResultError(oTaskReturn, oReturn);
                            oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                        }
                        else
                        {
                            oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                        }
                        oTaskReturn.rowsCount += oReturn.rowsCount;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        public TaskStatus ProcessPayroll360ScheduledRptMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;

            const string C_Task = "ProcessPayroll360ScheduledRpt";

            DataSet ds = new();

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    ds = GetPayrollReverseFeedContractsMigrated();
                    oReturn = RunPayroll360ScheduledReportsMigrated(ds);

                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    else
                    {
                        oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        public TaskStatus ProcessPASSScheduledPendingRptMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            const string C_Task = "PASSScheduledPendingRpt";
            DataSet ds = new();
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    if (DateTime.Now.DayOfWeek != DayOfWeek.Sunday)
                    {
                        InitTaskStatus(oTaskReturn, C_Task);

                        ds = _oPASSSchedulerDC.GetALLScheduledReports(C_SCHEDULE_RUN_RESULT_REPORTPENDING, DateTime.Now);

                        oReturn = RunPendingReports(ds);

                        if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                        {
                            General.CopyResultError(oTaskReturn, oReturn);
                            oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                        }
                        else
                        {
                            oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                        }
                        oTaskReturn.rowsCount += oReturn.rowsCount;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        public TaskStatus ProcessSundayScheduledPendingRptMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            const string C_Task = "PASSScheduledPendingRpt";
            DataSet ds = new();
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    // IMP: On Sunday reports are run though a separate weekly task at different time.
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Sunday)  // Extra security, run on sunday only
                    {
                        InitTaskStatus(oTaskReturn, C_Task);

                        ds = _oPASSSchedulerDC.GetALLScheduledReports(C_SCHEDULE_RUN_RESULT_REPORTPENDING, DateTime.Now);

                        oReturn = RunPendingReports(ds);

                        if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                        {
                            General.CopyResultError(oTaskReturn, oReturn);
                            oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                        }
                        else
                        {
                            oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                        }
                        oTaskReturn.rowsCount += oReturn.rowsCount;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        public TaskStatus ProcessCreateScheduledReportsMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn = new();
            ResultReturn oReturnDel = new();
            const string C_Task = "ProcessCreateScheduledReports";
            string sContractsToInsertSchedulesXML = "";
            string sSchedulessToDeleteXML = "";
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    DataSet dsDelta = new();
                    DataTable dtAdd;
                    DataTable dtDelete;
                    oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                    dsDelta = _oPASSSchedulerDC.GetTpaPlanAdminReportsSchedulesDelta();

                    if (dsDelta != null && dsDelta.Tables.Count > 0)
                    {
                        dtAdd = dsDelta.Tables[0];
                        foreach (DataRow dr in dtAdd.Rows)
                        {
                            sContractsToInsertSchedulesXML = dr["ContractsToInsertSchedules"].ToString();
                            oReturn = CreateTpaPlanAdminReportsSchedulesMigrated(sContractsToInsertSchedulesXML);
                            if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                            {
                                General.CopyResultError(oTaskReturn, oReturn);
                                oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                            }

                        }

                        if (dsDelta.Tables.Count > 1)
                        {
                            dtDelete = dsDelta.Tables[1];
                            foreach (DataRow dr in dtDelete.Rows)
                            {
                                sSchedulessToDeleteXML = dr["SchedulesToDelete"].ToString();


                                oReturnDel = DeleteTpaPlanAdminReportsSchedulesMigrated(sSchedulessToDeleteXML);

                                if (oReturnDel.returnStatus != ReturnStatusEnum.Succeeded || oReturnDel.Errors.Count > 0)
                                {
                                    General.CopyResultError(oTaskReturn, oReturnDel);
                                    oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                }
                            }
                        }
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        public SOAModel.ContractInfo GetContractInfoFromBFL(string sContractID, string sSubID)
        {
            SOAModel.ContractInfo oConInfo;

            BFL.Contract oCon = new(sContractID, sSubID);
            SOAModel.AdditionalData oAdditional = new();
            oAdditional.All_Provisions_Required = true;
            oAdditional.Basic_Provisions_Required = true;
            oAdditional.Contacts_Required = true;

            oConInfo = oCon.GetContractInformation(sContractID, sSubID, oAdditional);
            return oConInfo;
        }
        public DataSet GetPayrollReverseFeedContractsMigrated()
        {
            return _oPASSSchedulerDC.GetPayrollReverseFeedContracts();
        }
        public DataSet GetScheduledReportsByAppName(string sAppName, bool bExcludeSuppliedAppName)
        {
            // if bExcludeSuppliedAppName = false then return all the reports with AppName <> supplied Appname else return AppName = supplied Appname
            DataSet dsReturn = new();
            DataTable dtNew = new();
            DataSet ds = new();
            if (sAppName == C_ApplicationName_FTP_FIBI)
            {
                ds = _oPASSSchedulerDC.GetFIBIScheduledReports(); // it will collect only FIBI Reports in stored procedure report id =111
            }
            else
            {
                ds = _oPASSSchedulerDC.GetALLScheduledReports(); // it will exclude the FIBI reports in stored procedure
            }

            DataView dv = new();
            dv = ds.Tables[0].DefaultView;
            if (bExcludeSuppliedAppName == true)
            {
                dv.RowFilter = "app_name <> '" + sAppName + "'";
            }
            else
            {
                dv.RowFilter = "app_name = '" + sAppName + "'";
            }

            dtNew = dv.ToTable();
            dsReturn.Tables.Add(dtNew);

            dv = new DataView();
            dv = ds.Tables[1].DefaultView; // as o now no need to filter records from 2nd table.
            dsReturn.Tables.Add(dv.ToTable());

            return dsReturn;
        }
        public ResultReturn RunScheduledReports(DataSet ds)
        {
            ResultReturn oReturn = new();
            GeneralDC oGenDC = new();
            SOAModel.ContractInfo oConInfo;
            string sPartner = "";
            string reportFile = "";
            BFL.Model.ReportInfo oReportInfo = null;
            string sCid = "";
            string sSId = "";
            int schedule_id;
            string report_name = "";
            int report_type_id;
            string report_type_desc;
            string report_info;
            DateTime start_dt;
            DateTime end_dt;
            string plan_name;
            int iInloginId = 0;
            string sPartnerUserId = string.Empty;
            string app_name = string.Empty;
            string file_format = string.Empty;
            bool skip_notification = false;
            bool bCreateOnlyMEPLevelReports = true;
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            string sSleepTimeInSec = "5";
            int iSleepTimeInSec = 5;
            string sSkipNotifAppNames = "";

            if (ds != null && ds.Tables.Count > 0)
            {
                sSleepTimeInSec = AppSettings.GetValue("ReportRequest_Sleeptime");
                if (string.IsNullOrEmpty(sSleepTimeInSec))
                {
                    sSleepTimeInSec = "5";
                }
                iSleepTimeInSec = Convert.ToInt32(sSleepTimeInSec);
                sSkipNotifAppNames = AppSettings.GetValue("SkipNotificationAppNames");

                if (string.IsNullOrEmpty(sSkipNotifAppNames))
                {
                    sSkipNotifAppNames = C_ApplicationName_FTP + "|" + C_ApplicationName_TPAADMIN + "|" + C_ApplicationName_FTP_TAG + "|" + C_ApplicationName_FTP_TRINET + "|" + C_ApplicationName_FTP_932058 + "|" + C_ApplicationName_FTP_TRINET_SCHEDU + "|" + C_ApplicationName_FTP_CLCRC + "|" + C_ApplicationName_FTP_FIBI; //DDEV-47686
                }
                string[] sArySkipNotifAppNames = sSkipNotifAppNames.Split('|');

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    oReportInfo = null; reportFile = "";
                    schedule_id = Convert.ToInt32(dr["schedule_id"].ToString()); bCreateOnlyMEPLevelReports = true;
                    try
                    {
                        Thread.Sleep(new TimeSpan(0, 0, iSleepTimeInSec));
                        sCid = dr["contract_id"].ToString();
                        sSId = dr["sub_id"].ToString();
                        schedule_id = Convert.ToInt32(dr["schedule_id"].ToString());
                        report_type_id = Convert.ToInt32(dr["report_type_id"].ToString());
                        report_info = dr["report_info"].ToString();
                        plan_name = "";
                        if (dr["plan_name"] != null) { plan_name = dr["plan_name"].ToString(); }
                        report_type_desc = "";
                        if (dr["report_type_desc"] != null) { report_type_desc = dr["report_type_desc"].ToString(); }
                        start_dt = Convert.ToDateTime(dr["start_dt"].ToString());
                        end_dt = Convert.ToDateTime(dr["end_dt"].ToString());
                        report_name = dr["report_name"].ToString();
                        app_name = string.IsNullOrEmpty(dr["app_name"].ToString()) ? C_ApplicationName : dr["app_name"].ToString();
                        file_format = dr["file_format"].ToString();
                        skip_notification = Convert.ToBoolean(dr["skip_notification"]);
                        bCreateOnlyMEPLevelReports = Convert.ToBoolean(dr["CreateOnlyMEPLevelReports"]);
                        oConInfo = GetContractInfoFromBFL(sCid, sSId);

                        if ((oConInfo == null))
                        {
                            throw new Exception("ScheduleId = " + schedule_id.ToString() + ": No contract info found for contract_id-sub_id = " + sCid + "-" + sSId);
                        }

                        if (app_name == C_ApplicationName && GetKeyValue("PassPayroll", oConInfo.KeyValuePairs) != "1" && GetKeyValue("PassEnrollment", oConInfo.KeyValuePairs) != "1")
                        {
                            throw new Exception("ScheduleId = " + schedule_id.ToString() + ": PASS Payroll or Enrollment Services is NOT enabled for contract_id-sub_id = " + sCid + "-" + sSId);
                        }

                        _oPASSSchedulerDC.GetPartnerUserIdAndInLoginId(sCid, sSId, ref sPartnerUserId, ref iInloginId);
                        sPartner = oConInfo.PartnerID;
                        if ((report_type_id == 110) && app_name.Equals(C_ApplicationName_FTP_CLCRC) && oConInfo.ContractID.Equals("932003"))
                        {
                            oReportInfo = GetReportInfo(report_type_id, oConInfo, sPartnerUserId, iInloginId.ToString(), app_name, start_dt, end_dt, report_info, report_name, file_format, oConInfo.SubID);
                            _oPASSSchedulerDC.InsertReport(iInloginId, oConInfo.ContractID, oConInfo.SubID, 110, 2, oReportInfo.CustomReportName, "", 5, false, "", app_name, oReportInfo.CustomReportName);
                            _oPASSSchedulerDC.UpdateScheduledReportSetNextDates(schedule_id, C_SCHEDULE_RUN_RESULT_SUCCESS);
                        }
                        else if (bCreateOnlyMEPLevelReports || oConInfo.FlagValues.isMEP == false)
                        {
                            oReportInfo = GetReportInfo(report_type_id, oConInfo, sPartnerUserId, iInloginId.ToString(), app_name, start_dt, end_dt, report_info, report_name, file_format, oConInfo.SubID);
                            reportFile = TRSReportHelper.GetReport(iInloginId, oReportInfo);
                            if (reportFile.IndexOf("Error") != -1)
                            {
                                throw new Exception("ScheduleId = " + schedule_id.ToString() + ": Report file info not returned for contract_id-sub_id = " + sCid + "-" + sSId + " - Error - " + reportFile);
                            }
                            else
                            {
                                if (oReportInfo.ReportType == (int)BFL.Model.ReportInfo.ReportTypeEnum.PlanDataXlsFile)
                                {
                                    reportFile = Path.ChangeExtension(reportFile, "xls");
                                }

                                if (skip_notification || sArySkipNotifAppNames.Contains(app_name))
                                {
                                    _oPASSSchedulerDC.UpdateScheduledReportSetNextDates(schedule_id, C_SCHEDULE_RUN_RESULT_SUCCESS);
                                }
                                else
                                {
                                    _oPASSSchedulerDC.UpdateScheduledReportSetNextDates(schedule_id, C_SCHEDULE_RUN_RESULT_REPORTPENDING);
                                }
                            }
                        }
                        else
                        {

                            DataSet dsMEPSubIds = _oPASSSchedulerDC.GetMEPSubIds(sCid);
                            if (dsMEPSubIds != null && dsMEPSubIds.Tables.Count > 0)
                            {
                                foreach (DataRow drsubId in dsMEPSubIds.Tables[0].Rows)
                                {
                                    try
                                    {
                                        oReportInfo = GetReportInfo(report_type_id, oConInfo, sPartnerUserId, iInloginId.ToString(), app_name, start_dt, end_dt, report_info, report_name, file_format, drsubId["sub_id"].ToString());

                                        reportFile = TRSReportHelper.GetReport(iInloginId, oReportInfo);

                                        if (reportFile.IndexOf("Error") != -1)
                                        {
                                            throw new Exception("ScheduleId = " + schedule_id.ToString() + ": Report file info not returned for contract_id-sub_id = " + sCid + "-" + drsubId["sub_id"].ToString() + " - Error - " + reportFile);
                                        }
                                        else
                                        {
                                            if (oReportInfo.ReportType == (int)BFL.Model.ReportInfo.ReportTypeEnum.PlanDataXlsFile)
                                            {
                                                reportFile = Path.ChangeExtension(reportFile, "xls");
                                            }
                                        }

                                    }
                                    catch (Exception exi)
                                    {
                                        Utils.LogError(exi);
                                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                                        oReturn.isException = true;
                                        oReturn.confirmationNo = string.Empty;
                                        oReturn.Errors.Add(new ErrorInfo(-1, "ScheduleId = " + schedule_id.ToString() + "MEP contract_id-sub_id -" + oConInfo.ContractID + "-" + oConInfo.SubID + " - " + " for Report Name: " + report_name + " - " + exi.Message + "<br />", ErrorSeverityEnum.ExceptionRaised));
                                    }

                                }

                                if (skip_notification || sArySkipNotifAppNames.Contains(app_name))
                                {
                                    _oPASSSchedulerDC.UpdateScheduledReportSetNextDates(schedule_id, C_SCHEDULE_RUN_RESULT_SUCCESS);
                                }
                                else
                                {
                                    _oPASSSchedulerDC.UpdateScheduledReportSetNextDates(schedule_id, C_SCHEDULE_RUN_RESULT_REPORTPENDING);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex);
                        _oPASSSchedulerDC.UpdateScheduledReportSetNextDates(schedule_id, C_SCHEDULE_RUN_RESULT_ERROR);

                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.isException = true;
                        oReturn.confirmationNo = string.Empty;
                        oReturn.Errors.Add(new ErrorInfo(-1, "ScheduleId = " + schedule_id.ToString() + " - " + " for Report Name: " + report_name + " - " + ex.Message + "<br />", ErrorSeverityEnum.ExceptionRaised));
                    }
                }
            }
            return oReturn;
        }
        private ResultReturn RunPayroll360ScheduledReportsMigrated(DataSet ds) //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
        {
            ResultReturn oReturn = new();

            GeneralDC oGenDC = new();
            SOAModel.ContractInfo oConInfo = new();
            string sPartner = "";
            string reportFile = "";

            BFL.Model.ReportInfo oReportInfo = null;

            string sCid = "";
            string sSId = "";
            string report_name = "";
            int report_type_id;
            string report_info = "";
            DateTime start_dt;
            DateTime end_dt;
            int iInloginId = 0;
            string sPartnerUserId = string.Empty;
            string app_name = string.Empty;
            string file_format = string.Empty;
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            StringBuilder sbLog = new();
            sbLog.AppendLine("GetPayrollReverseFeedContracts data log:");
            if (ds != null && ds.Tables.Count > 0)
            {
                ds.Tables[0].DefaultView.Sort = "contract_id, sub_id";
                var processed = new List<string>();
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    //When 000 is included in the list, Don't trigger the call to PSD for other SubIDs
                    if (processed.Contains(dr["contract_id"].ToString().Trim()))
                    {
                        continue;
                    }
                    oReportInfo = null; reportFile = ""; //bReportAvailable = false; sError = "";                    
                    try
                    {
                        //create report
                        sCid = dr["contract_id"].ToString().Trim();
                        sSId = dr["sub_id"].ToString().Trim();
                        app_name = "PAYSTART_360";
                        report_type_id = 85;
                        sbLog.AppendLine(sCid + "-" + sSId);
                        start_dt = DateTime.Now.AddDays(-1);
                        end_dt = DateTime.Now.AddDays(-1);   //Convert.ToDateTime(dr["end_dt"].ToString());
                        file_format = "CSV";

                        //oConInfo = GetContractInfoFromBFL(sCid, sSId);
                        oConInfo.ContractID = sCid;
                        oConInfo.SubID = sSId;
                        oConInfo.PartnerPlanID = sCid;
                        switch (dr["admin_id"].ToString())
                        {
                            case "1300":    //ISC
                                oConInfo.PartnerID = "ISC";
                                break;
                            default:    //it should never come to this
                                oConInfo.PartnerID = "TRS";
                                break;
                        }

                        _oPASSSchedulerDC.GetPartnerUserIdAndInLoginId(sCid, sSId, ref sPartnerUserId, ref iInloginId);
                        sPartner = oConInfo.PartnerID;
                        oReportInfo = GetReportInfo(report_type_id, oConInfo, sPartnerUserId, iInloginId.ToString(), app_name, start_dt, end_dt, report_info, report_name, file_format, oConInfo.SubID);
                        oReportInfo.CustomReportName = "payrollvendorfeed_" + oConInfo.ContractID + "_" + oConInfo.SubID + "_" + end_dt.ToShortDateString().Replace("/", "") + ".csv";
                        oReportInfo.PartnerUserID = Regex.Replace(dr["vendorName"].ToString(), @"[^0-9a-zA-Z]+", "");//To use it as Folder Name - IT-109468

                        reportFile = TRSReportHelper.GetReport(iInloginId, oReportInfo);

                        // Send the notifications....
                        if (reportFile.IndexOf("Error") != -1)
                        {
                            throw new Exception("Report file info not returned for contract_id-sub_id = " + sCid + "-" + sSId + " - Error - " + reportFile);
                        }
                        else
                        {
                        }
                        if (dr["sub_id"].ToString().Trim() == "000")
                        {
                            processed.Add(dr["contract_id"].ToString().Trim());
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex);
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.isException = true;
                        oReturn.confirmationNo = string.Empty;
                        oReturn.Errors.Add(new ErrorInfo(-1, "Report Name: " + report_name + " - " + ex.Message + "<br />", ErrorSeverityEnum.ExceptionRaised));
                    }
                }
            }

            SendErrorMsgEmail("RunPayroll360ScheduledReports: GetPayrollReverseFeedContracts data ", sbLog.ToString());
            return oReturn;
        }
        private ResultReturn RunPendingReports(DataSet ds) //schedule_run_result: -1 = default/never ran; 0 = errored; 10 = Report pending ; 20 = Error sending notification; 100 = success;
        {
            ResultReturn oReturn = new();

            GeneralDC oGenDC = new();
            SOAModel.ContractInfo oConInfo;
            string reportFile = "";
            string sCid = "";
            string sSId = "";
            string report_name = "";
            int schedule_id;
            int report_type_id;
            string report_type_desc;
            DateTime start_dt;
            DateTime end_dt;
            string plan_name;
            List<int> lstDeliverTo;
            string sError = "";
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            if (ds != null && ds.Tables.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    sError = "";
                    schedule_id = Convert.ToInt32(dr["schedule_id"].ToString());
                    try
                    {
                        sCid = dr["contract_id"].ToString();
                        sSId = dr["sub_id"].ToString();
                        schedule_id = Convert.ToInt32(dr["schedule_id"].ToString());
                        report_type_id = Convert.ToInt32(dr["report_type_id"].ToString());
                        plan_name = "";
                        if (dr["plan_name"] != null) { plan_name = dr["plan_name"].ToString(); }
                        report_type_desc = "";
                        if (dr["report_type_desc"] != null) { report_type_desc = dr["report_type_desc"].ToString(); }
                        start_dt = Convert.ToDateTime(dr["start_dt"].ToString());
                        end_dt = Convert.ToDateTime(dr["end_dt"].ToString());

                        oConInfo = GetContractInfoFromBFL(sCid, sSId);
                        report_name = dr["report_name"].ToString();
                        if ((oConInfo == null))
                        {
                            throw new Exception("ScheduleId = " + schedule_id.ToString() + ": No contract info found for contract_id-sub_id = " + sCid + "-" + sSId);
                        }

                        if (dr["app_name"].ToString() == C_ApplicationName_NonPASS)
                        {
                            reportFile = TRSReportHelper.GetAvailableReportFileName(sCid, sSId, report_type_id, C_ApplicationName_NonPASS);
                        }
                        else
                        {
                            reportFile = TRSReportHelper.GetAvailableReportFileName(sCid, sSId, report_type_id, C_ApplicationName);
                        }

                        // Send the notifications....
                        if (reportFile.IndexOf("Error") != -1 || reportFile == "")
                        {
                            throw new Exception("ScheduleId = " + schedule_id.ToString() + ": Report file info not returned for contract_id-sub_id = " + sCid + "-" + sSId + " - Error - " + reportFile);
                        }
                        else
                        {
                            if (report_type_id == (int)BFL.Model.ReportInfo.ReportTypeEnum.PlanDataXlsFile)
                            {
                                reportFile = Path.ChangeExtension(reportFile, "xls");
                            }

                            lstDeliverTo = new List<int>();
                            lstDeliverTo = GetDeliverToIds(schedule_id, ds);

                            sError = SendNotifications(schedule_id, sCid, sSId, report_type_id, report_type_desc, reportFile, plan_name, start_dt, end_dt, oConInfo, lstDeliverTo, report_name);
                            if (sError != "")
                            {
                                sError = "ScheduleId = " + schedule_id.ToString() + " for Report Name: " + report_name + " : Error in SendNotifications - " + sError;
                                oReturn.Errors.Add(new ErrorInfo(-1, sError, ErrorSeverityEnum.ExceptionRaised));
                            }
                            else
                            {
                                _oPASSSchedulerDC.UpdateScheduledReportSetNextDates(schedule_id, C_SCHEDULE_RUN_RESULT_SUCCESS);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex);
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.isException = true;
                        oReturn.confirmationNo = string.Empty;
                        oReturn.Errors.Add(new ErrorInfo(-1, "ScheduleId = " + schedule_id.ToString() + " - " + " for Report Name: " + report_name + " - " + ex.Message + "<br />", ErrorSeverityEnum.ExceptionRaised));
                    }
                }// end for each
            }
            return oReturn;
        }
        private BFL.Model.ReportInfo GetReportInfo(int report_type_id, SOAModel.ContractInfo oContractInfo, string sPartnerUserID, string sLoginID,
                                    string sApplicationName, DateTime start_dt, DateTime end_dt, string report_info, string report_name, string file_format, string subid_MEPClient)
        {
            BFL.Model.ReportInfo oReportInfo = null; ;
            string sSUB_ID = oContractInfo.SubID;
            if (report_info != "")
            {
                oReportInfo = (BFL.Model.ReportInfo)TRSManagers.XMLManager.DeserializeXml(report_info, typeof(BFL.Model.ReportInfo));
            }

            if (oReportInfo == null)
            {
                oReportInfo = new BFL.Model.ReportInfo();
            }

            if (!string.IsNullOrEmpty(subid_MEPClient) && oContractInfo.SubID != subid_MEPClient && oContractInfo.FlagValues.isMEP == true)
            {
                sSUB_ID = subid_MEPClient;
            }
            else
            {
                sSUB_ID = oContractInfo.SubID;
            }

            //Report Type...
            //Changes to Contribution Rate = 4, 45	PASSEnrollment = 69

            oReportInfo.ReportType = report_type_id;
            oReportInfo.CustomReportName = CleanInvalidXmlChars(report_name);
            oReportInfo.ContractID = oContractInfo.ContractID;
            oReportInfo.SubID = sSUB_ID;
            oReportInfo.PlanID = oContractInfo.PartnerPlanID;
            //plan id
            oReportInfo.PartnerUserID = sPartnerUserID;
            //partner user id
            oReportInfo.PartnerID = oContractInfo.PartnerID;
            oReportInfo.UserID = sLoginID;
            oReportInfo.ApplicationName = sApplicationName;

            oReportInfo.StartDate = start_dt.ToString("MM/dd/yyyy");
            oReportInfo.EndDate = end_dt.ToString("MM/dd/yyyy");

            if (file_format.ToUpper() == "CSV")
            {
                oReportInfo.ReportDisplayType = BFL.Model.ReportInfo.ReportDisplayTypeEnum.CSV;
            }
            else if (file_format.ToUpper() == "PDF")
            {
                oReportInfo.ReportDisplayType = BFL.Model.ReportInfo.ReportDisplayTypeEnum.PDF;
            }
            else if (file_format.ToUpper() == "XLS")
            {
                oReportInfo.ReportDisplayType = BFL.Model.ReportInfo.ReportDisplayTypeEnum.XLS;
            }

            if (oContractInfo.PartnerID == "ISC" && file_format == "")// Hardcode to CSV
            {
                oReportInfo.ReportDisplayType = BFL.Model.ReportInfo.ReportDisplayTypeEnum.CSV;
            }

            switch (report_type_id)
            {
                case 69:
                    oReportInfo.ReportDisplayType = BFL.Model.ReportInfo.ReportDisplayTypeEnum.CSV;
                    break;
            }

            if (sApplicationName == C_ApplicationName_TPAADMIN)  //&& sPlanAdminReports.Contains(report_type_id.ToString())
            {
                int iMonthDiff = 0;
                // this is TPAPlanAdmin Report so assign custom file format //CCCCC_SSS_mmddccyy_Q_nnn.PDF //_Q: Quarterly Or _A: Annual
                iMonthDiff = ((end_dt.Year - start_dt.Year) * 12) + end_dt.Month - start_dt.Month;

                if (iMonthDiff == 2 || iMonthDiff == 3 || report_type_id == 71) // quarterly - 71 is special case eventhough its quarterly report in schedule startdate and enddate values are not 3 months apart because this report is snapshot of as of that date
                {
                    oReportInfo.CustomReportName = oContractInfo.ContractID + "_" + sSUB_ID + "_" + end_dt.ToString("MMddyyyy") + "_Q_" + report_type_id.ToString() + "." + oReportInfo.ReportDisplayType;
                }
                else if (iMonthDiff == 11 || iMonthDiff == 12) // Annual
                {
                    oReportInfo.CustomReportName = oContractInfo.ContractID + "_" + sSUB_ID + "_" + end_dt.ToString("MMddyyyy") + "_A_" + report_type_id.ToString() + "." + oReportInfo.ReportDisplayType;
                }
                else
                {
                    oReportInfo.CustomReportName = oReportInfo.CustomReportName + "_" + oContractInfo.ContractID + "_" + sSUB_ID + "_" + report_type_id.ToString() + "_" + DateTime.Now.ToString("MMddyyyy") + "." + oReportInfo.ReportDisplayType;
                }
            }
            else if (oContractInfo.PartnerID == "ISC" && sApplicationName == C_ApplicationName) // CMS_PASS_SCHEDULER
            {
                if (!string.IsNullOrEmpty(oReportInfo.CustomReportName) && oReportInfo.CustomReportName.Length > 50 && oReportInfo.CustomReportName.Substring(0, 50) != null)//Avoid too long filename
                {
                    oReportInfo.CustomReportName = oReportInfo.CustomReportName.Substring(0, 50);
                }
                oReportInfo.CustomReportName = Utils.MakeFileNameValid(oReportInfo.CustomReportName + "_" + oContractInfo.ContractID + sSUB_ID + "_" + report_type_id.ToString() + "_" + start_dt.ToString("MMddyyyy") + "_" + end_dt.ToString("MMddyyyy") + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".CSV"); // Hardcode .CSV for ISC
            }
            else if (sApplicationName == C_ApplicationName_FTP_TRINET)
            {
                if (oContractInfo.ContractID == "932003")
                {
                    oReportInfo.CustomReportName = string.Format("TA_Rate_Change_{0}_{1}.csv", start_dt.ToString("MMddyy"), end_dt.ToString("MMddyy"));
                }
                else if (oContractInfo.ContractID == "341368")
                {
                    oReportInfo.CustomReportName = string.Format("TA_Rate_Change_{0}_{1}_INT.csv", start_dt.ToString("MMddyy"), end_dt.ToString("MMddyy"));
                }
            }
            else if (sApplicationName == C_ApplicationName_FTP_TRINET_SCHEDU)
            {
                if (oReportInfo.StartDate == oReportInfo.EndDate)
                {
                    oReportInfo.CustomReportName = string.Format("{0}-{1}-20-{2}.csv", oContractInfo.ContractID, oContractInfo.SubID.PadRight(5, '0'), DateTime.Now.ToString("yyyyMMdd"));
                }
                else
                {
                    oReportInfo.CustomReportName = string.Format("{0}-{1}-20-{2}M.csv", oContractInfo.ContractID, oContractInfo.SubID.PadRight(5, '0'), end_dt.ToString("yyyyMM"));
                }
            }
            //DDEV-47686
            else if (sApplicationName == C_ApplicationName_FTP_CLCRC)
            {
                oReportInfo.CustomReportName = string.Format("401K_{0}_{1}.csv", oContractInfo.ContractID, end_dt.ToString("MMddyyyy"));
            }
            else if (sApplicationName == C_ApplicationName_FTP_FIBI)
            {
                oReportInfo.CustomReportName = string.Format("{0}_111_{1}.csv", oReportInfo.CustomReportName, DateTime.Now.ToString("yyyyMMddHHmmssfff"));
            }
            //DDEV-47686
            else
            {
                oReportInfo.CustomReportName = "";
            }
            return oReportInfo;
        }
        public static string CleanInvalidXmlChars(string text)
        {
            text = text.Replace("'", "~");
            char[] invalidChars = ['&', '<', '>', '"'];
            int iPosition = -1;
            iPosition = text.IndexOfAny(invalidChars);
            while (iPosition > 0)
            {
                text = text.Replace(text.Substring(iPosition, 1), "~");
                iPosition = text.IndexOfAny(invalidChars);
            }
            //----------------
            return text;
        }
        private string SendNotifications(int schedule_id, string sCid, string sSId, int report_type_id, string report_type_desc, string reportFile,
            string plan_name, DateTime start_date, DateTime end_date, SOAModel.ContractInfo oConInfo, List<int> lstDeliverTo, string report_name)// returns error string if any
        {
            StringBuilder sError = new();
            BFL.Model.E_ContactType eContactType;
            BFL.Model.E_TPACompanyContactType eTPACompanyContactType;
            BFL.Model.E_TPAContactType eTPAContactType;
            string sNames = "";
            string sEmailIds = "";
            string sEmail = "";
            bool otherEmailSent = false;
            string sInloginIds = "";
            string sIndividualIds = "";
            string sNamesAndTitle = "";
            string sLoginIdXML = "";
            string FromAddress = "auto-service@transamerica.com";
            string sPromptName = "";
            XElement elArrayOfInLoginId;
            XElement childElement;
            List<string> inloginOptOuts = new();
            List<KeyValuePair<string, string>> emailCollection = new();
            int SuccessCounter = 0;
            string sTmpError = "";
            string MessageText = GetMessageBody(report_type_id, plan_name, start_date, end_date, report_type_desc);

            string Subject = sCid + "-" + sSId + " - " + report_type_desc;
            if (Subject.ToLower().EndsWith("report") == false)
            {
                Subject = Subject + " Report";
            }

            foreach (int DeliverTo_id in lstDeliverTo)
            {
                try
                {
                    otherEmailSent = false; sLoginIdXML = ""; sEmailIds = ""; sInloginIds = ""; sNames = ""; sNamesAndTitle = ""; sIndividualIds = "";
                    sTmpError = "";
                    switch (DeliverTo_id)
                    {
                        case 1: //Sponsor Message Center
                            eContactType = TRS.IT.SI.BusinessFacadeLayer.Model.E_ContactType.PrimaryContact;
                            TRSReportHelper.GetContactNamesByType(oConInfo, eContactType, ref sNames, ref sEmailIds, ref sInloginIds, ref sIndividualIds);
                            break;

                        case 2: //TPA Message Center
                            TRSReportHelper.GetTpaMsgCenterContactName(oConInfo, ref sNames, ref sNamesAndTitle, ref sEmailIds, ref sInloginIds);
                            break;

                        case 3://PASS Folder // PASS PM Inbox  BSS Bug 1958: Instead of PASS Inbox send it to PASS Folder
                            if (report_type_id != 69)
                            {
                                sTmpError = SendToPASSFolder(sCid, sSId, oConInfo.PartnerID, report_name, reportFile, start_date, end_date);
                            }
                            else
                            {
                                sTmpError = SendToPASSFolderForEnrollmentReport(sCid, sSId, oConInfo.PartnerID, report_name, reportFile, start_date, end_date);
                            }

                            sError.AppendFormat(sTmpError);
                            if (sTmpError == "")
                            {
                                SuccessCounter++;
                            }
                            break;

                        case 4: //Client Company Primary Contact
                            eContactType = TRS.IT.SI.BusinessFacadeLayer.Model.E_ContactType.ClientCompanyPrimaryContact;
                            TRSReportHelper.GetContactNamesByType(oConInfo, eContactType, ref sNames, ref sEmailIds, ref sInloginIds, ref sIndividualIds);
                            break;

                        case 5: //Client Company Executive Contact
                            eContactType = TRS.IT.SI.BusinessFacadeLayer.Model.E_ContactType.ClientCompanyExecutiveContact;
                            TRSReportHelper.GetContactNamesByType(oConInfo, eContactType, ref sNames, ref sEmailIds, ref sInloginIds, ref sIndividualIds);
                            break;

                        case 7: //Customized contact types
                            DataSet dsContactTypes = _oPASSSchedulerDC.GetScheduledReportData_customized(schedule_id);

                            if (dsContactTypes.Tables.Count < 2)
                            {
                                sError.AppendFormat(" Error unexpeced.  No data available for sechedule id {0}", schedule_id.ToString());
                                break;
                            }

                            string emails = TRSCommon.GetData(dsContactTypes, "email_addresses", 0, 0);
                            if (emails.Length > 0)
                            {
                                try
                                {
                                    string emailList = emails.Replace(",", ";");
                                    if (reportFile.Length > 0)
                                    {
                                        TRS.IT.SI.Services.wsNotification.MessageAttachment[] attachments = [new TRS.IT.SI.Services.wsNotification.MessageAttachment()];
                                        attachments[0].Name = report_name + "_" + Path.GetFileName(reportFile);
                                        attachments[0].DateCreated = DateTime.Now;
                                        attachments[0].Content = File.ReadAllBytes(reportFile);
                                        sTmpError = SendZixReportEmail(sCid, sSId, emailList, Subject, report_type_id, plan_name, start_date, end_date, attachments);
                                        if (sTmpError == "")
                                        {
                                            otherEmailSent = true;
                                            SuccessCounter++;
                                        }
                                    }
                                    else
                                    {
                                        sError.AppendFormat(" Error in case 7 Customized contact types generating attachment for SendMailZixReport for {0}-{1} {2} ", sCid, sSId, "<br />");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Utils.LogError(ex);
                                    sError.AppendFormat(" Error in case 7 Customized contact types for {0}-{1} with exception: {2} {3} ", sCid, sSId, ex.Message, "<br />");
                                }
                            }

                            List<KeyValuePair<string, string>> emailCollectionClient = new();
                            List<KeyValuePair<string, string>> emailCollectionTpa = new();
                            List<string> clientOptOuts = new();
                            for (int i = 0; i < dsContactTypes.Tables[1].Rows.Count; i++)
                            {
                                if (TRSCommon.GetData(dsContactTypes, "contact_type", 1, i) == "client")
                                {
                                    eContactType = (BFL.Model.E_ContactType)Convert.ToInt32(TRSCommon.GetData(dsContactTypes, "contact_type_id", 1, i));
                                    emailCollectionClient = TRSReportHelper.GetContactEmailsByType(oConInfo, eContactType);
                                    if (emailCollectionClient.Count > 0)
                                    {
                                        emailCollection.AddRange(emailCollectionClient);
                                    }
                                }
                                else if (TRSCommon.GetData(dsContactTypes, "contact_type", 1, i) == "tpa")
                                {
                                    eTPACompanyContactType = (BFL.Model.E_TPACompanyContactType)Convert.ToInt32(TRSCommon.GetData(dsContactTypes, "contact_type_id", 1, i));
                                    emailCollectionTpa = TRSReportHelper.GetTpaContactEmails(oConInfo, eTPACompanyContactType);
                                    if (emailCollectionTpa.Count > 0)
                                    {
                                        emailCollection.AddRange(emailCollectionTpa);
                                    }
                                }
                                else if (TRSCommon.GetData(dsContactTypes, "contact_type", 1, i) == "tpa_assigned")
                                {
                                    eTPAContactType = (BFL.Model.E_TPAContactType)Convert.ToInt32(TRSCommon.GetData(dsContactTypes, "contact_type_id", 1, i));
                                    emailCollectionTpa = TRSReportHelper.GetAssignedTpaContactEmails(oConInfo, eTPAContactType);
                                    if (emailCollectionTpa.Count > 0)
                                    {
                                        emailCollection.AddRange(emailCollectionTpa);
                                    }
                                }
                            }

                            //Get optOuts
                            string user_id = "";
                            for (int i = 0; i < dsContactTypes.Tables[2].Rows.Count; i++)
                            {
                                user_id = TRSCommon.GetData(dsContactTypes, "user_id", 2, i);
                                if (TRSCommon.GetData(dsContactTypes, "user_id_type", 2, i) == "in_login_id")
                                {
                                    if (!inloginOptOuts.Contains(user_id))
                                    {
                                        inloginOptOuts.Add(user_id);
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }

                    if (DeliverTo_id != 3)
                    {

                        if (!string.IsNullOrEmpty(sInloginIds) || emailCollection.Count > 0)
                        {
                            //send to MessageCEnter
                            Hashtable hsUsers = new();

                            //Remove duplicates
                            if (sInloginIds.Length > 0)
                            {
                                string[] sAryInloginIds = sInloginIds.Split([';', ',']);
                                string[] sAryEmails = sEmailIds.Split([';', ',']);

                                for (int i = 0; i < sAryInloginIds.Count(); i++)
                                {
                                    if (hsUsers.ContainsKey(sAryInloginIds[i]) == false)
                                    {
                                        hsUsers.Add(sAryInloginIds[i], sAryEmails[i]);
                                    }
                                }
                            }

                            foreach (KeyValuePair<string, string> pair in emailCollection)
                            {
                                if (hsUsers.ContainsKey(pair.Key) == false)
                                {
                                    hsUsers.Add(pair.Key, pair.Value);
                                }
                            }

                            //Remove optouts
                            foreach (string inloginId in inloginOptOuts)
                            {
                                if (hsUsers.ContainsKey(inloginId))
                                {
                                    hsUsers.Remove(inloginId);
                                }
                            }

                            //Prepare the attachment
                            SI.Services.wsMessage.Attachment[] oAttachment = new SI.Services.wsMessage.Attachment[1];
                            sPromptName = Path.GetFileName(reportFile);
                            if ((report_name + "_" + sPromptName).Length < 100)//avoid very long file name
                            {
                                sPromptName = report_name + "_" + sPromptName;
                            }
                            sPromptName = Utils.MakeFileNameValid(sPromptName);
                            try
                            {
                                byte[] RawData = File.ReadAllBytes(reportFile);
                                oAttachment[0] = new SI.Services.wsMessage.Attachment();
                                oAttachment[0].Data = Convert.ToBase64String(RawData);
                                oAttachment[0].PromptFileName = sPromptName;
                            }
                            catch (Exception ex)
                            {
                                Utils.LogError(ex);
                                sError.AppendFormat(" Exception in Prepare the attachment reportFile={0}, scheduledId={1} {2} {3}", reportFile, schedule_id, ex.Message, "<br />");
                                return sError.ToString();
                            }

                            foreach (DictionaryEntry user in hsUsers)
                            {
                                sLoginIdXML = "";
                                elArrayOfInLoginId = new XElement("ArrayOfInLoginId");
                                childElement = new XElement("InLoginId", (string)user.Key);
                                elArrayOfInLoginId.Add(childElement);
                                sLoginIdXML = elArrayOfInLoginId.ToString();

                                ResultReturn oRet = SendToMessageCenter(sLoginIdXML, FromAddress, Subject, MessageText, oConInfo.PartnerID, sCid, sSId, oAttachment);

                                if (oRet.returnStatus == ReturnStatusEnum.Failed)
                                {
                                    foreach (ErrorInfo e in oRet.Errors)
                                    {
                                        sError.Append(e.errorDesc + "<br />");
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        SuccessCounter++;
                                        sEmail = (string)user.Value;// assuming number of elements same in all arrays (sAryInloginIds, sAryEmails, sAryNames)...
                                        if (sEmail != "")
                                        {
                                            ResultReturn oRet1 = SendEmailNotification(sEmail, sCid, sSId);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Utils.LogError(ex);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (DeliverTo_id == 7 && otherEmailSent == true)
                            {
                            }
                            else
                            {
                                sError.AppendFormat(" Messagecenter message was not sent because no inloginIds found for delivery type = {0} for schedule id = {1} for Report Name: {2} {3}", DeliverTo_id.ToString(), schedule_id.ToString(), report_name, "<br />");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    sError.AppendFormat("Execption detail: {0} {1}", ex.Message, "<br />");
                }

            }
            if (SuccessCounter > 0)
            {
                if (sError.Length > 0)
                {
                    SendErrorMsgEmail("PASSScheduler.SendNotifications warnings For: " + Subject, "Atleast one or more of the recepients were notified successfully but other/s failed.   " + sError.ToString());
                }

                return string.Empty;
            }
            else
            {
                return sError.ToString();
            }
        }
        private List<int> GetDeliverToIds(int schedule_id, DataSet ds)
        {
            List<int> lstDeliverTo = new();
            DataView dv = new();

            if (ds != null && ds.Tables.Count > 1)
            {
                dv = ds.Tables[1].DefaultView;
                dv.RowFilter = "schedule_id = " + schedule_id.ToString();
                foreach (DataRowView drv in dv)
                {
                    lstDeliverTo.Add(Convert.ToInt32(drv["DeliverTo_id"].ToString()));
                }
            }
            return lstDeliverTo;
        }
        private ResultReturn SendToMessageCenter(string sLoginIdXML, string FromAddress, string a_sSubject, string a_sBody, string a_sPartnerId, string sCid, string sSId, SI.Services.wsMessage.Attachment[] oAttachments)
        {
            ResultReturn oReturn = new();
            GeneralDC oGenDC = new();
            int iBendInLoginId = 0;

            iBendInLoginId = oGenDC.GetMsgCtrAcctByExLoginId("BendProcess");

            MsgCenterMessageService oWebMsgAdaptor = new();
            SI.Services.wsMessage.webMessage oWebMsgCtr = new();
            SI.Services.wsMessage.MsgData oWebMsgData = new();
            if (oAttachments != null)
            {
                oWebMsgData.Attachments = oAttachments;
            }
            oWebMsgData.ReplyAllowed = false;
            oWebMsgData.Body = a_sBody;
            oWebMsgCtr.Subject = a_sSubject;
            oWebMsgCtr.AttachmentCount = 1;
            oWebMsgCtr.MsgData = oWebMsgData;
            oWebMsgCtr.FromAddress = FromAddress;
            oWebMsgCtr.MsgSource = "System Back-end";
            oWebMsgCtr.CreateBy = iBendInLoginId.ToString();
            oWebMsgCtr.CreateDt = DateTime.Now.ToString();
            oWebMsgCtr.ExpireDt = DateTime.Now.AddDays(90).ToString();
            oWebMsgCtr.SendNotification = "N";
            oWebMsgCtr.FolderId = 1;// inbox
            oWebMsgCtr.MsgType = "0";
            oWebMsgCtr.SenderInLoginId = iBendInLoginId.ToString();

            oReturn = oWebMsgAdaptor.SendMessageCenterMessage(sLoginIdXML, oWebMsgCtr);
            return oReturn;
        }
        private string GetMessageBody(int report_type_id, string plan_name, DateTime start_date, DateTime end_date, string report_type_desc)
        {
            StringBuilder strMsgBody = new();
            switch (report_type_id)
            {
                case 4:
                case 45:
                    strMsgBody.Append("</br> Attached is the Changes to Contribution Rate report for ");
                    strMsgBody.AppendFormat("{0}. This file includes enrollments and/or changes received between ", plan_name);
                    strMsgBody.AppendFormat("{0} and {1}", start_date.ToShortDateString(), end_date.ToShortDateString());
                    strMsgBody.Append(". If the report is blank, then you can assume there were no enrollments or changes during this period.");
                    strMsgBody.Append("</br> </br>");
                    strMsgBody.Append("Please contact your PASS administrator if you have any questions.");
                    strMsgBody.Append("</br> </br>");
                    strMsgBody.Append("Thank you, </br> </br>");
                    strMsgBody.Append("Transamerica Retirement Solutions");
                    break;
                case 69:
                    strMsgBody.Append("</br> Attached is the eligibility report for ");
                    strMsgBody.AppendFormat("{0}. This file includes newly eligible participants for the plan entry date of ", plan_name);
                    strMsgBody.Append(end_date.ToShortDateString());
                    strMsgBody.Append(". If the report is blank, you can assume there are no new eligible participants this period.");
                    strMsgBody.Append("</br> </br>");
                    strMsgBody.Append("Please contact your PASS administrator if there are any discrepancies or changes needed.");
                    strMsgBody.Append("</br> </br>");
                    strMsgBody.Append("Thank you, </br> </br>");
                    strMsgBody.Append("Transamerica Retirement Solutions");
                    break;
                case 71:
                    strMsgBody.Append("</br> Attached is the loan register report for ");
                    strMsgBody.AppendFormat("{0}. This file includes newly loans registered as of ", plan_name);
                    strMsgBody.Append(end_date.ToShortDateString());
                    strMsgBody.Append(". If the report is blank, you can assume there are no loans registered this period.");
                    strMsgBody.Append("</br> </br>");
                    strMsgBody.Append("Please contact your PASS administrator if there are any discrepancies or changes needed.");
                    strMsgBody.Append("</br> </br>");
                    strMsgBody.Append("Thank you, </br> </br>");
                    strMsgBody.Append("Transamerica Retirement Solutions");
                    break;
                default:
                    strMsgBody.AppendFormat("</br> Attached is the {0} for ", report_type_desc);
                    strMsgBody.AppendFormat("{0}. This file includes updates received between ", plan_name);
                    strMsgBody.AppendFormat("{0} and {1}", start_date.ToShortDateString(), end_date.ToShortDateString());
                    strMsgBody.Append(". If the report is blank, then you can assume there were no changes during this period.");
                    strMsgBody.Append("</br> </br>");
                    strMsgBody.Append("Please contact your administrator if you have any questions.");
                    strMsgBody.Append("</br> </br>");
                    strMsgBody.Append("Thank you, </br> </br>");
                    strMsgBody.Append("Transamerica Retirement Solutions");
                    break;
            }
            return strMsgBody.ToString();
        }
        private string SendZixReportEmail(string cid, string sid, string emails, string subject, int report_type_id, string plan_name, DateTime start_date, DateTime end_date, TRS.IT.SI.Services.wsNotification.MessageAttachment[] attachments)
        {
            ResultReturn oResults = new();
            oResults.returnStatus = ReturnStatusEnum.Succeeded;

            string sError = "";
            MessageServiceKeyValue[] emailVariables = new MessageServiceKeyValue[5];
            emailVariables[0] = new MessageServiceKeyValue();
            emailVariables[0].key = "email_list";
            emailVariables[0].value = emails;
            emailVariables[1] = new MessageServiceKeyValue();
            emailVariables[1].key = "subject";
            emailVariables[1].value = subject;
            emailVariables[2] = new MessageServiceKeyValue();
            emailVariables[2].key = "plan_name";
            emailVariables[2].value = plan_name;
            emailVariables[3] = new MessageServiceKeyValue();
            emailVariables[3].key = "start_date";
            emailVariables[3].value = start_date.ToShortDateString();
            emailVariables[4] = new MessageServiceKeyValue();
            emailVariables[4].key = "end_date";
            emailVariables[4].value = end_date.ToShortDateString();

            switch (report_type_id)
            {
                case 4:
                case 45:
                    oResults = MS_SendMail(2870, cid, sid, "Eligibility report email", emailVariables, attachments);
                    break;

                case 69:
                    oResults = MS_SendMail(2880, cid, sid, "Contribution rate report email", emailVariables, attachments);
                    break;

                default:
                    break;
            }

            if (oResults.returnStatus == ReturnStatusEnum.Failed)
            {
                sError = oResults.Errors[0].errorDesc;
            }
            return sError;
        }
        private ResultReturn MS_SendMail(int templateID, string cid, string sid, string sourceName, MessageServiceKeyValue[] emailVariables, TRS.IT.SI.Services.wsNotification.MessageAttachment[] attachments)
        {
            ResultReturn oResults = new();
            try
            {
                MessageService oMS = new();
                oResults = oMS.SendMessage(cid, sid, templateID, emailVariables, sourceName, attachments);

                if (oResults == null)
                {
                    ErrorInfo oError = new();
                    oResults = new ResultReturn();
                    oError.errorNum = 0;
                    oResults.returnStatus = ReturnStatusEnum.Succeeded;
                    oResults.Errors.Add(oError);
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oResults.Errors.Add(new ErrorInfo(-1, "Contract: " + cid + "-" + sid + " TepmplateId:" + templateID.ToString() + " Ex: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
                oResults.returnStatus = ReturnStatusEnum.Failed;
            }
            return oResults;
        }
        private ResultReturn SendEmailNotification(string sEmail, string sCid, string sSId)
        {
            MessageServiceKeyValue[] Keys;
            MessageService oMS = new();
            ResultReturn oResults = null;
            const int C_MsgId_NotifyReportAvaialbe = 1950;

            Keys = new MessageServiceKeyValue[1];
            Keys[0] = new MessageServiceKeyValue();
            Keys[0].key = "to_email";
            Keys[0].value = sEmail;

            oResults = oMS.SendMessage(sCid, sSId, C_MsgId_NotifyReportAvaialbe, Keys, "BendProcessor/PASSScheduler");

            if (oResults == null)
            {
                ErrorInfo oError = new();
                oResults = new ResultReturn();
                oError.errorNum = 1;
                oError.errorDesc = "No result returned.";
            }
            return oResults;
        }
        private string SendToPASSFolder(string sCid, string sSId, string a_sPartnerId, string a_sReport_name, string a_sFilePathName, DateTime start_date, DateTime end_date)
        {
            string sError = "";
            string sErrorReturn = "";
            byte[] RawData;
            string sPASSFolderPath = "";
            string sNewFileName = "";
            try
            {
                RawData = File.ReadAllBytes(a_sFilePathName); // just read the file in bytes here
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                sErrorReturn = " Exception while reading the file from source -" + ex.Message;
                return sErrorReturn;
            }
            try
            {
                sNewFileName = sCid + "-" + sSId + "-" + a_sReport_name + " - " + start_date.ToString("MM-dd-yy") + " To " + end_date.ToString("MM-dd-yy") + " - " + DateTime.Now.ToString("dd hhmmss.fff") + Path.GetExtension(a_sFilePathName);
                sNewFileName = Utils.MakeFileNameValid(sNewFileName);

                sPASSFolderPath = AppSettings.GetValue("PASSFolderPath");

                if (sPASSFolderPath == null || sPASSFolderPath == "" || Directory.Exists(sPASSFolderPath) == false)
                {
                    sErrorReturn = " Invalid PASSFolderPath config setting - " + sPASSFolderPath;
                    return sErrorReturn;
                }

                sPASSFolderPath = Path.Combine(sPASSFolderPath, DateTime.Now.Year.ToString());
                if (!Directory.Exists(sPASSFolderPath))
                {
                    Directory.CreateDirectory(sPASSFolderPath);
                }

                sPASSFolderPath = Path.Combine(sPASSFolderPath, DateTime.Now.ToString("MMMM"));
                if (!Directory.Exists(sPASSFolderPath))
                {
                    Directory.CreateDirectory(sPASSFolderPath);
                }

                File.WriteAllBytes(Path.Combine(sPASSFolderPath, sNewFileName), RawData);
            }
            catch (Exception ex1)
            {
                Utils.LogError(ex1);
                sErrorReturn = " Exception while copying the file tp PASS Folder -" + ex1.Message;
                if (MoveFileToBackupPASSFolder(RawData, sNewFileName, ref sError) == false)
                {
                    sErrorReturn = sErrorReturn + " AND " + sError;
                }
            }
            return sErrorReturn;
        }
        private string SendToPASSFolderForEnrollmentReport(string sCid, string sSId, string a_sPartnerId, string a_sReport_name, string a_sFilePathName, DateTime start_date, DateTime end_date)
        {
            string sError = "";
            string sErrorReturn = "";
            string[] fileData;
            string[] results;

            byte[] RawData;
            string sPASSFolderPath = "";
            string sNewFileName = "";
            try
            {
                fileData = File.ReadAllLines(a_sFilePathName);
                results = new string[fileData.Length];
                for (int i = 0; i < fileData.Length; i++)
                {
                    results[i] = fileData[i].Replace("=", "");
                }

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                sErrorReturn = " Exception while reading the file from source -" + ex.Message;
                return sErrorReturn;
            }
            try
            {
                sNewFileName = sCid + "-" + sSId + "-" + a_sReport_name + " - " + start_date.ToString("MM-dd-yy") + " To " + end_date.ToString("MM-dd-yy") + " - " + DateTime.Now.ToString("dd hhmmss.fff") + "_Participant_Count_" + (fileData.Length - 1).ToString() + Path.GetExtension(a_sFilePathName);
                sNewFileName = Utils.MakeFileNameValid(sNewFileName);

                sPASSFolderPath = AppSettings.GetValue("PASSFolderPath");

                if (sPASSFolderPath == null || sPASSFolderPath == "" || Directory.Exists(sPASSFolderPath) == false)
                {
                    sErrorReturn = " Invalid PASSFolderPath config setting - " + sPASSFolderPath;
                    return sErrorReturn;
                }

                sPASSFolderPath = Path.Combine(sPASSFolderPath, DateTime.Now.Year.ToString());
                if (!Directory.Exists(sPASSFolderPath))
                {
                    Directory.CreateDirectory(sPASSFolderPath);
                }

                sPASSFolderPath = Path.Combine(sPASSFolderPath, DateTime.Now.ToString("MMMM"));
                if (!Directory.Exists(sPASSFolderPath))
                {
                    Directory.CreateDirectory(sPASSFolderPath);
                }

                File.WriteAllLines(Path.Combine(sPASSFolderPath, sNewFileName), results);

            }
            catch (Exception ex1)
            {
                Utils.LogError(ex1);
                sErrorReturn = " Exception while copying the file tp PASS Folder -" + ex1.Message;
                RawData = File.ReadAllBytes(a_sFilePathName);
                if (MoveFileToBackupPASSFolder(RawData, sNewFileName, ref sError) == false)
                {
                    sErrorReturn = sErrorReturn + " AND " + sError;
                }
            }
            return sErrorReturn;
        }
        private bool MoveFileToBackupPASSFolder(byte[] RawData, string sNewFileName, ref string sError)
        {
            bool bRet = false;
            string sBackupPASSFolder = string.Empty;
            string sBackupPASSFileName = string.Empty;
            string sTempFileName = string.Empty;
            try
            {
                sError = "";
                sBackupPASSFolder = AppSettings.GetValue("BackupPASSFolder");

                if (sBackupPASSFolder == null || sBackupPASSFolder == string.Empty)
                {
                    sBackupPASSFolder = "E:\\BendProcessor\\Scheduler Reports\\";
                }
                sBackupPASSFolder = Path.Combine(sBackupPASSFolder, DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MMMM"));
                Utils.ValidatePath(sBackupPASSFileName);
                File.WriteAllBytes(Path.Combine(sBackupPASSFolder, sNewFileName), RawData); // this function 

                bRet = true;
            }
            catch (Exception ex1)
            {
                Utils.LogError(ex1);
                sError = " Unable to move file " + sNewFileName + " To back up folder. " + sBackupPASSFolder + " Exception: " + ex1.Message;
                bRet = false;
            }
            return bRet;
        }
        public string GetKeyValue(string sKey, List<SOAModel.KeyValue> oKeyValuePair)
        {
            string strValue = "";
            if ((oKeyValuePair != null))
            {
                var KeyVal = (from kv in oKeyValuePair
                              where kv.key.ToLower() == sKey.ToLower()
                              select kv.value).FirstOrDefault();

                if ((KeyVal != null))
                {
                    strValue = KeyVal.ToString();
                }

            }
            return strValue;
        }
        private ResultReturn CreateTpaPlanAdminReportsSchedulesMigrated(string sContractsToInsertSchedulesXML)
        {
            ResultReturn oReturn = new();
            try
            {
                int i = _oPASSSchedulerDC.InsertBulkTPAPlanAdminScheduledReportData(sContractsToInsertSchedulesXML);
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateTpaPlanAdminReportsSchedules: -  sContractsToInsertSchedulesXML : " + sContractsToInsertSchedulesXML + "  Error:" + ex.Message + "<br />", ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }
        private ResultReturn DeleteTpaPlanAdminReportsSchedulesMigrated(string sSchedulessToDeleteXML)
        {
            ResultReturn oReturn = new();
            try
            {
                int i = _oPASSSchedulerDC.DeleteBulkTPAPlanAdminScheduledReportData(sSchedulessToDeleteXML);
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in DeleteTpaPlanAdminReportsSchedules: - sSchedulessToDeleteXML : " + sSchedulessToDeleteXML + "  Error:" + ex.Message + "<br />", ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }
        public TaskStatus ProcessFIBIScheduledRptMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;

            const string C_Task = "ProcessFIBIScheduledRpt";

            DataSet ds = new();

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    ds = GetScheduledReportsByAppName(C_ApplicationName_FTP_FIBI, false);
                    oReturn = RunScheduledReports(ds);
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    else
                    {
                        oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
    }
}
