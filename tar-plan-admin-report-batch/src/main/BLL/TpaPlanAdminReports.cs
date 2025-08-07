using System.Data;
using System.Text;
using System.Xml.Linq;
using AccountDataObj;
using SIUtil;
using TARSharedUtilLib.Utility;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
using TpaPlanAdminReportsDC = TarPlanAdminReportBatch.DAL.TpaPlanAdminReportsDC;

namespace TarPlanAdminReportBatch.BLL
{
    public class TpaPlanAdminReports : BendProcessorBase
    {
        TRS.IT.BendProcessor.BLL.FWBend fWBend;
        public TpaPlanAdminReports(TRS.IT.BendProcessor.BLL.FWBend a_fWBend): base("64", "TpaPlanAdminReports", "TRS")
        {
            fWBend = a_fWBend;
        }

        private TpaPlanAdminReportsDC _oTpaPAR = new();
        private eStatementDC _oeSDC = new();

        public TaskStatus ProcessTpaPlanAdminReportsISC()
        {
            TaskStatus oTaskReturn = new();
            TaskStatus oTaskStatus;

            const string C_Task = "ProcessTpaPlanAdminReports";
            ResultReturn oReturn = new();
            StringBuilder strB = new();

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    fWBend.InitTaskStatus(oTaskReturn, C_Task);

                    //1. Move ISC files
                    //1.1 Move files to FTP staging folder
                    oTaskStatus = new TaskStatus();
                    fWBend.InitTaskStatus(oTaskStatus, "MoveTpaPlanAdminReportsFromSourceToFtpStaging_ISC");

                    oReturn = MoveTpaPlanAdminReportsFromSourceToFtpStaging(ConstN.C_PARTNER_ISC);

                    oTaskStatus.rowsCount += oReturn.rowsCount;
                    oTaskStatus.endTime = DateTime.Now;

                    General.CopyResultError(oTaskStatus, oReturn);
                    strB.AppendLine(General.ParseTaskInfo(oTaskStatus));

                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        fWBend.SendTaskCompleteEmail("MoveTpaPlanAdminReportsFromSourceToFtpStaging_ISC Status - " + oReturn.returnStatus.ToString(), General.ParseTaskInfo(oTaskStatus), oTaskStatus.taskName);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    //1.2 Move files from FTP staging folder Final FTP destination folder
                    oTaskStatus = new TaskStatus();
                    fWBend.InitTaskStatus(oTaskStatus, "MoveTpaPlanAdminReportsFromFtpStagingToFtp_ISC");

                    oReturn = MoveTpaPlanAdminReportsFromFtpStagingToFtp(ConstN.C_PARTNER_ISC);
                    oTaskStatus.rowsCount += oReturn.rowsCount;
                    oTaskStatus.endTime = DateTime.Now;
                    General.CopyResultError(oTaskStatus, oReturn);
                    strB.AppendLine(General.ParseTaskInfo(oTaskStatus));

                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        fWBend.SendTaskCompleteEmail("MoveTpaPlanAdminReportsFromFtpStagingToFtp_ISC Status - " + oReturn.returnStatus.ToString(), General.ParseTaskInfo(oTaskStatus), oTaskStatus.taskName);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.endTime = DateTime.Now;

                    fWBend.SendTaskCompleteEmail("TpaPlanAdminReports  Backend Processing Complete", strB.ToString(), "ProcessTpaPlanAdminReportsISC");
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                fWBend.InitTaskError(oTaskReturn, ex, true);
            }

            return oTaskReturn;

        }

        private ResultReturn MoveTpaPlanAdminReportsFromSourceToFtpStaging(string sPartnerID, bool bProcessSubDirectory = false)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            string sDestinationDir;
            string sRootFolder = "";
            string sError = "";
            string sError1 = "";
            string sError2 = "";
            bool bIsGood;
            string sSubTemp = "";

