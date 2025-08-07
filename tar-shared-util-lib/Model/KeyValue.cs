namespace TRS.IT.SOA.Model
{
    [System.Xml.Serialization.XmlType(Namespace = "http://TRS.SOAModel/")]
    public class KeyValue
    {
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string key;
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string value;
    }
}
