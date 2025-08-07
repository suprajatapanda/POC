using System.Data;
using FWFundDocsToWMSBatch.DAL;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace FWFundDocsToWMSBatch.BLL
{
    public class FWBend(TRS.IT.BendProcessor.BLL.FWBend obj)
    {
        private readonly FWBendDC _fWBendDC = new();

        readonly TRS.IT.BendProcessor.BLL.FWBend fWBend = obj;

        public TaskStatus ProcessImageDocsToWms()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            int iAction;
            DateTime dtPegasysDt;
            string sPartnerId;
            string sConId;
            string sSubId;
            int iCaseNo;
            DataSet dsNewCase;
            const string C_Task = "ProcessImageDocsToWms";
            const string C_TaskName = ConstN.C_TAG_P_O + C_Task + ConstN.C_TAG_P_C;
            bool bChangeExists;

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;

                if (AppSettings.GetValue(C_Task) == "1")
                {
                    fWBend.InitTaskStatus(oTaskReturn, C_Task);

                    // Image New Case first
                    dsNewCase = _fWBendDC.GetPendingListNewCase();
                    foreach (DataRow drN in dsNewCase.Tables[0].Rows)
                    {
                        iAction = Utils.CheckDBNullInt(drN["change_type"]);
                        sPartnerId = Utils.CheckDBNullStr(drN["partner_id"]);
                        sConId = Utils.CheckDBNullStr(drN["contract_id"]);
                        sSubId = Utils.CheckDBNullStr(drN["sub_id"]);
                        iCaseNo = Utils.CheckDBNullInt(drN["case_no"]);

                        try
                        {
                            oReturn = ImageRiderNewCase(sConId, sSubId, iCaseNo);
                            if (oReturn.returnStatus != ReturnStatusEnum.Succeeded)
                            {
                                General.CopyResultError(oTaskReturn, oReturn);
                                fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, oReturn.Errors[0].errorDesc + C_TaskName);
                                oTaskReturn.fatalErrCnt += 1;
                                oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                            }
                            oTaskReturn.rowsCount += 1;
                        }
                        catch (Exception ex1)
                        {
                            Logger.LogMessage(ex1.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                            throw new Exception("ImageDocuments function ConId: " + sConId + "-" + sSubId + " CaseNo: " + iCaseNo.ToString() + " ex: " + ex1.Message);
                        }

                    }

                    //Regular cases

                    foreach (DataRow dr in fWBend.PendingFundChanges.Tables[0].Rows)
                    {
                        iAction = Utils.CheckDBNullInt(dr["change_type"]);
                        sPartnerId = Utils.CheckDBNullStr(dr["partner_id"]);
                        sConId = Utils.CheckDBNullStr(dr["contract_id"]);
                        sSubId = Utils.CheckDBNullStr(dr["sub_id"]);
                        iCaseNo = Utils.CheckDBNullInt(dr["case_no"]);

                        dtPegasysDt = Convert.ToDateTime("01/01/1990");

                        switch (sPartnerId.ToUpper())
                        {
                            case ConstN.C_PARTNER_TAE:
                            case ConstN.C_PARTNER_PENCO:
                            case ConstN.C_PARTNER_ISC:
                                dtPegasysDt = Convert.ToDateTime(TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetNextBusinessDay((DateTime)dr["pegasys_dt"], 2));
                                break;

                            case ConstN.C_PARTNER_CPC:
                            case ConstN.C_PARTNER_SEBS:
                            case ConstN.C_PARTNER_TRS:
                                dtPegasysDt = Convert.ToDateTime(TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetNextBusinessDay((DateTime)dr["pegasys_dt"], 3));
                                break;
                        }

                        if (iAction == 7)
                        {
                            ResultReturn oReturn1 = new();
                            TRS.IT.SI.BusinessFacadeLayer.FundWizard oWF = new(Guid.NewGuid().ToString(), sConId, sSubId);
                            string sInfo = "Contract: " + sConId + " SubId: " + sSubId + " CaseNo: " + iCaseNo.ToString() + " ";

                            try
                            {
                                oWF.GetCaseNo(iCaseNo);
                                dtPegasysDt = (DateTime)dr["pegasys_dt"];

                                if (dtPegasysDt.CompareTo(DateTime.Today) <= 0)
                                {
                                    TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse oResponse = oWF.UpdateFundChangeComplete();
                                    if (oResponse.Errors[0].Number != 0)
                                    {
                                        oReturn1.returnStatus = ReturnStatusEnum.Failed;
                                        oReturn1.Errors.Add(new ErrorInfo(-1, sInfo + oResponse.Errors[0].Description, ErrorSeverityEnum.Error));
                                    }
                                }
                            }
                            catch (Exception ex)
                            { Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat); }

                        }

                        else if (iAction == 8) //and no fund change
                        {
                            bChangeExists = PendingFundChangesByContract(sConId, sSubId);
                            if (!bChangeExists)
                            {
                                string sInfo;
                                ResultReturn oReturn1 = new();
                                TRS.IT.SI.BusinessFacadeLayer.FundWizard oWF = new(Guid.NewGuid().ToString(), sConId, sSubId);
                                sInfo = "Contract: " + sConId + " SubId: " + sSubId + " CaseNo: " + iCaseNo.ToString() + " ";
                                try
                                {
                                    oWF.GetCaseNo(iCaseNo);
                                    dtPegasysDt = (DateTime)dr["pegasys_dt"];

                                    if (dtPegasysDt.CompareTo(DateTime.Today) <= 0)
                                    {
                                        TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse oResponse = oWF.UpdateFundChangeComplete();
                                        if (oResponse.Errors[0].Number != 0)
                                        {
                                            oReturn1.returnStatus = ReturnStatusEnum.Failed;
                                            oReturn1.Errors.Add(new ErrorInfo(-1, sInfo + oResponse.Errors[0].Description, ErrorSeverityEnum.Error));
                                        }
                                    }
                                }
                                catch (Exception ex)
                                { Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat); }

                            }
                        }
                        else
                        {
                            if (dtPegasysDt.CompareTo(DateTime.Today) == 0)
                            {
                                try
                                {
                                    oReturn = ImageDocuments(sConId, sSubId, iCaseNo);
                                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded)
                                    {
                                        General.CopyResultError(oTaskReturn, oReturn);
                                        fWBend.SendErrorEmailToUsers(sConId, sSubId, iCaseNo, sPartnerId, oReturn.Errors[0].errorDesc + C_TaskName);
                                        oTaskReturn.fatalErrCnt += 1;
                                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                    }
                                    oTaskReturn.rowsCount += 1;
                                }
                                catch (Exception ex1)
                                {
                                    Logger.LogMessage(ex1.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                                    throw new Exception("ImageDocuments function ConId: " + sConId + "-" + sSubId + " CaseNo: " + iCaseNo.ToString() + " ex: " + ex1.Message);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                fWBend.InitTaskError(oTaskReturn, ex, true);
            }

            return oTaskReturn;
        }

        private ResultReturn ImageRiderNewCase(string a_sConId, string a_sSubId, int a_iCaseNo)
        {
            ResultReturn oReturn = new();
            TRS.IT.SI.BusinessFacadeLayer.FundWizard oWF = new(Guid.NewGuid().ToString(), a_sConId, a_sSubId);
            DataSet dsTask;
            DataSet dsDocToImage;
            string sError;
            bool bRiderDoc = false;
            string sFileNameNPath = string.Empty;
            string sGoodFiles = string.Empty;
            string sBadFiles = string.Empty;
            int iDocType;
            string sInfo = "ImageRiderNewCase() - Contract: " + a_sConId + " SubId: " + a_sSubId + " CaseNo: " + a_iCaseNo.ToString() + " ";
            TRS.IT.BendProcessor.DriverSOA.DocumentService oDocSrv = new();

            try
            {
                oWF.GetCaseNo(a_iCaseNo);
                dsTask = oWF.GetTaskByTaskNo(TRS.IT.SI.BusinessFacadeLayer.Model.FundWizardInfo.FwTaskTypeEnum.DocsImaged.GetHashCode());
                if (dsTask.Tables[0].Rows.Count > 0)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    TRS.IT.SI.BusinessFacadeLayer.Contract oBFLContract = new(oWF.ContractId, oWF.SubId);
                    bool bNavAtCSC = oBFLContract.IsNAVProduct(oWF.ContractId, oWF.SubId);

                    dsDocToImage = oWF.GetDocsToImage();

                    if (dsDocToImage.Tables[0].Rows.Count < 2 && oWF.Action != 4 && oWF.Action != 6)
                    {
                        //check if it is not NAV@CSC
                        if (!bNavAtCSC)
                        {
                            ResultReturn oR = fWBend.GenerateFundRider(oWF);
                            if (oR.returnStatus != ReturnStatusEnum.Succeeded)
                                throw new Exception("Error generating Fund Rider : " + oR.Errors[0].errorDesc);
                            else
                                dsDocToImage = oWF.GetDocsToImage();
                        }
                    }

                    foreach (DataRow dr in dsDocToImage.Tables[0].Rows)
                    {
                        try
                        {
                            sFileNameNPath = Path.Combine(dr["file_path"].ToString(), dr["file_name"].ToString());
                            iDocType = Convert.ToInt32(dr["DocTypeID"]);
                            switch (iDocType)
                            {
                                case 185:
                                    bRiderDoc = true;
                                    break;
                            }
                            if (bRiderDoc)
                            {
                                sError = oDocSrv.ImageDocument(a_sConId, a_sSubId, iDocType, sFileNameNPath, a_iCaseNo);
                                if (sError == string.Empty)
                                    sGoodFiles += sGoodFiles == "" ? sFileNameNPath : ";" + sFileNameNPath;
                                else
                                    sBadFiles += sBadFiles == "" ? sFileNameNPath : ";" + sFileNameNPath;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                            sBadFiles = sBadFiles == "" ? sFileNameNPath : ";" + sFileNameNPath;
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.Errors.Add(new ErrorInfo(-1, sInfo + ex.Message, ErrorSeverityEnum.ExceptionRaised));
                        }
                    }

                    oWF.InsertTaskImageWms(sGoodFiles.Split(';'), sBadFiles.Split(';'));

                    if (oReturn.returnStatus != ReturnStatusEnum.Failed || bNavAtCSC)
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                        TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse oResponse = oWF.UpdateFundChangeComplete();
                        if (oResponse.Errors[0].Number != 0)
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.Errors.Add(new ErrorInfo(-1, sInfo + " UpdateFundChangeComplete() Failed " + oResponse.Errors[0].Description, ErrorSeverityEnum.Error));
                        }
                    }
                    else
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(-1, sInfo + "InsertTaskImageWms() Failed. Some documents were not imaged.", ErrorSeverityEnum.Error));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, sInfo + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;

        }

        private ResultReturn ImageDocuments(string a_sConId, string a_sSubId, int a_iCaseNo)
        {

            ResultReturn oReturn = new();
            TRS.IT.SI.BusinessFacadeLayer.FundWizard oWF = new(Guid.NewGuid().ToString(), a_sConId, a_sSubId);
            DataSet dsTask;
            DataSet dsDocToImage;
            string sError;
            string sFileNameNPath = string.Empty;
            string sGoodFiles = string.Empty;
            string sBadFiles = string.Empty;
            int iDocType;
            bool bAllFile;
            bool bUpdateComplete; //whethe or not to update fund change status to complete
            int[] iDocsFound = [0, 0, 0, 0, 0, 0, 0, 0];
            string sFileMissing = "";
            const int C_Rider = 0;
            const int C_PptLetter = 1;
            const int C_InvestRequest = 2;
            const int C_InvestSigned = 3;
            const int C_QdiaNotice = 4;
            const int C_PXAddendumApproved = 5;
            const int C_PXCustomChangeRequestApproved = 6;
            const int C_PXChangeAuthorizationApproved = 7;

            string sInfo = "Error in ImageDocuments() - Contract: " + a_sConId + " SubId: " + a_sSubId + " CaseNo: " + a_iCaseNo.ToString() + " ";
            TRS.IT.BendProcessor.DriverSOA.DocumentService oDocSrv = new();

            try
            {
                oWF.GetCaseNo(a_iCaseNo);
                dsTask = oWF.GetTaskByTaskNo(TRS.IT.SI.BusinessFacadeLayer.Model.FundWizardInfo.FwTaskTypeEnum.DocsImaged.GetHashCode());
                if (dsTask.Tables[0].Rows.Count > 0)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    TRS.IT.SI.BusinessFacadeLayer.Contract oBFLContract = new(oWF.ContractId, oWF.SubId);
                    bool bNavAtCSC = oBFLContract.IsNAVProduct(oWF.ContractId, oWF.SubId);

                    dsDocToImage = oWF.GetDocsToImage();
                    foreach (DataRow drR in dsDocToImage.Tables[0].Rows)
                    {
                        if ((int)drR["DocTypeID"] == 185)
                        {
                            iDocsFound[C_Rider] = 1;
                            break;
                        }
                    }

                    if (iDocsFound[C_Rider] == 0 && oWF.Action != 4 && oWF.Action != 6) // exclude QDIA only and PX only fund changes
                    {
                        if (!bNavAtCSC)
                        {
                            ResultReturn oR = fWBend.GenerateFundRider(oWF);
                            if (oR.returnStatus != ReturnStatusEnum.Succeeded)
                                throw new Exception("Error generating Fund Rider : " + oR.Errors[0].errorDesc);
                            else
                                dsDocToImage = oWF.GetDocsToImage();
                        }
                    }
                    if (bNavAtCSC)
                        iDocsFound[C_Rider] = 2;
                    if (oWF.SignMethod == 0)
                        iDocsFound[C_InvestSigned] = 2;
                    if (oWF.Action == 6)
                    {
                        iDocsFound[C_InvestRequest] = 2;
                        iDocsFound[C_InvestSigned] = 2;
                        iDocsFound[C_Rider] = 2;
                    }
                    if (TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_default_fund_qdia_answer, oWF.PdfHeader)[0] != "Yes")
                        iDocsFound[C_QdiaNotice] = 2;

                    bool bPxISC = false;
                    if ((TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_portXpress_selected, oWF.PdfHeader)[0] == "true") && (oWF.PartnerId == "ISC"))
                    {
                        bPxISC = true;
                    }
                    if (oWF.Action == 4)
                    {
                        if (bPxISC == false)
                        {
                            iDocsFound[C_PptLetter] = 2; //ISC px docs exception
                        }
                        iDocsFound[C_Rider] = 2;
                    }

                    if (oWF.SignMethod == 1 && (TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_portXpress_selected, oWF.PdfHeader)[0] == "true"))
                    {
                        if ((oWF.NewFundsCustomPX == null) || oWF.NewFundsCustomPX.Rows.Count == 0)
                        {
                            iDocsFound[C_PXCustomChangeRequestApproved] = 2;
                        }

                        if ((TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_PortXpress_is_material, oWF.PdfHeader)[0] != "Yes" &&
                            TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_PortXpress_is_material_qdia, oWF.PdfHeader)[0] != "Yes" &&
                            TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_PortXpress_is_material_custom, oWF.PdfHeader)[0] != "Yes"))
                        {
                            iDocsFound[C_PXChangeAuthorizationApproved] = 2;
                        }

                        if (TRS.IT.SI.BusinessFacadeLayer.FWUtils.GetHdrData(TRS.IT.SI.BusinessFacadeLayer.FWUtils.C_hdr_default_fund_new, oWF.PdfHeader)[0] != "-1")
                        {
                            iDocsFound[C_PXAddendumApproved] = 2;
                        }

                    }
                    else // system will not generate px related approved (esigned) documents
                    {
                        iDocsFound[C_PXAddendumApproved] = 2;
                        iDocsFound[C_PXCustomChangeRequestApproved] = 2;
                        iDocsFound[C_PXChangeAuthorizationApproved] = 2;
                    }


                    foreach (DataRow dr in dsDocToImage.Tables[0].Rows)
                    {
                        try
                        {
                            bool bSkip = false;
                            sFileNameNPath = Path.Combine(dr["file_path"].ToString(), dr["file_name"].ToString());
                            iDocType = Convert.ToInt32(dr["DocTypeID"]);
                            switch (iDocType)
                            {
                                case 185: //rider
                                    iDocsFound[C_Rider] = 1;
                                    break;
                                case 284: //ppt letter
                                    iDocsFound[C_PptLetter] = 1;
                                    break;
                                case 324: // invest request app
                                    iDocsFound[C_InvestRequest] = 1;
                                    if (oWF.SignMethod == 1)
                                        bSkip = true;
                                    break;
                                case 673: // invest signed app
                                    iDocsFound[C_InvestSigned] = 1;
                                    break;
                                case 653: // qdia ppt notice
                                    iDocsFound[C_QdiaNotice] = 1;
                                    break;
                                case 685: // PXAddendum
                                    iDocsFound[C_PXAddendumApproved] = 1;
                                    break;

                                case 736: // PortfolioXpress Custom Investment Option Selection
                                    iDocsFound[C_PXCustomChangeRequestApproved] = 1;
                                    break;

                                case 737: // PortfolioXpress Change Authorization 
                                    iDocsFound[C_PXChangeAuthorizationApproved] = 1;
                                    break;

                            }
                            if (!bSkip)
                            {
                                sError = oDocSrv.ImageDocument(a_sConId, a_sSubId, iDocType, sFileNameNPath, a_iCaseNo);
                                if (sError == string.Empty)
                                    sGoodFiles += sGoodFiles == "" ? sFileNameNPath : ";" + sFileNameNPath;
                                else
                                    sBadFiles += sBadFiles == "" ? sFileNameNPath : ";" + sFileNameNPath;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                            sBadFiles = sBadFiles == "" ? sFileNameNPath : ";" + sFileNameNPath;
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.Errors.Add(new ErrorInfo(-1, sInfo + ex.Message, ErrorSeverityEnum.ExceptionRaised));
                        }
                    }

                    oWF.InsertTaskImageWms(sGoodFiles.Split(';'), sBadFiles.Split(';'));
                    bAllFile = true;
                    bUpdateComplete = true;

                    for (int iI = 0; iI < 8; iI++)
                    {
                        if (iDocsFound[iI] == 0)
                        {
                            bAllFile = false;
                            switch (iI)
                            {
                                case C_InvestRequest:
                                    sFileMissing += "Investment Change Request -";
                                    bUpdateComplete = false;
                                    break;
                                case C_InvestSigned:
                                    sFileMissing += "Investment Change Request Signed-";
                                    bUpdateComplete = false;
                                    break;
                                case C_PptLetter:
                                    sFileMissing += "PPT/Sponsor Letter -";
                                    bUpdateComplete = false;
                                    break;
                                case C_QdiaNotice:
                                    sFileMissing += "Annual PPT Notice -";
                                    break;
                                case C_Rider:
                                    sFileMissing += "Fund Rider -";
                                    bUpdateComplete = false;
                                    break;
                                case C_PXAddendumApproved:
                                    sFileMissing += "PortfolioXpress Addendum Signed-";
                                    bUpdateComplete = false;
                                    break;
                                case C_PXChangeAuthorizationApproved:
                                    sFileMissing += "PortfolioXpress ChangeAuthorization Signed-";
                                    bUpdateComplete = false;
                                    break;
                                case C_PXCustomChangeRequestApproved:
                                    sFileMissing += "PortfolioXpress CustomChangeRequest Signed-";
                                    bUpdateComplete = false;
                                    break;
                            }
                        }
                    }


                    if (bUpdateComplete && oReturn.returnStatus != ReturnStatusEnum.Failed)
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                        TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse oResponse = oWF.UpdateFundChangeComplete();
                        if (oResponse.Errors[0].Number != 0)
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.Errors.Add(new ErrorInfo(-1, sInfo + oResponse.Errors[0].Description, ErrorSeverityEnum.Error));
                        }

                        if (!bAllFile)
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.Errors.Add(new ErrorInfo(-1, sInfo + " Missing file(s): " + sFileMissing, ErrorSeverityEnum.Error));
                        }

                    }
                    else
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(-1, sInfo + "Missing file(s): " + sFileMissing, ErrorSeverityEnum.Error));
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, sInfo + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        public bool PendingFundChangesByContract(string contractId, string subId)
        {
            DataSet _pendingFundChanges;
            {
                _pendingFundChanges = _fWBendDC.GetPendingFundChangesForContract(contractId, subId);

                if (_pendingFundChanges.Tables.Count > 0)
                {
                    if (_pendingFundChanges.Tables[0].Rows.Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
