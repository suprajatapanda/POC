namespace TARSharedUtilLib.Utility
{
    public static class FileOperationHelpers
    {
        public static void CopyDirectory(string sourceDir, string destDir)
        {
            FileManagerSMB.CreateDirectory(destDir);

            var files = FileManagerSMB.GetFiles(sourceDir);
            foreach (var file in files)
            {
                var fileName = FileManagerSMB.GetFileName(file);
                var destFile = FileManagerSMB.Combine(destDir, fileName);
                FileManagerSMB.Copy(file, destFile);
            }

            var dirs = FileManagerSMB.GetDirectories(sourceDir);
            foreach (var dir in dirs)
            {
                var dirName = FileManagerSMB.GetFileName(dir);
                var destSubDir = FileManagerSMB.Combine(destDir, dirName);
                CopyDirectory(dir, destSubDir);
            }
        }

        public static void DeleteDirectory(string path)
        {
            var files = FileManagerSMB.GetFiles(path);
            foreach (var file in files)
            {
                FileManagerSMB.Delete(file);
            }

            var dirs = FileManagerSMB.GetDirectories(path);
            foreach (var dir in dirs)
            {
                DeleteDirectory(dir);
            }
        }

        public static void EnsureDirectoryExists(string path)
        {
            if (!FileManagerSMB.DirectoryExists(path))
            {
                FileManagerSMB.CreateDirectory(path);
            }
        }

        public static void SafeWriteFile(string path, byte[] content)
        {
            string backupPath = path + ".bak";

            if (FileManagerSMB.Exists(path))
            {
                FileManagerSMB.Copy(path, backupPath);
            }

            try
            {
                FileManagerSMB.WriteAllBytes(path, content);

                if (FileManagerSMB.Exists(backupPath))
                {
                    FileManagerSMB.Delete(backupPath);
                }
            }
            catch
            {
                if (FileManagerSMB.Exists(backupPath))
                {
                    FileManagerSMB.Copy(backupPath, path);
                    FileManagerSMB.Delete(backupPath);
                }
                throw;
            }
        }
    }
}
