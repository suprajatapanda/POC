using System.Data;
using System.Text;
using SIUtil;
using TARSharedUtilLib.Utility;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using Mail = System.Net.Mail;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace DailyAuditPackageISCBatch.BLL
{
    public class AuditPackage : BendProcessorBase
    {
        private readonly eStatementDC _oeSDC = new();
        private DataSet _dsPptOptin;
        public AuditPackage() : base("537", "AuditPackage", "TRS") { }

        public TaskStatus ProcesseAuditPkgISC()
        {
            return ProcesseAuditPkgFromFolder(ConstN.C_PARTNER_ISC);
        }
        public TaskStatus ProcesseAuditPkgFromFolder(string partnerId)
        {
            var taskStatus = new TaskStatus
            {
                retStatus = TaskRetStatus.NotRun
            };

            var taskName = $"ProcesseAuditPkg{partnerId}";
            var logBuilder = new StringBuilder();

            try
            {
                if (AppSettings.GetValue(taskName) != "1")
                    return taskStatus;

                InitTaskStatus(taskStatus, taskName);

                var processingStatus = ProcessMoveFolderFilesToStaging(partnerId);
                logBuilder.Append(General.ParseTaskInfo(processingStatus));

                if (processingStatus.retStatus is TaskRetStatus.Failed or TaskRetStatus.FailedAborted)
                {
                    taskStatus.retStatus = TaskRetStatus.Failed;
                    SendTaskCompleteEmail(
                        $"AuditPackage Status - {processingStatus.retStatus}",
                        logBuilder.ToString(),
                        processingStatus.taskName
                    );
                }

                SendTaskCompleteEmail(
                    $"AuditPackage Status - {partnerId} Succeeded",
                    logBuilder.ToString(),
                    "AuditPackage Backend Processing"
                );

                _dsPptOptin?.Clear();
                _dsPptOptin = null;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                InitTaskError(taskStatus, ex, true);
            }
            finally
            {
                taskStatus.endTime = DateTime.Now;
            }

            return taskStatus;
        }

        private ResultReturn MoveSatementsFromStagingFtp2(string sPartnerID)
        {
            ResultReturn oReturn = new();
            ResultReturn oMoveFileReturn = new();
            List<string> result = new();
            List<string> ConvertError = [];

            FtpFolderTracking oFtpFolderTracking;
            FileInfo oFileInfo = null;

            try
            {
                var sStagingDir = AppSettings.GetValue($"AuditPackageStagingDirPath{sPartnerID}");
                var sDestinationDir = AppSettings.GetValue($"AuditPackageDestinationDirPath{sPartnerID}");

                if (string.IsNullOrWhiteSpace(sStagingDir) || string.IsNullOrWhiteSpace(sDestinationDir))
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "Invalid Partner ID - " + sPartnerID, ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                var oFtp = new FTPUtility(
                AppSettings.GetVaultValue("FTPHostName"),
                AppSettings.GetVaultValue("FTPUserName"),
                AppSettings.GetVaultValue("FTPPassword")
                );

                ////1. Get list of files to move from Staging folder.
                FTPdirectory oFTPDir = oFtp.ListDirectoryDetail(sStagingDir);
                oFtpFolderTracking = GetFtpFolderInfo(oFtp, sDestinationDir);

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

                                    var subDirectory = oFtp.ListDirectoryDetail(ff.FullName);

                                    foreach (var subFile in subDirectory.Where(f => f.FileType == FTPfileInfo.DirectoryEntryTypes.File))
                                    {
                                        try
                                        {
                                            if (!oFtpFolderTracking.IsUnderLimit)
                                            {
                                                oFtpFolderTracking.subFolderCount++;
                                                oFtpFolderTracking = GetFtpFolderInfo(oFtp, sDestinationDir);

                                                if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                                {
                                                    throw new Exception($"Unable to create FTP subfolder: {oFtpFolderTracking.errors[0].errorDesc}");
                                                }
                                            }

                                            var isCsvToConvert = subFile.Filename.Contains("10002") && subFile.Filename.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
                                            FTPfileInfo convertedFile = null;

                                            if (isCsvToConvert)
                                            {
                                                var conversionResult = ConverttoXls(oFtp, subFile, ref convertedFile, ref ConvertError);
                                                oMoveFileReturn = conversionResult.returnStatus == ReturnStatusEnum.Succeeded
                                                ? MoveFtpFile2(oFtp, oFileInfo, convertedFile, sPartnerID, oFtpFolderTracking.GetCurrentFolder)
                                                : new ResultReturn { returnStatus = ReturnStatusEnum.Failed };
                                            }
                                            else
                                            {
                                                oMoveFileReturn = MoveFtpFile2(oFtp, oFileInfo, subFile, sPartnerID, oFtpFolderTracking.GetCurrentFolder);
                                            }

                                            if (oMoveFileReturn.returnStatus == ReturnStatusEnum.Succeeded)
                                            {
                                                oReturn.rowsCount++;
                                                oFtpFolderTracking.runningCount++;
                                            }
                                            else
                                            {
                                                General.CopyResultError(oReturn, oMoveFileReturn);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                                            oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
                                        }
                                    }

                                    break;
                                case FTPfileInfo.DirectoryEntryTypes.File:

                                    try
                                    {
                                        if (!oFtpFolderTracking.IsUnderLimit)
                                        {
                                            oFtpFolderTracking = GetFtpFolderInfo(oFtp, sDestinationDir);
                                            if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                            {
                                                throw new Exception($"Unable to create FTP subfolder: {oFtpFolderTracking.errors[0].errorDesc}");
                                            }
                                        }

                                        var isCsvToConvert = ff.Filename.Contains("10002") && ff.Filename.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
                                        FTPfileInfo convertedFile = null;

                                        if (isCsvToConvert)
                                        {
                                            var conversionResult = ConverttoXls(oFtp, ff, ref convertedFile, ref ConvertError);
                                            oMoveFileReturn = conversionResult.returnStatus == ReturnStatusEnum.Succeeded
                                            ? MoveFtpFile2(oFtp, oFileInfo, convertedFile, sPartnerID, oFtpFolderTracking.GetCurrentFolder)
                                            : new ResultReturn { returnStatus = ReturnStatusEnum.Failed };
                                        }
                                        else
                                        {
                                            oMoveFileReturn = MoveFtpFile2(oFtp, oFileInfo, ff, sPartnerID, oFtpFolderTracking.GetCurrentFolder);
                                        }

                                        if (oMoveFileReturn.returnStatus == ReturnStatusEnum.Succeeded)
                                        {
                                            oReturn.rowsCount++;
                                            oFtpFolderTracking.runningCount++;
                                        }
                                        else
                                        {
                                            General.CopyResultError(oReturn, oMoveFileReturn);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                                        oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
                                    }

                                    break;
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                            oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
                        }

                    }//foreach
                }//oFtpFolderTracking
                else
                {
                    oReturn.Errors.Add(new ErrorInfo(-1, oFtpFolderTracking.errors[0].errorDesc, ErrorSeverityEnum.Failed));
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "ex: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            ///  Send Convert Error Email

            if (ConvertError.Count > 0)
            {
                var convertNoticeTo = AppSettings.GetValue($"AuditPackageConvertNotice{sPartnerID}");
                var bcc = AppSettings.GetValue("BCCEmailNotification");
                SendConvertFileNotice(convertNoticeTo, ConvertError, bcc);
            }

            return oReturn;
        }
        private static void SendConvertFileNotice(string convertNoticeTo, List<string> convertErrors, string bcc)
        {
            var message = new Mail.MailMessage
            {
                From = new Mail.MailAddress("AuditPackage@transamerica.com"),
                Subject = "Participant data file conversion",
                IsBodyHtml = true
            };

            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("The audit package participant data file failed to convert from .CSV to .XLSX for the contract(s) listed below. Please investigate the issue and resubmit the file once it has been corrected.<br /><br />");

            foreach (var contract in convertErrors)
            {
                bodyBuilder.AppendLine($"{contract}<br />");
            }

            bodyBuilder.AppendLine("<br />This is an automated email; please do not respond.");
            message.Body = bodyBuilder.ToString();

            foreach (var recipient in convertNoticeTo.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                message.To.Add(recipient.Trim());
            }

            foreach (var bccRecipient in bcc.Split(';', StringSplitOptions.RemoveEmptyEntries))
            {
                message.Bcc.Add(bccRecipient.Trim());
            }

            TRS.IT.TRSManagers.MailManager.SendEmail((MimeKit.MimeMessage)message);
        }

        private ResultReturn ConverttoXls(FTPUtility oFtp, FTPfileInfo ff_csv, ref FTPfileInfo ff_xls, ref List<string> ConvertError)
        {
            ResultReturn oReturn = new();
            string sDebug = ff_csv.FullName;
            string sReturnDir = ff_csv.Path;
            string sXLSFullName = "AA";
            try
            {

                bool bIsGood;
                string sError = "";
                //1. download
                var sCSVFullName = Path.Combine(AppSettings.GetValue("AuditPackageConvertXLS"), ff_csv.Filename);

                //1.1 rename if exist , if error continue download
                try
                {
                    if (File.Exists(sCSVFullName))
                    {
                        string sSaveName = sCSVFullName.Replace(".csv", "_") + DateTime.Now.ToShortTimeString().Replace(":", "") + ".csv";
                        File.Move(sCSVFullName, sSaveName);
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                }

                // Download CSV from FTP
                bIsGood = oFtp.Download(ff_csv.FullName, sCSVFullName, true);

                // Delete original CSV from FTP
                oFtp.FtpDelete(ff_csv.FullName);

                // Convert to XLS
                sXLSFullName = "";
                sXLSFullName = GetXlsFileName(sCSVFullName);

                // Upload XLS to FTP
                bIsGood = oFtp.UploadFile(sXLSFullName, sReturnDir + "/" + Path.GetFileName(sXLSFullName), ref sError);

                if (bIsGood)
                {
                    FTPdirectory oFTPDir = oFtp.ListDirectoryDetail(sReturnDir);
                    foreach (FTPfileInfo ff in oFTPDir)
                    {
                        if (Path.GetFileName(sXLSFullName) == ff.Filename)
                        {
                            ff_xls = ff;
                        }
                    }
                    File.Delete(sXLSFullName);
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    oReturn.Errors.Add(new ErrorInfo(-1, "Upload to staging failed file: " + sXLSFullName + " Error: " + sError, ErrorSeverityEnum.Error));
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                if (sXLSFullName.Length == 0)
                {
                    string[] sHold = ff_csv.Filename.Split('_');
                    ConvertError.Add(sHold[0]);
                }
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "  ConverttoXls -  Debug: " + sDebug + " Ex: " + ex.Message + "\r\n", ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }

        private static string GetXlsFileName(string fileFullPath)
        {
            if (string.IsNullOrWhiteSpace(fileFullPath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(fileFullPath));

            var outputXlsxFile = Path.ChangeExtension(fileFullPath, ".xlsx");

            AuditXlsxConverter.XlsxConverter.ConvertCsvToXlsx(fileFullPath, outputXlsxFile);

            if (File.Exists(fileFullPath))
                File.Delete(fileFullPath);

            return outputXlsxFile;
        }

        private ResultReturn MoveFtpFile2(FTPUtility a_oFtp, FileInfo a_oFileInfo, FTPfileInfo a_ff, string a_sPartnerId, string a_sTargetFtpFolder)
        {
            ResultReturn oReturn = new();
            ResultReturn oResRet;
            string[] s_arr;
            string sErrorInfo = "";
            bool bTemp = false;
            DocIndexFileInfo oIndexFileInfo;
            string sConId = "";
            string sSubId = "";
            string sFromPeriod = "";
            string sDocType = "";
            long lFileSize = 0;
            string sFileName = "";
            string sFileExtension = "";
            bool aValidLength = false;
            string sDebug = "s";
            try
            {
                s_arr = a_ff.NameOnly.Split(['_']);

                if (s_arr.Length == 4)
                {
                    sConId = RemoveLeadingZeros(s_arr[0].Trim());
                    sSubId = s_arr[1].Trim();

                    sFromPeriod = s_arr[2];
                    sDocType = s_arr[3];
                    lFileSize = a_ff.Size;
                    sFileName = a_ff.Filename;
                    sFileExtension = a_ff.Extension;
                    aValidLength = true;
                }

                if (aValidLength)
                {
                    sDebug += ";oPptStFeedInfo";
                    if (a_oFileInfo == null)
                    {
                        try
                        {
                            bTemp = a_oFtp.FtpRenameBlind(a_ff.FullName, a_sTargetFtpFolder + a_ff.Filename);
                            sDebug += "try";
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex);
                            if (ex.Message.Contains("already exists"))
                            {
                                sDebug += ";exist";
                                string sErrorFtpFolder = AppSettings.GetValue("AuditPackageFTPMoveError" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");
                                if (IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                                {
                                    sDebug += ";FTPMoveError";
                                    string sDuplicateFolder = sErrorFtpFolder + "/Duplicate";
                                    if (IsDirectoryExist(a_oFtp, sDuplicateFolder, true, ref sErrorInfo))
                                    {
                                        sDebug += ";Duplicate";
                                        if (AppSettings.GetValue("AuditPackagePDFOverride") == "1")
                                        {
                                            //move exist file to dup folder

                                            bTemp = a_oFtp.FtpRename(a_sTargetFtpFolder + a_ff.Filename, sDuplicateFolder + "/" + a_ff.Filename);
                                            sDebug += ";Target2Dup";
                                            if (!bTemp)
                                            {
                                                oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                                            }
                                            else
                                            {
                                                bTemp = a_oFtp.FtpRename(a_ff.FullName, a_sTargetFtpFolder + a_ff.Filename);
                                                sDebug += ";Source2Target";
                                                if (!bTemp)
                                                {
                                                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                                                }
                                                else
                                                {
                                                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " file already existed and was replaced. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Warning));
                                                    return oReturn;
                                                }
                                            }

                                        }
                                        else
                                        {
                                            bTemp = a_oFtp.FtpRename(a_ff.FullName, sDuplicateFolder + "/" + a_ff.Filename);
                                            sDebug += ";DupOnly";
                                            if (!bTemp)
                                            {
                                                oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                                            }
                                        }

                                    }
                                    else
                                    {
                                        oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + sDuplicateFolder + " sDocType:" + sDocType + "  C: " + sConId + " S: " + sSubId, ErrorSeverityEnum.Failed));
                                    }
                                }
                                else
                                {
                                    oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + sErrorFtpFolder + " sDocType:" + sDocType + "  C: " + sConId + " S: " + sSubId, ErrorSeverityEnum.Failed));
                                }
                            }

                            throw new Exception(a_ff.FullName + "Debug: " + sDebug + " Ex: " + ex.Message);
                        }
                    }
                    else
                    {
                        if (a_sPartnerId == ConstN.C_PARTNER_ISC || a_sPartnerId == ConstN.C_PARTNER_TAE)
                        {
                            bTemp = a_oFtp.UploadFile(a_oFileInfo.FullName, a_sTargetFtpFolder + a_oFileInfo.Name, ref sErrorInfo);
                            if (bTemp)
                            {
                                File.Delete(a_oFileInfo.FullName);
                            }
                        }
                    }
                    if (bTemp)
                    {
                        // update database
                        oIndexFileInfo = new DocIndexFileInfo
                        {
                            contractId = sConId,
                            subId = sSubId,
                            partnerId = a_sPartnerId,
                            docType = Convert.ToInt32(sDocType),
                            fileSize = Convert.ToInt32((lFileSize > 0 ? (Convert.ToDouble(lFileSize) / 1024.0) : 0.0)),
                            fileType = sFileExtension,
                            downloadType = 100, // FTP Download
                            sysAssignedFilename = a_sTargetFtpFolder + sFileName,
                            promptFilename = "AuditPackage_" + sDocType + "." + sFileExtension,
                            toPeriod = GetDate(sFromPeriod)
                        };
                        oIndexFileInfo.fromPeriod = oIndexFileInfo.toPeriod.AddYears(-1);
                        if (oIndexFileInfo.toPeriod.Month < 12)
                        {
                            oIndexFileInfo.displayDesc = oIndexFileInfo.fromPeriod.ToString("MM/dd/yyyy");
                        }
                        else
                        {
                            oIndexFileInfo.displayDesc = oIndexFileInfo.toPeriod.ToString("MM/dd/yyyy");
                        }

                        oIndexFileInfo.expireDt = oIndexFileInfo.toPeriod.AddMonths(11);  // good for 11 month
                        oIndexFileInfo.connectParms = "<ConnectParm><ParmId ParmName=\"DiaFtpAuditPackage\">" + sDocType + "</ParmId></ConnectParm>";
                        oResRet = _oeSDC.InsertDocumentIndex(oIndexFileInfo);
                        //   sDebug += ";PptStatementAvailable";

                        oReturn.rowsCount += 1;
                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;

                    }
                    else
                    {
                        // File move/rename failed...log error
                        //oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + "  could not be moved to :   " + a_sTargetFtpFolder + a_ff.Filename + "\r\n", ErrorSeverityEnum.Failed));
                    }

                }
                else
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " is not in expected file naming convention." + "\r\n", ErrorSeverityEnum.Failed));

                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "Debug: " + sDebug + " Ex: " + ex.Message + "\r\n", ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;

        }
        public TaskStatus ProcessMoveFolderFilesToStaging(string sPartnerID)
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            string C_Task = "ProcessMoveStagingFiles" + sPartnerID;

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    oReturn = MoveFromFilerToStaging(sPartnerID);
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;

                    oReturn = MoveSatementsFromStagingFtp2(sPartnerID);
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
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
        private ResultReturn MoveFromFilerToStaging(string sPartnerID)
        {
            ResultReturn oReturn = new();
            string sError = "";
            bool bIsGood;
            string sSubTemp;

            var result = new ResultReturn { returnStatus = ReturnStatusEnum.Succeeded };
            string debugStep = "Start";
            string rootFolder = AppSettings.GetValue($"AuditPackageStagingFolder{sPartnerID}");
            string folderNoDelete = AppSettings.GetValue($"{sPartnerID}StagingFolderNoDelete");
            string destinationDir = AppSettings.GetValue($"AuditPackageStagingDirPath{sPartnerID}");

            if (string.IsNullOrWhiteSpace(rootFolder) || string.IsNullOrWhiteSpace(destinationDir))
            {
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "Missing connection setting entries ", ErrorSeverityEnum.Failed));
                return oReturn;
            }

            var oFtp = new FTPUtility(
             AppSettings.GetVaultValue("FTPHostName"),
             AppSettings.GetVaultValue("FTPUserName"),
             AppSettings.GetVaultValue("FTPPassword")
            );

            debugStep = "oImpersonate";
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            try
            {
                foreach (var subDir in FileManagerSMB.GetDirectories(rootFolder))
                {
                    debugStep = subDir;
                    sSubTemp = destinationDir;
                    if (IsDirectoryExist(oFtp, sSubTemp, true, ref sError))
                    {
                        foreach (var sFileName in FileManagerSMB.GetFiles(subDir))
                        {
                            debugStep = sFileName;
                            try
                            {
                                bIsGood = oFtp.UploadFile(sFileName, sSubTemp + "/" + Path.GetFileName(sFileName), ref sError);
                                if (bIsGood)
                                {
                                    if (folderNoDelete != "1")
                                    {
                                        FileManagerSMB.Delete(sFileName);
                                    }
                                }
                                else
                                {
                                    oReturn.Errors.Add(new ErrorInfo(-1, "Upload to staging failed file: " + sFileName + " Error: " + sError, ErrorSeverityEnum.Error));
                                }

                            }
                            catch (Exception ex)
                            {
                                Utils.LogError(ex);
                                oReturn.Errors.Add(new ErrorInfo(-1, "Unable to upload to staging file: " + sFileName, ErrorSeverityEnum.ExceptionRaised));
                            }
                        }
                    }
                    else
                    {
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message + " MysDebug " + debugStep, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;

        }

    }
}
