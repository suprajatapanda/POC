using System.ServiceModel;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.SI.Services;
using TRS.IT.TrsAppSettings;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;

namespace PlanComplianceResultBatch.SOA
{
    public class ContractServ
    {
        private ContractService _wsContract;

        public ContractServ()
        {
            _wsContract = new ContractService(AppSettings.GetValue("ContractWebServiceURL"));
        }

        public ResultReturn SubmitTestingResults(string xmlTestingData)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            BFLModel.SIResponse oResponse;
            try
            {
                oResponse = (BFLModel.SIResponse)TRS.IT.TRSManagers.XMLManager.DeserializeXml(_wsContract.SubmitTestingResults(xmlTestingData), typeof(BFLModel.SIResponse));
                if (oResponse.Errors[0].Number != 0)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(oResponse.Errors[0].Number,
                        oResponse.Errors[0].Description + " xml:" + xmlTestingData, ErrorSeverityEnum.Error));
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message + " xml:" + xmlTestingData, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }    

    }
}