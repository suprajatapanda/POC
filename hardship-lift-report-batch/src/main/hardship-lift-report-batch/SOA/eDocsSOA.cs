using System.Data;
using TRS.IT.SI.Services;

namespace HardshipLiftReport.SOA
{
    class eDocsSOA
    {
        private TRSPlanProvService _wsTrsPlanProv;
        public eDocsSOA()
        {
            _wsTrsPlanProv = new TRSPlanProvService(TRS.IT.TrsAppSettings.AppSettings.GetValue("EDocsWebServiceURL"));
        }        
        public DataSet ListForHardshipLift(string strStartDate, string strEndDate, string CallAppUser)
        {
            return _wsTrsPlanProv.ListForHardshipLift(strStartDate, strEndDate);
        }
    }
}
