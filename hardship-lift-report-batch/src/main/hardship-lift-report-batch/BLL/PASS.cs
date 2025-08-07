using System.Data;
using TRS.IT.BendProcessor.DAL;
using TRS.IT.BendProcessor.DriverSOA;
using TRS.IT.BendProcessor.Model;
using TRS.IT.BendProcessor.Util;
using TRS.IT.TrsAppSettings;
using TaskStatus = TRS.IT.BendProcessor.Model.TaskStatus;
namespace HardshipLiftReport.BLL
{
    public class PASS : BendProcessorBase
    {
        private DAL.PASSDC _oPassDC;
        public PASS() : base("100", "PayStart", "TRS") { _oPassDC = new DAL.PASSDC(); }
        public TaskStatus ProcessSendHardShipLiftRpt()
        {
            var oTaskReturn = new TaskStatus();            
            var dtToday = DateTime.Now.AddMonths(1);
            //always start for the following month
            var dtReportStart = new DateTime(dtToday.Year, dtToday.Month, 1);
            var dtReportEnd= dtReportStart.AddMonths(1).AddDays(-1);
           
            try
            {
                InitTaskStatus(oTaskReturn, "ProcessUpdatePartner"); 

                //step1. Web service call/stored procedure to get list if participants (ssn)                
                var hardshipData = _oPassDC.GetHardshipLiftData(dtReportStart, dtReportEnd);

               if (hardshipData .Tables.Count > 0)
                {
                    hardshipData .DataSetName = "HardShipDistributions";
                    //step2.	Lookup Participant address info from web_main
                    DataSet participantInfo = _oPassDC.GetHDParticipantInfo(hardshipData .Tables[0]);

                    if (participantInfo.Tables.Count > 0)
                    {                        
                        //step3.	Generate csv raw file.                        

                        var oReturn = CreateCsvReport(participantInfo.Tables[1]);
                        if (oReturn.returnStatus == ReturnStatusEnum.Succeeded)
                        {
                            //step4.Send to static email distribution with csv raw file as attachment.
                            var sSubject = "Hardship Suspension Lift Report for - " + dtReportStart.ToString("MM-yyyy");
                            var sBody = "Data for Hardship Suspension Restriction Lift is attached.";
                            SendToPASSInbox(Path.GetFileName(oReturn.confirmationNo), oReturn.confirmationNo, sSubject, sBody);
                            oTaskReturn.retStatus = TaskRetStatus.Succeeded;
                            oTaskReturn.partnerId = oReturn.confirmationNo;
                        }
                        else
                        {
                            oTaskReturn.retStatus = TaskRetStatus.Failed;
                            General.CopyResultError(oTaskReturn, oReturn);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                InitTaskError(oTaskReturn, ex, true);
            }
            oTaskReturn.endTime = DateTime.Now;
            return oTaskReturn;
        }
        private ResultReturn SendToPASSInbox(string a_sPromptName, string a_sFilePathName, string a_sSubject, string a_sBody)
        {
            var oReturn= new ResultReturn();
            var oGenDC = new GeneralDC();
            
            var oWebMsgAdaptor = new MsgCenterMessageService();           
            int iBendInLoginId = oGenDC.GetMsgCtrAcctByExLoginId("BendProcess");
            int iPASSInbox = oGenDC.GetMsgCtrAcctByExLoginId("NBI-PASS");
            string sLoginIdXML = "<ArrayOfInLoginId> <InLoginId>" + iPASSInbox.ToString() + "</InLoginId></ArrayOfInLoginId>";
            byte[] RawData = File.ReadAllBytes(a_sFilePathName);

            var attachment = new TRS.IT.SI.Services.wsMessage.Attachment
            {
                Data = Convert.ToBase64String(RawData),
                PromptFileName = a_sPromptName
            };

            var oWebMsgData = new TRS.IT.SI.Services.wsMessage.MsgData
            {
                Attachments = new[] { attachment },
                ReplyAllowed = false,
                Body = a_sBody
            };
            var oWebMsgCtr = new TRS.IT.SI.Services.wsMessage.webMessage
            {
                Subject = a_sSubject,
                AttachmentCount = 1,
                MsgData = oWebMsgData,
                FromAddress = "BendProcess",
                MsgSource = "System Back-end",
                CreateBy = iBendInLoginId.ToString(),
                CreateDt = DateTime.Now.ToString(),
                ExpireDt = DateTime.Now.AddDays(90).ToString(),
                SendNotification = "N",
                FolderId = 1, // Inbox
                MsgType = "0",
                SenderInLoginId = iBendInLoginId.ToString()
            };

            oReturn = oWebMsgAdaptor.SendMessageCenterMessage(sLoginIdXML, oWebMsgCtr);

            return oReturn;

        }
        private ResultReturn CreateCsvReport(DataTable dt)
        {
            var oReturn = new ResultReturn();
            //put filename in confirmationNO
            oReturn.confirmationNo = AppSettings.GetValue("HardShipReportFolder") + "HardshipLiftReport_" + DateTime.Now.ToString("MM-yyyy") + ".csv";
                        
            try
            {
                using var sw = new StreamWriter(oReturn.confirmationNo, false);
                // Step 1: Write headers
                var columnNames = dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName);
                sw.WriteLine(string.Join(",", columnNames));

                // Step 2: Write rows
                foreach (DataRow row in dt.Rows)
                {
                    var fields = row.ItemArray.Select(field =>
                        Convert.IsDBNull(field) ? string.Empty : Utils.PayrollCheckNull(field.ToString()));
                    sw.WriteLine(string.Join(",", fields));
                }

                oReturn.returnStatus = ReturnStatusEnum.Succeeded;
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                oReturn.Errors.Add(new TRS.IT.BendProcessor.Model.ErrorInfo(-1, ex.Message, ErrorSeverityEnum.ExceptionRaised));
                oReturn.returnStatus = ReturnStatusEnum.Failed;

            }

            return oReturn;

        }
    }
}
