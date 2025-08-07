namespace TARSharedUtilLib.Utility
{
    public class DirectorySystemInfo
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime LastWriteTime { get; set; }
        public DateTime LastAccessTime { get; set; }
        public System.IO.FileAttributes Attributes { get; set; }
        public bool Exists => true;

        public DirectorySystemInfo() { }

        public DirectorySystemInfo(DirectoryInfo info)
        {
            Name = info.Name;
            FullName = info.FullName;
            CreationTime = info.CreationTime;
            LastWriteTime = info.LastWriteTime;
            LastAccessTime = info.LastAccessTime;
            Attributes = info.Attributes;
        }
    }
}
