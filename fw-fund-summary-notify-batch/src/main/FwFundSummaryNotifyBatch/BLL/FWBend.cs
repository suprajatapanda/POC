using System.Data;
using SIUtil;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;


namespace FWFundSummaryNotifyBatch.BLL
{
    public class FWBend
    {

        private readonly FWBendDC _oFWDC = new();
        TRS.IT.BendProcessor.BLL.FWBend fWBend;
        public FWBend(TRS.IT.BendProcessor.BLL.FWBend obj) 
        {
            fWBend = obj;
        }

        public TaskStatus ProcessSendSummary()
        {
            const string C_Task = "ProcessSendSummary";
            var taskStatus = new TaskStatus { retStatus = TaskRetStatus.NotRun };

            try
            {
                var configValue = TRS.IT.TrsAppSettings.AppSettings.GetValue(C_Task);
                if (string.IsNullOrEmpty(configValue) || configValue != "1")
                    return taskStatus;

                fWBend.InitTaskStatus(taskStatus, C_Task);

                var nextBusinessDay = Convert.ToDateTime(TARSharedUtilLibBFLBLL.FWUtils.GetNextBusinessDay(DateTime.Today, 1));
                var result = SendSummaryToPartner(ConstN.C_PARTNER_TAE, nextBusinessDay);

                if (result.returnStatus != ReturnStatusEnum.Succeeded)
                {
                    taskStatus.retStatus = TaskRetStatus.Failed;
                    General.CopyResultError(taskStatus, result);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                fWBend.InitTaskError(taskStatus, ex, true);
            }

            taskStatus.endTime = DateTime.Now;
            return taskStatus;
        }

        private ResultReturn SendSummaryToPartner(string a_sPartnerId, DateTime a_dtEffectiveDt)
        {

            var result = new ResultReturn();
            var docGen = new FWDocGen();
            string returnFileName = string.Empty;

            try
            {
                SetFWDocGenPaths(docGen);

                var error = docGen.CreateFundChangesSummary(a_sPartnerId, a_dtEffectiveDt, ref returnFileName);

                if (string.IsNullOrEmpty(error))
                {
                    var fromEmail = AppSettings.GetValue("FWBendEmailAddr");
                    var toEmail = AppSettings.GetValue("FWDailySummaryEmail" + a_sPartnerId);

                    Utils.SendMail(
                    fromEmail,
                    toEmail,
                    "Fund Change Summary",
                    "Fund Change Summary data is attached.",
                    new[] { returnFileName },
                    AppSettings.GetValue("BCCEmailNotification")
                    );

                    result.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    result.returnStatus = ReturnStatusEnum.Failed;
                    result.Errors.Add(new ErrorInfo(-1, error, ErrorSeverityEnum.Error));
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                result.returnStatus = ReturnStatusEnum.Failed;
                result.isException = true;
                result.confirmationNo = string.Empty;
                result.Errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return result;
        }

        private static void SetFWDocGenPaths(FWDocGen a_oFWDocGen)
        {
            a_oFWDocGen.OutputPath = AppSettings.GetValue("FWDocGenOutputPath");
            a_oFWDocGen.TemplatePath = AppSettings.GetValue("FWDocGenTemplatePath");
            a_oFWDocGen.LocalPath = AppSettings.GetValue("FWDocGenLocalPath");
            a_oFWDocGen.LicenseFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Aspose.Total.lic");
        }
    }
}
