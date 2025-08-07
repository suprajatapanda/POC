using TRS.IT.SI.BusinessFacadeLayer.Model;

namespace TRS.IT.SOA.Model
{
    [System.Xml.Serialization.XmlType(Namespace = "http://TRS.SOAModel/")]
    public class TPACompanyContactInformations : TWS_Response
    {
        public List<TPAContactInformation> TPAContactInfo = new();
    }

    [System.Xml.Serialization.XmlType(Namespace = "http://TRS.SOAModel/")]
    public class TPAContactInformation : TWS_Response
    {
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Contact_id = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Web_InLoginId = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PartnerUserID = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string FirstName = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string LastName = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AddressInfo Address;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public CommunicationInfo CommunicationInfo = new();

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Title;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public E_TPACompanyContactType ContactType;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public E_TPAContactType ContractContactType;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string BirthDay = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Department = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string JobTitle = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Personality_Type = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Comments = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CompanyName = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<TPAContactTypeInfo> TPAContactTypes = new();

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string IsKeyContact = string.Empty;
    }

    [System.Xml.Serialization.XmlType(Namespace = "http://TRS.SOAModel/")]
    public class TPAContactTypeInfo
    {
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public E_TPACompanyContactType ContactType;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CompanyName;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CompanyID;
    }

}
