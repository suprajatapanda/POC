using System.Data;
using System.Xml.Linq;
using System.Xml.XPath;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;
namespace FWFundUpdatesToISCBatch.BLL
{
    public class FundWizard
    {
        TARSharedUtilLibBFLBLL.FundWizard oFW;
        public FundWizard(TARSharedUtilLibBFLBLL.FundWizard obj)
        {
            oFW = obj;
        }
        public string GetInputXml_GetPxWithFunds(bool bCustomPX = false)
        {
            var xEl = new XElement("Funds");
            string sXML = xEl.ToString();
            var tbPx21 = oFW.TblDefPx21();
            DataRow drFL;

            var tbFundList = oFW.GetFundList(false);
            var tbActiveFunds = oFW.GetActiveFunds(false, false);

            bool bFoundFund = false;
            string[] nonPXStyleCodes = ["37", "28", "29", "16", "38", "40", "41", "42", "43"];
            DataRow[] drTemp;
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

                if (TARSharedUtilLibBFLBLL.FWUtils.GetHdrData(TARSharedUtilLibBFLBLL.FWUtils.C_hdr_PortXpress_custom, oFW.PdfHeader)[0] == "true")
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

            sXML = xEl.ToString();

            return sXML;
        }
        public void GetDefaultForfeitureFunds(ref string DefaultFundID, ref string ForfFundID, ref string QDIA, ref string FundSeries, ref bool PX, ref string PartnerDefaultFundID, ref string PartnerForfFundID, ref string FidName)
        {
            var a_xDoc = GetFWxDoc(true);
            string sTemp;
            string FundName = "";

            DefaultFundID = "";
            ForfFundID = "";

            var xEl = a_xDoc.XPathSelectElement("/FMRS/Contract");

            DefaultFundID = oFW.CheckNull(xEl.Attribute("DefaultFundID"));
            ForfFundID = oFW.CheckNull(xEl.Attribute("ForfeitureFundID"));



            if (!string.IsNullOrEmpty(DefaultFundID))
            {
                oFW.GetPartnerFundID(DefaultFundID, ref FundName, ref PartnerDefaultFundID);
            }
            if (!string.IsNullOrEmpty(ForfFundID))
            {
                oFW.GetPartnerFundID(ForfFundID, ref FundName, ref PartnerForfFundID);
            }

            // QDIASelect
            QDIA = "false";
            sTemp = oFW.CheckNull(xEl.Attribute("QDIASelect"));
            if (!string.IsNullOrEmpty(sTemp))
            {
                QDIA = Convert.ToString(sTemp == "0001" ? "true" : "false");
            }
            // TMF
            sTemp = oFW.CheckNull(xEl.Attribute("TMFSelect"));
            FundSeries = "false";
            if (!string.IsNullOrEmpty(sTemp))
            {
                FundSeries = Convert.ToString(sTemp == "0001" ? "true" : "false");
            }

            // Portfolio Express settings

            PX = false;
            // PortfolioXpress Default
            if (xEl.HasElements)
            {
                var xElPX = a_xDoc.XPathSelectElement("/FMRS/Contract/PortfolioExpress");
                if (!(xElPX == null))
                {
                    sTemp = oFW.CheckNull(xElPX.Attribute("Selected"));
                    if (!string.IsNullOrEmpty(sTemp))
                    {
                        PX = true;
                        sTemp = oFW.CheckNull(xElPX.Attribute("FiduciaryName"));
                        FidName = !string.IsNullOrEmpty(sTemp) ? sTemp : "Plan Sponsor";
                    }

                    sTemp = oFW.CheckNull(xElPX.Attribute("DefaultFund"));
                    if (!string.IsNullOrEmpty(sTemp))
                    {
                        if (sTemp == "true")
                        {
                            DefaultFundID = "PX";
                        }
                    }

                    sTemp = oFW.CheckNull(xElPX.Attribute("QDIA"));
                    if (!string.IsNullOrEmpty(sTemp))
                    {
                        QDIA = Convert.ToString(sTemp == "true" ? "true" : "false");
                    }
                }
            }
        }
        public XDocument GetFWxDoc(bool bActiveFunds = false, bool bRestrictions = false)
        {
            if (oFW._xDoc == null)
            {
                oFW.LoadFMRS(bActiveFunds, bRestrictions);
            }
            return oFW._xDoc;
        }
    }
}
