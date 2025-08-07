namespace TARSharedUtilLib.Utility
{
    public static class SMBPathUtilities
    {
        public static bool TryParseSmbPath(string path, out string hostname, out string shareName, out string relativePath)
        {
            hostname = null;
            shareName = null;
            relativePath = null;

            if (!path.StartsWith(@"\\"))
                return false;

            var parts = path.TrimStart('\\').Split('\\');
            if (parts.Length < 2)
                return false;

            hostname = parts[0];
            shareName = parts[1];
            relativePath = string.Join("\\", parts.Skip(2));

            return true;
        }
        public static string BuildSmbPath(string hostname, string shareName, string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return $@"\\{hostname}\{shareName}";

            return $@"\\{hostname}\{shareName}\{relativePath.TrimStart('\\')}";
        }
        public static bool IsSmbPath(string path)
        {
            return !string.IsNullOrEmpty(path) && path.StartsWith(@"\\");
        }
        public static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            if (IsSmbPath(path))
                return path.Replace('/', '\\');

            return path.Replace('\\', '/');
        }
        public static string GetServerFromPath(string path)
        {
            if (TryParseSmbPath(path, out string hostname, out _, out _))
                return hostname;

            return null;
        }
        public static string GetShareFromPath(string path)
        {
            if (TryParseSmbPath(path, out _, out string shareName, out _))
                return shareName;

            return null;
        }
    }
}
