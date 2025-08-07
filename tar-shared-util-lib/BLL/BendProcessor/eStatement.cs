using System.Collections;
using System.Data;
using System.Text;
using System.Xml.Linq;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.DriverSOA;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using BFL = TRS.IT.SI.BusinessFacadeLayer;
using SOAModel = TRS.IT.SOA.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace TRS.IT.BendProcessor.BLL
{
    public class eStatement : BendProcessorBase
    {
        public eStatement() : base("54", "eStatement", "TRS") { }
        private eStatementDC _oeSDC = new();
        private DataSet _dsPptOptin;
        private string _sContact_GAC = AppSettings.GetValue("ProductFamilyContactGAC");
        private string _sContact_NAV = AppSettings.GetValue("ProductFamilyContactNAV");
        private const int C_eConfirmNotificationType = 1;
        private const int C_ParticipantReqdNoticesNotificationType = 4;
        private const int C_ParticipantReqdNoticesNotificationType_ISC = 5;
        public TaskStatus ProcessNotifyDIA(int a_iNotificationType)
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            const string C_Task = "ProcessNotifyDIA";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    oReturn = GetDIAFeed(a_iNotificationType);
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.fatalErrCnt += 1;
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                    }
                    else
                    {
                        oTaskReturn.rowsCount = oReturn.rowsCount;
                        oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                        if (oReturn.rowsCount > 0)
                        {
                            string sError = "";
                            if (a_iNotificationType != C_ParticipantReqdNoticesNotificationType_ISC)
                            {
                                
                                FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
                                 AppSettings.GetVaultValue("FTPUserName"),
                                 AppSettings.GetVaultValue("FTPPassword"));
                                bool b = false;
                                b = oFtp.UploadFile(oReturn.confirmationNo, AppSettings.GetValue("FTPDIAFeedFolder") + Path.GetFileName(oReturn.confirmationNo), ref sError);
                                if (!b)
                                {
                                    oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                    oTaskReturn.errors.Add(new ErrorInfo(-1, "Failed to upload feed file to DIA - " + sError, ErrorSeverityEnum.Error));
                                }
                            }
                            else
                            {
                                try
                                {
                                    string sFileName = AppSettings.GetValue("ReqdNoticesIMFolder") + "StatementFeed_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                                    TRSManagers.FileManager.CopyFileToRemote(oReturn.confirmationNo, sFileName, false);
                                }
                                catch (Exception ex)
                                {
                                    Utils.LogError(ex);
                                    oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                    oTaskReturn.errors.Add(new ErrorInfo(-1, "Failed to upload feed file to IM drop location - " + ex.Message, ErrorSeverityEnum.Error));
                                }

                            }
                        }
                        else
                        {
                            oTaskReturn.errors.Add(new ErrorInfo(1, "Did not upload file to DIA because file is empty.", ErrorSeverityEnum.Warning));
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
        public TaskStatus ProcessClearDailyDiaFeed(int a_iNotificationType)
        {
            TaskStatus oTaskReturn = new();

            const string C_Task = "ProcessClearDailyDiaFeed";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);

                    oTaskReturn.rowsCount = _oeSDC.ClearDailyDiaFeed(a_iNotificationType);

                    oTaskReturn.retStatus = TaskRetStatus.Succeeded;
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
        public string RemoveLeadingZeros(string a_sConId)
        {
            int iNum;
            if (int.TryParse(a_sConId, out iNum))
            {
                return iNum.ToString();
            }
            else
            {
                return a_sConId;
            }
        }
        public DateTime GetDate(string sDate, string a_sFormat)//MMDDYYYY format
        {
            DateTime dt = new(1900, 01, 01);
            switch (a_sFormat.ToUpper())
            {
                case "MMDDYYYY":
                    dt = new DateTime(Convert.ToInt32(sDate.Substring(4, 4)), Convert.ToInt32(sDate.Substring(0, 2)), Convert.ToInt32(sDate.Substring(2, 2)));
                    break;
                case "YYYYMMDD":
                    dt = new DateTime(Convert.ToInt32(sDate.Substring(0, 4)), Convert.ToInt32(sDate.Substring(4, 2)), Convert.ToInt32(sDate.Substring(6, 2)));
                    break;
                case "YYYY-MM-DD":
                    dt = Convert.ToDateTime(sDate);
                    break;
            }

            return dt;
        }
        public FtpFolderTracking GetFtpFolderInfo(FTPUtility a_oFtp, string a_sRootFolder)
        {
            List<string> directories = new();
            string sError = "";
            FtpFolderTracking oFtpFolder = new();
            oFtpFolder.rootFolder = a_sRootFolder;
            oFtpFolder.yearFolder = DateTime.Now.Year.ToString();
            oFtpFolder.maxLimit = Convert.ToInt32(AppSettings.GetValue("MaxFilesPerFolder"));

            if (IsDirectoryExist(a_oFtp, oFtpFolder.rootFolder + oFtpFolder.yearFolder, true, ref sError))
            {
                directories = a_oFtp.ListDirectory(oFtpFolder.rootFolder + oFtpFolder.yearFolder);
                if (directories.Count > 0)
                {
                    oFtpFolder.subFolderCount = directories.Count - 1;
                }
                else
                {
                    oFtpFolder.subFolderCount = 0;
                }

                if (CreateFtpSubFolder(a_oFtp, oFtpFolder, ref sError))
                {
                    if (!oFtpFolder.IsUnderLimit)
                    {
                        oFtpFolder.subFolderCount++;
                        if (CreateFtpSubFolder(a_oFtp, oFtpFolder, ref sError))
                        {
                            oFtpFolder.returnStatus = ReturnStatusEnum.Succeeded;
                        }
                        else
                        {
                            oFtpFolder.returnStatus = ReturnStatusEnum.Failed;
                            oFtpFolder.errors.Add(new ErrorInfo(-1, "Folder: " + oFtpFolder.yearFolder + " Error: " + sError, ErrorSeverityEnum.Failed));
                        }

                    }
                    else
                    {
                        oFtpFolder.returnStatus = ReturnStatusEnum.Succeeded;
                    }
                }
                else
                {
                    oFtpFolder.returnStatus = ReturnStatusEnum.Failed;
                    oFtpFolder.errors.Add(new ErrorInfo(-1, "Folder: " + oFtpFolder.yearFolder + " Error: " + sError, ErrorSeverityEnum.Failed));

                }
            }
            else
            {
                oFtpFolder.returnStatus = ReturnStatusEnum.Failed;
                oFtpFolder.errors.Add(new ErrorInfo(-1, "Folder: " + oFtpFolder.yearFolder + " Error: " + sError, ErrorSeverityEnum.Failed));
            }
            return oFtpFolder;
        }
        private bool CreateFtpSubFolder(FTPUtility a_oFtp, FtpFolderTracking a_oFtpFolder, ref string sError)
        {
            bool bOk = false;
            //create folder
            a_oFtpFolder.currentFolder = a_oFtpFolder.subFolderCount.ToString("00000");
            if (IsDirectoryExist(a_oFtp, a_oFtpFolder.GetCurrentFolder, true, ref sError))
            {
                FTPdirectory oDir = a_oFtp.ListDirectoryDetail(a_oFtpFolder.GetCurrentFolder);
                a_oFtpFolder.initialCount = oDir.Count;
                a_oFtpFolder.runningCount = a_oFtpFolder.initialCount;
                bOk = true;
            }
            else
            {
                throw new Exception("Could not create directory: " + a_oFtpFolder.currentFolder + "0/  Error: " + sError);
            }

            return bOk;
        }
        public bool IsDirectoryExist(FTPUtility oFtp, string a_sDirectory, bool a_bCreate, ref string a_sError)
        {
            bool bReturn = false;
            bool bIsGood;
            List<string> result = new();
            try
            {
                try
                {
                    result = oFtp.ListDirectory(a_sDirectory);
                    bReturn = true;
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    bReturn = false;
                }

                if (!bReturn && a_bCreate)
                {
                    bIsGood = oFtp.FtpCreateDirectory(a_sDirectory, ref a_sError);
                    bReturn = bIsGood;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                bReturn = false;
            }
            return bReturn;
        }
        public ResultReturn PptStatementAvailable(PartStFeedInfo a_oPptStFeedInfo, DocIndexFileInfo a_oIndexFileInfo)
        {
            ResultReturn oReturn = new();
            try
            {
                if (a_oPptStFeedInfo.found)
                {
                    a_oIndexFileInfo.connectParms = "<ConnectParm><ParmId ParmName=\"DiaFtpeStatement\">101</ParmId></ConnectParm>";
                    ResultReturn oRDiaFeed = _oeSDC.InsertDiaFeed(a_oPptStFeedInfo, a_oIndexFileInfo);
                    if (oRDiaFeed.returnStatus == ReturnStatusEnum.Succeeded)
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                    }
                    else
                    {
                        //may need to log error here
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(-1, General.ParseErrorText(oRDiaFeed.Errors, ";"), ErrorSeverityEnum.Failed));
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }
        public PartStFeedInfo GetPptFeedInfo(string a_sSSN, string a_sConId, string a_sSubId, NotificationTypeEnum a_eNotificationType)
        {
            PartStFeedInfo oPptFeedInfo = new();
            DataSet dseInfo = _oeSDC.GeteStatementFeedInfo(a_sSSN, a_sConId, a_sSubId);
            if (dseInfo.Tables[0].Rows.Count > 0)
            {
                oPptFeedInfo.notificationType = a_eNotificationType.GetHashCode();
                AssignFeedInfo(dseInfo.Tables[0].Rows[0], oPptFeedInfo);
            }
            return oPptFeedInfo;
        }
        private void AssignFeedInfo(DataRow a_dr, PartStFeedInfo a_oPartFeedInfo)
        {
            a_oPartFeedInfo.found = true;
            a_oPartFeedInfo.inLoginId = Utils.CheckDBNullInt(a_dr["in_login_id"]);
            a_oPartFeedInfo.contractId = Utils.CheckDBNullStr(a_dr["contract_id"]);
            a_oPartFeedInfo.subId = Utils.CheckDBNullStr(a_dr["sub_id"]);
            a_oPartFeedInfo.email = Utils.CheckDBNullStr(a_dr["email"]);
            a_oPartFeedInfo.firstName = Utils.CheckDBNullStr(a_dr["first_name"]);
            a_oPartFeedInfo.lastName = Utils.CheckDBNullStr(a_dr["last_name"]);
            a_oPartFeedInfo.middleName = Utils.CheckDBNullStr(a_dr["middle_name"]);
            a_oPartFeedInfo.companyUrl = "http://www.ta-retirement.com/";
            a_oPartFeedInfo.companyPhone = Utils.CheckDBNullStr(a_dr["product_family"]) == "NAV" ? _sContact_NAV : _sContact_GAC;
        }
        private ResultReturn GetDIAFeed(int a_iNotificationType)
        {
            const char C_Delimiter = ',';
            const string C_MissingEmail = "MissingEmail@transamerica.com";
            StringBuilder strB = new();
            ResultReturn oReturn = new();
            oReturn.confirmationNo = AppSettings.GetValue("eStatementDIAFeedFolder") + "StatementFeed" + DateTime.Now.ToString("MMddyyyyhhmmss") + ".txt";

            StreamWriter sw = null;
            string sEmail;
            try
            {
                sw = new StreamWriter(oReturn.confirmationNo);
                DataSet dsFeed = _oeSDC.GetDIAFeed(a_iNotificationType);
                if (a_iNotificationType != C_ParticipantReqdNoticesNotificationType_ISC)
                {
                    foreach (DataRow dr in dsFeed.Tables[0].Rows)
                    {
                        strB.Append(Utils.CheckDBNullStr(dr["notification_type"]));
                        strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["in_login_id"]));
                        strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["contract_id"]));
                        strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["sub_id"]));
                        sEmail = Utils.CheckDBNullStr(dr["email"]);
                        if (string.IsNullOrEmpty(sEmail))
                        {
                            sEmail = C_MissingEmail;
                        }

                        strB.Append(C_Delimiter + sEmail);
                        strB.Append(C_Delimiter + "\"" + Utils.CheckDBNullStr(dr["first_name"]) + "\"");
                        strB.Append(C_Delimiter + "");
                        strB.Append(C_Delimiter + "\"" + Utils.CheckDBNullStr(dr["last_name"]) + "\"");
                        strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["company_url"]));
                        strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["company_phone"]));
                        strB.Append(C_Delimiter + "\"" + Utils.CheckDBNullStr(dr["plan_name"]) + "\"");

                        if (a_iNotificationType == C_ParticipantReqdNoticesNotificationType)
                        {
                            if (Utils.CheckDBNullInt(dr["sub_notication_type"]) != 0)
                            {
                                strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["sub_notication_type"]));
                            }

                            if (Utils.CheckDBNullStr(dr["feed"]) != string.Empty)
                            {
                                strB.Append(C_Delimiter + Utils.CheckDBNullStr(dr["feed"]));
                            }

                        }

                        sw.WriteLine(strB.ToString());
                        strB.Remove(0, strB.Length);
                    }
                    oReturn.rowsCount = dsFeed.Tables[0].Rows.Count;
                }
                else
                {
                    foreach (DataRow dr in dsFeed.Tables[0].Rows)
                    {
                        strB.Append(Utils.CheckDBNullStr(dr["feed"]));
                        sw.WriteLine(strB.ToString());
                        strB.Remove(0, strB.Length);
                    }
                    oReturn.rowsCount += dsFeed.Tables[0].Rows.Count;
                }

                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));

            }
            finally
            {
                sw.Close();
            }
            return oReturn;
        }
        public DocFileInfo GetDocFileInfo(string a_sPartnerId, NotificationTypeEnum eNotficationType, FTPfileInfo a_ff)
        {
            DocFileInfo oDocFileInfo = new();
            string[] s_arr;

            string sFileName = a_ff.NameOnly;  //Path.GetFileName(a_ff.Filename);
            try
            {
                switch (eNotficationType)
                {
                    case NotificationTypeEnum.eConfirm:
                        int iEnd, iIndex, iIndex1, iIndex2;
                        string sTagName1;
                        string sVal;
                        const int C_iOffset = 3;
                        //All partners have the same file format
                        //CN$300069SC$000SN$111222333DT$101DR$2010-01-15TN$RB101120.pdf
                        iEnd = sFileName.ToUpper().IndexOf("END");
                        if (iEnd < 0)
                        {
                            iEnd = sFileName.IndexOf(".");
                        }

                        sFileName = sFileName.Substring(0, iEnd);
                        iIndex = 1;
                        while (iIndex > 0)
                        {
                            iIndex1 = sFileName.IndexOf("$", iIndex);
                            iIndex = iIndex1 + 1;
                            if (iIndex1 > 1)
                            {
                                sTagName1 = sFileName.Substring(iIndex1 - 2, 3);
                                iIndex2 = sFileName.IndexOf("$", iIndex1 + 1);
                                if (iIndex2 < 0)
                                {
                                    iIndex2 = iEnd + 2;
                                }

                                if (iIndex2 > iIndex1)
                                {
                                    sVal = sFileName.Substring(iIndex1 + 1, iIndex2 - (iIndex1 + C_iOffset));
                                    switch (sTagName1.ToUpper())
                                    {
                                        case "CN$":
                                            oDocFileInfo.contractId = sVal;
                                            break;
                                        case "SC$":
                                            oDocFileInfo.subId = sVal;
                                            break;
                                        case "SN$":
                                            oDocFileInfo.ssn = sVal;
                                            break;
                                        case "DT$":
                                            oDocFileInfo.docType = Convert.ToInt32(sVal);
                                            break;
                                        case "DR$":
                                            oDocFileInfo.trxDate = sVal;
                                            break;
                                        case "TN$":
                                            oDocFileInfo.transId = sVal;
                                            break;

                                    }
                                }
                            }
                        }
                        //Default to "000" if there is no SC$ tag
                        if (string.IsNullOrEmpty(oDocFileInfo.subId))
                        {
                            oDocFileInfo.subId = "000";
                        }

                        if (string.IsNullOrEmpty(oDocFileInfo.contractId) || string.IsNullOrEmpty(oDocFileInfo.ssn) || oDocFileInfo.docType == 0)
                        {
                            oDocFileInfo.parseError = "Error parsing file";
                        }

                        break;
                    case NotificationTypeEnum.eStatement:
                        switch (a_sPartnerId)
                        {
                            case ConstN.C_PARTNER_PENCO:
                                // 300069_000_111222333_08312010_101.pdf
                                s_arr = a_ff.NameOnly.Split(['_']);
                                if (s_arr.Length == 5)
                                {
                                    oDocFileInfo.contractId = RemoveLeadingZeros(s_arr[0].Trim());
                                    oDocFileInfo.subId = s_arr[1].Trim();
                                    oDocFileInfo.ssn = s_arr[2];
                                    oDocFileInfo.trxDate = GetDate(s_arr[3], "MMDDYYYY").ToString("yyyy-MM-dd");
                                    oDocFileInfo.docType = Convert.ToInt32(s_arr[4]);
                                }
                                break;
                            case ConstN.C_PARTNER_TAE:
                                //PPPP_CCCCC_SSS_MMMMMMMMM_mmddccyy_101.PDF
                                s_arr = a_ff.NameOnly.Split(['_']);

                                if (s_arr.Length == 6)
                                {
                                    oDocFileInfo.contractId = RemoveLeadingZeros(s_arr[1].Trim());
                                    oDocFileInfo.subId = s_arr[2].Trim();
                                    oDocFileInfo.ssn = s_arr[3];
                                    oDocFileInfo.trxDate = GetDate(s_arr[4], "MMDDYYYY").ToString("yyyy-MM-dd");
                                    oDocFileInfo.docType = Convert.ToInt32(s_arr[5]);
                                }

                                break;
                        }

                        break;
                    case NotificationTypeEnum.PxNotification:
                        //Only one partner
                        // 300069_000_111222333_YYYY-MM-DD_692.pdf
                        s_arr = a_ff.NameOnly.Split(['_']);
                        if (s_arr.Length == 5)
                        {
                            oDocFileInfo.contractId = RemoveLeadingZeros(s_arr[0].Trim());
                            oDocFileInfo.subId = s_arr[1].Trim();
                            oDocFileInfo.ssn = s_arr[2];
                            oDocFileInfo.trxDate = GetDate(s_arr[3], "YYYY-MM-DD").ToString("yyyy-MM-dd");
                            oDocFileInfo.docType = Convert.ToInt32(s_arr[4]);
                        }
                        break;
                    case NotificationTypeEnum.RequiredNotifications:
                        switch (a_sPartnerId)
                        {
                            case ConstN.C_PARTNER_PENCO:
                                // 932003_000_08312010_695.pdf
                                s_arr = a_ff.NameOnly.Split(['_']);
                                if (s_arr.Length == 4)
                                {
                                    oDocFileInfo.contractId = RemoveLeadingZeros(s_arr[0].Trim());
                                    oDocFileInfo.subId = s_arr[1].Trim();
                                    oDocFileInfo.trxDate = GetDate(s_arr[2], "MMDDYYYY").ToString("yyyy-MM-dd");
                                    oDocFileInfo.docType = Convert.ToInt32(s_arr[3]);
                                }
                                break;
                            case ConstN.C_PARTNER_TAE:
                                //PPPP_CCCCC_SSS_mmddccyy_101.PDF
                                s_arr = a_ff.NameOnly.Split(['_']);

                                if (s_arr.Length == 5)
                                {
                                    oDocFileInfo.contractId = RemoveLeadingZeros(s_arr[1].Trim());
                                    oDocFileInfo.subId = s_arr[2].Trim();
                                    oDocFileInfo.trxDate = GetDate(s_arr[3], "MMDDYYYY").ToString("yyyy-MM-dd");
                                    oDocFileInfo.docType = Convert.ToInt32(s_arr[4]);

                                }

                                break;
                        }
                        break;

                }

                if (string.IsNullOrEmpty(oDocFileInfo.contractId) || (string.IsNullOrEmpty(oDocFileInfo.ssn) && eNotficationType != NotificationTypeEnum.RequiredNotifications) || oDocFileInfo.docType == 0)
                {
                    oDocFileInfo.parseError = "Error parsing file: " + sFileName;
                }

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oDocFileInfo.parseError = "File name: " + sFileName + "  parse error: " + ex.Message;
            }

            return oDocFileInfo;
        }
        public TaskStatus ProcessInvalidPptAddressReportMigrated()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            const string C_Task = "ProcessInvalidPptAddressReport";
            StringBuilder strB = new();
            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    oReturn = CreateInvalidPptAddressReportDataMigrated();
                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded)
                    {
                        General.CopyResultError(oTaskReturn, oReturn);
                        oTaskReturn.fatalErrCnt += 1;
                        oTaskReturn.retStatus = TaskRetStatus.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            SendTaskCompleteEmail("ProcessInvalidPptAddressReport - " + oTaskReturn.retStatus.ToString(), General.ParseTaskInfo(oTaskReturn), "ProcessInvalidPptAddressReport Backend Processing");
            return oTaskReturn;
        }
        private ResultReturn CreateInvalidPptAddressReportDataMigrated()
        {
            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            try
            {
                string contract_id = "";
                string sub_id = "";
                string ssn_no = "";
                string first_name = "";
                string last_name = "";
                string MessageDesc = "";

                string sReponse = "";
                string sError = "";
                string sInfo = "";
                int iError = 0;
                Hashtable htDistContracts = new();
                DateTime dtStartDate = new(DateTime.Now.Year, DateTime.Now.Month, 1);
                dtStartDate = dtStartDate.AddMonths(-1); // begining of previous month
                DateTime dtEndDate = dtStartDate.AddMonths(1).AddDays(-1); // end of previous month

                sReponse = _oeSDC.GetInvalidPptAddressReportData(dtStartDate, dtEndDate);

                XElement xEl = XElement.Parse(sReponse);
                iError = CheckP3ErrorMigrated(xEl, ref sError);

                if (iError != 0)
                {
                    //failed
                    throw new Exception("Error in GetMissingAddressData: " + sError);
                }
                else
                {
                    IEnumerable<XElement> xElRows = from row in xEl.Descendants("MAROW") select row;


                    if ((xElRows != null))
                    {
                        foreach (XElement xElrow in xElRows)
                        {
                            try
                            {
                                contract_id = ""; sub_id = ""; ssn_no = ""; first_name = ""; last_name = ""; MessageDesc = "";

                                if (xElrow.Element("CONTRACTID") != null)
                                {
                                    contract_id = xElrow.Element("CONTRACTID").Value.Trim();
                                }

                                if (xElrow.Element("SUBID") != null)
                                {
                                    sub_id = xElrow.Element("SUBID").Value.Trim();
                                    sub_id = Utils.SubIn(sub_id);
                                }

                                if (xElrow.Element("SSN") != null)
                                {
                                    ssn_no = xElrow.Element("SSN").Value.Trim();
                                }

                                if (xElrow.Element("FNAME") != null)
                                {
                                    first_name = xElrow.Element("FNAME").Value.Trim();
                                }

                                if (xElrow.Element("LNAME") != null)
                                {
                                    last_name = xElrow.Element("LNAME").Value.Trim();
                                }

                                if (xElrow.Element("MSG") != null)
                                {
                                    MessageDesc = xElrow.Element("MSG").Value.Trim();
                                }

                                sInfo = "Contract: " + contract_id + "-" + sub_id + " ssn_no: " + ssn_no;

                                _oeSDC.InsertInvalidPptAddressReportData(contract_id, sub_id, ssn_no, dtStartDate, dtEndDate, first_name, last_name, MessageDesc);

                                if (htDistContracts.ContainsKey(contract_id + "_" + sub_id) == false)
                                {
                                    htDistContracts.Add(contract_id + "_" + sub_id, "");
                                }

                            }
                            catch (Exception exi)
                            {
                                Utils.LogError(exi);
                                oReturn.returnStatus = ReturnStatusEnum.Failed;
                                oReturn.Errors.Add(new ErrorInfo(-1, "Error in CreateInvalidPptAddressReportData:   " + sInfo + "Error: " + exi.Message + "\r\n", ErrorSeverityEnum.Failed));

                            }

                        }

                        //TBD: Send Email
                        string[] s_arr;
                        foreach (DictionaryEntry ct in htDistContracts)
                        {
                            contract_id = ""; sub_id = "";
                            s_arr = ct.Key.ToString().Split('_');
                            if (s_arr.Length > 0)
                            {
                                contract_id = s_arr[0];
                                sub_id = s_arr[1];
                                sInfo = "Contract: " + contract_id + "-" + sub_id;

                                ResultReturn oRes = SendNotificationMigrated(2890, contract_id, sub_id);
                                if (oRes.returnStatus != ReturnStatusEnum.Succeeded)
                                {
                                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                                    oReturn.Errors.Add(new ErrorInfo(-1, "Failure while sending Notification in CreateInvalidPptAddressReportData:   " + sInfo + "Error: " + oRes.Errors[0].errorDesc + "\r\n", ErrorSeverityEnum.Failed));
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, "Exception in CreateInvalidPptAddressReportData:   " + ex.Message + "\r\n", ErrorSeverityEnum.Failed));
            }
            return oReturn;
        }
        private int CheckP3ErrorMigrated(XElement xEl, ref string sError)
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
        private ResultReturn SendNotificationMigrated(int iMsg_Id, string sContract_id, string sSub_Id)
        {
            ResultReturn oResults = new();

            MessageServiceKeyValue[] Keys = null;

            if (iMsg_Id == 0)
            {
                oResults = new ResultReturn();
                oResults.returnStatus = ReturnStatusEnum.Succeeded;
            }
            else
            {
                ContractServ DriverSOACon = new();

                SOAModel.ContractInfo oConInfo;
                oConInfo = DriverSOACon.GetContractInformation(sContract_id, sSub_Id);

                MessageServiceKeyValue nKey = new();
                nKey.key = "mep_sep_phone_number";
                if (oConInfo.FlagValues.isMEP == true)
                {
                    nKey.value = "800-875-8877";
                }
                else
                {
                    nKey.value = "866-498-4557";
                }

                Keys = new MessageServiceKeyValue[1];
                Keys[0] = nKey;

                MessageService oMS = new();
                oResults = oMS.SendMessage(sContract_id, sSub_Id, iMsg_Id, Keys, "TRS-Auto-Message-Service");

                if (oResults == null)
                {
                    ErrorInfo oError = new();
                    oResults = new ResultReturn();
                }
            }
            return oResults;
        }
    }

}
