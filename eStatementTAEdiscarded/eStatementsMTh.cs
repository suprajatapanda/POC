using System.Data;
using System.Text;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace bend_fund_wizard_poc.eStatementTAEdiscarded
{
    public class eStatementMTh : BendProcessorBase
    {
        public eStatementMTh() : base("54", "eStatementMTh", "TRS") { }

        private eStatementDC _oeSDC = new();
        private DataSet _dsPptOptin;
        private string _sContact_GAC = AppSettings.GetValue("ProductFamilyContactGAC");
        private string _sContact_NAV = AppSettings.GetValue("ProductFamilyContactNAV");
        private const int C_eStatementNotificationType = 2;

        public void ProcesseStatementTAEContinuous(object objStageFolderPath)
        {
            string sStageFolderPath = objStageFolderPath.ToString();
            TaskStatus oTaskReturn = new();
            TaskStatus oTaskStatus = null;
            const string C_Task = "ProcesseStatementTAEContinuous";
            StringBuilder strB = new();
            string sSleepMinutes = "";
            int iSleepMinutes = 2;
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

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

                    bool bLoop = true;
                    while (bLoop)
                    {
                        try
                        {
                            oTaskStatus = null;
                            if (IsTAEeStatementFileExists(sStageFolderPath))
                            {
                                oTaskStatus = ProcesseStatementTAE(sStageFolderPath);// this call will process max 10000 files but since this is in infinite while loop it will be called again.
                            }
                            else
                            {
                                Thread.Sleep(new TimeSpan(0, iSleepMinutes, 0)); // Take 1 minute rest. 
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
                            InitTaskError(oTaskStatus, ex, true);
                            Thread.Sleep(new TimeSpan(0, iSleepMinutes, 0));
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }

        }
        private TaskStatus ProcesseStatementTAE(string sStageFolderPath)
        {
            TaskStatus oTaskReturn = new();
            TaskStatus oTaskStatus = null;
            const string C_Task = "ProcesseStatementTAE";

            StringBuilder strB = new();

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    oTaskStatus = ProcessMoveStagingFilesTAE(sStageFolderPath);
                    strB.Append(General.ParseTaskInfo(oTaskStatus));

                    if (oTaskStatus.retStatus == TaskRetStatus.Failed || oTaskStatus.retStatus == TaskRetStatus.FailedAborted)
                    {
                        //send error
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                        SendTaskCompleteEmail("eStatement ProcessMoveStagingFilesTAE Task Status - " + oTaskStatus.retStatus.ToString() + " - " + sStageFolderPath, strB.ToString(), oTaskStatus.taskName + " - " + sStageFolderPath);
                    }
                    else
                    {
                        oTaskStatus = ProcessNotifyDIA(C_eStatementNotificationType);
                        strB.Append(General.ParseTaskInfo(oTaskStatus));
                        if (oTaskStatus.retStatus == TaskRetStatus.Failed || oTaskStatus.retStatus == TaskRetStatus.FailedAborted)
                        {
                            oTaskReturn.retStatus = TaskRetStatus.Failed;
                            //send error
                            SendTaskCompleteEmail("eStatement ProcessNotifyDIA TAE Task Status - " + oTaskStatus.retStatus.ToString() + " - " + sStageFolderPath, strB.ToString(), oTaskStatus.taskName + " - " + sStageFolderPath);
                        }
                        else
                        {
                            oTaskStatus = ProcessClearDailyDiaFeed(C_eStatementNotificationType);
                            strB.Append(General.ParseTaskInfo(oTaskStatus));
                        }
                    }
                    oTaskReturn.endTime = DateTime.Now;
                    SendTaskCompleteEmail("eStatement TAE Status - " + oTaskReturn.retStatus.ToString() + " - " + sStageFolderPath, strB.ToString(), "eStatement Backend Processing" + " - " + sStageFolderPath);

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
        private TaskStatus ProcessMoveStagingFilesTAE(string sStageFolderPath)
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            const string C_Task = "ProcessMoveStagingFilesTAE";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    oReturn = MoveFromTAEFilesToStaging(sStageFolderPath);
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded || oReturn.Errors.Count > 0)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    oTaskReturn.rowsCount += oReturn.rowsCount;

                    oReturn = MoveeSatementsFromStagingFtp2(ConstN.C_PARTNER_TAE, sStageFolderPath);
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
        private TaskStatus ProcessNotifyDIA(int a_iNotificationType)
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            const string C_Task = "ProcessNotifyDIA";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    oReturn = GetDIAFeed(a_iNotificationType);
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.fatalErrCnt += 1;
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                    }
                    else
                    {
                        oTaskReturn.rowsCount = oReturn.rowsCount;
                        oTaskReturn.retStatus = TaskRetStatus.Succeeded;

                        // Do not upload if rowCount = 0
                        if (oReturn.rowsCount > 0)
                        {
                            string sError = "";

                            
                            FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
                             AppSettings.GetVaultValue("FTPUserName"),
                             AppSettings.GetVaultValue("FTPPassword"));

                            bool b = false;
                            b = oFtp.UploadFile(oReturn.confirmationNo, AppSettings.GetValue("FTPDIAFeedFolder") + Path.GetFileName(oReturn.confirmationNo), ref sError);
                            if (!b)
                            {
                                oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                oTaskReturn.errors.Add(new ErrorInfo(-1, "Failed to upload feed file to DIA - " + sError, ErrorSeverityEnum.Error));
                            }
                        }
                        else
                        {
                            oTaskReturn.errors.Add(new ErrorInfo(1, "Did not upload file to DIA because file is empty.", ErrorSeverityEnum.Warning));
                        }
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
        private TaskStatus ProcessClearDailyDiaFeed(int a_iNotificationType)
        {
           TaskStatus oTaskReturn = new();

            const string C_Task = "ProcessClearDailyDiaFeed";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    oTaskReturn.rowsCount = _oeSDC.ClearDailyDiaFeed(a_iNotificationType);
                    oTaskReturn.retStatus = TaskRetStatus.Succeeded;
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

        private bool IsTAEeStatementFileExists(string sStageFolderPath)
        {
            bool bRet = false;

            string sRootFolder = sStageFolderPath;

            if (sRootFolder == string.Empty)
            {
                return false;
            }

            if (!Directory.Exists(sRootFolder))
            {
                return false;
            }

            bRet = Directory.EnumerateFileSystemEntries(sRootFolder).Any();
            return bRet;
        }
        private ResultReturn MoveFromTAEFilesToStaging(string sStageFolderPath)
        {
            ResultReturn oReturn = new();
            string sDestinationDir;

            string sNoTAEDel = AppSettings.GetValue("TAEStagingFolderNoDelete");
            string sError = "";
            bool bIsGood;
            string sSubTemp;
            int iMaxFilePerSession = 10000;
            int iMaxFileCnt = 0;

            if (!string.IsNullOrEmpty(AppSettings.GetValue("TAEStagingMaxFilesPerSession")))
            {
                iMaxFilePerSession = Convert.ToInt32(AppSettings.GetValue("TAEStagingMaxFilesPerSession"));
            }

            sDestinationDir = AppSettings.GetValue(ConstN.C_PARTNER_TAE + "StagingDirPath");
            if (sDestinationDir == string.Empty)
            {
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "Missing connection setting entries TAEStagingDirPath", ErrorSeverityEnum.Failed));
                return oReturn;
            }

            
            FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
             AppSettings.GetVaultValue("FTPUserName"),
             AppSettings.GetVaultValue("FTPPassword"));

            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            try
            {
                string fullPath = Path.GetFullPath(sStageFolderPath).TrimEnd(Path.DirectorySeparatorChar);
                string sSub = Path.GetFileName(fullPath);// gets the last sub directory name from path

                sSubTemp = sDestinationDir + sSub.Substring(sSub.LastIndexOf(@"\") + 1);
                if (IsDirectoryExist(oFtp, sSubTemp, true, ref sError))
                {
                    string[] Files = Directory.GetFiles(fullPath);
                    //var Files = Directory.
                    foreach (string sFileName in Files)
                    {
                        try
                        {
                            bIsGood = oFtp.UploadFile(sFileName, Path.Combine(sSubTemp, Path.GetFileName(sFileName)), ref sError);
                            if (bIsGood)
                            {
                                iMaxFileCnt++;
                                if (sNoTAEDel != "1")
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
                            oReturn.Errors.Add(new ErrorInfo(-1, "Unable to upload to staging file: " + sFileName + " Exception: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
                        }
                        if (iMaxFileCnt > iMaxFilePerSession) //limit processing because of dia ftp issue
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if (sError != "")
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.confirmationNo = string.Empty;
                        oReturn.Errors.Add(new ErrorInfo(-1, sError, ErrorSeverityEnum.Error));
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
        private ResultReturn MoveeSatementsFromStagingFtp2(string sPartnerID, string sStageFolderPath)
        {
            ResultReturn oReturn = new();
            ResultReturn oMoveFileReturn;
            string sError = "";
            List<string> result = new();
            string sFtpSourceFolderPath = "";
            string sDestinationDir = "";
            FtpFolderTracking oFtpFolderTracking;
            FileInfo oFileInfo = null;

            try
            {

                sDestinationDir = AppSettings.GetValue(sPartnerID + "DestinationDirPath");
                //// Note: parnerid_DestinationDirPath is a path where we upload files temporarily on ftp server, we did it in MoveFromTAEFilesToStaging() function. Now it becomes source path
                sFtpSourceFolderPath = AppSettings.GetValue(sPartnerID + "StagingDirPath");

                if (sFtpSourceFolderPath == string.Empty || sDestinationDir == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "Missing config setting - DestinationDirPath or Invalid Partner ID - " + sPartnerID, ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                
                FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
                 AppSettings.GetVaultValue("FTPUserName"),
                 AppSettings.GetVaultValue("FTPPassword"));

                string fullPath = Path.GetFullPath(sStageFolderPath).TrimEnd(Path.DirectorySeparatorChar);
                string sSub = Path.GetFileName(fullPath);// gets the last sub directory name from path

                sFtpSourceFolderPath = sFtpSourceFolderPath + sSub.Substring(sSub.LastIndexOf(@"\") + 1) + "/"; // final ftp sourcepath
                if (IsDirectoryExist(oFtp, sFtpSourceFolderPath, false, ref sError) == false)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, " - Invalid Source FTP Directory Path: " + sFtpSourceFolderPath + " OR Invalid Partner ID - " + sPartnerID, ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                if (IsDirectoryExist(oFtp, sDestinationDir, true, ref sError))
                {
                    sDestinationDir = sDestinationDir + sSub.Substring(sSub.LastIndexOf(@"\") + 1) + "/"; // final ftp Destination path

                    if (IsDirectoryExist(oFtp, sDestinationDir, true, ref sError))
                    {
                        ////1. Get list of files to move from Staging folder.
                        FTPdirectory oFTPDir = oFtp.ListDirectoryDetail(sFtpSourceFolderPath);

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

                            }//foreach
                        }//oFtpFolderTracking
                        else
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, oFtpFolderTracking.errors[0].errorDesc, ErrorSeverityEnum.Failed));
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                        }
                    }

                }
                else
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, " - Invalid Destination FTP Directory Path: " + sDestinationDir, ErrorSeverityEnum.Failed));
                    return oReturn;
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
            //string newFtpDirName = "";
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

            try
            {
                if (a_sPartnerId == ConstN.C_PARTNER_TAE)
                {
                    //PPPP_CCCCC_SSS_MMMMMMMMM_mmddccyy_101.PDF
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
                        sConId = RemoveLeadingZeros(s_arr[1].Trim());
                        sSubId = s_arr[2].Trim();
                        sSSN = s_arr[3];
                        sFromPeriod = s_arr[4];
                        sDocType = s_arr[5];
                        aValidLength = true;
                    }

                }
                else if (a_sPartnerId == ConstN.C_PARTNER_PENCO)
                {
                    // 300069_000_111222333_08312010_101.pdf
                    s_arr = a_ff.NameOnly.Split(['_']);

                    if (s_arr.Length == 5)
                    {
                        sConId = RemoveLeadingZeros(s_arr[0].Trim());
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


                if (aValidLength)//else ...not a valid file to move
                {

                    oPptStFeedInfo = GetPptFeedInfo(sSSN, sConId, sSubId, NotificationTypeEnum.eStatement);
                    sDebug += ";oPptStFeedInfo";
                    if (oPptStFeedInfo.found)
                    {
                        //2.2 Move file                                    
                        if (a_oFileInfo == null)
                        {
                            //oPptStFeedInfo.notificationType = 2;
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
                                    string sErrorFtpFolder = AppSettings.GetValue("FTPMoveError" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");
                                    if (IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                                    {
                                        sDebug += ";FTPMoveError";
                                        string sDuplicateFolder = sErrorFtpFolder + "/Duplicate";
                                        if (IsDirectoryExist(a_oFtp, sDuplicateFolder, true, ref sErrorInfo))
                                        {
                                            sDebug += ";Duplicate";
                                            if (AppSettings.GetValue("eStatementPDFOverride") == "1")
                                            {
                                                //move existing file to duplicate folder and then try again

                                                bTemp = a_oFtp.FtpRename(Path.Combine(a_sTargetFtpFolder, a_ff.Filename), sDuplicateFolder + "/" + a_ff.Filename, ref sErr, true);
                                                sDebug += ";Target2Dup";
                                                if (!bTemp)
                                                {
                                                    if (sErr.Contains("already exists")) // file with the same name already exists in duplicate folder
                                                    {
                                                        //now one last try.... change the file name - add time(only) stamp
                                                        bTemp = a_oFtp.FtpRename(Path.Combine(a_sTargetFtpFolder, a_ff.Filename), sDuplicateFolder + "/" + a_ff.NameOnly + DateTime.Now.ToString("_HHmmss.") + a_ff.Extension, ref sErr, true);
                                                    }
                                                    if (!bTemp)
                                                    {
                                                        oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to diplicate folder. PartnerId " + a_sPartnerId, ErrorSeverityEnum.Failed));
                                                    }
                                                }

                                                if (bTemp) //== true
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
                                                    //now one last try.... change the file name - add time(only) stamp
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
                            // update database
                            oIndexFileInfo = new DocIndexFileInfo();
                            oIndexFileInfo.contractId = sConId;
                            oIndexFileInfo.subId = sSubId;
                            oIndexFileInfo.partnerId = a_sPartnerId;
                            oIndexFileInfo.docType = Convert.ToInt32(sDocType);
                            oIndexFileInfo.fileSize = Convert.ToInt32(lFileSize > 0 ? Convert.ToDouble(lFileSize) / 1024.0 : 0.0);
                            oIndexFileInfo.fileType = sFileExtension;
                            oIndexFileInfo.downloadType = 100; // FTP Download
                            oIndexFileInfo.sysAssignedFilename = a_sTargetFtpFolder + sFileName;
                            oIndexFileInfo.promptFilename = "QuarterlyStatement." + sFileExtension;
                            oIndexFileInfo.toPeriod = GetDate(sFromPeriod, "MMDDYYYY");
                            oIndexFileInfo.fromPeriod = AdjustFromDate(oIndexFileInfo.toPeriod); // (oIndexFileInfo.toPeriod.AddMonths(-3)).AddDays(1);
                            oIndexFileInfo.displayDesc = "From "
                                + oIndexFileInfo.fromPeriod.ToString("MM/dd/yyyy") + " To " + oIndexFileInfo.toPeriod.ToString("MM/dd/yyyy");
                            oIndexFileInfo.expireDt = DateTime.Now.AddYears(2);

                            oResRet = PptStatementAvailable(oPptStFeedInfo, oIndexFileInfo);
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
                            // File move/rename failed...log error
                            //oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + "  could not be moved to :   " + a_sTargetFtpFolder + a_ff.Filename + "(ex: " + sCouldNotMoveError + ")\r\n", ErrorSeverityEnum.Failed));
                        }


                    }
                    else
                    {
                        oReturn.Errors.Add(new ErrorInfo(-1, "PPT not found " + sSSN + " C: " + sConId + " S: " + sSubId, ErrorSeverityEnum.Failed));
                        //Fpt processing. Move to error folder
                        if (a_oFileInfo == null)
                        {
                            string sErrorFtpFolder = AppSettings.GetValue("FTPMoveError" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");
                            if (IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                            {
                                sErrorInfo = "";
                                bTemp = a_oFtp.FtpRename(a_ff.FullName, sErrorFtpFolder + "/" + a_ff.Filename, ref sErrorInfo, true);// supress exception
                                if (!bTemp)
                                {
                                    if (sErrorInfo.Contains("already exists"))
                                    {
                                        //now one last try.... change the file name - add time(only) stamp
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
                    string sErrorFtpFolder = AppSettings.GetValue("FTPMoveError" + a_sPartnerId) + DateTime.Now.ToString("yyyyMMdd");
                    oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " is not in expected file naming convention. File Moved to " + sErrorFtpFolder + " folder. \r\n", ErrorSeverityEnum.Failed));

                    if (IsDirectoryExist(a_oFtp, sErrorFtpFolder, true, ref sErrorInfo))
                    {
                        sErrorInfo = "";
                        bTemp = a_oFtp.FtpRename(a_ff.FullName, sErrorFtpFolder + "/" + a_ff.Filename, ref sErrorInfo, true);// supress exception
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
            if (a_ToPeriod.AddMonths(-3).AddDays(1).Day != 1)
            {
                return a_ToPeriod.AddMonths(-3).AddDays(2);
            }
            else
            {
                return a_ToPeriod.AddMonths(-3).AddDays(1);
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
        private FtpFolderTracking GetFtpFolderInfo(FTPUtility a_oFtp, string a_sRootFolder)
        {
            List<string> directories = new();
            string sError = "";
            FtpFolderTracking oFtpFolder = new();
            oFtpFolder.rootFolder = a_sRootFolder;
            oFtpFolder.yearFolder = DateTime.Now.Year.ToString();
            oFtpFolder.maxLimit = Convert.ToInt32(AppSettings.GetValue("MaxFilesPerFolder"));

            if (IsDirectoryExist(a_oFtp, oFtpFolder.rootFolder + oFtpFolder.yearFolder, true, ref sError))
            {
                directories = a_oFtp.ListDirectory(oFtpFolder.rootFolder + oFtpFolder.yearFolder);
                if (directories.Count > 0)
                {
                    oFtpFolder.subFolderCount = directories.Count - 1;
                }
                else
                {
                    oFtpFolder.subFolderCount = 0;
                }

                if (CreateFtpSubFolder(a_oFtp, oFtpFolder, ref sError))
                {
                    if (!oFtpFolder.IsUnderLimit)
                    {
                        oFtpFolder.subFolderCount++;
                        if (CreateFtpSubFolder(a_oFtp, oFtpFolder, ref sError))
                        {
                            oFtpFolder.returnStatus = ReturnStatusEnum.Succeeded;
                        }
                        else
                        {
                            oFtpFolder.returnStatus = ReturnStatusEnum.Failed;
                            oFtpFolder.errors.Add(new ErrorInfo(-1, "Folder: " + oFtpFolder.yearFolder + " Error: " + sError, ErrorSeverityEnum.Failed));
                        }

                    }
                    else
                    {
                        oFtpFolder.returnStatus = ReturnStatusEnum.Succeeded;
                    }
                }
                else
                {
                    oFtpFolder.returnStatus = ReturnStatusEnum.Failed;
                    oFtpFolder.errors.Add(new ErrorInfo(-1, "Folder: " + oFtpFolder.yearFolder + " Error: " + sError, ErrorSeverityEnum.Failed));

                }
            }
            else
            {
                oFtpFolder.returnStatus = ReturnStatusEnum.Failed;
                oFtpFolder.errors.Add(new ErrorInfo(-1, "Folder: " + oFtpFolder.yearFolder + " Error: " + sError, ErrorSeverityEnum.Failed));
            }
            return oFtpFolder;
        }
        private bool CreateFtpSubFolder(FTPUtility a_oFtp, FtpFolderTracking a_oFtpFolder, ref string sError)
        {
            bool bOk = false;
            //create folder
            a_oFtpFolder.currentFolder = a_oFtpFolder.subFolderCount.ToString("00000");
            if (IsDirectoryExist(a_oFtp, a_oFtpFolder.GetCurrentFolder, true, ref sError))
            {
                FTPdirectory oDir = a_oFtp.ListDirectoryDetail(a_oFtpFolder.GetCurrentFolder);
                a_oFtpFolder.initialCount = oDir.Count;
                a_oFtpFolder.runningCount = a_oFtpFolder.initialCount;
                bOk = true;
            }
            else
            {
                throw new Exception("Could not create directoty: " + a_oFtpFolder.currentFolder + "0/  Error: " + sError);
            }

            return bOk;
        }
        private bool IsDirectoryExist(FTPUtility oFtp, string a_sDirectory, bool a_bCreate, ref string a_sError)
        {
            bool bReturn = false;
            bool bIsGood;
            List<string> result = new();
            try
            {
                try
                {
                    result = oFtp.ListDirectory(a_sDirectory);
                    bReturn = true;
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    bReturn = false;
                }

                if (!bReturn && a_bCreate)
                {
                    bIsGood = oFtp.FtpCreateDirectory(a_sDirectory, ref a_sError);
                    bReturn = bIsGood;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                bReturn = false;
                a_sError = a_sError + " Exception:" + ex.Message;
            }
            return bReturn;
        }
        private ResultReturn PptStatementAvailable(PartStFeedInfo a_oPptStFeedInfo, DocIndexFileInfo a_oIndexFileInfo)
        {
            /// <summary>
            /// Update Index Engine when ppt statement is available for given ppt and contract
            /// </summary>
            /// <Author(s)>
            /// Hao Dinh - 08/31/2010 -Created
            /// </Author(s)> 
            /// 

            ResultReturn oReturn = new();
            try
            {
                //oPptStFeedInfo = GetPptFeedInfo(a_sSSN, a_sConId, a_sSubId);
                if (a_oPptStFeedInfo.found)
                {
                    a_oIndexFileInfo.connectParms = "<ConnectParm><ParmId ParmName=\"DiaFtpeStatement\">101</ParmId></ConnectParm>";
                    ResultReturn oRDiaFeed = _oeSDC.InsertDiaFeed(a_oPptStFeedInfo, a_oIndexFileInfo);
                    if (oRDiaFeed.returnStatus == ReturnStatusEnum.Succeeded)
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                    }
                    else
                    {
                        //may need to log error here
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(-1, General.ParseErrorText(oRDiaFeed.Errors, ";"), ErrorSeverityEnum.Failed));
                    }
                }
                //else
                //{
                //    //may need to log error here
                //    oReturn.returnStatus = ReturnStatusEnum.Failed;
                //    oReturn.Errors.Add(new ErrorInfo(-1, "PPT not found " + a_sSSN + " C: " + a_sConId + " S: " + a_sSubId, ErrorSeverityEnum.Failed));

                //}

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;

        }
        private PartStFeedInfo GetPptFeedInfo(string a_sSSN, string a_sConId, string a_sSubId, NotificationTypeEnum a_eNotificationType)
        {
            PartStFeedInfo oPptFeedInfo = new();



            DataSet dseInfo = _oeSDC.GeteStatementFeedInfo(a_sSSN, a_sConId, a_sSubId);
            if (dseInfo.Tables[0].Rows.Count > 0)
            {
                oPptFeedInfo.notificationType = a_eNotificationType.GetHashCode();
                AssignFeedInfo(dseInfo.Tables[0].Rows[0], oPptFeedInfo);
            }
            return oPptFeedInfo;
        }
        private void AssignFeedInfo(DataRow a_dr, PartStFeedInfo a_oPartFeedInfo)
        {
            a_oPartFeedInfo.found = true;
            a_oPartFeedInfo.inLoginId = Utils.CheckDBNullInt(a_dr["in_login_id"]);
            a_oPartFeedInfo.contractId = Utils.CheckDBNullStr(a_dr["contract_id"]);
            a_oPartFeedInfo.subId = Utils.CheckDBNullStr(a_dr["sub_id"]);
            a_oPartFeedInfo.email = Utils.CheckDBNullStr(a_dr["email"]);
            a_oPartFeedInfo.firstName = Utils.CheckDBNullStr(a_dr["first_name"]);
            a_oPartFeedInfo.lastName = Utils.CheckDBNullStr(a_dr["last_name"]);
            a_oPartFeedInfo.middleName = Utils.CheckDBNullStr(a_dr["middle_name"]);
            a_oPartFeedInfo.companyUrl = "http://www.ta-retirement.com/";
            a_oPartFeedInfo.companyPhone = Utils.CheckDBNullStr(a_dr["product_family"]) == "NAV" ? _sContact_NAV : _sContact_GAC;
        }
        private ResultReturn GetDIAFeed(int a_iNotificationType)
        {
            const char C_Delimiter = ',';
            const string C_MissingEmail = "MissingEmail@transamerica.com";
            StringBuilder strB = new();
            ResultReturn oReturn = new();
            oReturn.confirmationNo = AppSettings.GetValue("eStatementDIAFeedFolder")
                + "StatementFeed" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".txt";
            StreamWriter sw = null;
            string sEmail;
            try
            {
                sw = new StreamWriter(oReturn.confirmationNo);
                DataSet dsFeed = _oeSDC.GetDIAFeed(a_iNotificationType);
                foreach (DataRow dr in dsFeed.Tables[0].Rows)
                {
                    strB.Append(Utils.CheckDBNullStr(dr["notification_type"]));
                    strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["in_login_id"]));
                    strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["contract_id"]));
                    strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["sub_id"]));
                    sEmail = Utils.CheckDBNullStr(dr["email"]);
                    if (string.IsNullOrEmpty(sEmail))
                    {
                        sEmail = C_MissingEmail;
                    }
                    //strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["email"]));
                    strB.Append(C_Delimiter + sEmail);
                    strB.Append(C_Delimiter + "\"" + Utils.CheckDBNullStr(dr["first_name"]) + "\"");
                    //strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["mi"]));
                    //do not pass middle init value. Already embeded in the first name.
                    strB.Append(C_Delimiter + "");
                    strB.Append(C_Delimiter + "\"" + Utils.CheckDBNullStr(dr["last_name"]) + "\"");
                    strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["company_url"]));
                    strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["company_phone"]));
                    strB.Append(C_Delimiter + "\"" + Utils.CheckDBNullStr(dr["plan_name"]) + "\"");

                    sw.WriteLine(strB.ToString());
                    strB.Remove(0, strB.Length);
                }
                oReturn.rowsCount = dsFeed.Tables[0].Rows.Count;
                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));

            }
            finally
            {
                sw.Close();
            }
            return oReturn;
        }
    }
}
