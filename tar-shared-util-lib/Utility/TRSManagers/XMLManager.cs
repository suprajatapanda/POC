using System.Xml.Serialization;

namespace TRS.IT.TRSManagers
{
    public class XMLManager
    {
        public static string GetXML(object obj)
        {
            string empty = string.Empty;
            XmlSerializer xmlSerializer = new(obj.GetType());
            MemoryStream memoryStream = new();
            xmlSerializer.Serialize(memoryStream, obj);
            memoryStream.Position = 0L;
            StreamReader streamReader = new(memoryStream);
            string end = streamReader.ReadToEnd();
            streamReader.Close();
            memoryStream.Close();
            return end.Replace("xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"", "");
        }
        public static object DeserializeXml(string xmlString, Type objType)
        {
            XmlSerializer xmlSerializer = new(objType);
            StringReader stringReader = new(xmlString);
            object obj = xmlSerializer.Deserialize(stringReader);
            stringReader.Close();
            return obj;
        }
        public static T DeserializeFromXml<T>(string xml)
        {
            using (TextReader textReader = new StringReader(xml))
            {
                return (T)new XmlSerializer(typeof(T)).Deserialize(textReader);
            }
        }
    }
}
