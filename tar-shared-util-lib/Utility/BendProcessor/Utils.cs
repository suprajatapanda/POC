using System.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SIUtil;
using TRS.IT.BendProcessor.Model;
using TRS.IT.TrsAppSettings;
using TRS.SqlHelper;

namespace TRS.IT.BendProcessor.Util
{
    public class Utils
    {
        private Utils() { }

        public static string SubIn(string a_sSubId)
        {
            string sConnectString = "";
            sConnectString = AppSettings.GetConnectionString("ConnectString");
            return TRSSqlHelper.ExecuteScalar(sConnectString, "pSI_GetSubIn", [a_sSubId]).ToString();
        }
        public static string CheckDBNullStr(object a_oVal)
        {
            if (a_oVal == DBNull.Value)
            {
                return string.Empty;
            }
            else
            {
                return a_oVal.ToString();
            }
        }
        public static int CheckDBNullInt(object a_oVal)
        {
            if (a_oVal == DBNull.Value)
            {
                return 0;
            }
            else
            {
                return Convert.ToInt32(a_oVal);
            }
        }
        public static double CheckDBNullDb(object a_oVal)
        {
            if (a_oVal == DBNull.Value)
            {
                return 0.0;
            }
            else
            {
                return Convert.ToDouble(a_oVal);
            }
        }
        public static string CheckDBNull(object a_oData) => a_oData is DBNull | a_oData == null ? "" : a_oData.ToString().Trim();
        public static string PayrollCheckNull(string a_sStr)
        {
            if (a_sStr == null)
            {
                return string.Empty;
            }
            else
            {
                return a_sStr.Replace(",", "");
            }
        }
        public static void ValidatePath(string path)
        {
            string directory = Path.GetDirectoryName(path);
            CreateDirectory(new DirectoryInfo(directory));
        }
        public static void CreateDirectory(DirectoryInfo directory)
        {
            if (!directory.Parent.Exists)
            {
                CreateDirectory(directory.Parent);
            }

            directory.Create();
        }
        public static string MakeFileNameValid(string filename)
        {
            char[] invalidFileChars = Path.GetInvalidFileNameChars();
            int iPosition = -1;
            iPosition = filename.IndexOfAny(invalidFileChars);
            while (iPosition > 0)
            {
                filename = filename.Replace(filename.Substring(iPosition, 1), "~");
                iPosition = filename.IndexOfAny(invalidFileChars);
            }
            return filename;
        }
        public static void SendMail(string FromAddress, string ToAddress, string Subject, string MessageText)
        {
            SendEmail(FromAddress, ToAddress, Subject, MessageText, [], "");
        }
        public static void SendMail(string FromAddress, string ToAddress, string Subject, string MessageText, string Bcc)
        {
            SendEmail(FromAddress, ToAddress, Subject, MessageText, [], Bcc);
        }
        public static void SendMail(string FromAddress, string ToAddress, string Subject, string MessageText, string[] AttachedFiles, string Bcc)
        {
            SendEmail(FromAddress, ToAddress, Subject, MessageText, AttachedFiles, Bcc);
        }
        public static void SendMail(MimeMessage a_oMailMsg)
        {
            using (var oMail = new SmtpClient())
            {
                oMail.ServerCertificateValidationCallback = (s, c, h, e) => {
                    return true;
                };
                oMail.Connect(AppSettings.GetValue("SMTPServer"), 587, SecureSocketOptions.StartTls);
                oMail.Send(a_oMailMsg);
                oMail.Disconnect(true);
            }
        }
        public static void LogError(Exception ex)
        {
            try
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
            catch (Exception exp)
            {
                Logger.LogMessage(exp.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
        }
        public static void LogInfo(string infoToLog)
        {
            try
            {
                Logger.LogMessage(infoToLog, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
            }
            catch (Exception exp)
            {
                Logger.LogMessage(exp.ToString(), Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);
            }
        }
        private static void SendEmail(string FromAddress, string ToAddress, string Subject, string MessageText, string[] AttachedFiles, string Bcc)
        {
            var oMailMsg = new MimeMessage();
            string sIDInfo = "";

            try
            {
                sIDInfo = "<p style=\"color:white\"><BR /><BR /><BR /><BR />tid =" + System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
                sIDInfo = sIDInfo + "<BR />mid=" + Environment.MachineName + "</p>";
            }
            catch (Exception ex)
            {
                LogError(ex);
            }

            oMailMsg.From.Add(new MailboxAddress("", FromAddress));

            string[] sTos = ToAddress.Split([';', ',']);
            foreach (string sTo in sTos)
            {
                if (!string.IsNullOrEmpty(sTo))
                {
                    oMailMsg.To.Add(new MailboxAddress("", sTo.Trim()));
                }
            }

            if (!string.IsNullOrEmpty(Bcc))
            {
                string[] sBccs = Bcc.Split([';', ',']);
                foreach (string sB in sBccs)
                {
                    if (!string.IsNullOrEmpty(sB))
                    {
                        oMailMsg.Bcc.Add(new MailboxAddress("", sB.Trim()));
                    }
                }
            }

            oMailMsg.Subject = Subject;

            var bodyBuilder = new BodyBuilder();
            MessageText = MessageText + sIDInfo;
            bodyBuilder.HtmlBody = MessageText;

            foreach (string sFile in AttachedFiles)
            {
                if (!string.IsNullOrEmpty(sFile))
                {
                    try
                    {
                        bodyBuilder.Attachments.Add(sFile);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex);
                    }
                }
            }
            oMailMsg.Body = bodyBuilder.ToMessageBody();
            SendMail(oMailMsg);
        }
       
        public static string ParseError(List<ErrorInfo> errors) =>
        string.Join(Environment.NewLine, errors.Select(e => e.errorDesc));

        public static void AddErrorEventLog(string message) =>
        Logger.LogMessage(message, Logger.LoggerType.BendProcessor, Logger.LogInfoType.ErrorFormat);

        public static void SendErrorEmail(string subject, string body, bool includeUser)
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
