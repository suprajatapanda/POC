using System.Text.RegularExpressions;
namespace TRS.IT.TRSManagers
{
    public class RegularExpression
    {
        public static string RegExpReplace(string sValue)
        {
            string str = string.Empty;
            try
            {
                str = new Regex("[^A-Za-z 0-9 \\.,\\?'\"!@#\\$%\\^&\\*\\(\\)-_=\\+;:<>\\/\\\\\\|\\}\\{\\[\\]`~]*").Replace(sValue, string.Empty);
            }
            catch
            {
            }
            return str;
        }
    }
}
