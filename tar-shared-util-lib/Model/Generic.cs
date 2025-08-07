namespace TRS.IT.SI.BusinessFacadeLayer.Model
{
    public class GeneralConstants
    {
        public const string C_NON_US_STATE = "ZZ";
        public const string C_PARTNER_ID_TAE = "TAE";
        public const string C_PARTNER_ID_PENCO = "PENCO";
        public const string C_PARTNER_ID_TRS = "TRS";
        public const string C_PARTNER_ID_SEBS = "SEBS";
        public const string C_PARTNER_ID_CPC = "CPC";
        public const string C_InstallationTracker_CalledFrom = "TRSWebSite";
    }

    public enum PartnerFlag : int
    {
        TAE = 1,
        DIA = 2,
        Penco = 3,
        TRS = 4,
        ISC = 5,
        MCR = 6
    }
    public enum ErrorCodes
    {
        Unknown = -100,
        InvalidPassword = 1,
        StatementError = 2,
        PartnerUnavailable = 3,
        MappingError = 4,
        TransactionNotFound = 500,
        InvalidLogin = 10,
        DuplicateParticpant = 101,
        InvalidTransaction = 102,
        TimeoutError = 999,
        IncompleteData = 103,
        IncompleteResponse = 104,
        NoParticipantCache = 5,
        AccountLocked = 99,
        NoPendingTransaction = 105,
        NoCensusDataAvailable = 106,
        NoOnlineAccessAvailable = 100,
        EDocsError = 110,
        MQException = 120,
        GenericException = 130
    }
    public class ErrorMessages
    {
        public const string Unknown = "Unknown Error.";
        public const string InvalidPassword = "Invalid Password.";
        public const string StatementError = "Invalid Statement Request.";
        public const string PartnerUnavailable = "Partner is unavailable";
        public const string DuplicateParticpant = "Duplicate Participant for this plan";
        public const string InvalidTransaction = "Invalid Transaction";
        public const string TimeoutError = "Timeout Error";
        public const string IncompleteData = "Incomplete Data";
        public const string IncompleteResponse = "Incomplete or missing response";
        public const string GenericException = "Generic exception";

        public const string MQException = "MQ exception";
    }
}

