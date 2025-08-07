using System.Xml.Serialization;

namespace TRS.IT.SI.BusinessFacadeLayer.Model
{
    public interface ISponsorAdapter
    {
        ReportResponse GetReport(string sessionID, ReportInfo oReportInfo, ref bool bAvail, PartnerFlag Partner);
    }
    #region "*** Classes ***"
    [Serializable(), XmlType(Namespace = "http://SI_Schema.schSponsorInfo")]
    public class SourceInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SourceID;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SourceName;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double TotalBalance;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VestingBalance;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double VestingPercent;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double AvailableHardshipBalance;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double AvailableInserviceBalance;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SourceTypeC;

    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schSponsorInfo")]
    public class SourceGroupInfo
    {
        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SourceGroupID;

        //<remarks/>
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SourceGroupName;

        //<remarks/>
        [XmlElement("FundInfo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public FundInfo[] FundInfo;
    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schSponsorInfo")]
    public class QbadDistributionFees
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TpaFeeAmount;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TrsFeeAmount;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TpaFeePaidByCode;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TrsFeePaidByCode;
    }

    [Serializable(), XmlType(Namespace = "http://SI_Schema.schSponsorInfo")]
    public class PriorQbadWithdrawals
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ChildTIN;

        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AmountRequested;
    }

    #endregion
}