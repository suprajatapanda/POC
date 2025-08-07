using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.SI.Services;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;

namespace TRS.IT.BendProcessor.DriverSOA
{
    public class ContractServ
    {
        private ContractService _wsContract;

        public ContractServ()
        {
            _wsContract = new ContractService(TrsAppSettings.AppSettings.GetValue("ContractWebServiceURL"));
        }
        public ResultReturn SubmitTestingResults(string xmlTestingData)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            BFLModel.SIResponse oResponse;
            try
            {
                oResponse = (BFLModel.SIResponse)TRSManagers.XMLManager.DeserializeXml(_wsContract.SubmitTestingResults(xmlTestingData), typeof(BFLModel.SIResponse));
                if (oResponse.Errors[0].Number != 0)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(oResponse.Errors[0].Number,
                        oResponse.Errors[0].Description + " xml:" + xmlTestingData, ErrorSeverityEnum.Error));
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message + " xml:" + xmlTestingData, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        public SOA.Model.ContractInfo GetContractInformation(string contractId, string subId)
        {
            var cinfo = _wsContract.GetContractInformation(contractId, subId);
            return (SOA.Model.ContractInfo)TRSManagers.ConvertManager.CType(cinfo, typeof(SOA.Model.ContractInfo));
        }

        public ResultReturn NotifyToConsolidateMessages(string xmlWsDocumentServiceDocumentEx, string dataToConsolidate, string xmlMessageVariablesEx)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            SOA.Model.TWS_Response oResponse;
            try
            {
                oResponse = (SOA.Model.TWS_Response)TRSManagers.XMLManager.DeserializeXml(_wsContract.NotifyToConsolidateMessages(xmlWsDocumentServiceDocumentEx, dataToConsolidate, xmlMessageVariablesEx), typeof(SOA.Model.TWS_Response));
                if (oResponse.Errors[0].Number != 0)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(oResponse.Errors[0].Number,
                        oResponse.Errors[0].Description, ErrorSeverityEnum.Error));
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }
    }
}