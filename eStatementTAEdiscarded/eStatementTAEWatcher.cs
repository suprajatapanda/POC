using SIUtil;
using TRS.IT.TrsAppSettings;

namespace bend_fund_wizard_poc.eStatementTAEdiscarded
{
    internal class eStatementTAEWatcher
    {
        private Thread oThreadeStatement0;
        private Thread oThreadeStatement1;
        private Thread oThreadeStatement2;
        private Thread oThreadeStatement3;
        private Thread oThreadeStatement4;
        private Thread oThreadeStatement5;
        private Thread oThreadeStatement6;
        private Thread oThreadeStatement7;
        private Thread oThreadeStatement8;
        private Thread oThreadeStatement9;
        public void Run()
        {
            string seStatementTAEStagingRootFolder = AppSettings.GetValue("eStatementTAEStagingFolder");
            string sStageFolderPath = "";
            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_0") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_0") + @"\"; ;
                eStatementMTh oeStMTH0 = new();
                oThreadeStatement0 = new Thread(new ParameterizedThreadStart(oeStMTH0.ProcesseStatementTAEContinuous));
                oThreadeStatement0.Name = "TASTM_eStatements_PDF_0";
                oThreadeStatement0.Start(sStageFolderPath);
            }
            sStageFolderPath = "";

            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_1") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_1") + @"\"; ;
                eStatementMTh oeStMTH1 = new();
                oThreadeStatement1 = new Thread(new ParameterizedThreadStart(oeStMTH1.ProcesseStatementTAEContinuous));
                oThreadeStatement1.Name = "TASTM_eStatements_PDF_1";
                oThreadeStatement1.Start(sStageFolderPath);
            }
            sStageFolderPath = "";

            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_2") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_2") + @"\"; ;
                eStatementMTh oeStMTH2 = new();
                oThreadeStatement2 = new Thread(new ParameterizedThreadStart(oeStMTH2.ProcesseStatementTAEContinuous));
                oThreadeStatement2.Name = "TASTM_eStatements_PDF_2";
                oThreadeStatement2.Start(sStageFolderPath);
            }
            sStageFolderPath = "";

            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_3") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_3") + @"\"; ;
                eStatementMTh oeStMTH3 = new();
                oThreadeStatement3 = new Thread(new ParameterizedThreadStart(oeStMTH3.ProcesseStatementTAEContinuous));
                oThreadeStatement3.Name = "TASTM_eStatements_PDF_3";
                oThreadeStatement3.Start(sStageFolderPath);
            }
            sStageFolderPath = "";

            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_4") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_4") + @"\"; ;
                eStatementMTh oeStMTH4 = new();
                oThreadeStatement4 = new Thread(new ParameterizedThreadStart(oeStMTH4.ProcesseStatementTAEContinuous));
                oThreadeStatement4.Name = "TASTM_eStatements_PDF_4";
                oThreadeStatement4.Start(sStageFolderPath);
            }
            sStageFolderPath = "";

            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_5") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_5") + @"\"; ;
                eStatementMTh oeStMTH5 = new();
                oThreadeStatement5 = new Thread(new ParameterizedThreadStart(oeStMTH5.ProcesseStatementTAEContinuous));
                oThreadeStatement5.Name = "TASTM_eStatements_PDF_5";
                oThreadeStatement5.Start(sStageFolderPath);
            }
            sStageFolderPath = "";

            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_6") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_6") + @"\"; ;
                eStatementMTh oeStMTH6 = new();
                oThreadeStatement6 = new Thread(new ParameterizedThreadStart(oeStMTH6.ProcesseStatementTAEContinuous));
                oThreadeStatement6.Name = "TASTM_eStatements_PDF_6";
                oThreadeStatement6.Start(sStageFolderPath);
            }
            sStageFolderPath = "";

            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_7") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_7") + @"\"; ;
                eStatementMTh oeStMTH7 = new();
                oThreadeStatement7 = new Thread(new ParameterizedThreadStart(oeStMTH7.ProcesseStatementTAEContinuous));
                oThreadeStatement7.Name = "TASTM_eStatements_PDF_7";
                oThreadeStatement7.Start(sStageFolderPath);
            }
            sStageFolderPath = "";

            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_8") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_8") + @"\"; ;
                eStatementMTh oeStMTH8 = new();
                oThreadeStatement8 = new Thread(new ParameterizedThreadStart(oeStMTH8.ProcesseStatementTAEContinuous));
                oThreadeStatement8.Name = "TASTM_eStatements_PDF_8";
                oThreadeStatement8.Start(sStageFolderPath);
            }
            sStageFolderPath = "";

            if (AppSettings.GetValue("Disable_TASTM_eStatements_PDF_9") != "1")
            {
                sStageFolderPath = Path.Combine(seStatementTAEStagingRootFolder, "TASTM_eStatements_PDF_9") + @"\"; ;
                eStatementMTh oeStMTH9 = new();
                oThreadeStatement9 = new Thread(new ParameterizedThreadStart(oeStMTH9.ProcesseStatementTAEContinuous));
                oThreadeStatement9.Name = "TASTM_eStatements_PDF_9";
                oThreadeStatement9.Start(sStageFolderPath);
            }
            sStageFolderPath = "";
            Logger.LogMessage("TAE eStatement Watcher is on", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
        }
    }
}
