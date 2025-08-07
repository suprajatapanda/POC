namespace TRS.IT.BendProcessor.Model
{
    public class ConstN
    {
        public const string C_TAB = "\t";
        public const string C_CRLF = "\r\n";
        public const string C_TAG_TABLE_O = "<table cellSpacing='0' cellPadding='0 border='0'>";
        public const string C_TAG_TABLE_C = "</table>";
        public const string C_TAG_TR_O = "<tr>";
        public const string C_TAG_TR_C = "</tr>";
        public const string C_TAG_TD_O = "<td>";
        public const string C_TAG_TD_C = "</td>";
        public const string C_TAG_BOLD_O = "<b>";
        public const string C_TAG_BOLD_C = "</b>";
        public const string C_TAG_BR = "<br/>";
        public const string C_TAG_P_O = "<p>";
        public const string C_TAG_P_C = "</p>";
        public const string C_TAG_NBSP3 = "-   ";
        public const string C_PARTNER_TAE = "TAE";
        public const string C_PARTNER_ISC = "ISC";
        public const string C_PARTNER_PENCO = "PENCO";
        public const string C_PARTNER_DIA = "DIA";
        public const string C_PARTNER_SEBS = "SEBS";
        public const string C_PARTNER_CPC = "CPC";
        public const string C_PARTNER_TRS = "TRS";
        public const string C_BPROCESSOR_EMAIL = "BendFromEmail";
        public const string C_BPROCESSOR_OUTSIDE_FROM_EMAIL = "BendOutsideFromEmail";
        public const string C_CONNECT_STRING = "ConnectString";

    }

    public enum TaskRetStatus
    {
        NotRun = 0,
        Succeeded = 1, // Completed with no error
        ToCompletionWithErr = 2, //Completed with Errors
        Failed = 3,
        FailedAborted = 4, // Did not run to completion
        Warning = 5 // no critical errors warning
    }
    public enum ErrorSeverityEnum
    {
        Warning = 0,
        Error = 1,
        Failed = 2,
        ExceptionRaised = 3
    }
    public enum ReturnStatusEnum
    {
        Failed = -1,
        Unknown = 0,
        Succeeded = 1,
        NotRun = 2,
    }
    public enum NotificationTypeEnum
    {
        eConfirm = 1,
        eStatement = 2,
        PxNotification = 3,
        RequiredNotifications = 4 // These are plan level documents for Participants (same document for all Participants in the plan)
    }
}

