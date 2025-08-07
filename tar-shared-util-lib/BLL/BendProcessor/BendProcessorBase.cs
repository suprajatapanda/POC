using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;

namespace TRS.IT.BendProcessor.Model
{
    abstract public class BendProcessorBase
    {
        protected string _sJobId;
        protected string _sJobName;
        protected string _sPartnerID;
        protected string _sFatalEmailNotification = AppSettings.GetValue("FatalErrorEmailNotification");
        protected string _sBCCEmailNotification = AppSettings.GetValue("BCCEmailNotification");

        public BendProcessorBase(string a_sJobId, string a_sJobName, string a_sPartnerId)
        {
            _sJobId = a_sJobId;
            _sJobName = a_sJobName;
            _sPartnerID = a_sPartnerId;
        }
        public void SendErrorEmail(Exception ex)
        {
            Utils.SendMail(AppSettings.GetValue(ConstN.C_BPROCESSOR_EMAIL), _sFatalEmailNotification, "Error Exception", ex.Message + "\n\n\n Stack Trace:\n" + ex.StackTrace);

        }
        public void SendErrorMsgEmail(String sSubject, String sErrMsg)
        {
            Utils.SendMail(AppSettings.GetValue(ConstN.C_BPROCESSOR_EMAIL), _sFatalEmailNotification, sSubject, sErrMsg);
        }
        public void SendTaskCompleteEmail(string a_sSubject, string a_sBody, string a_sJobName)
        {
            string sFr = AppSettings.GetValue("BendFromEmail");
            string sTo = AppSettings.GetValue(_sJobName + "_EmailNotification");
            if (sTo == string.Empty || sTo == null)
            {
                sTo = AppSettings.GetValue("ProcessingCompleteNotification");
            }
            Utils.SendMail(sFr, sTo, a_sSubject, a_sBody);
        }
        public void InitTaskStatus(TaskStatus a_oTaskStatus, string a_sTaskName)
        {
            a_oTaskStatus.taskName = a_sTaskName;
            a_oTaskStatus.startTime = DateTime.Now;
            a_oTaskStatus.retStatus = TaskRetStatus.Succeeded;
        }
        public void InitTaskError(TaskStatus a_oTaskStatus, Exception a_ex, bool a_bSendEmail)
        {
            a_oTaskStatus.retStatus = TaskRetStatus.Failed;
            a_oTaskStatus.fatalErrCnt += 1;
            a_oTaskStatus.endTime = DateTime.Now;
            a_oTaskStatus.errors.Add(new ErrorInfo(-1, a_ex.Message, ErrorSeverityEnum.ExceptionRaised));
            if (a_bSendEmail)
            {
                SendErrorEmail(a_ex);
            }
        }
        protected FtpFolderTracking GetFtpFolderInfo(FTPUtility a_oFtp, string a_sRootFolder)
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
        protected bool CreateFtpSubFolder(FTPUtility a_oFtp, FtpFolderTracking a_oFtpFolder, ref string sError)
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
        protected bool IsDirectoryExist(FTPUtility oFtp, string a_sDirectory, bool a_bCreate, ref string a_sError)
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
            }
            return bReturn;
        }
        protected string RemoveLeadingZeros(string a_sConId)
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
        protected DateTime GetDate(string sDate)//MMDDYYYY format
        {
            DateTime dt = new();
            int iLength = 0;
            int year = 0;
            int month = 0;
            int day = 0;
            iLength = sDate.Length;
            if (iLength == 8)
            {
                month = Convert.ToInt32(sDate.Substring(0, 2));
                day = Convert.ToInt32(sDate.Substring(2, 2));
                year = Convert.ToInt32(sDate.Substring(4, 4));
                dt = new DateTime(year, month, day);
            }
            return dt;
        }
    }
}
