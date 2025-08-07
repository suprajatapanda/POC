using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using PlanComplianceResultBatch.SOA;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace PlanComplianceResultBatch.BLL
{
    public class TestingResults(TRS.IT.BendProcessor.BLL.FWBend obj) //: BendProcessorBase
    {
        TRS.IT.BendProcessor.BLL.FWBend fWBend = obj;

        public TaskStatus ProcessTestingResultsTAE()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;
            string[] Files;
            const string C_Task = "ProcessTestingResultsTAE";

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    fWBend.InitTaskStatus(oTaskReturn, C_Task);
                    oReturn = MoveXmlToStagingTAE(AppSettings.GetValue("TestingResultFolder"));
                    General.CopyResultError(oTaskReturn, oReturn);
                    //Files = Directory.GetFiles(AppSettings.GetValue("TestingResultStaging"));
                    Files = Directory.GetFiles("C:\\1\\");
                    if (Files.Length > 0)
                    {
                        foreach (string sFileName in Files)
                        {
                            oReturn = LoadXmlTAE(sFileName);
                            oTaskReturn.rowsCount += oReturn.rowsCount;
                            General.CopyResultError(oTaskReturn, oReturn);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                fWBend.SendErrorEmail(ex);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }

        private ResultReturn MoveXmlToStagingTAE(string a_sPath)
        {
            ResultReturn oReturn = new();
            string[] Files;
            string sXmlStagingFolderTae = AppSettings.GetValue("TestingResultStaging");
            string sNewFile;

            try
            {
                oReturn.returnStatus = ReturnStatusEnum.Succeeded;

                Files = Directory.GetFiles(a_sPath);
                if (Files.Length > 0)
                {
                    foreach (string sFileName in Files)
                    {
                        sNewFile = sXmlStagingFolderTae + Path.GetFileName(sFileName);
                        File.Copy(sFileName, sNewFile, true);
                        if (File.Exists(sNewFile))
                        {
                            File.Delete(sFileName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.confirmationNo = string.Empty;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            return oReturn;

        }

        private ResultReturn LoadXmlTAE(string a_sFileName)
        {
            ResultReturn oReturn = new();
            ContractServ DriverSOACon = new();
            XmlReader reader = null;
            string sXmlData;
            bool bExit = false;
            ResultReturn oResultContract;
            string sXmlArchiveFolderTae = AppSettings.GetValue("TestingResultArchiveFolder");
            string sNewFile;
            try
            {
                reader = XmlReader.Create(a_sFileName);
                while (reader.Read() && (!bExit))
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "TestingResults":
                                sXmlData = reader.ReadOuterXml();
                                oResultContract = DriverSOACon.SubmitTestingResults(sXmlData);
                                if (oResultContract.returnStatus != ReturnStatusEnum.Succeeded)
                                {
                                    General.CopyResultError(oReturn, oResultContract);
                                }

                                oReturn.rowsCount++;
                                break;
                        }
                    }
                }
                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                if (reader != null)
                {
                    reader.Close();
                }

                sNewFile = sXmlArchiveFolderTae + Path.GetFileName(a_sFileName);
                if (File.Exists(sNewFile))
                {
                    sNewFile = sXmlArchiveFolderTae + Path.GetFileNameWithoutExtension(a_sFileName)
                        + " _" + DateTime.Now.ToString("yyyyMMddhhmm") + Path.GetExtension(a_sFileName);
                }

                File.Move(a_sFileName, sNewFile);
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.isException = true;
                oReturn.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return oReturn;
        }

    }
}
