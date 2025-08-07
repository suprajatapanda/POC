using System.Collections;
using System.Text;

namespace TRS.IT.SI.BusinessFacadeLayer.Model
{

    [Serializable()]
    public class MessageVariable
    {
        public string Key;
        public string Value;
    }
    public class ToEmail
    {
        public string To;
    }

    [Serializable()]
    public class MessageData : ICloneable
    {
        private string sFrom = string.Empty;
        private string sFromDisplayName = string.Empty;
        private string sSourceName = string.Empty;
        private string sNotificationType = string.Empty;
        private string sContractID = string.Empty;
        private string sSubID = string.Empty;
        private string sSubject = string.Empty;
        private string sErrorDescription = string.Empty;
        private System.Net.Mail.MailAddressCollection bcclist = null;
        private System.Net.Mail.MailAddressCollection tolist = null;
        private System.Net.Mail.MailAddressCollection cclist = null;
        private StringBuilder objStringbuilder = null;
        private Hashtable objEmailVariable = null;
        private int iErrorNumber = 0;
        private int iMessageID = -1;
        private int iTaskCode = -1;
        private string sDepositID = "N/A";
        private E_AdminEmailType m_eMailType = E_AdminEmailType.SUCCESS;
        private string eStatus = "Pass";
        private SortedList<E_Variable, string> objGEmailVariable = null;
        private E_ReportColor m_eColorType = E_ReportColor.NOTIFICATION_SERVICE;
        private bool m_isHTML = true;
        private string sbodyText = string.Empty;
        private string sTransLogID = string.Empty;
        private DateTime dDate;
        private List<MessageVariable> variable = new();
        private List<ToEmail> toXML = new();
        private string sXMLWrap = string.Empty;
        private string sConfirmationNO = string.Empty;
        private int iSent = 0;
        private int iAttempt = 0;
        private string sEmailVariable = string.Empty;
        private bool bOverride = false;
        private E_MessageType eMessageType = E_MessageType.Email;
        private E_ImageOption eImageOptions = E_ImageOption.ImageMessageAndAttachments;
        private string sSSN = string.Empty;

        public MessageData()
        {
            objStringbuilder = new StringBuilder();
            objEmailVariable = new Hashtable();
            bcclist = new System.Net.Mail.MailAddressCollection();
            tolist = new System.Net.Mail.MailAddressCollection();
            cclist = new System.Net.Mail.MailAddressCollection();
            objGEmailVariable = new SortedList<E_Variable, string>();
            sFrom = "NotificationService@transamerica.com";
            sFromDisplayName = "Transamerica Retirement Solutions";
            sSourceName = "NotificationService";
            ErrorDescription = "Success!!";
            sContractID = "N/A";
            sSubID = "N/A";
            sNotificationType = "N/A";
            eStatus = E_Status.PASS.ToString();
            m_eMailType = E_AdminEmailType.SUCCESS;
            IsHTML = true;
            TransLogID = "-1";
            dDate = new DateTime();
            BodyText = "<b>Daily Notification Service Report. Date <u>" + DateTime.Now.ToShortDateString() + "</u></b><br>";
            eMessageType = E_MessageType.Email;
        }

        [System.Xml.Serialization.XmlIgnore()]
        public E_ImageOption EImageOption
        {
            get { return eImageOptions; }
            set { eImageOptions = value; }
        }
        [System.Xml.Serialization.XmlIgnore()]
        public E_MessageType Service_MessageType
        {
            get { return eMessageType; }
            set { eMessageType = value; }
        }
        public int Attempt
        {
            get { return iAttempt; }
            set { iAttempt = value; }
        }
        public string EmailVariable
        {
            get { return sEmailVariable; }
            set { sEmailVariable = value; }
        }
        public bool Override
        {
            get { return bOverride; }
            set { bOverride = value; }
        }
        [System.Xml.Serialization.XmlIgnore()]
        public DateTime Date
        {

            get { return dDate; }
            set { dDate = value; }
        }
        public string MetaData
        {
            get { return "<![CDATA[" + sXMLWrap + "]]>"; }
            set { sXMLWrap = value; }
        }
        public string CustomData
        {
            get { return sDepositID; }
            set { sDepositID = value; }
        }
        public string EmailTableHead
        {
            get { return EmailTable(); }
        }
        [System.Xml.Serialization.XmlIgnore()]
        public StringBuilder TextBuilder
        {
            get { return objStringbuilder; }
            set { objStringbuilder = value; }
        }

