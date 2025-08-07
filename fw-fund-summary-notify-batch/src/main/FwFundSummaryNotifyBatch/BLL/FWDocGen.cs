using System.Data;
using SIUtil;
using TARSharedUtilLib.Utility;
using TRS.IT.SI.BusinessFacadeLayer;

namespace FWFundSummaryNotifyBatch.BLL
{
    public class FWDocGen
    {
        private readonly FundWizard? _fundWizard;
        public string _sTemplatePath { get; set; } = string.Empty;
        public string _sOutputPath { get; set; } = string.Empty;
        public string _sLocalPath { get; set; } = string.Empty;
        private string _sLicenseFile { get; set; } = string.Empty;

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

        public FWDocGen(FundWizard fundWizard)
        {
            _fundWizard = fundWizard;
        }
        public FWDocGen()
        {
        }
        public string CreateFundChangesSummary(string sPartnerID, DateTime dtEffectiveDt, ref string a_sOutFileName)
        {
            string error = string.Empty;

            try
            {
                var dataSet = DAL.FWDocGenDC.GetSummaryFundChangesData(sPartnerID, dtEffectiveDt);

                if (dataSet?.Tables.Count > 0)
                {
                    var dataTable = dataSet.Tables[0];
                    var dateSuffix = dtEffectiveDt.ToString("yyyyMMdd");
                    a_sOutFileName = WriteFundChangesSummaryFile(dataTable, $"{sPartnerID}-Efdt-{dateSuffix}");
                }

            }
            catch (Exception ex)
            {
                a_sOutFileName = string.Empty;
                error = ex.Message;
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
            }

            return error;
        }

        private string WriteFundChangesSummaryFile(DataTable dtFinal, string sAppendFileName)
        {
            if (dtFinal == null)
                return string.Empty;

            var workbook = new Aspose.Cells.Workbook();
            SetCellsLicense();
            var sheet = workbook.Worksheets[0];
            
            Aspose.Cells.ImportTableOptions importTableOptions = new Aspose.Cells.ImportTableOptions();
            importTableOptions.IsFieldNameShown = true;
            
            int row = 1;
            int column = 0;
            
            sheet.Cells.ImportData(dtFinal, row, column, importTableOptions);

            var style = workbook.CreateStyle();
            style.Custom = "mm/dd/yyyy";
            
            var styleFlag = new Aspose.Cells.StyleFlag();
            styleFlag.NumberFormat = true;
            
            sheet.Cells.Columns[3].ApplyStyle(style, styleFlag);
            sheet.Cells.Columns[4].ApplyStyle(style, styleFlag);
            

            var fileName = $"FundChangesSummary-{sAppendFileName}.xlsx";
            string outputBasePath = AppDomain.CurrentDomain.BaseDirectory;
            var localPath = Path.Combine(outputBasePath, fileName);

            var fullPath = Path.Combine(OutputPath, fileName);

            workbook.Save(localPath);
            AutofitExcelColumnsClosedXML(localPath);
            FileManagerSMB.Move(localPath, fullPath);
            return fullPath;
        }
        private void SetCellsLicense()
        {
            var license = new Aspose.Cells.License();
            if (File.Exists(LicenseFile))
            {
                license.SetLicense(LicenseFile);
            }
        }
        private static void AutofitExcelColumnsClosedXML(string localPath)
        {
            using (var workbook = new ClosedXML.Excel.XLWorkbook(localPath))
            {
                foreach (var worksheet in workbook.Worksheets)
                {
                    worksheet.Columns().AdjustToContents();
                }

                workbook.Save();
            }
            Logger.LogMessage($"Autofit applied to Excel file using ClosedXML", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
        }

    }
}