using System.Data;
using System.Text;
using System.Xml.Linq;
using Aspose.Words;
using ClosedXML.Excel;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.DriverSOA;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using License = Aspose.Words.License;
using SOAModel = TRS.IT.SOA.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
using wsMS = TRS.IT.SI.Services.wsNotification;

namespace TRS.IT.BendProcessor.BLL
{
    public class RMDMigrated : BendProcessorBase
    {
        public RMDMigrated() : base("76", "RMD", "TRS") { }
        public TaskStatus ProcessRMDLettersMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn1 = new();
            ResultReturn oReturn2 = new();
            ResultReturn oReturn3 = new();
            StringBuilder sbErr = new();
            const string C_Task = "ProcessRMDLetters";
            try
            {

                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (TrsAppSettings.AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    string sRMDLettersTaskEnabledMonths = TrsAppSettings.AppSettings.GetValue("RMDLettersTaskEnabledMonths");
                    if (sRMDLettersTaskEnabledMonths == null || sRMDLettersTaskEnabledMonths == string.Empty)
                    {
                        sRMDLettersTaskEnabledMonths = "10";
                    }
                    int iCurrentMonth = DateTime.Now.Month;
                    if (sRMDLettersTaskEnabledMonths == iCurrentMonth.ToString())
                    {
                        oReturn1 = ProcessRMDLettersDataFromFile();
                    }

                    if (oReturn1.returnStatus != ReturnStatusEnum.Succeeded || oReturn1.Errors.Count > 0)
                    {
                        sbErr.Length = 0;
                        //send error
                        foreach (ErrorInfo oEr in oReturn1.Errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("ProcessRMDLetters Status - " + oReturn1.returnStatus.ToString(), sbErr.ToString(), oTaskReturn.taskName);

                        General.CopyResultError(oTaskReturn, oReturn1);
                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
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

        public ResultReturn ProcessRMDLettersDataFromFile()
        {
            ResultReturn oReturn = new();
            ResultReturn oRetFiles = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            DataSet ds = new();

            try
            {
                string sRMDDataFilePath = "";
                //string sRMDDataFileName = "";
                sRMDDataFilePath = TrsAppSettings.AppSettings.GetValue("RMDDataFilePath");

                string[] Files = Directory.GetFiles(sRMDDataFilePath);
                foreach (string sRMDDataFileName in Files)
                {
                    try
                    {
                        FileInfo file = new(sRMDDataFileName);

                        ds = LoadRMDLettersDataFromFile(file);

                        if (ds != null && ds.Tables.Count > 1)
                        {
                            oRetFiles = GenerateRMDDataFiles(ds);
                            if (oRetFiles.returnStatus != ReturnStatusEnum.Succeeded)
                            {
                                General.CopyResultError(oReturn, oRetFiles);
                            }

                            string sFileMoveToPath = TrsAppSettings.AppSettings.GetValue("RMDDataFileArchivePath");
                            if (string.IsNullOrEmpty(sFileMoveToPath))
                            {
                                sFileMoveToPath = sFileMoveToPath = Path.Combine(file.DirectoryName, "Complete", Path.GetFileNameWithoutExtension(file.Name) + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + file.Extension);
                            }
                            else
                            {
                                sFileMoveToPath = Path.Combine(sFileMoveToPath, Path.GetFileNameWithoutExtension(file.Name) + "_" + DateTime.Now.ToString("yyyyMMdd_hhmmss") + file.Extension);
                            }

                            Utils.ValidatePath(sFileMoveToPath);
                            file.MoveTo(sFileMoveToPath);

                        }
                        else
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Exception in ProcessRMDLettersDataFromFile  -  File Name : " + sRMDDataFileName + "Error:  Invalid File. <BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
                        }
                    }
                    catch (Exception exi)
                    {
                        Utils.LogError(exi);
                        oReturn.Errors.Add(new ErrorInfo(-1, "Exception in ProcessRMDLettersDataFromFile  -  File Name : " + sRMDDataFileName + "Error: " + exi.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
                    }
                }

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in ProcessRMDLettersDataFromFile  - Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }


            return oReturn;
        }

        public DataSet LoadRMDLettersDataFromFile(FileInfo file)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            DataSet ds = new();

            try
            {
                if (file == null || !file.Exists)
                {
                    Utils.LogError(new Exception("File is null or does not exist"));
                    return ds;
                }

                XLWorkbook workbook = null;
                string extension = Path.GetExtension(file.Name).ToLower();

                switch (extension)
                {
                    case ".csv":
                        // For CSV files, we'll read them as text and create a simple workbook
                        workbook = LoadCsvAsWorkbook(file.FullName);
                        break;
                    case ".xls":
                    case ".xlsx":
                        workbook = new XLWorkbook(file.FullName);
                        break;
                    default:
                        Utils.LogError(new Exception("Unsupported file extension: " + extension));
                        return ds;
                }

                using (workbook)
                {
                    DataTable dtRMDData = GetRMDDatatable(workbook);
                    ds.Tables.Add(dtRMDData);

                    DataTable dtRMDVariables = GetRMDVariablestable(workbook);
                    ds.Tables.Add(dtRMDVariables);
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in LoadRMDLettersDataFromFile - File: " + (file != null ? file.Name : "null") + " Error: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return ds;
        }
        private XLWorkbook LoadCsvAsWorkbook(string filePath)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            using (var reader = new StreamReader(filePath))
            {
                int row = 1;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(',');
                    for (int col = 0; col < values.Length; col++)
                    {
                        worksheet.Cell(row, col + 1).Value = values[col].Trim('"');
                    }
                    row++;
                }
            }

            return workbook;
        }
        private static DataTable GetRMDDatatable(XLWorkbook workbook)
        {
            DataTable dt = new("RMDData");
            dt.Columns.Add("Contract", typeof(string));
            dt.Columns.Add("Affiliate", typeof(string));
            dt.Columns.Add("SSN", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("DOB", typeof(string));
            dt.Columns.Add("Age", typeof(string));
            dt.Columns.Add("Divisor", typeof(string));
            dt.Columns.Add("DOT", typeof(string));
            dt.Columns.Add("5PctOwner", typeof(string));
            dt.Columns.Add("AccountBalance", typeof(string));
            dt.Columns.Add("ConversionBalance", typeof(string));
            dt.Columns.Add("RMD", typeof(string));
            dt.Columns.Add("CurrentYearDistributions", typeof(string));
            dt.Columns.Add("RMDPaid", typeof(string));
            dt.Columns.Add("RMDPay", typeof(string));
            dt.Columns.Add("RMDDoNotPay", typeof(string));

            // Try to get worksheet by name first, then by index
            IXLWorksheet ws = null;
            if (workbook.Worksheets.Contains("full listing"))
            {
                ws = workbook.Worksheet("full listing");
            }
            else if (workbook.Worksheets.Count > 2)
            {
                ws = workbook.Worksheets.Worksheet(3); // 1-based index in ClosedXML
            }
            else
            {
                // Fallback to first worksheet
                ws = workbook.Worksheets.First();
            }

            if (ws == null)
            {
                Utils.LogError(new Exception("Could not find the required worksheet"));
                return dt;
            }

            // Get the used range, starting from row 2 (skip header)
            var usedRange = ws.RangeUsed();
            if (usedRange == null)
            {
                return dt;
            }

            int startRow = 2; // Skip header row
            int lastRow = usedRange.LastRow().RowNumber();
            int lastCol = Math.Min(usedRange.LastColumn().ColumnNumber(), dt.Columns.Count);

            for (int row = startRow; row <= lastRow; row++)
            {
                // Check if row is empty
                bool isEmpty = true;
                for (int col = 1; col <= lastCol; col++)
                {
                    if (!ws.Cell(row, col).IsEmpty())
                    {
                        isEmpty = false;
                        break;
                    }
                }

                if (isEmpty)
                {
                    continue;
                }

                DataRow dr = dt.NewRow();

                for (int col = 1; col <= lastCol; col++)
                {
                    var cell = ws.Cell(row, col);
                    string columnName = dt.Columns[col - 1].ColumnName;

                    try
                    {
                        if (cell.IsEmpty())
                        {
                            dr[columnName] = string.Empty;
                        }
                        else
                        {
                            // Handle different data types
                            if (cell.DataType == XLDataType.DateTime)
                            {
                                dr[columnName] = cell.GetDateTime().ToString("MM/dd/yyyy");
                            }
                            else if (cell.DataType == XLDataType.Number)
                            {
                                dr[columnName] = cell.GetDouble().ToString();
                            }
                            else
                            {
                                dr[columnName] = cell.GetText();
                            }
                        }
                    }
                    catch
                    {
                        // If there's any issue getting the value, use the text representation
                        dr[columnName] = cell.GetText();
                    }
                }

                dt.Rows.Add(dr);
            }

            // Process the data rows for formatting
            DateTime dtTemp;
            double iTemp = 0;
            decimal dcTemp = 0;
            bool bTemp = false;

            foreach (DataRow dr in dt.Rows)
            {
                // Process DOB
                iTemp = 0;
                if (dr["DOB"] != null && !string.IsNullOrEmpty(dr["DOB"].ToString()))
                {
                    if (double.TryParse(dr["DOB"].ToString(), out iTemp) == true)
                    {
                        dr["DOB"] = TranslateDateFromExcel(iTemp);
                    }
                    else if (DateTime.TryParse(dr["DOB"].ToString(), out dtTemp) == true)
                    {
                        dr["DOB"] = dtTemp.ToString("MM/dd/yyyy");
                    }
                }

                // Process DOT
                iTemp = 0;
                if (dr["DOT"] != null && !string.IsNullOrEmpty(dr["DOT"].ToString()))
                {
                    if (double.TryParse(dr["DOT"].ToString(), out iTemp) == true)
                    {
                        dr["DOT"] = TranslateDateFromExcel(iTemp);
                    }
                    else if (DateTime.TryParse(dr["DOT"].ToString(), out dtTemp) == true)
                    {
                        dr["DOT"] = dtTemp.ToString("MM/dd/yyyy");
                    }
                }

                // Process currency fields
                dcTemp = 0;
                if (dr["AccountBalance"] != null)
                {
                    bTemp = decimal.TryParse(dr["AccountBalance"].ToString(), out dcTemp);
                }

                dr["AccountBalance"] = string.Format("{0:C}", dcTemp);

                dcTemp = 0;
                if (dr["ConversionBalance"] != null)
                {
                    bTemp = decimal.TryParse(dr["ConversionBalance"].ToString(), out dcTemp);
                }

                dr["ConversionBalance"] = string.Format("{0:C}", dcTemp);

                dcTemp = 0;
                if (dr["RMD"] != null)
                {
                    bTemp = decimal.TryParse(dr["RMD"].ToString(), out dcTemp);
                }

                dr["RMD"] = string.Format("{0:C}", dcTemp);

                dcTemp = 0;
                if (dr["CurrentYearDistributions"] != null)
                {
                    bTemp = decimal.TryParse(dr["CurrentYearDistributions"].ToString(), out dcTemp);
                }

                dr["CurrentYearDistributions"] = string.Format("{0:C}", dcTemp);
            }

            return dt;
        }
        private static DataTable GetRMDVariablestable(XLWorkbook workbook)
        {
            // Try to get worksheet by name first, then by index
            IXLWorksheet ws = null;
            if (workbook.Worksheets.Contains("dates"))
            {
                ws = workbook.Worksheet("dates");
            }
            else if (workbook.Worksheets.Count > 3)
            {
                ws = workbook.Worksheets.Worksheet(4); // 1-based index in ClosedXML
            }
            else
            {
                // Fallback to last worksheet
                ws = workbook.Worksheets.Last();
            }

            if (ws == null)
            {
                Utils.LogError(new Exception("Could not find the dates worksheet"));
                return new DataTable("RMDVariables");
            }

            DataTable dt = new("RMDVariables");
            dt.Columns.Add("RmdYr", typeof(string));
            dt.Columns.Add("ExcpDt", typeof(string));
            dt.Columns.Add("CurYrEndDt", typeof(string));
            dt.Columns.Add("RespByDt", typeof(string));
            dt.Columns.Add("PrvYrEndDt", typeof(string));
            dt.Columns.Add("PrcByDt", typeof(string));
            dt.Columns.Add("PrvYr", typeof(string));
            dt.Columns.Add("ProcessDate1", typeof(string));

            // Get the used range, starting from row 2 (skip header)
            var usedRange = ws.RangeUsed();
            if (usedRange == null)
            {
                return dt;
            }

            int startRow = 2; // Skip header row
            int lastRow = usedRange.LastRow().RowNumber();
            int lastCol = Math.Min(usedRange.LastColumn().ColumnNumber(), dt.Columns.Count);

            for (int row = startRow; row <= lastRow; row++)
            {
                // Check if row is empty (stop at first empty row)
                bool isEmpty = true;
                for (int col = 1; col <= lastCol; col++)
                {
                    if (!ws.Cell(row, col).IsEmpty())
                    {
                        isEmpty = false;
                        break;
                    }
                }

                if (isEmpty)
                {
                    break; // Stop at first empty row
                }

                DataRow dr = dt.NewRow();

                for (int col = 1; col <= lastCol; col++)
                {
                    var cell = ws.Cell(row, col);
                    string columnName = dt.Columns[col - 1].ColumnName;

                    try
                    {
                        if (cell.IsEmpty())
                        {
                            dr[columnName] = string.Empty;
                        }
                        else
                        {
                            // Handle different data types
                            if (cell.DataType == XLDataType.DateTime)
                            {
                                dr[columnName] = cell.GetDateTime().ToString("MMMM dd, yyyy");
                            }
                            else if (cell.DataType == XLDataType.Number)
                            {
                                dr[columnName] = cell.GetDouble().ToString();
                            }
                            else
                            {
                                dr[columnName] = cell.GetText();
                            }
                        }
                    }
                    catch
                    {
                        // If there's any issue getting the value, use the text representation
                        dr[columnName] = cell.GetText();
                    }
                }

                dt.Rows.Add(dr);
            }

            // Process the data rows for date formatting
            DateTime dtTemp;
            double iTemp = 0;

            foreach (DataRow dr in dt.Rows)
            {
                // Process all date fields
                string[] dateFields = ["ExcpDt", "CurYrEndDt", "RespByDt", "PrvYrEndDt", "PrcByDt"];

                foreach (string field in dateFields)
                {
                    iTemp = 0;
                    if (dr[field] != null && !string.IsNullOrEmpty(dr[field].ToString()))
                    {
                        if (double.TryParse(dr[field].ToString(), out iTemp) == true)
                        {
                            dr[field] = TranslateDateFromExcel(iTemp);
                        }
                        else if (DateTime.TryParse(dr[field].ToString(), out dtTemp) == true)
                        {
                            dr[field] = dtTemp.ToString("MMMM dd, yyyy");
                        }
                    }
                }

                dr["ProcessDate1"] = DateTime.Now.ToString("MMMM dd, yyyy");
            }

            return dt;
        }

        private static string TranslateDateFromExcel(double iTemp)
        {
            string sReturn = "";

            if (iTemp < 61)
            {
                sReturn = DateTime.FromOADate(iTemp).AddDays(1).ToString("MM/dd/yyyy");
            }
            else
            {
                sReturn = DateTime.FromOADate(iTemp).ToString("MM/dd/yyyy");
            }

            return sReturn;
        }

        private ResultReturn GenerateRMDDataFiles(DataSet ds)
        {
            // creates 1 data file for each contract
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            ResultReturn oRetDataFile = new();
            ResultReturn oRetSpLtrFile = new();
            ResultReturn oRetTPAComm = new();
            ResultReturn oRetTPACvrLtrFile = new();
            ResultReturn oRetTPALtrFile = new();

            string contract_id = "";
            string sub_id = "";
            DataTable dtRMDData = null;
            DataTable dtRMDVariables = null;
            DataView dvRMDData = null; //dtRMDData.DefaultView;

            ConsolidatedNotifications oConsObj = new();
            SOAModel.ContractInfo oConInfo = null;
            DocumentService oDocSrv = new();

            try
            {
                License license = new();
                license.SetLicense("Aspose.Total.lic");

                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        dtRMDData = ds.Tables[0];
                    }

                    if (ds.Tables.Count > 1)
                    {
                        dtRMDVariables = ds.Tables[1];
                    }

                    if (dtRMDData != null && dtRMDData.Rows.Count > 0)
                    {
                        dvRMDData = dtRMDData.DefaultView;

                        var distinctContracts = dtRMDData.AsEnumerable()
                            .Select(s => new
                            {
                                Contractid = s.Field<string>("Contract"),
                            })
                            .Distinct().ToList();

                        if (distinctContracts != null)
                        {
                            string sRMDTemplatePath = Path.Combine(TrsAppSettings.AppSettings.GetValue("RMDTemplatePath"));
                            string docRMDDataTemplatePath = Path.Combine(sRMDTemplatePath, "RMD_Template.docx");

                            Document docRMDDataTemplate = new(docRMDDataTemplatePath);
                            Document docRMDSponsorLetterTemplate = new(Path.Combine(sRMDTemplatePath, "RMD_SponsorTemplate.docx"));
                            Document docRMDTPALetterTemplate = new(Path.Combine(sRMDTemplatePath, "RMD_TPATemplate.docx"));

                            DataTable dtTPACommunication = CreateTPACommunicationTable();

                            foreach (var contract in distinctContracts)
                            {
                                try
                                {
                                    contract_id = contract.Contractid;
                                    dvRMDData.RowFilter = "Contract = '" + contract.Contractid + "'";
                                    oRetDataFile = CreateRMDDataFile(contract.Contractid, dvRMDData, dtRMDVariables, docRMDDataTemplate);

                                    if (oRetDataFile.returnStatus == ReturnStatusEnum.Succeeded)
                                    {
                                        oConInfo = oConsObj.GetContractInfoFromSRV(contract.Contractid.Trim(), "000");
                                        //Create Sponsor Letter
                                        oRetSpLtrFile = CreateSponsorRMDLetter(oConInfo, dtRMDVariables, docRMDSponsorLetterTemplate, oRetDataFile.confirmationNo);
                                        if (oRetSpLtrFile.returnStatus != ReturnStatusEnum.Succeeded)
                                        {
                                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                                            General.CopyResultError(oReturn, oRetSpLtrFile);
                                        }

                                        oRetTPAComm = PopulateTPACommunicationInfo(oConInfo, oRetDataFile.confirmationNo, ref dtTPACommunication);
                                        if (oRetTPAComm.returnStatus != ReturnStatusEnum.Succeeded)
                                        {
                                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                                            General.CopyResultError(oReturn, oRetTPAComm);
                                        }
                                    }
                                    else
                                    {
                                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                                        General.CopyResultError(oReturn, oRetDataFile);
                                    }
                                }
                                catch (Exception exi)
                                {
                                    Utils.LogError(exi);
                                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                                    oReturn.isException = true;
                                    oReturn.confirmationNo = string.Empty;
                                    oReturn.Errors.Add(new ErrorInfo(-1, "Exception in GenerateRMDDataFile.FilesGeneration  - contract_id = " + contract_id + " sub_id = " + sub_id + " Error: " + exi.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
                                }
                            }

                            //Create TPa Messages one for each TPA contact (consolidated contracts)

                            if (dtTPACommunication.Rows.Count > 0)
                            {
                                Document docTPACoverLettrDoc = new(Path.Combine(TrsAppSettings.AppSettings.GetValue("RMDTemplatePath"), "RMD_TPATemplate.docx"));
                                oRetTPACvrLtrFile = CreateTPARMDCoverLetter(dtRMDVariables, ref docTPACoverLettrDoc);
                                if (oRetTPACvrLtrFile.returnStatus != ReturnStatusEnum.Succeeded)
                                {
                                    General.CopyResultError(oReturn, oRetTPACvrLtrFile);
                                }

                                oRetTPALtrFile = CreateTPARMDFinalLetter(dtTPACommunication, docTPACoverLettrDoc);

                            }
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
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in GenerateRMDDataFile  - contract_id = " + contract_id + " sub_id = " + sub_id + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        private ResultReturn CreateRMDDataFile(string contract_id, DataView dvRMDData, DataTable dtRMDVariables, Document docRMDDataTemplate)
        {
            // creates 1 data file for each contract
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            string sub_id = "00000";
            string sFileName = "";
            try
            {
                Document dstDataDoc = (Document)docRMDDataTemplate.Clone(true);

                dstDataDoc.MailMerge.Execute(dtRMDVariables);
                dstDataDoc.MailMerge.ExecuteWithRegions(dvRMDData);

                // Save the document.
                sFileName = GetRMDFileName(contract_id, sub_id, "0", 3, ".docx");
                dstDataDoc.Save(sFileName);

                oReturn.confirmationNo = sFileName;

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateRMDDataFile  - contract_id = " + contract_id + " sub_id = " + sub_id + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        private string GetRMDFileName(string cid, string sid, string sIn_login_id, int doctype, string ext)
        {
            //doctype = 1 -> Sponsor CoverLetter
            //doctype = 2 -> TPA CoverLetter
            //doctype = 3 -> RMD Data
            //doctype = 4 -> TPA Message attachment ( cid and sid are empty for this type)
            string strFileName = "";
            string sPath;
            DateTime dtToday = DateTime.Now;
            sPath = TrsAppSettings.AppSettings.GetValue("RMDOutputFolderPath");

            if (!ext.StartsWith("."))
            {
                ext = "." + ext;
            }
            strFileName = Path.Combine(sPath, dtToday.Year.ToString(), cid + "_" + sid + "_" + sIn_login_id + "_" + dtToday.ToString("yyyyMMdd_hhmmss") + "_" + doctype.ToString() + ext);
            Utils.ValidatePath(strFileName);

            return strFileName;

        }

        private ResultReturn CreateSponsorRMDLetter(SOAModel.ContractInfo oConInfo, DataTable dtRMDVariables, Document docRMDSponsorLetterTemplate, string sRMDDataFileName)
        {
            // creates 1 data file for each contract
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            string contract_id = "";
            string sub_id = "000";
            string sFileName = "";
            string sIn_login_id = "";
            string sContactName = "";
            List<SOAModel.PlanContactInfo> lstPlanContacts = null;
            ConsolidatedNotifications oConsObj = new();
            string sEmailIds = "";
            string sMsgCenterIds = "";
            try
            {
                contract_id = oConInfo.ContractID;
                Document dstSPDoc = (Document)docRMDSponsorLetterTemplate.Clone(true);

                if (dstSPDoc == null)
                {
                    dstSPDoc = new Document(Path.Combine(TrsAppSettings.AppSettings.GetValue("RMDTemplatePath"), "RMD_SponsorTemplate.docx"));
                }

                lstPlanContacts = oConsObj.GetContactsByContactType(oConInfo, TRS.IT.SI.BusinessFacadeLayer.Model.E_ContactType.PrimaryContact);

                if (lstPlanContacts.Count == 0)
                {
                    throw new Exception("Primary Contact information not found");
                }
                if (docRMDSponsorLetterTemplate == null)
                {
                    throw new Exception("RMDSponsorLetterTemplate not created");
                }

                sEmailIds = ""; sMsgCenterIds = "";
                foreach (SOAModel.PlanContactInfo oCt in lstPlanContacts)
                {
                    sContactName = (string.IsNullOrEmpty(sContactName)) ? oCt.FirstName : "";
                    sEmailIds = (string.IsNullOrEmpty(sEmailIds)) ? oCt.Email : sEmailIds + ";" + oCt.Email;
                    sMsgCenterIds = (string.IsNullOrEmpty(sMsgCenterIds)) ? oCt.WebInLoginID + "|" + GetFullName(oCt.FirstName, oCt.LastName) : sMsgCenterIds + ";" + oCt.WebInLoginID + "|" + GetFullName(oCt.FirstName, oCt.LastName);
                }

                sContactName = (string.IsNullOrEmpty(sContactName)) ? "Sir/Madam" : sContactName;

                //----------------------------------------------------------------------------

                dstSPDoc.MailMerge.Execute(dtRMDVariables); // instead of bookmarks populate merge fields


                DocumentBuilder builder = new(dstSPDoc);

                builder.MoveToBookmark("PlanName1", false, true);
                builder.Write(oConInfo.PlanName);

                builder.MoveToBookmark("AccountNumber1", false, true);
                builder.Write(oConInfo.ContractID + "-" + oConInfo.SubID);

                builder.MoveToBookmark("ContactName1", false, true);
                builder.Write(sContactName);

                // Merge Coverletter and DataFile
                Document docTemp = new(sRMDDataFileName);

                dstSPDoc.AppendDocument(docTemp, ImportFormatMode.KeepSourceFormatting);

                sIn_login_id = lstPlanContacts[0].WebInLoginID;
                // Save the document.
                sFileName = GetRMDFileName(contract_id, sub_id, sIn_login_id, 1, ".Pdf");
                dstSPDoc.Save(sFileName, Aspose.Words.SaveFormat.Pdf);

                oReturn.confirmationNo = sFileName;


                DocumentService oDocSrv = new();
                string sError = "";
                sError = oDocSrv.ImageDocument(contract_id, sub_id, 353, sFileName, 0, "Backend Process - RMD");
                if (sError != string.Empty)
                {
                    throw new Exception("Error in ImageDocument SponsorRMD - contract_id = " + contract_id + " sub_id = " + sub_id + " Error: " + sError + "<BR />" + Environment.NewLine);
                }
                // Send Sponsor Notification


                ResultReturn oRetNotification = SendEmailNotification(3400, contract_id, sub_id, oConInfo.PlanName, sEmailIds, sMsgCenterIds, ""); // no attachment for sponsor
                if (oRetNotification.returnStatus != ReturnStatusEnum.Succeeded)
                {
                    General.CopyResultError(oReturn, oRetNotification);
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateRMDDataFile  - contract_id = " + contract_id + " sub_id = " + sub_id + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        private string GetFullName(string sFirstName, string sLastName)
        {
            string sName = "";

            if (string.IsNullOrEmpty(sFirstName))
            {
                sFirstName = "";
            }

            if (string.IsNullOrEmpty(sLastName))
            {
                sFirstName = "";
            }

            sName = sFirstName.Trim() + " " + sLastName.Trim();

            if (sName.Trim() == "")
            {
                sName = "Name not found";
            }

            return sName;
        }

        private ResultReturn CreateTPARMDCoverLetter(DataTable dtRMDVariables, ref Document dstTPACoverLettrDoc)
        {
            // creates 1 data file for each contract
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            string contract_id = "";
            string sub_id = "000";
            string sFileName = "";

            ConsolidatedNotifications oConsObj = new();

            try
            {
                if (dstTPACoverLettrDoc == null)
                {
                    dstTPACoverLettrDoc = new Document(Path.Combine(TrsAppSettings.AppSettings.GetValue("RMDTemplatePath"), "RMD_TPATemplate.docx"));
                }

                dstTPACoverLettrDoc.MailMerge.Execute(dtRMDVariables); // instead of bookmarks populate merge fields

                DocumentBuilder builder = new(dstTPACoverLettrDoc);
                sFileName = GetRMDFileName("TPA", "RMD", "Letter_" + dtRMDVariables.Rows[0]["RmdYr"].ToString(), 2, ".doc");

                dstTPACoverLettrDoc.Save(sFileName);

                oReturn.confirmationNo = sFileName;

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateTPARMDLetter  - contract_id = " + contract_id + " sub_id = " + sub_id + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }

        private ResultReturn CreateTPARMDFinalLetter(DataTable dtTPACommunication, Document docTPACoverLettrDoc)
        {
            // creates 1 data file for each contract
            ResultReturn oReturn = new();
            ResultReturn oRetNotification = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            string FromAddress = "auto-service@transamerica.com";

            string contract_id = "";
            string sub_id = "000";

            string sTPAName = "";
            string sTPAInloginId = "";
            string sTPAEmail = "";
            string sLoginIdXML = "";                                //<ArrayOfInLoginId> <InLoginId>" + sinloginID "</InLoginId></ArrayOfInLoginId>
            XElement elArrayOfInLoginId;
            XElement childElement;
            StringBuilder sb = new();
            ConsolidatedNotifications oConsObj = new();
            GeneralDC oGenDC = new();
            int iBendInLoginId = 0;
            Document dstTPACoverLettrDoc;
            try
            {
                if (docTPACoverLettrDoc == null)
                {
                    docTPACoverLettrDoc = new Document();// should we generate TPA messages if coverletter is not generated ?
                }

                string sTPAFileName = "";
                DataView dvTpaComm = dtTPACommunication.DefaultView;
                if (dtTPACommunication.Rows.Count > 0)
                {
                    iBendInLoginId = oGenDC.GetMsgCtrAcctByExLoginId("BendProcess");

                    dvTpaComm = dtTPACommunication.DefaultView;
                    var distinctTPAContacts = dtTPACommunication.AsEnumerable()
                        .Select(s => new
                        {
                            in_login_id = s.Field<string>("in_login_id"),
                        })
                        .Distinct().ToList();

                    if (distinctTPAContacts != null)
                    {
                        foreach (var contact in distinctTPAContacts)
                        {
                            sb.Clear(); sTPAInloginId = ""; sTPAName = ""; sTPAEmail = ""; sTPAFileName = ""; sLoginIdXML = "";//reset
                            try
                            {
                                dstTPACoverLettrDoc = (Document)docTPACoverLettrDoc.Clone(true);
                                sTPAInloginId = contact.in_login_id;
                                dvTpaComm.RowFilter = "in_login_id = '" + sTPAInloginId + "'";

                                sb.Append("<table cellpadding=\"5\" cellspacing=\"5\"><tr style=\"font-weight:bold;\"><td>ContractId</td><td>Plan Name</td></tr>");
                                foreach (DataRowView drvTPaCt in dvTpaComm)
                                {
                                    if (string.IsNullOrEmpty(sTPAName))
                                    {
                                        sTPAName = drvTPaCt["Name"].ToString();
                                    }

                                    if (string.IsNullOrEmpty(sTPAEmail))
                                    {
                                        sTPAEmail = drvTPaCt["email_id"].ToString();
                                    }

                                    dstTPACoverLettrDoc.AppendDocument(new Document(drvTPaCt["RMDDataFile"].ToString()), ImportFormatMode.KeepSourceFormatting);
                                    sb.Append("<tr>");
                                    sb.AppendFormat("<td>{0}-{1}</td>", drvTPaCt["contract_Id"].ToString(), "00000");

                                    sb.AppendFormat("<td>{0}</td>", drvTPaCt["PlanName"].ToString());
                                    sb.AppendFormat("</tr>");

                                }
                                sb.Append("</table>");

                                sTPAFileName = GetRMDFileName("", "", sTPAInloginId, 4, "PDF");
                                dstTPACoverLettrDoc.Save(sTPAFileName, Aspose.Words.SaveFormat.Pdf);

                                //// Send TPa Notification (email and msg center msg)
                                ////oRetNotification = SendEmailNotification(3350, "", "", sb.ToString(), sTPAEmail, sTPAInloginId + "|" + sTPAName, sTPAFileName);// first send the msgcenter with attachment

                                elArrayOfInLoginId = new XElement("ArrayOfInLoginId");
                                childElement = new XElement("InLoginId", sTPAInloginId);
                                elArrayOfInLoginId.Add(childElement);
                                sLoginIdXML = elArrayOfInLoginId.ToString();

                                oRetNotification = SendToMessageCenter(sLoginIdXML, FromAddress, "Required Minimum Distribution package", GetMessageBody(sb.ToString()), sTPAFileName);// first send the msgcenter with attachment

                                if (oRetNotification.returnStatus != ReturnStatusEnum.Succeeded)
                                {
                                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                                    General.CopyResultError(oReturn, oRetNotification);
                                }
                                else
                                {
                                    oRetNotification = SendEmailNotification(3340, "", "", "", sTPAEmail, sTPAInloginId + "|" + sTPAName, "");// then send the email without attachment
                                }
                            }
                            catch (Exception exi)
                            {
                                Utils.LogError(exi);
                                oReturn.returnStatus = ReturnStatusEnum.Failed;
                                oReturn.isException = true;
                                oReturn.confirmationNo = string.Empty;
                                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in GenerateRMDDataFile.FilesGeneration  - contract_id = " + contract_id + " sub_id = " + sub_id + " Error: " + exi.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
                            }
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
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateTPARMDFinalLetter  - contract_id = " + contract_id + " sub_id = " + sub_id + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }
        private ResultReturn PopulateTPACommunicationInfo(SOAModel.ContractInfo oConInfo, string sRMDDataFileName, ref DataTable dtTPACommunication)
        {
            // creates 1 data file for each contract
            ResultReturn oReturn = new();
            string contract_id = "";
            string sub_id = "000";

            List<SOAModel.TPAContactInformation> lstTPAPlanContacts = null;
            List<SOAModel.TPAContactInformation> oTpaCt = null;
            SOAModel.TPACompanyContactInformations oTPACompanyInfos;
            bool bFoundAssignedTPA = false;
            ConsolidatedNotifications oConsObj = new();
            string sTPAName = "";
            string sTPAContactId = "";
            string sPlanName = "";
            string sTPAInloginId = "";
            string sTPAEmail = "";
            string sTPAFirmName = "";
            string sAddressLine1 = "";
            string sAddressLine2 = "";
            string sAddressLine3 = "";
            DataRow drTPAComm;

            try
            {
                contract_id = oConInfo.ContractID;
                sPlanName = oConInfo.PlanName;
                lstTPAPlanContacts = oConsObj.GetTpaAssignedContacts(oConInfo, TRS.IT.SI.BusinessFacadeLayer.Model.E_TPAContactType.TPASrPlanAdministrator, 0);
                if (lstTPAPlanContacts.Count > 0)
                {
                    for (int i = 0; i < lstTPAPlanContacts.Count; i++)
                    {
                        if ((!string.IsNullOrEmpty(lstTPAPlanContacts[i].Web_InLoginId)) && lstTPAPlanContacts[i].Web_InLoginId != "0")
                        {
                            bFoundAssignedTPA = true;
                            sTPAName = GetFullName(lstTPAPlanContacts[i].FirstName, lstTPAPlanContacts[0].LastName);

                            sTPAContactId = lstTPAPlanContacts[i].Contact_id;
                            sTPAInloginId = lstTPAPlanContacts[i].Web_InLoginId;
                            sTPAEmail = lstTPAPlanContacts[i].CommunicationInfo.EmailAddress;
                            sTPAFirmName = lstTPAPlanContacts[i].CompanyName;
                            sAddressLine1 = lstTPAPlanContacts[i].Address.Address1;
                            sAddressLine2 = lstTPAPlanContacts[i].Address.Address2 + " " + lstTPAPlanContacts[i].Address.Address3;
                            sAddressLine3 = lstTPAPlanContacts[i].Address.City + ", " + lstTPAPlanContacts[i].Address.State + " " + lstTPAPlanContacts[i].Address.ZipCode;


                            drTPAComm = dtTPACommunication.NewRow();
                            // fill up dataRow
                            drTPAComm["contract_id"] = oConInfo.ContractID;
                            drTPAComm["PlanName"] = oConInfo.PlanName;
                            drTPAComm["contact_id"] = sTPAContactId;
                            drTPAComm["in_login_id"] = sTPAInloginId;
                            drTPAComm["email_id"] = sTPAEmail;
                            drTPAComm["Name"] = sTPAName;
                            drTPAComm["TPALetterFile"] = "";// sFileName;
                            drTPAComm["RMDDataFile"] = sRMDDataFileName;
                            dtTPACommunication.Rows.Add(drTPAComm);

                            break; // only  1
                        }
                    }
                }

                if (bFoundAssignedTPA == false)
                {
                    // get TPAOwner
                    oTPACompanyInfos = oConsObj.GetTPAContractContactInfoFromSRV(oConInfo.ContractID, oConInfo.SubID);
                    oTpaCt = oConsObj.GetTPAContactsByContactType(oTPACompanyInfos, TRS.IT.SI.BusinessFacadeLayer.Model.E_TPACompanyContactType.TPAOwner);
                    if (oTpaCt == null || oTpaCt.Count == 0)
                    {
                        throw new Exception("TPA Contact information not found");
                    }

                    for (int i = 0; i < oTpaCt.Count; i++)
                    {
                        sTPAName = ""; sTPAContactId = ""; sPlanName = ""; sTPAInloginId = ""; sTPAEmail = "";//sTPAFirmName = "";sAddressLine1 = "";sAddressLine2 = "";sAddressLine3 = "";

                        if ((!string.IsNullOrEmpty(oTpaCt[i].Web_InLoginId)) && oTpaCt[i].Web_InLoginId != "0")
                        {
                            sTPAName = GetFullName(oTpaCt[i].FirstName, oTpaCt[i].LastName);
                            sTPAContactId = oTpaCt[i].Contact_id;
                            sTPAInloginId = oTpaCt[i].Web_InLoginId;
                            sTPAEmail = oTpaCt[i].CommunicationInfo.EmailAddress;
                            sTPAFirmName = oTpaCt[i].CompanyName;
                            sAddressLine1 = oTpaCt[i].Address.Address1;
                            sAddressLine2 = oTpaCt[i].Address.Address2 + " " + oTpaCt[i].Address.Address3;
                            sAddressLine3 = oTpaCt[i].Address.City + ", " + oTpaCt[i].Address.State + " " + oTpaCt[i].Address.ZipCode;

                            drTPAComm = dtTPACommunication.NewRow();
                            // fill up dataRow
                            drTPAComm["contract_id"] = oConInfo.ContractID;
                            drTPAComm["PlanName"] = oConInfo.PlanName;
                            drTPAComm["contact_id"] = sTPAContactId;
                            drTPAComm["in_login_id"] = sTPAInloginId;
                            drTPAComm["email_id"] = sTPAEmail;
                            drTPAComm["Name"] = sTPAName;
                            drTPAComm["TPALetterFile"] = "";// sFileName;
                            drTPAComm["RMDDataFile"] = sRMDDataFileName;


                            dtTPACommunication.Rows.Add(drTPAComm);

                        }
                    }

                }

                if ((string.IsNullOrEmpty(sTPAInloginId)) || sTPAInloginId == "0")
                {
                    throw new Exception("TPA Contact information not found");
                }


            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in PopulateTPACommunicationInfo  - contract_id = " + contract_id + " sub_id = " + sub_id + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;
        }
        private DataTable CreateTPACommunicationTable()
        {
            DataTable dt = new("TPACommunication");

            dt.Columns.Add("contract_id", Type.GetType("System.String"));
            dt.Columns.Add("PlanName", Type.GetType("System.String"));
            dt.Columns.Add("contact_id", Type.GetType("System.String"));
            dt.Columns.Add("in_login_id", Type.GetType("System.String"));
            dt.Columns.Add("email_id", Type.GetType("System.String"));
            dt.Columns.Add("Name", Type.GetType("System.String"));
            //dt.Columns.Add("TPAFirmName", System.Type.GetType("System.String"));
            //dt.Columns.Add("AddressLine1", System.Type.GetType("System.String"));
            //dt.Columns.Add("AddressLine2", System.Type.GetType("System.String"));
            //dt.Columns.Add("AddressLine3", System.Type.GetType("System.String"));
            dt.Columns.Add("TPALetterFile", Type.GetType("System.String"));
            dt.Columns.Add("RMDDataFile", Type.GetType("System.String"));

            return dt;
        }

        private ResultReturn SendEmailNotification(int iMsg_Id, string sContract_id, string sSub_Id, string sPlanName, string sEmailIds, string sMsgcenterIds, string sAttachmentPath)
        {
            ResultReturn oResults = null;
            MessageServiceKeyValue[] Keys = null;
            string sKeyName_email = "to_email";
            string sKeyName_MsgCtr = "to_MessageCenter";
            string sKeyName_planName = "plan_name_or_company_name";

            wsMS.MessageAttachment[] attachments = null;
            if (iMsg_Id == 0)
            {
                oResults = new ResultReturn();
                oResults.returnStatus = ReturnStatusEnum.Succeeded;
                //return oResults;
            }
            else
            {
                if (sEmailIds != null)
                {
                    sEmailIds = sEmailIds.Trim();
                }

                if (sMsgcenterIds != null)
                {
                    sMsgcenterIds = sMsgcenterIds.Trim();
                }

                sKeyName_email = "to_email";
                if (!string.IsNullOrEmpty(sEmailIds))
                {
                    Keys = new MessageServiceKeyValue[1];

                    MessageServiceKeyValue nKey_emails = new();
                    nKey_emails.key = sKeyName_email;
                    nKey_emails.value = sEmailIds;

                    Keys[0] = nKey_emails; //add key at last position.
                }

                sKeyName_MsgCtr = "to_MessageCenter";
                if (!string.IsNullOrEmpty(sMsgcenterIds))
                {
                    MessageServiceKeyValue nKey_MsgCtr = new();
                    nKey_MsgCtr.key = sKeyName_MsgCtr;
                    nKey_MsgCtr.value = sMsgcenterIds;

                    if (Keys != null && Keys.GetLength(0) > 0)
                    {
                        Array.Resize(ref Keys, Keys.GetLength(0) + 1); // resize to add custom Message center Ids data key
                        Keys[Keys.GetLength(0) - 1] = nKey_MsgCtr; //add key at last position.
                    }
                    else
                    {
                        Keys = new MessageServiceKeyValue[1];
                        Keys[0] = nKey_MsgCtr;
                    }

                }

                sKeyName_planName = "plan_name_or_company_name";
                if (!string.IsNullOrEmpty(sPlanName))
                {
                    MessageServiceKeyValue nKey_PlanName = new();
                    nKey_PlanName.key = sKeyName_planName;
                    nKey_PlanName.value = sPlanName;

                    if (Keys != null && Keys.GetLength(0) > 0)
                    {
                        Array.Resize(ref Keys, Keys.GetLength(0) + 1); // resize to add custom Message center Ids data key
                        Keys[Keys.GetLength(0) - 1] = nKey_PlanName; //add key at last position.
                    }
                    else
                    {
                        Keys = new MessageServiceKeyValue[1];
                        Keys[0] = nKey_PlanName;
                    }
                }

                MessageService oMS = new();
                if (!string.IsNullOrEmpty(sAttachmentPath))
                {
                    attachments = new wsMS.MessageAttachment[1];
                    attachments[0] = new wsMS.MessageAttachment();
                    attachments[0].Name = Path.GetFileName(sAttachmentPath);
                    attachments[0].DateCreated = DateTime.Now;
                    attachments[0].Content = File.ReadAllBytes(sAttachmentPath);
                }

                if (!string.IsNullOrEmpty(sContract_id))
                {
                    oResults = oMS.SendMessage(sContract_id, sSub_Id, iMsg_Id, Keys, "TRS-Auto-Message-Service", attachments);
                }
                else
                {
                    oResults = oMS.SendMessage_NoContract(iMsg_Id, Keys, "TRS-Auto-Message-Service", attachments);
                }

                if (oResults == null)
                {
                    ErrorInfo oError = new();
                    oResults = new ResultReturn();
                    oError.errorNum = 1;
                    oError.errorDesc = "No result returned by MessageService.";
                }

            }

            return oResults;
        }
        private ResultReturn SendToMessageCenter(string sLoginIdXML, string FromAddress, string a_sSubject, string a_sBody, string sAttachmentPath, int iBendInLoginId = 0)
        {
            ResultReturn oReturn = new();

            if (iBendInLoginId == 0)
            {
                GeneralDC oGenDC = new();
                iBendInLoginId = oGenDC.GetMsgCtrAcctByExLoginId("BendProcess");
            }

            MsgCenterMessageService oWebMsgAdaptor = new();
            SI.Services.wsMessage.webMessage oWebMsgCtr = new();
            SI.Services.wsMessage.MsgData oWebMsgData = new();

            //Prepare the attachment
            SI.Services.wsMessage.Attachment[] oAttachment;
            if (!string.IsNullOrEmpty(sAttachmentPath))
            {
                oAttachment = new SI.Services.wsMessage.Attachment[1];
                string sPromptName = Path.GetFileName(sAttachmentPath);

                byte[] RawData = File.ReadAllBytes(sAttachmentPath);
                oAttachment[0] = new SI.Services.wsMessage.Attachment();
                oAttachment[0].Data = Convert.ToBase64String(RawData);
                oAttachment[0].PromptFileName = sPromptName;

                oWebMsgData.Attachments = oAttachment;
            }

            oWebMsgData.ReplyAllowed = false;
            oWebMsgData.Body = a_sBody;
            oWebMsgCtr.Subject = a_sSubject;
            oWebMsgCtr.AttachmentCount = 1;
            oWebMsgCtr.MsgData = oWebMsgData;
            oWebMsgCtr.FromAddress = FromAddress;
            oWebMsgCtr.MsgSource = "System Back-end";
            oWebMsgCtr.CreateBy = iBendInLoginId.ToString();
            oWebMsgCtr.CreateDt = DateTime.Now.ToString();
            oWebMsgCtr.ExpireDt = DateTime.Now.AddDays(90).ToString();
            oWebMsgCtr.SendNotification = "N";
            oWebMsgCtr.FolderId = 1;// inbox
            oWebMsgCtr.MsgType = "0";
            oWebMsgCtr.SenderInLoginId = iBendInLoginId.ToString();

            oReturn = oWebMsgAdaptor.SendMessageCenterMessage(sLoginIdXML, oWebMsgCtr);

            return oReturn;

        }

        private string GetMessageBody(string sPlanNames)
        {
            StringBuilder strMsgBody = new();


            strMsgBody.AppendLine("<p align=center><b>TIME SENSITIVE COMMUNICATION</b></p> <br /> <p>Attached is a consolidated listing of participants included in the annual Required Minimum Distribution (RMD) package for the contracts you administer.</p>");
            strMsgBody.AppendLine("<br />");
            strMsgBody.AppendLine("<p>The annual RMD package for each separate contract has been posted to the online Document Center. Go to Plan Information > Document Center > Reporting & Testing tab > Required Minimum Distribution Package. The package consists of an explanatory letter and a listing of participants. The attached consolidated listing has been prepared so that you can more easily review the material without having to access each contract. We recommend reading the letter so you are aware of the process and deadlines before proceeding with your review.</p>");
            strMsgBody.AppendLine("<br /> <br />");
            strMsgBody.AppendLine(sPlanNames);
            strMsgBody.Append("<br /> <br /> <br />");
            strMsgBody.AppendLine("Sincerely, <br />");
            strMsgBody.AppendLine("Transamerica Retirement Solutions<br />");
            strMsgBody.AppendLine("<IMG src=\"https://www.ta-retirement.com/MessageService/Images/trslogo_solutions.gif\" align=left vspace=7 border=0><br />");
            strMsgBody.Append("<br /> <br /> <br /> <br /> <br />");
            strMsgBody.AppendLine("<p>Transamerica or Transamerica Retirement Solutions refers to Transamerica Retirement Solutions, LLC, which is headquartered in Los Angeles, CA.</p>");
            strMsgBody.AppendLine("<font size=2><br /><p>This is an automated message. Please do not reply.</p></font>");

            return strMsgBody.ToString();
        }

    }
}
