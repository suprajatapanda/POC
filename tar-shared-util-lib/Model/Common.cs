using TRS.IT.SI.BusinessFacadeLayer.Model;

namespace TRS.IT.SOA.Model
{
    [System.Xml.Serialization.XmlType(Namespace = "http://TRS.SOAModel/")]
    public class AddressInfo
    {

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public E_AddressType Type = E_AddressType.Other;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Address1 = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Address2 = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Address3 = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string City = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string State = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Country = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ZipCode = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Boolean InternationalAddress;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int AddressID = 9999;
    }

    [System.Xml.Serialization.XmlType(Namespace = "http://TRS.SOAModel/")]
    public class TelephoneInfo
    {
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public E_PhoneType Type = E_PhoneType.Other;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Number;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Extension;
    }

    [System.Xml.Serialization.XmlType(Namespace = "http://TRS.SOAModel/")]
    public class CommunicationInfo
    {
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<TelephoneInfo> TelephoneInfo;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EmailAddress = string.Empty;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string WebsiteAddress = string.Empty;
    }

    [System.Xml.Serialization.XmlType(Namespace = "http://TRS.SOAModel/")]
    public class ErrorInfo
    {
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int Number;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Description;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Type;
    }

    [System.Xml.Serialization.XmlType(Namespace = "http://TRS.SOAModel/")]
    public class TWS_Response
    {
        public TWS_Response()
        {
            ErrorInfo[] localError;

            localError = new ErrorInfo[1];
            localError[0] = new ErrorInfo();
            Errors = localError;
        }

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ErrorInfo[] Errors;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ConfirmationNumber;

        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<string> Warnings;
    }

}
