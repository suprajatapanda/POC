using System.Runtime.Serialization;

namespace TRS.IT.SI.BusinessFacadeLayer.Model.HSA
{
    [DataContract]
    public class HSAInfo
    {
        [DataMember]
        public string contractId { get; set; }

        [DataMember]
        public string affiliateNumber { get; set; }

        [DataMember]
        public Boolean hsaIndicator { get; set; }

    }

    [DataContract]
    public class PlanHSAInfo
    {
        [DataMember]
        public List<HSAInfo> HSAInfos { get; set; }

        [DataMember]
        public ErrorInfo[] Errors;

        public PlanHSAInfo()
        {
            HSAInfos = new List<HSAInfo>();
            Errors = new ErrorInfo[1];
            Errors[0] = new ErrorInfo();
        }

        public static implicit operator PlanHSAInfo(List<HSAInfo> v)
        {
            throw new NotImplementedException();
        }
    }


}
