using System.Data;
using DailyVendorNotificationBatch.DAL;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace DailyVendorNotificationBatch.BLL
{
    public class PayStart : BendProcessorBase
    {
        private PayStartDC _oPDC = new();
        public PayStart() : base("100", "PayStart", "TRS") { }

        public TaskStatus SendVendorNotification()
        {
            TaskStatus oTaskStatus = new();
            DateTime dtEnd = DateTime.Now;
            DateTime dtStart = dtEnd.AddDays(-1);
            DataSet ds = new();
            StreamWriter sw = null;
            string sPath;
            string sFileName;
            DataSet dsVendor = new DataSet();
            try
            {
                sPath = AppSettings.GetValue("PayrollVendorNotificationFolder");
                dsVendor = _oPDC.GetPayStartVenders();
                if (dsVendor.Tables[0].Rows.Count>0)
                {
                    foreach (DataRow  dr in dsVendor.Tables[0].Rows)
                    {
                        ds = _oPDC.GetDailyJobActivitySummary(dr["VendorName"].ToString(), dtStart, dtEnd);
                        if (ds.Tables[0].Rows.Count + ds.Tables[1].Rows.Count + ds.Tables[2].Rows.Count + ds.Tables[3].Rows.Count > 0)
                        {
                            sFileName = sPath + "PYSum_" + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + "_" + dr["VendorName"].ToString() + ".xml";
                            sw = new StreamWriter(sFileName);
                            sw.WriteLine("<DailyPayrollActiviySummary FileCreateDt=\"" + DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss") + "\""
                                + " FromDt=\"" + dtStart.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss") + "\" "
                                + " ToDt=\"" + dtEnd.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss") + "\">");
                            ds.DataSetName = "NewFiles";
                            ds.Tables[0].TableName = "NewFile";
                            ds.Tables[0].WriteXml(sw);
                            sw.WriteLine();

                            ds.DataSetName = "ProcessedFiles";
                            ds.Tables[1].TableName = "ProcessedFile";
                            ds.Tables[1].WriteXml(sw);
                            sw.WriteLine();
                            ds.DataSetName = "ErrorRejectFiles";
                            ds.Tables[2].TableName = "ErrorRejectFile";
                            ds.Tables[2].WriteXml(sw);
                            sw.WriteLine();
                            ds.DataSetName = "DeletedFiles";
                            ds.Tables[3].TableName = "DeletedFile";
                            ds.Tables[3].WriteXml(sw);
                            sw.WriteLine();
                            sw.WriteLine("</DailyPayrollActiviySummary>");
                            sw.Flush();
                            sw.Close();

                            Utils.SendMail(AppSettings.GetValue(ConstN.C_BPROCESSOR_OUTSIDE_FROM_EMAIL), dr["VendorEmail"].ToString(), "Daily Payroll Activity Summary", "Data attached.", [sFileName], _sBCCEmailNotification);
                        }
                    }
                }
                oTaskStatus.retStatus = TaskRetStatus.Succeeded;
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oTaskStatus.errors.Add(new ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
                oTaskStatus.retStatus = TaskRetStatus.Failed;
                SendErrorEmail(ex);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }
            return oTaskStatus;
        }
    }
}
