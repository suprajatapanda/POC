using System.Data;
using Aspose.Words;
using System.Text;
using System.Xml.Linq;
using TRS.IT.BendProcessor.Util;
using TRS.IT.SOA.Model.PreSales.FundLineupData;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
using FWBamlDocsToWMSBatch.DAL;
using TARSharedUtilLib.Utility;
using FWBamlDocsToWMSBatch.Model;

namespace FWBamlDocsToWMSBatch.BLL
{
    /// <summary>
    /// This class is to generate BAML pending document that to be delivered to WMS
    /// </summary>
    public class BAMLFundDocGen : TRS.IT.BendProcessor.Model.BendProcessorBase
    {

        private const string CONST_JOB_NAME = "BAMLFundDocGen";
        private const string CONST_RUN_TYPE = "RUN";
        private const string CONST_APPLICATIONID = "16";
        private const int CONST_RUN_COMPLETE = 1;
        private string PSFPlans = AppSettings.GetValue("PSFPlans");
        StringBuilder _log = new StringBuilder();
        List<BamlFundInfo> FilesUploaded = new List<BamlFundInfo>();

        ScheduleDC _oScheduleDC = new ScheduleDC();
        FWBendDC _oFWDC = new FWBendDC();

        public BAMLFundDocGen() : base("140", "BAMLFundDocGen", "TRS") { }
        public BAMLFundDocGen(string a_sJobId, string a_sJobName, string a_sPartnerId) : base(a_sJobId, a_sJobName, a_sPartnerId) { }

        /// <summary>
        /// Generate the report and deliver to WMS
        /// </summary>
        /// <returns></returns>
        public TaskStatus Start()
        {
            TaskStatus oTaskReturn = new TaskStatus();
            InitTaskStatus(oTaskReturn, "Start");

            DataSet dsRunSchD = _oScheduleDC.GetScheduleDRunDays(CONST_JOB_NAME);
            if (dsRunSchD != null && dsRunSchD.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow drRunSchD in dsRunSchD.Tables[0].Rows)
                {
                    ProcessDocuments(Convert.ToDateTime(drRunSchD["run_dt"].ToString()).AddDays(1));
                    _oScheduleDC.SetScheduleDStatus(CONST_JOB_NAME, Convert.ToInt32(drRunSchD["sch_id"]), CONST_RUN_COMPLETE, CONST_RUN_TYPE.ToUpper());
                    oTaskReturn.rowsCount += 1;
                }
                SendStatusEmail();
                oTaskReturn.retStatus = TRS.IT.BendProcessor.Model.TaskRetStatus.Succeeded;
            }
            else
            {
                oTaskReturn.retStatus = TRS.IT.BendProcessor.Model.TaskRetStatus.NotRun;
            }
            return oTaskReturn;
        }

