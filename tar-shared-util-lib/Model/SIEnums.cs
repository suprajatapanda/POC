namespace SIModel
{
    public class SIEnums
    {
        #region "** CONSTANTS **"
        public const string C_SessionInfo = "SessionInfo";
        public const string C_SessionActAsInfo = "SessionActAsInfo";
        public const string C_PartnerID_TAE = "TAE";
        public const string C_PartnerID_DIA = "DIA";
        public const string C_PartnerID_PENCO = "PENCO";
        public const string C_PartnerID_SEBS = "SEBS";
        public const string C_PartnerID_TRS = "TRS";
        public const string C_PartnerID_FUNDS = "FUNDS";
        public const string C_PartnerID_CPC = "CPC";
        public const string C_PartnerID_ISC = "ISC";
        public const string C_PartnerID_Mercer = "MCR";
        public const string C_Audience_PARTICIPANT = "PARTICIPANT";
        public const string C_Audience_SPONSOR = "SPONSOR";
        public const string C_Audience_TPA = "TPA";
        public const string C_Audience_PRODUCER = "PRODUCER";
        public const string C_Audience_SSO = "SSO";
        public const string C_HomePage = "/Default.aspx";
        public const string C_EmailMEPClient = "860";
        public const string C_ClientFirstLogin = "870";
        public const string C_ClientAcceptAAA = "880";
        public const string C_ClientRequestChangesAAA = "885";
        public const string C_MEPApprovedAAA = "890";
        public const string C_MEPRequestChangesAAA = "892";
        public const string C_PlanComplianceApprovedAAA = "891";
        public const string C_PlanComplianceRevisedRequestChangesAAA = "894";
        public const string C_PlanComplianceRequestChangesAAA = "895";
        public const string C_ContactUs = "/Portal/po_popUp.aspx?id=/content/taweb/Portal/ContactTransamerica&UserType=C";
        public const string C_ForgotPassword = "/SIP/Shared/co_ForgotPassword.aspx";
        public const string C_FirstTimeLogin = "/SIP/Shared/co_FirstTimeLogin.aspx";
        public const string C_LoginError = "/SIP/Shared/co_LoginError.aspx";
        public const string C_MultiplePlans = "/SIP/Shared/co_MultiplePlans.aspx";
        public const string C_Logout = "/Portal/Logout.aspx";
        public const string C_ErrorPage = "/TRSMaint.aspx";
        public const string C_TANY = "TANY";
        public const string C_TAPP = "TAPP";
        public const string C_TRAM = "TRAM";
        public const string C_SessionContractInfo = "SessionContractInfo";
        public const string C_LastLoginDate = "LastLoginDate";
        public const string C_LastLoginDateInitialized = "LastLoginDateInitialized";
        public const string C_SessionInstallationContractInfo = "InstallationContractInfo";
        public const string C_LDAPUserName = "LDAPUserName";
        public const int C_TRS_ID = 4;
        public const string C_NON_US_STATE = "ZZ";
        public const string C_Confirm_Section_Start = "<!--CONF_SECTION_START-->";
        public const string C_Confirm_Section_End = "<!--CONF_SECTION_END-->";
        public const string C_Confirm_Detail_Start = "<!--CONF_DETAIL_START-->";

        public const string C_Confirm_Detail_End = "<!--CONF_DETAIL_END-->";
        public const int C_ADMINID_DIA = 1200;
        public const int C_ADMINID_TAE = 200;
        public const int C_ADMINID_PENCO = 800;
        public const int C_ADMINID_CERIDIAN = 4040;
        public const int C_ADMINID_CPC = 4020;
        public const int C_ADMINID_RHI = 4000;
        public const int C_ADMINID_SEBS = 400;
        public const int C_ADMINID_ISC = 1300;
        //Fund ID
        public const int C_LoanFund = 10997;
        public const int C_TrustFund = 10076;

        public const int C_LowRiskFund = 10033;
        //Missing spec for error handling

        public const string C_MissingSpecs = "(Missing error text or error text not in specs) ";
        //Signix communication session name
        public const string C_eSignInterCom = "eSignInterCom";
        public const string C_HttpHeaderClientIP = "X-Forwarded-For";
        #endregion
    }
}
