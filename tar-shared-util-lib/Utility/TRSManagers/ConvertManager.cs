namespace TRS.IT.TRSManagers
{
    public class ConvertManager
    {
        public static object CType(object Source, Type destobjType)
        {
            return XMLManager.DeserializeXml(XMLManager.GetXML(Source), destobjType);
        }
    }
}
