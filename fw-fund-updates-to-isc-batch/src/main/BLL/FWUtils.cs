using System.Data;
using BFLModel = TRS.IT.SI.BusinessFacadeLayer.Model;
using System.Xml.Linq;
using SIUtil;
using FWFundUpdatesToISCBatch.DAL;
using TRS.IT.TrsAppSettings;
using TARSharedUtilLibBFLBLL = TRS.IT.SI.BusinessFacadeLayer;

namespace FWFundUpdatesToISCBatch.BLL
{
    public class FWUtils
    {
        public static BFLModel.ContractsFundsInfo GeteDocsSubmittedFWCases(string StartDate, string EndDate, ref BFLModel.ContractsFundsInfo oContractFundsInfo)
        {
            TARSharedUtilLibBFLBLL.FundWizard oFw;
            string sSessionID = "";
            string ContractID = "";
            string SubId = "";
            var tbActiveFunds = new DataTable();
            BFLModel.AddFundsInfo oAddFundsInfo;
            var oTransferFundsInfo = new BFLModel.TransferFundsInfo();
            BFLModel.FWFundsInfo oFundInfo;
            string DefaultFundID = "";
            string ForfFundID = "";
            string QDIA = "";
            string FundSeries = "";
            var PX = default(bool);
            string PartnerDefaultFundID = "";
            string PartnerForfFundID = "";
            var oAddDeleteFundsInfo = new BFLModel.AddDeleteFundsInfo();
            string eDocsXML = "";
            int iCnt;
            string FidName = "";
            int iRowNo = 0;
            try
            {
                if (oContractFundsInfo == null)
                {
                    oContractFundsInfo = new BFLModel.ContractsFundsInfo();
                }
                if (oContractFundsInfo.AddDeleteFundsInfo == null)
                {
                    oContractFundsInfo.AddDeleteFundsInfo = new List<BFLModel.AddDeleteFundsInfo>();
                }

                TRS.IT.SI.Services.TRSPlanService.P3Item[] P3;

                eDocsXML = new SOA.eDocsSOA().GeteDocsFWPendingCases(StartDate, EndDate);
                P3 = (TRS.IT.SI.Services.TRSPlanService.P3Item[])TRS.IT.TRSManagers.XMLManager.DeserializeXml(eDocsXML, typeof(TRS.IT.SI.Services.TRSPlanService.P3Item[]));

                if (P3.Length > 0)
                {

                    var loopTo = P3.Length - 1;
                    for (iCnt = 0; iCnt <= loopTo; iCnt++)
                    {

                        try
                        {
                            iRowNo = 0;
                            ContractID = "";
                            SubId = "";
                            oAddDeleteFundsInfo = new BFLModel.AddDeleteFundsInfo();

                            iRowNo = General.InsertFWeDocsCases(TRS.IT.TRSManagers.XMLManager.GetXML(P3[iCnt]), StartDate, EndDate);

                            ContractID = P3[iCnt].ContractID;
                            SubId = P3[iCnt].SubID;

                            oAddFundsInfo = new BFLModel.AddFundsInfo();
                            oAddDeleteFundsInfo.ContractID = ContractID;
                            oAddDeleteFundsInfo.SubID = TARSharedUtilLibBFLBLL.DAL.General.SubOut(SubId);
                            oAddDeleteFundsInfo.ProjectManager = P3[iCnt].ProjectManager;
                            oAddDeleteFundsInfo.Type = "NEW";
                            string errorMessage = "";
                            // GetManagedAdviceInfo
                            var maInfo = new SOA.eDocsSOA().GetManagedAdvice(oAddDeleteFundsInfo.ContractID, oAddDeleteFundsInfo.SubID, out errorMessage);

                            // Get Active Funds
                            oFw = new TARSharedUtilLibBFLBLL.FundWizard(sSessionID, ContractID, SubId);
                            oFw.PartnerId = "ISC";
                            eDocsXML += Environment.NewLine + "Going to get Active Funds listing";
                            tbActiveFunds = oFw.GetActiveFunds(false, false, true, false, true);
                            foreach (DataRow oRow in tbActiveFunds.Rows)
                            {
                                if (oRow["fund_id"].ToString() == "-1" | oRow["fund_id"].ToString() == "-2")
                                {
                                    continue;
                                }

                                if (oAddFundsInfo.FundsInfo == null)
                                {
                                    oAddFundsInfo.FundsInfo = new List<BFLModel.FWFundsInfo>();
                                }
                                oFundInfo = new BFLModel.FWFundsInfo();
                                oFundInfo.FundID = oRow["fund_id"].ToString();
                                eDocsXML += Environment.NewLine + "Going to get P3 Fund Info";
                                oFundInfo.PartnerFundID = oRow["FundDescriptor"].ToString();
                                oAddFundsInfo.FundsInfo.Add(oFundInfo);

                            }
                            oAddDeleteFundsInfo.AddFundsInfo = oAddFundsInfo;
                            oAddDeleteFundsInfo.TransferFundsInfo = oTransferFundsInfo;
                            eDocsXML += Environment.NewLine + "Going to get default and forfeiture";
                            FidName = "";
                            new FundWizard(oFw).GetDefaultForfeitureFunds(ref DefaultFundID, ref ForfFundID, ref QDIA, ref FundSeries, ref PX, ref PartnerDefaultFundID, ref PartnerForfFundID, ref FidName);

                            var oDefaultFundInfo = new BFLModel.DefaultFundInfo();
                            var oDefFundInfo = new BFLModel.DfltFundInfo();
                            if (!string.IsNullOrEmpty(DefaultFundID.Trim()))
                            {
                                oDefFundInfo.FundID = DefaultFundID;
                                if (DefaultFundID.Trim() == "PX")
                                {
                                    oDefFundInfo.PartnerFundID = "PX";
                                    oDefFundInfo.Fiduciary = FidName;
                                }
                                else
                                {
                                    oDefFundInfo.PartnerFundID = PartnerDefaultFundID;
                                }
                                oDefFundInfo.FundSeries = FundSeries;
                                oDefFundInfo.QDIA = QDIA;
                                oDefFundInfo.PptAgree = P3[iCnt].PXAgreementCode;
                                oDefFundInfo.TransferDfltMny = "false";
                                oDefaultFundInfo.FundsInfo = oDefFundInfo;
                            }

                            // Default Fund for ManagedAdvice - This over rides above value at oDefaultFundInfo.FundsInfo
                            if (string.IsNullOrEmpty(errorMessage) && (maInfo.ma_selected == "0001" || maInfo.ma_selected == "1"))
                            {
                                // Fix for QDIA element missing in FW Xml for new cases - SP 2019-02-06
                                if (maInfo.ma_as_default == "0001" || maInfo.ma_as_default == "1")  // ma as qdia
                                {
                                    oDefFundInfo.FundID = "MA";
                                    oDefFundInfo.PartnerFundID = "MA";
                                    oDefFundInfo.TransferDfltMny = "true";
                                    oDefFundInfo.QDIA = "true";
                                }
                                else if (maInfo.ma_as_qdia == "0001" || maInfo.ma_as_qdia == "1")  // differnt fund is qdia not MA
                                {
                                    oDefFundInfo.QDIA = "true";
                                    oDefFundInfo.FundID = oDefaultFundInfo.FundsInfo.FundID;
                                    oDefFundInfo.PartnerFundID = oDefaultFundInfo.FundsInfo.PartnerFundID;
                                    oDefFundInfo.TransferDfltMny = "false";
                                }
                                else
                                {
                                    oDefFundInfo.QDIA = "false";
                                }
                                if (maInfo.ma_selected == "0001" || maInfo.ma_selected == "1")
                                {
                                    oDefFundInfo.PptAgree = "DCMA";
                                }
                                oDefaultFundInfo.FundsInfo = oDefFundInfo;
                            }

                            oAddDeleteFundsInfo.DefaultFundInfo = oDefaultFundInfo;
                            var oForfeitureFundInfo = new BFLModel.ForfeitureFundInfo();
                            if (!string.IsNullOrEmpty(ForfFundID.Trim()))
                            {
                                oFundInfo = new BFLModel.FWFundsInfo();
                                oFundInfo.FundID = ForfFundID;
                                oFundInfo.PartnerFundID = PartnerForfFundID;
                                oForfeitureFundInfo.FundsInfo = oFundInfo;
                            }

                            oAddDeleteFundsInfo.ForfeitureFundInfo = oForfeitureFundInfo;

                            if (string.IsNullOrEmpty(errorMessage) && (maInfo.ma_selected == "1" || maInfo.ma_selected == "0001"))
                            {
                                var managedAdviceInstallInfo = new BFLModel.ManagedAdviceInstallInfo();
                                if (maInfo.ma_as_default == "0001" || maInfo.ma_as_default == "1")
                                {
                                    managedAdviceInstallInfo.InDefaultStrategy = "1";
                                }
                                else
                                {
                                    managedAdviceInstallInfo.InDefaultStrategy = "0";
                                }
                                if (maInfo.ma_conversion_method == "0001" || maInfo.ma_conversion_method == "1")
                                {
                                    managedAdviceInstallInfo.OutofDefaultStrategy = "1";
                                }
                                else
                                {
                                    managedAdviceInstallInfo.OutofDefaultStrategy = "0";
                                }
                                managedAdviceInstallInfo.PXDisablewithMA = "false";
                                if (maInfo.ma_selected == "0001" || maInfo.ma_selected == "1")
                                {                                    
                                }
                                managedAdviceInstallInfo.FreeLookDays = maInfo.ma_free_look_days;
                                managedAdviceInstallInfo.FeeTotalBasisPoints = maInfo.ma_fee;

                                oAddDeleteFundsInfo.ManagedAdviceInstallInfo = managedAdviceInstallInfo;
                            }
                            if (P3[iCnt].MDP_ProductID == "122" | P3[iCnt].MDP_ProductID == "123")
                            {
                                var xMdp = MDP.GetMdpLegacyCaseObject(Convert.ToInt32(P3[iCnt].MDP_RunID));
                                oAddDeleteFundsInfo.FeeInfo = GetFeeConfigurationInfo(xMdp.Element("MDP"));
                            }
                            oContractFundsInfo.AddDeleteFundsInfo.Add(oAddDeleteFundsInfo); 
                            General.UpdateFWeDocsCases(iRowNo, 100, ContractID, SubId, oAddDeleteFundsInfo.Type, P3[iCnt].ProjectManager, oAddDeleteFundsInfo);
                        }

                        catch (Exception exi)
                        {
                            Logger.LogMessage(exi.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                            General.UpdateFWeDocsCases(iRowNo, -1, ContractID, SubId, "NEW", "", oAddDeleteFundsInfo);
                            TRS.IT.SI.BusinessFacadeLayer.Util.SendMail(AppSettings.GetValue("BendFromEmail"), AppSettings.GetValue("TRSWebDevelopment"), "GeteDocs FW case", eDocsXML + Environment.NewLine + exi.Message, false, true, AppSettings.GetValue("TRSWebDevelopment"));
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                Logger.LogMessage(ex.ToString(), Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                TRS.IT.SI.BusinessFacadeLayer.Util.SendMail(AppSettings.GetValue("BendFromEmail"), AppSettings.GetValue("GeteDocsSubmittedFWCasesToEmail"), "GeteDocs FW case", eDocsXML + Environment.NewLine + ex.Message);
            }

            return oContractFundsInfo;
        }
        public static string GetParticipantAgreementCode(string ContractID, string SubID)
        {
            string eDocsXML = "";
            string PPTAgreementCode = "";
            eDocsXML = new SOA.eDocsSOA().GetParticipantAgreementCode(ContractID, SubID);

            TRS.IT.SI.Services.TRSPlanService.P3Item[] P3;
            P3 = (TRS.IT.SI.Services.TRSPlanService.P3Item[])TRS.IT.TRSManagers.XMLManager.DeserializeXml(eDocsXML, typeof(TRS.IT.SI.Services.TRSPlanService.P3Item[]));

            if (P3.Length > 0)
            {
                PPTAgreementCode = P3[0].PXAgreementCode;
            }

            return PPTAgreementCode;
        }
        public static string GetP3FundInfo(string FundID)
        {
            var ds = FWDocGenDC.GetP3FundsData(FundID);
            if (ds.Tables.Count >= 1)
            {
                return Convert.ToString(ds.Tables[0].Rows[0]["p3_fund_descriptor"]);
            }
            else
            {
                return "";
            }

        }
        private static BFLModel.FeeConfigurations GetFeeConfigurationInfo(XElement srcMdpXml)
        {
            var objFeeInfo = new BFLModel.FeeConfigurations();
            objFeeInfo.CACCommCalcTypeID = srcMdpXml.Element("RunDetail").Element("CACCommCalcTypeID").Value;
            objFeeInfo.TakeoverAssets = srcMdpXml.Element("RunDetail").Element("TakeoverAssets").Value;
            objFeeInfo.AnnualFlow = srcMdpXml.Element("RunDetail").Element("AnnualFlow").Value;
            objFeeInfo.RolloverAmount = srcMdpXml.Element("RunDetail").Element("RolloverAmount").Value;
            objFeeInfo.Participants = srcMdpXml.Element("RunDetail").Element("Participants").Value;
            objFeeInfo.EligibleEmployees = srcMdpXml.Element("RunDetail").Element("EligibleEmployees").Value;
            objFeeInfo.ProductID = srcMdpXml.Element("RunDetail").Element("ProductID").Value;
            objFeeInfo.PAATypeID = srcMdpXml.Element("RunDetail").Element("PAAType").Element("Id").Value;
            objFeeInfo.Features = GetFeaturesList(srcMdpXml);
            objFeeInfo.LoiBand = new List<BFLModel.Band>() { GetBandInfo(srcMdpXml.Element("VACCommCfg").Element("LoiBand")) };
            objFeeInfo.CommCfgs = GetCommConfigList(srcMdpXml);
            return objFeeInfo;
        }
        private static List<BFLModel.Feature> GetFeaturesList(XElement mdp)
        {
            var objFeatures = new List<BFLModel.Feature>();
            var includedFeatures = new List<string>() { "112", "113", "114", "115", "116" };
            foreach (XElement feature in mdp.Element("RunDetail").Element("Features").Elements())
            {
                if (includedFeatures.Contains(feature.Element("FeatureID").Value))
                {
                    if (feature.Element("FeatureDtlName").Value == "None" && feature.Element("FeatureDtlID").Value == "0")
                    {
                    }
                    else
                    {
                        var eleBand = new BFLModel.Feature()
                        {
                            Id = feature.Element("FeatureID").Value,
                            Name = feature.Element("FeatureName").Value,
                            DtlId = feature.Element("FeatureDtlID").Value,
                            Value = feature.Element("FeatureDtlName").Value
                        };
                        objFeatures.Add(eleBand);
                    }
                }
            }
            return objFeatures;
        }
        private static List<BFLModel.CommCfg> GetCommConfigList(XElement mdp)
        {
            var objCommCfgs = new List<BFLModel.CommCfg>();
            foreach (XElement config in mdp.Descendants("CommCfgID"))
            {
                if (config.Parent.Element("CommType") != null && !config.Parent.Element("CommType").IsEmpty && config.Parent.Element("CommCfgComboID").Value != "2004")
                {
                    var objCommCfg = new BFLModel.CommCfg();
                    objCommCfg.Id = config.Value;
                    objCommCfg.Name = config.Parent.Element("CommCfgName").Value;
                    objCommCfg.ComboId = config.Parent.Element("CommCfgComboID").Value;
                    var objCommTypes = new List<BFLModel.CommType>();
                    foreach (XElement commType in config.Parent.Elements("CommType"))
                    {
                        var objCommType = new BFLModel.CommType();
                        objCommType.Id = commType.Element("CommTypeID").Value;
                        objCommType.Name = commType.Element("CommTypeName").Value;
                        var objBands = new List<BFLModel.Band>();
                        foreach (XElement commScale in commType.Descendants("CommScale"))
                            objBands.Add(GetBandInfo(commScale));
                        objCommType.CommScaleBands = objBands;
                        objCommTypes.Add(objCommType);
                    }
                    objCommCfg.CommTypes = objCommTypes;
                    objCommCfgs.Add(objCommCfg);
                }
            }
            if (mdp.Element("RunDetail").Elements("PpRequiredRevenueInput").Any())
            {
                objCommCfgs.Add(GetPptFeeConfig(mdp));
            }
            return objCommCfgs;
        }
        private static BFLModel.CommCfg GetPptFeeConfig(XElement mdp)
        {
            string pptReqRevId = mdp.Element("RunDetail").Element("PpRequiredRevenueInput").Element("PptRrId").Value;
            string pptReqRevName = mdp.Element("RunDetail").Element("PpRequiredRevenueInput").Element("PptRrName").Value;
            var objCommConfig = new BFLModel.CommCfg();
            objCommConfig.Id = mdp.Element("SFeeCfg").Element("SFeeCfgID").Value;
            objCommConfig.Name = mdp.Element("SFeeCfg").Element("SFeeCfgName").Value;
            objCommConfig.PptRrId = pptReqRevId;
            objCommConfig.PptRrName = pptReqRevName;
            var feeTypes = new List<string>() { "112", "140", "141", "142", "143", "144" };
            if (pptReqRevId == "102" | pptReqRevId == "103")
            {
                feeTypes.Remove("112");
            }
            var fees = mdp.Element("SFeeCfg").Elements("Fee").Where(x => feeTypes.Contains(x.Element("TypeID").Value));
            var objCommTypes = new List<BFLModel.CommType>();
            var objCommType = new BFLModel.CommType();
            objCommType.Id = fees.First().Element("TypeID").Value;
            objCommType.Name = fees.First().Element("TypeName").Value;
            var objBands = new List<BFLModel.Band>();

            foreach (XElement fee in fees)
            {
                foreach (XElement feeScale in fee.Descendants("ScaleDetail"))
                {
                    var objBand = GetBandInfo(feeScale);
                    if (objBand.Id == "1" && objBands.Count > 0)
                    {
                        objBand.Id = (objBands.Count + 1).ToString();
                    }
                    objBands.Add(objBand);
                }
            }

            objCommType.CommScaleBands = objBands;
            objCommTypes.Add(objCommType);
            objCommConfig.CommTypes = objCommTypes;
            return objCommConfig;
        }
        private static BFLModel.Band GetBandInfo(XElement bandElement)
        {
            var objBand = new BFLModel.Band();

            if (bandElement != null && !bandElement.IsEmpty)
            {
                objBand.Id = bandElement.Element("BandID").Value;
                objBand.Start = bandElement.Element("BandStart").Value;
                objBand.End = bandElement.Element("BandEnd").Value;
                objBand.Rate = bandElement.Element("BandRate").Value;
            }

            return objBand;
        }
    }
}