using System.Xml.Serialization;
namespace TRS.IT.SOA.Model
{
    [Serializable, XmlType(Namespace = "http://TRS.SOAModel/")]
    public class FundPendingChanges
    {
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CaseNo;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Action;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FundID;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public string FundName;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ClosingDate;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ToFundID;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public string ToFundName;
        [XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public string EffectiveDate;
    }
}