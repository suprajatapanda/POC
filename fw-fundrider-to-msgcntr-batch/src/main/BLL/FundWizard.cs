using System.Text;
using System.Xml.Linq;
using SIUtil;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
using TRS.IT.SI.BusinessFacadeLayer;


namespace FWFundRiderToMsgcntrBatch.BLL
{
    public class FundWizard
    {
        TARSharedUtilLibBFLBLL.FundWizard fundWizard;
        public FundWizard(TARSharedUtilLibBFLBLL.FundWizard obj)
        {
            fundWizard = obj;
        }

        public TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse SendFundRiderToMC(string promptFileName, string filePath)
        {
            var response = new TRS.IT.SI.BusinessFacadeLayer.Model.SIResponse();
            var webMessage = new TRS.IT.SI.Services.wsMessage.webMessage();
            var messageData = new TRS.IT.SI.Services.wsMessage.MsgData();
            var designatedContacts = new List<TRS.IT.SOA.Model.PlanContactInfo>();

            try
            {
                
                designatedContacts = fundWizard.GetDesignatedContacts();

                if (designatedContacts.Count == 0)
                    throw new Exception("Contract is missing primary contact");

                int bendInLoginId = (int)WebMessageCenter.GetMsgCntrCMSAcctById("BendProcess");

                var loginIdXmlBuilder = new StringBuilder();
                var loginIdList = new List<string>();
                var emailList = new List<string>();

                foreach (var contact in designatedContacts)
                {
                    if (!string.IsNullOrWhiteSpace(contact.WebInLoginID))
                    {
                        loginIdXmlBuilder.Append($"<InLoginId>{contact.WebInLoginID}</InLoginId>");
                        loginIdList.Add(contact.WebInLoginID);
                    }

                    if (!string.IsNullOrWhiteSpace(contact.Email))
                    {
                        emailList.Add(contact.Email);
                    }
                }

                if (loginIdList.Count == 0)
                    throw new Exception("No Web login contact is available");

                string loginIdXml = $"<ArrayOfInLoginId>{loginIdXmlBuilder}</ArrayOfInLoginId>";
                string loginIdCsv = string.Join(";", loginIdList);
                string emailsCsv = string.Join(";", emailList);

                // Prepare attachment
                var attachment = new TRS.IT.SI.Services.wsMessage.Attachment
                {
                    Data = Convert.ToBase64String(File.ReadAllBytes(filePath)),
                    PromptFileName = promptFileName
                };

                messageData.Attachments = new[] { attachment };
                messageData.ReplyAllowed = false;
                messageData.Body = "<br/><br/>Congratulations! Your investment choice change is now complete.<br/><br/>" +
                                   "As part of this investment choice change, attached is an updated investment choice rider. " +
                                   "The updated document is part of your plan contract. Please retain it with your plan documents.<br/><br/><br/>" +
                                   "Sincerely,<br/>Transamerica Retirement Solutions";

                webMessage.MsgSource = "FW-Backend";
                webMessage.MsgData = messageData;
                webMessage.Subject = "Investment Choice Change Documentation";
                webMessage.CreateBy = bendInLoginId.ToString();
                webMessage.CreateDt = DateTime.Now.ToString();
                webMessage.ExpireDt = DateTime.Now.AddDays(90).ToString();
                webMessage.SendNotification = "N";
                webMessage.FolderId = (int)TRS.IT.SOA.Model.webMsgGlobalFolderEnum.Inbox;
                webMessage.MsgType = "0";
                webMessage.FromAddress = "Transamerica Retirement Solutions";
                webMessage.SenderInLoginId = bendInLoginId.ToString();
                webMessage.AttachmentCount = 1;

                // Send web message
                var webMessageCenter = new WebMessageCenter();
                var sendResponse = webMessageCenter.SendWebMessages(loginIdXml, webMessage, "", "");

                // Send email notification
                var emailData = new TRS.IT.SI.BusinessFacadeLayer.Model.MessageData
                {
                    MessageID = 1820,
                    ContractID = fundWizard.ContractId,
                    SubID = fundWizard.SubId,
                    EImageOption = TRS.IT.SI.BusinessFacadeLayer.Model.E_ImageOption.None
                };
                emailData.EmailVariableContainer.Add("fw_designated_contacts", emailsCsv);

                var emailResponse = MessageService.MessageServiceSendEmail(emailData);
                string emailError = emailResponse.Errors[0].Number != 0 ? emailResponse.Errors[0].Description : string.Empty;

                if (sendResponse.Errors[0].Number == 0)
                {
                    response.Errors[0].Number = 0;
                    InsertTaskFundRiderToMC(loginIdCsv, emailsCsv, emailError);
                }
                else
                {
                    response.Errors[0].Number = -1;
                    response.Errors[0].Description = sendResponse.Errors[0].Description;
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                response.Errors[0].Number = -1;
                response.Errors[0].Description = $"FileNameNPath: {filePath} PromptFileName: {promptFileName} ex: {ex.Message}";
            }

            return response;
        }

        public int InsertTaskFundRiderToMC(string inLoginIds, string toEmail, string error)
        {
            var taskXml = new XElement("MessageCenter",
                new XAttribute("InloginId", inLoginIds),
                new XAttribute("ToEmail", toEmail),
                new XElement("Error", error ?? string.Empty)
            );


            return fundWizard.InsertTask(
                TRS.IT.SI.BusinessFacadeLayer.Model.FundWizardInfo.FwTaskTypeEnum.FundRiderSentToMC,
                100,
                new[] { taskXml }
            );
        }


    }

}
