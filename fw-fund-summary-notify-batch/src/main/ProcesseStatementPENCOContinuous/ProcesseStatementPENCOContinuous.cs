using ProcesseStatementPENCOContinuousBatch.BLL;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;


namespace ProcesseStatementPENCOContinuousBatch
{
    public class ProcesseStatementPENCOContinuous
    {
        public void Run(string a_sScheduleName)
        {
            try
            {
                Logger.LogMessage(a_sScheduleName + " " + DateTime.Now.ToString());
                Logger.LogMessage("Starting job execution...", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                TRS.IT.BendProcessor.BLL.eStatement eStat = new TRS.IT.BendProcessor.BLL.eStatement();
                EStatement processor = new(eStat);
                processor.ProcesseStatementPENCOContinuous();
                Logger.LogMessage("Job execution completed", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);

            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.Message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                SendErrorEmail($"{a_sScheduleName} - {TaskRetStatus.Failed}", ex.Message, true);
            }
        }

        private static void SendErrorEmail(string subject, string body, bool includeUser)
        {
            string from = AppSettings.GetValue("BendFromEmail");
            string to = AppSettings.GetValue("SystemErrorEmailNotification");

            if (includeUser)
            {
                string userEmail = AppSettings.GetValue("ProcessingErrorEmailNotification");
                to = $"{to};{userEmail}";
            }

            Utils.SendMail(from, to, subject, body);
        }
    }
}
