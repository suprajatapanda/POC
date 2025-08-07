using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using TRS.IT.TrsAppSettings;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
using TARSharedUtilLibBLL = TRS.IT.BendProcessor.BLL;
using TARSharedUtilLibModel = TRS.IT.BendProcessor.Model;
using TARSharedUtilLibUtil = TRS.IT.BendProcessor.Util;

namespace FWSignedDocsToMsgcntrBatch.BLL;
public class FWBend
{
    readonly TARSharedUtilLibBLL.FWBend fWBend;
    public FWBend(TARSharedUtilLibBLL.FWBend obj)
    {
        fWBend = obj;
    }
    public TARSharedUtilLibModel.TaskStatus ProcessInterimTasksISC()
    {
        TARSharedUtilLibModel.TaskStatus oTaskReturn = new();
        TARSharedUtilLibModel.ResultReturn oReturn = new();
        oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
        TARSharedUtilLibModel.ResultReturn oRetSnPLtr;
        TARSharedUtilLibModel.ResultReturn oRQdia;
        int iAction;
        DateTime dtPegasysDt;
        DateTime dtCreateDt;
        DateTime dtPartnerUpdateDt;
        string sPartnerId;
        string sQdia = "";
        string sInfo = "";
        DataSet dsTask;
        const string C_Task = "ProcessInterimTasksISC";
        const string C_TaskName = TARSharedUtilLibModel.ConstN.C_TAG_P_O + C_Task + TARSharedUtilLibModel.ConstN.C_TAG_P_C;
        bool bPXISC = false;
        try
        {
            oTaskReturn.retStatus = TARSharedUtilLibModel.TaskRetStatus.NotRun;
            if (AppSettings.GetValue(C_Task) == "1")
            {
                fWBend.InitTaskStatus(oTaskReturn, C_Task);
                if (fWBend.PendingFundChanges.Tables.Count > 1)
                {
                    foreach (DataRow dr in fWBend.PendingFundChanges.Tables[1].Rows) // rows from second table, all pending ISC FW requests.
                    {
                        try
                        {
                            iAction = (int)dr["change_type"];
                            dtPegasysDt = (DateTime)dr["pegasys_dt"];

                            sPartnerId = dr["partner_id"].ToString();
                            sQdia = "";
                            sInfo = "Contract: " + dr["contract_id"].ToString() + " SubId: " + dr["sub_id"].ToString() + " CaseNo: " + dr["case_no"].ToString() + " ";
                            TARSharedUtilLibBFLBLL.FundWizard oWF = new(Guid.NewGuid().ToString(), dr["contract_id"].ToString(), dr["sub_id"].ToString());
                            oWF.GetCaseNo((int)dr["case_no"]);
                            if (oWF.CreateDate != string.Empty) { dtCreateDt = Convert.ToDateTime(oWF.CreateDate); }
                            else { dtCreateDt = (DateTime)dr["create_dt"]; }

                            ////dtCreateDt = dtCreateDt.Date.AddDays(3);//Hardcode 3 days.
                            dsTask = oWF.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.UpdatePartnerSystem.GetHashCode());
                            if (dsTask.Tables[0].Rows.Count > 0 && Convert.ToInt32(dsTask.Tables[0].Rows[0]["status"].ToString()) == 100)// proceeed only if UpdatePartnerSystem task is completed
                            {
                                XElement xEl = XElement.Load(new StringReader(dsTask.Tables[0].Rows[0]["task_data"].ToString()));
                                XElement xProfile = xEl.XPathSelectElement("//UserProfile");
                                if ((xProfile != null))
                                {
                                    dtPartnerUpdateDt = Convert.ToDateTime((xProfile.Attribute("CreateDt").Value));
                                    dtPartnerUpdateDt = Convert.ToDateTime(TARSharedUtilLibBFLBLL.FWUtils.GetNextBusinessDay(dtPartnerUpdateDt.Date, 2)); // Hardcode 2 BUSINESS days days.

                                    //1.Generate QDIANotice
                                    sQdia = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_qdia_answer, oWF.PdfHeader)[0];

                                    if ((sQdia == "Yes") && (dtPartnerUpdateDt.CompareTo(DateTime.Today) <= 0)) // min wait 2 business days
                                    {
                                        if (oWF == null)
                                        {
                                            throw new Exception("Before QDIANotice() Lost oFW object");
                                        }

                                        oRQdia = fWBend.QDIANotice(oWF);
                                        if (oRQdia.returnStatus != TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
                                        {
                                            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                                            TARSharedUtilLibUtil.General.CopyResultError(oReturn, oRQdia);
                                            fWBend.SendErrorEmailToUsers(dr["contract_id"].ToString(), dr["sub_id"].ToString(), (int)dr["case_no"], sPartnerId, C_TaskName + TARSharedUtilLibUtil.General.ParseErrorText(oRQdia.Errors, "<br />"));
                                        }
                                        else
                                        {
                                            oTaskReturn.rowsCount += 1;
                                        }
                                    }

                                    //2. Generate sponsor letter documents

                                    bPXISC = false;
                                    if ((TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_portXpress_selected, oWF.PdfHeader)[0] == "true") && (oWF.PartnerId == "ISC"))
                                    {
                                        bPXISC = true;
                                    }

                                    if (iAction != 4 || bPXISC == true)
                                    {
                                        if (dtPartnerUpdateDt.CompareTo(DateTime.Today) <= 0) // min wait 2 business days
                                        {
                                            if (oWF == null)
                                            {
                                                throw new Exception("Before SponsorLetter() Lost oFW object");
                                            }

                                            oRetSnPLtr = SponsorLetter(oWF);

                                            if (oRetSnPLtr.returnStatus != TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
                                            {
                                                oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                                                TARSharedUtilLibUtil.General.CopyResultError(oReturn, oRetSnPLtr);
                                                fWBend.SendErrorEmailToUsers(dr["contract_id"].ToString(), dr["sub_id"].ToString(), (int)dr["case_no"], sPartnerId, C_TaskName + TARSharedUtilLibUtil.General.ParseErrorText(oRetSnPLtr.Errors, "<br />"));
                                            }
                                            else
                                            {
                                                oTaskReturn.rowsCount += 1;
                                            }
                                        }

                                    }

                                }
                            }

                        }
                        catch (Exception exi)
                        {
                            TARSharedUtilLibUtil.Utils.LogError(exi);
                            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                            oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, sInfo + " - " + exi.Message, TARSharedUtilLibModel.ErrorSeverityEnum.ExceptionRaised));
                        }
                    }//end foreach
                }
            }

            if (oReturn.returnStatus != TARSharedUtilLibModel.ReturnStatusEnum.Succeeded)
            {
                oTaskReturn.retStatus = TARSharedUtilLibModel.TaskRetStatus.ToCompletionWithErr;
                TARSharedUtilLibUtil.General.CopyResultError(oTaskReturn, oReturn);
            }
        }
        catch (Exception ex)
        {
            TARSharedUtilLibUtil.Utils.LogError(ex);
            fWBend.InitTaskError(oTaskReturn, ex, true);
        }


        return oTaskReturn;
    }
    private TARSharedUtilLibModel.ResultReturn SponsorLetter(TARSharedUtilLibBFLBLL.FundWizard a_oFW)
    {
        TARSharedUtilLibModel.ResultReturn oReturn = new();
        DataSet dsTask;
        string sFileName;
        string sError = "";
        string sInfo = "";
        try
        {
            sInfo = "Contract: " + a_oFW.ContractId + " SubId: " + a_oFW.SubId + " CaseNo: " + a_oFW.CaseNo.ToString() + " ";

            dsTask = a_oFW.GetTaskByTaskNo(BFLModel.FundWizardInfo.FwTaskTypeEnum.SponsorPptLetters.GetHashCode());
            if (dsTask.Tables[0].Rows.Count > 0 && (int)dsTask.Tables[0].Rows[0]["status"] == 100)
            {
                oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
            }
            else
            {
                FWDocGen oFWDocGen = new(a_oFW);
                SetFWDocGenPaths(oFWDocGen);
                sFileName = oFWDocGen.CreateSPnPPtLetters(ref sError, "pdf");
                if (!string.IsNullOrEmpty(sFileName) && string.IsNullOrEmpty(sError))
                {
                    BFLModel.SIResponse oResponse;
                    oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Succeeded;
                    // letter
                    oResponse = new FundWizard(a_oFW).SendSponsorPPTLetterToMC("PptNotice" + Path.GetExtension(sFileName), sFileName);
                    if (oResponse.Errors[0].Number != 0)
                    {
                        oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(oResponse.Errors[0].Number, sInfo + " - " + oResponse.Errors[0].Description, TARSharedUtilLibModel.ErrorSeverityEnum.Error));
                    }
                }
                else
                {
                    oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, "Unable to generate Sponsor/PPT letter " + sInfo + " - " + sError, TARSharedUtilLibModel.ErrorSeverityEnum.Failed));
                }
            }
        }
        catch (Exception ex)
        {
            TARSharedUtilLibUtil.Utils.LogError(ex);
            oReturn.returnStatus = TARSharedUtilLibModel.ReturnStatusEnum.Failed;
            oReturn.isException = true;
            oReturn.confirmationNo = string.Empty;
            oReturn.Errors.Add(new TARSharedUtilLibModel.ErrorInfo(-1, sInfo + " - " + ex.Message, TARSharedUtilLibModel.ErrorSeverityEnum.ExceptionRaised));
        }
        return oReturn;
    }
    private void SetFWDocGenPaths(FWDocGen a_oFWDocGen)
    {
        a_oFWDocGen.LicenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Aspose.Total.lic");
        a_oFWDocGen.OutputPath = AppSettings.GetValue("FWDocGenOutputPath");
        a_oFWDocGen.TemplatePath = AppSettings.GetValue("FWDocGenTemplatePath");
        a_oFWDocGen.LocalPath = AppSettings.GetValue("FWDocGenLocalPath");
    }
}