namespace TARSharedUtilLib.Utility
{
    public class SmbFileInfo : FileSystemInfo
    {
        private string _name;
        private string _fullName;

        public override string Name
        {
            get => _name;
        }

        public override string FullName
        {
            get => _fullName;
        }

        public override bool Exists => true;

        public void SetName(string name)
        {
            _name = name;
        }

        public void SetFullName(string fullName)
        {
            _fullName = fullName;
        }

        public override void Delete()
        {
            throw new NotSupportedException("Use FileManagerSMB.Delete instead");
        }
    }
}