        [System.Xml.Serialization.XmlIgnore()]
        public Hashtable EmailVariableContainer
        {
            get { return objEmailVariable; }
            set { objEmailVariable = value; }
        }
        public List<MessageVariable> VariableList
        {
            get { return VariableTable(); }
        }
        public string From
        {
            get { return sFrom; }
            set { sFrom = value; }
        }
        public string FromDisplayName
        {
            get { return sFromDisplayName; }
            set { sFromDisplayName = value; }
        }
        public string SourceName
        {
            get { return sSourceName; }
            set { sSourceName = value; }
        }
        [System.Xml.Serialization.XmlIgnore()]
        public System.Net.Mail.MailAddressCollection BCC
        {
            get { return bcclist; }
            set { bcclist = value; }
        }
        public string GetBCC
        {
            get { return BuildList(BCC); }
        }
        public string ContractID
        {
            get { return sContractID; }
            set { sContractID = value; }
        }
        public string SubID
        {
            get { return sSubID; }
            set { sSubID = value; }
        }
        public int MessageID
        {
            get { return iMessageID; }
            set { iMessageID = value; }
        }
        public string DepositID
        {
            get { return sDepositID; }
            set { sDepositID = value; }
        }
        public int TaskCode
        {
            get { return iTaskCode; }
            set { iTaskCode = value; }
        }
        public string NotificationType
        {
            get { return sNotificationType; }
            set { sNotificationType = value; }
        }
        public E_AdminEmailType EMailType
        {
            get { return m_eMailType; }
            set { m_eMailType = value; }
        }
        public string ErrorDescription
        {
            get { return sErrorDescription; }
            set { sErrorDescription = value; }
        }
        public int ErrorNumber
        {
            get { return iErrorNumber; }
            set { iErrorNumber = value; }
        }
        [System.Xml.Serialization.XmlIgnore()]
        public System.Net.Mail.MailAddressCollection To
        {
            get { return tolist; }
            set { tolist = value; }
        }
        public string Status
        {
            get { return eStatus; }
            set { eStatus = value; }
        }
        public string Subject
        {
            get { return sSubject; }
            set { sSubject = value; }
        }
        public bool IsHTML
        {
            get { return m_isHTML; }
            set { m_isHTML = value; }
        }
        [System.Xml.Serialization.XmlIgnore()]
        public System.Net.Mail.MailAddressCollection CC
        {
            get { return cclist; }
            set { cclist = value; }
        }
        public string GetCC
        {
            get { return BuildList(CC); }
        }
        public string GetTo
        {
            get { return BuildList(To); }
        }
        public E_ReportColor EColorType
        {
            get { return m_eColorType; }
            set { m_eColorType = value; }
        }

        [System.Xml.Serialization.XmlIgnore()]
        public string BodyText
        {
            get { return sbodyText; }
            set { sbodyText = value; }
        }
        public string TransLogID
        {
            get { return sTransLogID; }
            set { sTransLogID = value; }
        }

        [System.Xml.Serialization.XmlIgnore()]
        public SortedList<E_Variable, string> EmailVariables
        {

            get { return objGEmailVariable; }

            set
            {
                objGEmailVariable = value;
                objEmailVariable.Add(objGEmailVariable.Keys, objGEmailVariable.Values);
            }
        }

        public List<ToEmail> SendTO
        {
            get { return TOCCXML(); }
        }
        public string ConfirmationNO
        {
            get { return sConfirmationNO; }
            set { sConfirmationNO = value; }
        }
        public int Sent
        {
            get { return iSent; }
            set { iSent = value; }
        }

        public string SSN
        {
            get { return sSSN; }
            set { sSSN = value; }
        }

        public object Clone()
        {
            return Copy();
        }
        public MessageData Copy()
        {
            MessageData objMsgdata = MemberwiseClone() as MessageData;
            objMsgdata.ContractID = (string)ContractID.Clone();
            objMsgdata.SubID = (string)SubID.Clone();
            objMsgdata.CustomData = (string)CustomData.Clone();
            objMsgdata.BodyText = (string)BodyText.Clone();
            objMsgdata.NotificationType = (string)NotificationType.Clone();
            objMsgdata.MessageID = MessageID;
            objMsgdata.TransLogID = (string)TransLogID.Clone();
            objMsgdata.Attempt = Attempt;
            objMsgdata.ConfirmationNO = (string)ConfirmationNO.Clone();
            objMsgdata.DepositID = (string)DepositID.Clone();
            objMsgdata.EmailVariable = (string)EmailVariable.Clone();
            objMsgdata.ErrorDescription = (string)ErrorDescription.Clone();
            objMsgdata.Sent = Sent;
            objMsgdata.Status = (string)Status.Clone();
            objMsgdata.TaskCode = TaskCode;
            objMsgdata.TransLogID = TransLogID;
            return objMsgdata;
        }

        private string BuildList(System.Net.Mail.MailAddressCollection tobcc)
        {
            if (tobcc.Count > 0)
            {
                StringBuilder sbToOrBccList = new();
                for (int index = 0; index <= tobcc.Count - 1; index++)
                {
                    sbToOrBccList.Append(tobcc[index].ToString());
                    if (index != tobcc.Count - 1)
                    {
                        sbToOrBccList.Append(";");
                    }
                }
                return sbToOrBccList.ToString();
            }
            return string.Empty;
        }
        private string EmailTable()
        {
            StringBuilder objStrHeader = new();
            foreach (string str in Enum.GetNames(typeof(E_TableHeader)))
            {
                objStrHeader.Append("<th>" + str + "</th>");
            }
            return objStrHeader.ToString();
        }

        private List<MessageVariable> VariableTable()
        {
            variable.Clear();
            foreach (DictionaryEntry ev in EmailVariableContainer)
            {
                MessageVariable objMsgVar = new();
                objMsgVar.Key = ev.Key.ToString() + "*";
                objMsgVar.Value = ev.Value.ToString() + ";";
                variable.Add(objMsgVar);
            }
            return variable;
        }

        private List<ToEmail> TOCCXML()
        {
            toXML.Clear();
            if (To.Count > 0)
            {
                for (int index = 0; index <= To.Count - 1; index++)
                {
                    ToEmail To = new();
                    To.To = this.To[index].ToString();
                    toXML.Add(To);
                }
            }
            return toXML;
        }


    }
}