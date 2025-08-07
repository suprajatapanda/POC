using TRS.IT.SI.Services.wsDocumentService;

namespace FWBamlDocsToWMSBatch.SOA
{
    public class DocumentService
    {
        public DocumentService()
        {
        }
        public ImportFileResponse ImportFiles(ref ImportFileRequest request)
        {
            return new TRS.IT.SI.Services.DocumentService(TRS.IT.TrsAppSettings.AppSettings.GetValue("DocSrvWebServiceURL")).ImportFiles(request);
        }
    }
}
