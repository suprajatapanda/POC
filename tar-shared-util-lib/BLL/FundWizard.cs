using MimeKit;
using SIUtil;
using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using TRS.IT.BendProcessor.DriverSOA;
using TRS.IT.SI.BusinessFacadeLayer.SOA;
using TRS.IT.SOA.Model.PreSales.FundLineupData;
using MO = TRS.IT.SI.BusinessFacadeLayer.Model;
using SOAModel = TRS.IT.SOA.Model;

namespace TRS.IT.SI.BusinessFacadeLayer
{

    [Serializable()]
    public class FundWizard
    {
        public string _sSessionId { get; set; }
        private string _sConId;
        private string _sSubId;
        private DAL.ContractDC _ContractDC;
        private ContractSoa _wsContract = new("Fund Wizard");
        private SOAModel.ContractInfo _oConInfo;
        private SOAModel.AdditionalData _oBasic = new();
        public XDocument _xDoc;
        private DataTable _tbFundPending;
        public DataTable _tbFundPendingCustomPX;
        private DataTable _tbFundPendingDel;
        private DataTable _tblPdfHdr;
        private DataTable _tblManagedAdvice;
        private int _iAction;
        private int _iCaseNo;
        public int _iCaseStatus;
        private string _sCaseComplete;
        private string _sEffectiveDt;
        private string _sTransferDt;
        private string _sPegasysDt;
        private string _sCreateDt;
        private string _sCreateDateTime;
        private string _sPartnerId;
        private string _sPmName;
        private int _iPXConsentMethod;
        private string _sUserName;
        private bool _ReadFromCache = false;
        private bool _bNewCaseOverride = false;
        private int _iSignMethod = 0;
        private string _sConfirms;
        private bool _bRefreshFmrs = false;
        public bool isAddMA
        {
            get
            {
                return GetisAddMA();
            }
        }
        public bool isRemovePX
        {
            get
            {
                return GetisRemovePX();
            }
        }
        public bool isInvestmentChangeRequested
        {
            get
            {
                return GetisInvestmentChangeRequested();
            }
        }
        public bool isNewDefaultInvestmentChoiceRequested
        {
            get
            {
                return GetisNewDefaultInvestmentChoice();
            }
        }
        public bool isFWv2Impl
        {
            get
            {
                return isAddMA | isRemovePX | isInvestmentChangeRequested;
            }
        }
        public SOAModel.ContractInfo ContractInfo
        {
            get
            {
                if (_oConInfo == null)
                {
                    _oBasic.Basic_Provisions_Required = true;
                    _oBasic.All_Provisions_Required = true;
                    _oBasic.Contacts_Required = true;
                    _oConInfo = _wsContract.GetContractInformation(_sConId, _sSubId, _oBasic);
                }
                return _oConInfo;
            }
            set
            {
                _oConInfo = value;
            }
        }
        public bool RefreshFMRS
        {
            get
            {
                return _bRefreshFmrs;
            }
            set
            {
                _bRefreshFmrs = value;
            }
        }
        public string ContractId
        {
            get
            {
                return _sConId;
            }
        }
        public string SubId
        {
            get
            {
                return _sSubId;
            }
        }
        public DataTable NewFunds
        {
            get
            {
                if (_tbFundPending == null)
                {
                    _tbFundPending = TblDefNewFund("fwFundAdd");
                }
                return _tbFundPending;
            }
        }
        public DataTable NewFundsCustomPX
        {
            get
            {
                if (_tbFundPendingCustomPX == null)
                {
                    _tbFundPendingCustomPX = TblDefNewFund("fwFundAddCustomPX");
                }
                return _tbFundPendingCustomPX;
            }
        }
        public DataTable PendingDel
        {
            get
            {
                if (_tbFundPendingDel == null)
                {
                    _tbFundPendingDel = TblDefNewFund("fwFundDel");
                }
                return _tbFundPendingDel;
            }
            set
            {
                _tbFundPendingDel = value;
            }
        }
        public DataTable PdfHeader
        {
            get
            {
                if (_tblPdfHdr == null)
                {
                    _tblPdfHdr = TblDefPdfHeader();
                }
                return _tblPdfHdr;
            }
            set
            {
                _tblPdfHdr = value;
            }
        }
        public DataTable ManagedAdvice
        {
            get
            {
                if (_tblManagedAdvice == null)
                {
                    _tblManagedAdvice = TblDefManagedAdvice();
                }
                return _tblManagedAdvice;
            }
            set
            {
                _tblManagedAdvice = value;
            }
        }
        public DataTable TblDefManagedAdvice(string a_sTableName = "")
        {
            if (string.IsNullOrEmpty(a_sTableName))
            {
                a_sTableName = "FwMA";
            }
            var tbl = new DataTable(a_sTableName);

            tbl.Columns.Add(new DataColumn("ContractID", typeof(string)));
            tbl.PrimaryKey = [tbl.Columns["ContractID"]];
            tbl.Columns.Add(new DataColumn("ma_selected", typeof(string)));
            tbl.Columns.Add(new DataColumn("ma_conversion_method", typeof(string)));
            tbl.Columns.Add(new DataColumn("ma_qdia", typeof(string)));
            tbl.Columns.Add(new DataColumn("ma_px", typeof(string)));
            tbl.Columns.Add(new DataColumn("ma_tdf", typeof(string)));
            tbl.Columns.Add(new DataColumn("ma_fee", typeof(string)));
            tbl.Columns.Add(new DataColumn("ma_effective_date", typeof(string)));
            tbl.Columns.Add(new DataColumn("ma_start_date", typeof(string)));
            tbl.Columns.Add(new DataColumn("ma_free_look_days", typeof(int)));

            return tbl;
        }
        public int Action
        {
            get
            {
                return _iAction;
            }
            set
            {
                _iAction = value;
            }
        }
        public int SignMethod
        {
            get
            {
                return _iSignMethod;
            }
            set
            {
                _iSignMethod = value;
            }
        }
        public int CaseNo
        {
            get
            {
                return _iCaseNo;
            }
        }
        public string UserName
        {
            get
            {
                if (string.IsNullOrEmpty(_sUserName))
                {
                    return @"US\sptlatrssoa";
                }
                else
                {
                    return _sUserName;
                }
            }
            set
            {
                _sUserName = value;
            }
        }
        public int CaseStatus
        {
            get
            {
                return _iCaseStatus;
            }
        }
        public string PegasysDate
        {
            get
            {
                return _sPegasysDt;
            }
            set
            {
                _sPegasysDt = value;
            }
        }
        public string CreateDate
        {
            get
            {
                return _sCreateDt;
            }
            set
            {
                _sCreateDt = value;
            }
        }
        public string PartnerId
        {
            get
            {
                return _sPartnerId;
            }
            set
            {
                _sPartnerId = value;
            }
        }
        public string PMName
        {
            get
            {
                if (string.IsNullOrEmpty(_sPmName))
                {
                    return "Transamerica Retirement Solutions";
                }
                else
                {
                    return _sPmName;
                }
            }
        }
        public FundWizard(string a_sSessionId, string a_sConId, string a_sSubId)
        {
            _sSessionId = a_sSessionId;
            _sConId = a_sConId;
            _sSubId = a_sSubId;
            _ContractDC = new DAL.ContractDC(_sSessionId, _sConId, _sSubId);

        }
        public void LoadFMRS(bool bActiveFundList = false, bool bRestriction = false)
        {
            var oFundInfo = new FundInfoSoa();
            string strXML = "";
            if (_ReadFromCache)
            {
                strXML = DAL.AudienceDC.GetObjectDataByDate(DateTime.Now.Date.ToString(), MO.Enums.E_ObjectType.FMRS, _sConId, _sSubId);
            }
            if (string.IsNullOrEmpty(strXML))
            {
                strXML = oFundInfo.GetFmrxXml(GetFMRSInput(bActiveFundList, bRestriction));
                // Write to database cache
                if (_ReadFromCache)
                {
                    DAL.AudienceDC.SaveObjectDataByDate(_sSessionId, MO.Enums.E_ObjectType.FMRS, strXML, ContractId, SubId);
                }
            }
            _xDoc = XDocument.Load(new StringReader(strXML));
            RefreshFMRS = false;
            SetDefaults(_xDoc);
            RemoveNotNeededTags(ref _xDoc);
        }
        public int GetCaseNo(int a_iCaseNo)
        {
            var ds = _ContractDC.FwGetFundSelection(_sConId, _sSubId, a_iCaseNo);
            if (ds.Tables[0].Rows.Count == 0)
            {
                return -1;
            }
            var xDoc = XDocument.Load(new StringReader(Convert.ToString(ds.Tables[0].Rows[0]["fund_data"])));
            _iCaseNo = Convert.ToInt32(FWUtils.CheckDBNullInt(ds.Tables[0].Rows[0]["case_no"]));
            _iCaseStatus = Convert.ToInt32(FWUtils.CheckDBNullInt(ds.Tables[0].Rows[0]["status"]));
            _sCaseComplete = FWUtils.CheckDBNullDt(ds.Tables[0].Rows[0]["complete_dt"]);
            _sConfirms = FWUtils.CheckDBNull(ds.Tables[0].Rows[0]["confirms"]);
            _iSignMethod = Convert.ToInt32(FWUtils.CheckDBNullInt(ds.Tables[0].Rows[0]["sign_method"]));

            _sEffectiveDt = FWUtils.CheckDBNullDt(ds.Tables[0].Rows[0]["effective_dt"]);
            _sTransferDt = FWUtils.CheckDBNullDt(ds.Tables[0].Rows[0]["transfer_dt"]);
            _sPegasysDt = FWUtils.CheckDBNullDt(ds.Tables[0].Rows[0]["pegasys_dt"]);
            _sCreateDt = FWUtils.CheckDBNullDt(ds.Tables[0].Rows[0]["create_dt"]);
            _sCreateDateTime = FWUtils.CheckDBNullDateTime(ds.Tables[0].Rows[0]["create_dt"]);

            _sPartnerId = ds.Tables[0].Rows[0]["partner_id"] is DBNull ? "" : ds.Tables[0].Rows[0]["partner_id"].ToString();
            _sPmName = ds.Tables[0].Rows[0]["pm_name"] is DBNull ? "" : ds.Tables[0].Rows[0]["pm_name"].ToString();
            _iPXConsentMethod = Convert.ToInt32(FWUtils.CheckDBNullInt(ds.Tables[0].Rows[0]["px_consent_method"]));
            if (!(xDoc.Element("FWUpdate").Element("PdfHdr") == null))
            {
                if (PdfHeader.Rows.Count > 0)
                {
                    PdfHeader.Clear();
                }

                PdfHeader.ReadXml(new StringReader(xDoc.Element("FWUpdate").Element("PdfHdr").ToString()));
            }
            Action = Convert.ToInt32(FWUtils.CheckDBNullInt(ds.Tables[0].Rows[0]["change_type"]));
            if (!(xDoc.Element("FWUpdate").Element("NewFund") == null))
            {
                if (NewFunds.Rows.Count > 0)
                    NewFunds.Clear();
                NewFunds.ReadXml(new StringReader(xDoc.Element("FWUpdate").Element("NewFund").ToString()));
            }
            if (!(xDoc.Element("FWUpdate").Element("DelFund") == null))
            {
                if (PendingDel.Rows.Count > 0)
                    PendingDel.Clear();
                PendingDel.ReadXml(new StringReader(xDoc.Element("FWUpdate").Element("DelFund").ToString()));
            }
            if (!(xDoc.Element("FWUpdate").Element("NewFundCustomPX") == null))
            {
                if (NewFundsCustomPX.Rows.Count > 0)
                    NewFundsCustomPX.Clear();
                NewFundsCustomPX.ReadXml(new StringReader(xDoc.Element("FWUpdate").Element("NewFundCustomPX").ToString()));
            }
            if (!(xDoc.Element("FWUpdate").Element("ManagedAdvice") == null))
            {
                if (ManagedAdvice.Rows.Count > 0)
                    ManagedAdvice.Clear();
                ManagedAdvice.ReadXml(new StringReader(xDoc.Element("FWUpdate").Element("ManagedAdvice").ToString()));
            }
            return _iCaseNo;
        }
        public DataTable GetActiveFunds(bool a_bPromptRow, bool a_bXpress, bool bActiveFundList = false, bool bCheckFundGroup = false, bool FundDescriptor = false)
        {
            var tbActiveFunds = TblDefActiveFund("", FundDescriptor);
            DataRow dr;
            DataView dv;
            var strTmp = new string[2];

            if (_xDoc == null)
            {
                LoadFMRS(bActiveFundList);
            }

            if (a_bPromptRow)
            {
                dr = tbActiveFunds.NewRow();
                dr["fund_id"] = 0;
                dr["fund_name"] = "Select an investment choice";
                tbActiveFunds.Rows.Add(dr);
            }
            if (a_bXpress)
            {
                if (FWUtils.GetHdrData(FWUtils.C_hdr_portXpress_selected, PdfHeader)[0] == "true")
                {
                    dr = tbActiveFunds.NewRow();
                    dr["fund_id"] = -1;
                    dr["fund_name"] = "PortfolioXpress";
                    tbActiveFunds.Rows.Add(dr);
                }
                if (FWUtils.GetHdrData(FWUtils.C_hdr_ManagedAdvice_Addition, PdfHeader)[0] == "true")
                {
                    dr = tbActiveFunds.NewRow();
                    dr["fund_id"] = -2;
                    dr["fund_name"] = "ManagedAdvice";
                    tbActiveFunds.Rows.Add(dr);
                }
            }
            // Add newly active funds first
            if (_tbFundPending != null)
            {
                foreach (DataRow rw in _tbFundPending.Rows)
                {
                    if (Convert.ToInt32(rw["action"]) == 1)
                    {
                        dr = tbActiveFunds.NewRow();
                        dr["fund_id"] = rw["fund_id"];
                        dr["fund_name"] = rw["fund_name"].ToString().Replace("<sup>[QDIA]</sup>", "[QDIA]") + " (added)";
                        tbActiveFunds.Rows.Add(dr);
                    }
                }
            }
            bool bAddRecord = true;
            foreach (XElement El in _xDoc.XPathSelectElements("//Fund[@PegStatusCurr=\"1\"]"))
            {
                if ((Convert.ToInt64(El.Parent.Parent.Attribute("Visible").Value) & 1L) != 0 | (Convert.ToInt64(El.Parent.Parent.Attribute("Visible").Value) & 2L) != 0)
                {
                    if (!bBlackOut(El) && tbActiveFunds.Rows.Find(El.Attribute("FundID").Value) == null)
                    {
                        bAddRecord = true;
                        if (bCheckFundGroup)
                        {
                            if (El.Parent.Parent.Attribute("FundGroupID").Value == "100")
                            {
                                bAddRecord = false;
                            }
                        }
                        if (bAddRecord)
                        {
                            dr = tbActiveFunds.NewRow();
                            dr["fund_id"] = El.Attribute("FundID").Value;
                            dr["fund_name"] = El.Element("Name").Value + GetQDIAText(CheckNull(El.Attribute("QDIAEligible")));
                            if (FundDescriptor)
                            {
                                dr["FundDescriptor"] = (from elem in El.Element("XRef").Elements("Partner")
                                                        where elem.Attribute("PartnerID").Value.ToString() == "1300" && elem.Attribute("FundCodeTypeID").Value.ToString() == "101" | elem.Attribute("FundCodeTypeID").Value.ToString() == "108"
                                                        select elem.Attribute("FundID").Value).FirstOrDefault();
                                dr["OldFundDescriptor"] = (from elem in El.Element("XRef").Elements("Partner")
                                                           where elem.Attribute("PartnerID").Value.ToString() == "1200" && elem.Attribute("FundCodeTypeID").Value.ToString() == "101"
                                                           select elem.Attribute("FundID").Value).FirstOrDefault();
                            }
                            tbActiveFunds.Rows.Add(dr);
                        }
                    }

                }
            }
            if (!(_tbFundPending == null))
            {
                dv = new DataView(_tbFundPending, "action=2", "", DataViewRowState.CurrentRows);
                if (dv.Count > 0)
                {
                    DataRow drD;
                    for (int iI = 0, loopTo = dv.Count - 1; iI <= loopTo; iI++)
                    {
                        drD = tbActiveFunds.Rows.Find(dv[iI]["fund_id"]);
                        if (!(drD == null))
                        {
                            tbActiveFunds.Rows.Remove(drD);
                        }
                    }
                }
            }
            return tbActiveFunds;
        }
        public MO.SIResponse UpdateFundChangeComplete()
        {

            var oResponse = new MO.SIResponse();
            try
            {
                _ContractDC.FwUpdateComplete(_iCaseNo, _sConId, _sSubId);
                oResponse.Errors[0].Number = 0;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oResponse.Errors[0].Number = -1;
                oResponse.Errors[0].Description = ex.Message;
            }

            return oResponse;
        }
        public int InsertTaskSponsorNPartLetter(string a_sPath, string a_sFileName)
        {
            var xEl = GetFileProfile(a_sPath, a_sFileName, "Sponsor-Notice-Participant-Letter", true);
            var xElDate = new XElement("Dates", new XAttribute("PegasysEffectiveDt", FWUtils.GetHdrData("pegasys_effective_date", _tblPdfHdr)[0]), new XAttribute("EffectiveDt", FWUtils.GetHdrData("effective_date", _tblPdfHdr)[0]), new XAttribute("TransferDt", FWUtils.GetHdrData("transfer_date", _tblPdfHdr)[0]), new XAttribute("PM", FWUtils.GetHdrData("project_manager", _tblPdfHdr)[0]));
            return InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.SponsorPptLetters, 100, [xEl, xElDate]);
        }
        public int InsertTaskFundMappingSpreadsheet(string a_sPath, string a_sFileName)
        {
            var xEl = GetFileProfile(a_sPath, a_sFileName, "Fund-Mapping-Spreadsheet", false);
            return InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.MappingSpreadSheet, 100, [xEl]);
        }
        public int InsertTaskFundRaider(string a_sPath, string a_sFileName, string a_sEdocFileName)
        {
            var xEl = GetFileProfile(a_sPath, a_sFileName, "Fund-Rider", false);
            xEl.Add(new XAttribute("EdocsFile", a_sEdocFileName));
            return InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.FundRider, 100, [xEl]);
        }
        public int InsertTaskFundQDIA(string a_sPath, string a_sFileName, string a_sEdocFileName)
        {
            var xEl = GetFileProfile(a_sPath, a_sFileName, "AnnualPptNotice", false);
            xEl.Add(new XAttribute("EdocsFile", a_sEdocFileName));
            return InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.AnnualPptNotice, 100, [xEl]);
        }
        public int InsertTaskSponsorPPTLetterToMC(string a_sFileNPath, string a_iInLoginIds, string a_sToEmail, string a_sError)
        {
            var xEmail = new XElement("MessageCenter", new XAttribute("InloginId", a_iInLoginIds), new XAttribute("ToEmail", a_sToEmail), new XElement("Error", a_sError));
            var xEl = GetFileProfile(Path.GetDirectoryName(a_sFileNPath) + @"\", Path.GetFileName(a_sFileNPath), "PPTNotice", false);
            return InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.SponsorPptLetterSentToMC, 100, [xEmail, xEl]);
        }
        public int InsertTaskSponsorQdiaNoticeToMC(string a_sFileNPath, string a_iInLoginIds, string a_sToEmail, string a_sError)
        {
            var xEmail = new XElement("MessageCenter", new XAttribute("InloginId", a_iInLoginIds), new XAttribute("ToEmail", a_sToEmail), new XElement("Error", a_sError));
            var xEl = GetFileProfile(Path.GetDirectoryName(a_sFileNPath) + @"\", Path.GetFileName(a_sFileNPath), "PptAnnualNotice", false);
            return InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.AnnualPptNoticeSentToMC, 100, [xEmail, xEl]);
        }
        public int InsertTaskUpdateISC(string a_sResult, string a_sRequest)
        {
            var xEl = new XElement("UpdatePartnerSystem", new XAttribute("P3XML", a_sRequest), new XAttribute("Response", "Success"), a_sResult);

            int iStatus = 100;
            if (a_sResult != "Succeeded")
            {
                iStatus = -1;
            }

            return InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.UpdatePartnerSystem, iStatus, [xEl]);
        }
        public int InsertTaskImageWms(string[] a_sGoodFiles, string[] a_sBadFiles)
        {
            var xElGood = new XElement("FilesImagedSuccesfully");
            var xElBad = new XElement("FilesImagedFailed");

            foreach (string s in a_sGoodFiles)
            {
                xElGood.Add(new XElement("File", s));
            }

            foreach (string s in a_sBadFiles)
            {
                xElBad.Add(new XElement("File", s));
            }

            return InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.DocsImaged, 100, [xElGood, xElBad]);
        }
        public DataSet GetTaskByTaskNo(int a_TaskNo)
        {
            return _ContractDC.FwGetTaskByTaskNo(_iCaseNo, a_TaskNo);
        }
        public DataSet GetDocsToImage()
        {
            return _ContractDC.FwGetDocsToImage(_iCaseNo);
        }
        public DataTable GetFundList(bool bRestriction = false)
        {
            var tbFunds = TblDefFundList();
            DataRow dr;
            string sAssetName, sSubAssetName;
            int iSubAssetOrderId, iAssetOrderId, iAssetId, iSubAssetId;
            int iNestedLevel;
            string sFMRS_PartnerID = "";
            sFMRS_PartnerID = TranslatePartnerID();
            if (_xDoc == null | RefreshFMRS)
            {
                LoadFMRS(false, bRestriction);
            }
            foreach (XElement El in _xDoc.XPathSelectElements("//Fund"))
            {
                if ((Convert.ToInt64(El.Parent.Parent.Attribute("Visible").Value) & 1L) != 0 | (Convert.ToInt64(El.Parent.Parent.Attribute("Visible").Value) & 2L) != 0)
                {
                    dr = tbFunds.NewRow();
                    dr["fund_id"] = El.Attribute("FundID").Value;
                    dr["fund_name"] = El.Element("Name").Value;
                    dr["fund_order_id"] = El.Attribute("OrderID").Value;
                    dr["qdia_eligible"] = CheckNull(El.Attribute("QDIAEligible"));
                    dr["qdia_option"] = CheckNull(El.Attribute("QDIAOption"));
                    dr["peg_status_curr"] = CheckNull(El.Attribute("PegStatusCurr"));
                    if ((Convert.ToInt64(El.Parent.Parent.Attribute("Visible").Value) & 2L) != 0)
                    {
                        dr["hide_add_delete"] = 1;
                    }
                    else
                    {
                        dr["hide_add_delete"] = 0;
                    }

                    iAssetId = 0;
                    sAssetName = "";
                    iAssetOrderId = 0;
                    iSubAssetId = 0;
                    sSubAssetName = "";
                    iSubAssetOrderId = 0;
                    iNestedLevel = El.Parent.Parent.Attribute("FundGroupIDConcat").Value.ToString().Trim().Split('|').Count() - 1; // Remove extra "|"
                    if (iNestedLevel > 2)
                    {
                        iSubAssetId = Convert.ToInt32(El.Parent.Parent.Attribute("FundGroupID").Value);
                        sSubAssetName = El.Parent.Parent.Attribute("Name").Value;
                        iSubAssetOrderId = Convert.ToInt32(El.Parent.Parent.Attribute("OrderID").Value);
                        if (iNestedLevel == 3)
                        {
                            iAssetId = Convert.ToInt32(El.Parent.Parent.Parent.Parent.Attribute("FundGroupID").Value);
                            sAssetName = El.Parent.Parent.Parent.Parent.Attribute("Name").Value;
                            iAssetOrderId = Convert.ToInt32(El.Parent.Parent.Parent.Parent.Attribute("OrderID").Value);
                        }
                        else if (iNestedLevel >= 4) // must go up two more level
                        {
                            iAssetId = Convert.ToInt32(El.Parent.Parent.Parent.Parent.Parent.Parent.Attribute("FundGroupID").Value);
                            sAssetName = El.Parent.Parent.Parent.Parent.Parent.Parent.Attribute("Name").Value;
                            iAssetOrderId = Convert.ToInt32(El.Parent.Parent.Parent.Parent.Parent.Parent.Attribute("OrderID").Value);
                        }
                    }
                    else
                    {
                        iAssetId = Convert.ToInt32(El.Parent.Parent.Attribute("FundGroupID").Value);
                        sAssetName = El.Parent.Parent.Attribute("Name").Value;
                        iAssetOrderId = Convert.ToInt32(El.Parent.Parent.Attribute("OrderID").Value);
                    }
                    dr["asset_id"] = iAssetId;
                    dr["asset_name"] = sAssetName;
                    dr["asset_order_id"] = iAssetOrderId;
                    dr["sub_asset_id"] = iSubAssetId;
                    dr["sub_asset_name"] = sSubAssetName;
                    dr["sub_asset_order_id"] = iSubAssetOrderId;

                    if (FWUtils.GetHdrData(FWUtils.C_hdr_portXpress_selected, PdfHeader)[0] == "true")
                    {
                        dr["px_custom_status"] = CheckNull(El.Attribute("PXCustomStatusCurr"));
                        string sFID = (from elem in El.Element("XRef").Elements("Partner")
                                       where elem.Attribute("PartnerID").Value.ToString() == "1200"
                                       where elem.Attribute("FundCodeTypeID").Value.ToString() == "104" | elem.Attribute("FundCodeTypeID").Value.ToString() == "105"
                                       select elem.Attribute("FundID").Value).FirstOrDefault();
                        if (!string.IsNullOrEmpty(sFID))
                        {
                            dr["px_style_code"] = sFID;
                        }
                        else
                        {
                            dr["px_style_code"] = "";
                        }
                    }

                    try
                    {
                        dr["FundDescriptor"] = (from elem in El.Element("XRef").Elements("Partner")
                                                where elem.Attribute("PartnerID").Value.ToString() == "1300" && elem.Attribute("FundCodeTypeID").Value.ToString() == "101" | elem.Attribute("FundCodeTypeID").Value.ToString() == "108"
                                                select elem.Attribute("FundID").Value).FirstOrDefault();
                        dr["OldFundDescriptor"] = (from elem in El.Element("XRef").Elements("Partner")
                                                   where elem.Attribute("PartnerID").Value.ToString() == "1200" && elem.Attribute("FundCodeTypeID").Value.ToString() == "101"
                                                   select elem.Attribute("FundID").Value).FirstOrDefault();
                        dr["partner_fund_id"] = (from elem in El.Element("XRef").Elements("Partner")
                                                 where elem.Attribute("PartnerID").Value.ToString() == sFMRS_PartnerID && elem.Attribute("FundCodeTypeID").Value.ToString() == "101"
                                                 select elem.Attribute("FundID").Value).FirstOrDefault();

                        if (Convert.ToDouble(sFMRS_PartnerID) == 1300d)
                        {
                            if (dr["FundDescriptor"] != null && !string.IsNullOrEmpty(FWUtils.CheckDBNull(dr["FundDescriptor"])))
                            {
                                dr["partner_fund_id"] = dr["FundDescriptor"];
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                        dr["partner_fund_id"] = "";
                    }

                    tbFunds.Rows.Add(dr);
                }
            }
            return tbFunds;
        }
        public string GenerateFundRaider(string sContractID, string sSubID, string sUserID)
        {

            string sResponseXml = "";
            sResponseXml = new eDocsSOA().DocGenFundRider(sContractID, sSubID, sUserID);
            return sResponseXml;

        }
        public DataTable GeneratePX21(bool bCustomPX = false)
        {

            var tbPx21 = TblDefPx21();
            DataRow drFL;
            var tbFundList = GetFundList(false);
            var tbActiveFunds = GetActiveFunds(false, false);
            var xEl = new XElement("Funds");
            string sPXxml;
            bool bFoundFund = false;
            string[] nonPXStyleCodes = ["37", "28", "29", "16", "38", "40", "41", "42", "43"];
            DataRow[] drTemp;
            // build input fundlist from active funds
            if (bCustomPX)
            {
                foreach (DataRow oRow in _tbFundPendingCustomPX.Select(" action = 2 "))
                {
                    if (tbFundList.Select("fund_id = " + oRow["fund_id"].ToString()).Length > 0)
                    {
                        tbFundList.Rows.Remove(tbFundList.Select("fund_id = " + oRow["fund_id"].ToString())[0]);
                        tbFundList.AcceptChanges();
                    }
                }
                foreach (DataRow oRow in _tbFundPendingCustomPX.Select(" action = 1 "))
                {
                    DataRow[] oFundRow = tbFundList.Select("fund_id = " + oRow["fund_id"].ToString());
                    if (oFundRow.Length > 0)
                    {
                        oFundRow[0]["px_custom_status"] = 1;
                        tbFundList.AcceptChanges();
                    }
                }
                drTemp = tbFundList.Select(" px_custom_status = 1 ");
            }
            else
            {
                drTemp = tbActiveFunds.Select();
            }
            foreach (DataRow drI in drTemp)
            {
                bFoundFund = false;
                drFL = tbFundList.Rows.Find(drI["fund_id"]);
                if (nonPXStyleCodes.Contains(Convert.ToString(drFL["px_style_code"])) == true)
                {
                    continue;
                }

                if (FWUtils.GetHdrData(FWUtils.C_hdr_PortXpress_custom, PdfHeader)[0] == "true")
                {
                    if (!(drFL == null) && Convert.ToInt32(drFL["fund_id"]) > 0 && !string.IsNullOrEmpty(drFL["px_style_code"].ToString()))
                    {
                        bFoundFund = true;
                    }
                }
                else if (!(drFL == null) && Convert.ToInt32(drFL["fund_id"]) > 0 && !string.IsNullOrEmpty(drFL["px_style_code"].ToString()))
                {
                    bFoundFund = true;
                }
                if (bFoundFund)
                {
                    xEl.Add(new XElement("FundInfo", new XElement("FundID", drFL["fund_id"].ToString()), new XElement("FundName", drFL["fund_name"].ToString()), new XElement("FundStyleCode", drFL["px_style_code"].ToString()), new XElement("FundDescriptor", drFL["FundDescriptor"].ToString())));
                }
            }

            sPXxml = new PXEngine().GetPxWithFunds(ContractId, SubId, Convert.ToInt32(FWUtils.GetHdrData(FWUtils.C_hdr_PortXpress_glidepath, PdfHeader)[0]), xEl.ToString());

            var xElPx21 = XElement.Load(new StringReader(sPXxml.Replace("xsi:nil=\"true\"", "")));

            var xElCur = xElPx21.XPathSelectElement("/Versions/VersionInfo/EndDt[not(text())]");

            if (!(xElCur == null))
            {
                if (xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").Count() == 21)
                {
                    // all portfolios must have the same # of funds
                    DataRow drF;
                    XElement xElFunds;
                    for (int iJ = 0, loopTo = xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").ElementAtOrDefault(0).Element("Funds").Elements("FundInfo").Count() - 1; iJ <= loopTo; iJ++)
                    {
                        xElFunds = xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").ElementAtOrDefault(0).Element("Funds").Elements("FundInfo").ElementAtOrDefault(iJ);
                        drF = tbPx21.NewRow();
                        drF["fund_id"] = xElFunds.Element("FundID").Value;
                        drF["px_style_code"] = xElFunds.Element("FundStyleCode").Value;

                        for (int iK = 0, loopTo1 = xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").Count() - 1; iK <= loopTo1; iK++)
                            drF["p" + iK] = xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").ElementAtOrDefault(iK).Element("Funds").Elements("FundInfo").ElementAtOrDefault(iJ).Element("Percentage").Value;
                        drFL = tbFundList.Rows.Find(drF["fund_id"]);

                        if (!(drFL == null))
                        {
                            drF["fund_name"] = drFL["fund_name"];
                            drF["fund_order_id"] = drFL["fund_order_id"];
                            drF["asset_id"] = drFL["asset_id"];
                            drF["asset_name"] = drFL["asset_name"];
                            drF["asset_order_id"] = drFL["asset_order_id"];
                            drF["sub_asset_id"] = drFL["sub_asset_id"];
                            drF["sub_asset_name"] = drFL["sub_asset_name"];
                            drF["sub_asset_order_id"] = drFL["sub_asset_order_id"];
                            drF["asset_group"] = Px21AssetGroup(Convert.ToInt32(xElFunds.Element("DIA_AssetClassID").Value));
                        }

                        tbPx21.Rows.Add(drF);
                    }
                }
            }
            tbPx21.DefaultView.Sort = "asset_group, asset_order_id, sub_asset_order_id,fund_order_id ASC";
            return tbPx21.DefaultView.ToTable();

        }
        public string GenerateQDIA(string sContractID, string sSubID, string sUserID)
        {
            string sResponseXml = "";
            sResponseXml = new eDocsSOA().DocGenQDIA(sContractID, sSubID, sUserID);
            return sResponseXml;
        }
        public void GetPartnerFundID(string sFundId, ref string sAbbrev_fund_name, ref string partner_FundID)
        {
            XElement xEl;
            string sFMRS_PartnerID = "";

            if (string.IsNullOrEmpty(sFundId))
            {
                return;
            }

            sFMRS_PartnerID = TranslatePartnerID();

            if (_xDoc == null)
            {
                LoadFMRS();
            }

            xEl = _xDoc.XPathSelectElement("//Fund[@FundID=" + sFundId + "]");

            if (!(xEl == null))
            {
                sAbbrev_fund_name = xEl.Attribute("Abbrev")?.Value;
                var query = (from elem in xEl.Elements("XRef").Elements("Partner")
                             where elem.Attribute("PartnerID")?.Value == sFMRS_PartnerID && elem.Attribute("FundCodeTypeID")?.Value == "101"
                             select elem.Attribute("FundID")?.Value).FirstOrDefault();
                if (!(query == null))
                {
                    partner_FundID = query.ToString();
                }
                else
                {
                    query = (from elem in xEl.Elements("XRef").Elements("Partner")
                             where elem.Attribute("PartnerID")?.Value == sFMRS_PartnerID && elem.Attribute("FundCodeTypeID")?.Value == "108"
                             select elem.Attribute("FundID")?.Value).FirstOrDefault();
                    if (!(query == null))
                    {
                        partner_FundID = query.ToString();
                    }
                    else
                    {
                        partner_FundID = string.Empty;
                    }
                }
            }
            else
            {
                partner_FundID = string.Empty;
                sAbbrev_fund_name = string.Empty;
            }
        }
        public DataTable TblDefPdfHeader(string a_sTableName = "")
        {
            if (string.IsNullOrEmpty(a_sTableName))
            {
                a_sTableName = "FwHeader";
            }
            var tbl = new DataTable(a_sTableName);

            tbl.Columns.Add(new DataColumn("row_id", typeof(string)));
            tbl.PrimaryKey = [tbl.Columns["row_id"]];
            tbl.Columns.Add(new DataColumn("row_desc", typeof(string)));
            tbl.Columns.Add(new DataColumn("row_val", typeof(string)));
            return tbl;
        }
        public DataTable TblDefActiveFund(string a_sTableName = "", bool FundDescriptor = false)
        {
            if (string.IsNullOrEmpty(a_sTableName))
            {
                a_sTableName = "FwActive";
            }
            var tbl = new DataTable(a_sTableName);
            tbl.Columns.Add(new DataColumn("fund_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("fund_name", typeof(string)));
            if (FundDescriptor)
            {
                tbl.Columns.Add(new DataColumn("FundDescriptor", typeof(string)));
                tbl.Columns.Add(new DataColumn("OldFundDescriptor", typeof(string)));
            }
            tbl.PrimaryKey = [tbl.Columns["fund_id"]];
            return tbl;
        }
        public DataTable TblDefNewFund(string a_sTableName = "")
        {
            if (string.IsNullOrEmpty(a_sTableName))
            {
                a_sTableName = "FwNewFund";
            }
            var tbl = new DataTable(a_sTableName);
            tbl.Columns.Add(new DataColumn("fund_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("fund_name", typeof(string)));
            tbl.Columns.Add(new DataColumn("asset_id", typeof(int)));
            tbl.PrimaryKey = [tbl.Columns["fund_id"]];
            tbl.Columns.Add(new DataColumn("sub_asset_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("action", typeof(int)));
            tbl.Columns.Add(new DataColumn("validated", typeof(int)));
            tbl.Columns.Add(new DataColumn("Error", typeof(string)));
            tbl.Columns.Add(new DataColumn("to_fund_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("to_fund_name", typeof(string)));
            tbl.Columns.Add(new DataColumn("qdia_eligible", typeof(string)));

            tbl.Columns.Add(new DataColumn("partner_fund_id", typeof(string)));
            tbl.Columns.Add(new DataColumn("Abbrev_fund_name", typeof(string)));
            tbl.Columns.Add(new DataColumn("to_partner_fund_id", typeof(string)));
            tbl.Columns.Add(new DataColumn("to_Abbrev_fund_name", typeof(string)));

            return tbl;
        }
        public DataTable TblDefFundList(string a_sTableName = "")
        {
            if (string.IsNullOrEmpty(a_sTableName))
            {
                a_sTableName = "FwFund";
            }
            var tbl = new DataTable(a_sTableName);
            tbl.Columns.Add(new DataColumn("fund_id", typeof(int)));
            tbl.PrimaryKey = [tbl.Columns["fund_id"]];
            tbl.Columns.Add(new DataColumn("fund_name", typeof(string)));
            tbl.Columns.Add(new DataColumn("fund_order_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("asset_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("asset_name", typeof(string)));
            tbl.Columns.Add(new DataColumn("asset_order_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("sub_asset_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("sub_asset_name", typeof(string)));
            tbl.Columns.Add(new DataColumn("sub_asset_order_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("account_type", typeof(string)));
            tbl.Columns.Add(new DataColumn("qdia_eligible", typeof(string)));
            tbl.Columns.Add(new DataColumn("qdia_option", typeof(string)));
            tbl.Columns.Add(new DataColumn("peg_status_curr", typeof(int)));
            tbl.Columns.Add(new DataColumn("hide_add_delete", typeof(int)));
            tbl.Columns.Add(new DataColumn("partner_fund_id", typeof(string)));
            tbl.Columns.Add(new DataColumn("px_style_code", typeof(string)));
            tbl.Columns.Add(new DataColumn("px_custom_status", typeof(string)));
            tbl.Columns.Add(new DataColumn("FundDescriptor", typeof(string)));
            tbl.Columns.Add(new DataColumn("OldFundDescriptor", typeof(string)));
            return tbl;
        }
        public DataTable TblDefPx21(string a_sTableName = "")
        {
            if (string.IsNullOrEmpty(a_sTableName))
            {
                a_sTableName = "FwFundPx21";
            }
            var tbl = new DataTable(a_sTableName);
            tbl.Columns.Add(new DataColumn("fund_id", typeof(int)));
            tbl.PrimaryKey = [tbl.Columns["fund_id"]];
            tbl.Columns.Add(new DataColumn("fund_name", typeof(string)));
            tbl.Columns.Add(new DataColumn("fund_order_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("asset_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("asset_name", typeof(string)));
            tbl.Columns.Add(new DataColumn("asset_order_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("sub_asset_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("sub_asset_name", typeof(string)));
            tbl.Columns.Add(new DataColumn("sub_asset_order_id", typeof(int)));
            tbl.Columns.Add(new DataColumn("asset_group", typeof(string)));
            tbl.Columns.Add(new DataColumn("px_style_code", typeof(string)));
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
            return tbl;
        }
        public MO.SIResponse SendSponsorPPTLetterToMCMigrated(string a_sPromptFileName, string a_sFilePathNName)
        {
            var oReturn = new MO.SIResponse();

            var oWebMessage = new Services.wsMessage.webMessage();
            var oWebMessageData = new Services.wsMessage.MsgData();

            var oDesignatedContacts = new List<SOAModel.PlanContactInfo>();
            int iBendInLoginId;
            string sError = string.Empty;
            string sLoginIdXML = string.Empty;
            string sEmails = string.Empty;
            string sInLoginIdList = string.Empty;

            try
            {
                oDesignatedContacts = GetDesignatedContacts();

                if (oDesignatedContacts.Count == 0)
                {
                    throw new Exception("Contract is missing primary contact");
                }

                iBendInLoginId = (int)WebMessageCenter.GetMsgCntrCMSAcctById("BendProcess");

                foreach (SOAModel.PlanContactInfo oContact in oDesignatedContacts)
                {
                    if (!string.IsNullOrEmpty(oContact.WebInLoginID))
                    {
                        sLoginIdXML += "<InLoginId>" + oContact.WebInLoginID + "</InLoginId>";
                        sInLoginIdList = sInLoginIdList + (string.IsNullOrEmpty(sInLoginIdList) ? oContact.WebInLoginID : ";" + oContact.WebInLoginID);
                    }
                    if (!string.IsNullOrEmpty(oContact.Email))
                    {
                        sEmails = sEmails + (string.IsNullOrEmpty(sEmails) ? oContact.Email : ";" + oContact.Email);
                    }
                }
                if (string.IsNullOrEmpty(sLoginIdXML))
                {
                    throw new Exception("No Web login contact is available");
                }
                // Contacts
                sLoginIdXML = "<ArrayOfInLoginId>" + sLoginIdXML + "</ArrayOfInLoginId>";

                // upload files
                var oAttachment = new Services.wsMessage.Attachment[1];
                oAttachment[0] = new Services.wsMessage.Attachment();
                byte[] byIn = File.ReadAllBytes(a_sFilePathNName);
                oAttachment[0].Data = Convert.ToBase64String(byIn);
                oAttachment[0].PromptFileName = a_sPromptFileName;

                oWebMessageData.Attachments = oAttachment;

                // set reply and message attributes
                oWebMessage.MsgSource = "FW-Backend";
                oWebMessageData.ReplyAllowed = false;

                oWebMessageData.Body = "Documentation regarding your recent investment choice change is now available.  The following notice(s) can be found on the \"Add & Delete Investment Choices - Pending Change Status\" screen:" + 
				"<br /><br />" + "<UL>" + 
				"<LI>Notice to Participants - Change of Investment Choices – This memo can be transferred to your company letterhead and distributed to your plan participants in advance of the change. " + 
				"Please note that effective August 30, 2012, the Department of Labor regulations under ERISA section 404(a) generally require plan sponsors to provide eligible employees with at least thirty (30) days, but no more than ninety (90) days advance notice of changes that affect the fees charged under the plan.<br /></LI>" + 
				"</UL>" + 
				"Please note that it is the plan sponsor's responsibility to communicate investment choice changes to participants.  It is important that you provide the appropriate participant notices communicating the investment choice change to participants promptly.<br />" + 
				"<br />Sincerely,<br/>Transamerica Retirement Solutions<br /><br />";

                oWebMessage.MsgData = oWebMessageData;
                oWebMessage.Subject = "Investment Choice Change Documentation";
                oWebMessage.CreateBy = iBendInLoginId.ToString();
                oWebMessage.CreateDt = Convert.ToString(DateTime.Now);
                oWebMessage.ExpireDt = Convert.ToString(DateTime.Now.AddDays(90d)); // expiration
                oWebMessage.SendNotification = "N";
                oWebMessage.FolderId = (int)SOAModel.webMsgGlobalFolderEnum.Inbox;
                oWebMessage.MsgType = "0";
                oWebMessage.FromAddress = "Transamerica Retirement Solutions";
                oWebMessage.SenderInLoginId = iBendInLoginId.ToString();
                oWebMessage.AttachmentCount = 1;

                var oWebMessageCenter = new WebMessageCenter();
                var oResponse = oWebMessageCenter.SendWebMessages(sLoginIdXML, oWebMessage, "", "");

                var objMessageData = new MO.MessageData();
                var objMessageService = new MessageService();
                objMessageData.MessageID = 2070;
                objMessageData.ContractID = _sConId;
                objMessageData.SubID = _sSubId;
                objMessageData.EmailVariableContainer.Add("fw_designated_contacts", sEmails);
                objMessageData.EImageOption = MO.E_ImageOption.None;
                var oNotSerResponse = MessageService.MessageServiceSendEmail(objMessageData);
                if (oNotSerResponse.Errors[0].Number != 0)
                {
                    sError = oNotSerResponse.Errors[0].Description;
                }

                if (oResponse.Errors[0].Number == 0)
                {
                    oReturn.Errors[0].Number = 0;
                    InsertTaskSponsorPPTLetterToMC(a_sFilePathNName, sInLoginIdList, sEmails, sError);
                }
                else
                {
                    oReturn.Errors[0].Number = -1;
                    oReturn.Errors[0].Description = oResponse.Errors[0].Description;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oReturn.Errors[0].Number = -1;
                oReturn.Errors[0].Description = "FileNameNPath: " + a_sFilePathNName + " PromptFileName: " + a_sPromptFileName + " ex: " + ex.Message;
            }
            return oReturn;
        }
        public MO.SIResponse SendSponsorQdiaNoticeToMC(string a_sPromptFileName, string a_sFilePathNName)
        {
            var oReturn = new MO.SIResponse();

            var oWebMessage = new Services.wsMessage.webMessage();
            var oWebMessageData = new Services.wsMessage.MsgData();

            var oDesignatedContacts = new List<SOAModel.PlanContactInfo>();
            int iBendInLoginId;
            string sError = string.Empty;
            string sLoginIdXML = string.Empty;
            string sEmails = string.Empty;
            string sInLoginIdList = string.Empty;

            try
            {
                oDesignatedContacts = GetDesignatedContacts();

                if (oDesignatedContacts.Count == 0)
                {
                    throw new Exception("Contract is missing primary contact");
                }

                iBendInLoginId = (int)WebMessageCenter.GetMsgCntrCMSAcctById("BendProcess");

                foreach (SOAModel.PlanContactInfo oContact in oDesignatedContacts)
                {
                    if (!string.IsNullOrEmpty(oContact.WebInLoginID))
                    {
                        sLoginIdXML += "<InLoginId>" + oContact.WebInLoginID + "</InLoginId>";
                        sInLoginIdList = sInLoginIdList + (string.IsNullOrEmpty(sInLoginIdList) ? oContact.WebInLoginID : ";" + oContact.WebInLoginID);
                    }
                    if (!string.IsNullOrEmpty(oContact.Email))
                    {
                        sEmails = sEmails + (string.IsNullOrEmpty(sEmails) ? oContact.Email : ";" + oContact.Email);
                    }
                }
                if (string.IsNullOrEmpty(sLoginIdXML))
                {
                    throw new Exception("No Web login contact is available");
                }

                sLoginIdXML = "<ArrayOfInLoginId>" + sLoginIdXML + "</ArrayOfInLoginId>";

                var oAttachment = new Services.wsMessage.Attachment[1];
                oAttachment[0] = new Services.wsMessage.Attachment();
                byte[] byIn = File.ReadAllBytes(a_sFilePathNName);
                oAttachment[0].Data = Convert.ToBase64String(byIn);
                oAttachment[0].PromptFileName = a_sPromptFileName;

                oWebMessageData.Attachments = oAttachment;
                oWebMessageData.ReplyAllowed = false;
                oWebMessageData.Body = "Documentation regarding your recent investment choice change is now available.  The following notice(s) can be found on the \"Add & Delete Investment Choices - Pending Change Status\" screen:" + 
				"<br /><br />" + "<UL>" + 
				"<LI>Annual Participant Notice – <b>This notice must be distributed 30 days prior to adding a Qualified Default Investment Alternative (QDIA) designation to your plan</b>. </LI>" + 
				"</UL>" + 
				"Please note that it is the plan sponsor's responsibility to communicate investment choice changes to participants.  It is important that you provide the appropriate participant notices communicating the investment choice change to participants promptly.<br />" + 
				"<br />Sincerely,<br/>Transamerica Retirement Solutions<br /><br />";

                oWebMessage.MsgSource = "FW-Backend";
                oWebMessage.MsgData = oWebMessageData;
                oWebMessage.Subject = "Investment Choice Change Documentation";
                oWebMessage.CreateBy = iBendInLoginId.ToString();
                oWebMessage.CreateDt = Convert.ToString(DateTime.Now);
                oWebMessage.ExpireDt = Convert.ToString(DateTime.Now.AddDays(90d)); // expiration
                oWebMessage.SendNotification = "N";
                oWebMessage.FolderId = (int)SOAModel.webMsgGlobalFolderEnum.Inbox;
                oWebMessage.MsgType = "0";
                oWebMessage.FromAddress = "Transamerica Retirement Solutions";
                oWebMessage.SenderInLoginId = iBendInLoginId.ToString();
                oWebMessage.AttachmentCount = 1;

                var oWebMessageCenter = new WebMessageCenter();
                var oResponse = oWebMessageCenter.SendWebMessages(sLoginIdXML, oWebMessage, "", "");

                var objMessageData = new MO.MessageData();
                var objMessageService = new MessageService();
                objMessageData.MessageID = 2070;
                objMessageData.ContractID = _sConId;
                objMessageData.SubID = _sSubId;
                objMessageData.EmailVariableContainer.Add("fw_designated_contacts", sEmails);
                objMessageData.EImageOption = MO.E_ImageOption.None;
                var oNotSerResponse = MessageService.MessageServiceSendEmail(objMessageData);
                if (oNotSerResponse.Errors[0].Number != 0)
                {
                    sError = oNotSerResponse.Errors[0].Description;
                }

                if (oResponse.Errors[0].Number == 0)
                {
                    oReturn.Errors[0].Number = 0;
                    InsertTaskSponsorQdiaNoticeToMC(a_sFilePathNName, sInLoginIdList, sEmails, sError);
                }
                else
                {
                    oReturn.Errors[0].Number = -1;
                    oReturn.Errors[0].Description = oResponse.Errors[0].Description;
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oReturn.Errors[0].Number = -1;
                oReturn.Errors[0].Description = "FileNameNPath: " + a_sFilePathNName + " PromptFileName: " + a_sPromptFileName + " ex: " + ex.Message;
            }
            return oReturn;
        }
        public void SendErrorNotification(string a_sTo, string a_sSubject, string a_sBody)
        {
            var oMail = new MimeMessage();
            var strStatus = new StringBuilder();
            string sFWToEmail = TrsAppSettings.AppSettings.GetValue("FWGeneralErrorNotification");
            string sFWFromEmail = TrsAppSettings.AppSettings.GetValue("FWWebEmailAddr");
            if (string.IsNullOrEmpty(sFWToEmail))
            {
                sFWToEmail = TrsAppSettings.AppSettings.GetValue("FWToErrorEmail");
            }
            if (string.IsNullOrEmpty(a_sTo))
            {
                a_sTo = sFWToEmail;
            }
            foreach (string sEmail in a_sTo.Split(';'))
            {
                if (!string.IsNullOrEmpty(sEmail.Trim()))
                {
                    oMail.To.Add(new MailboxAddress("", sEmail.Trim()));
                }
            }
            if (oMail.To.Count == 0)
            {
                oMail.To.Add(new MailboxAddress("", "hao.dinh@transamerica.com"));
            }
            oMail.From.Add(new MailboxAddress("", sFWFromEmail));
            oMail.Subject = a_sSubject + " (" + _sConId + "*" + _sSubId + ")";
            strStatus.Append("SessionID: " + _sSessionId.ToString() + Environment.NewLine);
            strStatus.Append(a_sBody);
            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = strStatus.ToString();
            oMail.Body = bodyBuilder.ToMessageBody();
            TRSManagers.MailManager.SendEmail(oMail);
        }
        public List<SOAModel.PlanContactInfo> GetDesignatedContacts()
        {
            var oContacts = new List<SOAModel.PlanContactInfo>();
            var oFWContacts = new List<SOAModel.PlanContactInfo>();

            oContacts = new FundContractsSoa().GetDesignatedContacts(ContractId, SubId);

            for (int iI = 0, loopTo = oContacts.Count - 1; iI <= loopTo; iI++)
            {
                for (int iJ = 0, loopTo1 = ContractInfo.PlanContacts.Count - 1; iJ <= loopTo1; iJ++)
                {
                    if (oContacts[iI].IndividualID == ContractInfo.PlanContacts[iJ].IndividualID)
                    {
                        oFWContacts.Add(ContractInfo.PlanContacts[iJ]);
                        break;
                    }
                }
            }
            if (oFWContacts.Count == 0)
            {
                oFWContacts.Add(GetPrimaryContact());
            }


            return oFWContacts;
        }
        public SOAModel.PlanContactInfo GetPrimaryContact()
        {
            var oPrimaryContact = new SOAModel.PlanContactInfo();
            oPrimaryContact.WebInLoginID = "-1";
            bool bFound = false;
            int iI, iJ;

            var loopTo = ContractInfo.PlanContacts.Count - 1;
            for (iI = 0; iI <= loopTo; iI++)
            {
                var loopTo1 = ContractInfo.PlanContacts[iI].Type.Count - 1;
                for (iJ = 0; iJ <= loopTo1; iJ++)
                {
                    if (ContractInfo.PlanContacts[iI].Type[iJ] == TRS.IT.SI.BusinessFacadeLayer.Model.E_ContactType.PrimaryContact)
                    {
                        oPrimaryContact = ContractInfo.PlanContacts[iI];
                        bFound = true;
                        break;
                    }
                }
                if (bFound)
                {
                    break;
                }
            }

            return oPrimaryContact;
        }
        public int iBlackOutDays(DateTime dtLiquidation)
        {
            int iBlackOutDaysRet = 15;
            try
            {
                
                string sTemp = "liq" + dtLiquidation.ToString("MM/dd/yyyy");
                string sHold = TrsAppSettings.AppSettings.GetValue(sTemp);
                if (sHold != null)
                {
                    iBlackOutDaysRet = Convert.ToInt32(sHold);
                }
                return iBlackOutDaysRet;
            }
            catch (Exception)
            {
                return iBlackOutDaysRet;
            }
            
        }
        public List<SOAModel.FundPendingChanges> GetPendingFundChangeByContractMigrated()
        {
            return _ContractDC.FwGetPendingFundChangeByContractMigrated();
        }
        public DataSet GetCommunicationInfoByContract()
        {
            DataSet ds;
            var obj = new FundContractsSoa();
            ds = obj.GetContractsDataSet(_sConId, _sSubId);
            return ds;

        }
        private string Px21AssetGroup(int a_iAssetId)
        {
            switch (a_iAssetId)
            {
                case 40:
                case 41:
                case 42:
                    {
                        return "Bonds/Cash";
                    }

                default:
                    {
                        return "Stocks";
                    }
            }
        }
        public void SendErrorEmail(MO.FundWizardInfo.FmrsUpdateReturn a_oResults)
        {
            var oMail = new MimeMessage();
            var strStatus = new StringBuilder();

            string sFromEmail = TrsAppSettings.AppSettings.GetValue("FWFromEmail");
            string sToEmail = TrsAppSettings.AppSettings.GetValue("FWToErrorEmail"); // "hao.dinh@transamerica.com"

            if (string.IsNullOrEmpty(sFromEmail))
            {
                sFromEmail = "TRSFundWizard@Transamerica.com";
            }

            if (string.IsNullOrEmpty(sToEmail))
            {
                sToEmail = "hao.dinh@transamerica.com";
            }

            oMail.From.Add(new MailboxAddress("", sFromEmail));

            foreach (string sE in sToEmail.Split(';'))
            {
                if (!string.IsNullOrEmpty(sE.Trim()))
                {
                    oMail.To.Add(new MailboxAddress("", sE.Trim()));
                }
            }

            oMail.Subject = "Fund Wizard " + _sConId + "*" + _sSubId;

            strStatus.Append("SessionID: " + _sSessionId.ToString() + Environment.NewLine);

            if (!(a_oResults.xInputXml == null))
            {
                strStatus.Append("xInputXml: " + a_oResults.xInputXml.ToString() + Environment.NewLine);
            }

            if (!(a_oResults.xResult == null))
            {
                strStatus.Append("xResult: " + a_oResults.xResult.ToString() + Environment.NewLine);
            }

            if (!(a_oResults.xOutputXml == null))
            {
                strStatus.Append("xOutputXml: " + a_oResults.xOutputXml.ToString() + Environment.NewLine);
            }

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.TextBody = strStatus.ToString();
            oMail.Body = bodyBuilder.ToMessageBody();

            TRSManagers.MailManager.SendEmail(oMail);
        }
        private string GetSaveData2Xml()
        {
            var strB = new StringBuilder();
            var swH = new StringWriter();
            var swA = new StringWriter();
            var swD = new StringWriter();
            var swMA = new StringWriter();

            var swACustomPX = new StringWriter();

            UpdateDDnFFPartnerFundId();

            strB.Append("<?xml version=\"1.0\"?> <FWUpdate>");
            // Header Info
            strB.Append("<PdfHdr>");
            PdfHeader.WriteXml(swH);
            strB.Append(swH.ToString());
            strB.Append("</PdfHdr>" + Environment.NewLine);

            UpdateTableForFundMapping();
            UpdateTableForFundMappingCustomPX();
            UpdateTableForManagedAdvice();

            strB.Append("<NewFund>");
            NewFunds.WriteXml(swA);
            strB.Append(swA.ToString());
            strB.Append("</NewFund>" + Environment.NewLine);

            strB.Append("<NewFundCustomPX>");
            NewFundsCustomPX.WriteXml(swACustomPX);
            strB.Append(swACustomPX.ToString());
            strB.Append("</NewFundCustomPX>" + Environment.NewLine);

            strB.Append("<ManagedAdvice>");
            ManagedAdvice.WriteXml(swMA);
            strB.Append(swMA.ToString());
            strB.Append("</ManagedAdvice>" + Environment.NewLine);

            strB.Append("</FWUpdate>");


            return strB.ToString();
        }
        private void UpdateDDnFFPartnerFundId()
        {
            var strTmp = new string[2];
            string partner_FundID = "";
            string sAbbrev = "";


            strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_default_fund_new, PdfHeader);
            if (!string.IsNullOrEmpty(strTmp[0]))
            {
                GetPartnerFundID(strTmp[0], ref sAbbrev, ref partner_FundID);
                AddPdfRow(PdfHeader, FWUtils.C_hdr_default_fund_new + "_partner_id", sAbbrev, partner_FundID);
                strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_default_fund, PdfHeader);
                if (!string.IsNullOrEmpty(strTmp[0]))
                {
                    GetPartnerFundID(strTmp[0], ref sAbbrev, ref partner_FundID);
                    AddPdfRow(PdfHeader, FWUtils.C_hdr_default_fund + "_partner_id", sAbbrev, partner_FundID);
                }
            }

            strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_forfeiture_fund_new, PdfHeader);
            if (!string.IsNullOrEmpty(strTmp[0]))
            {
                GetPartnerFundID(strTmp[0], ref sAbbrev, ref partner_FundID);
                AddPdfRow(PdfHeader, FWUtils.C_hdr_forfeiture_fund_new + "_partner_id", sAbbrev, partner_FundID);
                strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_forfeiture_fund, PdfHeader);
                if (!string.IsNullOrEmpty(strTmp[0]))
                {
                    GetPartnerFundID(strTmp[0], ref sAbbrev, ref partner_FundID);
                    AddPdfRow(PdfHeader, FWUtils.C_hdr_forfeiture_fund + "_partner_id", sAbbrev, partner_FundID);
                }
            }

        }
        private void UpdateTableForFundMapping()
        {
            int iI, iCnt;
            string partner_FundID = "";
            string sAbbrev = "";
            string sFMRS_PartnerID = "";
            sFMRS_PartnerID = TranslatePartnerID();
            iCnt = NewFunds.Rows.Count - 1;
            if (_xDoc == null)
            {
                LoadFMRS();
            }
            var loopTo = iCnt;
            for (iI = 0; iI <= loopTo; iI++)
            {
                if (!(NewFunds.Rows[iI]["fund_id"] is DBNull))
                {

                    GetPartnerFundID(NewFunds.Rows[iI]["fund_id"].ToString(), ref sAbbrev, ref partner_FundID);
                    NewFunds.Rows[iI]["Abbrev_fund_name"] = sAbbrev;
                    NewFunds.Rows[iI]["partner_fund_id"] = partner_FundID;

                }

                partner_FundID = "";
                sAbbrev = "";

                if (!(NewFunds.Rows[iI]["to_fund_id"] is DBNull))
                {

                    GetPartnerFundID(NewFunds.Rows[iI]["to_fund_id"].ToString(), ref sAbbrev, ref partner_FundID);
                    NewFunds.Rows[iI]["to_Abbrev_fund_name"] = sAbbrev;
                    NewFunds.Rows[iI]["to_partner_fund_id"] = partner_FundID;
                }

            }
            NewFunds.AcceptChanges();
        }
        private void UpdateTableForFundMappingCustomPX()
        {
            int iI, iCnt;
            string partner_FundID = "";
            string sAbbrev = "";
            string sFMRS_PartnerID = "";
            sFMRS_PartnerID = TranslatePartnerID();
            iCnt = NewFundsCustomPX.Rows.Count - 1;
            if (_xDoc == null)
            {
                LoadFMRS();
            }
            var loopTo = iCnt;
            for (iI = 0; iI <= loopTo; iI++)
            {
                if (!(NewFundsCustomPX.Rows[iI]["fund_id"] is DBNull))
                {

                    GetPartnerFundID(NewFundsCustomPX.Rows[iI]["fund_id"].ToString(), ref sAbbrev, ref partner_FundID);
                    NewFundsCustomPX.Rows[iI]["Abbrev_fund_name"] = sAbbrev;
                    NewFundsCustomPX.Rows[iI]["partner_fund_id"] = partner_FundID;

                }

                partner_FundID = "";
                sAbbrev = "";

                if (!(NewFundsCustomPX.Rows[iI]["to_fund_id"] is DBNull))
                {

                    GetPartnerFundID(NewFundsCustomPX.Rows[iI]["to_fund_id"].ToString(), ref sAbbrev, ref partner_FundID);
                    NewFundsCustomPX.Rows[iI]["to_Abbrev_fund_name"] = sAbbrev;
                    NewFundsCustomPX.Rows[iI]["to_partner_fund_id"] = partner_FundID;
                }

            }
            NewFundsCustomPX.AcceptChanges();
        }
        private void UpdateTableForManagedAdvice()
        {
            string a_rowId = "0";
            int iCnt;
            string sFMRS_PartnerID = "";
            sFMRS_PartnerID = TranslatePartnerID();
            iCnt = NewFundsCustomPX.Rows.Count - 1;
            if (_xDoc == null)
            {
                LoadFMRS();
            }

            var drw = ManagedAdvice.Rows.Find(a_rowId);
            if (!(drw == null))
            {
                ManagedAdvice.Rows.Find(a_rowId).Delete();
                if (!(ManagedAdvice.Rows.Find(_oConInfo.ContractID) == null))
                {
                    ManagedAdvice.Rows.Find(_oConInfo.ContractID).Delete();
                }
                drw = ManagedAdvice.NewRow();
                drw["ContractId"] = _oConInfo.ContractID;

                if (_oConInfo.MA.MAMethod == TRS.IT.SI.BusinessFacadeLayer.Model.E_MAMethodType.PARTICIPANTS | _oConInfo.MA.MAMethod == TRS.IT.SI.BusinessFacadeLayer.Model.E_MAMethodType.VOLUNTARY)
                {
                    drw["ma_selected"] = true;
                }
                else
                {
                    drw["ma_selected"] = false;
                }

                drw["ma_conversion_method"] = _oConInfo.MA.MAMethod;
                drw["ma_qdia"] = _oConInfo.MA.MAPptGroup.MAasQDIA;
                drw["ma_tdf"] = _oConInfo.MA.MAPptGroup.MapTDFPptInMA;
                drw["ma_px"] = _oConInfo.MA.MAPptGroup.MapPXPptInMA;
                drw["ma_fee"] = _oConInfo.MA.MAFee;
                drw["ma_effective_date"] = _oConInfo.MA.EffectiveDate;
                drw["ma_start_date"] = _oConInfo.MA.MAStartDate;
                drw["ma_free_look_days"] = _oConInfo.MA.FreeLookDays;
                ManagedAdvice.Rows.Add(drw);

            }
            ManagedAdvice.AcceptChanges();
        }
        private string TranslatePartnerID()
        {
            if (PartnerId == null)
            {
                return "";
            }
            switch (PartnerId.ToUpper() ?? "")
            {
                case "DIA":
                    {
                        return "1200";
                    }
                case "TAE":
                    {
                        return "200";
                    }
                case "ISC":
                    {
                        return "1300";
                    }

                default:
                    {
                        return "";
                    }
            }

        }
        public MO.FundWizardInfo.FmrsUpdateReturn UpdateFMRS(int a_iFundChangeType)
        {
            var oFmrsUpdateReturn = new MO.FundWizardInfo.FmrsUpdateReturn();
            var xRoot = new XElement("UpdateResponse");
            var oFundInfo = new FundInfoSoa();
            var xElV = new XElement("Validation");
            var xElP = new XElement("Pegasys");
            var xElW = new XElement("Wizard");
            var xElT = new XElement("ProcessTime");
            string sEffectiveDt = string.Empty;
            var dtStart = DateTime.Now;
            DateTime dtEnd;
            TimeSpan elapsedTime;
            try
            {
                xElT.Add(new XAttribute("Start", dtStart.ToString("MM/dd/yyyy: hh:mm:ss")));
                sEffectiveDt = Convert.ToString(DateTime.Parse(_sPegasysDt));
                var xInputXml = GetFMRSUpdateInput(DateTime.Parse(sEffectiveDt), a_iFundChangeType);
                string sOutputXml = oFundInfo.UpdateFmrsFundLineup(xInputXml.ToString());

                var xResults = XDocument.Load(new StringReader(sOutputXml));
                var xEl = xResults.XPathSelectElement("/FMRS/Results/Rules");

                oFmrsUpdateReturn.xInputXml = xInputXml;
                oFmrsUpdateReturn.xOutputXml = XElement.Load(new StringReader(sOutputXml));

                if (xEl.Attribute("Pass").Value == "1")
                {
                    xElV.Add(new XAttribute("Error", "0"));
                }
                else
                {
                    xElV.Add(new XAttribute("Error", "-1"));
                    foreach (XElement xR in xResults.XPathSelectElements("/FMRS/Results/Rules/Rule"))
                    {
                        if (xR.Attribute("Pass").Value == "0")
                        {
                            xElV.Add(new XElement("Rule", new XAttribute(xR.Attribute("FundGroupIDConcat")), new XAttribute(xR.Attribute("Description"))));
                        }
                    }
                }
                xRoot.Add(xElV);
                xEl = xResults.XPathSelectElement("/FMRS/Results/Pegasys");
                if (!(xEl == null))
                {
                    if (xEl.Attribute("Success").Value == "1") // Good no erros
                    {
                        xElP.Add(new XAttribute("Error", "0"));
                    }
                    else
                    {
                        xElP.Add(new XAttribute("Error", "-1"));
                        foreach (XElement xP in xResults.XPathSelectElements("/FMRS/Results/Pegasys/Results/Result"))
                        {
                            xElP.Add(new XElement("Fund", new XAttribute("FundID", CheckNull(xP.Attribute("FundID"))), new XAttribute(xP.Attribute("Msg"))));
                        }
                    }
                }
                else
                {
                    throw new Exception("Missing Pegasys status node.");
                }

                xRoot.Add(xElP);
                xElW.Add(new XAttribute("Error", "0"));
                xRoot.Add(xElW);
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                xElW.Add(new XAttribute("Error", "-1"), new XAttribute("ErrorMsg", ex.Message));
                xRoot.Add(xElW);
                oFmrsUpdateReturn.xResult = xRoot;
            }
            dtEnd = DateTime.Now;
            elapsedTime = dtEnd.Subtract(dtStart);
            xElT.Add(new XAttribute("End", dtEnd.ToString("MM/dd/yyyy: hh:mm:ss")));
            xElT.Add(new XAttribute("ElapsedTime", elapsedTime.TotalSeconds.ToString("0.000000")));
            xRoot.Add(xElT);

            oFmrsUpdateReturn.xResult = xRoot;
            return oFmrsUpdateReturn;

        }
        private string GetFMRSInput(bool bActiveFundList = false, bool bRestriction = false)
        {
            return GetFMRSHdrInput(1, 1, bActiveFundList, bRestriction).ToString();
        }
        private XElement GetFMRSHdrInput(int a_iType, int a_iFundChangeType, bool bActiveFundList = false, bool bRestriction = false)
        {
            XElement xEl;
            string sUser = UserName;
            var strTmp = new string[2];
            string sAsOfDate = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
            string sLineupDate = Convert.ToDateTime(CreateDate).ToString("yyyy-MM-ddTHH:mm:ssZ");

            if (sUser.Length > 30)
            {
                sUser = UserName.Substring(0, 29);
            }
            if (bRestriction)
            {
                if (!bActiveFundList)
                {
                    sAsOfDate = DateTime.Now.ToString("yyyy-MM-ddT00:00:00Z");
                }
                else
                {
                    var gen = new GeneralService();
                    sAsOfDate = string.Format("{0:yyyy-MM-dd}", DateTime.Parse(gen.GetLastBusinessDay()));
                }
            }

            xEl = new XElement("FMRS", new XAttribute("Type", "FundLineup"), new XAttribute("AsOfDate", sAsOfDate), new XAttribute("LineupDate", sLineupDate), new XElement("Contract", new XAttribute("ContractID", _sConId), new XAttribute("SubID", _sSubId)), new XElement("User", new XAttribute("UsrName", sUser)));

            switch (a_iType)
            {
                case 1: // FundLineup
                    {
                        if (!bRestriction)
                        {
                            if (bActiveFundList)
                            {
                                xEl.Add(new XElement("Application", new XAttribute("ApplicationID", "1033")));
                            }
                            else
                            {
                                xEl.Add(new XElement("Application", new XAttribute("ApplicationID", "80")));
                            }
                        }
                        else // applicationID Active 1 + notes 1024
                        {
                            xEl.Add(new XElement("Application", new XAttribute("ApplicationID", "1025")));
                        }

                        break;
                    }
                case 2: // Update
                    {
                        xEl.Attribute("Type").Value = "FundLineupUpdate";
                        xEl.Add(new XElement("Session", new XAttribute("SessionID", _sSessionId)));
                        if (!bRestriction)
                        {
                            xEl.Add(new XElement("Application", new XAttribute("ApplicationID", "64")));
                        }
                        else
                        {
                            xEl.Add(new XElement("Application", new XAttribute("ApplicationID", "1")));
                        }
                        xEl.Add(new XAttribute("FundChangeType", a_iFundChangeType.ToString()));

                        strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_default_fund_new, PdfHeader);
                        if (!string.IsNullOrEmpty(strTmp[0]) && strTmp[0] != "-1")
                        {
                            xEl.Element("Contract").Add(new XAttribute("DefaultFundID", strTmp[0]));


                            strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_default_fund_qdia_answer, PdfHeader);
                            if (!string.IsNullOrEmpty(strTmp[0]))
                            {
                                xEl.Element("Contract").Add(new XAttribute("QDIASelect", strTmp[0] == "Yes" ? "0001" : "0000"));
                                xEl.Element("Contract").Add(new XAttribute("QDIAStartDate", DateTime.Now.AddDays(30d).ToString("yyyy-MM-ddT00:00:00Z")));
                            }
                            else
                            {
                                xEl.Element("Contract").Add(new XAttribute("QDIASelect", "0000"));
                            }
                            strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_default_fund_tmf_select, PdfHeader);
                            if (!string.IsNullOrEmpty(strTmp[0]))
                            {
                                xEl.Element("Contract").Add(new XAttribute("TMFSelect", strTmp[0] == "Yes" ? "0001" : "0000"));
                            }
                            else
                            {
                                xEl.Element("Contract").Add(new XAttribute("TMFSelect", "0000"));
                            }

                        }

                        strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_forfeiture_fund_new, PdfHeader);
                        if (!string.IsNullOrEmpty(strTmp[0]))
                        {
                            xEl.Element("Contract").Add(new XAttribute("ForfeitureFundID", strTmp[0]));
                        }

                        // PortfolioXpress
                        strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_portXpress_selected, PdfHeader);
                        if (!string.IsNullOrEmpty(strTmp[0]) && strTmp[0] == "true")
                        {
                            var xElPx = new XElement("PortfolioExpress");
                            xElPx.Add(new XAttribute("RuleMaterialChangePass", "1"));

                            // Default
                            strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_default_fund_new, PdfHeader);
                            if (strTmp[0] == "-1")
                            {
                                xElPx.Add(new XAttribute("DefaultFund", "true"));
                            }
                            else
                            {
                                xElPx.Add(new XAttribute("DefaultFund", "false"));
                            }
                            // QDIA
                            strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_default_fund_qdia_answer, PdfHeader);
                            if (!string.IsNullOrEmpty(strTmp[0]) && strTmp[0] == "Yes")
                            {
                                xElPx.Add(new XAttribute("QDIA", "true"));
                            }
                            else
                            {
                                xElPx.Add(new XAttribute("QDIA", "false"));
                            }

                            // FiduciaryType
                            strTmp = FWUtils.GetHdrData(FWUtils.C_hdr_PortXpress_fiduciary_type_select, PdfHeader);
                            if (!string.IsNullOrEmpty(strTmp[0]) && strTmp[0] == "1") // Note: update only when = 1
                            {
                                xElPx.Add(new XAttribute("FiduciaryType", "1"));
                            }

                            xEl.Element("Contract").Add(new XElement(xElPx));

                        }

                        break;
                    }
            }

            return xEl;
        }
        private XElement GetFMRSUpdateInput(DateTime a_dtEffectiveDate, int a_iFundChangeType)
        {
            var xEl = GetFMRSHdrInput(2, a_iFundChangeType);
            var xElFund = GetFMRSFundListInput(a_dtEffectiveDate);

            if (xElFund.HasElements)
            {
                xEl.Add(GetFMRSFundListInput(a_dtEffectiveDate));
            }

            return xEl;
        }
        private XElement GetFMRSFundListInput(DateTime a_dtEffectiveDate)
        {
            var xFundList = new XElement("FundList");
            XElement el;
            foreach (DataRow dr in _tbFundPending.Rows)
            {
                el = new XElement("Fund", new XAttribute("FundID", dr["fund_id"]), new XAttribute("Status", dr["action"]), new XAttribute("StartDate", a_dtEffectiveDate.ToString("yyyy-MM-ddTHH:mm:ssZ")), new XAttribute("EffectiveDate", DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")));
                if (el.Attribute("Status").Value == "2")
                {
                    el.Add(new XAttribute("TransferToFundID", dr["to_fund_id"]));
                }
                xFundList.Add(el);
            }
            return xFundList;
        }
        private void AddPdfRow(DataTable a_tb, string a_sRowId, string a_sRowDesc, string a_sRowVal)
        {
            var drw = a_tb.Rows.Find(a_sRowId);
            if (drw == null)
            {
                drw = a_tb.NewRow();
                drw["row_id"] = a_sRowId;
                drw["row_desc"] = a_sRowDesc;
                drw["row_val"] = a_sRowVal;
                a_tb.Rows.Add(drw);
            }
            else
            {
                drw["row_desc"] = a_sRowDesc;
                drw["row_val"] = a_sRowVal;
            }
        }
        private XElement GetUserProfile()
        {
            var el = new XElement("UserProfile", new XAttribute("UserName", UserName), new XAttribute("CreateDt", DateTime.Now.ToString()));
            return el;
        }
        public XElement GetFileProfile(string a_sPath, string a_sFileName, string a_sPromptName, bool a_bMsgCtr, string a_sEsignTransID = "")
        {
            var el = new XElement("FileProfile", new XAttribute("FilePath", a_sPath), new XAttribute("FileName", a_sFileName), new XAttribute("PromptName", a_sPromptName), new XAttribute("MsgCtr", a_bMsgCtr.ToString().ToLower()));

            if (!string.IsNullOrEmpty(a_sEsignTransID))
            {
                el.Add(new XAttribute("ESignTransID", a_sEsignTransID));
            }

            return el;
        }
        private XElement GetRootTask()
        {
            var xRoot = new XElement("FWTasks");
            return xRoot;
        }
        public int InsertTask(MO.FundWizardInfo.FwTaskTypeEnum a_iTaskType, int a_iStatus, XElement[] a_xEl)
        {
            var xRoot = GetRootTask();
            foreach (XElement el in a_xEl)
            {
                xRoot.Add(el);
            }

            xRoot.Add(GetUserProfile());
            return _ContractDC.FwInsertTask(CaseNo, (int)a_iTaskType, a_iStatus, xRoot.ToString());
        }
        public string CheckNull(XAttribute a_xAtt)
        {
            if (!(a_xAtt == null))
            {
                return a_xAtt.Value;
            }
            else
            {
                return "";
            }
        }
        private string GetQDIAText(string a_sElig)
        {
            if (a_sElig == "1")
            {
                return "[QDIA]";
            }
            else
            {
                return string.Empty;
            }
        }
        public void SetDefaults(XDocument a_xDoc)
        {
            var xEl = a_xDoc.XPathSelectElement("/FMRS/Contract");
            XElement xTmpEl;
            string sTemp = string.Empty;
            string sFundName = string.Empty;
            if (!(xEl == null))
            {

                sTemp = CheckNull(xEl.Attribute("DefaultFundID"));
                if (!string.IsNullOrEmpty(sTemp))
                {
                    xTmpEl = xEl.XPathSelectElement("//Fund[@FundID=" + sTemp + "]");
                    if (!(xTmpEl == null))
                    {
                        sFundName = xTmpEl.Element("Name").Value;
                        // AddPdfRow(PdfHeader, "default_fund_name", "Default Fund Name", xTmpEl.Element("Name").Value)
                    }
                    AddPdfRow(PdfHeader, FWUtils.C_hdr_default_fund, sFundName, sTemp);
                }
                sTemp = CheckNull(xEl.Attribute("ForfeitureFundID"));
                if (!string.IsNullOrEmpty(sTemp))
                {
                    xTmpEl = xEl.XPathSelectElement("//Fund[@FundID=" + sTemp + "]");
                    if (!(xTmpEl == null))
                    {
                        sFundName = xTmpEl.Element("Name").Value;
                    }
                    AddPdfRow(PdfHeader, FWUtils.C_hdr_forfeiture_fund, sFundName, sTemp);
                }
                // QDIASelect
                sTemp = CheckNull(xEl.Attribute("QDIASelect"));
                if (!string.IsNullOrEmpty(sTemp))
                {
                    AddPdfRow(PdfHeader, FWUtils.C_hdr_qdia_select, "QDIA Select", Convert.ToString(sTemp == "0001" ? "Yes" : "No"));
                }
                // TMF
                sTemp = CheckNull(xEl.Attribute("TMFSelect"));
                if (!string.IsNullOrEmpty(sTemp))
                {
                    AddPdfRow(PdfHeader, FWUtils.C_hdr_default_fund_tmf_select, "TMF Select", Convert.ToString(sTemp == "0001" ? "Yes" : "No"));
                }
                // Mesirow
                sTemp = CheckNull(xEl.Attribute("FiduciaryServicesProviderID"));
                if (!string.IsNullOrEmpty(sTemp) && sTemp != "0")
                {
                    AddPdfRow(PdfHeader, FWUtils.C_hdr_fiduciary_services_ProviderID, "FiduciaryServicesProviderID", sTemp);
                }

                // PortfolioXpress Default
                if (xEl.HasElements)
                {
                    var xElPX = a_xDoc.XPathSelectElement("/FMRS/Contract/PortfolioExpress");
                    if (!(xElPX == null))
                    {
                        sTemp = CheckNull(xElPX.Attribute("Selected"));
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            AddPdfRow(PdfHeader, FWUtils.C_hdr_portXpress_selected, "PortfolioXpress Selected", sTemp.ToLower());
                        }

                        sTemp = CheckNull(xElPX.Attribute("Custom"));
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            AddPdfRow(PdfHeader, FWUtils.C_hdr_PortXpress_custom, "PortfolioXpress Custom", sTemp.ToLower());
                        }
                        sTemp = CheckNull(xElPX.Attribute("DefaultFund"));
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            if (sTemp == "true")
                            {
                                AddPdfRow(PdfHeader, FWUtils.C_hdr_default_fund, "PortfolioXpress", "-1");
                            }
                        }
                        sTemp = CheckNull(xElPX.Attribute("GlidePath"));
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            if (sTemp == "1")
                            {
                                AddPdfRow(PdfHeader, FWUtils.C_hdr_PortXpress_glidepath, "PortfolioXpress GlidePath - Lifetime", sTemp);
                            }
                            else if (sTemp == "2")
                            {
                                AddPdfRow(PdfHeader, FWUtils.C_hdr_PortXpress_glidepath, "PortfolioXpress GlidePath - Retirement", sTemp);
                            }
                            else
                            {
                                AddPdfRow(PdfHeader, FWUtils.C_hdr_PortXpress_glidepath, "PortfolioXpress GlidePath", sTemp);
                            }

                        }
                        sTemp = CheckNull(xElPX.Attribute("QDIA"));
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            AddPdfRow(PdfHeader, FWUtils.C_hdr_qdia_select, "PortfolioXpress QDIA", Convert.ToString(sTemp == "true" ? "Yes" : "No"));
                        }

                        sTemp = CheckNull(xElPX.Attribute("RiskPreference"));
                        if (!string.IsNullOrEmpty(sTemp))
                        {
                            AddPdfRow(PdfHeader, FWUtils.C_hdr_PortXpress_RiskPreference, "PortfolioXpress RiskPreference", sTemp);
                        }

                    }
                }

            }
        }
        private void RemoveNotNeededTags(ref XDocument a_xDoc)
        {
            string[] sRemoveEl = ["RiskMeasures", "Performance", "MPT", "Score", "InvestmentInformation", "InvestmentStragegy", "Platform"];   // , "Fees"
            string[] sRemoveAtt = ["IndexID", "IndexName", "FundGroupIDConcat", "PDFFundSheet", "InceptionDate"];

            a_xDoc.XPathSelectElements("//Indices").Remove();
            foreach (XElement xE in a_xDoc.XPathSelectElements("//Fund"))
            {
                foreach (string s in sRemoveEl)
                {
                    xE.Elements(s).Remove();
                }

                foreach (string att in sRemoveAtt)
                {
                    xE.Attributes((XName)att).Remove();
                }
            }

        }
        public string GetPegasysMessage(MO.FundWizardInfo.FmrsUpdateReturn oResult)
        {
            string GetPegasysMessageRet = default;
            GetPegasysMessageRet = "";
            // Dim xDoc As XDocument = XDocument.Load("C:\temp\Pegasys.xml")
            // oResult.xResult = xDoc.Root
            try
            {
                if (oResult.xResult.Element("Pegasys").Attribute("Error").Value == "-1")
                {
                    foreach (XElement xNode in oResult.xResult.Element("Pegasys").Elements())
                    {
                        GetPegasysMessageRet += xNode.Attribute("FundID").Value + " - " + xNode.Attribute("Msg").Value + "</br>";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                GetPegasysMessageRet += "Exception raised: " + ex.Message;
            }

            return GetPegasysMessageRet;
        }
        private bool bBlackOut(XElement x)
        {
            bool bBlackOutRet = false;

            if (x.Attributes("LiquidationDate").Any())
            {
                var dtLiquidation = Convert.ToDateTime(x.Attribute("LiquidationDate").Value);
                if ((dtLiquidation - DateTime.Today).Days < iBlackOutDays(dtLiquidation))
                {
                    bBlackOutRet = true;
                }
            }
            return bBlackOutRet;
        }
        private bool GetisAddMA()
        {
            // Purpose to determine if a "Add MA" type request is submitted
            bool isAddMA = false;
            var xDoc = new XmlDocument();
            xDoc.LoadXml(GetSaveData2Xml()); // Load the pdfHdr xml
            if (xDoc.GetElementsByTagName("ManagedAdvice").Count > 0)
            {
                isAddMA = !hasOnlyAndEmptyChild(xDoc.GetElementsByTagName("ManagedAdvice").Item(0));
            }
            return isAddMA;
        }
        private bool GetisRemovePX()
        {
            // Purpose to determine if a "Add Remove PX" type request is submitted
            bool isRemovePX = false;
            if (FWUtils.GetHdrData("PortXpressRequestRemoval ", PdfHeader).Length > 0)
            {
                isRemovePX = FWUtils.GetHdrData(FWUtils.C_hdr_PortXpress_removal, PdfHeader)[0].ToString().ToUpper() == "TRUE";
            }
            return isRemovePX;
        }
        private bool GetisNewDefaultInvestmentChoice()
        {
            bool isNewDefaultInvestmentChoice = false;
            string NewDefaultInvestmentChoiceVal = FWUtils.GetHdrData(FWUtils.C_hdr_default_fund_new, PdfHeader)[0].ToString().ToUpper();
            isNewDefaultInvestmentChoice = NewDefaultInvestmentChoiceVal != "TRUE" & !string.IsNullOrEmpty(NewDefaultInvestmentChoiceVal);
            return isNewDefaultInvestmentChoice;
        }
        private bool GetisInvestmentChangeRequested()
        {
            bool isInvestmentChangeRequested = false;
            var xDoc = new XmlDocument();
            xDoc.LoadXml(GetSaveData2Xml()); // Load the pdfHdr xml
            if (xDoc.GetElementsByTagName("NewFund").Count > 0)
            {
                isInvestmentChangeRequested = !hasOnlyAndEmptyChild(xDoc.GetElementsByTagName("NewFund").Item(0));
            }
            return isInvestmentChangeRequested;
        }
        private bool hasOnlyAndEmptyChild(XmlNode element)
        {
            if (element.HasChildNodes)
            {
                return element.ChildNodes.Count == 1 & element.ChildNodes[0].Name == "DocumentElement" & !element.ChildNodes[0].HasChildNodes;
            }
            else
            {
                return false;
            }
        }
    }
}