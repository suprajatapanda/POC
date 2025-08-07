using System.Data;
using SIUtil;
using TRS.IT.SI.BusinessFacadeLayer.Model;
using TRS.SqlHelper;

namespace TRS.IT.SI.BusinessFacadeLayer.DAL
{
    public class FundMappingsDC
    {
        public static bool UpdatePartnerFundMappings(PartnerFlag partnerID, ref FundInfo[] oFundInfo, ref string sMsg, string sContractID = "", string sSubID = "")
        {
            // prepare funds xml
            DataView dv;
            int Index;
            DataRow dr;
            DataSet ds;
            int iCnt = 0;

            try
            {
                if (partnerID == PartnerFlag.ISC)
                {
                    ds = TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_FundMappings", [partnerID, TRSManagers.XMLManager.GetXML(oFundInfo)]);
                }
                else
                {
                    ds = TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_FundMappings", [partnerID, GetPartnerFundsXml(oFundInfo)]);
                }

                dv = ds.Tables[0].DefaultView;
                dv.Sort = "partner_fund_id";

                foreach (var oFund in oFundInfo)
                {
                    if (partnerID == PartnerFlag.ISC)
                    {
                        Index = dv.Find(oFund.AssetName);
                    }
                    else
                    {
                        Index = dv.Find(oFund.PartnerFundID);
                    }

                    if (Index != -1)
                    {
                        dr = dv[Index].Row;
                        oFund.AssetID = Convert.ToString(dr["asset_id"]);
                        oFund.AssetName = Convert.ToString(dr["asset_name"]).Trim();
                        oFund.FundName = Convert.ToString(dr["fund_name"]).Trim();
                        oFund.FundID = Convert.ToString(dr["fund_id"]);
                        switch (partnerID)
                        {
                            case PartnerFlag.Penco:
                            {
                                iCnt += 1;
                                oFund.FundSequenceNumber = iCnt.ToString();
                                break;
                            }
                            case PartnerFlag.ISC:
                            {
                                oFund.PartnerFundID = Convert.ToString(dr["partner_fund_id"]).Trim();
                                if (!(General.ValidateDBNull(dr["FundSequenceNumber"]) == null))
                                {
                                    oFund.FundSequenceNumber = Convert.ToString(dr["FundSequenceNumber"]).Trim();
                                }

                                break;
                            }
                        }
                    }

                    else
                    {
                        oFund.PartnerFundID = oFund.AssetName;
                        oFund.AssetName = "Outside Assets";
                        oFund.FundSequenceNumber = oFund.FundID;
                        oFund.FundID = 1000.ToString();
                        oFund.DisplayOnly = false;
                        oFund.FundType = FundType.PS58InsuranceFund;
                        oFund.MaxContribution = 0d;
                        oFund.TransfersInAllowed = false;
                        oFund.TransfersOutAllowed = false;
                    }
                }
                ds.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                sMsg = "Error in UpdatePartnerAccountMappings: " + ex.Message;
                return false;
            }

        }

        public static bool UpdatePartnerAccountMappings(PartnerFlag partnerID, ref AccountInfo[] oAccountInfo, ref string sMsg)
        {
            // prepare funds xml
            DataView dv;
            int Index;
            DataRow dr;
            DataSet ds;

            try
            {
                ds = TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_FundMappings", [partnerID, GetPartnerFundsXml(oAccountInfo)]);

                dv = ds.Tables[0].DefaultView;

                dv.Sort = "partner_fund_id";

                AccountInfo oAccount;
                foreach (var currentOAccount in oAccountInfo)
                {
                    oAccount = currentOAccount;
                    Index = dv.Find(oAccount.PartnerFundID);
                    if (Index != -1)
                    {
                        dr = dv[Index].Row;
                        oAccount.FundName = Convert.ToString(dr["fund_name"]).Trim();
                        oAccount.FundID = Convert.ToString(dr["fund_id"]);
                    }
                    else if (Convert.ToDouble(oAccount.PartnerFundID) == 0d)
                    {
                        oAccount.FundName = "All Funds";
                        oAccount.FundID = 0.ToString();
                    }
                }
                ds.Dispose();

                // get account mappings
                ds = TRSSqlHelper.ExecuteDataset(General.ConnectionString, "pSI_AccountMappings", [partnerID, GetPartnerAccountsXml(oAccountInfo)]);
                dv = ds.Tables[0].DefaultView;
                dv.Sort = "partner_acc_id";
                foreach (var currentOAccount1 in oAccountInfo)
                {
                    oAccount = currentOAccount1;
                    Index = dv.Find(oAccount.PartnerAccountID);
                    if (Index != -1)
                    {
                        dr = dv[Index].Row;
                        oAccount.AccName = Convert.ToString(dr["acc_name"]).Trim();
                        oAccount.AccID = Convert.ToString(dr["acc_id"]);
                    }
                    else if (Convert.ToDouble(oAccount.PartnerAccountID) == 0d)
                    {
                        oAccount.AccName = "Total Account Balance";
                        oAccount.AccID = 0.ToString();
                    }
                }
                ds.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                sMsg = "Error in UpdatePartnerAccountMappings: " + ex.Message;
                return false;
            }

        }

        private static string GetPartnerFundsXml(FundInfo[] oFundInfo)
        {
            var sbXml = new System.Text.StringBuilder();
            sbXml.Append("<Funds>" + Environment.NewLine);
            if (oFundInfo != null)
            {
                foreach (var oFund in oFundInfo)
                {
                    sbXml.Append("<Fund>" + oFund.PartnerFundID + "</Fund>" + Environment.NewLine);
                }
            }

            sbXml.Append("</Funds>");
            return sbXml.ToString();
        }

        private static string GetPartnerFundsXml(AccountInfo[] oAccountInfo)
        {
            var sbXml = new System.Text.StringBuilder();
            sbXml.Append("<Funds>" + Environment.NewLine);
            if (oAccountInfo != null)
            {
                foreach (var oAccount in oAccountInfo)
                {
                    sbXml.Append("<Fund>" + oAccount.PartnerFundID + "</Fund>" + Environment.NewLine);
                }
            }
            sbXml.Append("</Funds>");
            return sbXml.ToString();
        }

        private static string GetPartnerAccountsXml(AccountInfo[] oAccountInfo)
        {
            var sbXml = new System.Text.StringBuilder();
            sbXml.Append("<Accounts>" + Environment.NewLine);
            foreach (var oAccount in oAccountInfo)
            {
                sbXml.Append("<Account>" + oAccount.PartnerAccountID + "</Account>" + Environment.NewLine);
            }

            sbXml.Append("</Accounts>");
            return sbXml.ToString();
        }
    }
}