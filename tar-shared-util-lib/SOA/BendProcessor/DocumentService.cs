namespace TRS.IT.BendProcessor.DriverSOA
{
    public class DocumentService
    {
        private SI.Services.DocumentService _wsDoc;

        public DocumentService()
        {
            _wsDoc = new SI.Services.DocumentService(TrsAppSettings.AppSettings.GetValue("DocSrvWebServiceURL"));
        }
        public string ImageDocument(string a_sContractId, string a_sSubId, int iDocTypeCode,string sFullFileName, int a_iCaseNo, string sSourceName = "FundWizard BackEnd Process", string SSN = "")
        {           
            byte[] tempFile = File.ReadAllBytes(sFullFileName);
            return _wsDoc.ImageDocument(a_sContractId, a_sSubId, iDocTypeCode, tempFile, Path.GetFileName(sFullFileName), a_iCaseNo, sSourceName, SSN);
        }
    }
}
