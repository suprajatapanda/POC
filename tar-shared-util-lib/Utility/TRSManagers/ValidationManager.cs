using System.Text.RegularExpressions;


namespace TRS.IT.TRSManagers
{
    public class ValidationManager
    {
        public static bool IsNumeric(string sValue)
        {
            try
            {
                int.Parse(sValue);
            }
            catch
            {
                return false;
            }
            return true;
        }
        public static bool IsValidEmailAddress(string sEmail)
        {
            string pattern = "^([a-zA-Z0-9_?=&amp;\\-\\.\\'\\*\\{\\}\\^\\/\\+])*@\\w+([-\\.]\\w+)*\\.([a-zA-Z]{2,6})$";
            return sEmail != null && Regex.Match(sEmail.Trim(), pattern, RegexOptions.IgnoreCase).Success;
        }
    }
}
