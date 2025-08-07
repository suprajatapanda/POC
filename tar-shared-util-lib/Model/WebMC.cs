using System.Xml.Serialization;

namespace TRS.IT.SOA.Model
{
    public enum webMsgGlobalFolderEnum : int
    {
        [XmlEnum(Name = "Inbox")]
        Inbox = 1,
        [XmlEnum(Name = "Sent")]
        Sent = 2,
        [XmlEnum(Name = "Delete")]
        Delete = 3,
        [XmlEnum(Name = "Saved")]
        Saved = 4,
        [XmlEnum(Name = "Unread")]
        Unread = 5,
        [XmlEnum(Name = "Flagged")]
        Flagged = 6,
        [XmlEnum(Name = "Other")]
        Other = 7,
    }
}
