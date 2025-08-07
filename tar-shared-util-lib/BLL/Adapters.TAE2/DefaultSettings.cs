namespace TRS.IT.SI.BusinessFacadeLayer.Adapters
{
    public class DefaultSettings
    {

        private const string C_TANY = "TANY";
        private const string C_TAPP = "TAPP";
        private const int C_LOCATION_LEN = 5;
        public static string FPID(string PlanID)
        {
            if (Convert.ToInt32(PlanID) > 8448)
            {
                return C_TANY;
            }
            else if (Convert.ToInt32(PlanID) > 8191)
            {
                return C_TAPP;
            }
            else
            {
                return TrsAppSettings.AppSettings.GetValue("TAE_DEFAULT_FPID");
            }
        }

        public static string CID()
        {
            return TrsAppSettings.AppSettings.GetValue("TAE_DEFAULT_CID");
        }

        public static string USERID()
        {
            return TrsAppSettings.AppSettings.GetValue("TAE_DEFAULT_USERID");
        }

        public static string BANKCODE()
        {
            return TrsAppSettings.AppSettings.GetValue("TAE_DEFAULT_BANKCODE");
        }

        public static string BANKCODE(string subID)
        {
            if (subID == "000" | string.IsNullOrEmpty(subID))
            {
                return TrsAppSettings.AppSettings.GetValue("TAE_DEFAULT_BANKCODE");
            }
            else
            {
                return "L" + DAL.General.SubOut(subID);
            }
        }
        public static string BANKCODE(string subID, string LocationCode)
        {
            if (!string.IsNullOrEmpty(LocationCode))
            {
                if (LocationCode.Length <= C_LOCATION_LEN)
                {
                    return "L" + LocationCode.PadRight(C_LOCATION_LEN, ' ');
                }
                else
                {
                    throw new Exception("Invalid location code:" + LocationCode);
                }
            }
            else if (subID == "000" | string.IsNullOrEmpty(subID))
            {
                return TrsAppSettings.AppSettings.GetValue("TAE_DEFAULT_BANKCODE");
            }
            else
            {
                return "L" + DAL.General.SubOut(subID);
            }
        }
    }
}