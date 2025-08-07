namespace TRS.IT.SI.BusinessFacadeLayer.SOA
{
    public class FundInfoSoa
    {
        private Services.FmrsService _wsFmrs;

        public FundInfoSoa()
        {
            _wsFmrs = new Services.FmrsService(TrsAppSettings.AppSettings.GetValue("FMRSURL"));
        }
        public string GetFmrxXml(string xml)
        {
            return _wsFmrs.GetFMRSFundsXml(xml);
        }

        public string UpdateFmrsFundLineup(string xml)
        {
            return _wsFmrs.UpdateFundLineup(xml);
        }
    }
}