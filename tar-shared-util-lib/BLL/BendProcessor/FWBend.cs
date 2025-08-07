using System.Collections;
using System.Data;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.SI.BusinessFacadeLayer;
using TRS.IT.TrsAppSettings;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace TRS.IT.BendProcessor.BLL
{
    public class FWBend : BendProcessorBase
    {
        public FWBend() : base("174", "FundWizard", "TRS") { }

        FWBendDC _oFWDC = new();
        DataSet _dsPending;

        public DataSet PendingFundChanges
        {
            get
            {
                if (_dsPending == null)
                {
                    _dsPending = _oFWDC.GetPendingList(DateTime.Today);
                }

                return _dsPending;
            }
        }
        public void SendErrorEmailToUsers(string a_sConId, string a_sSubId, int a_iCaseNo, string sPartnerId, string a_sError)
        {
            string sFromEmail = AppSettings.GetValue("FWBendEmailAddr");
            string sToEmails = AppSettings.GetValue("UpdateErrorNotifyEmails");

            Utils.SendMail(sFromEmail, sToEmails, "Immediate attention required - Contract: " + a_sConId + " CaseNo: " + a_iCaseNo.ToString() + " Partner: " + sPartnerId, a_sError, _sBCCEmailNotification);
        }
        private void SetFWDocGenPaths(FWDocGen a_oFWDocGen)
        {
            a_oFWDocGen.LicenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Aspose.Total.lic");
            a_oFWDocGen.OutputPath = AppSettings.GetValue("FWDocGenOutputPath");
            a_oFWDocGen.TemplatePath = AppSettings.GetValue("FWDocGenTemplatePath");
            a_oFWDocGen.LocalPath = AppSettings.GetValue("FWDocGenLocalPath");
        }
        public ResultReturn GenerateFundRider(FundWizard a_oFW)
        {
            ResultReturn oReturn = new();
            string sError = String.Empty;
            try
            {
                //check if it is not NAV@CSC
                FWDocGen oFWDocGen = new(a_oFW);

                SetFWDocGenPaths(oFWDocGen);
                oReturn.confirmationNo = oFWDocGen.CreateFundRaiders(ref sError);
                if (!string.IsNullOrEmpty(oReturn.confirmationNo) && oReturn.confirmationNo != "N/A")
                {
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.confirmationNo = string.Empty;
                    oReturn.Errors.Add(new ErrorInfo(-1, sError, ErrorSeverityEnum.Error));
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
        public ResultReturn QDIANotice(FundWizard a_oFW)
        {
            ResultReturn oReturn = new();
            DataSet dsTask;
            string sFileName;
            string sError = "";
            string sInfo = "";
            try
            {
                sInfo = "Contract: " + a_oFW.ContractId + " SubId: " + a_oFW.SubId + " CaseNo: " + a_oFW.CaseNo.ToString();
                dsTask = a_oFW.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.AnnualPptNotice.GetHashCode());
                if (dsTask.Tables[0].Rows.Count > 0 && (int)dsTask.Tables[0].Rows[0]["status"] == 100)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    FWDocGen oFWDocGen = new(a_oFW);
                    SetFWDocGenPaths(oFWDocGen);

                    if ((FWUtils.GetHdrData(FWUtils.C_hdr_PortXpress_custom, a_oFW.PdfHeader)[0] == "true") &&
                        (FWUtils.GetHdrData(FWUtils.C_hdr_default_fund_new, a_oFW.PdfHeader)[0] == "-1"))// -1 is portfolioxpress
                    {
                        sFileName = "N/A"; // do NOT Create FundQDIANotice when Custom PX = yes AND Sponsor changes QDIA to be PX (JIRA # IT-66105)
                    }
                    else
                    {
                        sFileName = oFWDocGen.CreateFundQDIANotice(ref sError);
                    }
                    if (!string.IsNullOrEmpty(sFileName) && sFileName != "N/A")
                    {
                        BFLModel.SIResponse oResponse;
                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                        // letter
                        oResponse = a_oFW.SendSponsorQdiaNoticeToMC(Path.GetFileName(sFileName), sFileName);
                        if (oResponse.Errors[0].Number != 0)
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.Errors.Add(new ErrorInfo(oResponse.Errors[0].Number, sInfo + "Error in SendSponsorQdiaNoticeToMC(): " + oResponse.Errors[0].Description, ErrorSeverityEnum.Error));
                        }
                    }
                    else
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(-1, sInfo + " - Unable to generate Annual Participant Notice: " + sError, ErrorSeverityEnum.Failed));

                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                        ResultReturn oRQdia = SendQDiaFailureEmail(a_oFW.ContractId, a_oFW.SubId, a_oFW.CaseNo);
                        if (oRQdia.returnStatus == ReturnStatusEnum.Failed)
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            General.CopyResultError(oReturn, oRQdia);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, sInfo + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }
        private ResultReturn SendQDiaFailureEmail(string a_sConId, string a_sSubId, int a_iCaseNo)//IT-87161
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            try
            {
                MessageServiceKeyValue[] Keys;
                DriverSOA.MessageService oMS = new();
                const int C_MsgId_Qdia_CCI = 3100;
                Keys = new MessageServiceKeyValue[3];

                Keys[0] = new MessageServiceKeyValue();
                Keys[0].key = "contract_number";
                Keys[0].value = a_sConId;

                Keys[1] = new MessageServiceKeyValue();
                Keys[1].key = "sub_code";
                Keys[1].value = a_sSubId;

                Keys[2] = new MessageServiceKeyValue();
                Keys[2].key = "tracking_number";
                Keys[2].value = a_iCaseNo.ToString();

                oReturn = oMS.SendMessage(a_sConId, a_sSubId, C_MsgId_Qdia_CCI, Keys, "FundWizard-Backend");

                if (oReturn == null)
                {
                    ErrorInfo oError = new();
                    oReturn = new ResultReturn();
                    oError.errorNum = 1;
                    oError.errorDesc = "No result returned.";
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
