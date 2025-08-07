using TRS.IT.TrsAppSettings;

namespace TRS.IT.BendProcessor.DAL
{
    public class ISCDataDC
    {
        private string _sConnectString;

        public ISCDataDC()
        {
            _sConnectString = AppSettings.GetConnectionString("ConnectString");
        }

        public string getLoanDefaultQtrlyReport(DateTime start_date, DateTime end_date)
        {
            string sReturnXML = "";  // Format: "<SIResponse><IsPending>0</IsPending><ConfirmationNumber>20150713050208</ConfirmationNumber><Errors><Error><Number>0</Number><Description/><Type>0</Type></Error></Errors><AdditionalData><Cases><Case cid="932115" sid="00066" type="LNDEFRPT" /><Case cid="932115" sid="00066" type="LNMATRPT" /><Case cid="932232" sid="00043" type="LNDEFRPT" /><Case cid="932256" sid="00013" type="LNDEFRPT" /><Case cid="932256" sid="00013" type="LNMATRPT" /></Cases></AdditionalData><SessionID/><TransIDs>0</TransIDs></SIResponse>"

            DriverSOA.WithdrawalsService DriverSOAwthd = new();
            sReturnXML = DriverSOAwthd.getLoanDefaultQtrlyReport(start_date, end_date);
            return sReturnXML;
        }

        public string getLoanPayOffData(DateTime start_date, DateTime end_date)
        {
            string sReturnXML = "";  // Format: "<SIResponse><IsPending>0</IsPending><ConfirmationNumber>20150713050208</ConfirmationNumber><Errors><Error><Number>0</Number><Description/><Type>0</Type></Error></Errors><AdditionalData><Cases><Case cid="932115" sid="00066" type="LNDEFRPT" /><Case cid="932115" sid="00066" type="LNMATRPT" /><Case cid="932232" sid="00043" type="LNDEFRPT" /><Case cid="932256" sid="00013" type="LNDEFRPT" /><Case cid="932256" sid="00013" type="LNMATRPT" /></Cases></AdditionalData><SessionID/><TransIDs>0</TransIDs></SIResponse>"

            DriverSOA.WithdrawalsService DriverSOAwthd = new();
            sReturnXML = DriverSOAwthd.getLoanPaidOff(start_date, end_date);
            return sReturnXML;
        }


    }
}
