using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SIUtil;
using TarPptBouncedEmailBatch.BLL;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace TarPptBouncedEmailBatch
{
    public class DailyeStatementProcessBouncedEmail
    {
        public void Run(string a_sScheduleName)
        {
            TaskStatus? oTaskStatus = null;
            try
            {
                if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                {
                    ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);
                    TRS.IT.BendProcessor.BLL.eStatement eStat = new TRS.IT.BendProcessor.BLL.eStatement();
                    EStatement oSt2 = new(eStat);
                    oTaskStatus = oSt2.ProcessBouncedEmail();
                    if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                    {
                        SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), ParseError(oTaskStatus.errors), true);
                        AddErrorEventLog(a_sScheduleName + ": " + ParseError(oTaskStatus.errors));
                    }
                    else
                    {
                        ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                        AddInfoEventLog(a_sScheduleName + ": Success");
                    }
                    eStat.SendTaskCompleteEmail(a_sScheduleName, General.ParseTaskInfo(oTaskStatus), a_sScheduleName);
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                string sStatusError = string.Empty;
                if (oTaskStatus != null) ParseError(oTaskStatus.errors);

                SendErrorEmail("ScheduleCallBackDaily: " + a_sScheduleName, sStatusError + " ex: " + ex.Message, false);
            }
        }


        private string ParseError(List<ErrorInfo> a_oError)
        {
            StringBuilder strB = new StringBuilder();

            foreach (ErrorInfo oE in a_oError)
            {
                strB.Append(oE.errorDesc + "\n");
            }
            return strB.ToString();
        }

        private void SendErrorEmail(string a_sSubject, string a_sBody, System.Boolean a_bToUserAsWell)
        {
            string sFr = AppSettings.GetValue("BendFromEmail");
            string sTo = AppSettings.GetValue("SystemErrorEmailNotification");
            if (a_bToUserAsWell)
            {
                sTo += ";" + AppSettings.GetValue("ProcessingErrorEmailNotification");
            }
            Utils.SendMail(sFr, sTo, a_sSubject, a_sBody);
        }

        private void AddErrorEventLog(string eventLog)
        {
            Logger.LogMessage(eventLog, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
        }

        private void AddInfoEventLog(string eventLog)
        {
            Logger.LogMessage(eventLog, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
        }
    }   
}
