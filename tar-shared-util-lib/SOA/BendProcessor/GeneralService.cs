namespace TRS.IT.BendProcessor.DriverSOA
{
    public class GeneralService
    {
        private SI.Services.GeneralService _wsGeneral;

        public GeneralService()
        {
            _wsGeneral = new SI.Services.GeneralService(TrsAppSettings.AppSettings.GetValue("GeneralWebServiceURL"));
        }
        public string GetLastBusinessDay()
        {
            return _wsGeneral.GetLastBusinessDay();
        }
        public string GetFormattedLastBusinessDay()
        {
            return string.Format("{0:yyyy-MM-dd}", DateTime.Parse(GetLastBusinessDay()));
        }
    }
}
