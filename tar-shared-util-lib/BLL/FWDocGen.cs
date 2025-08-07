using System.Data;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using Aspose.Cells;
using Aspose.Words;
using SIUtil;
using BFL = TRS.IT.SI.BusinessFacadeLayer;
namespace TRS.IT.SI.BusinessFacadeLayer
{
    public class FWDocGen
    {

        private FundWizard _oFw;
        private string _sLicenseFile;
        private string _sTemplatePath;
        private string _sOutputPath;
        private string _sLocalPath;
        public string LicenseFile
        {
            get
            {
                return _sLicenseFile;
            }
            set
            {
                _sLicenseFile = value;
            }
        }
        public string TemplatePath
        {
            get
            {
                return _sTemplatePath;
            }
            set
            {
                _sTemplatePath = value;
            }
        }
        public string OutputPath
        {
            get
            {
                return _sOutputPath;
            }
            set
            {
                _sOutputPath = value;
            }
        }
        public string LocalPath
        {
            get
            {
                return _sLocalPath;
            }
            set
            {
                _sLocalPath = value;
            }
        }
        public FWDocGen(FundWizard a_oFw)
        {
            _oFw = a_oFw;
        }
        public void removeBaddata(ref DataTable oTable)
        {
            foreach (DataRow oRow in oTable.Rows)
            {
                if (oRow["fund_name"].ToString().IndexOf("<b>") >= 0)
                {
                    oRow["fund_name"] = oRow["fund_name"].ToString().Replace("<b>", "");
                    oRow["fund_name"] = oRow["fund_name"].ToString().Replace("</b>", "");
                    oRow.AcceptChanges();
                }
            }
        }
        public string CreateFundMappingSpreadsheet(ref string o_sError)
        {

            string sReturnFilePathNname = "";
            string sFileName = "";
            var workbook = new Workbook();
            Worksheet sheet;
            Cells cells;

            string sOrigDefFundId = "";
            string sNewDefFundId = "";
            string sOrigDefFundIdDesc = "";
            string sNewDefFundIdDesc = "";
            string sOrigForfeitFundId = "";
            string sNewForfeitFundId = "";
            string sOrigForfeitFundIdDesc = "";
            string sNewForfeitFundIdDesc = "";
            string sConsentMethod = "N/A";

            try
            {
                o_sError = "";
                SetCellsLicense();
                workbook = new Workbook(TemplatePath + "Fund Mapping Spreadsheet_Template.xls");
                sheet = workbook.Worksheets[0];
                cells = sheet.Cells;

                // Put a string value into the cell using its name
                cells["B1"].PutValue(BFL.FWUtils.GetHdrData("contract_id", _oFw.PdfHeader)[0]);
                cells["B2"].PutValue(BFL.FWUtils.GetHdrData("plan_name", _oFw.PdfHeader)[0]);
                cells["B3"].PutValue(_oFw.PartnerId);

                switch (_oFw.Action)
                {
                    case 1:
                        {
                            cells["B4"].PutValue("Add");
                            break;
                        }
                    case 2:
                        {
                            cells["B4"].PutValue("Delete");
                            break;
                        }
                    case 3:
                        {
                            cells["B4"].PutValue("Add and Delete");
                            break;
                        }
                    case 4:
                        {
                            cells["B4"].PutValue("Default or QDIA");
                            break;
                        }

                    default:
                        {
                            cells["B4"].PutValue("");
                            break;
                        }
                }

                if (_oFw.Action != 8)
                {
                    cells["B5"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_portXpress_selected, _oFw.PdfHeader)[0] == "true" ? "YES" : "NO");
                }
                var strTmp = new string[2];
                strTmp = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_PortXpress_changeauthorization_type, _oFw.PdfHeader);
                if (!string.IsNullOrEmpty(strTmp[0]))
                {

                    if (strTmp[0].ToLower().Trim() == "unsubscribe")
                    {
                        sConsentMethod = "Unsubscribe";
                    }

                    if (strTmp[0].ToLower().Trim() == "indemnify")
                    {
                        sConsentMethod = "Indemnification";
                    }

                }

                cells["B6"].PutValue(sConsentMethod);
                cells["B7"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_PortXpress_custom, _oFw.PdfHeader)[0] == "true" ? "YES" : "NO");

