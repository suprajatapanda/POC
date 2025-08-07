namespace TRS.IT.BendProcessor.Model
{
    public class DocIndexFileInfo
    {
        public int docId;
        public int docType;
        public int downloadType;
        public string displayDesc;
        public string promptFilename;
        public string sysAssignedFilename;
        public string transId;
        public int fileSize;
        public string fileType;
        public string audienceType;
        public DateTime expireDt;
        public string linkKey;
        public string contractId;
        public string subId;
        public string partnerId;
        public string connectParms;
        public DateTime fromPeriod;
        public DateTime toPeriod;
    }
    public class DocFileInfo
    {
        public string contractId;
        public string subId;
        public string ssn;
        public int docType;
        public string transId;
        public string trxDate;
        public string parseError = string.Empty;
    }

    public class PartStFeedInfo
    {
        public bool found = false;
        public int inLoginId;
        public string contractId;
        public string subId;
        public string email;
        public string firstName;
        public string middleName;
        public string lastName;
        public string companyUrl;
        public string companyPhone;
        public int notificationType;
    }
    public class FtpFolderTracking
    {
        public string rootFolder;
        public string yearFolder;
        public int subFolderCount;
        public string currentFolder;
        public int initialCount;
        public int runningCount;
        public int maxLimit;
        public ReturnStatusEnum returnStatus;
        public List<ErrorInfo> errors = new();

        public bool IsUnderLimit
        {
            get
            {
                if (runningCount >= maxLimit)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public string GetCurrentFolder
        {
            get { return rootFolder + yearFolder + "/" + currentFolder + "/"; }
        }

    }
}
