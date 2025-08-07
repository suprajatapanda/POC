namespace TRS.IT.BendProcessor.Model
{
    [Serializable()]
    public class ErrorInfo
    {
        public ErrorInfo() { }
        public ErrorInfo(int a_iErroNum, string a_sErrorDesc, ErrorSeverityEnum eSeverity)
        {
            errorNum = a_iErroNum;
            errorDesc = a_sErrorDesc;
            severity = eSeverity;
        }
        public int errorNum;
        public string errorDesc;
        public ErrorSeverityEnum severity;
    }
    [Serializable()]
    public class ResultReturn
    {
        public List<ErrorInfo> Errors = new();
        public string request;
        public string response;
        public string confirmationNo;
        public ReturnStatusEnum returnStatus;
        public bool isException;
        public int rowsCount;

    }
    [Serializable()]
    public class TaskStatus
    {
        public TaskRetStatus retStatus;
        public string partnerId;
        public string taskName;
        public DateTime startTime;
        public DateTime endTime;
        public int fatalErrCnt;
        public int warningCnt;
        public int rowsCount;
        public List<ErrorInfo> errors = new();

    }
    public class MessageServiceKeyValue
    {
        public string key;
        public string value;
    }
    public class MessageTemplateContact
    {
        public string name;
        public string email;
        public string phone;

    }
}
