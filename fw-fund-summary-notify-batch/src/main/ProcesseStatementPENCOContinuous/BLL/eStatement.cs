using System.Data;
using System.Text;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace ProcesseStatementPENCOContinuousBatch.BLL;

public class EStatement(TRS.IT.BendProcessor.BLL.eStatement obj)
{
    TRS.IT.BendProcessor.BLL.eStatement eStat = obj;


    private DataSet _dsPptOptin;
    private const int C_eStatementNotificationType = 2;


    public void ProcesseStatementPENCOContinuous()
    {
        TaskStatus oTaskReturn = new();
        TaskStatus oTaskStatus = null;
        const string C_Task = "ProcesseStatementPENCOContinuous";
        StringBuilder strB = new();
        string sSleepMinutes = "";
        int iSleepMinutes = 2;
        try
        {
            oTaskReturn.retStatus = TaskRetStatus.NotRun;
            if (AppSettings.GetValue(C_Task) == "1")
            {
                eStat.InitTaskStatus(oTaskReturn, C_Task);

                sSleepMinutes = AppSettings.GetValue("ContinuousProcessSleepMinutes");
                try
                {
                    iSleepMinutes = Convert.ToInt32(sSleepMinutes);
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    iSleepMinutes = 1;
                }
                if (iSleepMinutes < 1)
                {
                    iSleepMinutes = 1;
                }

                try
                {
                    oTaskStatus = null;
                    if (IsPENCOeStatementFileExists())
                    {
                        oTaskStatus = ProcesseStatementPENCO();
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    if (oTaskStatus == null)
                    {
                        oTaskStatus = new TaskStatus();
                        oTaskStatus.retStatus = TaskRetStatus.Failed;
                    }
                    eStat.InitTaskError(oTaskStatus, ex, true);
                    Thread.Sleep(new TimeSpan(0, iSleepMinutes, 0));
                }


            }
        }
        catch (Exception ex)
        {
            Utils.LogError(ex);
            eStat.InitTaskError(oTaskReturn, ex, true);
        }

    }

    private bool IsPENCOeStatementFileExists()
    {
        string sStagingDir = "";
        string sPartnerID = "PENCO";

        sStagingDir = AppSettings.GetValue(sPartnerID + "StagingDirPath");
        if (sStagingDir == string.Empty)
        {
            return false;
        }
        FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
                     AppSettings.GetVaultValue("FTPUserName"),
                     AppSettings.GetVaultValue("FTPPassword"));
        FTPdirectory oFTPDir = oFtp.ListDirectoryDetail(sStagingDir);
        foreach (FTPfileInfo ff in oFTPDir)
        {
            switch (ff.FileType)
            {
                case FTPfileInfo.DirectoryEntryTypes.Directory:
                    FTPdirectory oFTPDir2 = oFtp.ListDirectoryDetail(ff.FullName);

                    foreach (FTPfileInfo ff2 in oFTPDir2)
                    {
                        if (ff2.FileType == FTPfileInfo.DirectoryEntryTypes.File)
                        {
                            return true;
                        }
                    }
                    break;
                case FTPfileInfo.DirectoryEntryTypes.File:
                    return true;
            }
        }
        return false;
    }
    private TaskStatus ProcesseStatementPENCO()
    {
        TaskStatus oTaskReturn = new();
        TaskStatus oTaskStatus = null;
        const string C_Task = "ProcesseStatementPENCO";

        StringBuilder strB = new();

        try
        {
            oTaskReturn.retStatus = TaskRetStatus.NotRun;
            if (AppSettings.GetValue(C_Task) == "1")
            {
                eStat.InitTaskStatus(oTaskReturn, C_Task);

                oTaskStatus = ProcessMoveStagingFilesPENCO();
                strB.Append(General.ParseTaskInfo(oTaskStatus));

                if (oTaskStatus.retStatus == TaskRetStatus.Failed || oTaskStatus.retStatus == TaskRetStatus.FailedAborted)
                {
                    oTaskReturn.retStatus = TaskRetStatus.Failed;
                    eStat.SendTaskCompleteEmail("eStatement ProcessMoveStagingFilesPENCO Task Status - " + oTaskStatus.retStatus.ToString(), strB.ToString(), oTaskStatus.taskName);
                }
                else
                {
                    oTaskStatus = eStat.ProcessNotifyDIA(C_eStatementNotificationType);
                    strB.Append(General.ParseTaskInfo(oTaskStatus));
                    if (oTaskStatus.retStatus == TaskRetStatus.Failed || oTaskStatus.retStatus == TaskRetStatus.FailedAborted)
                    {
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                        eStat.SendTaskCompleteEmail("eStatement ProcessNotifyDIA PENCO Task Status - " + oTaskStatus.retStatus.ToString(), strB.ToString(), oTaskStatus.taskName);
                    }
                    else
                    {
                        oTaskStatus = eStat.ProcessClearDailyDiaFeed(C_eStatementNotificationType);
                        strB.Append(General.ParseTaskInfo(oTaskStatus));
                    }
                }
                oTaskReturn.endTime = DateTime.Now;
                eStat.SendTaskCompleteEmail("eStatement PENCO Status - " + oTaskReturn.retStatus.ToString(), strB.ToString(), "eStatement Backend Processing");

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
    private TaskStatus ProcessMoveStagingFilesPENCO()
    {
        TaskStatus oTaskReturn = new();
        ResultReturn oReturn;
        const string C_Task = "ProcessMoveStagingFilesPENCO";

        try
        {
            oTaskReturn.retStatus = TaskRetStatus.NotRun;
            if (AppSettings.GetValue(C_Task) == "1")
            {
                eStat.InitTaskStatus(oTaskReturn, C_Task);

                oReturn = MoveeSatementsFromStagingFtp2(ConstN.C_PARTNER_PENCO);
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
            eStat.InitTaskError(oTaskReturn, ex, true);
        }

        oTaskReturn.endTime = DateTime.Now;

        return oTaskReturn;

    }
    private ResultReturn MoveeSatementsFromStagingFtp2(string sPartnerID)
    {
        ResultReturn oReturn = new();
        ResultReturn oMoveFileReturn;

        List<string> result = new();

        string sStagingDir = "";
        string sDestinationDir = "";
        FtpFolderTracking oFtpFolderTracking;
        FileInfo oFileInfo = null;

        try
        {
            sStagingDir = AppSettings.GetValue(sPartnerID + "StagingDirPath");
            sDestinationDir = AppSettings.GetValue(sPartnerID + "DestinationDirPath");
            if (sStagingDir == string.Empty || sDestinationDir == string.Empty)
            {
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "Invalid Partner ID - " + sPartnerID, ErrorSeverityEnum.Failed));
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
                                if (sPartnerID != "TAE" || IsDirectoryInConfig(ff.FullName))
                                {
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
                                            oMoveFileReturn = MoveFtpFile2(oFtp, oFileInfo, ff2, sPartnerID, oFtpFolderTracking.GetCurrentFolder);
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
                                oMoveFileReturn = MoveFtpFile2(oFtp, oFileInfo, ff, sPartnerID, oFtpFolderTracking.GetCurrentFolder);
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

    private ResultReturn MoveFtpFile2(FTPUtility a_oFtp, FileInfo a_oFileInfo, FTPfileInfo a_ff, string a_sPartnerId, string a_sTargetFtpFolder)
    {
        ResultReturn oReturn = new();
        ResultReturn oResRet;
        string[] s_arr;
        PartStFeedInfo oPptStFeedInfo;
        string sErrorInfo = "";
        bool bTemp = false;
        DocIndexFileInfo oIndexFileInfo;
        string sConId = "";
        string sSubId = "";
        string sSSN = "";
        string sFromPeriod = "";
        string sDocType = "";
        long lFileSize = 0;
        string sFileName = "";
        string sFileExtension = "";
        bool aValidLength = false;
        string sDebug = "s";
        bool bMovedOk = false;
        string sCouldNotMoveError = "";
        string sErr = "";
        string sErrorFtpFolder = "";
        try
        {
            sErrorFtpFolder = AppSettings.GetValue("FTPMoveError" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");

            if (a_sPartnerId == ConstN.C_PARTNER_TAE)
            {
                if (a_oFileInfo == null)
                {
                    s_arr = a_ff.NameOnly.Split(['_']);
                    lFileSize = a_ff.Size;
                    sFileName = a_ff.Filename;
                    sFileExtension = a_ff.Extension;
                }
                else
                {
                    s_arr = a_oFileInfo.Name.Split('_');
                    lFileSize = a_oFileInfo.Length;
                    sFileName = a_oFileInfo.Name;
                    sFileExtension = a_oFileInfo.Extension;

                }
                if (s_arr.Length == 6)
                {
                    sConId = eStat.RemoveLeadingZeros(s_arr[1].Trim());
                    sSubId = s_arr[2].Trim();
                    sSSN = s_arr[3];
                    sFromPeriod = s_arr[4];
                    sDocType = s_arr[5];
                    aValidLength = true;
                }
            }
            else if (a_sPartnerId == ConstN.C_PARTNER_PENCO)
            {
                s_arr = a_ff.NameOnly.Split(['_']);

                if (s_arr.Length == 5)
                {
                    sConId = eStat.RemoveLeadingZeros(s_arr[0].Trim());
                    sSubId = s_arr[1].Trim();
                    sSSN = s_arr[2];
                    sFromPeriod = s_arr[3];
                    sDocType = s_arr[4];
                    lFileSize = a_ff.Size;
                    sFileName = a_ff.Filename;
                    sFileExtension = a_ff.Extension;
                    aValidLength = true;
                }
            }

            if (aValidLength)
            {

                oPptStFeedInfo = eStat.GetPptFeedInfo(sSSN, sConId, sSubId, NotificationTypeEnum.eStatement);
                sDebug += ";oPptStFeedInfo";
                if (oPptStFeedInfo.found)
                {
                    if (a_oFileInfo == null)
                    {
                        try
                        {
                            bMovedOk = a_oFtp.FtpRename(a_ff.FullName, Path.Combine(a_sTargetFtpFolder, a_ff.Filename));
                            sDebug += "try";
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex);
                            sErr = "";
                            sCouldNotMoveError = ex.Message;
                            if (ex.Message.Contains("already exists"))
                            {
                                sDebug += ";exist";

                                if (eStat.IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                                {
                                    sDebug += ";FTPMoveError";
                                    string sDuplicateFolder = sErrorFtpFolder + "/Duplicate";
                                    if (eStat.IsDirectoryExist(a_oFtp, sDuplicateFolder, true, ref sErrorInfo))
                                    {
                                        sDebug += ";Duplicate";
                                        if (AppSettings.GetValue("eStatementPDFOverride") == "1")
                                        {
                                            bTemp = a_oFtp.FtpRename(Path.Combine(a_sTargetFtpFolder, a_ff.Filename), sDuplicateFolder + "/" + a_ff.Filename, ref sErr, true);
                                            sDebug += ";Target2Dup";
                                            if (!bTemp)
                                            {
                                                if (sErr.Contains("already exists"))
                                                {
                                                    bTemp = a_oFtp.FtpRename(Path.Combine(a_sTargetFtpFolder, a_ff.Filename), sDuplicateFolder + "/" + a_ff.NameOnly + DateTime.Now.ToString("_HHmmss.") + a_ff.Extension, ref sErr, true);
                                                }
                                                if (!bTemp)
                                                {
                                                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to diplicate folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                                                }
                                            }

                                            if (bTemp)
                                            {
                                                bMovedOk = a_oFtp.FtpRename(a_ff.FullName, Path.Combine(a_sTargetFtpFolder, a_ff.Filename), ref sErr, true);
                                                sDebug += ";Source2Target";
                                                if (!bMovedOk)
                                                {
                                                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to target folder after moving existing file to duplicate folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
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
                                            bTemp = a_oFtp.FtpRename(a_ff.FullName, sDuplicateFolder + "/" + a_ff.Filename, ref sErr, true);
                                            sDebug += ";DupOnly";
                                            if (sErr.Contains("already exists"))
                                            {
                                                bTemp = a_oFtp.FtpRename(a_ff.FullName, sDuplicateFolder + "/" + a_ff.NameOnly + DateTime.Now.ToString("_HHmmss.") + a_ff.Extension, ref sErr, true);
                                            }
                                            if (!bTemp)
                                            {
                                                oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                                            }
                                        }

                                    }
                                    else
                                    {
                                        oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + sDuplicateFolder + " SSN:" + sSSN + "  C: " + sConId + " S: " + sSubId, ErrorSeverityEnum.Failed));
                                    }
                                }
                                else
                                {
                                    oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + sErrorFtpFolder + " SSN:" + sSSN + "  C: " + sConId + " S: " + sSubId, ErrorSeverityEnum.Failed));
                                }
                            }

                            throw new Exception(a_ff.FullName + "Debug: " + sDebug + " Ex: " + ex.Message);
                        }
                    }
                    else
                    {
                        if (a_sPartnerId == ConstN.C_PARTNER_TAE)
                        {
                            bMovedOk = a_oFtp.UploadFile(a_oFileInfo.FullName, a_sTargetFtpFolder + a_oFileInfo.Name, ref sErrorInfo);
                            if (bMovedOk)
                            {
                                File.Delete(a_oFileInfo.FullName);
                            }
                        }
                    }
                    if (bMovedOk)
                    {
                        oIndexFileInfo = new DocIndexFileInfo();
                        oIndexFileInfo.contractId = sConId;
                        oIndexFileInfo.subId = sSubId;
                        oIndexFileInfo.partnerId = a_sPartnerId;
                        oIndexFileInfo.docType = Convert.ToInt32(sDocType);
                        oIndexFileInfo.fileSize = Convert.ToInt32((lFileSize > 0 ? (Convert.ToDouble(lFileSize) / 1024.0) : 0.0));
                        oIndexFileInfo.fileType = sFileExtension;
                        oIndexFileInfo.downloadType = 100;
                        oIndexFileInfo.sysAssignedFilename = a_sTargetFtpFolder + sFileName;
                        oIndexFileInfo.promptFilename = "QuarterlyStatement." + sFileExtension;
                        oIndexFileInfo.toPeriod = eStat.GetDate(sFromPeriod, "MMDDYYYY");
                        oIndexFileInfo.fromPeriod = AdjustFromDate(oIndexFileInfo.toPeriod);
                        oIndexFileInfo.displayDesc = "From "
                            + oIndexFileInfo.fromPeriod.ToString("MM/dd/yyyy") + " To " + oIndexFileInfo.toPeriod.ToString("MM/dd/yyyy");
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
                        oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + "  could not be moved to :   " + a_sTargetFtpFolder + a_ff.Filename + "(ex: " + sCouldNotMoveError + ")\r\n", ErrorSeverityEnum.Failed));
                    }

                }
                else
                {
                    oReturn.Errors.Add(new ErrorInfo(-1, "PPT not found " + sSSN + " C: " + sConId + " S: " + sSubId, ErrorSeverityEnum.Failed));
                    if (a_oFileInfo == null)
                    {

                        if (eStat.IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                        {
                            sErrorInfo = "";
                            bTemp = a_oFtp.FtpRename(a_ff.FullName, sErrorFtpFolder + "/" + a_ff.Filename, ref sErrorInfo, true);
                            if (!bTemp)
                            {
                                if (sErrorInfo.Contains("already exists"))
                                {
                                    bTemp = a_oFtp.FtpRename(a_ff.FullName, sErrorFtpFolder + "/" + a_ff.NameOnly + DateTime.Now.ToString("_HHmmss.") + a_ff.Extension, ref sErrorInfo, true);
                                }
                                if (!bTemp)
                                {
                                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder (ppt not found) . PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                                }
                            }
                        }
                        else
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + sErrorFtpFolder + " SSN:" + sSSN + "  C: " + sConId + " S: " + sSubId, ErrorSeverityEnum.Failed));
                        }
                    }
                }

            }
            else
            {
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " is not in expected file naming convention. File Moved to " + sErrorFtpFolder + " folder. \r\n", ErrorSeverityEnum.Failed));
                if (eStat.IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                {
                    sErrorInfo = "";
                    bTemp = a_oFtp.FtpRename(a_ff.FullName, sErrorFtpFolder + "/" + a_ff.Filename, ref sErrorInfo, true);
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
    private bool IsDirectoryInConfig(string sDirectoryName)
    {
        bool bRet = false;
        string sTAESubFolders = AppSettings.GetValue("eStatementTAESubFolders");

        if (sTAESubFolders == "")
        {
            return true;
        }

        string fullPath = Path.GetFullPath(sDirectoryName).TrimEnd(Path.DirectorySeparatorChar);
        string sSub = Path.GetFileName(fullPath);

        if (sTAESubFolders.IndexOf(sSub) > -1)
        {
            bRet = true;
        }

        return bRet;
    }
    
}