            string sExcelFileName = string.Empty;
            FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
             AppSettings.GetVaultValue("FTPUserName"),
             AppSettings.GetVaultValue("FTPPassword"));
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            sRootFolder = AppSettings.GetValue("TpaPlanAdminReportsStagingFolder" + sPartnerID);
            //For Local
            //sRootFolder = "C:\\1\\";
            sDestinationDir = AppSettings.GetValue("TpaPlanAdminReportsStagingDirPath" + sPartnerID);

            if (sRootFolder == string.Empty || sDestinationDir == string.Empty)
            {
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "MoveTpaPlanAdminReportsFromStagingToFtp: Missing connection setting entries ", ErrorSeverityEnum.Failed));
                return oReturn;
            }


            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            try
            {
                if (sPartnerID != ConstN.C_PARTNER_ISC)
                {
                    ResultReturn oRConv = ConvertAccountDataCSVFilesToExcel(sRootFolder, sPartnerID);
                    General.CopyResultError(oReturn, oRConv);
                }
                sError = "";
                if (oFtp.IsDirectoryExist(sDestinationDir, true, ref sError))
                {
                    //1. move files under stagging root directory
                    string[] Files1 = Directory.GetFiles(sRootFolder);
                    //TODO:Need to test. All System.IO methods needs to be migrated to SMb calls.
                    try
                    {
                        string[] Files2 = FileManagerSMB.GetFiles(sRootFolder);
                    }
                    catch (Exception)
                    {
                        //do nothing
                    }

                    foreach (string sFileName1 in Files1)
                    {
                        sError = ""; sError1 = ""; sError2 = "";
                        try
                        {
                            if (ValidateFileName(sFileName1, sPartnerID, ref sError2) == false)
                            {
                                oReturn.Errors.Add(new ErrorInfo(-1, "ValidateFileName failed for file: " + sFileName1 + " Error: " + sError2, ErrorSeverityEnum.Error));

                                if (MoveFileToErrorFolder(sFileName1, sRootFolder, sPartnerID, ref sError1) == false)
                                {
                                    oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToErrorFolder: " + sError1, ErrorSeverityEnum.Warning));
                                }
                            }
                            else
                            {
                                bIsGood = oFtp.UploadFile(sFileName1, Path.Combine(sDestinationDir, Path.GetFileName(sFileName1)), ref sError);
                                if (bIsGood)
                                {
                                    oReturn.rowsCount++;
                                    if (MoveFileToCompleteFolder(sFileName1, sRootFolder, sPartnerID, ref sError1) == false)
                                    {
                                        oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToCompleteFolder: " + sError1, ErrorSeverityEnum.Warning));
                                    }
                                }
                                else
                                {
                                    oReturn.Errors.Add(new ErrorInfo(-1, "Upload to staging failed file: " + sFileName1 + " Error: " + sError, ErrorSeverityEnum.Error));

                                    if (MoveFileToErrorFolder(sFileName1, sRootFolder, sPartnerID, ref sError1) == false)
                                    {
                                        oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToErrorFolder: " + sError1, ErrorSeverityEnum.Warning));
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                            oReturn.Errors.Add(new ErrorInfo(-1, "Ex: " + ex.Message + " Unable to upload to staging file: " + sFileName1, ErrorSeverityEnum.ExceptionRaised));
                            if (MoveFileToErrorFolder(sFileName1, sRootFolder, sPartnerID, ref sError1) == false)
                            {
                                oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToErrorFolder: " + sError1, ErrorSeverityEnum.Warning));
                            }
                        }
                    }

                    //2. move files under subdirectories, if any
                    if (bProcessSubDirectory)
                    {
                        string[] sSubDirectories = Directory.GetDirectories(sRootFolder);
                        foreach (string sSub in sSubDirectories)
                        {
                            sError = "";
                            sSubTemp = sDestinationDir + sSub.Substring(sSub.LastIndexOf(@"\") + 1);
                            if (oFtp.IsDirectoryExist(sSubTemp, true, ref sError))
                            {
                                string[] Files = Directory.GetFiles(sSub);
                                foreach (string sFileName in Files)
                                {
                                    sError = ""; sError1 = "";
                                    try
                                    {
                                        if (ValidateFileName(sFileName, sPartnerID, ref sError2) == false)
                                        {
                                            oReturn.Errors.Add(new ErrorInfo(-1, "ValidateFileName failed for file: " + sFileName + " Error: " + sError2, ErrorSeverityEnum.Error));

                                            if (MoveFileToErrorFolder(sFileName, sRootFolder, sPartnerID, ref sError1) == false)
                                            {
                                                oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToErrorFolder: " + sError1, ErrorSeverityEnum.Warning));
                                            }
                                        }
                                        else
                                        {
                                            bIsGood = oFtp.UploadFile(sFileName, Path.Combine(sSubTemp, Path.GetFileName(sFileName)), ref sError);
                                            if (bIsGood)
                                            {
                                                if (MoveFileToCompleteFolder(sFileName, sRootFolder, sPartnerID, ref sError1) == false)
                                                {
                                                    oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToCompleteFolder: " + sError1, ErrorSeverityEnum.Warning));
                                                }
                                            }
                                            else
                                            {
                                                oReturn.Errors.Add(new ErrorInfo(-1, "Upload to staging failed file: " + sFileName + " Error: " + sError, ErrorSeverityEnum.Error));
                                                if (MoveFileToErrorFolder(sFileName, sRootFolder, sPartnerID, ref sError1) == false)
                                                {
                                                    oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToErrorFolder: " + sError1, ErrorSeverityEnum.Warning));
                                                }
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                                        oReturn.Errors.Add(new ErrorInfo(-1, "Unable to upload to staging file: " + sFileName + " Exception: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
                                        if (MoveFileToErrorFolder(sFileName, sRootFolder, sPartnerID, ref sError1) == false)
                                        {
                                            oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToErrorFolder: " + sError1, ErrorSeverityEnum.Warning));
                                        }
                                    }
                                }
                            }
                            else
                            {
                                oReturn.Errors.Add(new ErrorInfo(-1, "IsDirectoryExist failed: " + sSubTemp + " Error: " + sError, ErrorSeverityEnum.Error));
                            }

                        }
                    }

                }
                else
                {
                    oReturn.Errors.Add(new ErrorInfo(-1, "IsDirectoryExist failed: " + sDestinationDir + " Error: " + sError, ErrorSeverityEnum.Error));
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        private ResultReturn ConvertAccountDataCSVFilesToExcel(string sRootFolder, string sPartnerID)
        {
            ResultReturn oReturn = new();
            DataSet dsAccountTypes;
            DataView dvAcctTypes;
            string sExcelFileName = string.Empty;
            string sError1 = "";
            string[] AccountDataCSVFiles = Directory.GetFiles(sRootFolder, "*_28A.CSV", SearchOption.TopDirectoryOnly);

            if (AccountDataCSVFiles != null && AccountDataCSVFiles.Length > 0)
            {
                dsAccountTypes = _oTpaPAR.GetAccountTypes();
                if (dsAccountTypes != null && dsAccountTypes.Tables.Count > 0)
                {
                    dvAcctTypes = new DataView(dsAccountTypes.Tables[0]);

                    foreach (string sCSVFile in AccountDataCSVFiles)
                    {
                        try
                        {
                            sExcelFileName = string.Empty;
                            sExcelFileName = sCSVFile.Replace("_28A.CSV", "_61.CSV");
                            sExcelFileName = Path.ChangeExtension(sExcelFileName, "xls");
                            AccountDataConverter.ConvertCsvToXlsx(dvAcctTypes, sCSVFile, sExcelFileName, "");
                            if (MoveFileToCompleteFolder(sCSVFile, sRootFolder, sPartnerID, ref sError1) == false)
                            {
                                oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToCompleteFolder: " + sError1, ErrorSeverityEnum.Warning));
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                            oReturn.Errors.Add(new ErrorInfo(-1, "Unable to convert AccountData CSV File to Excel file: " + sCSVFile + " Exception: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));

                            if (MoveFileToErrorFolder(sCSVFile, sRootFolder, sPartnerID, ref sError1) == false)
                            {
                                oReturn.Errors.Add(new ErrorInfo(-1, "Error in MoveFileToErrorFolder: " + sError1, ErrorSeverityEnum.Warning));
                            }
                        }
                    }
                }
            }

            return oReturn;
        }

        private bool ValidateFileName(string sFile, string a_sPartnerId, ref string sError)
        {
            bool bRet = false;
            string sFileNameOnly = "";

            DocFileInfo oDocFileInfo;


            try
            {
                sFileNameOnly = Path.GetFileNameWithoutExtension(sFile);

                oDocFileInfo = GetDocFileInfo(a_sPartnerId, sFileNameOnly);

                if (!string.IsNullOrEmpty(oDocFileInfo.parseError))
                {
                    sError = oDocFileInfo.parseError;
                    return false;
                }
                else
                {
                    bRet = true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                sError = "Exception in ValidateFileName " + ex.Message;
            }

            return bRet;
        }

        private bool MoveFileToErrorFolder(string sFile, string sRootFolder, string sPartnerID, ref string sError)
        {
            bool bRet = false;
            string sErrorFolder = string.Empty;
            string sErrorFileName = string.Empty;
            string sTempFileName = string.Empty;
            sErrorFolder = AppSettings.GetValue("TpaPlanAdminReportsErrorFolder" + sPartnerID);
            try
            {
                sError = "";
                if (sErrorFolder == null || sErrorFolder == string.Empty)
                {
                    sErrorFolder = Path.Combine(sRootFolder, "Error");
                }

                sErrorFolder = Path.Combine(sErrorFolder, DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MMMM"), DateTime.Now.ToString("dd"));
                sErrorFileName = Path.Combine(sErrorFolder, Path.GetFileName(sFile));

                Utils.ValidatePath(sErrorFileName);

                File.Move(sFile, sErrorFileName);
                bRet = true;
            }
            catch (Exception ex1)
            {
                Utils.LogError(ex1);
                sError = "Unable to move file " + sFile + " To folder. " + sErrorFolder + " Exception: " + ex1.Message;
                bRet = false;

                try
                {
                    if (File.Exists(sErrorFileName))
                    {
                        sErrorFileName = Path.Combine(sErrorFolder, DateTime.Now.ToString("hh-mm tt"), Path.GetFileName(sErrorFileName));
                        // move to Hour-minute sub folder
                        Utils.ValidatePath(sErrorFileName);
                        File.Move(sFile, sErrorFileName);
                        bRet = true;
                    }

                }
                catch (Exception eXIn)
                {
                    Utils.LogError(eXIn);
                    sError = sError + " AND Unable to move file " + sFile + " To folder. " + sErrorFileName + " Exception: " + eXIn.Message;
                }
            }
            return bRet;
        }

        private bool MoveFileToCompleteFolder(string sFile, string sRootFolder, string sPartnerID, ref string sError)
        {
            bool bRet = false;
            string sCompleteFolder = string.Empty;
            string sCompleteFileName = string.Empty;
            string sTempFileName = string.Empty;
            try
            {
                sError = "";
                sCompleteFolder = AppSettings.GetValue("TpaPlanAdminReportsCompleteFolder" + sPartnerID);

                if (sCompleteFolder == null || sCompleteFolder == string.Empty)
                {
                    sCompleteFolder = Path.Combine(sRootFolder, "Complete");
                }

                sCompleteFolder = Path.Combine(sCompleteFolder, DateTime.Now.ToString("yyyy"), DateTime.Now.ToString("MMMM"), DateTime.Now.ToString("dd"));
                sCompleteFileName = Path.Combine(sCompleteFolder, Path.GetFileName(sFile));

                Utils.ValidatePath(sCompleteFileName);
                File.Move(sFile, sCompleteFileName);
                bRet = true;
            }
            catch (Exception ex1)
            {
                Utils.LogError(ex1);
                sError = "Unable to move file " + sFile + " To folder. " + sCompleteFolder + " Exception: " + ex1.Message;
                bRet = false;
                try
                {
                    if (File.Exists(sCompleteFileName))
                    {
                        sCompleteFileName = Path.Combine(sCompleteFolder, DateTime.Now.ToString("hh-mm tt"), Path.GetFileName(sCompleteFileName));
                        // move to Hour-minute sub folder
                        Utils.ValidatePath(sCompleteFileName);
                        File.Move(sFile, sCompleteFileName);
                        bRet = true;
                    }
                }
                catch (Exception eXIn)
                {
                    Utils.LogError(eXIn);
                    sError = sError + " AND Unable to move file " + sFile + " To folder. " + sCompleteFileName + " Exception: " + eXIn.Message;
                }
            }
            return bRet;
        }

        private ResultReturn MoveTpaPlanAdminReportsFromFtpStagingToFtp(string sPartnerID)
        {
            ResultReturn oReturn = new();
            ResultReturn oMoveFileReturn;

            List<string> result = new();
            string sStagingDir = "";
            string sDestinationDir = "";

            FtpFolderTracking oFtpFolderTracking;
            try
            {
                sStagingDir = AppSettings.GetValue("TpaPlanAdminReportsStagingDirPath" + sPartnerID);
                sDestinationDir = AppSettings.GetValue("TpaPlanAdminReportsDestinationDirPath" + sPartnerID);
                if (sStagingDir == string.Empty || sDestinationDir == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "Invalid Partner ID - " + sPartnerID, ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
                AppSettings.GetVaultValue("FTPUserName"),
                AppSettings.GetVaultValue("FTPPassword"));
                

                ////1. Get list of files to move from Staging folder.
                FTPdirectory oFTPDir = oFtp.ListDirectoryDetail(sStagingDir);

                oFtpFolderTracking = oFtp.GetFtpFolderInfo(sDestinationDir);
                if (oFtpFolderTracking.returnStatus == ReturnStatusEnum.Succeeded)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                    foreach (FTPfileInfo ff in oFTPDir)
                    {
                        try
                        {
                            switch (ff.FileType)
                            {
                                case FTPfileInfo.DirectoryEntryTypes.Directory:
                                    // Only go one-level deep
                                    FTPdirectory oFTPDir2 = oFtp.ListDirectoryDetail(ff.FullName);

                                    foreach (FTPfileInfo ff2 in oFTPDir2)
                                    {
                                        if (ff2.FileType == FTPfileInfo.DirectoryEntryTypes.File)
                                        {
                                            if (!oFtpFolderTracking.IsUnderLimit)
                                            {
                                                oFtpFolderTracking.subFolderCount += 1;
                                                oFtpFolderTracking = oFtp.GetFtpFolderInfo(sDestinationDir);
                                                if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                                {
                                                    throw new Exception("Unable to create Ftp sub folder under : " + sDestinationDir + oFtpFolderTracking.errors[0].errorDesc);
                                                }
                                            }

                                            oMoveFileReturn = MoveAndIndexTpaPlanAdminReportFtpFile(oFtp, ff2, sPartnerID, oFtpFolderTracking.GetCurrentFolder);
                                            General.CopyResultError(oReturn, oMoveFileReturn);
                                            if (oMoveFileReturn.returnStatus == ReturnStatusEnum.Succeeded)
                                            {
                                                oReturn.rowsCount++;
                                                oFtpFolderTracking.runningCount++;
                                            }

                                        }
                                    }

                                    break;
                                case FTPfileInfo.DirectoryEntryTypes.File:
                                    if (!oFtpFolderTracking.IsUnderLimit)
                                    {
                                        oFtpFolderTracking = oFtp.GetFtpFolderInfo(sDestinationDir);
                                        if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                        {
                                            throw new Exception("Unable to create Ftp sub folder : " + oFtpFolderTracking.errors[0].errorDesc);
                                        }
                                    }

                                    oMoveFileReturn = MoveAndIndexTpaPlanAdminReportFtpFile(oFtp, ff, sPartnerID, oFtpFolderTracking.GetCurrentFolder);
                                    General.CopyResultError(oReturn, oMoveFileReturn);
                                    if (oMoveFileReturn.returnStatus == ReturnStatusEnum.Succeeded)
                                    {
                                        oReturn.rowsCount++;
                                        oFtpFolderTracking.runningCount++;
                                    }

                                    break;
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                            oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
                        }

                    }
                }
                else
                {
                    oReturn.Errors.Add(new ErrorInfo(-1, oFtpFolderTracking.errors[0].errorDesc, ErrorSeverityEnum.Failed));
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "ex: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }

        private ResultReturn MoveAndIndexTpaPlanAdminReportFtpFile(FTPUtility a_oFtp, FTPfileInfo a_ff, string a_sPartnerId, string a_sTargetFtpFolder)
        {
            ResultReturn oReturn = new();
            ResultReturn oResRet;
            string sStageErrorFolder = "";
            string sErrorInfo = "";
            DocFileInfo oDocFileInfo;
            DocIndexFileInfo oIndexFileInfo;
            string sErrorFtpFolder = AppSettings.GetValue("TpaPlanAdminReportsFTPMoveError" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");

            try
            {
                oDocFileInfo = GetDocFileInfo(a_sPartnerId, a_ff.NameOnly);

                if (!string.IsNullOrEmpty(oDocFileInfo.parseError))
                {
                    sErrorInfo = "";
                    sStageErrorFolder = sErrorFtpFolder + "_StageErr";
                    if (a_oFtp.IsDirectoryExist(sStageErrorFolder, true, ref sErrorInfo))
                    {
                        oReturn = a_oFtp.MoveFtpFile(a_ff, sStageErrorFolder, sErrorFtpFolder, true);
                    }
                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " is not in expected file naming convention." + "\r\n" + oDocFileInfo.parseError, ErrorSeverityEnum.Failed));
                    oReturn.returnStatus = ReturnStatusEnum.Failed; // must set to failed here
                    return oReturn;
                }
                else
                {
                    //2. Move file                                    

                    bool a_bOverWriteExistingFile = false;
                    if (AppSettings.GetValue("TpaPlanAdminReportsPDFOverride" + a_sPartnerId) == "1")
                    {
                        a_bOverWriteExistingFile = true;

                    }
                    oResRet = a_oFtp.MoveFtpFile(a_ff, a_sTargetFtpFolder, sErrorFtpFolder, a_bOverWriteExistingFile);

                    General.CopyResultError(oReturn, oResRet);

                    if (oResRet.returnStatus != ReturnStatusEnum.Failed)
                    {
                        // 3. update database (Index file)

                        oIndexFileInfo = new DocIndexFileInfo();
                        oIndexFileInfo.contractId = oDocFileInfo.contractId;
                        oIndexFileInfo.subId = oDocFileInfo.subId;
                        oIndexFileInfo.partnerId = a_sPartnerId;
                        oIndexFileInfo.docType = oDocFileInfo.docType;
                        oIndexFileInfo.fileSize = Convert.ToInt32((a_ff.Size > 0 ? (Convert.ToDouble(a_ff.Size) / 1024.0) : 0.0));
                        oIndexFileInfo.fileType = a_ff.Extension;
                        oIndexFileInfo.downloadType = 100; // FTP Download
                        oIndexFileInfo.sysAssignedFilename = Path.Combine(a_sTargetFtpFolder, a_ff.Filename);
                        oIndexFileInfo.promptFilename = "TpaPlanAdminReport_" + oDocFileInfo.docType.ToString() + "." + a_ff.Extension;

                        oIndexFileInfo.toPeriod = GetDate(oDocFileInfo.trxDate, "yyyy-MM-dd");

                        if (a_ff.Filename.Contains("_A_") || a_ff.Filename.Contains("_a_"))
                        {
                            oIndexFileInfo.docType = oIndexFileInfo.docType * -1; //since there is no separate doctype id for annual files 
                                                                                  //Save Annualfiles with -ve doctype_ids
                            oIndexFileInfo.fromPeriod = oIndexFileInfo.toPeriod.AddYears(-1).AddDays(1);
                            oIndexFileInfo.expireDt = oIndexFileInfo.toPeriod.AddYears(7);  // good for 7 years 
                            oIndexFileInfo.displayDesc = "Year_" + oIndexFileInfo.toPeriod.ToString("MM/dd/yyyy");
                        }
                        else if (a_ff.Filename.Contains("_Q_") || a_ff.Filename.Contains("_q_"))
                        {
                            oIndexFileInfo.fromPeriod = AdjustFromDate(oIndexFileInfo.toPeriod);
                            oIndexFileInfo.expireDt = oIndexFileInfo.toPeriod.AddYears(2).AddDays(1);  // good for 2 years 
                            oIndexFileInfo.displayDesc = "Quarter_" + oIndexFileInfo.toPeriod.ToString("MM/dd/yyyy");
                        }

                        oIndexFileInfo.connectParms = "<ConnectParm><ParmId ParmName=\"DiaFtpTpaPlanAdminReports\">" + oIndexFileInfo.docType.ToString() + "</ParmId></ConnectParm>";
                        oResRet = _oeSDC.InsertDocumentIndex(oIndexFileInfo);
                        if (oResRet.returnStatus != ReturnStatusEnum.Succeeded)
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            sErrorInfo = "InsertDocumentIndex Failed: ";
                            if (oResRet.Errors.Count > 0)
                            {
                                sErrorInfo = sErrorInfo + oResRet.Errors[0].errorDesc + "\r\n";
                            }
                            oReturn.Errors.Add(new ErrorInfo(-1, sErrorInfo, ErrorSeverityEnum.Failed));
                        }
                        else
                        {
                            ResultReturn oRet = SendConsolidatedNotifications(oIndexFileInfo.contractId, oIndexFileInfo.subId, oIndexFileInfo.docType);
                            if (oRet.returnStatus != ReturnStatusEnum.Succeeded)
                            {
                                oReturn.returnStatus = ReturnStatusEnum.Failed;
                                sErrorInfo = "SendConsolidatedNotifications Failed: ";
                                if (oRet.Errors.Count > 0)
                                {
                                    sErrorInfo = sErrorInfo + oRet.Errors[0].errorDesc + "\r\n";
                                }
                                oReturn.Errors.Add(new ErrorInfo(-1, sErrorInfo, ErrorSeverityEnum.Failed));
                            }
                            else
                            {
                                oReturn.rowsCount += 1;
                                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                            }
                        }

                    }
                    else
                    {
                        oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + "  could not be moved to :   " + a_sTargetFtpFolder + a_ff.Filename + "\r\n", ErrorSeverityEnum.Failed));
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, " Exception: " + ex.Message + "\r\n", ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;

        }

        public ResultReturn SendConsolidatedNotifications(string a_sConId, string a_sSubId, int a_iDocType)
        {

            TRS.IT.BendProcessor.DriverSOA.ContractServ DriverSOACon = new();

            XElement xEl =
                new("WsDocumentServiceDocumentEx",
                    new XElement("ContractID", a_sConId.Trim()),
                    new XElement("SubID", a_sSubId.Trim()),
                    new XElement("DocTypeCode", a_iDocType)
                    );


            ResultReturn oRet = new();
            oRet = DriverSOACon.NotifyToConsolidateMessages(xEl.ToString(), "", "");

            return oRet;
        }
        private DocFileInfo GetDocFileInfo(string a_sPartnerId, string a_FileNameOnly) //a_FileNameOnly means no path and no extension
        {
            DocFileInfo oDocFileInfo = new();
            string[] s_arr;

            try
            {

                switch (a_sPartnerId)
                {
                    case ConstN.C_PARTNER_PENCO:
                    case ConstN.C_PARTNER_ISC:
                        //CCCCC_SSS_mmddccyy_Q_nnn.PDF //_Q: Quarterly Or _A: Annual
                        // 300069_000_08312010_A_70.pdf
                        s_arr = a_FileNameOnly.Split(['_']);
                        if (s_arr.Length == 5)
                        {
                            oDocFileInfo.contractId = RemoveLeadingZeros(s_arr[0].Trim());
                            oDocFileInfo.subId = s_arr[1].Trim();
                            oDocFileInfo.trxDate = GetDate(s_arr[2], "MMDDYYYY").ToString("yyyy-MM-dd");
                            oDocFileInfo.docType = Convert.ToInt32(s_arr[4]);
                        }
                        break;
                    case ConstN.C_PARTNER_TAE:
                        //PPPP_CCCCC_SSS_mmddccyy_Q_nnn.PDF //_Q: Quarterly Or _A: Annual
                        //8553_932003_000_06302010_Q_70.PDF
                        s_arr = a_FileNameOnly.Split(['_']);

                        if (s_arr.Length == 6)
                        {
                            oDocFileInfo.contractId = RemoveLeadingZeros(s_arr[1].Trim());
                            oDocFileInfo.subId = s_arr[2].Trim();
                            oDocFileInfo.trxDate = GetDate(s_arr[3], "MMDDYYYY").ToString("yyyy-MM-dd");
                            oDocFileInfo.docType = Convert.ToInt32(s_arr[5]);
                        }

                        break;
                }

                if (string.IsNullOrEmpty(oDocFileInfo.contractId) || oDocFileInfo.docType == 0)
                {
                    oDocFileInfo.parseError = "Error parsing file: " + a_FileNameOnly;
                }

            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oDocFileInfo.parseError = "File name: " + a_FileNameOnly + "  parse error: " + ex.Message;
            }

            return oDocFileInfo;
        }

        private string RemoveLeadingZeros(string a_sConId)
        {
            int iNum;
            if (int.TryParse(a_sConId, out iNum))
            {
                return iNum.ToString();
            }
            else
            {
                return a_sConId;
            }
        }

        private DateTime AdjustFromDate(DateTime a_ToPeriod)
        {
            if (((a_ToPeriod.AddMonths(-3)).AddDays(1)).Day != 1)
            {
                return (a_ToPeriod.AddMonths(-3)).AddDays(2);
            }
            else
            {
                return (a_ToPeriod.AddMonths(-3)).AddDays(1);
            }
        }
        private DateTime GetDate(string sDate, string a_sFormat)//MMDDYYYY format
        {
            DateTime dt = new(1900, 01, 01);
            switch (a_sFormat.ToUpper())
            {
                case "MMDDYYYY":
                    dt = new DateTime(Convert.ToInt32(sDate.Substring(4, 4)), Convert.ToInt32(sDate.Substring(0, 2)), Convert.ToInt32(sDate.Substring(2, 2)));
                    break;
                case "YYYYMMDD":
                    dt = new DateTime(Convert.ToInt32(sDate.Substring(0, 4)), Convert.ToInt32(sDate.Substring(4, 2)), Convert.ToInt32(sDate.Substring(6, 2)));
                    break;
                case "YYYY-MM-DD":
                    dt = Convert.ToDateTime(sDate);
                    break;
            }

            return dt;
        }
    }


}
