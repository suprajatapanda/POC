namespace TRS.IT.SI.BusinessFacadeLayer.Model
{
    [Serializable(), System.Xml.Serialization.XmlType(Namespace = "http://SI_Schema.schParticipantInfo")]
    public class TestingResults
    {
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ContractID;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SubID;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PartnerID;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ProcessDate;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TestingPYE;

        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TestYear;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PTNRAnalystFirst;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PTNRAnalystLast;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PTNRAnalystPhone;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PTNRAnalystEmail;

        ///<remarks>
        ///value: 0 = NO, 1 = YES
        ///<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool SafeHarbor;

        ///<remarks>
        ///value: 0 = FAIL, 1 = PASS , ElementName = "FourOneSixTestResult"
        ///<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, ElementName = "FourOneSixTestResults")]
        public bool FourOneSixTestResult;

        ///<remarks>
        ///value: 0 = FAIL, 1 = PASS
        ///<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool ADPStatus;

        ///<remarks>
        ///value: 0 = FAIL, 1 = PASS
        ///<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool ACPStatus;

        ///<remarks>
        ///value: 1 = PRIORYEAR, 2 = CURRENTYEAR
        ///<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public E_ADPACPtestingmethodType ADPACPTestingMethod;

        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double ADPRefundAmt;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double ACPRefundAmt;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double ADPHCEPercentage;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double ADPNHCEPercentage;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double ACPHCEPercentage;
        //<remarks/>
        [System.Xml.Serialization.XmlElement(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public double ACPNHCEPercentage;

    }
}
