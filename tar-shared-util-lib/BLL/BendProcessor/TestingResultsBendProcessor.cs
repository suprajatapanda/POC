using System.Collections;
using System.Text.RegularExpressions;
using System.Xml;
using TRS.IT.BendProcessor.DriverSOA;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using BFL = TRS.IT.SI.BusinessFacadeLayer;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace TRS.IT.BendProcessor.BLL
{
    public class TestingResults : BendProcessorBase
    {
        public TestingResults() : base("2", "TestingResults", "TRS") { }

        List<string> FinalFiles = new();
        List<string> FinalFiles_Failed = new();

        public TaskStatus ProcessFinalResultsMigrated()
        {
            FinalFiles = new List<string>();
            FinalFiles_Failed = new List<string>();

            TaskStatus oTaskReturn = new();
            const string C_Task = "ProcessFinalResults";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    MoveFinalResultsFilesMigrated();
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                SendErrorEmail(ex);
            }

            SendStatusEmail("ProcessFinalResults Status");
            oTaskReturn.endTime = DateTime.Now;

            return oTaskReturn;
        }

        public TaskStatus ProcessTestingResultsMigrated()
        {
            FinalFiles = new List<string>();
            FinalFiles_Failed = new List<string>();

            TaskStatus oTaskReturn = new();
            const string C_Task = "ProcessTestingResults";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    InitTaskStatus(oTaskReturn, C_Task);
                    GetTestingResultsMigrated();
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                SendErrorEmail(ex);
            }

            SendStatusEmail("ProcessTestingResults Status");
            oTaskReturn.endTime = DateTime.Now;

            return oTaskReturn;
        }

        private void MoveFinalResultsFilesMigrated()
        {
            string WMSFolderPath = AppSettings.GetValue("WMSFinalResultFolder");
            string FinalResultsDocTypeID = AppSettings.GetValue("Final_Results_DOC_TYPE_ID");
            string FinalLetterDocTypeID = AppSettings.GetValue("Final_Letter_DOC_TYPE_ID");
            string fileFinalReslutsAttribute = "*_FinalResults.pdf";
            string fileFinalLeterAttribute = "*_FinalLetters.pdf";

            Hashtable htResultDocType = GetResultLetterHS(FinalResultsDocTypeID);
            Hashtable htLetterDocType = GetResultLetterHS(FinalLetterDocTypeID);

            int year = Convert.ToInt32(DateTime.Today.ToString("yy"));
            string resultDocTypeID;
            string letterDocTypeID;

            for (int y = year - 2; y <= year; y++)
            {
                string curYear = y.ToString();
                if (htResultDocType.Contains(curYear))
                {
                    resultDocTypeID = htResultDocType[curYear].ToString();
                }
                else
                {
                    return;
                }

                if (htLetterDocType.Contains(curYear))
                {
                    letterDocTypeID = htLetterDocType[curYear].ToString();
                }
                else
                {
                    return;
                }

                string folderPath = String.Format("{0}YEFORMS.{1}\\ASC Testing Results\\", AppSettings.GetValue("Path_Forms"), curYear);
                if (Directory.Exists(folderPath))
                {
                    string[] directories = Directory.GetDirectories(folderPath);
                    foreach (string subDir in directories)
                    {
                        DirectoryInfo di = new(subDir);
                        FileInfo[] Resultfiles = di.GetFiles(fileFinalReslutsAttribute, SearchOption.TopDirectoryOnly);

                        foreach (FileInfo file in Resultfiles)
                        {
                            try
                            {
                                MoveFileToWMSMigrated(file, WMSFolderPath, resultDocTypeID);
                                RenameFileDone(file);
                                FinalFiles.Add(file.FullName);
                            }
                            catch (Exception ex)
                            {
                                Utils.LogError(ex);
                                RenameFileFailed(file);
                                FinalFiles_Failed.Add(file.FullName);
                                SendErrorEmail(ex);
                            }
                        }

                        FileInfo[] Lettersfiles = di.GetFiles(fileFinalLeterAttribute, SearchOption.TopDirectoryOnly);
                        foreach (FileInfo file in Lettersfiles)
                        {
                            try
                            {
                                //RenameFileInProcess(file);
                                MoveFileToWMSMigrated(file, WMSFolderPath, letterDocTypeID);
                                RenameFileDone(file);
                                FinalFiles.Add(file.FullName);
                            }
                            catch (Exception ex)
                            {
                                Utils.LogError(ex);
                                RenameFileFailed(file);
                                FinalFiles_Failed.Add(file.FullName);
                                SendErrorEmail(ex);
                            }
                        }
                    }
                }
            }
        }

        private void MoveFileToWMSMigrated(FileInfo file, string WMSFolderPath, string DocTypeID)
        {
            string[] fileParts = file.Name.Split('_');
            string ContractID = fileParts[0].Substring(0, 6).TrimStart('0');
            string SubID = GetSubID(fileParts[0].Substring(6));
            string wmsFileNameName = WMSFolderPath + string.Format("CN${0}SC${1}DT${2}.pdf", ContractID, SubID, DocTypeID);
            File.Copy(file.FullName, wmsFileNameName, true);
        }

        private void GetTestingResultsMigrated()
        {
            string fileAttribute = "*_testingresults_*.csv";
            int year = Convert.ToInt32(DateTime.Today.ToString("yy"));

            for (int y = year - 2; y <= year; y++)
            {
                string folderPath = String.Format("{0}YEFORMS.{1}\\ASC Testing Results\\", AppSettings.GetValue("Path_Forms"), y.ToString());
                //Scan Files
                if (Directory.Exists(folderPath))
                {
                    string[] directories = Directory.GetDirectories(folderPath);
                    foreach (string subDir in directories)
                    {
                        DirectoryInfo di = new(subDir);
                        FileInfo[] files = di.GetFiles(fileAttribute, SearchOption.TopDirectoryOnly);
                        foreach (FileInfo file in files)
                        {
                            if (!file.Name.ToLower().Contains("submitted") && !file.Name.ToLower().Contains("submission"))
                            {
                                try
                                {
                                    BFLModel.TestingResults results = ExtractData(file);
                                    FinalFiles.Add(file.FullName);
                                    if (results != null && SubmitResultsMigrated(results))
                                    {
                                        RenameFileDone(file);
                                    }
                                    else
                                    {
                                        RenameFileFailed(file);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Utils.LogError(ex);
                                    RenameFileFailed(file);
                                    FinalFiles_Failed.Add(file.FullName);
                                    SendErrorEmail(ex);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool SubmitResultsMigrated(BFLModel.TestingResults results)
        {
            string response;
            BFL.Contract oCon = new(results.ContractID, results.SubID);
            string xml = TRSManagers.XMLManager.GetXML(results);
            response = oCon.SubmitTestingResults(xml);
            if (!string.IsNullOrEmpty(response))
            {
                if (response.Contains("<ConfirmationNumber>"))
                {
                    return true;
                }
            }
            return false;
        }

        private BFLModel.TestingResults ExtractData(FileInfo file)
        {
            BFLModel.TestingResults results = new();
            try
            {
                string[] lines = File.ReadAllLines(file.FullName);
                string[] fileParts = file.Name.Split('_');
                results.ContractID = fileParts[0].Substring(0, 6).TrimStart('0');
                results.SubID = GetSubID(fileParts[0].Substring(6));
                results.PartnerID = "ISC";
                if (lines.Length == 2)
                {
                    Regex CSVParser = new(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

                    string[] cols = CSVParser.Split(lines[1]);
                    results.TestingPYE = cols[1];
                    results.TestYear = cols[2];
                    results.SafeHarbor = cols[3].ToLower() == "true" ? true : false;
                    results.ProcessDate = cols[5];
                    results.ADPStatus = cols[6].ToLower() == "true" ? true : false;
                    results.ACPStatus = cols[7].ToLower() == "true" ? true : false;
                    results.ADPACPTestingMethod = (BFLModel.E_ADPACPtestingmethodType)Convert.ToInt16(cols[8]);
                    results.ADPRefundAmt = Convert.ToDouble(Regex.Replace(cols[9], @"[^\d.]", ""));
                    results.ACPRefundAmt = Convert.ToDouble(Regex.Replace(cols[10], @"[^\d.]", ""));
                    results.ADPHCEPercentage = Convert.ToDouble(Regex.Replace(cols[11], @"[^\d.]", ""));
                    results.ADPNHCEPercentage = Convert.ToDouble(Regex.Replace(cols[12], @"[^\d.]", ""));
                    results.ACPHCEPercentage = Convert.ToDouble(Regex.Replace(cols[13], @"[^\d.]", ""));
                    results.ACPNHCEPercentage = Convert.ToDouble(Regex.Replace(cols[14], @"[^\d.]", ""));
                }

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                results = null;
            }
            return results;
        }

        private Hashtable GetResultLetterHS(string docTypeIDs)
        {
            Hashtable ht = new();
            string[] resultTypes = docTypeIDs.Split(';');

            if (resultTypes.Length > 0)
            {
                foreach (string t in resultTypes)
                {
                    string[] Id = t.Split(':');
                    if (Id.Length > 0)
                    {
                        ht.Add(Id[0], Id[1]);
                    }
                }
            }
            return ht;
        }
        private string GetSubID(string subID4)
        {
            if (subID4 == "0000")
            {
                return "000";
            }
            else
            {
                return SIPBO.TRSCommon.SubIn(subID4);
            }
        }

        private void RenameFileDone(FileInfo file)
        {
            string newFileName = file.FullName.Replace(file.Extension, string.Format(" Submitted {0}{1}", DateTime.Now.ToString("yyyy-MM-ddtHHmmss"), file.Extension));
            File.Copy(file.FullName, newFileName);
            file.Delete();
        }
        private void RenameFileFailed(FileInfo file)
        {
            string newFileName = file.FullName.Replace(file.Extension, string.Format(" Submission Failed{0}", file.Extension));
            bool overwrite = false;
            if (File.Exists(newFileName))
            {
                newFileName = newFileName.Replace(" Submission Failed", " Submission Failed1");
                overwrite = true;
            }
            File.Copy(file.FullName, newFileName, overwrite);
            file.Delete();
        }

        private void SendStatusEmail(string subject)
        {
            System.Text.StringBuilder sb = new();
            sb.Append("Files have been successfully processed <br />");
            sb.Append(String.Join("<br />", FinalFiles.ToArray()));
            sb.Append("<br /><br /><br />Files were failed to process <br /> ");
            sb.Append(String.Join("<br />", FinalFiles_Failed.ToArray()));
            Utils.SendMail(AppSettings.GetValue(ConstN.C_BPROCESSOR_EMAIL), AppSettings.GetValue("FinalEmail"), subject, sb.ToString());
        }

    }
}
