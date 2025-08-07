using System.Data;
using System.Text;
using System.Xml.Linq;
using ProcessRequiredNoticesProcBatch.DAL;
using TARSharedUtilLib.Utility;
using TRS.IT.BendProcessor.DriverSOA;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace ProcessRequiredNoticesProcBatch.BLL
{
    public class eStatement : BendProcessorBase
    {
        private static readonly TRS.IT.BendProcessor.BLL.eStatement _estatement = new();
        private eStatementDC _oeSDC = new();
        private DataSet? _dsPptOptin;
        private const int C_ParticipantReqdNoticesNotificationType = 4;

        public eStatement() : base("54", "eStatement", "TRS") { }


        public TaskStatus ProcessParticipantReqdNotices()
        {
            ResultReturn oReturn = new();
            var oTaskReturn = new TaskStatus { retStatus = TaskRetStatus.NotRun };
            var strB = new StringBuilder();
            const string C_Task = "ProcessParticipantReqdNotices";

            try
            {
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    TaskStatus oTaskStatus = MoveReqdNotices();
                    strB.Append(General.ParseTaskInfo(oTaskStatus));

                    if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                    {
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                        SendTaskCompleteEmail("ParticipantReqdNotices Status (MoveReqdNotices) - " + oTaskStatus.retStatus.ToString(), strB.ToString(), oTaskStatus.taskName);
                    }
                    oTaskStatus = _estatement.ProcessNotifyDIA(C_ParticipantReqdNoticesNotificationType); // create and upload dia notification file containing list of participants name and email id etc.
                    strB.Append(General.ParseTaskInfo(oTaskStatus));

                    if (oTaskStatus.retStatus == TaskRetStatus.Failed || oTaskStatus.retStatus == TaskRetStatus.FailedAborted)
                    {
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                        SendTaskCompleteEmail("ParticipantReqdNotices Status (ProcessNotifyDIA Failed)- " + oTaskStatus.retStatus.ToString(), strB.ToString(), oTaskStatus.taskName);
                    }
                    else
                    {
                        oTaskStatus = _estatement.ProcessClearDailyDiaFeed(C_ParticipantReqdNoticesNotificationType);
                        strB.Append(General.ParseTaskInfo(oTaskStatus));
                        SendTaskCompleteEmail("ParticipantReqdNotices Status (ProcessClearDailyDiaFeed)- " + oTaskStatus.retStatus.ToString(), strB.ToString(), oTaskStatus.taskName);
                    }

                    oTaskReturn.rowsCount += oReturn.rowsCount;
                    strB.Append(General.ParseTaskInfo(oTaskStatus));


                    oTaskReturn.endTime = DateTime.Now;
                    SendTaskCompleteEmail("ParticipantReqdNotices Task Completed", strB.ToString(), "ParticipantReqdNotices Backend Processing");

                    //Clear out cache dataset
                    if (_dsPptOptin != null)
                    {
                        _dsPptOptin.Clear();
                        _dsPptOptin = null;
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

        private TaskStatus MoveReqdNotices() // Move PENCO and TAE files..
        {
            var oTaskReturn = new TaskStatus { retStatus = TaskRetStatus.NotRun };
            ResultReturn oReturn;
            const string C_Task = "MoveReqdNotices";

            try
            {
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    //1. Move PENCO files
                    //1.1 Move files to FTP staging folder

                    oReturn = MoveReqdNoticesFromStagingToFtp(ConstN.C_PARTNER_PENCO);
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;

                    //1.2 Move files from FTP staging folder Final FTP destination folder
                    oReturn = MoveReqdNoticesFromFtpStagingToFtp(ConstN.C_PARTNER_PENCO, NotificationTypeEnum.RequiredNotifications);
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;

                    //2. Move TAE files
                    //2.1 Move files to FTP staging folder
                    oReturn = MoveReqdNoticesFromStagingToFtp(ConstN.C_PARTNER_TAE);
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;
                    //2.2 Move files from FTP staging folder Final FTP destination folder
                    oReturn = MoveReqdNoticesFromFtpStagingToFtp(ConstN.C_PARTNER_TAE, NotificationTypeEnum.RequiredNotifications);
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

        private ResultReturn MoveReqdNoticesFromStagingToFtp(string sPartnerID)
        {

            var oReturn = new ResultReturn { returnStatus = ReturnStatusEnum.Succeeded };
            string? error = null;
            bool bIsGood;
            string sSubTemp = "";

            try
            {
                FTPUtility oFtp =
                            new FTPUtility(
                            AppSettings.GetVaultValue("FTPHostName"),
                            AppSettings.GetVaultValue("FTPUserName"),
                            AppSettings.GetVaultValue("FTPPassword")
                           );

                oReturn.returnStatus = ReturnStatusEnum.Succeeded;

                var noDeleteFlag = AppSettings.GetValue($"ReqdNoticesStagingFolderNoDelete{sPartnerID}");
                var sRootFolder = AppSettings.GetValue($"ReqdNoticesStagingFolder{sPartnerID}");
                var sDestinationDir = AppSettings.GetValue($"ReqdNoticesStagingDirPath{sPartnerID}");


                if (string.IsNullOrWhiteSpace(sRootFolder) || string.IsNullOrWhiteSpace(sDestinationDir))
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "MoveReqdNoticesFromStagingToFtp: Missing connection setting entries ", ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                oReturn.returnStatus = ReturnStatusEnum.Succeeded;

                if (IsDirectoryExist(oFtp, sDestinationDir, true, ref error))
                {
                    //1. move files under stagging root directory

                    string[] Files1 = FileManagerSMB.GetFiles(sRootFolder);
                    foreach (string sFileName1 in Files1)
                    {
                        try
                        {
                            bIsGood = oFtp.UploadFile(sFileName1, Path.Combine(sDestinationDir, Path.GetFileName(sFileName1)), ref error);
                            if (bIsGood)
                            {
                                oReturn.rowsCount++;
                                if (noDeleteFlag != "1")
                                {
                                    File.Delete(sFileName1);
                                }
                            }
                            else
                            {
                                oReturn.Errors.Add(new ErrorInfo(-1, "Upload to staging failed file: " + sFileName1 + " Error: " + error, ErrorSeverityEnum.Error));
                            }

                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex);
                            oReturn.Errors.Add(new ErrorInfo(-1, "Ex: " + ex.Message + " Unable to upload to staging file: " + sFileName1, ErrorSeverityEnum.ExceptionRaised));
                        }
                    }

                    //2. move files under subdirectories, if any
                    string[] sSubDirectories = FileManagerSMB.GetDirectories(sRootFolder);

                    foreach (string sSub in sSubDirectories)
                    {
                        error = "";
                        sSubTemp = sDestinationDir + sSub.Substring(sSub.LastIndexOf(@"\") + 1);
                        if (IsDirectoryExist(oFtp, sSubTemp, true, ref error))
                        {
                            string[] Files = FileManagerSMB.GetFiles(sSub);
                            foreach (string sFileName in Files)
                            {
                                error = "";
                                try
                                {
                                    bIsGood = oFtp.UploadFile(sFileName, Path.Combine(sSubTemp, Path.GetFileName(sFileName)), ref error);
                                    if (bIsGood)
                                    {
                                        if (noDeleteFlag != "1")
                                        {
                                            File.Delete(sFileName);
                                        }
                                    }
                                    else
                                    {
                                        oReturn.Errors.Add(new ErrorInfo(-1, "Upload to staging failed file: " + sFileName + " Error: " + error, ErrorSeverityEnum.Error));
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Utils.LogError(ex);
                                    oReturn.Errors.Add(new ErrorInfo(-1, "Unable to upload to staging file: " + sFileName + " Exception: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
                                }
                            }
                        }
                        else
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "IsDirectoryExist failed: " + sSubTemp + " Error: " + error, ErrorSeverityEnum.Error));
                        }

                    }

                }
                else
                {
                    oReturn.Errors.Add(new ErrorInfo(-1, "IsDirectoryExist failed: " + sDestinationDir + " Error: " + error, ErrorSeverityEnum.Error));
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

        private ResultReturn MoveReqdNoticesFromFtpStagingToFtp(string sPartnerID, NotificationTypeEnum a_eNotificationType)
        {
            ResultReturn oReturn = new();
            ResultReturn oMoveFileReturn;

            List<string> result = new();

            FtpFolderTracking oFtpFolderTracking;
            DocFileInfo oDocFileInfo;
            try
            {
                string sStagingDir = AppSettings.GetValue("ReqdNoticesStagingDirPath" + sPartnerID);
                string sDestinationDir = AppSettings.GetValue("ReqdNoticesDestinationDirPath" + sPartnerID);
                if (sStagingDir == string.Empty || sDestinationDir == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "Invalid Partner ID - " + sPartnerID, ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                FTPUtility oFtp =
                    new FTPUtility(
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
                                    // Only go one-level deep
                                    FTPdirectory oFTPDir2 = oFtp.ListDirectoryDetail(ff.FullName);

                                    foreach (FTPfileInfo ff2 in oFTPDir2)
                                    {
                                        if (ff2.FileType == FTPfileInfo.DirectoryEntryTypes.File)
                                        {
                                            if (!oFtpFolderTracking.IsUnderLimit)
                                            {
                                                oFtpFolderTracking.subFolderCount += 1;
                                                oFtpFolderTracking = GetFtpFolderInfo(oFtp, sDestinationDir);
                                                if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                                {
                                                    throw new Exception("Unable to create Ftp sub folder under : " + sDestinationDir + oFtpFolderTracking.errors[0].errorDesc);
                                                }
                                            }
                                            // Parse File Name to make sure its valid file
                                            oDocFileInfo = _estatement.GetDocFileInfo(sPartnerID, a_eNotificationType, ff2);

                                            if (oDocFileInfo != null)
                                            {

                                                oMoveFileReturn = MoveRequiredNoticeFtpFile(oFtp, ff2, sPartnerID, oFtpFolderTracking.GetCurrentFolder, a_eNotificationType);

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
                                            else
                                            {
                                                //throw new Exception(" Unable to move Ftp file. Invalid File name. : " + ff2.Filename);
                                            }

                                        }
                                    }

                                    break;
                                case FTPfileInfo.DirectoryEntryTypes.File:
                                    if (!oFtpFolderTracking.IsUnderLimit)
                                    {
                                        oFtpFolderTracking = GetFtpFolderInfo(oFtp, sDestinationDir);
                                        if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                        {
                                            throw new Exception("Unable to create Ftp sub folder : " + oFtpFolderTracking.errors[0].errorDesc);
                                        }
                                    }
                                    oMoveFileReturn = MoveRequiredNoticeFtpFile(oFtp, ff, sPartnerID, oFtpFolderTracking.GetCurrentFolder, a_eNotificationType);
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
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "ex: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }

        private ResultReturn MoveRequiredNoticeFtpFile(FTPUtility a_oFtp, FTPfileInfo a_ff, string a_sPartnerId, string a_sTargetFtpFolder, NotificationTypeEnum a_eNotificationType)
        {
            ResultReturn oReturn = new();
            ResultReturn oResRet;
            string sDebug = "s";
            DocFileInfo oDocFileInfo;
            DocIndexFileInfo oIndexFileInfo;
            bool a_bOverWriteExistingFile = false;
            int iSubNotificationType = 0;
            try
            {
                string sErrorFtpFolder = AppSettings.GetValue("ReqdNoticesFTPMoveError" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");
                if (AppSettings.GetValue("ReqdNoticesPDFOverride" + a_sPartnerId) == "1")
                {
                    a_bOverWriteExistingFile = true;
                }

                oDocFileInfo = _estatement.GetDocFileInfo(a_sPartnerId, a_eNotificationType, a_ff);

                if (!string.IsNullOrEmpty(oDocFileInfo.parseError))
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " is not in expected file naming convention." + "\r\n" + oDocFileInfo.parseError, ErrorSeverityEnum.Failed));
                }
                else
                {
                    //2.2 Move file    
                    oResRet = a_oFtp.MoveFtpFile(a_ff, a_sTargetFtpFolder, sErrorFtpFolder, a_bOverWriteExistingFile);

                    General.CopyResultError(oReturn, oResRet);


                    //if (bTemp)
                    if (oResRet.returnStatus != ReturnStatusEnum.Failed)
                    {
                        // update database

                        oIndexFileInfo = new DocIndexFileInfo
                        {
                            contractId = oDocFileInfo.contractId,
                            subId = oDocFileInfo.subId,
                            partnerId = a_sPartnerId,
                            docType = oDocFileInfo.docType,
                            fileSize = Convert.ToInt32((a_ff.Size > 0 ? (Convert.ToDouble(a_ff.Size) / 1024.0) : 0.0)),
                            fileType = a_ff.Extension,
                            downloadType = 100, // FTP Download
                            sysAssignedFilename = Path.Combine(a_sTargetFtpFolder, a_ff.Filename),
                            promptFilename = "RequiredNotices_" + oDocFileInfo.docType.ToString() + "." + a_ff.Extension,
                            fromPeriod = _estatement.GetDate(oDocFileInfo.trxDate, "yyyy-MM-dd"),
                            toPeriod = _estatement.GetDate(oDocFileInfo.trxDate, "yyyy-MM-dd")
                        };

                        oIndexFileInfo.displayDesc = oIndexFileInfo.fromPeriod.ToString("MM/dd/yyyy");

                        oIndexFileInfo.expireDt = oIndexFileInfo.toPeriod.AddYears(1);  // good for 1 years ??

                        oIndexFileInfo.connectParms = "<ConnectParm><ParmId ParmName=\"DiaFtpReqdNotices\">" + oDocFileInfo.docType.ToString() + "</ParmId></ConnectParm>";
                        string feed = GetReqdNoticeFeed(oDocFileInfo.docType, a_sPartnerId, ref iSubNotificationType);
                        oResRet = _oeSDC.InsertDocumentIndexAndDIAFeed(oIndexFileInfo, C_ParticipantReqdNoticesNotificationType, iSubNotificationType, feed);

                        string sErrorInfo;
                        if (oResRet.returnStatus != ReturnStatusEnum.Succeeded)
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            sErrorInfo = "InsertDocumentIndexAndDIAFeed Failed: ";
                            if (oResRet.Errors.Count > 0)
                            {
                                sErrorInfo = sErrorInfo + oResRet.Errors[0].errorDesc + "\r\n";
                            }
                            oReturn.Errors.Add(new ErrorInfo(-1, sErrorInfo, ErrorSeverityEnum.Failed));
                        }
                        else
                        {
                            ResultReturn oRet = SendConsolidatedNotifications(oDocFileInfo.contractId, oDocFileInfo.subId, oDocFileInfo.docType);
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
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "Debug: " + sDebug + " Ex: " + ex.Message + "\r\n", ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;

        }

        private string GetReqdNoticeFeed(int iDocTypeId_orig, string PartnerID, ref int subNotificationType) // same copy of this function exists in ConsolidateNotifications class
        {
            string feed = "";
            if (PartnerID.ToUpper() == "PENCO")
            {
                feed = "";
                switch (iDocTypeId_orig)
                {
                    case 695: //SAR
                        subNotificationType = 5;
                        break;
                    case 696: //SMM
                        subNotificationType = 6;
                        break;
                    case 161: //SPD
                        subNotificationType = 7;
                        break;
                    case 746:
                        subNotificationType = 8;
                        break;
                    default:
                        break;
                }
            }
            return feed;
        }

        private ResultReturn SendConsolidatedNotifications(string a_sConId, string a_sSubId, int a_iDocType)
        {
            ContractServ DriverSOACon = new();
            XElement xEl =
                new("WsDocumentServiceDocumentEx",
                    new XElement("ContractID", a_sConId.Trim()),
                    new XElement("SubID", a_sSubId.Trim()),
                    new XElement("DocTypeCode", a_iDocType)
                    );
            switch (a_iDocType)
            {
                case 695:
                    break;
                default:
                    break;
            }

            ResultReturn oRet = new();
            oRet = DriverSOACon.NotifyToConsolidateMessages(xEl.ToString(), "", "");

            return oRet;
        }

    }
}
