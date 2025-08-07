using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer;
using TRS.IT.SI.BusinessFacadeLayer.SOA;
using MO = TRS.IT.SI.BusinessFacadeLayer.Model;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;

namespace FWSignedDocsToMsgcntrBatch.BLL
{
    public class FundWizard
    {
        TARSharedUtilLibBFLBLL.FundWizard oFW;
        private DAL.ContractDC _ContractDC;
        public FundWizard(TARSharedUtilLibBFLBLL.FundWizard obj)
        {
            oFW = obj;
            _ContractDC = new DAL.ContractDC(obj._sSessionId, obj.ContractId, obj.SubId);
        }
        public List<TRS.IT.SOA.Model.FundPendingChanges> GetPendingFundChangeByContract()
        {
            return _ContractDC.FwGetPendingFundChangeByContract();
        }
        public MO.SIResponse SendSponsorPPTLetterToMC(string a_sPromptFileName, string a_sFilePathNName)
        {
            var oReturn = new MO.SIResponse();

            var oWebMessage = new TRS.IT.SI.Services.wsMessage.webMessage();
            var oWebMessageData = new TRS.IT.SI.Services.wsMessage.MsgData();

            var oDesignatedContacts = new List<TRS.IT.SOA.Model.PlanContactInfo>();
            int iBendInLoginId;
            string sError = string.Empty;
            string sLoginIdXML = string.Empty;
            string sEmails = string.Empty;
            string sInLoginIdList = string.Empty;

            try
            {
                oDesignatedContacts = oFW.GetDesignatedContacts();

                if (oDesignatedContacts.Count == 0)
                {
                    throw new Exception("Contract is missing primary contact");
                }

                iBendInLoginId = (int)WebMessageCenter.GetMsgCntrCMSAcctById("BendProcess");

                foreach (TRS.IT.SOA.Model.PlanContactInfo oContact in oDesignatedContacts)
                {
                    if (!string.IsNullOrEmpty(oContact.WebInLoginID))
                    {
                        sLoginIdXML += "<InLoginId>" + oContact.WebInLoginID + "</InLoginId>";
                        sInLoginIdList = sInLoginIdList + (string.IsNullOrEmpty(sInLoginIdList) ? oContact.WebInLoginID : ";" + oContact.WebInLoginID);
                    }
                    if (!string.IsNullOrEmpty(oContact.Email))
                    {
                        sEmails = sEmails + (string.IsNullOrEmpty(sEmails) ? oContact.Email : ";" + oContact.Email);
                    }
                }
                if (string.IsNullOrEmpty(sLoginIdXML))
                {
                    throw new Exception("No Web login contact is available");
                }
                // Contacts
                sLoginIdXML = "<ArrayOfInLoginId>" + sLoginIdXML + "</ArrayOfInLoginId>";

                // upload files
                var oAttachment = new TRS.IT.SI.Services.wsMessage.Attachment[1];
                oAttachment[0] = new TRS.IT.SI.Services.wsMessage.Attachment();
                byte[] byIn = File.ReadAllBytes(a_sFilePathNName);
                oAttachment[0].Data = Convert.ToBase64String(byIn);
                oAttachment[0].PromptFileName = a_sPromptFileName;

                oWebMessageData.Attachments = oAttachment;

                // set reply and message attributes
                oWebMessage.MsgSource = "FW-Backend";
                oWebMessageData.ReplyAllowed = false;

                oWebMessageData.Body = "Documentation regarding your recent investment choice change is now available.  The following notice(s) can be found on the \"Add & Delete Investment Choices - Pending Change Status\" screen:" + "<br /><br />" + "<UL>" + "<LI>Notice to Participants - Change of Investment Choices – This memo can be transferred to your company letterhead and distributed to your plan participants in advance of the change. " + "Please note that effective August 30, 2012, the Department of Labor regulations under ERISA section 404(a) generally require plan sponsors to provide eligible employees with at least thirty (30) days, but no more than ninety (90) days advance notice of changes that affect the fees charged under the plan.<br /></LI>" + "</UL>" + "Please note that it is the plan sponsor’s responsibility to communicate investment choice changes to participants.  It is important that you provide the appropriate participant notices communicating the investment choice change to participants promptly.<br />" + "<br />Sincerely,<br/>Transamerica Retirement Solutions<br /><br />";

                oWebMessage.MsgData = oWebMessageData;
                oWebMessage.Subject = "Investment Choice Change Documentation";
                oWebMessage.CreateBy = iBendInLoginId.ToString();
                oWebMessage.CreateDt = Convert.ToString(DateTime.Now);
                oWebMessage.ExpireDt = Convert.ToString(DateTime.Now.AddDays(90d)); // expiration
                oWebMessage.SendNotification = "N";
                oWebMessage.FolderId = (int)TRS.IT.SOA.Model.webMsgGlobalFolderEnum.Inbox;
                oWebMessage.MsgType = 0.ToString();

                oWebMessage.FromAddress = "Transamerica Retirement Solutions";

                oWebMessage.SenderInLoginId = iBendInLoginId.ToString();
                oWebMessage.AttachmentCount = 1;

                // Send Web Message with AttachmentCount
                var oWebMessageCenter = new WebMessageCenter();
                var oResponse = oWebMessageCenter.SendWebMessages(sLoginIdXML, oWebMessage, "", "");

                // 'Send Email
                var objMessageData = new MO.MessageData();
                var objMessageService = new MessageService();
                objMessageData.MessageID = 2070; // 1770 old
                objMessageData.ContractID = oFW.ContractId;
                objMessageData.SubID = oFW.SubId;
                objMessageData.EmailVariableContainer.Add("fw_designated_contacts", sEmails);
                objMessageData.EImageOption = MO.E_ImageOption.None;
                var oNotSerResponse = MessageService.MessageServiceSendEmail(objMessageData);
                if (oNotSerResponse.Errors[0].Number != 0)
                {
                    sError = oNotSerResponse.Errors[0].Description;
                }

                // Don't care much about Message Service notice succeeded or failed
                if (oResponse.Errors[0].Number == 0)
                {
                    oReturn.Errors[0].Number = 0;
                    InsertTaskSponsorPPTLetterToMC(a_sFilePathNName, sInLoginIdList, sEmails, sError);
                }
                else
                {
                    oReturn.Errors[0].Number = -1;
                    oReturn.Errors[0].Description = oResponse.Errors[0].Description;
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                oReturn.Errors[0].Number = -1;
                oReturn.Errors[0].Description = "FileNameNPath: " + a_sFilePathNName + " PromptFileName: " + a_sPromptFileName + " ex: " + ex.Message;
            }
            return oReturn;
        }
        public int InsertTaskSponsorPPTLetterToMC(string a_sFileNPath, string a_iInLoginIds, string a_sToEmail, string a_sError)
        {
            var xEmail = new XElement("MessageCenter", new XAttribute("InloginId", a_iInLoginIds), new XAttribute("ToEmail", a_sToEmail), new XElement("Error", a_sError));
            var xEl = oFW.GetFileProfile(Path.GetDirectoryName(a_sFileNPath) + @"\", Path.GetFileName(a_sFileNPath), "PPTNotice", false);
            return oFW.InsertTask(MO.FundWizardInfo.FwTaskTypeEnum.SponsorPptLetterSentToMC, 100, [xEmail, xEl]);
        }
        public DataTable GeneratePX21(bool bCustomPX = false)
        {

            var tbPx21 = oFW.TblDefPx21();
            DataRow drFL;
            var tbFundList = oFW.GetFundList(false);
            var tbActiveFunds = oFW.GetActiveFunds(false, false);
            var xEl = new XElement("Funds");
            string sPXxml;
            bool bFoundFund = false;
            string[] nonPXStyleCodes = ["37", "28", "29", "16", "38", "40", "41", "42", "43"];
            DataRow[] drTemp;
            // build input fundlist from active funds
            if (bCustomPX)
            {
                foreach (DataRow oRow in oFW._tbFundPendingCustomPX.Select(" action = 2 "))
                {
                    if (tbFundList.Select("fund_id = " + oRow["fund_id"].ToString()).Length > 0)
                    {
                        tbFundList.Rows.Remove(tbFundList.Select("fund_id = " + oRow["fund_id"].ToString())[0]);
                        tbFundList.AcceptChanges();
                    }
                }
                foreach (DataRow oRow in oFW._tbFundPendingCustomPX.Select(" action = 1 "))
                {
                    DataRow[] oFundRow = tbFundList.Select("fund_id = " + oRow["fund_id"].ToString());
                    if (oFundRow.Length > 0)
                    {
                        oFundRow[0]["px_custom_status"] = 1;
                        tbFundList.AcceptChanges();
                    }
                }
                drTemp = tbFundList.Select(" px_custom_status = 1 ");
            }
            else
            {
                drTemp = tbActiveFunds.Select();
            }
            foreach (DataRow drI in drTemp)
            {
                bFoundFund = false;
                drFL = tbFundList.Rows.Find(drI["fund_id"]);
                if (nonPXStyleCodes.Contains(Convert.ToString(drFL["px_style_code"])) == true) // ignore non px stylecode funds
                {
                    continue;
                }

                if (FWUtils.GetHdrData(FWUtils.C_hdr_PortXpress_custom, oFW.PdfHeader)[0] == "true")
                {
                    if (!(drFL == null) && Convert.ToInt32(drFL["fund_id"]) > 0 && !string.IsNullOrEmpty(drFL["px_style_code"].ToString()))
                    {
                        bFoundFund = true;
                    }
                }
                else if (!(drFL == null) && Convert.ToInt32(drFL["fund_id"]) > 0 && !string.IsNullOrEmpty(drFL["px_style_code"].ToString()))
                {
                    bFoundFund = true;
                }
                if (bFoundFund)
                {
                    xEl.Add(new XElement("FundInfo", new XElement("FundID", drFL["fund_id"].ToString()), new XElement("FundName", drFL["fund_name"].ToString()), new XElement("FundStyleCode", drFL["px_style_code"].ToString()), new XElement("FundDescriptor", drFL["FundDescriptor"].ToString())));
                }
            }

            sPXxml = new PXEngine().GetPxWithFunds(oFW.ContractId, oFW.SubId, Convert.ToInt32(FWUtils.GetHdrData(FWUtils.C_hdr_PortXpress_glidepath, oFW.PdfHeader)[0]), xEl.ToString());

            var xElPx21 = XElement.Load(new StringReader(sPXxml.Replace("xsi:nil=\"true\"", "")));

            var xElCur = xElPx21.XPathSelectElement("/Versions/VersionInfo/EndDt[not(text())]");


            if (!(xElCur == null))
            {
                if (xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").Count() == 21)
                {
                    // all portfolios must have the same # of funds
                    DataRow drF;
                    XElement xElFunds;
                    for (int iJ = 0, loopTo = xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").ElementAtOrDefault(0).Element("Funds").Elements("FundInfo").Count() - 1; iJ <= loopTo; iJ++)
                    {
                        xElFunds = xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").ElementAtOrDefault(0).Element("Funds").Elements("FundInfo").ElementAtOrDefault(iJ);
                        drF = tbPx21.NewRow();
                        drF["fund_id"] = xElFunds.Element("FundID").Value;
                        drF["px_style_code"] = xElFunds.Element("FundStyleCode").Value;

                        for (int iK = 0, loopTo1 = xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").Count() - 1; iK <= loopTo1; iK++)
                        {
                            drF["p" + iK] = xElCur.Parent.Element("Porfolios").Elements("PortfolioInfo").ElementAtOrDefault(iK).Element("Funds").Elements("FundInfo").ElementAtOrDefault(iJ).Element("Percentage").Value;
                        }

                        drFL = tbFundList.Rows.Find(drF["fund_id"]);

                        if (!(drFL == null))
                        {
                            drF["fund_name"] = drFL["fund_name"];
                            drF["fund_order_id"] = drFL["fund_order_id"];
                            drF["asset_id"] = drFL["asset_id"];
                            drF["asset_name"] = drFL["asset_name"];
                            drF["asset_order_id"] = drFL["asset_order_id"];
                            drF["sub_asset_id"] = drFL["sub_asset_id"];
                            drF["sub_asset_name"] = drFL["sub_asset_name"];
                            drF["sub_asset_order_id"] = drFL["sub_asset_order_id"];
                            drF["asset_group"] = Px21AssetGroup(Convert.ToInt32(xElFunds.Element("DIA_AssetClassID").Value));
                        }

                        tbPx21.Rows.Add(drF);
                    }
                }
            }
            tbPx21.DefaultView.Sort = "asset_group, asset_order_id, sub_asset_order_id,fund_order_id ASC";
            return tbPx21.DefaultView.ToTable();
        }
        private string Px21AssetGroup(int a_iAssetId)
        {
            switch (a_iAssetId)
            {
                case 40:
                case 41:
                case 42:
                    {
                        return "Bonds/Cash";
                    }

                default:
                    {
                        return "Stocks";
                    }
            }
        }
    }
}
