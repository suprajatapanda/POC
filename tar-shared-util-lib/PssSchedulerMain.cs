using System.Text;
using SIUtil;
using TRS.IT.BendProcessor.BLL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.BendScheduler.BLL;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace wsBendPssScheduler
{
    public partial class PssSchedulerMain
    {
        public PssSchedulerMain()
        {
            ScheduleCallBackDaily("TAGScheduleRpt_Run");
            ScheduleCallBackDaily("ScheduleRpt_Notify");
            ScheduleCallBackDaily("ConsolidateNotifications");
            ScheduleCallBackDaily("SendConsolidatedNotifications");
            ScheduleCallBackDaily("CreateScheduledReports");
            ScheduleCallBackDaily("ISCParticipantCount");
            ScheduleCallBackDaily("LoanPayoffNotifications");
            ScheduleCallBackDaily("ProcessPayroll360ScheduledRpt");
            ScheduleCallBackDaily("ProcessRMDLetters");
            ScheduleCallBackDaily("FIBIScheduleRpt_Run");
            ScheduleCallBackWeekly("SundayScheduleRpt_Run");
            ScheduleCallBackWeekly("SundayScheduleRpt_Notify");
            ScheduleCallBackMonthly("InvalidPptAddressReport");
            ScheduleCallBackMonthly("ISCLateLoanLetters");
            ScheduleCallBackMonthly("ProcessAllMLFlatFiles");
            ScheduleCallBackMonthly("ProcessMLBORTOAndAWAY");
        }
        private string ParseError(List<ErrorInfo> a_oError)
        {
            StringBuilder strB = new();

            foreach (ErrorInfo oE in a_oError)
            {
                strB.AppendLine(oE.errorDesc);
            }
            return strB.ToString();
        }
        private void ScheduleCallBackDaily(string a_sScheduleName)
        {
            TaskStatus oTaskStatus = null;
            string sError;
            try
            {
                if (AppSettings.GetValue("ScheduleCallBackDaily") == "1")
                {
                    AddInfoEventLog(a_sScheduleName + " called ScheduleCallBackDaily" + DateTime.Now.ToString());
                }

                switch (a_sScheduleName)
                {
                    case "TAGScheduleRpt_Run":
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                            PASSScheduler oP = new();
                            oTaskStatus = oP.ProcessTAGScheduledRptMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                                AddErrorEventLog(a_sScheduleName + ": " + sError);
                            }
                            else
                            {
                                //ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                                AddInfoEventLog(a_sScheduleName + ": Success");
                            }
                            oP.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
                        }

                        break;
                    case "ScheduleRpt_Notify":
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                            PASSScheduler oP1 = new();
                            oTaskStatus = oP1.ProcessPASSScheduledPendingRptMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                                AddErrorEventLog(a_sScheduleName + ": " + sError);
                            }
                            else
                            {
                                //ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                                AddInfoEventLog(a_sScheduleName + ": Success");
                            }
                            oP1.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
                        }
                        break;
                    case "ConsolidateNotifications":
                        AddInfoEventLog(a_sScheduleName + ": Begin Task");
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);
                            ConsolidatedNotifications oNots = new();

                            oTaskStatus = oNots.ProcessNotificationInputDetailsMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                                AddErrorEventLog(a_sScheduleName + ": Error:" + sError);
                            }
                            else
                            {
                                //ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                                AddInfoEventLog(a_sScheduleName + ": End Task");
                            }
                            oNots.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddErrorEventLog(a_sScheduleName + ": Task did not run because it ran already.");
                        }
                        break;
                    case "SendConsolidatedNotifications":
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);
                            ConsolidatedNotifications oNots1 = new();

                            oTaskStatus = oNots1.ProcessMessageQueueMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                                AddErrorEventLog(a_sScheduleName + ": " + sError);
                            }
                            else
                            {
                                AddInfoEventLog(a_sScheduleName + ": End Task");
                                //ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                            }
                            oNots1.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddInfoEventLog(a_sScheduleName + ": Task did not run becuase it ran already.");
                        }
                        break;
                    case "CreateScheduledReports":
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                            PASSScheduler oP = new();
                            oTaskStatus = oP.ProcessCreateScheduledReportsMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                                AddErrorEventLog(a_sScheduleName + ": " + sError);
                            }
                            else
                            {
                                //ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                                AddInfoEventLog(a_sScheduleName + ": Success");
                            }
                            oP.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
                        }

                        break;
                    case "ISCParticipantCount":
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                            MLScorecard oP = new();
                            oTaskStatus = oP.ProcessISCParticipantCountMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, false);
                                AddErrorEventLog(a_sScheduleName + ": " + sError);
                            }
                            else
                            {
                                AddInfoEventLog(a_sScheduleName + ": Success");
                            }
                            oP.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
                        }

                        break;
                    case "LoanPayoffNotifications":
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                            ISCData oIsc = new();
                            oTaskStatus = oIsc.ProcessLoanPayoffNotificationsMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, false);
                                AddErrorEventLog(a_sScheduleName + ": " + sError);
                            }
                            else
                            {
                                AddInfoEventLog(a_sScheduleName + ": Success");
                            }
                            oIsc.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
                        }

                        break;
                    case "ProcessPayroll360ScheduledRpt":
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                            PASSScheduler oP = new();
                            oTaskStatus = oP.ProcessPayroll360ScheduledRptMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                                AddErrorEventLog(a_sScheduleName + ": " + sError);
                            }
                            else
                            {
                                AddInfoEventLog(a_sScheduleName + ": Success");
                            }
                            oP.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
                        }

                        break;
                    case "ProcessRMDLetters":
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                            RMDMigrated oRDM = new();
                            oTaskStatus = oRDM.ProcessRMDLettersMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                                AddErrorEventLog(a_sScheduleName + ": " + sError);
                            }
                            else
                            {
                                AddInfoEventLog(a_sScheduleName + ": Success");
                            }
                            oRDM.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
                        }

                        break;
                    case "FIBIScheduleRpt_Run":
                        if (!ScheduleUtils.HasJobRun(a_sScheduleName))
                        {
                            ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                            PASSScheduler oP = new();
                            oTaskStatus = oP.ProcessFIBIScheduledRptMigrated();
                            ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                            if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                            {
                                sError = ParseError(oTaskStatus.errors);
                                SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                                AddErrorEventLog(a_sScheduleName + ": " + sError);
                            }
                            else
                            {
                                AddInfoEventLog(a_sScheduleName + ": Success");
                            }
                            oP.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        }
                        else
                        {
                            AddErrorEventLog(a_sScheduleName + ": HasJobRun returned false.");
                        }

                        break;

                    default:
                        AddInfoEventLog(a_sScheduleName + ": NOT IMPLEMENTED.");
                        break;

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                string sStatusError = string.Empty;
                if (oTaskStatus != null)
                {
                    ParseError(oTaskStatus.errors);
                }

                SendErrorEmail("ScheduleCallBackDaily: " + a_sScheduleName, sStatusError + " ex: " + ex.Message, false);
            }
        }
        private void ScheduleCallBackWeekly(string a_sScheduleName)
        {
            TaskStatus oTaskStatus = null;
            string sError = "";
            try
            {
                if (AppSettings.GetValue("ScheduleCallBackWeekly") == "1")
                {
                    AddInfoEventLog(a_sScheduleName + " " + DateTime.Now.ToString());
                }

                switch (a_sScheduleName)
                {
                    case "SundayScheduleRpt_Run":
                        ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.DAILY, "TRS", string.Empty, true);

                        PASSScheduler oP = new();
                        oTaskStatus = oP.ProcessSundayScheduledRptMigrated();
                        ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);//Always Mark as complete

                        if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                        {
                            sError = ParseError(oTaskStatus.errors);
                            SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                            AddErrorEventLog(a_sScheduleName + ": " + sError);
                        }
                        else
                        {
                            AddInfoEventLog(a_sScheduleName + ": Success");
                        }
                        oP.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        break;
                    case "SundayScheduleRpt_Notify":
                        ScheduleUtils.InsertScheduleLog(DateTime.Today, a_sScheduleName, ScheduleType.WEEKLY, "TRS", string.Empty, true);

                        PASSScheduler oP1 = new();
                        oTaskStatus = oP1.ProcessSundayScheduledPendingRptMigrated();
                        ScheduleUtils.UpdateScheduleLogComplete(DateTime.Today, a_sScheduleName, "Done", DateTime.Now, 0);
                        if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                        {
                            sError = ParseError(oTaskStatus.errors);
                            SendErrorEmail(a_sScheduleName + "-" + oTaskStatus.retStatus.ToString(), sError, true);
                            AddErrorEventLog(a_sScheduleName + ": " + sError);
                        }
                        else
                        {
                            AddInfoEventLog(a_sScheduleName + ": Success");
                        }
                        oP1.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                string sStatusError = string.Empty;
                if (oTaskStatus != null)
                {
                    ParseError(oTaskStatus.errors);
                }

                SendErrorEmail("ScheduleCallBackWeekly: " + a_sScheduleName, sStatusError + " ex: " + ex.Message, false);
            }
        }
        private void ScheduleCallBackMonthly(string a_sScheduleName)
        {
            TaskStatus oTaskStatus = null;

            try
            {
                if (AppSettings.GetValue("ScheduleCallBackMonthly") == "1")
                {
                    AddInfoEventLog(a_sScheduleName + " " + DateTime.Now.ToString());
                }

                switch (a_sScheduleName)
                {
                    case "InvalidPptAddressReport":
                        eStatement oeSti = new();
                        oTaskStatus = oeSti.ProcessInvalidPptAddressReportMigrated();
                        if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                        {
                            AddErrorEventLog(a_sScheduleName + ": " + ParseError(oTaskStatus.errors));
                        }

                        oeSti.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        break;
                    case "ISCLateLoanLetters":
                        ISCData oIsc = new();
                        oTaskStatus = oIsc.ProcessLateLoanLettersMigrated();
                        if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                        {
                            AddErrorEventLog(a_sScheduleName + ": " + ParseError(oTaskStatus.errors));
                        }

                        oIsc.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        break;
                    case "ProcessAllMLFlatFiles":
                        MLScorecard oMLc = new();
                        oTaskStatus = oMLc.ProcessAllMLFlatFilesMigrated();
                        if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                        {
                            AddErrorEventLog(a_sScheduleName + ": " + ParseError(oTaskStatus.errors));
                        }

                        oMLc.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        break;
                    case "ProcessMLBORTOAndAWAY":
                        MLScorecard oMLc1 = new();
                        oTaskStatus = oMLc1.ProcessMLBORTOAndAWAYMigrated();
                        if (oTaskStatus.retStatus != TaskRetStatus.Succeeded)
                        {
                            AddErrorEventLog(a_sScheduleName + ": " + ParseError(oTaskStatus.errors));
                        }

                        oMLc1.SendTaskCompleteEmail(a_sScheduleName, oTaskStatus.retStatus.ToString(), a_sScheduleName);
                        break;

                    default:
                        break;

                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                string sStatusError = string.Empty;
                if (oTaskStatus != null)
                {
                    ParseError(oTaskStatus.errors);
                }

                SendErrorEmail("ScheduleCallBackMonthly: " + a_sScheduleName, sStatusError + " ex: " + ex.Message, false);
            }
        }

        private void SendErrorEmail(string a_sSubject, string a_sBody, Boolean a_bToUserAsWell)
        {
            string sFr = AppSettings.GetValue("BendFromEmail");
            string sTo = AppSettings.GetValue("SystemErrorEmailNotification");
            if (a_bToUserAsWell)
            {
                sTo += ";" + AppSettings.GetValue("ProcessingErrorEmailNotification");
            }
            Utils.SendMail(sFr, sTo, a_sSubject, a_sBody);
        }

        private void AddInfoEventLog(string eventLog)
        {
            Logger.LogMessage(eventLog, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
        }

        private void AddErrorEventLog(string eventLog)
        {
            Logger.LogMessage(eventLog, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
        }

    }
}
