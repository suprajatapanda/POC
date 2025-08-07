using System.Text;
using SIUtil;
using TarPptBouncedEmailBatch.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using BFL = TRS.IT.SI.BusinessFacadeLayer;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;

namespace TarPptBouncedEmailBatch.BLL
{
    public class EStatement(TRS.IT.BendProcessor.BLL.eStatement obj)
    {
        private EStatementDC _oeSDC = new();
        TRS.IT.BendProcessor.BLL.eStatement eStat = obj;
        public TaskStatus ProcessBouncedEmail()
        {
            TaskStatus oTaskReturn = new();
            ResultReturn oReturn;

            const string C_Task = "ProcessBouncedEmail";

            StringBuilder strB = new();

            try
            {
                oTaskReturn.retStatus = TaskRetStatus.NotRun;
                if (AppSettings.GetValue(C_Task) == "1")
                {
                    eStat.InitTaskStatus(oTaskReturn, C_Task);

                    oReturn = GetBouncedEmailFile();

                    while (oReturn.rowsCount > 0 && oReturn.returnStatus == ReturnStatusEnum.Succeeded)
                    {

                        if (oReturn.returnStatus == ReturnStatusEnum.Succeeded)
                        {
                            string sData = "";
                            string[] s_arr;
                            bool bIsReset = false;
                            int iBouncedEmailCount;
                            int iRet;
                            string sEmail = "";
                            int iInLoginId = 0;
                            string sConId = "";
                            string sSubId = "";
                            string sSMTPCode = "";
                            string sSMTPReason = "";
                            int iPrefId = 1;
                            int iProcessStatus;
                            int iNotificationType = 0;

                            using (StreamReader sr = new(oReturn.confirmationNo))
                            {
                                sr.ReadLine();//skip first line
                                while (sr.Peek() >= 0)
                                {
                                    try
                                    {
                                        iProcessStatus = 0;
                                        sData = sr.ReadLine();
                                        s_arr = sData.Split(',');
                                        if (s_arr.Length != 7)
                                        {
                                            oTaskReturn.errors.Add(new ErrorInfo(-1, "Unable to process bad email:  " + sData, ErrorSeverityEnum.Failed));
                                        }
                                        else
                                        {
                                            iNotificationType = Convert.ToInt32(s_arr[0]);
                                            sEmail = s_arr[1];
                                            iInLoginId = Convert.ToInt32(s_arr[2]);
                                            sConId = s_arr[3];
                                            sSubId = s_arr[4];
                                            sSMTPCode = s_arr[5];
                                            sSMTPReason = s_arr[6];

                                            bool bIsEmailFailureOption1 = IsBadEmailOption1(s_arr[5], s_arr[6]);

                                            switch (iNotificationType)
                                            {
                                                case 1:
                                                    iPrefId = 2;
                                                    break;
                                                case 2:
                                                    iPrefId = 1;
                                                    break;
                                                case 4:
                                                    iPrefId = 3;
                                                    break;
                                            }
                                            if (bIsEmailFailureOption1)
                                            {
                                                bIsReset = true;
                                            }
                                            else
                                            {
                                                iBouncedEmailCount = _oeSDC.GetEmailBouncedCount(iInLoginId, sConId, sSubId, iPrefId);
                                                if (iBouncedEmailCount > 0)
                                                {
                                                    bIsReset = true;
                                                }
                                                else
                                                {
                                                    iRet = _oeSDC.IncreaseEmailBouncedCount(iInLoginId, sConId, sSubId, iPrefId);
                                                }
                                            }

                                            if (bIsReset)
                                            {
                                                oReturn = ReseteDocumentsPreference(iInLoginId, sConId, sSubId, iNotificationType);
                                                if (oReturn.returnStatus == ReturnStatusEnum.Succeeded)
                                                {
                                                    iProcessStatus = 100;
                                                    oReturn = SendEmailBouncedLetter(iInLoginId, sConId, sSubId, iNotificationType);

                                                    if (oReturn.returnStatus != ReturnStatusEnum.Succeeded)
                                                    {
                                                        iProcessStatus = -1;
                                                        General.CopyResultError(oTaskReturn, oReturn);
                                                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                                    }

                                                    iRet = _oeSDC.UpdateForcedOptOutDate(iInLoginId, sConId, sSubId, iPrefId);
                                                }
                                                else//error
                                                {
                                                    iProcessStatus = -1;
                                                    General.CopyResultError(oTaskReturn, oReturn);
                                                    oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;
                                                }
                                            }
                                            oTaskReturn.rowsCount++;
                                            _oeSDC.InsertBouncedEmailHistory(iInLoginId, sConId, sSubId, sEmail, sSMTPCode, sSMTPReason, iProcessStatus, iNotificationType);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                                        oTaskReturn.errors.Add(new ErrorInfo(-1, "ex: " + ex.Message + ". Unable to process bad email:  " + sData, ErrorSeverityEnum.ExceptionRaised));
                                        oTaskReturn.retStatus = TaskRetStatus.ToCompletionWithErr;

                                    }
                                }
                            }

                        }
                        else
                        {
                            oTaskReturn.retStatus = TaskRetStatus.Failed;
                            General.CopyResultError(oTaskReturn, oReturn);
                        }
                        oReturn = GetBouncedEmailFile();
                    } //while rowcount > 0
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
                eStat.InitTaskError(oTaskReturn, ex, true);
            }

            oTaskReturn.endTime = DateTime.Now;
            eStat.SendTaskCompleteEmail("eStatement bounced emails - " + oTaskReturn.retStatus.ToString(), General.ParseTaskInfo(oTaskReturn), "eStatement Backend Processing");


            return oTaskReturn;

        }

        private ResultReturn GetBouncedEmailFile()
        {
            ResultReturn oReturn = new();

            FTPUtility oFtp = new(AppSettings.GetVaultValue("FTPHostName"),
            AppSettings.GetVaultValue("FTPUserName"),
            AppSettings.GetVaultValue("FTPPassword"));

            FTPdirectory oFTPDir = oFtp.ListDirectoryDetail(AppSettings.GetValue("FTPBouncedEmailFolder"));

            oReturn.rowsCount = oFTPDir.Count;
            //Get one file at a time
            if (oFTPDir.Count > 0)
            {
                bool b = false;
                b = oFtp.Download(oFTPDir[0].FullName,
                    AppSettings.GetValue("LocalBouncedEmailFolder") + oFTPDir[0].Filename, true);
                if (b)
                {
                    oReturn.confirmationNo = AppSettings.GetValue("LocalBouncedEmailFolder") + oFTPDir[0].Filename;
                    FileInfo oFileInfo = new(oReturn.confirmationNo);
                    if (oFileInfo.Exists)
                    {
                        if (oFtp.FtpRename(oFTPDir[0].FullName, AppSettings.GetValue("FTPBouncedEmailProcessedFolder") + oFTPDir[0].Filename))
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                        }
                        else
                        {
                            oReturn.returnStatus = ReturnStatusEnum.Failed;
                        }
                    }
                    else
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                    }
                }
                else
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(-1, "Failed to download bounced email file from DIA FTP", ErrorSeverityEnum.Error));
                }
            }
            return oReturn;
        }

        private bool IsBadEmailOption1(string a_sSMTPCode, string a_sReason)
        {
            bool bOption1 = false;
            switch (a_sSMTPCode)
            {
                case "5.1.0":
                case "5.1.1":
                    bOption1 = true;
                    break;
                case "5.0.0":
                case "5.2.1":
                case "5.4.4":
                    if (a_sReason.Contains("Invalid recipient")
                        || a_sReason.Contains("User [] does not exist")
                        || a_sReason.Contains("invalid mailbox")
                        || a_sReason.Contains("not a valid user")
                        || a_sReason.Contains("User unknown")
                        || a_sReason.Contains("does not exist here")
                        || a_sReason.Contains("No account by that name here")
                        || a_sReason.Contains("MAILBOX NOT FOUND")
                        || a_sReason.Contains("unknown user")
                        || a_sReason.Contains("nonexistent domain")
                        || a_sReason.Contains("no such user")
                        || a_sReason.Contains("mailbox disabled")
                        || a_sReason.Contains("user account is unavailable")
                        || a_sReason.Contains("no account by that name"))
                    {
                        bOption1 = true;
                    }

                    break;
            }
            return bOption1;
        }

        private ResultReturn ReseteDocumentsPreference(int in_loging_id, string contract_id, string sub_id, int a_iNotificationType)
        {
            ResultReturn oReturn = new();
            BFL.Model.SIResponse oResponse = default(BFL.Model.SIResponse);
            Participant oPart = null;
            string sError = "Reset eDocuments Preference failed for in_login_id : " + in_loging_id.ToString() + " ConId " + contract_id + " SubId " + sub_id;
            string sSessionID = "";

            try
            {
                //update partner
                //sSessionID = BFL.Participant.CreateSession(in_loging_id, contract_id, sub_id, 4, "BkendProc", "");
                sSessionID = "E725BF6A-0A3D-4027-AA64-B576D6A49F62";
                oPart = new Participant(sSessionID);

                BFL.Model.ParticipantInfo oPartInfo = new();
                BFL.Model.PersonalProfile oPartProfile = new();
                //oPartProfile = oPart.GetPersonalProfile();

                oPartInfo = oPart.GetParticipantInfoStayed(false, false);// no need to get fund restrictions.
                if (oPartInfo.Errors[0].Number == 0)
                {
                    oPartProfile = oPartInfo.PersonalInfo;

                    //Reset eDocumentsPreference to N
                    switch (a_iNotificationType)
                    {
                        case 1: //eConfirm
                            oPartProfile.eConfirmPreference = "N";
                            break;
                        case 2: //eStatement
                            oPartProfile.eStmtPreference = "N";
                            break;
                        case 4: //ReqdNotices
                            oPartProfile.ReqdNoticesPreference = "N";
                            break;
                    }

                    oResponse = oPart.UpdatePersonalProfile(oPartProfile, false);

                    if (oResponse.Errors[0].Number == 0)
                    {
                        oReturn.confirmationNo = oResponse.ConfirmationNumber;
                        oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                    }
                    else
                    {
                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                        oReturn.Errors.Add(new ErrorInfo(oResponse.Errors[0].Number, sError + oResponse.Errors[0].Description, ErrorSeverityEnum.Failed));
                    }
                }
                else
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    oReturn.Errors.Add(new ErrorInfo(oPartInfo.Errors[0].Number, sError + oPartInfo.Errors[0].Description, ErrorSeverityEnum.Failed));
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                oReturn.Errors.Add(new ErrorInfo(-1, sError + "ex: " + ex.Message, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }

        private ResultReturn SendEmailBouncedLetter(int in_loging_id, string contract_id, string sub_id, int iNotificationType)
        {
            ResultReturn oReturn = new();
            BFL.Model.SIResponse oResponse = default(BFL.Model.SIResponse);
            Participant oPart = null;
            string sError = "";
            string sSessionID = "";

            try
            {
                //update partner
                sSessionID = Participant.CreateSession(in_loging_id, contract_id, sub_id, 4, "BkendProc", "");
                oPart = new Participant(sSessionID);
                BFL.Model.ConfirmationLetterInfo oConfirmationInfo = new();
                BFL.Model.ConfirmationLetterInfo.LetterInfo[] oLettersAry = new BFL.Model.ConfirmationLetterInfo.LetterInfo[1];
                BFL.Model.ConfirmationLetterInfo.LetterInfo oLetterInfo = new();

                oLetterInfo.LetterType = BFL.Model.LetterTypeEnum.NoticeOfUndeliverableEmail.GetHashCode();

                string[] sDocsAry = new string[1];
                switch (iNotificationType)
                {
                    case 1:
                        sDocsAry[0] = "Financial Confirmations";
                        break;
                    case 2:
                        sDocsAry[0] = "Statements";
                        break;
                    case 4:
                        sDocsAry[0] = "Required Notices";
                        break;
                }
                oLetterInfo.UndeliverableDocuments = sDocsAry;

                oLettersAry[0] = oLetterInfo;
                oConfirmationInfo.Letters = oLettersAry;

                oResponse = oPart.RequestConfirmationLetter(oConfirmationInfo);

                if (oResponse.Errors[0].Number == 0)
                {
                    oReturn.confirmationNo = oResponse.ConfirmationNumber;
                    oReturn.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    oReturn.returnStatus = ReturnStatusEnum.Failed;
                    sError = "SendEmailBouncedLetter failed Err No.: " + oResponse.Errors[0].Number + ": " + oResponse.Errors[0].Description;
                    oReturn.Errors.Add(new ErrorInfo(oResponse.Errors[0].Number, sError, ErrorSeverityEnum.Failed));
                }

            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.returnStatus = ReturnStatusEnum.Failed;
                sError = "SendEmailBouncedLetter Exception Raised:  " + ex.Message;
                oReturn.Errors.Add(new ErrorInfo(oResponse.Errors[0].Number, sError, ErrorSeverityEnum.ExceptionRaised));
            }

            return oReturn;
        }

        

    }
}
