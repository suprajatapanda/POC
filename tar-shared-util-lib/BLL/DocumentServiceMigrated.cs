using TRS.IT.SI.Services;
using TRS.IT.SI.Services.wsDocumentService;

namespace TRS.IT.SI.BusinessFacadeLayer
{
    public class DocumentServiceMigrated
    {
        private DocumentService _wsDoc;

        public DocumentServiceMigrated()
        {
            _wsDoc = new DocumentService(TrsAppSettings.AppSettings.GetValue("DocSrvWebServiceURL"));
        }
        public ImportFileResponse ImportFiles(ref ImportFileRequest request)
        {
            return _wsDoc.ImportFiles(request);
        }
    }
}