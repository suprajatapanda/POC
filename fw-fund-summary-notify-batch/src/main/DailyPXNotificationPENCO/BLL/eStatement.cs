using System.Text;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace DailyPXNotificationPENCOBatch.BLL
{
    public class eStatement : BendProcessorBase
    {
        TRS.IT.BendProcessor.BLL.eStatement EStatement;
        public eStatement() : base("54", "eStatement", "TRS") { }

        public TaskStatus ProcessPXNotificationPENCO()
        {
            const string C_Task = "ProcessPXNotificationPENCO";
            var oTaskReturn = new TaskStatus { retStatus = TaskRetStatus.NotRun };
            var strB = new StringBuilder();


            try
            {
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    var oTaskStatus = ProcessPXMoveStagingFilesPENCO();
                    strB.Append(General.ParseTaskInfo(oTaskStatus));
                    if (oTaskStatus.retStatus == TaskRetStatus.Failed || oTaskStatus.retStatus == TaskRetStatus.FailedAborted)
                    {
                        //send error
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                        SendTaskCompleteEmail("PXNotification Status - " + oTaskStatus.retStatus.ToString(), strB.ToString(), oTaskStatus.taskName);
                    }
                    else
                    {
                        oTaskStatus = EStatement.ProcessNotifyDIA(NotificationTypeEnum.PxNotification.GetHashCode());
                        strB.Append(General.ParseTaskInfo(oTaskStatus));
                        if (oTaskStatus.retStatus == TaskRetStatus.Failed || oTaskStatus.retStatus == TaskRetStatus.FailedAborted)
                        {
                            oTaskReturn.retStatus = TaskRetStatus.Failed;
                            //send error
                            SendTaskCompleteEmail("PXNotification Status - " + oTaskStatus.retStatus.ToString(), strB.ToString(), oTaskStatus.taskName);
                        }
                        else
                        {
                            oTaskStatus = EStatement.ProcessClearDailyDiaFeed(NotificationTypeEnum.PxNotification.GetHashCode());
                            strB.Append(General.ParseTaskInfo(oTaskStatus));
                        }
                    }

                    oTaskReturn.endTime = DateTime.Now;
                    SendTaskCompleteEmail("PXNotification Status - " + oTaskStatus.retStatus.ToString(), strB.ToString(), "PXNotification Backend Processing");

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

        private TaskStatus ProcessPXMoveStagingFilesPENCO()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            const string C_Task = "ProcessPXMoveStagingFilesPENCO";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    oReturn = MovePXFromPencoFilerToStaging();
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;

                    oReturn = MovePXDocsFromStagingFtp(ConstN.C_PARTNER_PENCO);
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

        private ResultReturn MovePXFromPencoFilerToStaging()
        {
            ResultReturn oReturn = new();
            SortedDictionary<string, int> dictFolder = new();
            string sError = "";
            bool bIsGood;
            var rootFolder = AppSettings.GetValue("PXNotificationStagingFolder");
            var noDelete = AppSettings.GetValue("PXNotificationStagingFolderNoDelete");
            var destinationDir = AppSettings.GetValue("PXNotificationStagingDirPathPENCO");


            if (string.IsNullOrWhiteSpace(rootFolder) || string.IsNullOrWhiteSpace(destinationDir))
            {
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "Missing connection setting entries", ErrorSeverityEnum.Failed));
                return oReturn;
            }

            FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
             AppSettings.GetVaultValue("FTPUserName"),
             AppSettings.GetVaultValue("FTPPassword"));
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            try
            {
                string[] sSubDirectories = Directory.GetDirectories(rootFolder);

                foreach (string sSub in sSubDirectories)
                {
                    string sSubTemp = destinationDir + sSub.Substring(sSub.LastIndexOf(@"\") + 1);
                    if (IsDirectoryExist(oFtp, sSubTemp, true, ref sError))
                    {
                        string[] Files = Directory.GetFiles(sSub);
                        foreach (string sFileName in Files)
                        {
                            try
                            {
                                bIsGood = oFtp.UploadFile(sFileName, sSubTemp + "/" + Path.GetFileName(sFileName), ref sError);
                                if (bIsGood)
                                {
                                    if (noDelete != "1")
                                    {
                                        File.Delete(sFileName);
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
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;

        }

        private ResultReturn MovePXDocsFromStagingFtp(string sPartnerID)
        {
            ResultReturn oReturn = new();
            ResultReturn oMoveFileReturn;
            FtpFolderTracking oFtpFolderTracking;

            try
            {

                string stagingDir = AppSettings.GetValue($"PXNotificationStagingDirPath{sPartnerID}");
                string destinationDir = AppSettings.GetValue($"PXNotificationDestinationDirPath{sPartnerID}");

                if (string.IsNullOrWhiteSpace(stagingDir) || string.IsNullOrWhiteSpace(destinationDir))
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "Invalid Partner ID - " + sPartnerID, ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                FTPUtility oFtp = new(
                    AppSettings.GetVaultValue("FTPHostName"),
                    AppSettings.GetVaultValue("FTPUserName"),
                    AppSettings.GetVaultValue("FTPPassword"));


                ////1. Get list of files to move from Staging folder.
                FTPdirectory oFTPDir = oFtp.ListDirectoryDetail(stagingDir);
                oFtpFolderTracking = GetFtpFolderInfo(oFtp, destinationDir);

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
                                    FTPdirectory oFTPDir2 = oFtp.ListDirectoryDetail(ff.FullName);

                                    foreach (FTPfileInfo ff2 in oFTPDir2)
                                    {
                                        if (ff2.FileType == FTPfileInfo.DirectoryEntryTypes.File)
                                        {
                                            if (!oFtpFolderTracking.IsUnderLimit)
                                            {
                                                oFtpFolderTracking.subFolderCount += 1;
                                                oFtpFolderTracking = GetFtpFolderInfo(oFtp, destinationDir);
                                                if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                                {
                                                    throw new Exception("Unable to create Ftp sub folder : " + oFtpFolderTracking.errors[0].errorDesc);
                                                }
                                            }
                                            oMoveFileReturn = MoveFtpFile(oFtp, ff2, sPartnerID, oFtpFolderTracking.GetCurrentFolder, NotificationTypeEnum.PxNotification);
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
                                    }

                                    break;
                                case FTPfileInfo.DirectoryEntryTypes.File:
                                    if (!oFtpFolderTracking.IsUnderLimit)
                                    {
                                        oFtpFolderTracking = GetFtpFolderInfo(oFtp, destinationDir);
                                        if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                        {
                                            throw new Exception("Unable to create Ftp sub folder : " + oFtpFolderTracking.errors[0].errorDesc);
                                        }
                                    }
                                    oMoveFileReturn = MoveFtpFile(oFtp, ff, sPartnerID, oFtpFolderTracking.GetCurrentFolder, NotificationTypeEnum.PxNotification);
                                    if (oMoveFileReturn.returnStatus == ReturnStatusEnum.Succeeded)
                                    {
                                        oReturn.rowsCount++;
                                        oFtpFolderTracking.runningCount++;
                                    }
                                    else
                                    {
                                        General.CopyResultError(oReturn, oMoveFileReturn);
                                    }
                                    break;
                            }

                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex);
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
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "ex: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }

        private ResultReturn MoveFtpFile(FTPUtility a_oFtp, FTPfileInfo a_ff, string a_sPartnerId, string a_sTargetFtpFolder, NotificationTypeEnum a_eNotificationType)
        {
            ResultReturn oReturn = new();
            ResultReturn oResRet;
            PartStFeedInfo oPptStFeedInfo;
            string sErrorInfo = "";
            bool bTemp = false;
            string sDebug = "s";

            DocFileInfo oDocFileInfo;

            try
            {

                oDocFileInfo = EStatement.GetDocFileInfo(a_sPartnerId, a_eNotificationType, a_ff);

                if (!string.IsNullOrEmpty(oDocFileInfo.parseError))
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " is not in expected file naming convention." + "\r\n" + oDocFileInfo.parseError, ErrorSeverityEnum.Failed));
                }
                else
                {
                    oPptStFeedInfo = EStatement.GetPptFeedInfo(oDocFileInfo.ssn, oDocFileInfo.contractId, oDocFileInfo.subId, a_eNotificationType);
                    if (oPptStFeedInfo.found)
                    {
                        //2.2 Move file                                    
                        try
                        {
                            //oPptStFeedInfo.notificationType = 1;
                            bTemp = a_oFtp.FtpRename(a_ff.FullName, a_sTargetFtpFolder + a_ff.Filename);
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex);
                            if (ex.Message.Contains("already exists"))
                            {
                                string sErrorFtpFolder = AppSettings.GetValue("FTPMoveError" + a_eNotificationType.GetHashCode().ToString() + "_" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");

                                if (IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                                {
                                    string sDuplicateFolder = sErrorFtpFolder + "/Duplicate";
                                    if (IsDirectoryExist(a_oFtp, sDuplicateFolder, true, ref sErrorInfo))
                                    {
                                        if (AppSettings.GetValue("PDFOverride" + a_eNotificationType.GetHashCode().ToString() + "_" + a_sPartnerId) == "1")
                                        {
                                            //move exist file to dup folder

                                            bTemp = a_oFtp.FtpRename(a_sTargetFtpFolder + a_ff.Filename, sDuplicateFolder + "/" + a_ff.Filename);
                                            if (!bTemp)
                                            {
                                                oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                                            }
                                            else
                                            {
                                                bTemp = a_oFtp.FtpRename(a_ff.FullName, a_sTargetFtpFolder + a_ff.Filename);
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
                                            if (!bTemp)
                                            {
                                                oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                                            }
                                        }

                                    }
                                    else
                                    {
                                        oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + sDuplicateFolder + " SSN:" + oDocFileInfo.ssn + "  C: " + oDocFileInfo.contractId + " S: " + oDocFileInfo.subId, ErrorSeverityEnum.Failed));
                                    }
                                }
                                else
                                {
                                    oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + sErrorFtpFolder + " SSN:" + oDocFileInfo.ssn + "  C: " + oDocFileInfo.contractId + " S: " + oDocFileInfo.subId, ErrorSeverityEnum.Failed));
                                }
                            }

                            throw new Exception(a_ff.FullName + "Debug: " + sDebug + " Ex: " + ex.Message);
                        }

                        if (bTemp)
                        {
                            // update database
                            oResRet = EStatement.PptStatementAvailable(oPptStFeedInfo, GetDocIndexFileInfo(oDocFileInfo, a_sPartnerId, a_sTargetFtpFolder, a_ff));
                            if (oResRet.returnStatus != ReturnStatusEnum.Succeeded)
                            {
                                oReturn.returnStatus = ReturnStatusEnum.Failed;
                                sErrorInfo = "PptStatementAvailable Failed: ";
                                if (oResRet.Errors.Count > 0)
                                {
                                    sErrorInfo = sErrorInfo + oResRet.Errors[0].errorDesc + "\r\n";
                                }
                                oReturn.Errors.Add(new ErrorInfo(-1, sErrorInfo, ErrorSeverityEnum.Failed));
                            }
                            else
                            {
                                oReturn.rowsCount += 1;
                                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                            }
                        }
                        else
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + "  could not be moved to :   " + a_sTargetFtpFolder + a_ff.Filename + "\r\n", ErrorSeverityEnum.Failed));
                        }


                    }
                    else
                    {
                        oReturn.Errors.Add(new ErrorInfo(-1, "PPT not found " + oDocFileInfo.ssn + " C: " + oDocFileInfo.contractId + " S: " + oDocFileInfo.subId, ErrorSeverityEnum.Failed));
                        //Fpt processing. Move to error folder
                        string sErrorFtpFolder = AppSettings.GetValue("FTPMoveError" + a_eNotificationType.GetHashCode().ToString() + "_" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");
                        if (IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                        {
                            bTemp = a_oFtp.FtpRename(a_ff.FullName, sErrorFtpFolder + "/" + a_ff.Filename);
                            if (!bTemp)
                            {
                                oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                            }
                        }
                        else
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + sErrorFtpFolder + " SSN:" + oDocFileInfo.ssn + "  C: " + oDocFileInfo.contractId + " S: " + oDocFileInfo.subId, ErrorSeverityEnum.Failed));
                        }
                    }
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

        private DocIndexFileInfo GetDocIndexFileInfo(DocFileInfo docFileInfo, string partnerId, string targetFtpFolder, FTPfileInfo ftpFileInfo)
        {
            return new DocIndexFileInfo
            {
                contractId = docFileInfo.contractId,
                subId = docFileInfo.subId,
                partnerId = partnerId,
                docType = docFileInfo.docType,
                fileSize = ftpFileInfo.Size > 0 ? (int)(ftpFileInfo.Size / 1024.0) : 0,
                fileType = ftpFileInfo.Extension,
                downloadType = 100, // FTP Download
                sysAssignedFilename = Path.Combine(targetFtpFolder, ftpFileInfo.Filename).Replace("\\", "/"),
                promptFilename = $"SaveDocAs.{ftpFileInfo.Extension}",
                transId = docFileInfo.transId,
                fromPeriod = Convert.ToDateTime(docFileInfo.trxDate),
                toPeriod = Convert.ToDateTime(docFileInfo.trxDate),
                displayDesc = Convert.ToDateTime(docFileInfo.trxDate).ToString("MM/dd/yyyy"),
                expireDt = DateTime.Now.AddYears(2)
            };
        }

    }
}
