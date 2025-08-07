using System.Data;
using System.Xml.Linq;
using Aspose.Words;
using SIUtil;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;

namespace FWSignedDocsToMsgcntrBatch.BLL
{
    class FWDocGen
    {
        private TARSharedUtilLibBFLBLL.FundWizard _oFw;
        private TARSharedUtilLibBFLBLL.FWDocGen objFwDocGen;
        private BLL.FundWizard fundWizard;
        public string LicenseFile { get; set; }
        public string TemplatePath { get; set; }
        public string OutputPath { get; set; }
        public string LocalPath { get; set; }
        public FWDocGen(TARSharedUtilLibBFLBLL.FundWizard a_oFw)
        {
            _oFw = a_oFw;
            objFwDocGen = new TARSharedUtilLibBFLBLL.FWDocGen(_oFw);
            fundWizard = new BLL.FundWizard(_oFw);
        }
        public string CreateSPnPPtLetters(ref string o_sError, string doc_type = "doc", string doc_from = "")
        {

            string sReturnFilePathNname = "";
            Document doc1;
            Document doc2;
            DocumentBuilder builder1;
            DocumentBuilder builder2;
            string sFileName = "";
            var tbl2 = new DataTable("HdrInfo");
            string sError = "";
            string inputXml = "";
            string noticeType = "";

            try
            {
                SetLicense();
                var argoTable = _oFw.NewFunds;
                objFwDocGen.removeBaddata(ref argoTable);
                var argoTable1 = _oFw.NewFundsCustomPX;
                objFwDocGen.removeBaddata(ref argoTable1);
                bool bskipPX = false;
                var oPendingFundChanges = new List<TRS.IT.SOA.Model.FundPendingChanges>();
                oPendingFundChanges = fundWizard.GetPendingFundChangeByContract();
                if (_oFw.Action == 8 && oPendingFundChanges.Count > 0)
                {
                    bskipPX = true;
                }

                if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_portXpress_selected, _oFw.PdfHeader)[0] == "true" & !bskipPX)
                {
                    inputXml = GetDocRulesInputXml();
                    noticeType = GetLetterNoticeType(inputXml);
                    if (string.IsNullOrEmpty(noticeType))
                    {
                        throw new Exception("unable to get noticeType, input :" + inputXml);
                    }
                    TARSharedUtilLibBFLBLL.FWUtils.AddPdfRow(_oFw.PdfHeader, TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_Sponsor_Letter_type, inputXml, noticeType);
                    sReturnFilePathNname = PXSponsorLetter(noticeType, ref sError, doc_type, doc_from);
                }
                else
                {

                    TARSharedUtilLibBFLBLL.FWUtils.AddPdfRow(_oFw.PdfHeader, TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_Sponsor_Letter_type, "no", "no");

                    if (string.IsNullOrEmpty(doc_from))
                    {
                        if (_oFw.SignMethod == 0)
                        {
                            doc1 = new Document(TemplatePath + "FundChangeSponsorLetter.doc");
                        }
                        else
                        {
                            doc1 = new Document(TemplatePath + "FundChangeSponsorLetterEsign.doc");
                        }
                    }
                    else
                    {
                        doc1 = new Document(TemplatePath + "FundChangeSponsorLetterEsign.doc");
                    }

                    doc2 = new Document(TemplatePath + "FundChangeParticipantLetter.doc");
                    builder1 = new DocumentBuilder(doc1);
                    builder2 = new DocumentBuilder(doc2);
                    var tables = builder2.Document.GetChildNodes(NodeType.Table, true);
                    if (_oFw.Action == 2)
                    {
                        tables[3].Remove();
                        tables[2].Remove();
                        tables[1].Remove();
                        tables[0].Remove();

                        builder2.MoveToMergeField("fund_notes");
                        builder2.Write("");
                    }
                    else if (IsNavPaaPlan())
                    {
                        tables[1].Remove();
                    }
                    else if (IsNavPlan())
                    {
                        tables[2].Remove();
                        tables[0].Remove();
                    }
                    else
                    {
                        tables[2].Remove();
                        tables[1].Remove();
                    }

                    o_sError = "";

                    tbl2.Columns.Add(new DataColumn("company_name", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("primary_addr1", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("primary_city_n_state", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("plan_name", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("contract_id", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("primary_first_name", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("primary_last_name", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("current_date", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("effective_date", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("transfer_date", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("project_manager", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("plan_admin", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("plan_admin_addr1", typeof(string)));
                    tbl2.Columns.Add(new DataColumn("plan_admin_city_n_state", typeof(string)));
                    TARSharedUtilLibBFLBLL.FWUtils.AddPdfRow(_oFw.PdfHeader, "pegasys_effective_date", "Pegasys Effective Date", "");
                    TARSharedUtilLibBFLBLL.FWUtils.AddPdfRow(_oFw.PdfHeader, "effective_date", "Effective Date", "");
                    TARSharedUtilLibBFLBLL.FWUtils.AddPdfRow(_oFw.PdfHeader, "transfer_date", "Transfer Date", "");
                    TARSharedUtilLibBFLBLL.FWUtils.AddPdfRow(_oFw.PdfHeader, "project_manager", "Project Manager", _oFw.PMName);
                    DataRow r1;
                    r1 = tbl2.NewRow();
                    r1["company_name"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("company_name", _oFw.PdfHeader)[0];
                    r1["plan_name"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("plan_name", _oFw.PdfHeader)[0];
                    r1["primary_addr1"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("primary_addr1", _oFw.PdfHeader)[0];
                    r1["primary_city_n_state"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("primary_city_n_state", _oFw.PdfHeader)[0];
                    r1["primary_first_name"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("primary_first_name", _oFw.PdfHeader)[0];
                    r1["primary_last_name"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("primary_last_name", _oFw.PdfHeader)[0];
                    r1["contract_id"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("contract_id", _oFw.PdfHeader)[0];
                    r1["current_date"] = DateTime.Today.ToString("MMMM dd, yyyy");
                    r1["effective_date"] = "";
                    r1["transfer_date"] = "";
                    if (_oFw.SignMethod == 0)
                    {
                        r1["project_manager"] = "Project Manager";
                    }
                    else
                    {
                        r1["project_manager"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("project_manager", _oFw.PdfHeader)[0];
                    }
                    r1["plan_admin"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("plan_admin", _oFw.PdfHeader)[0];
                    r1["plan_admin_addr1"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("plan_admin_addr1", _oFw.PdfHeader)[0];
                    r1["plan_admin_city_n_state"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("plan_admin_city_n_state", _oFw.PdfHeader)[0];

                    tbl2.Rows.Add(r1);
                    doc1.MailMerge.Execute(tbl2);
                    doc2.MailMerge.Execute(tbl2);
                    string sTxt1 = "In accordance with your request we will make the following Investment Choice changes to the above-named retirement Plan and Contract:";
                    builder1.MoveToMergeField("filler_add_fund");

                    int wW;
                    wW = 300;
                    if (_oFw.Action == 2)
                    {
                        sTxt1 = "In accordance with your request we will make the following Investment Choice changes to the above-named retirement Plan and Contract:";
                    }
                    builder1.Writeln(sTxt1);
                    builder1.Writeln();

                    if (_oFw.Action != 2)
                    {
                        // Sponsor letter
                        builder1.Write("We will ");
                        builder1.Bold = true;
                        builder1.Write("add");
                        builder1.Bold = false;
                        builder1.Writeln(" the following Investment Choice(s):");
                        builder1.Writeln();
                        builder1.ParagraphFormat.LeftIndent = 0.3d * 72d;
                        builder1.StartTable();
                        var dvNew = new DataView(_oFw.NewFunds, "action=1", "", DataViewRowState.CurrentRows);

                        foreach (DataRowView r in dvNew)
                        {
                            WriteRowL(builder1, Convert.ToString(r["fund_name"]), 350);
                        }

                        builder1.EndTable();
                        builder1.Writeln();
                        builder1.ParagraphFormat.LeftIndent = 0d;
                        builder1.Writeln();

                    }
                    if (_oFw.Action != 1)
                    {
                        if (_oFw.Action == 2)
                        {
                            builder1.Writeln("The following Investment Choice(s) will be deleted on or about the estimated Effective Date.");
                        }
                        else
                        {

                        }
                        builder1.Writeln();
                        builder1.Write("We will ");
                        builder1.Bold = true;
                        builder1.Write("delete");
                        builder1.Bold = false;
                        builder1.Writeln(" the following Investment Choice(s) and transfer any assets as shown:");
                        builder1.Writeln();
                        builder1.ParagraphFormat.LeftIndent = 0.3d * 72d;
                        builder1.StartTable();
                        WriteRowL(builder1, "Deleted Investment Choice(s)", "Assets Transferred to New/Existing Investment Choice(s)", wW, true);
                        var dvDel = new DataView(_oFw.NewFunds, "action=2", "", DataViewRowState.CurrentRows);

                        foreach (DataRowView r in dvDel)
                        {
                            WriteRowL(builder1, Convert.ToString(r["fund_name"]), Convert.ToString(r["to_fund_name"]), wW, false);
                        }

                        builder1.EndTable();
                        builder1.ParagraphFormat.LeftIndent = 0d;

                    }
                    builder1.MoveToMergeField("px_note");
                    if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_portXpress_selected, _oFw.PdfHeader)[0] == "true" && _oFw.Action != 8 && _oFw.Action != 7)
                    {
                        builder1.Write("and PortfolioXpress® notification ");
                    }
                    else
                    {
                        builder1.Write("");
                    }
                    builder1.MoveToMergeField("filler_pm");
                    builder1.Write("");
                    BuildPptSection(builder2, wW, ref o_sError);
                    if (!string.IsNullOrEmpty(o_sError))
                    {
                        return "";
                    }
                    // new column
                    if (_oFw.Action != 2)
                    {
                        var dtAddFunds = getAddedFundsInfo(builder2);
                        doc2.MailMerge.ExecuteWithRegions(dtAddFunds);
                    }

                    builder2.MoveToMergeField("estimated_effective_date");
                    if (!string.IsNullOrWhiteSpace(_oFw.PegasysDate) && DateTime.TryParse(_oFw.PegasysDate, out var pegasysDate))
                    {
                        builder2.Write(pegasysDate.ToString("MMMM dd, yyyy"));
                    }
                    else
                    {
                        builder2.Write("");
                    }

                    doc1.AppendDocument(doc2, ImportFormatMode.KeepSourceFormatting);
                    if (doc_type == "doc")
                    {
                        sFileName = "SponsorNoticePPTLetter" + _oFw.CaseNo + ".doc";
                        doc1.Save(LocalPath + sFileName);
                        objFwDocGen.CopyFileToRemote(LocalPath + sFileName, OutputPath + sFileName);
                        _oFw.InsertTaskSponsorNPartLetter(OutputPath, sFileName);
                    }
                    else
                    {
                        // doc for MC
                        string sLetterWord = "SponsorNoticePPTLetter" + _oFw.CaseNo + ".doc";
                        doc1.Save(LocalPath + sLetterWord);
                        objFwDocGen.CopyFileToRemote(LocalPath + sLetterWord, OutputPath + sLetterWord);
                        TARSharedUtilLibBFLBLL.FWUtils.AddPdfRow(_oFw.PdfHeader, TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_SponsorPPTPLetterFile, OutputPath + sLetterWord, OutputPath + sLetterWord);

                        sFileName = "SponsorNoticePPTLetter" + _oFw.CaseNo + ".pdf";
                        doc1.Save(OutputPath + sFileName, Aspose.Words.SaveFormat.Pdf);
                        _oFw.InsertTaskSponsorNPartLetter(OutputPath, sFileName);
                    }
                    sReturnFilePathNname = OutputPath + sFileName;
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                var el = new XElement("Error", new XAttribute("ErrorDescription", ex.Message), new XElement("NoticeType", noticeType), new XElement("RulesXml", inputXml));
                _oFw?.InsertTask(BFLModel.FundWizardInfo.FwTaskTypeEnum.SponsorPptLetters, -1, [el]); // Insert error log
                o_sError = ex.Message;
                throw;

            }

            return sReturnFilePathNname;

        }
        private void SetLicense()
        {
            var License = new Aspose.Words.License();
            if (File.Exists(LicenseFile))
            {
                License.SetLicense(LicenseFile);
            }
        }
        public string PXSponsorLetter(string sSample, ref string o_sError, string doc_type = "doc", string doc_from = "")
        {

            switch (sSample ?? "")
            {
                case "Sample 2":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_2", "2.0", ref o_sError, doc_type);
                    }
                case "Sample 2A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_2", "2.1", ref o_sError, doc_type);
                    }
                case "Sample 3":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_3", "3.0", ref o_sError, doc_type);
                    }
                case "Sample 3A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_3", "3.1", ref o_sError, doc_type);
                    }
                case "Sample 4":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_4", "4.0", ref o_sError, doc_type);
                    }
                case "Sample 4A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_4", "4.1", ref o_sError, doc_type);
                    }
                case "Sample 5":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_5", "5.0", ref o_sError, doc_type);
                    }
                case "Sample 5A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_5", "5.1", ref o_sError, doc_type);
                    }
                case "Sample 5B":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_5", "5.2", ref o_sError, doc_type);
                    }
                case "Sample 5C":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_5", "5.3", ref o_sError, doc_type);
                    }
                case "Sample 6":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_6", "6.0", ref o_sError, doc_type);
                    }
                case "Sample 6A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_6", "6.1", ref o_sError, doc_type);
                    }
                case "Sample 6B":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_6", "6.2", ref o_sError, doc_type);
                    }
                case "Sample 6C":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_6", "6.3", ref o_sError, doc_type);
                    }
                case "Sample 6.1":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_6", "61.0", ref o_sError, doc_type);
                    }
                case "Sample 6.1A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_6", "61.1", ref o_sError, doc_type);
                    }
                case "Sample 6.1B":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_6", "61.2", ref o_sError, doc_type);
                    }
                case "Sample 6.1C":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_6", "61.3", ref o_sError, doc_type);
                    }
                case "Sample 7":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_7_8", "FWPXFundChangeParticipantLetterType_7_8", "7.0", ref o_sError, doc_type);
                    }
                case "Sample 7A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_7_8", "FWPXFundChangeParticipantLetterType_7_8", "7.1", ref o_sError, doc_type);
                    }
                case "Sample 8":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_7_8", "FWPXFundChangeParticipantLetterType_7_8", "8.0", ref o_sError, doc_type);
                    }
                case "Sample 8A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_7_8", "FWPXFundChangeParticipantLetterType_7_8", "8.1", ref o_sError, doc_type);
                    }
                case "Sample 8.1":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_7_8", "FWPXFundChangeParticipantLetterType_7_8", "81.0", ref o_sError, doc_type);
                    }
                case "Sample 8.1A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_7_8", "FWPXFundChangeParticipantLetterType_7_8", "81.1", ref o_sError, doc_type);
                    }
                case "Sample 12":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_12", "12.0", ref o_sError, doc_type);
                    }
                case "Sample 12A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_12", "12.1", ref o_sError, doc_type);
                    }
                case "Sample 12B":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_12", "12.2", ref o_sError, doc_type);
                    }
                case "Sample 12C":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_12", "12.3", ref o_sError, doc_type);
                    }
                case "Sample 13":
                case "Sample 13A":
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_13", "FWPXFundChangeParticipantLetterType_13", "13.0", ref o_sError, doc_type);
                    }

                default:
                    {
                        return PXSponsorLetterDriver("FWPXFundChangeSponsorLetterType_2_3_4_5_6", "FWPXFundChangeParticipantLetterType_2", "2.0", ref o_sError, doc_type);
                    }
            }

        }
        private string PXSponsorLetterDriver(string sSponsorDoc, string sParticipantDoc, string sType, ref string o_sError, string doc_type = "doc")
        {
            string sReturnFilePathNname = "";
            Document doc1;
            Document doc2;
            DocumentBuilder builder1;
            DocumentBuilder builder2;
            string sFileName = "";
            var tblHdtInfo = new DataTable("HdrInfo");

            try
            {
                SetLicense();
                var argoTable = _oFw.NewFunds;
                objFwDocGen.removeBaddata(ref argoTable);
                var argoTable1 = _oFw.NewFundsCustomPX;
                objFwDocGen.removeBaddata(ref argoTable1);
                doc1 = new Document(TemplatePath + sSponsorDoc + ".doc");
                doc2 = new Document(TemplatePath + sParticipantDoc + ".doc");

                builder1 = new DocumentBuilder(doc1);
                builder2 = new DocumentBuilder(doc2);
                var tables = builder2.Document.GetChildNodes(NodeType.Table, true);
                if (chkDocSample(sType))
                {
                    if (_oFw.Action == 2)     // delete only
                    {
                        tables[3].Remove();      // foot note for cost
                        tables[2].Remove();      // foot note for NAV - PAA
                        tables[1].Remove();      // fund addition table for NAV
                        tables[0].Remove();      // fund addition table for GAC

                        builder2.MoveToMergeField("fund_notes");
                        builder2.Write("");
                    }
                    else if (IsNavPaaPlan())
                    {
                        tables[1].Remove();      // fund addition table for NAV
                    }
                    else if (IsNavPlan())
                    {
                        tables[2].Remove();      // foot note for NAV - PAA
                        tables[0].Remove();      // fund addition table for GAC
                    }
                    else
                    {
                        tables[2].Remove();      // foot note for NAV - PAA
                        tables[1].Remove();
                    }      // fund addition table for NAV
                }

                o_sError = "";

                bldPXHdrInfo(tblHdtInfo);

                doc1.MailMerge.Execute(tblHdtInfo);
                doc2.MailMerge.Execute(tblHdtInfo);

                string sTxt1 = "In accordance with your request we will make the following Investment Choice changes to the above-named retirement Plan and Contract:";
                builder1.MoveToMergeField("filler_add_fund");

                int wW;
                wW = 300;
                if (_oFw.Action == 2)
                {
                    sTxt1 = "In accordance with your request we will make the following Investment Choice changes to the above-named retirement Plan and Contract:";
                }
                builder1.Writeln(sTxt1);
                builder1.Writeln();

                var dvNew = new DataView(_oFw.NewFunds, "action=1", "", DataViewRowState.CurrentRows);
                if (dvNew.Count > 0)
                {
                    // Sponsor letter
                    builder1.Write("We will ");
                    builder1.Bold = true;
                    builder1.Write("add");
                    builder1.Bold = false;
                    builder1.Writeln(" the following Investment Choice(s):");
                    builder1.Writeln();
                    builder1.ParagraphFormat.LeftIndent = 0.3d * 72d;
                    builder1.StartTable();
                    foreach (DataRowView r in dvNew)
                    {
                        WriteRowL(builder1, Convert.ToString(r["fund_name"]), 350);
                    }

                    builder1.EndTable();
                    builder1.Writeln();
                    builder1.ParagraphFormat.LeftIndent = 0d;

                }
                var dvDel = new DataView(_oFw.NewFunds, "action=2", "", DataViewRowState.CurrentRows);
                if (dvDel.Count > 0)
                {
                    if (_oFw.Action == 2) // delete only
                    {
                        builder1.Writeln("The following Investment Choice(s) will be deleted on or about the estimated Effective Date.");
                    }
                    else
                    {
                    }

                    builder1.Write("We will ");
                    builder1.Bold = true;
                    builder1.Write("delete");
                    builder1.Bold = false;
                    builder1.Writeln(" the following Investment Choice(s) and transfer any assets as shown: ");
                    builder1.Writeln();
                    builder1.ParagraphFormat.LeftIndent = 0.3d * 72d;
                    builder1.StartTable();
                    WriteRowL(builder1, "Deleted Investment Choice(s)", "Assets Transferred to New/Existing Investment Choice(s)", wW, true);


                    foreach (DataRowView r in dvDel)
                    {
                        WriteRowL(builder1, Convert.ToString(r["fund_name"]), Convert.ToString(r["to_fund_name"]), wW, false);
                    }

                    builder1.EndTable();
                    builder1.ParagraphFormat.LeftIndent = 0d;
                }

                builder1.MoveToMergeField("filler_pm");
                builder1.Write("Transamerica Retirement Solutions");

                getSponsorLetterPXinfo(builder1, sType);
                getParticipantLetterPXMessage(builder2, doc2, sType, Convert.ToString(tblHdtInfo.Rows[0]["estimated_effective_date"]));

                if (chkDocSample(sType))
                {
                    BuildPptSection(builder2, wW, ref o_sError);
                }
                if (_oFw.Action != 2)
                {
                    var dtAddFunds = getAddedFundsInfo(builder2);
                    doc2.MailMerge.ExecuteWithRegions(dtAddFunds);
                }

                BuildPptSectionPx21(doc2, tblHdtInfo, ref o_sError);
                if (!string.IsNullOrEmpty(o_sError))
                {
                    return "";
                }

                MakeTbBorder(doc2.Sections[doc2.Sections.Count - 1].Body.Tables[doc2.Sections[doc2.Sections.Count - 1].Body.Tables.Count - 1]);

                doc1.AppendDocument(doc2, ImportFormatMode.KeepSourceFormatting);
                if (doc_type == "doc")
                {
                    sFileName = "SponsorNoticePPTLetter" + _oFw.CaseNo + ".doc";
                    doc1.Save(LocalPath + sFileName);
                    objFwDocGen.CopyFileToRemote(LocalPath + sFileName, OutputPath + sFileName);
                    _oFw.InsertTaskSponsorNPartLetter(OutputPath, sFileName);
                }
                else
                {
                    string sLetterWord = "SponsorNoticePPTLetter" + _oFw.CaseNo + ".doc";
                    doc1.Save(LocalPath + sLetterWord);
                    objFwDocGen.CopyFileToRemote(LocalPath + sLetterWord, OutputPath + sLetterWord);
                    TARSharedUtilLibBFLBLL.FWUtils.AddPdfRow(_oFw.PdfHeader, TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_SponsorPPTPLetterFile, OutputPath + sLetterWord, OutputPath + sLetterWord);

                    sFileName = "SponsorNoticePPTLetter" + _oFw.CaseNo + ".pdf";
                    doc1.Save(OutputPath + sFileName, Aspose.Words.SaveFormat.Pdf);
                    _oFw.InsertTaskSponsorNPartLetter(OutputPath, sFileName);
                }
                sReturnFilePathNname = OutputPath + sFileName;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                o_sError = ex.Message;
                throw;
            }
            return sReturnFilePathNname;
        }
        private void bldPXHdrInfo(DataTable tblHdtInfo)
        {
            tblHdtInfo.Columns.Add(new DataColumn("company_name", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("primary_addr1", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("primary_city_n_state", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("plan_name", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("contract_id", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("primary_first_name", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("primary_last_name", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("current_date", typeof(string)));

            tblHdtInfo.Columns.Add(new DataColumn("transfer_date", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("project_manager", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("plan_admin", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("plan_admin_addr1", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("plan_admin_city_n_state", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("estimated_effective_date", typeof(string)));
            tblHdtInfo.Columns.Add(new DataColumn("GACNAV_no", typeof(string)));

            DataRow r1;
            r1 = tblHdtInfo.NewRow();
            r1["company_name"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("company_name", _oFw.PdfHeader)[0];
            r1["plan_name"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("plan_name", _oFw.PdfHeader)[0];
            r1["primary_addr1"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("primary_addr1", _oFw.PdfHeader)[0];
            r1["primary_city_n_state"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("primary_city_n_state", _oFw.PdfHeader)[0];
            r1["primary_first_name"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("primary_first_name", _oFw.PdfHeader)[0];
            r1["primary_last_name"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("primary_last_name", _oFw.PdfHeader)[0];
            r1["contract_id"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("contract_id", _oFw.PdfHeader)[0];
            r1["current_date"] = DateTime.Today.ToString("MMMM dd, yyyy");

            r1["transfer_date"] = "";
            r1["project_manager"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("project_manager", _oFw.PdfHeader)[0];
            r1["plan_admin"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("plan_admin", _oFw.PdfHeader)[0];
            r1["plan_admin_addr1"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("plan_admin_addr1", _oFw.PdfHeader)[0];
            r1["plan_admin_city_n_state"] = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData("plan_admin_city_n_state", _oFw.PdfHeader)[0];

            if (_oFw.PegasysDate == null || !DateTime.TryParse(_oFw.PegasysDate.ToString(), out _))
            {
                r1["estimated_effective_date"] = "";
            }
            else
            {
                r1["estimated_effective_date"] = Convert.ToDateTime(_oFw.PegasysDate).ToString("MMMM dd, yyyy");
            }
            r1["GACNAV_no"] = "(800) 401-8726";
            tblHdtInfo.Rows.Add(r1);
        }
        private bool chkDocSample(string sDocSampleType)
        {
            switch (sDocSampleType ?? "")
            {
                case "7.0":
                case "7.1":
                    {
                        return false;
                    }
                case "8.0":
                case "8.1":
                    {
                        return false;
                    }
                case "81.0":
                case "81.1":
                    {
                        return false;
                    }
                case "13.0":
                    {
                        return false;
                    }

                default:
                    {
                        return true;
                    }
            }

        }
        private void getSponsorLetterPXinfo(DocumentBuilder builder, string sType)
        {

            if (chkDocSample(sType))
            {
                builder.MoveToMergeField("custom_PX");
                if (Convert.ToDecimal(sType) > 4m)
                {
                    builder.Write("and Custom PortfolioXpress ");
                }
                else
                {
                    builder.Write("");
                }
                builder.MoveToMergeField("Custom");
                if (Convert.ToDecimal(sType) > 4m)
                {
                    builder.Write("Custom");
                }
                else
                {
                    builder.Write("");
                }
            }
            builder.MoveToMergeField("PX_Information");

            string sTemp0 = "Investment Choice changes may impact your plan’s PortfolioXpress line up and asset allocation models.  Updates and more information regarding PortfolioXpress are provided in the enclosed Important Notice to Participants.";
            string sTemp1 = "For those participants who are currently subscribed to the service, the Plan’s fiduciary directs and instructs Transamerica to implement the following:    ";
            string sTemp2 = "participants from the service prior to making these changes.  Participants wishing to recommence the PortfolioXpress service after implementation of these changes will need to ";
            string sTemp3 = "Make the Investment Choice changes indicated above, and reallocate participants’ accounts based on these changes.  Allocate Participant accounts to the new portfolios on their currently scheduled next rebalancing date.  ";
            string sTemp4 = "By directing Transamerica to take this action, the Plan agrees to indemnify Transamerica and its affiliates for any claims which may be brought against, or losses incurred by, " + "Transamerica and its affiliates as a result of not obtaining the prior consent of these participants before reallocating their portfolios based on these instructions, so long as such claims or losses are not based on Transamerica’s negligence in implementing these instructions.";
            string sTemp5 = "Your request did not include any instructions to change the Investment Choices associated with the plan’s PortfolioXpress line up.  Therefore, the changes indicated above will have no impact on the PortfolioXpress asset allocation models.  ";

            bool bTmpISXPxQDIA = false;
            string sEffDate = "Estimated Effective Date";
            string sISCPxQTIATxt = "";
            string sISCPxQTIATxt1 = "";

            if (_oFw.PartnerId == "ISC" && TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_new, _oFw.PdfHeader)[0] == "-1") // fund_id -1 = portfolioXpress
            {
                bTmpISXPxQDIA = true;
                sISCPxQTIATxt = "By designating PortfolioXpress as the plan’s default investment, the Plan's Fiduciary directs Transamerica Retirement Solutions to move assets in all participants' accounts in the Plan, who don't have investment elections on file with us, into the applicable PortfolioXpress allocation models.  The affected participants' current balances will be invested in PortfolioXpress as of the ";

                sISCPxQTIATxt1 = " based on using an assumed age of 65 for the defaulted participants' retirement date.  Future deferrals will also be invested in this manner.";
            }

            switch (sType ?? "")
            {
                case "2.0":
                case "2.1":
                    {
                        builder.Writeln(sTemp0);
                        builder.Writeln("");
                        builder.Write("");
                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        break;
                    }


                case "3.0":
                case "3.1":
                    {
                        builder.Writeln(sTemp0);
                        builder.Writeln("");
                        builder.ParagraphFormat.KeepTogether = true;
                        builder.Writeln(sTemp1);
                        builder.Writeln("");

                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        builder.Bold = true;
                        builder.Write("Un-subscribe/remove ");
                        builder.Bold = false;
                        builder.Writeln(sTemp2);

                        builder.Bold = true;
                        builder.Write("re-subscribe");
                        builder.Bold = false;
                        builder.Write(" online after the change is completed.  ");
                        break;
                    }
                case "4.0":
                case "4.1":
                    {
                        builder.Writeln(sTemp0);
                        builder.Writeln("");
                        builder.Writeln(sTemp1);
                        builder.Writeln("");

                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        builder.Writeln(sTemp3);
                        builder.Writeln("");
                        builder.Bold = true;
                        builder.Write("Indemnification: ");
                        builder.Bold = false;
                        builder.Writeln(sTemp4);
                        break;
                    }
                case "5.0":
                case "5.1":
                case "5.2":
                case "5.3":
                    {
                        builder.Writeln(sTemp0);
                        builder.Writeln("");

                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        builder.Write("");
                        writePXAddDeleteFund(builder);
                        break;
                    }
                case "6.0":
                case "6.1":
                case "6.2":
                case "6.3":
                    {
                        builder.Writeln(sTemp0);
                        builder.Writeln("");
                        writePXAddDeleteFund(builder);
                        builder.Writeln(sTemp1);
                        builder.Writeln("");

                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        builder.Bold = true;
                        builder.Write("Un-subscribe/remove ");
                        builder.Bold = false;
                        builder.Writeln(sTemp2);
                        builder.Bold = true;
                        builder.Write("re-subscribe");
                        builder.Bold = false;
                        builder.Write(" online after the change is completed.  ");
                        break;
                    }
                case "61.0":
                case "61.1":
                case "61.2":
                case "61.3":
                    {
                        builder.Writeln(sTemp0);
                        builder.Writeln("");
                        writePXAddDeleteFund(builder);
                        builder.Writeln(sTemp1);
                        builder.Writeln("");

                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        builder.Writeln("");
                        builder.Writeln(sTemp3);
                        builder.Bold = true;
                        builder.Write("Indemnification: ");
                        builder.Bold = false;
                        builder.Writeln(sTemp4);
                        break;
                    }
                case "12.0":
                case "12.1":
                case "12.2":
                case "12.3":
                    {
                        builder.Writeln(sTemp5);
                        builder.Writeln("");

                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        builder.Write("");
                        break;
                    }
                case "7.0":
                case "7.1":
                    {
                        writePXAddDeleteFund(builder, false);
                        builder.MoveToMergeField("PX_Information2");
                        builder.Write("");
                        builder.Writeln("");

                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        builder.Write("");
                        break;
                    }
                case "8.0":
                case "8.1":
                    {
                        writePXAddDeleteFund(builder, false);
                        builder.MoveToMergeField("PX_Information2");
                        builder.Writeln(sTemp1);
                        builder.Writeln("");

                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        builder.Bold = true;
                        builder.Write("Un-subscribe/remove ");
                        builder.Bold = false;
                        builder.Writeln(sTemp2);
                        builder.Bold = true;
                        builder.Write("re-subscribe");
                        builder.Bold = false;
                        builder.Write(" online after the change is completed.  ");
                        break;
                    }
                case "81.0":
                case "81.1":
                    {
                        writePXAddDeleteFund(builder, false);
                        builder.MoveToMergeField("PX_Information2");
                        builder.Writeln(sTemp1);
                        builder.Writeln("");

                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                        }

                        builder.MoveToMergeField("message_indent");
                        builder.Writeln(sTemp3);
                        builder.Writeln("");
                        builder.Bold = true;
                        builder.Write("Indemnification: ");
                        builder.Bold = false;
                        builder.Writeln(sTemp4);
                        break;
                    }
                case "13.0":
                    {
                        if (bTmpISXPxQDIA)
                        {
                            builder.Write(sISCPxQTIATxt);
                            builder.Bold = true;
                            builder.Write(sEffDate);
                            builder.Bold = false;
                            builder.Write(sISCPxQTIATxt1);
                            builder.Writeln("");
                            builder.MoveToMergeField("message_indent");
                            builder.Bold = true;
                            builder.Write("Indemnification: ");
                            builder.Bold = false;
                            builder.Writeln(sTemp4);
                            builder.Writeln("");
                        }
                        string sName = "Default Alternative";
                        bool bQdia = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_qdia_answer, _oFw.PdfHeader)[0] == "Yes";
                        if (bQdia)
                        {
                            sName = "Qualified Default Investment Alternative (QDIA)";
                        }
                        else
                        {
                            sName = "Default Alternative";
                        }
                        builder.MoveToMergeField("QDIA_PX");
                        builder.Write(sName);
                        break;
                    }
            }
            builder.MoveToMergeField("QDIA_notice");
            switch (sType ?? "")
            {
                case "2.0":
                case "3.0":
                case "4.0":
                case "5.0":
                case "5.2":
                case "6.0":
                case "6.2":
                case "61.0":
                case "61.2":
                case "12.0":
                case "12.2":
                    {
                        SponsorQDIAmessage(builder);
                        break;
                    }

                default:
                    {
                        builder.Write("");
                        break;
                    }
            }
        }
        private void SponsorQDIAmessage(DocumentBuilder builder)
        {
            builder.Writeln();
            builder.Bold = true;
            builder.Italic = true;
            builder.Write("QDIA Annual Participant Notice. ");
            builder.Bold = false;
            builder.Italic = false;
            builder.Write("As a result of the changes to your Qualified Default Investment Alternative (QDIA), an updated Annual Participant Notice will be delivered to the Plan Sponsor’s inbox at the Message Center on ");

            builder.Font.Color = System.Drawing.Color.Blue;
            builder.Font.Underline = Underline.Single;
            builder.InsertHyperlink("www.TA-Retirement.com", "http://www.TA-Retirement.com/default.aspx?pid=message", false);
            builder.Font.Color = System.Drawing.Color.Black;
            builder.Font.Underline = Underline.None;
            builder.Writeln(" with instructions for distribution.  Please ensure that this notice is distributed to all eligible employees no less than 30 days before the Effective Date of the change to your Qualified Default Investment Alternative. ");

        }
        private string GetDocRulesInputXml()
        {
            string strReturn = "";
            string PX = "N/A";
            string CustomPX = "N/A";
            string AddDeleteFunds = "N/A";
            string CustomPXChanges = "N/A";
            string MaterialChange = "N/A";
            string Indemnify = "N/A";
            string Unsubscribe = "N/A";
            string QDIAChange = "N/A";
            string ThirdPartyInvFiduciary = "N/A";
            var strTmp = new string[2];
            // 
            if (!(_oFw.NewFunds == null) && _oFw.NewFunds.Rows.Count > 0)
            {
                AddDeleteFunds = "Yes";
            }
            else
            {
                AddDeleteFunds = "No";
            }

            bool bQdia = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_qdia_answer, _oFw.PdfHeader)[0] == "Yes";
            if (bQdia == true)
            {
                QDIAChange = "Yes";
            }
            else
            {
                QDIAChange = "No";
            }

            if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_portXpress_selected, _oFw.PdfHeader)[0] != "true")
            {
                PX = "No";
            }
            else
            {
                PX = "Yes";
                // 
                if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_custom, _oFw.PdfHeader)[0] == "true")
                {
                    CustomPX = "Yes";

                    if (!(_oFw.NewFundsCustomPX == null) && _oFw.NewFundsCustomPX.Rows.Count > 0)
                    {
                        CustomPXChanges = "Yes";
                    }
                    else
                    {
                        CustomPXChanges = "No";
                    }
                }
                else
                {
                    CustomPX = "No";
                }
                // 
                if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_is_material, _oFw.PdfHeader)[0] == "Yes" || TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_is_material_qdia, _oFw.PdfHeader)[0] == "Yes" || TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_is_material_custom, _oFw.PdfHeader)[0] == "Yes")

                {

                    MaterialChange = "Yes";

                    strTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_changeauthorization_type, _oFw.PdfHeader);
                    if (!string.IsNullOrEmpty(strTmp[0]))
                    {
                        if (strTmp[0].ToLower().Trim() == "indemnify")
                        {
                            Indemnify = "Yes";
                            Unsubscribe = "No";
                        }
                        else if (strTmp[0].ToLower().Trim() == "unsubscribe")
                        {
                            Unsubscribe = "Yes";
                            Indemnify = "No";
                        }
                    }
                }
                else
                {
                    MaterialChange = "No";
                }
                // 
                strTmp = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_fiduciary_type, _oFw.PdfHeader);
                if (!string.IsNullOrEmpty(strTmp[0]))
                {
                    if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_fiduciary_type, _oFw.PdfHeader)[0] == "2" || TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_fiduciary_type, _oFw.PdfHeader)[0] == "3")
                    {
                        ThirdPartyInvFiduciary = "Yes";
                    }
                    else
                    {
                        ThirdPartyInvFiduciary = "No";
                    }
                }

            }

            var xEl = new XElement("Rule", new XAttribute("PX", PX), new XAttribute("CustomPX", CustomPX), new XAttribute("AddDeleteFunds", AddDeleteFunds), new XAttribute("CustomPXChanges", CustomPXChanges), new XAttribute("MaterialChange", MaterialChange), new XAttribute("Unsubscribe", Unsubscribe), new XAttribute("Indemnify", Indemnify), new XAttribute("QDIAChange", QDIAChange), new XAttribute("ThirdPartyInvFiduciary", ThirdPartyInvFiduciary));
            strReturn = xEl.ToString();
            if (string.IsNullOrEmpty(strReturn))
            {
                strReturn = "<Rule PX=" + PX + "CustomPX=" + CustomPX + "AddDeleteFunds=" + AddDeleteFunds + "CustomPXChanges=" + CustomPXChanges + "MaterialChange=" + MaterialChange + "Unsubscribe=" + Unsubscribe + "Indemnify=" + Indemnify + "QDIAChange=" + QDIAChange + "ThirdPartyInvFiduciary=" + ThirdPartyInvFiduciary + "></Rule>";
            }
            return strReturn;
        }
        private string GetLetterNoticeType(string inputXml)
        {
            var dt = GetLetterNoticeRules();
            string filter = GetRulesFilter(inputXml);
            DataRow[] rows = dt.Select(filter);
            if (rows.Count() == 1)
            {
                return rows[0]["DocName"].ToString();
            }
            return string.Empty;
        }
        private DataTable GetLetterNoticeRules()
        {
            string fileName = GetFileName();
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }
            else
            {
                var workbook = new Aspose.Cells.Workbook(fileName);
                var worksheet = workbook.Worksheets[0];
                var dataTable = new DataTable();
                dataTable = worksheet.Cells.ExportDataTable(0, 0, worksheet.Cells.MaxRow + 1, worksheet.Cells.MaxColumn + 1, true);
                return dataTable;
            }
        }
        private string GetFileName()
        {
            string fileName = string.Empty;
            fileName = TRS.IT.TrsAppSettings.AppSettings.GetValue("FWLetterNoticeRulesFilePath");

            if (File.Exists(fileName) == false)
            {
                fileName = string.Empty;
            }
            return fileName;
        }
        private string GetRulesFilter(string inputXml)
        {
            string filter = string.Empty;
            var attributes = new List<string>();
            var xDoc = XDocument.Parse(inputXml);

            foreach (XAttribute att in xDoc.Element("Rule").Attributes())
            {
                // If att.Name <> "Id" Then
                // End If
                attributes.Add("(" + att.Name.ToString() + "='N/A'" + " OR " + att.Name.ToString() + "='" + att.Value.ToString() + "')");
            }

            return string.Join(" AND ", attributes);
        }
        internal bool IsNavPaaPlan()
        {
            return _oFw.ContractInfo.KeyValuePairs.Any(x => x.key.ToUpper() == "MDP_PRODUCTID" && x.value == "122");
        }
        internal bool IsNavPlan()
        {
            return _oFw.ContractInfo.KeyValuePairs.Any(x => x.key.ToUpper() == "NAVPRODUCT" && x.value == "1");
        }
        private void WriteRowL(DocumentBuilder a_builder, string a_sFundName, int a_wW)
        {
            // a_builder.ParagraphFormat.Alignment = ParagraphAlignment.Center
            string sFundName = a_sFundName;
            bool bQdia = false;
            string sQdiaText = "<sup>[QDIA]</sup>";
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Horizontal].LineStyle = LineStyle.None;
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Horizontal].LineWidth = 0d;
            a_builder.RowFormat.HeadingFormat = false;
            a_builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineStyle = LineStyle.None;
            a_builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineStyle = LineStyle.None;
            a_builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.Transparent;
            a_builder.Font.Bold = false;
            a_builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            a_builder.CellFormat.HorizontalMerge = Aspose.Words.Tables.CellMerge.None;

            a_builder.InsertCell();
            if (a_sFundName.IndexOf(sQdiaText) > 0)
            {
                sFundName = a_sFundName.Replace(sQdiaText, "");
                bQdia = true;
            }
            a_builder.Write(sFundName);
            if (bQdia)
            {
                double OldFont = a_builder.Font.Size;
                a_builder.Font.Size = 9d;
                a_builder.Font.Superscript = true;
                a_builder.Write("QDIA");
                a_builder.Font.Superscript = false;
                a_builder.Font.Size = OldFont;
            }

            a_builder.CellFormat.Width = a_wW;
            a_builder.EndRow();
        }
        private void BuildPptSectionPx21(Document a_oDoc, DataTable a_tb, ref string o_sError)
        {
            Document doc;
            DocumentBuilder builder;
            try
            {
                SetLicense();
                doc = new Document(TemplatePath + "FWPX21Table.docx");
                builder = new DocumentBuilder(doc);
                builder.InsertBreak(BreakType.PageBreak);
                doc.MailMerge.Execute(a_tb);
                builder.PageSetup.RightMargin = 25d; // 0.8 * 72
                builder.PageSetup.LeftMargin = 25d; // 0.8 * 72
                bool bCustomPx = false;
                if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_custom, _oFw.PdfHeader)[0] == "true")
                {
                    bCustomPx = true;
                }
                var tbPxFunds = new FundWizard(_oFw).GeneratePX21(bCustomPx);
                var tbHeadings = BuildPx21Headings(tbPxFunds);
                DataRow dr;
                string sAsset;
                string sAssetCur;
                string sHeading, sHeadingCur;

                builder.MoveToMergeField("filler_px_data");
                builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                builder.Font.Bold = true;
                builder.Write("Years to Retirement");

                builder.StartTable();
                builder.Font.Size = 8d;

                string[] sArrPercent = [(-20).ToString(), (-15).ToString(), (-10).ToString(), (-5).ToString(), 0.ToString(), 5.ToString(), 10.ToString(), 15.ToString(), 20.ToString(), 22.ToString(), 25.ToString(), 27.ToString(), 30.ToString(), 32.ToString(), 35.ToString(), 37.ToString(), 40.ToString(), 42.ToString(), 45.ToString(), 50.ToString(), 55.ToString()];

                string[] sArrPercentEmpty;

                sArrPercentEmpty = (string[])sArrPercent.Clone();
                Array.Clear(sArrPercentEmpty, 0, sArrPercentEmpty.Length);

                WriteRowPX21(builder, "Conservative", sArrPercent, 1);

                sArrPercent = null;
                sArrPercent = [(-30).ToString(), (-25).ToString(), (-20).ToString(), (-15).ToString(), (-10).ToString(), (-5).ToString(), 0.ToString(), 5.ToString(), 10.ToString(), 12.ToString(), 15.ToString(), 17.ToString(), 20.ToString(), 22.ToString(), 25.ToString(), 27.ToString(), 30.ToString(), 32.ToString(), 35.ToString(), 40.ToString(), 45.ToString()];
                WriteRowPX21(builder, "Moderate", sArrPercent, 1);
                sArrPercent = null;
                sArrPercent = [(-35).ToString(), (-30).ToString(), (-25).ToString(), (-20).ToString(), (-15).ToString(), (-10).ToString(), (-5).ToString(), 0.ToString(), 5.ToString(), 7.ToString(), 10.ToString(), 12.ToString(), 15.ToString(), 17.ToString(), 20.ToString(), 22.ToString(), 25.ToString(), 27.ToString(), 30.ToString(), 35.ToString(), 40.ToString()];
                WriteRowPX21(builder, "Aggressive", sArrPercent, 1);

                sHeading = string.Empty;
                sHeadingCur = sHeading;
                sAsset = string.Empty;
                sAssetCur = sAsset;

                foreach (DataRow r in tbPxFunds.Rows)
                {
                    sHeadingCur = Convert.ToString(r["asset_group"]);
                    if ((sHeadingCur ?? "") != (sHeading ?? ""))
                    {
                        sHeading = sHeadingCur;
                        dr = tbHeadings.Rows.Find(sHeadingCur);
                        if (!(dr == null))
                        {
                            BuildPx21Array(sArrPercent, dr);
                            WriteRowPX21(builder, "", sArrPercentEmpty, 4);
                            WriteRowPX21(builder, sHeadingCur, sArrPercent, 2);
                        }
                    }

                    sAssetCur = r["asset_name"].ToString() + (string.IsNullOrEmpty(r["sub_asset_name"].ToString()) ? "" : "-" + r["sub_asset_name"].ToString());
                    if ((sAssetCur ?? "") != (sAsset ?? ""))
                    {
                        sAsset = sAssetCur;
                        dr = tbHeadings.Rows.Find(r["asset_id"].ToString() + r["sub_asset_id"].ToString());
                        if (!(dr == null))
                        {
                            WriteRowPX21(builder, "", sArrPercentEmpty, 6);
                            BuildPx21Array(sArrPercent, dr);
                            WriteRowPX21(builder, sAssetCur, sArrPercent, 3);
                        }
                    }
                    BuildPx21Array(sArrPercent, r);
                    WriteRowPX21(builder, Convert.ToString(r["fund_name"]), sArrPercent, 5);

                }
                builder.EndTable();

                foreach (Section srS in doc)
                {
                    Section newSection = (Section)a_oDoc.ImportNode(srS, true, ImportFormatMode.KeepSourceFormatting);
                    a_oDoc.LastSection.AppendContent(newSection);
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                o_sError = ex.Message;
                throw;
            }


        }
        private void BuildPptSection(DocumentBuilder a_builder, int a_wW, ref string o_sError)
        {
            // px ppt section
            try
            {
                // ppt letter
                a_builder.MoveToMergeField("ppt_add_funds");
                switch (_oFw.Action) // add and add & delete
                {
                    case 1:
                    case 3:
                    case 8:
                        {
                            a_builder.Write("We will ");
                            a_builder.Font.Bold = true;
                            a_builder.Write("add ");
                            a_builder.Font.Bold = false;
                            a_builder.Writeln("the following Investment Choice(s):");
                            a_builder.Writeln();
                            a_builder.ParagraphFormat.LeftIndent = 0d;
                            a_builder.Font.Bold = false;
                            a_builder.MoveToMergeField("filler_current_invest_txt");
                            break;
                        }

                    default:
                        {
                            a_builder.Write("");
                            break;
                        }
                }
                a_builder.MoveToMergeField("ppt_del_funds");
                switch (_oFw.Action)
                {
                    case 2:
                    case 3:
                    case 8: // delete and add & delete
                        {
                            a_builder.Write("We will ");
                            a_builder.Font.Bold = true;
                            a_builder.Write("delete ");
                            a_builder.Font.Bold = false;
                            a_builder.Writeln("the following Investment Choice(s) and transfer any assets as shown: ");
                            a_builder.Writeln();
                            // a_builder.ParagraphFormat.LeftIndent = 0.3 * 72
                            // a_builder.StartTable()
                            var dvDel = new DataView(_oFw.NewFunds, "action=2", "", DataViewRowState.CurrentRows);

                            foreach (DataRowView r in dvDel)
                            {
                                a_builder.Writeln("       " + r["fund_name"].ToString());
                            }
                            // a_builder.EndTable()
                            a_builder.Writeln();
                            a_builder.ParagraphFormat.LeftIndent = 0d;

                            // Transfer
                            a_builder.Write("On or about the ");
                            a_builder.Font.Bold = true;
                            a_builder.Write("Estimated Effective Date");
                            a_builder.Font.Bold = false;
                            a_builder.Writeln(", existing account balances and future contribution allocations will automatically be transferred from the deleted Investment Choice to the new Investment Choice as shown in the schedule below:");
                            a_builder.Writeln();
                            a_builder.ParagraphFormat.LeftIndent = 0.3d * 72d;
                            a_builder.StartTable();
                            WriteRowL(a_builder, "Deleted Investment Choice(s)", "Assets Transferred to New/Existing Investment Choice(s)", a_wW - 50, true);
                            foreach (DataRowView r in dvDel)
                            {
                                WriteRowL(a_builder, Convert.ToString(r["fund_name"]), Convert.ToString(r["to_fund_name"]), a_wW - 50, false);
                            }

                            a_builder.EndTable();
                            // a_builder.Writeln()
                            a_builder.ParagraphFormat.LeftIndent = 0d;
                            a_builder.MoveToMergeField("filler_transfer_txt");
                            a_builder.Font.Bold = true;
                            a_builder.Font.Italic = true;
                            a_builder.Write("");
                            // a_builder.Writeln("If you want your account balance and future contribution allocations transferred as shown in the table above, you do not need to do anything.")
                            a_builder.Font.Bold = false;
                            a_builder.Font.Italic = false;
                            a_builder.Writeln();
                            a_builder.MoveToMergeField("filler_current_invest_txt");
                            // a_builder.Write("Whether or not you currently invest in any of the funds being eliminated, you may make transfers to or from, or allocate future contributions to any of the new investment options on or about the estimated Effective Date, in accordance with the terms of the Plan.")
                            a_builder.Write("");
                            break;
                        }

                    default:
                        {
                            a_builder.Write("");
                            a_builder.MoveToMergeField("filler_transfer_txt");
                            a_builder.Write("");
                            break;
                        }

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                o_sError = ex.Message;
                throw;
            }
        }
        private void WriteRowL(DocumentBuilder a_builder, string a_sFundDel, string a_sFundTransfer, int a_wW, bool a_bBold)
        {
            string sFundName;
            bool bQdia = false;
            string sQdiaText = "<sup>[QDIA]</sup>";
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Horizontal].LineStyle = LineStyle.None;
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Horizontal].LineWidth = 0d;
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Bottom].LineStyle = LineStyle.None;
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Bottom].LineWidth = 0d;
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Top].LineStyle = LineStyle.None;
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Top].LineWidth = 0d;
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Horizontal].Color = System.Drawing.Color.Transparent;
            a_builder.RowFormat.HeadingFormat = false;
            a_builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineStyle = LineStyle.None;
            a_builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineStyle = LineStyle.None;
            a_builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.Transparent;
            a_builder.Font.Bold = a_bBold;
            a_builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            a_builder.CellFormat.HorizontalMerge = Aspose.Words.Tables.CellMerge.None;

            a_builder.InsertCell();
            // a_builder.Write(a_sFundDel)
            if (a_sFundDel.IndexOf(sQdiaText) > 0)
            {
                sFundName = a_sFundDel.Replace(sQdiaText, "");
                bQdia = true;
            }
            else
            {
                sFundName = a_sFundDel;
            }
            a_builder.Write(sFundName);
            if (bQdia)
            {
                double OldFont = a_builder.Font.Size;
                a_builder.Font.Size = 9d;
                a_builder.Font.Superscript = true;
                a_builder.Write("QDIA");
                a_builder.Font.Superscript = false;
                a_builder.Font.Size = OldFont;
            }
            a_builder.CellFormat.Width = a_wW;
            a_builder.InsertCell();
            bQdia = false;
            if (a_sFundTransfer.IndexOf(sQdiaText) > 0)
            {
                sFundName = a_sFundTransfer.Replace(sQdiaText, "");
                bQdia = true;
            }
            else
            {
                sFundName = a_sFundTransfer;
            }
            a_builder.Write(sFundName);
            if (bQdia)
            {
                double OldFont = a_builder.Font.Size;
                a_builder.Font.Size = 9d;
                a_builder.Font.Superscript = true;
                a_builder.Write("QDIA");
                a_builder.Font.Superscript = false;
                a_builder.Font.Size = OldFont;
            }
            a_builder.CellFormat.Width = a_wW;

            a_builder.EndRow();
        }
        public DataTable getAddedFundsInfo(DocumentBuilder builder)
        {
            string sASofDate = "";
            var oMyDate = new DateTime(2012, 5, 31);
            var oFundInfoSoa = new SOA.FundInfoSoa();
            string fundLineupXml = oFundInfoSoa.GetFMRSFundCategory();
            var asOfDate = GetAsOfDate(fundLineupXml);

            var sNewFund = new string[_oFw.NewFunds.Rows.Count + 1];
            var xFundList = new XElement("FundList");
            XElement el;

            for (short iI = 0, loopTo = (short)(_oFw.NewFunds.Rows.Count - 1); iI <= loopTo; iI++)
            {
                if ((_oFw.NewFunds.Rows[iI]["action"] as int?) != 2)
                {
                    sNewFund[iI] = _oFw.NewFunds.Rows[iI][0].ToString();
                    el = new XElement("Fund", new XAttribute("FundID", _oFw.NewFunds.Rows[iI][0].ToString()), new XAttribute("Status", _oFw.NewFunds.Rows[iI]["action"]), new XAttribute("StartDate", asOfDate.ToString("yyyy-MM-ddTHH:mm:ssZ")));
                    xFundList.Add(el);
                }
            }
            string sApplicationID = "5129";
            // If _oFw.IsNAV Then sApplicationID = "5145"
            var xEl = new XElement("FMRS", new XAttribute("Type", "FundLineup"), new XAttribute("AsOfDate", asOfDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), new XAttribute("LineupDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")), new XElement("Contract", new XAttribute("ContractID", _oFw.ContractId), new XAttribute("SubID", _oFw.SubId)), new XElement("User", new XAttribute("UsrName", Environment.UserName)), new XElement("Application", new XAttribute("ApplicationID", sApplicationID)));
            xEl.Add(xFundList);

            var dsFundList = oFundInfoSoa.GetFMRSFundsDataset(xEl.ToString(), 0, sNewFund);
            if (!(dsFundList == null) && dsFundList.Tables.Count > 0)
            {
                dsFundList.Tables[0].TableName = "AddFunds";
                if (dsFundList.Tables[0].Rows.Count > 0)
                {
                    if (DateTime.TryParse(dsFundList.Tables[0].Rows[0]["AsOfDate"]?.ToString(), out DateTime asOfDates))
                    {
                        sASofDate = asOfDates.ToString("MMMM dd, yyyy");
                    }
                }
            }

            builder.MoveToMergeField("as_of_date");
            builder.Write(sASofDate);

            builder.MoveToMergeField("fund_notes");
            if (dsFundList.Tables[1].Rows.Count > 0)
            {
                bldLetterNoteHeader(builder);
                foreach (DataRow oRow in dsFundList.Tables[1].Rows)
                {
                    bldLetterNoteRow(builder, oRow[2].ToString(), oRow[1].ToString());
                }

                bldLetterNoteEnd(builder);
            }
            else
            {
                builder.Write("");
            }

            return dsFundList.Tables[0];

        }
        private DateTime GetAsOfDate(string fundLineupXml)
        {
            var xDoc = XDocument.Parse(fundLineupXml);
            var perfDates = (from period in xDoc.Descendants("Period")
                             where period.Parent.Parent.Attribute("ReportID").Value == "1003"
                             orderby DateTime.Parse(period.Attribute("Label").Value) descending
                             select DateTime.Parse(period.Attribute("Label").Value)).ToList();

            return perfDates.FirstOrDefault();
        }
        private void bldLetterNoteHeader(DocumentBuilder builder)
        {
            builder.StartTable();

            builder.InsertCell();

            builder.Font.Size = 8d;
            builder.RowFormat.Borders[Aspose.Words.BorderType.Top].LineWidth = 0.4d;
            builder.RowFormat.Borders[Aspose.Words.BorderType.Vertical].LineWidth = 0.4d;
            builder.RowFormat.HeadingFormat = false;

            builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.Transparent;
            builder.Font.Bold = false;
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            builder.CellFormat.HorizontalMerge = Aspose.Words.Tables.CellMerge.None;
            builder.CellFormat.Width = 350d;

            builder.Write("Notes (as applicable, based on the information provided in \"Shareholder-Type Fees/Comments\"):");
            builder.EndRow();
        }
        private void bldLetterNoteRow(DocumentBuilder builder, string sColumn1, string sColumn2)
        {
            builder.InsertCell();
            builder.Font.Size = 8d;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Top].LineWidth = (double)LineStyle.None;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Bottom].LineWidth = 0.4d;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineStyle = LineStyle.None;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineWidth = 0.4d;
            builder.CellFormat.Width = 15d;
            builder.Write(sColumn1);

            builder.InsertCell();
            builder.Font.Size = 8d;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Top].LineWidth = (double)LineStyle.None;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Bottom].LineWidth = 0.4d;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineWidth = 0.4d;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineStyle = LineStyle.None;

            builder.CellFormat.Width = 335d;
            builder.Write(sColumn2);
            builder.EndRow();
        }
        private void bldLetterNoteEnd(DocumentBuilder builder)
        {
            builder.InsertCell();
            builder.CellFormat.Borders[Aspose.Words.BorderType.Top].LineWidth = 0.4d;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Bottom].LineStyle = LineStyle.None;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineStyle = LineStyle.None;
            builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineStyle = LineStyle.None;
            builder.CellFormat.Width = 350d;
            builder.Write("");
            builder.EndRow();
            builder.EndTable();
        }
        private void WriteRowPX21(DocumentBuilder a_builder, string a_sFundName, string[] Px21, int a_iRowType)
        {
            // a_builder.ParagraphFormat.Alignment = ParagraphAlignment.Center

            bool bFundBold = false;
            bool bValueFold = false;
            string sFiller = "";
            string sPercent = "%";
            string sEmpty = "-";

            int iCnt = Px21.GetUpperBound(0);

            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Horizontal].LineStyle = LineStyle.Single;
            a_builder.RowFormat.Borders[Aspose.Words.BorderType.Horizontal].LineWidth = 0.5d;
            a_builder.RowFormat.HeadingFormat = false;

            a_builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineStyle = LineStyle.Single;
            a_builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineStyle = LineStyle.Single;
            a_builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineWidth = 0.5d;
            a_builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineWidth = 0.5d;

            a_builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.Transparent;
            a_builder.CellFormat.RightPadding = 0d;
            a_builder.CellFormat.LeftPadding = 0d;

            // check for rowtype
            switch (a_iRowType)
            {
                case 1: // hdr risk row
                    {
                        bFundBold = true;
                        sPercent = "";
                        break;
                    }
                case 2: // rollup group row
                    {
                        bFundBold = true;
                        bValueFold = true;
                        break;
                    }
                case 3: // Assset row
                    {
                        bFundBold = true;
                        sFiller = "  ";
                        break;
                    }
                case 4:
                    {
                        bFundBold = true;
                        sFiller = "     ";
                        a_builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineStyle = LineStyle.None;
                        a_builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineStyle = LineStyle.None;
                        a_builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineWidth = 0d;
                        a_builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineWidth = 0d;
                        a_builder.CellFormat.Shading.BackgroundPatternColor = System.Drawing.Color.LightGray;
                        sEmpty = "";
                        break;
                    }
                case 5: // fund row
                    {
                        sFiller = "   ";
                        break;
                    }
                case 6: // Empty Blank row
                    {
                        bFundBold = true;
                        sFiller = "     ";
                        a_builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineStyle = LineStyle.None;
                        a_builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineStyle = LineStyle.None;
                        a_builder.CellFormat.Borders[Aspose.Words.BorderType.Right].LineWidth = 0d;
                        a_builder.CellFormat.Borders[Aspose.Words.BorderType.Left].LineWidth = 0d;
                        sEmpty = "";
                        break;
                    }
            }



            a_builder.CellFormat.HorizontalMerge = Aspose.Words.Tables.CellMerge.None;

            a_builder.Font.Bold = bFundBold;
            a_builder.InsertCell();
            a_builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            a_builder.CellFormat.Width = 175d;
            a_builder.Write(sFiller + a_sFundName);

            a_builder.Font.Bold = bValueFold;
            for (int ii = iCnt; ii >= 0; ii -= 1)
            {
                a_builder.InsertCell();
                a_builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
                a_builder.CellFormat.Width = 18d;
                a_builder.Write(string.IsNullOrEmpty(Px21[ii]) ? sEmpty : Px21[ii] + sPercent);
            }


            a_builder.EndRow();
        }
        private void BuildPx21Array(string[] a_sArr, DataRow a_drRow)
        {
            Array.Clear(a_sArr, 0, a_sArr.Length);
            for (int iI = 0; iI <= 20; iI++)
            {
                a_sArr[iI] = (!a_drRow["p" + iI].Equals(0)) ? a_drRow["p" + iI].ToString() : "";
            }
        }
        private DataTable BuildPx21Headings(DataTable a_tbPx21)
        {
            DataRow dr;
            var tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("heading_id", typeof(string)));
            tbl.PrimaryKey = [tbl.Columns["heading_id"]];
            tbl.Columns.Add(new DataColumn("p0", typeof(int)));
            tbl.Columns.Add(new DataColumn("p1", typeof(int)));
            tbl.Columns.Add(new DataColumn("p2", typeof(int)));
            tbl.Columns.Add(new DataColumn("p3", typeof(int)));
            tbl.Columns.Add(new DataColumn("p4", typeof(int)));
            tbl.Columns.Add(new DataColumn("p5", typeof(int)));
            tbl.Columns.Add(new DataColumn("p6", typeof(int)));
            tbl.Columns.Add(new DataColumn("p7", typeof(int)));
            tbl.Columns.Add(new DataColumn("p8", typeof(int)));
            tbl.Columns.Add(new DataColumn("p9", typeof(int)));
            tbl.Columns.Add(new DataColumn("p10", typeof(int)));
            tbl.Columns.Add(new DataColumn("p11", typeof(int)));
            tbl.Columns.Add(new DataColumn("p12", typeof(int)));
            tbl.Columns.Add(new DataColumn("p13", typeof(int)));
            tbl.Columns.Add(new DataColumn("p14", typeof(int)));
            tbl.Columns.Add(new DataColumn("p15", typeof(int)));
            tbl.Columns.Add(new DataColumn("p16", typeof(int)));
            tbl.Columns.Add(new DataColumn("p17", typeof(int)));
            tbl.Columns.Add(new DataColumn("p18", typeof(int)));
            tbl.Columns.Add(new DataColumn("p19", typeof(int)));
            tbl.Columns.Add(new DataColumn("p20", typeof(int)));


            var queryH = from PxH in a_tbPx21.AsEnumerable()
                         group PxH by PxH.Field<string>("asset_group") into g
                         let AssetGroup = g.Key
                         select new
                         {
                             AssetGroup,
                             p0 = g.Sum(PxH => PxH.Field<int>("p0")),
                             p1 = g.Sum(PxH => PxH.Field<int>("p1")),
                             p2 = g.Sum(PxH => PxH.Field<int>("p2")),
                             p3 = g.Sum(PxH => PxH.Field<int>("p3")),
                             p4 = g.Sum(PxH => PxH.Field<int>("p4")),
                             p5 = g.Sum(PxH => PxH.Field<int>("p5")),
                             p6 = g.Sum(PxH => PxH.Field<int>("p6")),
                             p7 = g.Sum(PxH => PxH.Field<int>("p7")),
                             p8 = g.Sum(PxH => PxH.Field<int>("p8")),
                             p9 = g.Sum(PxH => PxH.Field<int>("p9")),
                             p10 = g.Sum(PxH => PxH.Field<int>("p10")),
                             p11 = g.Sum(PxH => PxH.Field<int>("p11")),
                             p12 = g.Sum(PxH => PxH.Field<int>("p12")),
                             p13 = g.Sum(PxH => PxH.Field<int>("p13")),
                             p14 = g.Sum(PxH => PxH.Field<int>("p14")),
                             p15 = g.Sum(PxH => PxH.Field<int>("p15")),
                             p16 = g.Sum(PxH => PxH.Field<int>("p16")),
                             p17 = g.Sum(PxH => PxH.Field<int>("p17")),
                             p18 = g.Sum(PxH => PxH.Field<int>("p18")),
                             p19 = g.Sum(PxH => PxH.Field<int>("p19")),
                             p20 = g.Sum(PxH => PxH.Field<int>("p20"))
                         };

            foreach (var h in queryH)
            {
                dr = tbl.NewRow();
                dr["heading_id"] = h.AssetGroup;
                dr["p0"] = h.p0;
                dr["p1"] = h.p1;
                dr["p2"] = h.p2;
                dr["p3"] = h.p3;
                dr["p4"] = h.p4;
                dr["p5"] = h.p5;
                dr["p6"] = h.p6;
                dr["p7"] = h.p7;
                dr["p8"] = h.p8;
                dr["p9"] = h.p9;
                dr["p10"] = h.p10;
                dr["p11"] = h.p11;
                dr["p12"] = h.p12;
                dr["p13"] = h.p13;
                dr["p14"] = h.p14;
                dr["p15"] = h.p15;
                dr["p16"] = h.p16;
                dr["p17"] = h.p17;
                dr["p18"] = h.p18;
                dr["p19"] = h.p19;
                dr["p20"] = h.p20;
                tbl.Rows.Add(dr);
            }


            var query = from pxGroup in a_tbPx21.AsEnumerable()
                        group pxGroup by new
                        {
                            AssetId = pxGroup.Field<int>("asset_id"),
                            SubAssetId = pxGroup.Field<int>("sub_asset_id")
                        } into g
                        select new
                        {
                            AssetId = g.Key.AssetId,
                            SubAssetId = g.Key.SubAssetId,
                            p0 = g.Sum(row => row.Field<int>("p0")),
                            p1 = g.Sum(row => row.Field<int>("p1")),
                            p2 = g.Sum(row => row.Field<int>("p2")),
                            p3 = g.Sum(row => row.Field<int>("p3")),
                            p4 = g.Sum(row => row.Field<int>("p4")),
                            p5 = g.Sum(row => row.Field<int>("p5")),
                            p6 = g.Sum(row => row.Field<int>("p6")),
                            p7 = g.Sum(row => row.Field<int>("p7")),
                            p8 = g.Sum(row => row.Field<int>("p8")),
                            p9 = g.Sum(row => row.Field<int>("p9")),
                            p10 = g.Sum(row => row.Field<int>("p10")),
                            p11 = g.Sum(row => row.Field<int>("p11")),
                            p12 = g.Sum(row => row.Field<int>("p12")),
                            p13 = g.Sum(row => row.Field<int>("p13")),
                            p14 = g.Sum(row => row.Field<int>("p14")),
                            p15 = g.Sum(row => row.Field<int>("p15")),
                            p16 = g.Sum(row => row.Field<int>("p16")),
                            p17 = g.Sum(row => row.Field<int>("p17")),
                            p18 = g.Sum(row => row.Field<int>("p18")),
                            p19 = g.Sum(row => row.Field<int>("p19")),
                            p20 = g.Sum(row => row.Field<int>("p20"))
                        };

            foreach (var h in query)
            {
                dr = tbl.NewRow();
                dr["heading_id"] = h.AssetId.ToString() + h.SubAssetId;
                dr["p0"] = h.p0;
                dr["p1"] = h.p1;
                dr["p2"] = h.p2;
                dr["p3"] = h.p3;
                dr["p4"] = h.p4;
                dr["p5"] = h.p5;
                dr["p6"] = h.p6;
                dr["p7"] = h.p7;
                dr["p8"] = h.p8;
                dr["p9"] = h.p9;
                dr["p10"] = h.p10;
                dr["p11"] = h.p11;
                dr["p12"] = h.p12;
                dr["p13"] = h.p13;
                dr["p14"] = h.p14;
                dr["p15"] = h.p15;
                dr["p16"] = h.p16;
                dr["p17"] = h.p17;
                dr["p18"] = h.p18;
                dr["p19"] = h.p19;
                dr["p20"] = h.p20;
                tbl.Rows.Add(dr);
            }

            return tbl;
        }
        private void MakeTbBorder(Aspose.Words.Tables.Table a_docTable)
        {
            foreach (Aspose.Words.Tables.Cell cell in a_docTable.FirstRow.Cells)
            {
                cell.CellFormat.TopPadding = 0d;
            }

            a_docTable.FirstRow.RowFormat.Borders[Aspose.Words.BorderType.Top].LineStyle = LineStyle.Single;
            a_docTable.FirstRow.RowFormat.Borders[Aspose.Words.BorderType.Top].LineWidth = 1d;
            a_docTable.LastRow.RowFormat.Borders[Aspose.Words.BorderType.Bottom].LineStyle = LineStyle.Single;
            a_docTable.LastRow.RowFormat.Borders[Aspose.Words.BorderType.Bottom].LineWidth = 1d;
            // Set fight and left borders
            foreach (Aspose.Words.Tables.Row row in a_docTable.Rows)
            {
                row.FirstCell.CellFormat.Borders[Aspose.Words.BorderType.Left].LineStyle = LineStyle.Single;
                row.FirstCell.CellFormat.Borders[Aspose.Words.BorderType.Left].LineWidth = 1d;
                row.LastCell.CellFormat.Borders[Aspose.Words.BorderType.Right].LineStyle = LineStyle.Single;
                row.LastCell.CellFormat.Borders[Aspose.Words.BorderType.Right].LineWidth = 1d;
            }

        }
        private void getParticipantLetterPXMessage(DocumentBuilder builder, Document doc, string sType, string sEstimatedEffectiveDate)
        {
            string sMsgTemp1 = "Please see the attached PortfolioXpress profiles which display the new allocation models. Note PortfolioXpress may not use all of the investment choices available in your Plan.";
            string sMsgTemp2 = "As always, you should consider your specific personal situation including assets, income, and investments outside of the plan when determining whether to participate in the PortfolioXpress service.";
            string sMsgTemp3 = "The changes described in this notice materially affect the asset allocation models of the service to which you are currently subscribed.  All participants using the current PortfolioXpress service will be automatically unsubscribed from the service on or about the ";
            builder.MoveToMergeField("PX_message");
            switch (sType ?? "")
            {
                case "2.0":
                case "2.1": // IT-68295
                    {
                        builder.Bold = false;
                        writePX_ISCInformation(builder, sType);
                        builder.Writeln("");
                        break;
                    }
                case "3.0":
                case "3.1":
                    {
                        builder.Bold = false;
                        writePX_ISCInformation(builder, sType);
                        builder.Writeln("");
                        sharedMessage01(builder, sEstimatedEffectiveDate);
                        break;
                    }
                case "4.0":
                case "4.1":
                    {
                        writePX_ISCInformation(builder, sType);
                        sharedMessage02(builder);
                        break;
                    }

                case "5.0":
                case "5.1":
                case "5.2":
                case "5.3":
                    {

                        disPXInvestmentChoice(builder, doc);
                        writePX_ISCInformation(builder, sType);
                        writeAdvisorName(builder, sType);
                        break;
                    }

                case "6.0":
                case "6.1":
                case "6.2":
                case "6.3":
                    {
                        builder.Write(sMsgTemp3);
                        builder.Bold = true;
                        builder.Writeln("Estimated Effective Date.");
                        builder.Bold = false;
                        sharedMessage01(builder, sEstimatedEffectiveDate);

                        disPXInvestmentChoice(builder, doc);
                        writePX_ISCInformation(builder, sType);
                        writeAdvisorName(builder, sType);
                        break;
                    }
                case "61.0":
                case "61.1":
                case "61.2":
                case "61.3":
                    {
                        builder.Writeln("Because this materially affects the asset allocation models of the service to which you are currently subscribed, all participants using the current PortfolioXpress service should review the changes in advance of the Estimated Effective Date to decide if they are comfortable with investment allocations going forward.");
                        builder.Writeln("");
                        sharedMessage02(builder);

                        disPXInvestmentChoice(builder, doc);
                        writePX_ISCInformation(builder, sType);
                        writeAdvisorName(builder, sType);
                        break;
                    }
                // InsertAdvText(builder)
                case "7.0":
                case "7.1":
                    {
                        builder.Bold = true;
                        builder.Writeln("PortfolioXpress Information");
                        builder.Write("");
                        builder.MoveToMergeField("PX_message2");

                        builder.Write("If you are currently subscribed to PortfolioXpress, Investment Choice additions and/or deletions to PortfolioXpress affect PortfolioXpress. Transfers and future contribution allocations will be processed as indicated in the schedule above.  " + "Updates to your PortfolioXpress allocations will be processed on your next rebalance date.  The PortfolioXpress allocation models will be updated on or about the ");
                        builder.Bold = true;
                        builder.Write("Estimated Effective Date");
                        builder.Bold = false;
                        builder.Writeln(" stated above.  ");
                        builder.Writeln("");

                        builder.Writeln(sMsgTemp1);
                        builder.Writeln("");
                        builder.Writeln("PortfolioXpress subscribers are unable to make transfers or change future contribution allocations.  If you wish to make transfers or change future contribution allocations, you must first unsubscribe to the PortfolioXpress service.  ");
                        builder.Writeln("");

                        disPXInvestmentChoice(builder, doc);
                        writePX_ISCInformation(builder, sType);
                        writeAdvisorName(builder, sType);
                        break;
                    }
                // InsertAdvText(builder)
                case "8.0":
                case "8.1":
                    {
                        InsertHeadingpMsg(builder);

                        builder.MoveToMergeField("PX_message2");

                        builder.Writeln(sMsgTemp1);
                        builder.Writeln("");

                        builder.Write(sMsgTemp3);
                        builder.Bold = true;
                        builder.Writeln("Estimated Effective Date.");
                        builder.Bold = false;
                        builder.Writeln("");

                        sharedMessage01(builder, sEstimatedEffectiveDate);
                        builder.Writeln("");
                        builder.Writeln(sMsgTemp2);
                        writeAdvisorName(builder, sType);
                        disPXInvestmentChoice(builder, doc);
                        writePX_ISCInformation(builder, sType);
                        break;
                    }
                case "81.0":
                case "81.1":
                    {
                        InsertHeadingpMsg(builder);
                        builder.MoveToMergeField("PX_message2");
                        builder.Writeln(sMsgTemp1);
                        builder.Writeln("");

                        builder.Write("The changes described in this notice materially affect the asset allocation models of the service to which you are currently subscribed. All participants using the current PortfolioXpress service should review the changes in advance of the ");
                        builder.Bold = true;
                        builder.Write("Estimated Effective Date");
                        builder.Bold = false;
                        builder.Writeln("to decide if they are comfortable with investment allocations going forward.");
                        builder.Writeln("");
                        sharedMessage02(builder);
                        builder.Writeln("");
                        builder.Writeln(sMsgTemp2);
                        writeAdvisorName(builder, sType);
                        disPXInvestmentChoice(builder, doc);
                        writePX_ISCInformation(builder, sType);
                        break;
                    }
                case "12.0":
                case "12.1":
                case "12.2":
                case "12.3":
                    {
                        writeAdvisorName(builder, sType);
                        writePX_ISCInformation(builder, sType);
                        break;
                    }
                case "13.0":
                    {
                        writeAdvisorName(builder, sType);
                        writePX_ISCInformation(builder, sType);
                        break;
                    }

            }

        }
        private void sharedMessage01(DocumentBuilder builder, string sEstimatedEffectiveDate)
        {

            builder.Bold = true;
            builder.Writeln("");
            builder.Write("This means that as a current PortfolioXpress subscriber, you will be automatically unsubscribed from the service on or about " + sEstimatedEffectiveDate + ".  Your current investments and your allocations for future contributions will remain the same as they are as of the Effective Date until you decide to change them; " + "but, all PortfolioXpress features, including automatic quarterly rebalancing, will be turned off.  ");


            builder.Bold = false;
            builder.Write("You may also log into your account and unsubscribe from the service prior to the ");
            builder.Bold = true;
            builder.Write("Estimated Effective Date, ");
            builder.Bold = false;
            builder.Writeln("at which time you may make Investment Choice transfers and/or investment elections for new contributions at any time.");

            builder.Writeln("");

            builder.Bold = true;
            builder.Write("After the Effective Date, you may also choose to re-subscribe to the new PortfolioXpress service, ");
            builder.Bold = false;
            builder.Writeln("in which case your plan account balance will be automatically reallocated among the Plan’s Investment Choices eligible for PortfolioXpress based on your selected retirement year. ");

        }
        private void sharedMessage02(DocumentBuilder builder)
        {
            builder.Bold = false;

            builder.Bold = true;
            builder.Writeln("If you are a current PortfolioXpress subscriber and decide to take no action your current investments and your allocations for future contributions will be updated according to the new PortfolioXpress allocation models on your first quarterly rebalance after the Effective Date. ");
            builder.Writeln("");

            builder.Bold = false;
            builder.Write("If you are not comfortable with these changes, you may unsubscribe from the service at any time prior to the ");
            builder.Bold = true;
            builder.Write("Estimated Effective Date, ");
            builder.Bold = false;
            builder.Writeln("at which time your current investments and allocations for future contributions will remain unchanged until you decide to change them. You may then make Investment Choice transfers and/or investment elections for new contributions at any time.");
            builder.Writeln("");
        }
        private void InsertHeadingpMsg(DocumentBuilder builder)
        {
            builder.Bold = true;

            builder.Writeln("                     **The PortfolioXpress Service is changing**");
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            builder.Writeln("**Please review the enclosed materials carefully**");
            builder.Writeln("**You may wish to take action**");
            builder.ParagraphFormat.Alignment = ParagraphAlignment.Left;
        }
        private void disPXInvestmentChoice(DocumentBuilder builder, Document doc)
        {
            builder.MoveToMergeField("PX_ppt_add_funds");
            var dvDel = new DataView(_oFw.NewFundsCustomPX, "action=1", "", DataViewRowState.CurrentRows);
            disPxFunds(builder, dvDel, "Add");
            builder.MoveToMergeField("PX_ppt_del_funds");
            var dvDel2 = new DataView(_oFw.NewFundsCustomPX, "action=2", "", DataViewRowState.CurrentRows);
            disPxFunds(builder, dvDel2, "Del");
        }
        private void disPxFunds(DocumentBuilder builder, DataView dvDel, string sAddDel)
        {
            // builder.StartTable()
            if (dvDel.Count > 0)
            {
                builder.Write("We will ");
                builder.Bold = true;
                if (sAddDel == "Add")
                {
                    builder.Write("add");
                }
                else
                {
                    builder.Write("delete");
                }
                builder.Bold = false;
                builder.Writeln(" the following Investment Choice(s) to the models:");

                foreach (DataRowView r in dvDel)
                {
                    builder.Writeln("       " + r["fund_name"].ToString());
                }
            }
            else
            {
                builder.Write("");
            }
            // builder.EndTable()
            builder.Writeln();
            builder.ParagraphFormat.LeftIndent = 0d;
        }
        private void writeAdvisorName(DocumentBuilder builder, string sType)
        {
            builder.MoveToMergeField("advisor_name");
            if (sType == "5.0" | sType == "5.1" | sType == "6.0" | sType == "6.1" | sType == "61.0" | sType == "61.1" | sType == "7.0" | sType == "8.0" | sType == "81.0" | sType == "12.0" | sType == "12.1")
            {
                builder.Write(" and " + TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_fiduciary_Name, _oFw.PdfHeader)[0] + ", together,");
            }
            else
            {
                builder.Write("");
            }
        }
        private void writePX_ISCInformation(DocumentBuilder builder, string sType)
        {
            string sName = "";
            string sEffDate = "Estimated Effective Date";
            string sISCPxQTIATxt = "";
            string sISCPxQTIATxt1 = "";
            string sISCPxQTIATxt2 = "";
            string sISCPxQTIATxt3 = "";
            builder.MoveToMergeField("PX_ISCInformation");
            if (_oFw.PartnerId == "ISC" && TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_new, _oFw.PdfHeader)[0] == "-1") // fund_id -1 = portfolioXpress
            {
                bool bQdia = TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_default_fund_qdia_answer, _oFw.PdfHeader)[0] == "Yes";
                if (bQdia)
                {
                    sName = "Qualified Default Investment Alternative (QDIA)";
                }
                else
                {
                    sName = "Default Alternative";
                }
                sISCPxQTIATxt = "PortfolioXpress has been selected as the Plan's " + sName + ". A Plan's" + sName + " is an Investment Choice or series of Investment Choices in which your account and contributions will be invested, if you fail to make an affirmative investment election under the Plan.  This means that if you do not have an investment election on file with Transamerica before the ";
                // 'Estimated Effective Date
                sISCPxQTIATxt1 = " indicated above, your entire account will be re-allocated on or about the ";
                // 'Estimated Effective Date
                sISCPxQTIATxt2 = ", based on the PortfolioXpress allocation models using an assumed retirement date at age 65.   If you do not want this to happen, you must log into your account or call Transamerica prior to the ";
                // 'Estimated Effective Date
                sISCPxQTIATxt3 = ", and complete an investment election for your account. When making your election, you should take into consideration any Investment Choice changes indicated above in this Notice.";

                builder.Writeln("");
                builder.Write("");
                builder.Write(sISCPxQTIATxt);
                builder.Bold = true;
                builder.Write(sEffDate);
                builder.Bold = false;
                builder.Write(sISCPxQTIATxt1);
                builder.Bold = true;
                builder.Write(sEffDate);
                builder.Bold = false;
                builder.Write(sISCPxQTIATxt2);
                builder.Bold = true;
                builder.Write(sEffDate);
                builder.Bold = false;
                builder.Write(sISCPxQTIATxt3);
                builder.Writeln("");
                builder.MoveToMergeField("QDIA_PX");
                if (sType == "13.0")
                {
                    builder.Write(sName);
                }
                else
                {
                    builder.Write("");
                }
            }
            else
            {
                builder.Write("");
            }
        }
        private void writePXAddDeleteFund(DocumentBuilder builder, bool bDoHeading = true)
        {
            if (bDoHeading)
            {
                builder.Bold = true;
                // builder.Writeln("Custom PortfolioXpress")
                builder.Writeln("");

                builder.Bold = false;
                // builder.Writeln("Below are the Investment Choice change(s) requested to the PortfolioXpress investment line up:")
                // builder.Writeln("")
            }
            DataView dvPXAdd;
            DataView dvPXDel;
            dvPXAdd = new DataView(_oFw.NewFundsCustomPX, "action=1", "", DataViewRowState.CurrentRows);
            if (dvPXAdd.Count > 0)
            {
                builder.Writeln("Added Investment Choice(s):");
                builder.Writeln("");
                // builder.ParagraphFormat.LeftIndent = 0.3 * 72
                foreach (DataRowView oRow in dvPXAdd)
                {
                    builder.Writeln("   " + oRow["fund_name"].ToString());
                }

                builder.Writeln("");
            }

            dvPXDel = new DataView(_oFw.NewFundsCustomPX, "action=2", "", DataViewRowState.CurrentRows);
            if (dvPXDel.Count > 0)
            {
                builder.Writeln("Deleted Investment Choice(s):");
                builder.Writeln("");
                foreach (DataRowView oRow in dvPXDel)
                {
                    builder.Writeln(Convert.ToString(oRow["fund_name"]));
                }

                builder.Writeln("");
            }
        }
    }
}
