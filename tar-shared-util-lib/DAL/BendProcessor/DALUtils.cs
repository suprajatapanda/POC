namespace TRS.IT.BendScheduler.DAL
{
    public class DALUtils
    {
        //mark private
        private DALUtils() { }

        public static string IsDBNullStr(object a_oVal)
        {
            if (a_oVal == DBNull.Value)
            {
                return string.Empty;
            }
            else
            {
                return a_oVal.ToString();
            }
        }
        public static int IsDBNullInt(object a_oVal)
        {
            if (a_oVal == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(a_oVal);
            }
        }
        public static DateTime IsDBNullDt(object a_oVal)
        {
            if (a_oVal == DBNull.Value)
            {
                return Convert.ToDateTime("01/01/1900");
            }
            else
            {
                return Convert.ToDateTime(a_oVal);
            }
        }
    }
}
