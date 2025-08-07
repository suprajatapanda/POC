using TRS.IT.TrsAppSettings;

namespace TARSharedUtilLib.Utility
{
    public class FileSystemPath
    {
        public bool IsSmb { get; set; }
        public string Hostname { get; set; }
        public string ShareName { get; set; }
        public string RelativePath { get; set; }

        public static FileSystemPath Parse(string path)
        {
            if (path.StartsWith(@"\\"))
            {
                var parts = path.TrimStart('\\').Split('\\');
                if (parts.Length < 2)
                    throw new ArgumentException($"Invalid SMB path: {path}");

                var hostname = parts[0];
                var shareName = parts[1];
                var relativePath = string.Join("\\", parts.Skip(2));
                if (AppSettings.GetValue("DISABLESMB") == "0")
                {
                    return new FileSystemPath
                    {
                        IsSmb = true,
                        Hostname = hostname,
                        ShareName = shareName,
                        RelativePath = relativePath
                    };
                }
            }

            return new FileSystemPath
            {
                IsSmb = false,
                RelativePath = path
            };
        }
    }
}
