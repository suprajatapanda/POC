namespace TRS.IT.TRSManagers
{
    public class FormatManager
    {
        public static string ParsePhoneNumber(string sValue)
        {
            if (sValue == null)
            {
                return "";
            }
            int num = 0;
            string text = "";
            char[] array = sValue.ToCharArray();
            for (num = 0; num <= array.Length - 1; num++)
            {
                if (ValidationManager.IsNumeric(array[num].ToString()))
                {
                    text += array[num];
                }
            }
            return text;
        }

        public static string FormatSSN(string sValue)
        {
            if (sValue == null)
            {
                return "";
            }
            if (sValue.Length == 9)
            {
                return sValue.Substring(0, 3) + "-" + sValue.Substring(3, 2) + "-" + sValue.Substring(5, 4);
            }
            return sValue;
        }
    }
}
