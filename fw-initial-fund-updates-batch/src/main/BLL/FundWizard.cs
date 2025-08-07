using SIUtil;
using MO = TRS.IT.SI.BusinessFacadeLayer.Model;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
namespace FWInitialFundUpdatesBatch.BLL
{
    internal class FundWizard
    {
        TARSharedUtilLibBFLBLL.FundWizard oFW;
        private DAL.ContractDC _ContractDC;
        private bool _bNewCaseOverride = false;
        public FundWizard(TARSharedUtilLibBFLBLL.FundWizard obj)
        {
            oFW = obj;
            _ContractDC = new DAL.ContractDC();
        }
        public MO.SIResponse UpdateFundLineup()
        {
            MO.FundWizardInfo.FmrsUpdateReturn oResult = null;
            var oResponse = new MO.SIResponse();
            int iNewCase = _bNewCaseOverride.GetHashCode();
            try
            {
                oResult = oFW.UpdateFMRS(1);
                if (oResult.xResult.Element("Wizard").Attribute("Error").Value == "0")
                {
                    if (oResult.xResult.Element("Validation").Attribute("Error").Value == "0" & oResult.xResult.Element("Pegasys").Attribute("Error").Value == "0")
                    {
                        oFW.InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.UpdatePegasys, 100, [oResult.xInputXml, oResult.xOutputXml, oResult.xResult]);
                        _ContractDC.FwUpdatePending(oFW.CaseNo, oFW.ContractId, oFW.SubId, iNewCase);
                        oFW._iCaseStatus = (int)MO.FundWizardInfo.fwCaseStatusEnum.Pending;
                        oResponse.Errors[0].Number = 0;
                    }

                    else
                    {
                        string sErrorText = string.Empty;
                        if (oResult.xResult.Element("Validation").Attribute("Error").Value == "-1" & oResult.xResult.Element("Pegasys").Attribute("Error").Value == "-1")
                        {
                            sErrorText = "FMRS update failed, Validation and Pegasys update error.</br></br>" + oFW.GetPegasysMessage(oResult);
                        }
                        else if (oResult.xResult.Element("Validation").Attribute("Error").Value == "-1")
                        {
                            sErrorText = "FMRS validation failed.";
                        }
                        else if (oResult.xResult.Element("Pegasys").Attribute("Error").Value == "-1")
                        {
                            sErrorText = "FMRS Pegasys update failed. </br></br>" + oFW.GetPegasysMessage(oResult);
                        }
                        oResponse.Errors[0].Number = -1;
                        oResponse.Errors[0].Description = sErrorText;
                        oFW.SendErrorEmail(oResult);
                    }
                }
                else
                {
                    oResponse.Errors[0].Number = -1;
                    oResponse.Errors[0].Description = oResult.xResult.Element("Wizard").Attribute("ErrorMsg").Value;
                    oFW.SendErrorEmail(oResult);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oResponse.Errors[0].Number = -1;
                oResponse.Errors[0].Description = ex.Message;
                if (oResult == null)
                {
                    oResult = new MO.FundWizardInfo.FmrsUpdateReturn();
                }
                oFW.SendErrorEmail(oResult);
            }

            return oResponse;
        }
    }
}