                sOrigDefFundId = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund, _oFw.PdfHeader)[0];
                sNewDefFundId = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund_new, _oFw.PdfHeader)[0];

                sOrigDefFundIdDesc = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund, _oFw.PdfHeader)[1];
                sNewDefFundIdDesc = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund_new, _oFw.PdfHeader)[1];

                string sTMFSeries = string.Empty;
                sTMFSeries = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund_tmf_select, _oFw.PdfHeader)[0];
                sOrigForfeitFundId = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_forfeiture_fund, _oFw.PdfHeader)[0];
                sNewForfeitFundId = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_forfeiture_fund_new, _oFw.PdfHeader)[0];

                sOrigForfeitFundIdDesc = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_forfeiture_fund, _oFw.PdfHeader)[1];
                sNewForfeitFundIdDesc = BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_forfeiture_fund_new, _oFw.PdfHeader)[1];

                if (!string.IsNullOrEmpty(sNewDefFundId))
                {

                    cells["B10"].PutValue("YES");

                    cells["C10"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund + "_partner_id", _oFw.PdfHeader)[0]);
                    cells["D10"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund + "_partner_id", _oFw.PdfHeader)[1]);
                    cells["E10"].PutValue(sOrigDefFundId);
                    cells["F10"].PutValue(sOrigDefFundIdDesc);
                    cells["G10"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund_new + "_partner_id", _oFw.PdfHeader)[0]);
                    cells["H10"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund_new + "_partner_id", _oFw.PdfHeader)[1]);
                    cells["I10"].PutValue(sNewDefFundId);
                    cells["J10"].PutValue(sNewDefFundIdDesc);
                }

                else
                {
                    cells["B10"].PutValue("NO");
                }


                if (!string.IsNullOrEmpty(sTMFSeries))
                {
                    cells["B13"].PutValue(sTMFSeries);
                }
                else
                {
                    cells["B13"].PutValue("NO");
                }

                cells["B14"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_default_fund_new, _oFw.PdfHeader)[0] == "-1" ? "YES" : "NO");

                if (!string.IsNullOrEmpty(sNewForfeitFundId))
                {

                    cells["B16"].PutValue("YES");
                    cells["C17"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_forfeiture_fund + "_partner_id", _oFw.PdfHeader)[0]);
                    cells["D17"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_forfeiture_fund + "_partner_id", _oFw.PdfHeader)[1]);
                    cells["E17"].PutValue(sOrigForfeitFundId);
                    cells["F17"].PutValue(sOrigForfeitFundIdDesc);
                    cells["G17"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_forfeiture_fund_new + "_partner_id", _oFw.PdfHeader)[0]);
                    cells["H17"].PutValue(BFL.FWUtils.GetHdrData(BFL.FWUtils.C_hdr_forfeiture_fund_new + "_partner_id", _oFw.PdfHeader)[1]);
                    cells["I17"].PutValue(sNewForfeitFundId);
                    cells["J17"].PutValue(sNewForfeitFundIdDesc);
                }
                else
                {
                    cells["B16"].PutValue("NO");
                }

                DataTable tblN;
                var argoTable = _oFw.NewFunds;
                removeBaddata(ref argoTable);
                var argoTable1 = _oFw.NewFundsCustomPX;
                removeBaddata(ref argoTable1);
                var dvNew = new DataView(_oFw.NewFunds, "action=1", "", DataViewRowState.CurrentRows);
                tblN = dvNew.ToTable("AddedFunds", false, "partner_fund_id", "Abbrev_fund_name", "fund_id", "fund_name");

                DataTable tblD;
                var dvDel = new DataView(_oFw.NewFunds, "action=2", "", DataViewRowState.CurrentRows);
                tblD = dvDel.ToTable("DeletedFunds", false, "partner_fund_id", "Abbrev_fund_name", "fund_id", "fund_name", "to_partner_fund_id", "to_Abbrev_fund_name", "to_fund_id", "to_fund_name");

                const int C_FIRST_TABLE_BEGIN_ROW = 19;
                int i = 0;
                i = C_FIRST_TABLE_BEGIN_ROW + tblN.Rows.Count + 4;
                ImportTableOptions importTableOptions = new()
                {
                    IsFieldNameShown = false,
                    InsertRows = true
                };
                // Added funds
                sheet.Cells.ImportData(tblN, C_FIRST_TABLE_BEGIN_ROW, 2, importTableOptions);

                // Deleted and transfered funds
                sheet.Cells.ImportData(tblD, i, 2, importTableOptions);

                sheet.AutoFitColumns();
                sheet.Name = "Case- " + _oFw.CaseNo + " - " + DateTime.Now.ToString("yyyyMMdd~HHmmss");

                sFileName = "FundMappingSpreadsheet" + _oFw.CaseNo + ".xls";
                workbook.Save(LocalPath + sFileName);
                CopyFileToRemote(LocalPath + sFileName, OutputPath + sFileName);
                _oFw.InsertTaskFundMappingSpreadsheet(OutputPath, sFileName);

                sReturnFilePathNname = OutputPath + sFileName;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                o_sError = ex.Message;
                sReturnFilePathNname = "";
            }
            return sReturnFilePathNname;

        }
        public string CreateFundRaiders(ref string sError)
        {
            string sReturnFilePathNname = "";
            string sFileName = "";
            string DocFileName = ""; // full file name
            string UserID = "";
            string sResponse = "";
            XDocument xResponseXml;
            var sb = new StringBuilder();
            UserID = @"US\SPTLATRSDOTCOM";

            try
            {
                sResponse = _oFw.GenerateFundRaider(_oFw.ContractId, _oFw.SubId, UserID);

                if (!string.IsNullOrEmpty(sResponse))
                {
                    xResponseXml = XDocument.Load(new StringReader(sResponse));
                    if (xResponseXml != null)
                    {
                        var xElError = xResponseXml.XPathSelectElement("//DocgenAsmResult//Docgen_Asm//asm_errordesc");
                        var xElMsg = xResponseXml.XPathSelectElement("//DocgenAsmResult//Docgen_Asm//asm_docmsg");
                        var xElPath = xResponseXml.XPathSelectElement("//DocgenAsmResult//Docgen_Asm//asm_filepath");

                        sError = "";
                        if (!(xElMsg == null) && xElMsg.Value == "Document generation is not applicable" | xElMsg.Value == "Manual Notice required")
                        {
                            sb.AppendLine("Message: " + xElMsg.Value);
                        }

                        if (!(xElError == null) && !string.IsNullOrEmpty(xElError.Value))
                        {
                            // sError = xElError.Value
                            sb.AppendLine("Error: " + xElError.Value);
                        }
                        if (!(xElPath == null) && !string.IsNullOrEmpty(xElPath.Value.Trim()))
                        {
                            DocFileName = xElPath.Value.Trim();
                        }
                        else
                        {
                            sb.AppendLine("Error: asm_filepath is empty.");
                        }

                        sError = sb.ToString();

                        if (!string.IsNullOrEmpty(sError.Trim()))
                        {
                            sReturnFilePathNname = "N/A";
                        }
                        // Return "N/A"
                        else
                        {

                            sFileName = "FundRider" + _oFw.CaseNo + ".pdf";
                            // CopyFileToRemote(DocFileName, Me.OutputPath & sFileName)
                            ConvertWordDocToPdf(DocFileName, OutputPath + sFileName);
                            _oFw.InsertTaskFundRaider(OutputPath, sFileName, DocFileName);
                            sReturnFilePathNname = OutputPath + sFileName;
                        }
                    }
                    else
                    {
                        throw new Exception("Invalid return from CreateFundRaiders()  output: " + sResponse);
                    }
                }
                else
                {
                    throw new Exception("Invalid return from CreateFundRaiders()  output: " + sResponse);
                }
            }

            catch (Exception ex)
            {
                sError = sError + " Exception : " + ex.Message;
                sReturnFilePathNname = "N/A";
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
            }

            if (!string.IsNullOrEmpty(sError.Trim()) || sReturnFilePathNname == "N/A")
            {
                try
                {
                    var xEl = new XElement("Error", sError)
    // In future if you enable this task to be shown in sponsors task list then do not log technical error here (SELECT * FROM dbo.si_code_master WHERE code_type =  'fw_tasks_sponsor')
    ;
                    _oFw.InsertTask(Model.FundWizardInfo.FwTaskTypeEnum.FundRider, -1, [xEl]);
                }
                catch (Exception exIn)
                {
                    string temp = exIn.Message; // do nothing
                    Logger.LogMessage(exIn.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                }
            }

            return sReturnFilePathNname;

        }
        public string CreateFundQDIANotice(ref string sError)
        {
            string sReturnFilePathNname = "";
            string sFileName;
            string DocFileName = ""; // full file name
            string UserID = "";
            string sResponse = "";
            XDocument xResponseXml;
            var sb = new StringBuilder();

            UserID = @"US\SPTLATRSDOTCOM";

            try
            {
                sResponse = _oFw.GenerateQDIA(_oFw.ContractId, _oFw.SubId, UserID);

                if (!string.IsNullOrEmpty(sResponse))
                {
                    xResponseXml = XDocument.Load(new StringReader(sResponse));
                    if (xResponseXml != null)
                    {
                        var xElError = xResponseXml.XPathSelectElement("//DocgenAsmResult//Docgen_Asm//asm_errordesc");
                        var xElMsg = xResponseXml.XPathSelectElement("//DocgenAsmResult//Docgen_Asm//asm_docmsg");
                        var xElPath = xResponseXml.XPathSelectElement("//DocgenAsmResult//Docgen_Asm//asm_filepath");

                        sError = "";

                        if (!(xElMsg == null) && xElMsg.Value == "Document generation is not applicable" | xElMsg.Value == "Manual Notice required")
                        {
                            sb.AppendLine("Message: " + xElMsg.Value);
                        }

                        if (!(xElError == null) && !string.IsNullOrEmpty(xElError.Value))
                        {
                            // sError = xElError.Value
                            sb.AppendLine("Error: " + xElError.Value);
                        }
                        if (!(xElPath == null) && !string.IsNullOrEmpty(xElPath.Value.Trim()))
                        {
                            DocFileName = xElPath.Value.Trim();
                        }
                        else
                        {
                            sb.AppendLine("Error: asm_filepath is empty.");
                        }
                        sError = sb.ToString();

                        if (!string.IsNullOrEmpty(sError.Trim()))
                        {
                            sReturnFilePathNname = "N/A";
                            return "N/A";
                        }
                        else
                        {
                            sFileName = "FundPptQdiaNotice" + _oFw.CaseNo + ".pdf";
                            ConvertWordDocToPdf(DocFileName, OutputPath + sFileName);
                            _oFw.InsertTaskFundQDIA(OutputPath, sFileName, DocFileName);
                            sReturnFilePathNname = OutputPath + sFileName;

                        }
                    }
                    else
                    {
                        throw new Exception("Invalid return from GenerateQDIA()  output: " + sResponse);
                    }
                }
                else
                {
                    throw new Exception("Invalid return from GenerateQDIA()  output: " + sResponse);
                }
            }

            catch (Exception ex)
            {
                sError = sError + " Exception : " + ex.Message;
                sReturnFilePathNname = "N/A";
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
            }

            return sReturnFilePathNname;

        }
        public string ConvertWordDocToPdf(string sFullWordFileName, string sOutputFileName = "")
        {
            string sExtension = "";
            SetLicense();


            sExtension = Path.GetExtension(sFullWordFileName);

            if (sExtension.ToUpper() != ".DOC" && sExtension.ToUpper() != ".DOCX")
            {
                throw new Exception("Invalid file format");
            }
            else
            {

                if (string.IsNullOrEmpty(sOutputFileName))
                {

                    sOutputFileName = sFullWordFileName;
                }

                else if (string.IsNullOrEmpty(Path.GetDirectoryName(sOutputFileName))) // only file name is supplied
                {
                    sOutputFileName = Path.Combine(Path.GetDirectoryName(sFullWordFileName), sOutputFileName);

                }

                sOutputFileName = Path.ChangeExtension(sOutputFileName, ".pdf");
                var doc = new Document(sFullWordFileName);
                doc.Save(sOutputFileName, Aspose.Words.SaveFormat.Pdf);

            }

            return sOutputFileName;

        }
        private void SetCellsLicense()
        {
            var License = new Aspose.Cells.License();
            if (File.Exists(LicenseFile))
            {
                License.SetLicense(LicenseFile);
            }
        }
        private void SetLicense()
        {
            var License = new Aspose.Words.License();
            if (File.Exists(LicenseFile))
            {
                License.SetLicense(LicenseFile);
            }
        }
        public void CopyFileToRemote(string a_SourceFile, string a_sTargetFile)
        {
            File.Copy(a_SourceFile, a_sTargetFile, true);
        }
    }
}