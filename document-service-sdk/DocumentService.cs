using System.ServiceModel;
using TRS.IT.SI.Services.wsDocumentService;

namespace TRS.IT.SI.Services
{
    public class DocumentService : IDisposable
    {
        private DocumentServiceSoapClient _wsDoc;
        private bool _disposed = false;

        public DocumentService(string soapEndpoint)
        {
            var endpointAddress = new EndpointAddress(soapEndpoint);
            var basicHttpBinding = new BasicHttpBinding(
                endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
                    ? BasicHttpSecurityMode.Transport
                    : BasicHttpSecurityMode.None);
            if (endpointAddress.Uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
            {
                basicHttpBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Windows;
            }
            basicHttpBinding.MaxReceivedMessageSize = 10 * 1024 * 1024;
            basicHttpBinding.MaxBufferSize = 10 * 1024 * 1024;
            basicHttpBinding.MaxBufferPoolSize = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxDepth = 128;
            basicHttpBinding.ReaderQuotas.MaxStringContentLength = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxArrayLength = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxBytesPerRead = 10 * 1024 * 1024;
            basicHttpBinding.ReaderQuotas.MaxNameTableCharCount = 10 * 1024 * 1024;
            _wsDoc = new DocumentServiceSoapClient(basicHttpBinding, endpointAddress);
            _wsDoc.ClientCredentials.Windows.ClientCredential = System.Net.CredentialCache.DefaultNetworkCredentials;
        }
        public ImportFileResponse ImportFiles(ImportFileRequest request)
        {
            return _wsDoc.ImportFile(request);
        }
        public string ImageDocument(string a_sContractId, string a_sSubId, int iDocTypeCode, byte[] tempFile, string sFullFileName,int a_iCaseNo, string sSourceName, string SSN)
        {
            string sError = "";
            string sConfNumber = "";
            ImportFileRequest oRequest = new();
            ImportFile[] importFiles = null;
            ImportFile importFile = null;
            importFiles = new ImportFile[1];
            importFile = new ImportFile();
            importFile.ContractID = a_sContractId;
            importFile.SubID = a_sSubId;
            importFile.DocTypeCode = iDocTypeCode;
            if (a_iCaseNo != 0)
            {
                importFile.TrackNumber = a_iCaseNo.ToString();
            }
            importFile.FileContent = tempFile;
            importFile.FileName = sFullFileName;
            if (SSN != "")
            {
                importFile.SSN = SSN;
            }
            importFiles[0] = importFile;
            oRequest.ImportFiles = importFiles;
            oRequest.ProcessType = E_ProcessType.None;
            oRequest.SourceName = sSourceName;
            ImportFileResponse oResponse = new();
            oResponse = ImportFiles(oRequest);
            sConfNumber = oResponse.ConfirmationNumber;
            foreach (ErrorInfo err in oResponse.Errors)
            {
                if (err.Number != 0) { sError = sError + err.Description + "\n"; }
            }
            return sError;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    if (_wsDoc?.State == CommunicationState.Opened)
                    {
                        _wsDoc.Close();
                    }
                }
                catch
                {
                    _wsDoc?.Abort();
                }
                _disposed = true;
            }
        }
    }
}
