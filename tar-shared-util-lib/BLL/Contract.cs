using System.Data;
using SOAModel = TRS.IT.SOA.Model;

namespace TRS.IT.SI.BusinessFacadeLayer
{
    [Serializable()]
    public class Contract
    {
        private string _sSessionId;
        private int _iInloginId;
        private string _sConId;
        private string _sSubId;
        private SOA.ContractSoa _SOA_ConAdapter;
        public Contract(string a_sConId, string a_sSubId, bool bIsSimple)
        {
            _sConId = a_sConId;
            _sSubId = a_sSubId;
            if (bIsSimple == false)
            {
                _SOA_ConAdapter = new SOA.ContractSoa("Test");
            }
        }
        public Contract(string a_sConId, string a_sSubId) : this(a_sConId, a_sSubId, bIsSimple: false)
        {
        }
        public string SessionId
        {
            get
            {
                return _sSessionId;
            }
        }
        public int InLoginId
        {
            get
            {
                return _iInloginId;
            }
        }
        public string ContractId
        {
            get
            {
                return _sConId;
            }
        }
        public string SubId
        {
            get
            {
                return _sSubId;
            }
        }

        public SOAModel.ContractInfo GetContractInformation(string ContractID, string SubID, SOAModel.AdditionalData a_oAdditionalData, string lSessionID = "")
        {
            string strXML = "";
            SOAModel.ContractInfo ContractInfo;

            if (IsPlanDataCachedOk())
            {
                strXML = DAL.AudienceDC.GetObjectDataByDate(DateTime.Now.Date.ToString(), Model.Enums.E_ObjectType.ContractInfo, ContractID, SubID);
            }
            if (string.IsNullOrEmpty(strXML))
            {
                ContractInfo = _SOA_ConAdapter.GetContractInformation(ContractID, SubID, a_oAdditionalData);
                if (string.IsNullOrEmpty(lSessionID))
                {
                    lSessionID = _sSessionId;
                }

                if (ContractInfo.Errors[0].Number == 0)
                {
                    if (!(lSessionID == null) && lSessionID.Length > 0)
                    {
                        strXML = TRSManagers.XMLManager.GetXML(ContractInfo);

                        DAL.AudienceDC.SaveObjectDataByDate(lSessionID, Model.Enums.E_ObjectType.ContractInfo, strXML, ContractID, SubID);
                    }
                }
            }
            else
            {
                ContractInfo = TRSManagers.XMLManager.DeserializeFromXml<SOAModel.ContractInfo>(strXML);
            }
            return ContractInfo;
        }
        public bool IsNAVProduct(string a_sConId, string a_sSubId)
        {
            bool bIsNAV = false;
            SOAModel.ContractInfo oContractInfo;
            var a_oAdditionalData = new SOAModel.AdditionalData();
            oContractInfo = GetContractInformation(a_sConId, a_sSubId, a_oAdditionalData, "");
            if (!(oContractInfo.KeyValuePairs == null))
            {
                string KeyValue = (from kv in oContractInfo.KeyValuePairs
                                   where kv.key.ToLower() == "navproduct"
                                   select kv.value).FirstOrDefault();
                if (KeyValue == "1")
                {
                    bIsNAV = true;
                }
                else if (KeyValue == "0")
                {
                    bIsNAV = false;
                }
                else
                {
                    bIsNAV = false;
                }
            }
            return bIsNAV;
        }
        public static bool IsPlanDataCachedOk()
        {
            string PlanDataCached = "True";
            if (!(TrsAppSettings.AppSettings.GetValue("PlanDataCached") == null))
            {
                PlanDataCached = TrsAppSettings.AppSettings.GetValue("PlanDataCached");
            }

            if (PlanDataCached.ToUpper() == "TRUE")
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public string SubmitTestingResults(string a_sXmlTestingResults)
        {
            return _SOA_ConAdapter.SubmitTestingResults(a_sXmlTestingResults);
        }
    }
}