        /// <summary>
        /// Post BAML fund reports to WMS
        /// </summary>
        /// <returns></returns>
        private TaskStatus ProcessDocuments(DateTime runDate)
        {
            TaskStatus oTaskReturn = new TaskStatus();
            InitTaskStatus(oTaskReturn, "ProcessDocuments");

            DataSet ds = _oFWDC.GetBAMLFundChange(runDate);
            if (ds.Tables[0].Rows.Count > 0)
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    BamlFundInfo oBamlFundInfo = new BamlFundInfo(PSFPlans);
                    oBamlFundInfo.ContractID = dr["contract_id"].ToString();
                    oBamlFundInfo.SubID = dr["sub_id"].ToString();
                    oBamlFundInfo.MDPProductID = dr["MDP_productId"].ToString();
                    oBamlFundInfo.ProductID = dr["product_id"].ToString();
                    oBamlFundInfo.CaseNo = dr["case_no"].ToString();
                    oBamlFundInfo.EffectiveDate = Convert.ToDateTime(dr["pegasys_dt"].ToString());
                    oBamlFundInfo.NewFundList = GetNewFundsFromFWXML(dr["fund_data"].ToString(), "1");
                    oBamlFundInfo.DeletedFundList = GetNewFundsFromFWXML(dr["fund_data"].ToString(), "2");
                    if (oBamlFundInfo.NewFundList.Count > 0)
                    {
                        GenerateDocument(oBamlFundInfo);
                    }
                }
                oTaskReturn.rowsCount += 1;
                oTaskReturn.retStatus = TRS.IT.BendProcessor.Model.TaskRetStatus.Succeeded;
            }
            else
            {
                oTaskReturn.rowsCount = 0;
                oTaskReturn.retStatus = TRS.IT.BendProcessor.Model.TaskRetStatus.NotRun;
            }
            return oTaskReturn;
        }

        private List<int> GetNewFundsFromFWXML(string fwXML, string action)
        {
            XDocument xdoc = XDocument.Parse(fwXML);
            List<int> FundList = (from f in xdoc.Descendants("fwFundAdd")
                                  where f.Element("action").Value == action
                                  select Convert.ToInt32(f.Element("fund_id").Value)).ToList();
            return FundList;
        }

        private void SendStatusEmail()
        {
            StringBuilder mailBody = new StringBuilder();
            string Subject = "BAMLFundDocGen WMS Files Uploaded";
            foreach (BamlFundInfo fund in FilesUploaded)
            {
                mailBody.AppendFormat("{0}-{1} caseNo:{2} --> WMSfile:{3}<br />", fund.ContractID, fund.SubID, fund.CaseNo, fund.OutputFile);
            }
            Utils.SendMail(AppSettings.GetValue(TRS.IT.BendProcessor.Model.ConstN.C_BPROCESSOR_EMAIL), AppSettings.GetValue("TRSWebDevelopment"), Subject, mailBody.ToString());
        }

        private void GenerateDocument(BamlFundInfo oBamlFundInfo)
        {
            FMRS oFunds = FundWizard.GetFMRSFundsByContract(oBamlFundInfo.ContractID, oBamlFundInfo.SubID, oBamlFundInfo.EffectiveDate, CONST_APPLICATIONID);
            foreach (FundGroup_Type fg in oFunds.FundGroups[0].FundGroups)
            {
                if (fg.FundGroups != null)
                {
                    GetFund(oBamlFundInfo, fg.FundGroups);
                }
            }
            DeliveryDocumentToWMS(oBamlFundInfo);
        }

        private void GetFund(BamlFundInfo oBamlFundInfo, FundGroup_Type[] FundGroups)
        {
            foreach (FundGroup_Type fg1 in FundGroups)
            {
                if (fg1.FundGroups != null)
                {
                    GetFund(oBamlFundInfo, fg1.FundGroups);
                }
                if (fg1.FundList != null)
                {
                    foreach (Fund_Type fund in fg1.FundList)
                    {
                        BamlFund oBamlFund = new BamlFund();
                        oBamlFund.FundClass = fg1.Name;
                        oBamlFund.FundId = fund.FundID;
                        oBamlFund.FundName = fund.Name;
                        oBamlFund.PSF = fund.Fees.Disclosure.VAC;  //Plan Service Fee
                        oBamlFund.RevenueBps = fund.Fees.Disclosure.Other;
                        var xref = fund.XRef.Where(x => x.PartnerID == "1300" && x.FundCodeTypeID == "101").Select(y => y.FundID);
                        foreach (var x in xref)
                        {
                            oBamlFund.Descriptor = x;
                        }
                        oBamlFundInfo.FundList.Add(oBamlFund);
                    }
                }
            }
        }

        private bool DeliveryDocumentToWMS(BamlFundInfo BamlInfo)
        {
            bool success = false;
            if (BamlInfo.FundList.Count > 0)
            {
                string filePath = FileManagerSMB.Combine(AppSettings.GetValue("FWDocGenLocalPath"), string.Format("FundChange_{0}_{1}_{2}.doc", BamlInfo.ContractID, BamlInfo.SubID, DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss")));
                if (GenerateReportData(BamlInfo, filePath))
                {
                    TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse oSIResponse = WMSUpload(BamlInfo);
                    success = (oSIResponse.Errors[0].Number == 0);
                }
            }
            return success;
        }

        private TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse WMSUpload(BamlFundInfo oBamlInfo)
        {
            string sError = "";
            TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse oSIResponse = new TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse();
            List<TRS.IT.SI.Services.wsDocumentService.ImportFile> ImportFiles = new List<TRS.IT.SI.Services.wsDocumentService.ImportFile>();
            TRS.IT.SI.Services.wsDocumentService.ImportFileRequest oRequest = new TRS.IT.SI.Services.wsDocumentService.ImportFileRequest();
            oSIResponse.Errors[0].Number = 0;
            oRequest.SourceName = oBamlInfo.SourceName;

            TRS.IT.SI.Services.wsDocumentService.ImportFile WMSFile = new TRS.IT.SI.Services.wsDocumentService.ImportFile();
            WMSFile.FileName = FileManagerSMB.GetFileName(oBamlInfo.OutputFile);
            WMSFile.ContractID = oBamlInfo.ContractID;
            WMSFile.SubID = oBamlInfo.SubID;
            WMSFile.DocTypeCode = 791;
            WMSFile.BatchTypeCode = 10;

            WMSFile.FileContent = FileManagerSMB.ReadAllBytes(oBamlInfo.OutputFile);
            ImportFiles.Add(WMSFile);
            oRequest.ImportFiles = ImportFiles.ToArray();
            TRS.IT.SI.Services.wsDocumentService.ImportFileResponse oResponse = new TRS.IT.SI.Services.wsDocumentService.ImportFileResponse();

            SOA.DocumentService BFLDocumentService = new SOA.DocumentService();
            oResponse = BFLDocumentService.ImportFiles(ref oRequest);

            foreach (TRS.IT.SI.Services.wsDocumentService.ErrorInfo oE in oResponse.Errors)
            {
                if (oE.Number != 0)
                {
                    sError += oE.Description;
                }
            }
            if (!String.IsNullOrEmpty(sError))
            {
                oSIResponse.Errors[0].Number = 1;
                oSIResponse.Errors[0].Description = sError;
            }
            else
            {
                oSIResponse.Errors[0].Number = 0;
                FilesUploaded.Add(oBamlInfo);
            }
            return oSIResponse;
        }

        private bool GenerateReportData(BamlFundInfo BamlInfo, string fileName)
        {
            string templateDoc = string.Format("{0}FundChange.doc", BamlInfo.ReportName);
            bool success = true;
            Aspose.Words.License license = new Aspose.Words.License();
            license.SetLicense("Aspose.Total.lic");

            string docTemplatePath = FileManagerSMB.Combine(AppSettings.GetValue("FWDocGenTemplatePath"), templateDoc);
            Document doc = new Document(docTemplatePath);
            try
            {
                doc.MailMerge.Execute(new string[] { "ReportName", "ContractID", "SubID", "CaseNo", "EffectiveDate" },
                  new object[] { BamlInfo.ReportName, BamlInfo.ContractID, BamlInfo.SubID, BamlInfo.CaseNo, BamlInfo.EffectiveDate.ToShortDateString() }
                  );

                DataSet ds = BamlInfo.GetFundListToDataSet();
                if (ds.Tables[0].Rows.Count > 0)
                {
                    doc.MailMerge.ExecuteWithRegions(ds);
                    doc.Save(fileName, SaveFormat.Doc);
                    BamlInfo.OutputFile = fileName;
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                success = false;
                _log.Append(string.Format("BAMLFundDocGen issue at GenerateReportData- {0}\n", ex.Message));
            }
            return success;
        }
       
    }

}
