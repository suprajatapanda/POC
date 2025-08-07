namespace TRS.IT.TRSManagers
{
    public class FileManager
    {
        public static string WriteRemoteFile(string a_sFileName, object a_sData, bool a_bAppend)
        {
            try
            {
                StreamWriter streamWriter = new(a_sFileName, a_bAppend);
                streamWriter.Write(a_sData);
                streamWriter.Close();
                return "0";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static void CopyFileToRemote(string a_sSourceFile, string a_sTargetFile, bool a_bDeleteLocal)
        {
            try
            {
                File.Copy(a_sSourceFile, a_sTargetFile, true);
                if (!a_bDeleteLocal)
                {
                    return;
                }

                File.Delete(a_sSourceFile);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
