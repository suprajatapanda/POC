using System.Data;
using System.Text;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace DailyeConfirmMainBatch.BLL
{
    public class eStatement
    {
        TRS.IT.BendProcessor.BLL.eStatement eStat;
        public eStatement(TRS.IT.BendProcessor.BLL.eStatement obj)
        {
            eStat = obj;
        }
        private eStatementDC _oeSDC = new();
        private DataSet _dsPptOptin;
        private string _sContact_GAC = AppSettings.GetValue("ProductFamilyContactGAC");
        private string _sContact_NAV = AppSettings.GetValue("ProductFamilyContactNAV");
        private const int C_eConfirmNotificationType = 1;
        private const int C_eStatementNotificationType = 2;
        private const int C_ParticipantReqdNoticesNotificationType = 4;
        private const int C_ParticipantReqdNoticesNotificationType_ISC = 5;

        public TaskStatus ProcesseConfirmMain()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oResultReturn;
            string[] sPartners = AppSettings.GetValue("eConfirmPartners").Split(';');
            TaskStatus oTaskStatusNotifyDIA = null;
            TaskStatus oTaskStatusClearFeed = null;
            TaskStatus oTaskStatus = new();
            const string C_Task = "ProcesseConfirmMain";

            StringBuilder strB = new();

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    eStat.InitTaskStatus(oTaskReturn, C_Task);
                    foreach (string s in sPartners)
                    {
                        oTaskStatus.startTime = DateTime.Now;
                        oTaskStatus.taskName = "eConfirmMoveFromStagingToFTP" + s;
                        oTaskStatus.retStatus = TaskRetStatus.Succeeded;
                        oResultReturn = eConfirmMoveFromStagingToFTP(s);
                        General.CopyResultError(oTaskReturn, oResultReturn);
                        General.CopyResultError(oTaskStatus, oResultReturn);
                        oTaskStatus.rowsCount = oResultReturn.rowsCount;
                        oTaskStatus.endTime = DateTime.Now;
                        strB.Append(General.ParseTaskInfo(oTaskStatus));

                    }
                    foreach (string s in sPartners)
                    {
                        oTaskStatus.startTime = DateTime.Now;
                        oTaskStatus.taskName = "eConfirmMoveStagingFTPToTarget" + s;
                        oTaskStatus.retStatus = TaskRetStatus.Succeeded;
                        oResultReturn = eConfirmFromFtpStagingToFtp(s);
                        General.CopyResultError(oTaskReturn, oResultReturn);
                        General.CopyResultError(oTaskStatus, oResultReturn);
                        oTaskStatus.rowsCount = oResultReturn.rowsCount;
                        oTaskStatus.endTime = DateTime.Now;
                        strB.Append(General.ParseTaskInfo(oTaskStatus));
                    }

                    oTaskStatusNotifyDIA = eStat.ProcessNotifyDIA(C_eConfirmNotificationType);
                    if (oTaskStatusNotifyDIA.retStatus == TaskRetStatus.Failed || oTaskStatusNotifyDIA.retStatus == TaskRetStatus.FailedAborted)
                    {
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                    }
                    else
                    {
                        oTaskStatusClearFeed = eStat.ProcessClearDailyDiaFeed(C_eConfirmNotificationType);

                    }
                    oTaskReturn.endTime = DateTime.Now;
                    if (oTaskStatusNotifyDIA != null)
                    {
                        strB.Append(General.ParseTaskInfo(oTaskStatusNotifyDIA));
                    }

                    if (oTaskStatusClearFeed != null)
                    {
                        strB.Append(General.ParseTaskInfo(oTaskStatusClearFeed));
                    }

                    strB.Append(General.ParseTaskInfo(oTaskReturn));

                    eStat.SendTaskCompleteEmail("eConfirm Status - " + oTaskReturn.retStatus.ToString(), strB.ToString(), "eConfirm Back-end Processing");

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
                eStat.InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;

            return oTaskReturn;



        }
        private ResultReturn eConfirmMoveFromStagingToFTP(string a_sPartnerId)
        {
            ResultReturn oReturn = new();
            string sDestinationDir;
            SortedDictionary<string, int> dictFolder = new();

            string sRootFolder = AppSettings.GetValue("eConfirmStagingFolder" + a_sPartnerId);
            string sNoDel = AppSettings.GetValue("eConfirmStagingFolderNoDelete");
            string sError = "";
            bool bIsGood;

            sDestinationDir = AppSettings.GetValue("eConfirmStagingDirPath" + a_sPartnerId);
            if (sRootFolder == string.Empty || sDestinationDir == string.Empty)
            {
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "Missing connection setting entries ", ErrorSeverityEnum.Failed));
                return oReturn;
            }

            FTPUtility oFtp = new FTPUtility(
                AppSettings.GetVaultValue("FTPHostName"),
                AppSettings.GetVaultValue("FTPUserName"),
                AppSettings.GetVaultValue("FTPPassword")
                );

            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            try
            {
                string[] Files = Directory.GetFiles(sRootFolder);
                foreach (string sFileName in Files)
                {
                    try
                    {
                        bIsGood = oFtp.UploadFile(sFileName, sDestinationDir + "/" + Path.GetFileName(sFileName), ref sError);
                        if (bIsGood)
                        {
                            oReturn.rowsCount++;
                            if (sNoDel != "1")
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
                        oReturn.Errors.Add(new ErrorInfo(-1, "Ex: " + ex.Message + " Unable to upload to staging file: " + sFileName, ErrorSeverityEnum.ExceptionRaised));
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
        private ResultReturn eConfirmFromFtpStagingToFtp(string a_sPartnerID)
        {
            ResultReturn oReturn = new();
            ResultReturn oMoveFileReturn;

            List<string> result = new();

            string sStagingDir = "";
            string sDestinationDir = "";
            FtpFolderTracking oFtpFolderTracking;

            try
            {
                sStagingDir = AppSettings.GetValue("eConfirmStagingDirPath" + a_sPartnerID);
                sDestinationDir = AppSettings.GetValue("eConfirmDestinationDirPath" + a_sPartnerID);
                if (sStagingDir == string.Empty || sDestinationDir == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "Invalid Partner ID - " + a_sPartnerID, ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
             AppSettings.GetVaultValue("FTPUserName"),
             AppSettings.GetVaultValue("FTPPassword"));

                FTPdirectory oFTPDir = oFtp.ListDirectoryDetail(sStagingDir);

                oFtpFolderTracking = eStat.GetFtpFolderInfo(oFtp, sDestinationDir);
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
                                                oFtpFolderTracking = eStat.GetFtpFolderInfo(oFtp, sDestinationDir);
                                                if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                                {
                                                    throw new Exception("Unable to create Ftp sub folder : " + oFtpFolderTracking.errors[0].errorDesc);
                                                }
                                            }
                                            oMoveFileReturn = eConfirmMoveFtpFile(oFtp, ff2, a_sPartnerID, oFtpFolderTracking.GetCurrentFolder);
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
                                        oFtpFolderTracking = eStat.GetFtpFolderInfo(oFtp, sDestinationDir);
                                        if (oFtpFolderTracking.returnStatus != ReturnStatusEnum.Succeeded)
                                        {
                                            throw new Exception("Unable to create Ftp sub folder : " + oFtpFolderTracking.errors[0].errorDesc);
                                        }
                                    }
                                    oMoveFileReturn = eConfirmMoveFtpFile(oFtp, ff, a_sPartnerID, oFtpFolderTracking.GetCurrentFolder);
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
        private ResultReturn eConfirmMoveFtpFile(FTPUtility a_oFtp, FTPfileInfo a_ff, string a_sPartnerId, string a_sTargetFtpFolder)
        {
            ResultReturn oReturn = new();
            ResultReturn oResRet;
            PartStFeedInfo oPptStFeedInfo;
            string sErrorInfo = "";
            bool bTemp = false;
            DocIndexFileInfo oIndexFileInfo;
            long lFileSize = 0;
            string sFileName = "";
            string sFileExtension = "";
            string sDebug = "s";

            DocFileInfo oDocFileInfo;
            try
            {
                oDocFileInfo = GetDocFileInfo(a_ff.Filename);

                if (!string.IsNullOrEmpty(oDocFileInfo.parseError))
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " is not in expected file naming convention." + "\r\n" + oDocFileInfo.parseError, ErrorSeverityEnum.Failed));
                }
                else
                {
                    lFileSize = a_ff.Size;
                    sFileName = a_ff.Filename;
                    sFileExtension = a_ff.Extension;

                    oPptStFeedInfo = eStat.GetPptFeedInfo(oDocFileInfo.ssn, oDocFileInfo.contractId, oDocFileInfo.subId, NotificationTypeEnum.eConfirm);
                    sDebug += ";oPptStFeedInfo";
                    if (oPptStFeedInfo.found)
                    {
                        try
                        {
                            bTemp = a_oFtp.FtpRename(a_ff.FullName, a_sTargetFtpFolder + a_ff.Filename);
                            sDebug += "try";

                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex);
                            if (ex.Message.Contains("already exists"))
                            {
                                sDebug += ";exist";
                                string sErrorFtpFolder = AppSettings.GetValue("eConfirmFTPMoveError" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");
                                if (eStat.IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                                {
                                    sDebug += ";FTPMoveError";
                                    string sDuplicateFolder = sErrorFtpFolder + "/Duplicate";
                                    if (eStat.IsDirectoryExist(a_oFtp, sDuplicateFolder, true, ref sErrorInfo))
                                    {
                                        sDebug += ";Duplicate";
                                        if (AppSettings.GetValue("eConfirmPDFOverride") == "1")
                                        {
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
                            oIndexFileInfo = new DocIndexFileInfo();
                            oIndexFileInfo.contractId = oDocFileInfo.contractId;
                            oIndexFileInfo.subId = oDocFileInfo.subId;
                            oIndexFileInfo.partnerId = a_sPartnerId;
                            oIndexFileInfo.docType = oDocFileInfo.docType;
                            oIndexFileInfo.fileSize = Convert.ToInt32((lFileSize > 0 ? (Convert.ToDouble(lFileSize) / 1024.0) : 0.0));
                            oIndexFileInfo.fileType = sFileExtension;
                            oIndexFileInfo.downloadType = 100;
                            oIndexFileInfo.sysAssignedFilename = a_sTargetFtpFolder + sFileName;
                            oIndexFileInfo.promptFilename = "eConfirm." + sFileExtension;
                            oIndexFileInfo.transId = oDocFileInfo.transId;
                            oIndexFileInfo.fromPeriod = Convert.ToDateTime(oDocFileInfo.trxDate);
                            oIndexFileInfo.toPeriod = oIndexFileInfo.fromPeriod;
                            oIndexFileInfo.displayDesc = oIndexFileInfo.fromPeriod.ToString("MM/dd/yyyy");
                            oIndexFileInfo.expireDt = DateTime.Now.AddYears(2);

                            oResRet = eStat.PptStatementAvailable(oPptStFeedInfo, oIndexFileInfo);
                            sDebug += ";PptStatementAvailable";
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
                        string sErrorFtpFolder = AppSettings.GetValue("eConfirmFTPMoveError" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");
                        if (eStat.IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
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
        private DocFileInfo GetDocFileInfo(string a_sFileName)
        {
            DocFileInfo oDocFileInfo = new();
            string sFileName = Path.GetFileName(a_sFileName);
            int iEnd, iIndex, iIndex1, iIndex2;
            string sTagName1;
            string sVal;
            const int C_iOffset = 3;

            try
            {

                iEnd = sFileName.ToUpper().IndexOf("END");
                if (iEnd < 0)
                {
                    iEnd = sFileName.IndexOf(".");
                }

                sFileName = sFileName.Substring(0, iEnd);
                iIndex = 1;
                while (iIndex > 0)
                {
                    iIndex1 = sFileName.IndexOf("$", iIndex);
                    iIndex = iIndex1 + 1;
                    if (iIndex1 > 1)
                    {
                        sTagName1 = sFileName.Substring(iIndex1 - 2, 3);
                        iIndex2 = sFileName.IndexOf("$", iIndex1 + 1);
                        if (iIndex2 < 0)
                        {
                            iIndex2 = iEnd + 2;
                        }

                        if (iIndex2 > iIndex1)
                        {
                            sVal = sFileName.Substring(iIndex1 + 1, iIndex2 - (iIndex1 + C_iOffset));
                            switch (sTagName1.ToUpper())
                            {
                                case "CN$":
                                    oDocFileInfo.contractId = sVal;
                                    break;
                                case "SC$":
                                    oDocFileInfo.subId = sVal;
                                    break;
                                case "SN$":
                                    oDocFileInfo.ssn = sVal;
                                    break;
                                case "DT$":
                                    oDocFileInfo.docType = Convert.ToInt32(sVal);
                                    break;
                                case "DR$":
                                    oDocFileInfo.trxDate = sVal;
                                    break;
                                case "TN$":
                                    oDocFileInfo.transId = sVal;
                                    break;

                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(oDocFileInfo.subId))
                {
                    oDocFileInfo.subId = "000";
                }

                if (string.IsNullOrEmpty(oDocFileInfo.contractId) || string.IsNullOrEmpty(oDocFileInfo.ssn) || oDocFileInfo.docType == 0)
                {
                    oDocFileInfo.parseError = "Error parsing file";
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oDocFileInfo.parseError = "Parse error: " + ex.Message;
            }

            return oDocFileInfo;
        }


    }

}
