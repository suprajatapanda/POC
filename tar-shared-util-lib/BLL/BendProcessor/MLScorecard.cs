using System.Collections;
using System.Data;
using System.Text;
using System.Xml.Linq;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using BFL = TRS.IT.SI.BusinessFacadeLayer;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace TRS.IT.BendProcessor.BLL
{
    public class MLScorecard : BendProcessorBase
    {

        private MLScorecardDC _oMLScorecardDC;

        public MLScorecard() : base("93", "MLScorecard", "TRS") { _oMLScorecardDC = new MLScorecardDC(); InitializeReportDateAndTimeAllianceKey(); }

        //values of these values are set in contructor via SetReportDateAndTimeAllianceKey() method
        private int iTimeAllianceKey;
        private string strPrvMonthDate;
        private DateTime dtReportDate;
        private DateTime dtReportBeginDate;
        private DateTime dtYearToDateFrom;
        private int iAllianceID = 64680;
        public TaskStatus ProcessAllMLFlatFilesMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn1 = new();
            ResultReturn oReturn2 = new();
            ResultReturn oReturn3 = new();

            const string C_Task = "ProcessAllMLFlatFiles";


            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    StringBuilder sbErr = new();

                    //----validate if constructor set these values properly
                    if (iTimeAllianceKey <= 0 || dtReportDate == DateTime.MinValue || dtReportDate == DateTime.MaxValue)
                    {
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                        oTaskReturn.errors.Add(new ErrorInfo(-1, "Error in ProcessMLFlatFiles() -  GetReportDateAndTimeAllianceKey returned invalid values: iTimeAllianceKey = " + iTimeAllianceKey.ToString() + " dtReportDate = " + dtReportDate.ToString(), ErrorSeverityEnum.Failed));
                        SendTaskCompleteEmail("ProcessMLFlatFiles Status - " + TaskRetStatus.ToCompletionWithErr.ToString(), General.ParseTaskInfo(oTaskReturn), oTaskReturn.taskName);
                        return oTaskReturn;
                    }

                    ////------------------------------------------------------------------------------------
                    oReturn1 = ProcessAgreegateFlatFiles();

                    if (oReturn1.returnStatus != ReturnStatusEnum.Succeeded || oReturn1.Errors.Count > 0)
                    {
                        sbErr.Length = 0;
                        //send error
                        foreach (ErrorInfo oEr in oReturn1.Errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("ProcessAgreegateFlatFiles Status - " + "Failed", sbErr.ToString(), oTaskReturn.taskName);

                        General.CopyResultError(oTaskReturn, oReturn1);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.rowsCount++;

                    ////------------------------------------------------------------------------------------
                    oReturn2 = ProcessRolloverFlatFiles();

                    if (oReturn2.returnStatus != ReturnStatusEnum.Succeeded || oReturn2.Errors.Count > 0)
                    {
                        sbErr.Length = 0;
                        //send error
                        foreach (ErrorInfo oEr in oReturn2.Errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("ProcessRolloverFlatFiles Status - " + "Failed", sbErr.ToString(), oTaskReturn.taskName);

                        General.CopyResultError(oTaskReturn, oReturn2);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.rowsCount++;

                    ////------------------------------------------------------------------------------------
                    oReturn3 = ProcessPlanDetailFlatFiles();

                    if (oReturn3.returnStatus != ReturnStatusEnum.Succeeded || oReturn3.Errors.Count > 0)
                    {
                        sbErr.Length = 0;
                        //send error
                        foreach (ErrorInfo oEr in oReturn3.Errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("ProcessPlanDetailFlatFiles Status - " + "Failed", sbErr.ToString(), oTaskReturn.taskName);


                        General.CopyResultError(oTaskReturn, oReturn3);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }

                    oTaskReturn.rowsCount++;
                    ////------------------------------------------------------------------------------------

                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }


        public ResultReturn ProcessAgreegateFlatFiles()
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            string sTxtFilename = "";
            string sCsvFileName = "";
            string sDestinationDir = "";
            try
            {
                sDestinationDir = AppSettings.GetValue("MLFlatFilesDestinationDirPath");

                if (string.IsNullOrEmpty(sDestinationDir))
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "ProcessAgreegateFlatFiles: Missing configuration appsetting entries - MLFlatFilesDestinationDirPath", ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                //-------------create files-------------
                ResultReturn oRet1 = CreateAgreegateFlatFiles(ref sTxtFilename, ref sCsvFileName);
                if (oRet1.returnStatus != ReturnStatusEnum.Succeeded || oRet1.Errors.Count > 0)
                {
                    General.CopyResultError(oReturn, oRet1);
                    foreach (ErrorInfo err in oRet1.Errors)
                    {
                        if (err.severity == ErrorSeverityEnum.Error || err.severity == ErrorSeverityEnum.ExceptionRaised || err.severity == ErrorSeverityEnum.Failed)
                        {
                            return oReturn;
                        }
                    }

                }

                if (sTxtFilename == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "ProcessAgreegateFlatFiles: Failed to create- TXT AgreegateFlatFile: " + sTxtFilename, ErrorSeverityEnum.Failed));
                }
                else
                {
                    //-----------upload file-------------
                    File.Copy(sTxtFilename, Path.Combine(sDestinationDir, "GAPTRAEWI.txt"), true);
                }

                if (sCsvFileName == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "ProcessAgreegateFlatFiles: Failed to create- CSV AgreegateFlatFile: " + sCsvFileName, ErrorSeverityEnum.Failed));
                }
                else
                {
                    //-----------upload file-------------
                    File.Copy(sCsvFileName, Path.Combine(sDestinationDir, Path.GetFileName(sCsvFileName)), true);
                }

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in ProcessAgreegateFlatFiles  - sTxtFilename = " + sTxtFilename + " sCsvFileName = " + sCsvFileName + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        public ResultReturn ProcessRolloverFlatFiles()
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            string sTxtFilename = "";
            string sCsvFileName = "";
            string sDestinationDir = "";
            try
            {
                sDestinationDir = AppSettings.GetValue("MLFlatFilesDestinationDirPath");

                if (string.IsNullOrEmpty(sDestinationDir))
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "ProcessRolloverFlatFiles: Missing configuration appsetting entries - MLFlatFilesDestinationDirPath", ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                //-------------create files-------------
                ResultReturn oRet1 = CreateRollOverFlatFiles(ref sTxtFilename, ref sCsvFileName);
                if (oRet1.returnStatus != ReturnStatusEnum.Succeeded || oRet1.Errors.Count > 0)
                {
                    General.CopyResultError(oReturn, oRet1);
                    foreach (ErrorInfo err in oRet1.Errors)
                    {
                        if (err.severity == ErrorSeverityEnum.Error || err.severity == ErrorSeverityEnum.ExceptionRaised || err.severity == ErrorSeverityEnum.Failed)
                        {
                            return oReturn;
                        }
                    }
                }

                if (sTxtFilename == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "ProcessRolloverFlatFiles: Failed to create- TXT RolloverFlatFile: " + sTxtFilename, ErrorSeverityEnum.Failed));
                }
                else
                {
                    //-----------upload file-------------
                    File.Copy(sTxtFilename, Path.Combine(sDestinationDir, "GAPTRARI.txt"), true);
                }

                if (sCsvFileName == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "ProcessRolloverFlatFiles: Failed to create- CSV RolloverFlatFile: " + sCsvFileName, ErrorSeverityEnum.Failed));
                }
                else
                {
                    //-----------upload file-------------
                    File.Copy(sCsvFileName, Path.Combine(sDestinationDir, Path.GetFileName(sCsvFileName)), true);
                }

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in ProcessRolloverFlatFiles  - sTxtFilename = " + sTxtFilename + " sCsvFileName = " + sCsvFileName + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        public ResultReturn ProcessPlanDetailFlatFiles()
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            string sTxtFilename = "";
            string sCsvFileName = "";
            string sDestinationDir = "";
            try
            {
                sDestinationDir = AppSettings.GetValue("MLFlatFilesDestinationDirPath");

                if (string.IsNullOrEmpty(sDestinationDir))
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "ProcessPlanDetailFlatFiles: Missing configuration appsetting entries - MLFlatFilesDestinationDirPath", ErrorSeverityEnum.Failed));
                    return oReturn;
                }

                //-------------create files-------------
                ResultReturn oRet1 = CreatePlanDetailFlatFiles(ref sTxtFilename, ref sCsvFileName);
                if (oRet1.returnStatus != ReturnStatusEnum.Succeeded || oRet1.Errors.Count > 0)
                {
                    General.CopyResultError(oReturn, oRet1);
                    foreach (ErrorInfo err in oRet1.Errors)
                    {
                        if (err.severity == ErrorSeverityEnum.Error || err.severity == ErrorSeverityEnum.ExceptionRaised || err.severity == ErrorSeverityEnum.Failed)
                        {
                            return oReturn;
                        }
                    }
                }

                if (sTxtFilename == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "ProcessPlanDetailFlatFiles: Failed to create- TXT PlanDetailFlatFile: " + sTxtFilename, ErrorSeverityEnum.Failed));
                }
                else
                {
                    //-----------upload file-----------
                    File.Copy(sTxtFilename, Path.Combine(sDestinationDir, "GAPTRADI.txt"), true);
                }

                if (sCsvFileName == string.Empty)
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "ProcessPlanDetailFlatFiles: Failed to create- CSV PlanDetailFlatFile: " + sCsvFileName, ErrorSeverityEnum.Failed));
                }
                else
                {
                    //-----------upload file-----------
                    File.Copy(sCsvFileName, Path.Combine(sDestinationDir, Path.GetFileName(sCsvFileName)), true);
                }

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in ProcessPlanDetailFlatFiles  - sTxtFilename = " + sTxtFilename + " sCsvFileName = " + sCsvFileName + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        public TaskStatus ProcessISCParticipantCountMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn1 = new();
            string sInputContracts = "";
            const string C_Task = "ProcessISCParticipantCount";
            int iRowCount = 0;
            int iTemp = 0;
            string prv_contract_id = "";
            string contract_id = "";
            string sInfo = "";
            int iPptWithBalance_MaxCasesCount = 200;
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    StringBuilder sbErr = new();
                    ////-----------------0. Delete original list of ISC contacts from wn_part_hdr---------------------------------------------------------
                    _oMLScorecardDC.Delete_wn_part_hdr_Data("ISC");

                    ////-----------------1. Get ISC contracts list from db-------------------------------------------------------------------
                    DataSet ds = _oMLScorecardDC.GetISCContracts(DateTime.Today);

                    if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                    {

                        iPptWithBalance_MaxCasesCount = 200;

                        if (AppSettings.GetValue("PptWithBalance_MaxCasesCount") != null && AppSettings.GetValue("PptWithBalance_MaxCasesCount") != "")
                        {
                            iPptWithBalance_MaxCasesCount = Convert.ToInt32(AppSettings.GetValue("PptWithBalance_MaxCasesCount"));
                        }

                        if (iPptWithBalance_MaxCasesCount <= 1)
                        {
                            iPptWithBalance_MaxCasesCount = 2; // <-- Paranoid programming ;-)
                        }

                        GeneralDC oGenDC = new();
                        StringBuilder sb = new();

                        DataView dv = ds.Tables[0].DefaultView;
                        dv.Sort = "contract_id asc"; // sorting is importatnt because we process only @200 contract rows in one batch and while saving the results we consolidate ppt count for all sub_ids so we waant to have all sub_ids for a contract in one batch
                        DataTable dt = dv.ToTable();
                        iRowCount = dt.Rows.Count;

                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow dr = dt.Rows[i];
                            iTemp++;

                            if (dr["contract_id"] != null && dr["contract_id"].ToString() != string.Empty)
                            {
                                try
                                {
                                    contract_id = "";
                                    contract_id = dr["contract_id"].ToString().Trim();
                                    sb.Append("'");
                                    sb.Append(contract_id);
                                    sb.Append("    ");// 4 spaces
                                    sb.Append(oGenDC.SubOut(dr["sub_id"].ToString()));
                                    sb.Append("',");
                                    // P3 webservice times out if we send too many contracts thats why limiting the batch to @200
                                    if ((prv_contract_id != contract_id && iTemp > iPptWithBalance_MaxCasesCount) || i == iRowCount - 1)
                                    {
                                        iTemp = 0; // reset

                                        sInputContracts = sb.ToString();
                                        if (sInputContracts != null && sInputContracts != string.Empty)
                                        {
                                            if (sInputContracts.EndsWith(","))
                                            {
                                                sInputContracts = sInputContracts.Remove(sInputContracts.Length - 1);
                                            }

                                            oReturn1 = GetISCpptCountFromP3AndSaveInDB(sInputContracts);

                                            if (oReturn1.returnStatus != ReturnStatusEnum.Succeeded || oReturn1.Errors.Count > 0)
                                            {
                                                sbErr.Length = 0;
                                                //send error
                                                foreach (ErrorInfo oEr in oReturn1.Errors)
                                                {
                                                    sbErr.AppendLine(oEr.errorDesc);
                                                }
                                                General.CopyResultError(oTaskReturn, oReturn1);
                                                oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                            }
                                        }

                                        sb.Length = 0;

                                    }

                                    prv_contract_id = contract_id;
                                }
                                catch (Exception exi2)
                                {
                                    Utils.LogError(exi2);
                                    oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                    oTaskReturn.fatalErrCnt++;
                                    oTaskReturn.errors.Add(new ErrorInfo(-1, "Error in ProcessISCParticipantCount:   " + sInfo + "Error: " + exi2.Message + "\r\n", ErrorSeverityEnum.Failed));

                                }
                            }

                        }

                    }

                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;

        }

        public TaskStatus ProcessMLBORTOAndAWAYMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn1 = new();

            string sFileName = "";
            const string C_Task = "ProcessMLBORTOAndAWAY";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    StringBuilder sbErr = new();

                    //----validate if constructor set these values properly
                    if (iTimeAllianceKey <= 0 || dtReportDate == DateTime.MinValue || dtReportDate == DateTime.MaxValue)
                    {
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                        oTaskReturn.errors.Add(new ErrorInfo(-1, "Error in ProcessMLBORTOAndAWAY() -  GetReportDateAndTimeAllianceKey returned invalid values: iTimeAllianceKey = " + iTimeAllianceKey.ToString() + " dtReportDate = " + dtReportDate.ToString(), ErrorSeverityEnum.Failed));
                        SendTaskCompleteEmail("ProcessMLBORTOAndAWAY Status - " + TaskRetStatus.ToCompletionWithErr.ToString(), General.ParseTaskInfo(oTaskReturn), oTaskReturn.taskName);
                        return oTaskReturn;
                    }

                    ////------------------------------------------------------------------------------------
                    oReturn1 = CreateBORTOANDAWAYReport(dtReportBeginDate, dtReportDate, dtReportBeginDate, dtReportBeginDate, ref sFileName); // generate only one months data

                    if (oReturn1.returnStatus != ReturnStatusEnum.Succeeded || oReturn1.Errors.Count > 0)
                    {
                        sbErr.Length = 0;
                        //send error
                        foreach (ErrorInfo oEr in oReturn1.Errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("ProcessMLBORTOAndAWAY Status - " + "Failed", sbErr.ToString(), oTaskReturn.taskName);

                        General.CopyResultError(oTaskReturn, oReturn1);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                    }
                    else
                    {
                        // send email?
                    }

                    oTaskReturn.rowsCount++;



                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        public ResultReturn CreateBORTOANDAWAYReport(DateTime dtBeginDate, DateTime dtReportEndDate, DateTime dtYTDFr, DateTime dtInception, ref string sCsvFileName)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            DataSet dsTo = null;
            DataSet dsAway = null;
            StringBuilder sbCSV = new();
            sCsvFileName = "";
            string sMLBORTOAndAWAYLocalFolder = "";
            int iTo = 0;
            int iAway = 0;
            int iMax = 0;
            string sPlanName = "";
            string sContractId = "";
            string sSubId = "";
            double dAsset = 0.0;
            double dTotalAssetTo = 0.0;
            double dTotalAssetAway = 0.0;
            DataRow drTo;
            DataRow drAway;
            string sErrInfo = "";
            BFL.Contract oBFLContract;
            try
            {
                sbCSV.Append(" ,Broker of Record Changes TO Merrill Lynch - " + dtReportDate.ToString("MMM yyyy") + ", , , , , , Broker of Record Changes AWAY From Merrill Lynch - " + dtReportDate.ToString("MMM yyyy") + " , \r\n");

                sbCSV.Append("Plan Number,Plan Name,Assets, , , ,Plan Number,Plan Name,Assets\r\n");
                sErrInfo = " In CreateBORTOANDAWAYReport \r\n";
                sMLBORTOAndAWAYLocalFolder = AppSettings.GetValue("MLBORTOAndAWAYLocalFolder");

                sCsvFileName = Path.Combine(sMLBORTOAndAWAYLocalFolder, "MLBORTOAndAWAY -" + dtReportDate.ToString("MMyyyy") + "-" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".csv");
                sErrInfo += " File name: " + sCsvFileName + "\r\n";

                sErrInfo += " Calling GetBORAssets() \r\n";
                dsTo = _oMLScorecardDC.GetBORAssets(iAllianceID, dtReportBeginDate, dtReportDate);
                sErrInfo += " Calling GetBORTermProcessed() \r\n";
                dsAway = _oMLScorecardDC.GetBORTermProcessed(iAllianceID, dtBeginDate, dtReportDate, dtBeginDate, dtBeginDate);

                if (dsTo != null && dsTo.Tables.Count > 0)
                {
                    iTo = dsTo.Tables[0].Rows.Count;
                }

                if (dsAway != null && dsAway.Tables.Count > 0)
                {
                    iAway = dsAway.Tables[0].Rows.Count;
                }

                iMax = iAway;
                if (iTo > iAway)
                {
                    iMax = iTo;
                }

                if (iMax > 0)
                {
                    for (int i = 0; i < iMax; i++)
                    {
                        sPlanName = "";
                        sContractId = "";
                        sSubId = "";
                        dAsset = 0.0;
                        if (i < iTo)
                        {
                            drTo = dsTo.Tables[0].Rows[i];

                            sContractId = Utils.CheckDBNullStr(drTo["contract_id"]);
                            sSubId = Utils.CheckDBNullStr(drTo["sub_id"]);
                            dAsset = Utils.CheckDBNullDb(drTo["assets"]);


                            //sbCSV.Append(sContractId + " - " + sSubId);
                            oBFLContract = new BFL.Contract(sContractId, sSubId);

                            if (oBFLContract.IsNAVProduct(sContractId, sSubId) == false)
                            {
                                dTotalAssetTo += dAsset;
                                sPlanName = GetPlanName(sContractId, sSubId);

                                sbCSV.Append(sContractId);
                                sbCSV.Append(", ");
                                sbCSV.Append(sPlanName);
                                sbCSV.Append(", ");
                                sbCSV.Append(dAsset.ToString("C").Replace(",", ""));
                                sbCSV.Append(", ");
                            }
                            else { sbCSV.Append(", , ,"); }
                        }
                        else
                        {
                            sbCSV.Append(", , ,");
                        }

                        sbCSV.Append(", , ,");

                        sPlanName = "";
                        sContractId = "";
                        sSubId = "";
                        dAsset = 0.0;
                        if (i < iAway)
                        {
                            drAway = dsAway.Tables[0].Rows[i];
                            sContractId = Utils.CheckDBNullStr(drAway["contract_id"]);
                            sSubId = Utils.CheckDBNullStr(drAway["sub_id"]);
                            dAsset = Utils.CheckDBNullDb(drAway["cur_mo_asset"]);
                            dTotalAssetAway += dAsset;
                            sPlanName = GetPlanName(sContractId, sSubId);

                            //sbCSV.Append(sContractId + " - " + sSubId);
                            sbCSV.Append(sContractId);
                            sbCSV.Append(", ");
                            sbCSV.Append(sPlanName);
                            sbCSV.Append(", ");
                            sbCSV.Append(dAsset.ToString("C").Replace(",", ""));

                        }
                        else
                        {
                            sbCSV.Append(", , ");
                        }

                        sbCSV.Append("\r\n");
                    }// for
                    sbCSV.Append(", , , , , , , , \r\n");
                    sbCSV.Append(" ,Total ," + dTotalAssetTo.ToString("C").Replace(",", "") + ", , , , , Total ," + dTotalAssetAway.ToString("C").Replace(",", "") + "\r\n");
                    sErrInfo += " Saving File \r\n";
                    StreamWriter oFile = new(sCsvFileName, false);
                    oFile.Write(sbCSV.ToString());
                    oFile.Close();

                    string sEmail = "";
                    //sEmail = @"Mindy.Oconnor@transamerica.com ;kathy.madigan@baml.com ;tori.white@baml.com ;Rose.Cozzolino@transamerica.com";
                    sErrInfo += " Sending Email\r\n";
                    sEmail = AppSettings.GetValue("MLBORTOAndAWAYEmailNotification");
                    if (sEmail == string.Empty)
                    {
                        sEmail = "Rose.Cozzolino@transamerica.com";
                    }
                    Utils.SendMail(AppSettings.GetValue(ConstN.C_BPROCESSOR_OUTSIDE_FROM_EMAIL), sEmail, "Broker of record changes To and Away Report: " + DateTime.Now.ToString("MMyyyy"), "Data attached.", [sCsvFileName], _sBCCEmailNotification);
                    sErrInfo += " Done. \r\n";
                }


            }
            catch (Exception exo)
            {
                Utils.LogError(exo);
                sCsvFileName = string.Empty;
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = sCsvFileName;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateBORTOANDAWAYReport() " + " Error: " + exo.Message + "<BR />" + Environment.NewLine + sErrInfo, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }
        #region **** private function ***

        private void InitializeReportDateAndTimeAllianceKey()// called from constructor
        {
            iTimeAllianceKey = 0;
            dtReportDate = DateTime.MinValue;
            strPrvMonthDate = "";
            DataSet ds = _oMLScorecardDC.GetReportDateAndTimeAllainceKey();
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                if (ds.Tables[0].Rows[0]["TimeAllianceKey"] != null)
                {
                    iTimeAllianceKey = Convert.ToInt32(ds.Tables[0].Rows[0]["TimeAllianceKey"].ToString());
                }

                if (ds.Tables[0].Rows[0]["ReportDate"] != null)
                {
                    strPrvMonthDate = ds.Tables[0].Rows[0]["ReportDate"].ToString(); // this is MMyyyy format

                    //dtReportDate = set the last day date from strPrvMonthDate which is in MMyyyy format
                    dtReportDate = new DateTime(Convert.ToInt32(strPrvMonthDate.Substring(2, 4)), Convert.ToInt32(strPrvMonthDate.Substring(0, 2)), 1);
                    dtReportBeginDate = dtReportDate; // first day of month

                    dtYearToDateFrom = new DateTime(dtReportDate.Year, 1, 1);

                    dtReportDate = dtReportDate.AddMonths(1);
                    dtReportDate = dtReportDate.AddDays(-1);// last day of the month
                }
            }
        }

        private string GetPlanName(string sContractId, string sSubId)
        {
            string sPlanName = "";

            try
            {
                sPlanName = SIPBO.SIShared.GetPlanName(sContractId, sSubId);
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                sPlanName = "";

            }
            sPlanName = sPlanName.Replace(",", ""); // remove comma because we are printing these values in .csv file
            return sPlanName;
        }
        private ResultReturn CreateAgreegateFlatFiles(ref string sTxtFilename, ref string sCsvFileName)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            DataSet ds = null;
            sTxtFilename = "";
            sCsvFileName = "";
            string sError = "";
            string sMlFlatFilesLocalFolder = "";
            try
            {
                sMlFlatFilesLocalFolder = AppSettings.GetValue("MlFlatFilesLocalFolder");
                sMlFlatFilesLocalFolder = Path.Combine(sMlFlatFilesLocalFolder, DateTime.Now.ToString("MM") + DateTime.Now.ToString("yyyy"));
                sTxtFilename = sMlFlatFilesLocalFolder;
                sCsvFileName = sMlFlatFilesLocalFolder;

                string sAggregateFlatFileHeader = "UHDRGAP-TRA " + dtReportDate.ToString("yyyyMMdd");
                string sAggregateFlatFileTrailer = "UTRL3";

                StringBuilder sb = new();
                ds = _oMLScorecardDC.GetAgreegateFileData(iTimeAllianceKey, strPrvMonthDate);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    if (dr != null)
                    {
                        sb.Append(sAggregateFlatFileHeader);
                        sb.AppendLine();

                        //-----Section 1 - Asset Data----------------------------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALACT"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "AggregateFlatFile Field: " + sError, ErrorSeverityEnum.Warning)); // start 1
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALORPH"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALBR"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALNONBR"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALLOAN"], 11, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALUNIQ"], 11, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVAL500K"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVAL1MM"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVAL5MM"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVAL10MM"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVAL>10MM"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALACT1"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALTERM"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        //-----Section 1 - Plan Counts----------------------------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLNACT"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 166
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLNORPH"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMORPHPROCED"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLUNIQ"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLN500K"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLN1MM"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLN5MM"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLN10MM"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLAN>10MM"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLN401K"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLNPSP"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLNMPP"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLN403B"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLN457"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLANNQ"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLNDB"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLNOTHER"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        //-----Section 1 - Participant Data----------------------------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPARTELIG"], 8, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 285
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMACTVBAL"], 8, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["TERMVESTBAL"], 8, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["TotalLOANOUT"], 8, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        //-----Section 2 - Pipeline/New Business Proposals----------------------------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPROP"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 317
                        }

                        //-----Section 2 - Sales----------------------------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLANSLD"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 322
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALPLNSLD"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMTKVRPLSLD"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["MKTVALTKVSLD"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMSTARTUPSLD"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMACTSTARTUP"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMWHLSALE"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["BORCHGAWAY"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["BDCHGAWAY"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["BORCHGIN"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["BDCHGIN"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        //-----Section 3 - Conversions-----------------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-NUMCONV"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 382
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-MKTVAL"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-NUMOUTSOLD"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-MKTOUTSTD"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-OUTSTD"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-MKTUNIQ"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-NUMPRTL"], 5, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-MKTPRTL"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        //------Section 4 - Deconversions------------------------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-NUMPL"], 4 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 447
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-NUMSVC"], 4 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-NUMIRS"], 4 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-MKTPL"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-MKTSVC"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-MKTIRS"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-NUMOUTSTD"], 4 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-MKTOUTSTD"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-NUMPTP"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-MKTUNIQ"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-NUMPRTL"], 4 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECONV-MKTPRTL"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["AVGLOS-SVC"], 4 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["AVGLOS-IRS"], 4 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        //---Section 5 - Cashiering / Contributions & Distributions-----------
                        sb.Append(CheckDBNullAndLengthStr(dr["CNTB-MKTVALEE"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 550
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNTB-MKTVALER"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNTB-MKTVALPS"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNTB-MKTVALROL"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DIST-MKTVAL"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DIST-MKTVALROL"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DIST-MKTBACROL"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        //----------Section 6 - Compliance---------------------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["CNTB-NUMOUTSTD"], 5 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 620
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNTB-MKTVALOUTSTD"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNTB-NUM>15"], 5 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DIST-NUMOUTSTD"], 5 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DIST-MKTVALOUTSTD"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTAUTOPAY"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTAUTOSTAT"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTELECCOMP"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTELEC5500"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVC-NUMPLN"], 6 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVC-MKTVALPLN"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVC-NUMNEWPLN"], 5 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVC-MKTVALNEW"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVC-NUMNOHLD"], 5 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVC-MKTVALREM"], 9 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        //----------Section 7 - Client Experience-----------------
                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPTPWEB"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 723
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTPTPSITE"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMPLSPWEB"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTPSSITE"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMFAWEB"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTFASITE"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMIVR"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTIVR"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMCLLCNTR-PTP"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMABAND-PTP"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTCALLOUTSTD-PTP"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["AVGCALL-PTP"], 4 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PSACCTMGR"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PSCALLCNTR"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMABAND-PS"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["PCNTCALLOUTSTD-PS"], 3 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["AVGCALL-PS"], 4 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CRMCLLRESOLV"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CRMCLLNOTRESLV"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        //------Section 8 - Auto Programs---------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["NUMAUTOENRL"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning)); // start 845
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-AUTOENRL"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["EXIST-AUTOENRL"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["TERM-AUTOENRL"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["EXIST-AEOPTOUT"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMAUTOINCR"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-AUTOINCR"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["EXIST-AUTOINCR"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["TERM-AUTOINCR"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["EXIST-AIOPTOUT"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NUMASSETALLOC"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CONV-ASSETALLOC"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["EXIST-ASSETALLOC"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["TERM-ASSETALLOC"], 7 + 1, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["EXIST-ALLOCOPTOUT"], 7, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Agreegate Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.AppendLine();
                        sb.Append(sAggregateFlatFileTrailer);

                        StreamWriter sw = null;
                        try
                        {

                            sTxtFilename = Path.Combine(sTxtFilename, "AgreegateFile_" + strPrvMonthDate + "_GAPTRAEWI.txt");
                            Utils.ValidatePath(sTxtFilename);
                            sw = new StreamWriter(sTxtFilename);
                            sw.WriteLine(sb.ToString());
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex);
                            sTxtFilename = "";
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateAgreegateFlatFiles() While creating TXT file. Error: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));

                        }
                        finally
                        {
                            if (sw != null)
                            {
                                sw.Close();
                            }
                        }


                        // now create CSV file
                        StreamWriter swCSV = null;
                        try
                        {
                            sCsvFileName = Path.Combine(sCsvFileName, "AgreegateFile_" + strPrvMonthDate + "_GAPTRAEWI.CSV");
                            Utils.ValidatePath(sCsvFileName);
                            swCSV = new StreamWriter(sCsvFileName);

                            string[] columnNames = (from dc in ds.Tables[0].Columns.Cast<DataColumn>()
                                                    select dc.ColumnName).ToArray();

                            if (columnNames != null && columnNames.Length > 0)
                            {
                                swCSV.WriteLine(string.Join(",", columnNames));
                            }

                            string[] RowValues = ds.Tables[0].Rows[0].ItemArray.Select(x => x.ToString().Replace(",", "")).ToArray();

                            if (RowValues != null && RowValues.Length > 0)
                            {
                                //Array.ConvertAll(RowValues, x=> x.Replace(",", ""));
                                swCSV.WriteLine(string.Join(",", RowValues));
                            }

                        }
                        catch (Exception ex1)
                        {
                            Utils.LogError(ex1);
                            sCsvFileName = "";
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateAgreegateFlatFiles() While creating CSV file. Error: " + ex1.Message, ErrorSeverityEnum.ExceptionRaised));

                        }
                        finally
                        {
                            if (swCSV != null)
                            {
                                swCSV.Close();
                            }
                        }

                    }
                }

            }
            catch (Exception exo)
            {
                Utils.LogError(exo);
                sTxtFilename = string.Empty;
                sCsvFileName = string.Empty;
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateAgreegateFlatFiles()  sTxtFilename = " + " Error: " + exo.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised + 1));
            }
            return oReturn;
        }
        private ResultReturn CreateRollOverFlatFiles(ref string sTxtFilename, ref string sCsvFileName)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            DataSet ds = null;
            sTxtFilename = "";
            sCsvFileName = "";
            string sError = "";
            string sMlFlatFilesLocalFolder = "";
            try
            {
                sMlFlatFilesLocalFolder = AppSettings.GetValue("MlFlatFilesLocalFolder");
                sMlFlatFilesLocalFolder = Path.Combine(sMlFlatFilesLocalFolder, DateTime.Now.ToString("MM") + DateTime.Now.ToString("yyyy"));
                sTxtFilename = sMlFlatFilesLocalFolder;
                sCsvFileName = sMlFlatFilesLocalFolder;

                string sRolloverFlatFileHeader = "UHDRGAP-TRA " + dtReportDate.ToString("yyyyMMdd");
                string sRolloverFlatFileTrailer = "UTRL3";

                StringBuilder sb = new();
                ds = _oMLScorecardDC.GetRolloverFileData(iTimeAllianceKey);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].Rows[0];
                    if (dr != null)
                    {
                        sb.Append(sRolloverFlatFileHeader);
                        sb.AppendLine();
                        //-----Section -  Data----------------------------------------
                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-1"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-1A"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-2"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-2A"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-3"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-3A"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-4"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-4A"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-5"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["NONBA-MKTVALROL-5A"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Rollover Field " + sError, ErrorSeverityEnum.Warning));
                        }

                        sb.AppendLine();
                        sb.Append(sRolloverFlatFileTrailer);

                        StreamWriter sw = null;
                        try
                        {

                            sTxtFilename = Path.Combine(sTxtFilename, "RolloverFile_" + strPrvMonthDate + "_GAPTRARI.txt");
                            Utils.ValidatePath(sTxtFilename);
                            sw = new StreamWriter(sTxtFilename);
                            sw.WriteLine(sb.ToString());
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex);
                            sTxtFilename = "";
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateRollOverFlatFiles() While creating TXT file. Error: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));

                        }
                        finally
                        {
                            if (sw != null)
                            {
                                sw.Close();
                            }
                        }

                        // now create CSV file
                        StreamWriter swCSV = null;
                        try
                        {
                            sCsvFileName = Path.Combine(sCsvFileName, "RolloverFile_" + strPrvMonthDate + "_GAPTRARI.CSV");
                            Utils.ValidatePath(sCsvFileName);
                            swCSV = new StreamWriter(sCsvFileName);

                            string[] columnNames = (from dc in ds.Tables[0].Columns.Cast<DataColumn>()
                                                    select dc.ColumnName).ToArray();

                            if (columnNames != null && columnNames.Length > 0)
                            {
                                swCSV.WriteLine(string.Join(",", columnNames));
                            }

                            string[] RowValues = ds.Tables[0].Rows[0].ItemArray.Select(x => x.ToString().Replace(",", "")).ToArray();
                            if (RowValues != null && RowValues.Length > 0)
                            {
                                //Array.ConvertAll(RowValues, x => x.Replace(",", ""));
                                swCSV.WriteLine(string.Join(",", RowValues));
                            }

                        }
                        catch (Exception ex1)
                        {
                            Utils.LogError(ex1);
                            sCsvFileName = "";
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateRollOverFlatFiles() While creating CSV file. Error: " + ex1.Message, ErrorSeverityEnum.ExceptionRaised));

                        }
                        finally
                        {
                            if (swCSV != null)
                            {
                                swCSV.Close();
                            }
                        }

                    }
                }

            }
            catch (Exception exo)
            {
                Utils.LogError(exo);
                sTxtFilename = string.Empty;
                sCsvFileName = string.Empty;
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateRollOverFlatFiles()  sTxtFilename = " + " Error: " + exo.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }
        private ResultReturn CreatePlanDetailFlatFiles(ref string sTxtFilename, ref string sCsvFileName)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            DataSet ds = null;
            sTxtFilename = "";
            sCsvFileName = "";
            string sError = "";
            string sMlFlatFilesLocalFolder = "";
            try
            {
                sMlFlatFilesLocalFolder = AppSettings.GetValue("MlFlatFilesLocalFolder");
                sMlFlatFilesLocalFolder = Path.Combine(sMlFlatFilesLocalFolder, DateTime.Now.ToString("MM") + DateTime.Now.ToString("yyyy"));
                sTxtFilename = sMlFlatFilesLocalFolder;
                sCsvFileName = sMlFlatFilesLocalFolder;

                string sPlanDetailFlatFileHeader = "UHDRGAP-TRA " + dtReportDate.ToString("yyyyMMdd");
                string sPlanDetailFlatFileTrailer = "UTRL";

                StringBuilder sb = new();
                ds = _oMLScorecardDC.GetPlanDetailFileData(iTimeAllianceKey);

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    sb.Append(sPlanDetailFlatFileHeader);
                    sb.AppendLine();
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        //-------------Section 9 - Conversion In Detail - plans live this month-------------
                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-PLNNM"], 150, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-BUSNM"], 150, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		151
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-MLACCT"], 8, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		301
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-PLTIN"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		309
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-CORPTIN"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		319
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-ASST"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		329
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-PART"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		342
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-PRIREC"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		355
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-PRICUST"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		359
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-PRIFA"], 25, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		363
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-CURRFA"], 25, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		388
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["CNV-EXCPTN"], 3, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		413
                        }

                        //-------------Section 9 - De-Conversion Detail - plans zeroed out this month-------------
                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-PLNNM"], 150, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		416
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-PLNNM1"], 150, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		566
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-MLACCT"], 8, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		716
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-PLTIN"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		724
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-CORPTIN"], 10, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		734
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-ASST"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		744
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-PART"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		757
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-SUCREC"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		770
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-SUCCUST"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		774
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-REAS-1"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		778
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-REAS-2"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		782
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-LOS"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		786
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-PRIFA"], 25, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		790
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["DECNV-CURRFA"], 25, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		815
                        }

                        //-------------Section 9 - At Risk Clients - plans identified this month-------------
                        sb.Append(CheckDBNullAndLengthStr(dr["RSK-PLNNM"], 150, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		840
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["RSK-MLACCT"], 8, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		990
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["RSK-ASST"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		998
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["RSK-PART"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1011
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["RSK-REAS-1"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1024
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["RSK-REAS-2"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1028
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["RSK-LOS"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1032
                        }

                        //-------------Section 9 - Stable Value Fund  (SVF) Processing Detail - for trading this month-------------
                        sb.Append(CheckDBNullAndLengthStr(dr["SVF-CUSIP"], 9, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1036
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVF-PLNNM"], 150, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1045
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVF-MLACCT"], 8, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1195
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVF-TXNTP"], 4, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1203
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVF-TD"], 9, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1207
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVF-ASST"], 13, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1216
                        }

                        sb.Append(CheckDBNullAndLengthStr(dr["SVF-TXVAL"], 12, ref sError)); if (!string.IsNullOrEmpty(sError))
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "PlanDetails Field " + sError, ErrorSeverityEnum.Warning)); // starts at		1229
                        }

                        sb.AppendLine();
                    }
                    sb.Append(sPlanDetailFlatFileTrailer);

                    StreamWriter sw = null;
                    try
                    {

                        sTxtFilename = Path.Combine(sTxtFilename, "PlanDetailFile_" + strPrvMonthDate + "_GAPTRADI.txt");
                        Utils.ValidatePath(sTxtFilename);
                        sw = new StreamWriter(sTxtFilename);
                        sw.WriteLine(sb.ToString());
                    }
                    catch (Exception ex)
                    {
                        Utils.LogError(ex);
                        sTxtFilename = "";
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.isException = true;
                        oReturn.confirmationNo = string.Empty;
                        oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreatePlanDetailFlatFiles() While creating TXT file. Error: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));

                    }
                    finally
                    {
                        if (sw != null)
                        {
                            sw.Close();
                        }
                    }

                    // now create CSV file
                    StreamWriter swCSV = null;
                    try
                    {
                        sCsvFileName = Path.Combine(sCsvFileName, "PlanDetailFile_" + strPrvMonthDate + "_GAPTRADI.CSV");
                        Utils.ValidatePath(sCsvFileName);
                        swCSV = new StreamWriter(sCsvFileName);

                        string[] columnNames = (from dc in ds.Tables[0].Columns.Cast<DataColumn>()
                                                select dc.ColumnName).ToArray();

                        if (columnNames != null && columnNames.Length > 0)
                        {
                            swCSV.WriteLine(string.Join(",", columnNames));
                        }

                        string[] RowValues = null;
                        foreach (DataRow drC in ds.Tables[0].Rows)
                        {
                            RowValues = null;
                            RowValues = drC.ItemArray.Select(x => x.ToString().Replace(",", "")).ToArray();

                            if (RowValues != null && RowValues.Length > 0)
                            {
                                //Array.ConvertAll(RowValues, x => x.Replace(",", ""));
                                swCSV.WriteLine(string.Join(",", RowValues));
                            }
                        }

                    }
                    catch (Exception ex1)
                    {
                        Utils.LogError(ex1);
                        sCsvFileName = "";
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.isException = true;
                        oReturn.confirmationNo = string.Empty;
                        oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreatePlanDetailFlatFiles() While creating CSV file. Error: " + ex1.Message, ErrorSeverityEnum.ExceptionRaised));

                    }
                    finally
                    {
                        if (swCSV != null)
                        {
                            swCSV.Close();
                        }
                    }


                }

            }
            catch (Exception exo)
            {
                Utils.LogError(exo);
                sTxtFilename = string.Empty;
                sCsvFileName = string.Empty;
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreatePlanDetailFlatFiles()  sTxtFilename = " + " Error: " + exo.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }
        private ResultReturn GetISCpptCountFromP3AndSaveInDB(string sInputContracts)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            string sError = "";
            string sReponse = "";
            int iError = 0;
            string contract_id = "";
            string sub_id = "";
            int iPptWBalanceCount = 0;
            string sInfo = "";
            int iIndexofSpace = 0;
            Hashtable htDistContracts = new();
            try
            {
                if (sInputContracts != null && sInputContracts != string.Empty)
                {
                    if (sInputContracts.EndsWith(","))
                    {
                        sInputContracts = sInputContracts.Remove(sInputContracts.Length - 1);
                    }

                    sReponse = _oMLScorecardDC.GetPptWithBalanceCount(sInputContracts, DateTime.Today);

                    XElement xEl = XElement.Parse(sReponse); // format: <Cases><Case><Id>300019    00000</Id><PptCountWbal>510</PptCountWbal></Case><Case><Id>300050    00000</Id><PptCountWbal>222</PptCountWbal></Case><Case><Id>300069    00000</Id><PptCountWbal>39</PptCountWbal></Case></Cases>
                    iError = CheckP3Error(xEl, ref sError);

                    if (iError != 0)
                    {
                        //failed
                        throw new Exception("Error in GetPptWithBalanceCount: " + sError);
                    }
                    else
                    {
                        foreach (XElement xElrow in xEl.Descendants("Case").ToList())
                        {
                            try
                            {
                                contract_id = ""; sub_id = ""; iPptWBalanceCount = 0; sInfo = ""; iIndexofSpace = 0;

                                if (xElrow.Element("Id") != null && xElrow.Element("Id").Value.Trim() != string.Empty)
                                {
                                    contract_id = xElrow.Element("Id").Value.Trim();
                                    iIndexofSpace = contract_id.IndexOf(" ");
                                    //MUST get SUB_id first
                                    sub_id = Utils.SubIn(contract_id.Substring(contract_id.Length - 5)); // assumption: contract_id.Length is always greater than 5
                                    contract_id = contract_id.Substring(0, iIndexofSpace);


                                    if (xElrow.Element("PptCountWbal") != null)
                                    {
                                        iPptWBalanceCount = Convert.ToInt32(xElrow.Element("PptCountWbal").Value);
                                    }
                                    sInfo = "Contract: " + contract_id + "-" + sub_id + " iPptWBalanceCount: " + iPptWBalanceCount;

                                    if (htDistContracts.ContainsKey(contract_id) == false)
                                    {
                                        htDistContracts.Add(contract_id, iPptWBalanceCount);
                                    }
                                    else
                                    {
                                        htDistContracts[contract_id] = (int)htDistContracts[contract_id] + iPptWBalanceCount; // sum up all sub_id ppts
                                    }
                                }
                            }
                            catch (Exception exi)
                            {
                                Utils.LogError(exi);
                                oReturn.returnStatus = ReturnStatusEnum.Failed;
                                oReturn.isException = true;
                                oReturn.Errors.Add(new ErrorInfo(-1, "Error in GetISCpptCountFromP3AndSaveInDB:   " + sInfo + "Error: " + exi.Message + "\r\n", ErrorSeverityEnum.Failed));
                            }

                        }

                        if (htDistContracts.Count <= 0)
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.Errors.Add(new ErrorInfo(-1, "GetISCpptCountFromP3AndSaveInDB: No Data found for:   " + sInputContracts + "\r\n", ErrorSeverityEnum.Failed));
                        }

                        ////-----------------2. Save ppt with balance count in wn_part_hdr table -------------------------------------------------------------------
                        foreach (DictionaryEntry ct in htDistContracts)
                        {
                            try
                            {
                                contract_id = ""; iPptWBalanceCount = 0; sub_id = "000"; // always 000
                                contract_id = ct.Key.ToString();
                                iPptWBalanceCount = Convert.ToInt32(ct.Value);
                                sInfo = "Contract: " + contract_id + "-" + sub_id + " iPptWBalanceCount: " + iPptWBalanceCount;
                                // as of now total_employees  and num_elig_parts are 0
                                // for ISC iPptWBalanceCount is same for total_ees_partic and gt0_bal_parts
                                _oMLScorecardDC.Insert_wn_part_hdr_Data(contract_id, sub_id, "ISC", 0, iPptWBalanceCount, 0, iPptWBalanceCount);

                            }
                            catch (Exception exi2)
                            {
                                Utils.LogError(exi2);
                                oReturn.returnStatus = ReturnStatusEnum.Failed;
                                oReturn.isException = true;
                                oReturn.Errors.Add(new ErrorInfo(-1, "Error in GetISCpptCountFromP3AndSaveInDB while calling Insert_wn_part_hdr_Data:   " + sInfo + "Error: " + exi2.Message + "\r\n", ErrorSeverityEnum.Failed));
                            }
                        }
                    }
                }
            }
            catch (Exception ex1)
            {
                Utils.LogError(ex1);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in GetISCpptCountFromP3AndSaveInDB(). Error: " + ex1.Message, ErrorSeverityEnum.ExceptionRaised));

            }
            finally
            {

            }
            return oReturn;
        }
        private string CheckDBNullAndLengthStr(object a_oVal, int length, ref string sError)
        {
            sError = string.Empty;
            string sOut = "";

            if (a_oVal != DBNull.Value)
            {
                sOut = a_oVal.ToString();
                sOut = sOut.Trim();
                if (sOut.Length > length)
                {
                    sError = "Truncation Error: Expected Length of Data is " + length.ToString() + ", actual Length of data is " + sOut.Length.ToString() + ". Original data: " + sOut;
                    sOut = sOut.Substring(0, length);
                    sError += "Truncted value printed in file is " + sOut;
                }
                //sOut = sOut.PadRight(length + 1);
            }
            //sOut = sOut.Replace(",", ""); // remove comma because we are printing these values in .csv file

            sOut = sOut.PadRight(length);

            return sOut;

        }
        private int CheckP3Error(XElement xEl, ref string sError)
        {
            int iErrorNo = 0;
            sError = string.Empty;
            try
            {
                IEnumerable<XElement> chkErr = from err in xEl.Descendants("Errors") select err;

                if ((chkErr != null))
                {
                    foreach (XElement err in chkErr)
                    {
                        if ((err.Element("Error").Value != null) && (err.Element("Error").Element("Number").Value != null) && Convert.ToInt32(err.Element("Error").Element("Number").Value) != 0)
                        {
                            // error
                            iErrorNo = Convert.ToInt32(err.Element("Error").Element("Number").Value);
                            sError = sError + " | " + err.Element("Error").Element("Description").Value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                iErrorNo = 9999;
                sError = "Exception in CheckP3Error() ex: " + ex.Message;
            }
            return iErrorNo;
        }

        #endregion
    }
}
