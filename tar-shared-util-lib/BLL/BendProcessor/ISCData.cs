using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Aspose.Words;
using Aspose.Words.MailMerging;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using License = Aspose.Words.License;
using SOAModel = TRS.IT.SOA.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace TRS.IT.BendProcessor.BLL
{
    public class ISCData : BendProcessorBase
    {
        private ISCDataDC _oISCDataDC;
        public ISCData() : base("96", "ISCData", "TRS") { _oISCDataDC = new ISCDataDC(); }
        public TaskStatus ProcessLateLoanLettersMigrated() // Monthly Task :  IMP: make sure in config file set monthly task startdate as mm/26/yyyy 
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn1 = new();
            ResultReturn oReturn2 = new();
            ResultReturn oReturn3 = new();

            const string C_Task = "ProcessLateLoanLetters";
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (TrsAppSettings.AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    StringBuilder sbErr = new();

                    DateTime start_date = DateTime.Now.AddDays(-1).Date;
                    DateTime end_date = DateTime.Now.Date;

                    string sLateLoanLettersTaskEnabledMonths = TrsAppSettings.AppSettings.GetValue("LateLoanLettersTaskEnabledMonths");
                    if (sLateLoanLettersTaskEnabledMonths == null || sLateLoanLettersTaskEnabledMonths == string.Empty)
                    {
                        sLateLoanLettersTaskEnabledMonths = "2|5|8|11";
                    }
                    string[] sAryMonths = sLateLoanLettersTaskEnabledMonths.Split('|');
                    if (sAryMonths != null && sAryMonths.Length > 0)
                    {
                        DateTime current_date = DateTime.Now;
                        int iCurrentMonth = current_date.Month;
                        int[] iAryMonths = Array.ConvertAll(sAryMonths, int.Parse);

                        if (iAryMonths != null && iAryMonths.Length > 0 && iAryMonths.Contains(iCurrentMonth))
                        {
                            // IMP: make sure in config file set monthly task startdate as mm/26/yyyy 
                            start_date = new DateTime(DateTime.Now.Year, iCurrentMonth, 21);
                            end_date = new DateTime(DateTime.Now.Year, iCurrentMonth, 26);

                            oReturn1 = GetLateLoanLettersData(start_date, end_date);
                        }

                        if (oReturn1.returnStatus != ReturnStatusEnum.Succeeded || oReturn1.Errors.Count > 0)
                        {
                            sbErr.Length = 0;
                            //send error
                            foreach (ErrorInfo oEr in oReturn1.Errors)
                            {
                                sbErr.AppendLine(oEr.errorDesc);
                            }
                            SendTaskCompleteEmail("GetLateLoanLetters Status - " + oReturn1.returnStatus.ToString(), sbErr.ToString(), oTaskReturn.taskName);

                            General.CopyResultError(oTaskReturn, oReturn1);
                            oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                        }

                        oTaskReturn.rowsCount++;
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
        public ResultReturn GetLateLoanLettersData(DateTime start_date, DateTime end_date)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            string sReponse = "";
            int iError = 0;
            string sError = "";
            string contract_id = "";
            string sub_id = "";
            string sDocType = "";
            int iDocType = 0;
            SOAModel.WsDocumentServiceDocumentEx oInputDoc;
            DriverSOA.ContractServ DriverSOACon = new();
            ResultReturn oRet;
            Hashtable htMEPContract = new();
            try
            {
                sReponse = _oISCDataDC.getLoanDefaultQtrlyReport(start_date, end_date);

                XElement xEl = XElement.Parse(sReponse); // format: <SIResponse><IsPending>0</IsPending><ConfirmationNumber>20150716130526</ConfirmationNumber><Errors><Error><Number>0</Number><Description></Description><Type>0</Type></Error></Errors><AdditionalData><Cases><Case cid="932115" sid="00066" type="LNDEFRPT" /><Case cid="932115" sid="00066" type="LNMATRPT" /><Case cid="932256" sid="00013" type="LNDEFRPT" /><Case cid="932256" sid="00013" type="LNMATRPT" /></Cases></AdditionalData><SessionID/><TransIDs>0</TransIDs></SIResponse>
                iError = CheckP3Error(xEl, ref sError);

                if (iError != 0)
                {
                    //failed
                    throw new Exception("Error in GetLateLoanLettersData: " + sError);
                }
                else
                {
                    foreach (XElement xElrow in xEl.Descendants("Case").ToList())
                    {
                        try
                        {
                            contract_id = ""; sub_id = ""; sDocType = ""; iDocType = 0;

                            if (xElrow.Attribute("cid") != null && xElrow.Attribute("cid").Value.Trim() != string.Empty)
                            {
                                contract_id = xElrow.Attribute("cid").Value.Trim();
                                sub_id = Utils.SubIn(xElrow.Attribute("sid").Value.Trim());
                                sDocType = xElrow.Attribute("type").Value.Trim();

                                switch (sDocType)
                                {
                                    case "LNDEFRPT":
                                        iDocType = 36500; //made up doc type code (365 * 10 = 3650)
                                        break;
                                    case "LNGRACERPT":
                                        iDocType = 36510; //made up doc type code ((365 * 10) + 1 = 3651)
                                        break;
                                    case "LNMATRPT":
                                        iDocType = 36520; //made up doc type code ((365 * 10) + 2 = 3652)
                                        break;
                                }

                                //---------To SendConsolidatedNotifications: -----------------
                                oInputDoc = new SOAModel.WsDocumentServiceDocumentEx();
                                oInputDoc.SourceName = "AWD"; // not from WMS
                                oInputDoc.ContractID = contract_id;
                                oInputDoc.SubID = sub_id;
                                oInputDoc.DocTypeCode = iDocType; // in future add additional data fields if needed
                                oInputDoc.DateReceived = start_date; // <-- this date will be fetched from pSI_GetMepDocs and based on this date we will display "quarter" display on website
                                oInputDoc.WmsTimeReceived = start_date;
                                oInputDoc.SystemTimeReceived = DateTime.Now;

                                // now insert in nt_inputdetails table via contract service
                                oReturn = new ResultReturn();
                                oRet = DriverSOACon.NotifyToConsolidateMessages(TRSManagers.XMLManager.GetXML(oInputDoc), "", "");

                                if (oRet.Errors.Count > 0)
                                {
                                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                                    oReturn.Errors.Add(new ErrorInfo(-1, "NotifyToConsolidateMessages Failed. Contract_id: " + contract_id + "sub_id:  " + sub_id + "iDocType: " + iDocType.ToString() + oRet.Errors[0].errorDesc + "\r\n", ErrorSeverityEnum.Failed));
                                }


                                if (sub_id != "000" && sub_id != "00000" && htMEPContract.ContainsKey(contract_id + "_" + iDocType.ToString()) == false)
                                {
                                    htMEPContract.Add(contract_id + "_" + iDocType.ToString(), iDocType);
                                }

                                //----------------------end-----------------------------------
                            }
                        }
                        catch (Exception exi)
                        {
                            Utils.LogError(exi);
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "Exception in GetLateLoanLettersData() while  processing: " + xElrow.ToString() + ". Error: " + exi.Message, ErrorSeverityEnum.ExceptionRaised));
                        }
                    }//foreach

                    // AWD creates the document at 000 level but getLoanDefaultQtrlyReport do not return the record at 000 level
                    // Hence manually create 000 level records for notification and display purpose...
                    string sTemp = "";
                    foreach (DictionaryEntry entry in htMEPContract)
                    {
                        try
                        {
                            sTemp = "";
                            sTemp = Convert.ToString(entry.Key);

                            if (sTemp.IndexOf("_") > -1)
                            {
                                oInputDoc = new SOAModel.WsDocumentServiceDocumentEx();
                                oInputDoc.SourceName = "AWD"; // not from WMS
                                oInputDoc.ContractID = sTemp.Substring(0, sTemp.IndexOf("_"));
                                oInputDoc.SubID = "000";
                                oInputDoc.DocTypeCode = Convert.ToInt32(entry.Value); // in future add additional data fields if needed
                                oInputDoc.DateReceived = start_date; // <-- this date will be fetched from pSI_GetMepDocs and based on this date we will display "quarter" display on website
                                oInputDoc.WmsTimeReceived = start_date;
                                oInputDoc.SystemTimeReceived = DateTime.Now;

                                // now insert in nt_inputdetails table via contract service
                                oReturn = new ResultReturn();
                                oRet = DriverSOACon.NotifyToConsolidateMessages(TRSManagers.XMLManager.GetXML(oInputDoc), "", "");

                                if (oRet.Errors.Count > 0)
                                {
                                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                                    oReturn.Errors.Add(new ErrorInfo(-1, "NotifyToConsolidateMessages Failed. Contract_id: " + contract_id + "sub_id:  " + sub_id + "iDocType: " + iDocType.ToString() + oRet.Errors[0].errorDesc + "\r\n", ErrorSeverityEnum.Failed));
                                }
                            }
                        }
                        catch (Exception exi2)
                        {
                            Utils.LogError(exi2);
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "Exception in GetLateLoanLettersData() while  processing: " + entry.Key.ToString() + ". Error: " + exi2.Message, ErrorSeverityEnum.ExceptionRaised));
                        }
                    }//foreach
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in GetLateLoanLetters  - start_date = " + start_date.ToString() + " end_date = " + end_date.ToString() + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }


            return oReturn;
        }
        public TaskStatus ProcessLoanPayoffNotificationsMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn1 = new();
            ResultReturn oReturn2 = new();
            ResultReturn oReturn3 = new();

            const string C_Task = "ProcessLoanPayoffNotifications";
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (TrsAppSettings.AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    StringBuilder sbErr = new();

                    DateTime start_date = DateTime.Now.AddDays(-1).Date;
                    DateTime end_date = DateTime.Now.Date;


                    DateTime current_date = DateTime.Now;
                    int iCurrentMonth = current_date.Month;

                    oReturn1 = ProcessLoanPayOffData(start_date, end_date);


                    if (oReturn1.returnStatus != ReturnStatusEnum.Succeeded || oReturn1.Errors.Count > 0)
                    {
                        sbErr.Length = 0;
                        //send error
                        foreach (ErrorInfo oEr in oReturn1.Errors)
                        {
                            sbErr.AppendLine(oEr.errorDesc);
                        }
                        SendTaskCompleteEmail("LoanPaidOffLetter Status - " + oReturn1.returnStatus.ToString(), sbErr.ToString(), oTaskReturn.taskName);

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
        public ResultReturn ProcessLoanPayOffData(DateTime start_date, DateTime end_date)
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            //string Case_No = "";
            string strFileName = "";
            string docTemplatePath = "";
            string sReponse = "";
            int iError = 0;
            string sError = "";
            string contract_id = "";
            string sub_id = "";
            int iDocType = 0;
            string ln_no = ""; string ln_iss_dt = ""; string ln_amt = ""; string repay_amt = ""; string repay_freq_cd = ""; string ppt_name = "";
            string plan_name = "";
            string comp_name = "";
            string ln_pay_dt = ""; string ssn = "";
            string fname = ""; string lname = "";
            //string address = ""; string city = ""; string state = ""; string zip = ""; 
            string ppt_zip = "";
            //string plan_contact = "";
            int icontractid = 0;
            int TempNumber = 0;
            string sid = "";
            string contact_no = "";
            SOAModel.ContractInfo oContractInfo;
            List<SOAModel.PlanContactInfo> oPlanContacts = new();
            DriverSOA.ContractServ DriverSOACon = new();
            ConsolidatedNotifications _oConsolidatedNotifications = new();
            //Hashtable htMEPContract = new Hashtable();
            try
            {
                sReponse = _oISCDataDC.getLoanPayOffData(start_date, end_date);


                XElement xEl = XElement.Parse(sReponse);

                iError = CheckP3Error(xEl, ref sError);

                if (iError != 0)
                {
                    //failed
                    throw new Exception("Error in ProcessLoanPayOffData: " + sError);
                }
                else
                {
                    foreach (XElement xElrow in xEl.Descendants("LoanPaidOffInfo").ToList())
                    {
                        try
                        {

                            iDocType = 678;
                            sid = ""; contact_no = "";
                            contract_id = ""; sub_id = ""; icontractid = 0; ssn = ""; ppt_zip = ""; fname = ""; lname = "";
                            ln_no = ""; ln_amt = ""; ln_iss_dt = ""; repay_amt = ""; ln_pay_dt = ""; repay_freq_cd = "";
                            ppt_name = ""; plan_name = ""; comp_name = "";
                            //address = ""; city = ""; state = ""; zip = ""; plan_contact = "";

                            //0. read values from xml (XML not finalized yet)
                            if (xElrow.Element("CONT__ID") != null)
                            {
                                contract_id = xElrow.Element("CONT__ID").Value.Trim();
                            }
                            if (xElrow.Element("SUB__ID") != null)
                            {
                                sub_id = xElrow.Element("SUB__ID").Value.Trim();
                                sid = Utils.SubIn(sub_id);
                            }
                            if (xElrow.Element("SOC__SEC__NO") != null)
                            {
                                ssn = xElrow.Element("SOC__SEC__NO").Value.Trim();
                            }
                            if (xElrow.Element("ZIP__CD") != null)
                            {
                                ppt_zip = xElrow.Element("ZIP__CD").Value.Trim();
                            }
                            if (xElrow.Element("FST__MID__NM") != null)
                            {
                                fname = xElrow.Element("FST__MID__NM").Value.Trim();
                            }
                            if (xElrow.Element("LAST__NM") != null)
                            {
                                lname = xElrow.Element("LAST__NM").Value.Trim();
                            }
                            if (xElrow.Element("LOAN__NO") != null)
                            {
                                ln_no = xElrow.Element("LOAN__NO").Value.Trim();
                            }
                            if (xElrow.Element("INIT__LN__AMT") != null)
                            {
                                ln_amt = xElrow.Element("INIT__LN__AMT").Value.Trim();
                            }
                            if (xElrow.Element("ORIG__LN__ISS__DT") != null)
                            {
                                ln_iss_dt = xElrow.Element("ORIG__LN__ISS__DT").Value.Trim();
                                if (Int32.TryParse(ln_iss_dt, out TempNumber)) // which means date is in yyyyMMdd format
                                {
                                    DateTime theDateTime = DateTime.ParseExact(ln_iss_dt, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
                                    ln_iss_dt = theDateTime.ToShortDateString();
                                }
                            }
                            if (xElrow.Element("LN__REPAY__AMT") != null)
                            {
                                repay_amt = xElrow.Element("LN__REPAY__AMT").Value.Trim();
                            }
                            if (xElrow.Element("LN__PAYOFF__D") != null)
                            {
                                ln_pay_dt = xElrow.Element("LN__PAYOFF__D").Value.Trim();
                                if (ln_pay_dt != string.Empty)
                                {
                                    ln_pay_dt = Convert.ToDateTime(ln_pay_dt).ToShortDateString();
                                }
                            }
                            if (xElrow.Element("LN__REPAY__FREQ__CD") != null)
                            {
                                repay_freq_cd = xElrow.Element("LN__REPAY__FREQ__CD").Value.Trim();
                            }

                            if (Int32.TryParse(contract_id, out icontractid))
                            {
                                //oContractInfo = DriverSOACon.GetContractInformation(contract_id,  sub_id);
                                oContractInfo = DriverSOACon.GetContractInformation(contract_id, sid);

                                ppt_name = fname + " " + lname;
                                plan_name = oContractInfo.PlanName;
                                comp_name = oContractInfo.ContractName;

                                if (oContractInfo.FlagValues.isMEP == true)
                                {
                                    contact_no = "TRSConnect at (800) 875-8877";
                                }
                                else
                                {
                                    contact_no = "SponsorConnect at (866) 498-4557";
                                }
                            }
                            else
                            {
                                throw new Exception("Invalid Contract ID: " + contract_id);
                            }

                            //1. create the document for each contract using above values
                            License license = new();
                            license.SetLicense("Aspose.Total.lic");
                            Document Doc;
                            strFileName = GetLoanPaidOffFileName(contract_id, sub_id, ssn, iDocType, ".PDF");
                            Utils.ValidatePath(strFileName);

                            docTemplatePath = TrsAppSettings.AppSettings.GetValue("LoanPayoffDocGenTemplatePath");

                            Doc = new Document(System.IO.Path.Combine(docTemplatePath, "Loan_Payoff_Template.docx"));
                            Doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveEmptyParagraphs;

                            string[] MailMergeFields = new string[10];
                            object[] MailMergeFieldValues = new object[10];


                            MailMergeFields[0] = "Date";
                            MailMergeFields[1] = "Company_Name";

                            MailMergeFields[2] = "Payoff_Date";
                            MailMergeFields[3] = "ContractID";
                            MailMergeFields[4] = "Participant";
                            MailMergeFields[5] = "Loan_Number";
                            MailMergeFields[6] = "Loan_Issue_Date";
                            MailMergeFields[7] = "Payment_Amount";
                            MailMergeFields[8] = "Frequency";
                            MailMergeFields[9] = "ContactNo";

                            MailMergeFieldValues[0] = DateTime.Now.ToShortDateString();
                            MailMergeFieldValues[1] = comp_name;

                            MailMergeFieldValues[2] = ln_pay_dt;
                            MailMergeFieldValues[3] = contract_id + "-" + sub_id;

                            MailMergeFieldValues[4] = ppt_name;
                            MailMergeFieldValues[5] = ln_no;
                            MailMergeFieldValues[6] = ln_iss_dt;
                            if (repay_amt != "")
                            {
                                repay_amt = Convert.ToDecimal(repay_amt).ToString("C");
                            }
                            MailMergeFieldValues[7] = repay_amt;

                            switch (repay_freq_cd)
                            {
                                case "12":
                                    MailMergeFieldValues[8] = "Monthly";
                                    break;
                                case "24":
                                    MailMergeFieldValues[8] = "Semi-Monthly";
                                    break;
                                case "26":
                                    MailMergeFieldValues[8] = "Bi-Weekly";
                                    break;
                                case "4":
                                case "04":
                                    MailMergeFieldValues[8] = "Quarterly";
                                    break;
                                case "52":
                                    MailMergeFieldValues[8] = "Weekly";
                                    break;
                                default:
                                    MailMergeFieldValues[8] = "";
                                    break;
                            }

                            MailMergeFieldValues[9] = contact_no;

                            Doc.MailMerge.Execute(MailMergeFields, MailMergeFieldValues);
                            Doc.Save(strFileName, SaveFormat.Pdf);


                            //2. image to WMS
                            DriverSOA.DocumentService ImageLoanPayoffDoc = new();


                            string oResponse = ImageLoanPayoffDoc.ImageDocument(contract_id, sid, 678, strFileName, 0, "Backend Process - LoanPayoff", ssn);

                            if (oResponse != "")
                            {
                                oReturn.returnStatus = ReturnStatusEnum.Failed;
                                oReturn.Errors.Add(new ErrorInfo(-1, "ImageToWMS Failed. Contract_id: " + contract_id + "sub_id:  " + sub_id + "iDocType: " + iDocType.ToString() + oResponse + "\r\n", ErrorSeverityEnum.Failed));
                            }

                        }

                        catch (Exception exi)
                        {
                            Utils.LogError(exi);
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                            oReturn.isException = true;
                            oReturn.confirmationNo = string.Empty;
                            oReturn.Errors.Add(new ErrorInfo(-1, "Exception in ProcessLoanPayOffData() while  processing: " + xElrow.ToString() + ". Error: " + exi.Message, ErrorSeverityEnum.ExceptionRaised));
                        }
                    }//foreach


                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in LoanPaidOffNotifications  - start_date = " + start_date.ToString() + " end_date = " + end_date.ToString() + " Error: " + ex.Message + "<BR />" + Environment.NewLine, ErrorSeverityEnum.ExceptionRaised));
            }


            return oReturn;
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

        #region **** GetLoanPaidOffFileName ***
        private string GetLoanPaidOffFileName(string cid, string sid, string ssn, int doctype, string ext)
        {

            string sPath;
            DateTime dtToday = DateTime.Now;
            sPath = TrsAppSettings.AppSettings.GetValue("LoanPaidOffDocFolder");
            if (!ext.StartsWith("."))
            {
                ext = "." + ext;
            }
            return System.IO.Path.Combine(sPath, dtToday.Year.ToString(), cid + "_" + sid + "_" + ssn + "_" + dtToday.ToString("MM_dd_yyyy_hhmmss") + "_" + doctype.ToString() + ext);
        }
        #endregion
    }
}
