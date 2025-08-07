namespace FWUpdateRKPartner.Utils
{
    public class Utils
    {
        public static string GetFID(string a_sPlanID)
        {
            if (a_sPlanID.Length > 4)
            {
                return "    ";
            }
            else
            {
                if (Convert.ToInt32(a_sPlanID) > 8448)
                    return "TANY";
                else if (Convert.ToInt32(a_sPlanID) > 8191)
                    return "TAPP";
                else
                    return "TRAM";
            }
        }
    }
}
