using DailyAuditPackageISCBatch;
using DailyeConfirmMainBatch;
using DailyPXNotificationPENCOBatch;
using DailyVendorNotificationBatch;
using FwApprovalsNotificationBatch;
using FWBamlDocsToWmsBatch;
using FWFundLineupUpdatesBatch;
using FWFundSummaryNotifyBatch;
using FWFundUpdatesToISCBatch;
using FWInitialFundUpdatesBatch;
using FWSignedDocsToMsgcntrBatch;
using FWUpdateRKPartner;
using HardshipLiftReport;
using Microsoft.Data.SqlClient;
using PlanComplianceResultBatch;
using ProcesseStatementPENCOContinuousBatch;
using ProcessRequiredNoticesProcBatch;
using ReminderNotificationBatch;
using SIUtil;
using System.Data;
using TarPlanAdminReportBatch;
using TarPptBouncedEmailBatch;
using TarReportGenerationBatch;
using TARSharedUtilLib.Utility;
using TpaUsersCreateBatch;
using TRS.IT.BendProcessor.Util;
using TRS.IT.SharedLib.PlatformIntegration;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace bend_fund_wizard_poc
{
    class Program : BaseConsoleApplication
    {
        private const string FWApprovalsNotificationBatch = "FWReminderNotification";
        private const string FWUpdatePartner = "FWUpdatePartner";
        private const string FWBAMLDocGen = "FWBAMLDocGen";
        private const string FWImageToWMS = "FWImageToWMS";
        private const string FWPegasysUpdate = "FWPegasysUpdate"; 
        private const string FWFundRiderToMsgcntrBatch = "FWSendFundRiderToMC";
        private const string DailyAuditPackageISCBatch = "DailyAuditPackageISC";
        private const string FWDailySummaryNotificationBatch = "FWDailySummaryNotification";
        private const string DailyPXNotificationPENCOBatch = "DailyPXNotificationPENCO";
        private const string ProcesseStatementPENCOContinuous = "ProcesseStatementPENCOContinuous";
        private const string FWUpdateISC = "FWUpdateISC";
        private const string intInitialPegasysUpdate = "intInitialPegasysUpdate";
        private const string FWInterimTasksISC = "FWInterimTasksISC";
        private const string ProcessCreateTpaLiteIds = "ProcessCreateTpaLiteIds";
        private const string HardshipLiftReport = "HardshipLiftRpt";
        private const string DailyProcessTestingResultsTAE = "DailyProcessTestingResultsTAE";
        private const string ProcessParticipantReqdNotices = "ProcessParticipantReqdNotices";
        private const string ProcessPptReqdNoticesISC = "ProcessPptReqdNoticesISC";
        private const string DailyConfirmMainBatch = "DailyeConfirmMain";
        private const string DailyVendorNotification = "DailyVendorNotification";
        private const string ReminderNotification = "ReminderNotification";
        private const string TarPlanAdminReportBatch = "TpaPlanAdminReportsISC";
        private const string ScheduleRptRun = "ScheduleRpt_Run";
        private const string TarPptBouncedEmailBatch = "DailyeStatementProcessBouncedEmail";

        public static string? LicenseFile { get; set; }
        public static void Main(string[] args)
        {
            new Program().Run(args);
        }
        protected override string GetApplicationName() => "Bend Fund Wizard POC";
        protected override void Execute()
        {
            switch (ApplicationName)
            {
                case "Bend Fund Wizard POC":
                    TestConfigurationRetrieval();
                    RunExistingLogic();
                    break;
                case FWApprovalsNotificationBatch:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new Fwapprovalsnotificationbatch().Run(FWApprovalsNotificationBatch);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                    case FWUpdatePartner:
                        try
                        {
                            Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                            new FWUpdatePartnerJobRunner().Run(FWUpdatePartner);
                            Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                        }
                    break;
                    case FWBAMLDocGen:
                        try
                        {
                            Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                            new FWBAMLDocGenJob().Run(FWBAMLDocGen);
                            Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        }

                        catch (Exception ex)
                        {
                            Logger.LogMessage("Unable to run job: " + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                        }
                    break;
                case FWImageToWMS:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new FWFundDocsToWMSBatch.FWFundDocsToWMSBatch().Run(FWImageToWMS);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case FWPegasysUpdate:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new FWPegasysUpdateJob().Run(FWPegasysUpdate);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }

                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job: " + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }

                    break;
                case FWFundRiderToMsgcntrBatch:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new FWFundRiderToMsgcntrBatch.FWFundRiderToMsgcntrBatch().Run(FWFundRiderToMsgcntrBatch);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case DailyAuditPackageISCBatch:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new DailyAuditPackageISC().Run(DailyAuditPackageISCBatch);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case FWDailySummaryNotificationBatch:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new Fwfundsummarynotifybatch().Run(FWDailySummaryNotificationBatch);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case DailyPXNotificationPENCOBatch:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new DailyPXNotificationPENCO().Run(DailyPXNotificationPENCOBatch);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case ProcesseStatementPENCOContinuous:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new ProcesseStatementPENCOContinuous().Run(ProcesseStatementPENCOContinuous);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case FWUpdateISC:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new FWUpdateISC().Run(FWUpdateISC);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }

                    break;
                case intInitialPegasysUpdate:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new fwInitialfundUpdates().Run(intInitialPegasysUpdate);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }

                    break;
                case FWInterimTasksISC:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new FWInterimTasksISC().Run(FWInterimTasksISC);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case ProcessCreateTpaLiteIds:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new ProcessCreateTpaLiteIds().Run(ProcessCreateTpaLiteIds);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case HardshipLiftReport:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new HardshipLiftRpt().Run(HardshipLiftReport);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case DailyProcessTestingResultsTAE:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new DailyProcessTestingResultsTAE().Run(DailyProcessTestingResultsTAE);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }

                    break;
                case ProcessParticipantReqdNotices:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new ProcessParticipantReqdNotices().Run(ProcessParticipantReqdNotices);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case ProcessPptReqdNoticesISC:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new RequiredNoticesProcBatch.ProcessPptReqdNoticesISC().Run(ProcessPptReqdNoticesISC);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case DailyConfirmMainBatch:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new DailyeConfirmMain().Run(DailyConfirmMainBatch);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case DailyVendorNotification:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new DailyVendorNotification().Run(DailyVendorNotification);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                 case ReminderNotification:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new ReminderNotification().Run(ReminderNotification);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case TarPlanAdminReportBatch:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new TpaPlanAdminReportsISC().Run(TarPlanAdminReportBatch);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                    break;
                case ScheduleRptRun:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new ScheduleRptRun().Run(ScheduleRptRun);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }

                    break;
                case TarPptBouncedEmailBatch:
                    try
                    {
                        Logger.LogMessage("Begin Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        new DailyeStatementProcessBouncedEmail().Run(TarPptBouncedEmailBatch);
                        Logger.LogMessage("End Job Execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogMessage("Unable to run job:" + ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }

                    break;
                default:
                    Logger.LogMessage($"Unknown Application Name: {ApplicationName}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    break;
            }            
        }
        private static void TestConfigurationRetrieval()
        {
            try
            {
                Logger.LogMessage("Testing Configuration Retrieval using AppSettings...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                string springConfigUri = AppSettings.GetValue("SPRING_CLOUD_CONFIG_URI");
                string springProfiles = AppSettings.GetValue("SPRING_PROFILES_ACTIVE");
                string artifactId = AppSettings.GetValue("ARTIFACT_ID");
                string vaultHost = AppSettings.GetValue("SPRING_CLOUD_VAULT_HOST");

                Logger.LogMessage($"Environment Variables from Docker:", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                Logger.LogMessage($"  SPRING_CLOUD_CONFIG_URI: {springConfigUri}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                Logger.LogMessage($"  SPRING_PROFILES_ACTIVE: {springProfiles}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                Logger.LogMessage($"  ARTIFACT_ID: {artifactId}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                Logger.LogMessage($"  SPRING_CLOUD_VAULT_HOST: {vaultHost}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                
                string fwBendEmailAddr = AppSettings.GetValue("FWBendEmailAddr");

                Logger.LogMessage($"AppSettings.json Values:", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                Logger.LogMessage($"  FWBendEmailAddr: {fwBendEmailAddr}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                string connectionString = AppSettings.GetConnectionString("ConnectString");
                Logger.LogMessage($"Connection String exists: {!string.IsNullOrEmpty(connectionString)}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                string configUriFromEnv = AppSettings.GetValue("SPRING_CLOUD_CONFIG_URI");
                Logger.LogMessage($"Config URI (should come from environment): {configUriFromEnv}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("Unable to test configurations" + ex.StackTrace, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
        }
        private static void RunExistingLogic()
        {
            try
            {
                Logger.LogMessage("Begin FTP Connection Testing...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                TestFtpOperations();
                Logger.LogMessage("End FTP Connection Testing...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("Error in FTP connection testing" + ex.Message + "StackTrace: " + ex.StackTrace, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
            try
            {
                Logger.LogMessage("Begin SendErrorEmailToUsers...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                SendErrorEmailToUsers();
                Logger.LogMessage("End SendErrorEmailToUsers...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("Error in SendErrorEmailToUsers" + ex.Message + "StackTrace: " + ex.StackTrace, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
            try
            {
                Logger.LogMessage("Begin SQL Connection...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                CallStoredProcedure();
                Logger.LogMessage("End SQL Connection...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("Error in SQL connection" + ex.StackTrace, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }

            try
            {
                Logger.LogMessage("Begin SOAP Service Connection...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                CallSoapService();
                Logger.LogMessage("End SOAP Service Connection...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("SOAP Service Connection Error " + ex.StackTrace, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }

            try
            {
                string licenseFileName = "Aspose.Total.lic";
                LicenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, licenseFileName);
                Logger.LogMessage($"License file path: {LicenseFile}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("ID", typeof(int));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("Description", typeof(string));
                dataTable.Columns.Add("Date", typeof(DateTime));
                dataTable.Columns.Add("Amount", typeof(decimal));

                dataTable.Rows.Add(1, "Fund A", "Growth Fund", DateTime.Now, 1250.75);
                dataTable.Rows.Add(2, "Fund B", "Income Fund", DateTime.Now.AddDays(-30), 850.50);
                dataTable.Rows.Add(3, "Fund C", "Balanced Fund", DateTime.Now.AddDays(-60), 2340.25);
                dataTable.Rows.Add(4, "Fund D", "Index Fund", DateTime.Now.AddDays(-90), 1675.80);
                dataTable.Rows.Add(5, "Fund E", "Bond Fund", DateTime.Now.AddDays(-120), 945.60);

                WriteFundChangesSummaryFile(dataTable, "Test");
                Logger.LogMessage($"File Created Successfully ", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("Aspose Error" + ex.StackTrace, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
            SMBUsageExample.DemonstrateFileOperations();
        }

        private static void WriteFundChangesSummaryFile(DataTable dtFinal, string sAppendFileName)
        {
            var workbook = new Aspose.Cells.Workbook();
            SetCellsLicense();
            var sheet = workbook.Worksheets[0];

            Aspose.Cells.ImportTableOptions importTableOptions = new Aspose.Cells.ImportTableOptions();
            importTableOptions.IsFieldNameShown = true;

            int row = 1;
            int column = 0;

            sheet.Cells.ImportData(dtFinal, row, column, importTableOptions);
            try
            {
                var style = workbook.CreateStyle();
                style.Custom = "mm/dd/yyyy";

                var styleFlag = new Aspose.Cells.StyleFlag();
                styleFlag.NumberFormat = true;

                sheet.Cells.Columns[3].ApplyStyle(style, styleFlag);
                sheet.Cells.Columns[4].ApplyStyle(style, styleFlag);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("Error in font formatting" + ex.StackTrace, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }

            var fileName = $"FundChangesSummary-{sAppendFileName}.xlsx";
            string outputBasePath = AppDomain.CurrentDomain.BaseDirectory;
            var localPath = Path.Combine(outputBasePath, fileName);

            workbook.Save(localPath);
            AutofitExcelColumnsClosedXML(localPath);
            Logger.LogMessage($"Saved locally to {localPath}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

            try
            {
                string smbFolder = AppSettings.GetValue("SMBOutputFolder");
                FileManagerSMB.Copy(localPath, $"{smbFolder}{fileName}");
                Logger.LogMessage("Successfully saved to SMB location", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Failed to save to SMB location: {ex.Message}" + ex.StackTrace, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
        }

        private static void AutofitExcelColumnsClosedXML(string localPath)
        {
            using (var workbook = new ClosedXML.Excel.XLWorkbook(localPath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    worksheet.Columns().AdjustToContents();
                }

                workbook.Save();
            }
            Logger.LogMessage($"Autofit applied to Excel file using ClosedXML", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
        }

        private static void SetCellsLicense()
        {
            var license = new Aspose.Cells.License();

            string licenseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Aspose.Total.lic");

            if (System.IO.File.Exists(licenseFilePath))
            {
                Logger.LogMessage($"License file found at: {licenseFilePath}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                license.SetLicense(licenseFilePath);
                Logger.LogMessage("License applied successfully", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            else
            {
                Logger.LogMessage($"License file not found at: {licenseFilePath}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
        }

        private static void CallStoredProcedure()
        {
            try
            {
                string connectionString = AppSettings.GetConnectionString("ConnectString");
                string contractId = AppSettings.GetValue("TestContractId");
                string subId = AppSettings.GetValue("TestSubId");
                string storedProcName = "fwp_GetFWPendingByContract";

                Logger.LogMessage($"Executing stored procedure: {storedProcName} with ContractId: {contractId}, SubId: {subId}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                using (SqlDataReader reader = TRSSqlHelper.ExecuteReader(connectionString, storedProcName, contractId, subId))
                {
                    while (reader.Read())
                    {
                        Logger.LogMessage("Stored Procedure Result: " + reader[0].ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                }

                Logger.LogMessage("Stored procedure executed successfully using TRSSqlHelper.", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage("Authentication or SQL Server connection failed" + ex.StackTrace, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
        }

        static void CallSoapService()
        {
            try
            {
                TRS.IT.SI.Services.ContractService contractService = new TRS.IT.SI.Services.ContractService(AppSettings.GetValue("ContractWebServiceURL"));
                var result = contractService.GetContractInformation("932551", "000");
                string responseContent = result?.TotalAssets.ToString();
                Logger.LogMessage("SOAP response: " + responseContent, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                var result2 = contractService.GetCustomPxFunds("932551", "000");
                responseContent = result2.ToString();
                Logger.LogMessage("SOAP response: " + responseContent, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                //string xml = "<CustomFunds><FundID EffectiveDate=9/18/2013>1543</FundID></CustomFunds>";
                //var result3 = contractService.SetCustomPxFunds("932551", "000", xml);
                //responseContent = result3.ToString();
                //Logger.LogMessage("SOAP response: " + responseContent, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                //var result4 = contractService.SubmitTestingResults("932551", "000", xml);
                //responseContent = result4.ToString();
                //Logger.LogMessage("SOAP response: " + responseContent, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                //var result4 = contractService.NotifyToConsolidateMessages("932551", "000", xml);
                //responseContent = result4.ToString();
                //Logger.LogMessage("SOAP response: " + responseContent, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);



                
            }
            catch (Exception ex)
            {
                Logger.LogMessage("SOAP Service call failed" + ex.StackTrace, Logger.LoggerType.BendProcessor,Logger.LogInfoType.ErrorFormat);
            }
        }
        private static void TestFtpOperations()
        {
            try
            {
                string ftpHost = AppSettings.GetVaultValue("FTPHostName");
                string ftpUsername = AppSettings.GetVaultValue("FTPUserName");
                string ftpPassword = AppSettings.GetVaultValue("FTPPassword");
                string ftpTestDirectory = AppSettings.GetValue("FTPTestDirectory");

                Logger.LogMessage($"FTP Configuration:", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                Logger.LogMessage($"  Host: {ftpHost}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                Logger.LogMessage($"  Username: {ftpUsername}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                Logger.LogMessage($"  Test Directory: {ftpTestDirectory}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                FTPUtility ftpUtil = new FTPUtility(ftpHost, ftpUsername, ftpPassword);
                Logger.LogMessage("Testing FTP directory listing...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                try
                {
                    List<string> files = ftpUtil.ListDirectory(ftpTestDirectory);
                    Logger.LogMessage($"Directory listing successful. Found {files.Count} items:", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    foreach (string file in files.Take(5))
                    {
                        Logger.LogMessage($"  - {file}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"Directory listing failed: {ex.Message}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                }

                Logger.LogMessage("Testing FTP file upload...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                string testFileName = $"ftp_test_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string localTestFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, testFileName);

                try
                {
                    File.WriteAllText(localTestFilePath, $"FTP Test File Created: {DateTime.Now}\nThis is a test file for FTP upload functionality.");
                    Logger.LogMessage($"Created local test file: {localTestFilePath}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                    string ftpError = "";
                    string targetPath = $"{ftpTestDirectory}/{testFileName}";
                    bool uploadResult = ftpUtil.UploadFile(localTestFilePath, targetPath, ref ftpError);

                    if (uploadResult)
                    {
                        Logger.LogMessage($"File upload successful to: {targetPath}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    else
                    {
                        Logger.LogMessage($"File upload failed: {ftpError}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"File upload error: {ex.Message}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                }

                Logger.LogMessage("Testing FTP file existence check...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                try
                {
                    string targetPath = $"{ftpTestDirectory}/{testFileName}";
                    bool fileExists = ftpUtil.FtpFileExists(targetPath);
                    Logger.LogMessage($"File existence check for {targetPath}: {fileExists}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                    if (fileExists)
                    {
                        long fileSize = ftpUtil.GetFileSize(targetPath);
                        Logger.LogMessage($"File size: {fileSize} bytes", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"File existence check error: {ex.Message}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                }

                Logger.LogMessage("Testing FTP file download...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                try
                {
                    string downloadFileName = $"downloaded_{testFileName}";
                    string downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, downloadFileName);
                    string targetPath = $"{ftpTestDirectory}/{testFileName}";

                    bool downloadResult = ftpUtil.Download(targetPath, downloadPath, true);

                    if (downloadResult && File.Exists(downloadPath))
                    {
                        string downloadedContent = File.ReadAllText(downloadPath);
                        Logger.LogMessage($"File download successful. Content length: {downloadedContent.Length}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        Logger.LogMessage($"Downloaded content preview: {downloadedContent.Substring(0, Math.Min(100, downloadedContent.Length))}...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

                        File.Delete(downloadPath);
                    }
                    else
                    {
                        Logger.LogMessage("File download failed", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"File download error: {ex.Message}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                }
                Logger.LogMessage("Testing FTP directory creation...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                try
                {
                    string testDirName = $"test_dir_{DateTime.Now:yyyyMMdd_HHmmss}";
                    string testDirPath = $"{ftpTestDirectory}/{testDirName}";
                    string dirError = "";

                    bool dirResult = ftpUtil.FtpCreateDirectory(testDirPath, ref dirError);

                    if (dirResult)
                    {
                        Logger.LogMessage($"Directory creation successful: {testDirPath}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    else
                    {
                        Logger.LogMessage($"Directory creation failed: {dirError}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"Directory creation error: {ex.Message}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                }
                Logger.LogMessage("Testing FTP file deletion...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                try
                {
                    string targetPath = $"{ftpTestDirectory}/{testFileName}";
                    bool deleteResult = ftpUtil.FtpDelete(targetPath);

                    if (deleteResult)
                    {
                        Logger.LogMessage($"File deletion successful: {targetPath}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                    else
                    {
                        Logger.LogMessage("File deletion failed", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"File deletion error: {ex.Message}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                }
                try
                {
                    if (File.Exists(localTestFilePath))
                    {
                        File.Delete(localTestFilePath);
                        Logger.LogMessage("Local test file cleaned up", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogMessage($"Local file cleanup error: {ex.Message}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                }

                Logger.LogMessage("FTP testing completed successfully", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"FTP testing failed: {ex.Message}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                throw;
            }
        }
        private static void SendErrorEmailToUsers()
        {
            string value = "suprajata.panda@transamerica.com";
            string value2 = "suprajata.panda@transamerica.com";
            Utils.SendMail(value, value2, "Immediate attention required - Contract: " + "513245" + " CaseNo: " + "405" + " Partner: " + "sPartnerId", "a_sError", "suprajata.panda@transamerica.com");
        }
    }
}