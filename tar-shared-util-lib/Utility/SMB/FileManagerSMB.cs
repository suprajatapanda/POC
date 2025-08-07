using SMBLibrary.Client;
using SMBLibrary;
using System.Text;

namespace TARSharedUtilLib.Utility
{
    public static class FileManagerSMB
    {
        private static SMBConnectionConfig GetSMBCredentials(string hostname, string shareName)
        {
            return new SMBConnectionConfig
            {
                Hostname = hostname,
                ShareName = shareName,
                Domain = TRS.IT.TrsAppSettings.AppSettings.GetValue("CyberArkDomain"),
                Username = TRS.IT.TrsAppSettings.AppSettings.GetValue("CyberArkUserName"),
                Password = TRS.IT.TrsAppSettings.AppSettings.GetValue("CyberArkPassword")
            };
        }

        public static byte[] ReadAllBytes(string path)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
                return File.ReadAllBytes(path);

            return ReadSmbFileBytes(fsPath);
        }

        public static string[] ReadAllLines(string path)
        {
            var content = ReadAllText(path);
            return content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        public static string ReadAllText(string path)
        {
            var bytes = ReadAllBytes(path);
            return Encoding.UTF8.GetString(bytes);
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
            {
                File.WriteAllBytes(path, bytes);
                return;
            }

            WriteSmbFileBytes(fsPath, bytes);
        }

        public static void WriteAllLines(string path, string[] contents)
        {
            var content = string.Join(Environment.NewLine, contents);
            WriteAllText(path, content);
        }

        public static void WriteAllText(string path, string contents)
        {
            var bytes = Encoding.UTF8.GetBytes(contents);
            WriteAllBytes(path, bytes);
        }

        public static bool Exists(string path)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
                return File.Exists(path);

            return SmbFileExists(fsPath);
        }

        public static void Delete(string path)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
            {
                File.Delete(path);
                return;
            }

            DeleteSmbFile(fsPath);
        }

        public static void Move(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName);
            Delete(sourceFileName);
        }

        public static void Copy(string sourceFileName, string destFileName)
        {
            var content = ReadAllBytes(sourceFileName);
            WriteAllBytes(destFileName, content);
        }

        public static string[] GetDirectories(string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
                return Directory.GetDirectories(path, "*", searchOption);

            return GetSmbDirectories(fsPath, searchOption);
        }

        public static string[] GetFiles(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
                return Directory.GetFiles(path, searchPattern, searchOption);

            return GetSmbFiles(fsPath, searchPattern, searchOption);
        }

        public static bool DirectoryExists(string path)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
                return Directory.Exists(path);

            return SmbDirectoryExists(fsPath);
        }

        public static void CreateDirectory(string path)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
            {
                Directory.CreateDirectory(path);
                return;
            }

            CreateSmbDirectory(fsPath);
        }

        public static string Combine(params string[] paths)
        {
            return Path.Combine(paths);
        }

        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public static string GetFileNameWithoutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public static string ChangeExtension(string path, string extension)
        {
            return Path.ChangeExtension(path, extension);
        }

        public static string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }

        public static char DirectorySeparatorChar => Path.DirectorySeparatorChar;

        public static char[] GetInvalidFileNameChars()
        {
            return Path.GetInvalidFileNameChars();
        }

        public static Stream OpenRead(string path)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
                return File.OpenRead(path);

            var bytes = ReadSmbFileBytes(fsPath);
            return new MemoryStream(bytes);
        }

        public static StreamReader OpenText(string path)
        {
            var stream = OpenRead(path);
            return new StreamReader(stream);
        }

        public static FileSystemInfo GetFileInfo(string path)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
                return new FileInfo(path);

            return GetSmbFileInfo(fsPath);
        }

        public static DirectorySystemInfo GetDirectoryInfo(string path)
        {
            var fsPath = FileSystemPath.Parse(path);

            if (!fsPath.IsSmb)
                return new DirectorySystemInfo(new DirectoryInfo(path));

            return GetSmbDirectoryInfo(fsPath);
        }

        private static byte[] ReadSmbFileBytes(FileSystemPath fsPath)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            return ExecuteSmbOperation(config, (fileStore) =>
            {
                var status = fileStore.CreateFile(out object handle,
                    out FileStatus fileStatus,
                    fsPath.RelativePath,
                    AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.Read,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_NON_DIRECTORY_FILE,
                    null);

                if (status != NTStatus.STATUS_SUCCESS)
                    throw new IOException($"Failed to open file: {fsPath.RelativePath}, Status: {status}");

                var memoryStream = new MemoryStream();
                byte[] buffer = new byte[65536];
                long offset = 0;

                while (true)
                {
                    status = fileStore.ReadFile(out byte[] data, handle, offset, buffer.Length);
                    if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_END_OF_FILE)
                        throw new IOException($"Failed to read file: {fsPath.RelativePath}, Status: {status}");

                    if (data == null || data.Length == 0)
                        break;

                    memoryStream.Write(data, 0, data.Length);
                    offset += data.Length;
                }

                fileStore.CloseFile(handle);
                return memoryStream.ToArray();
            });
        }

        private static void WriteSmbFileBytes(FileSystemPath fsPath, byte[] content)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            ExecuteSmbOperation(config, (fileStore) =>
            {
                var directoryPath = GetDirectoryName(fsPath.RelativePath);
                if (!string.IsNullOrEmpty(directoryPath))
                {
                    EnsureSmbDirectoryExists(config, fileStore, directoryPath);
                }

                var status = fileStore.CreateFile(out object handle,
                    out FileStatus fileStatus,
                    fsPath.RelativePath,
                    AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.None,
                    CreateDisposition.FILE_OVERWRITE_IF,
                    CreateOptions.FILE_NON_DIRECTORY_FILE,
                    null);

                if (status != NTStatus.STATUS_SUCCESS)
                    throw new IOException($"Failed to create file: {fsPath.RelativePath}, Status: {status}");

                int chunkSize = 65536;
                for (int offset = 0; offset < content.Length; offset += chunkSize)
                {
                    int bytesToWrite = Math.Min(chunkSize, content.Length - offset);
                    byte[] chunk = new byte[bytesToWrite];
                    Array.Copy(content, offset, chunk, 0, bytesToWrite);

                    status = fileStore.WriteFile(out int bytesWritten, handle, offset, chunk);
                    if (status != NTStatus.STATUS_SUCCESS)
                        throw new IOException($"Failed to write to file: {fsPath.RelativePath}, Status: {status}");
                }

                fileStore.CloseFile(handle);
                return 0;
            });
        }

        private static bool SmbFileExists(FileSystemPath fsPath)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            return ExecuteSmbOperation(config, (fileStore) =>
            {
                var status = fileStore.CreateFile(out object handle,
                    out FileStatus fileStatus,
                    fsPath.RelativePath,
                    AccessMask.GENERIC_READ,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.Read,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_NON_DIRECTORY_FILE,
                    null);

                if (status == NTStatus.STATUS_SUCCESS)
                {
                    fileStore.CloseFile(handle);
                    return true;
                }

                return false;
            });
        }

        private static void DeleteSmbFile(FileSystemPath fsPath)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            ExecuteSmbOperation(config, (fileStore) =>
            {
                var status = fileStore.CreateFile(out object handle,
                    out FileStatus fileStatus,
                    fsPath.RelativePath,
                    AccessMask.DELETE | AccessMask.SYNCHRONIZE,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.None,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_DELETE_ON_CLOSE,
                    null);

                if (status != NTStatus.STATUS_SUCCESS)
                    throw new IOException($"Failed to delete file: {fsPath.RelativePath}, Status: {status}");

                fileStore.CloseFile(handle);
                return 0;
            });
        }

        private static string[] GetSmbDirectories(FileSystemPath fsPath, SearchOption searchOption)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            return ExecuteSmbOperation(config, (fileStore) =>
            {
                var directories = new List<string>();
                EnumerateSmbEntries(fileStore, fsPath.RelativePath, "*", searchOption, true, directories, null);
                return directories.Select(d => $@"\\{fsPath.Hostname}\{fsPath.ShareName}\{d}").ToArray();
            });
        }

        private static string[] GetSmbFiles(FileSystemPath fsPath, string searchPattern, SearchOption searchOption)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            return ExecuteSmbOperation(config, (fileStore) =>
            {
                var files = new List<string>();
                EnumerateSmbEntries(fileStore, fsPath.RelativePath, searchPattern, searchOption, false, null, files);
                return files.Select(f => $@"\\{fsPath.Hostname}\{fsPath.ShareName}\{f}").ToArray();
            });
        }

        private static void EnumerateSmbEntries(ISMBFileStore fileStore, string path, string searchPattern, SearchOption searchOption, bool directoriesOnly, List<string> directories, List<string> files)
        {
            var status = fileStore.CreateFile(out object handle,
                out FileStatus fileStatus,
                path,
                AccessMask.GENERIC_READ,
                SMBLibrary.FileAttributes.Directory,
                ShareAccess.Read | ShareAccess.Write,
                CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_DIRECTORY_FILE,
                null);

            if (status != NTStatus.STATUS_SUCCESS)
                return;

            try
            {
                List<QueryDirectoryFileInformation> entries;
                status = fileStore.QueryDirectory(out entries, handle, "*", FileInformationClass.FileDirectoryInformation);

                if (status != NTStatus.STATUS_SUCCESS)
                    return;

                foreach (var entry in entries)
                {
                    var info = (FileDirectoryInformation)entry;
                    if (info.FileName == "." || info.FileName == "..")
                        continue;

                    var fullPath = string.IsNullOrEmpty(path) ? info.FileName : Path.Combine(path, info.FileName);

                    if ((info.FileAttributes & SMBLibrary.FileAttributes.Directory) != 0)
                    {
                        if (directories != null)
                            directories.Add(fullPath);

                        if (searchOption == SearchOption.AllDirectories)
                        {
                            EnumerateSmbEntries(fileStore, fullPath, searchPattern, searchOption,
                                directoriesOnly, directories, files);
                        }
                    }
                    else if (!directoriesOnly && files != null)
                    {
                        if (MatchesPattern(info.FileName, searchPattern))
                            files.Add(fullPath);
                    }
                }
            }
            finally
            {
                fileStore.CloseFile(handle);
            }
        }

        private static bool MatchesPattern(string filename, string pattern)
        {
            if (pattern == "*")
                return true;

            pattern = pattern.Replace(".", @"\.");
            pattern = pattern.Replace("*", ".*");
            pattern = pattern.Replace("?", ".");

            return System.Text.RegularExpressions.Regex.IsMatch(filename, $"^{pattern}$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        private static bool SmbDirectoryExists(FileSystemPath fsPath)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            return ExecuteSmbOperation(config, (fileStore) =>
            {
                var status = fileStore.CreateFile(out object handle,
                    out FileStatus fileStatus,
                    fsPath.RelativePath,
                    AccessMask.GENERIC_READ,
                    SMBLibrary.FileAttributes.Directory,
                    ShareAccess.Read | ShareAccess.Write,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_DIRECTORY_FILE,
                    null);

                if (status == NTStatus.STATUS_SUCCESS)
                {
                    fileStore.CloseFile(handle);
                    return true;
                }

                return false;
            });
        }

        private static void CreateSmbDirectory(FileSystemPath fsPath)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            ExecuteSmbOperation(config, (fileStore) =>
            {
                EnsureSmbDirectoryExists(config, fileStore, fsPath.RelativePath);
                return 0;
            });
        }

        private static void EnsureSmbDirectoryExists(SMBConnectionConfig config, ISMBFileStore fileStore, string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            var parts = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var currentPath = "";

            foreach (var part in parts)
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? part : Path.Combine(currentPath, part);

                var status = fileStore.CreateFile(out object handle,
                    out FileStatus fileStatus,
                    currentPath,
                    AccessMask.GENERIC_READ,
                    SMBLibrary.FileAttributes.Directory,
                    ShareAccess.Read | ShareAccess.Write,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_DIRECTORY_FILE,
                    null);

                if (status == NTStatus.STATUS_SUCCESS)
                {
                    fileStore.CloseFile(handle);
                }
                else if (status == NTStatus.STATUS_OBJECT_NAME_NOT_FOUND)
                {
                    status = fileStore.CreateFile(out handle,
                        out fileStatus,
                        currentPath,
                        AccessMask.GENERIC_ALL,
                        SMBLibrary.FileAttributes.Directory,
                        ShareAccess.None,
                        CreateDisposition.FILE_CREATE,
                        CreateOptions.FILE_DIRECTORY_FILE,
                        null);

                    if (status == NTStatus.STATUS_SUCCESS)
                        fileStore.CloseFile(handle);
                    else
                        throw new IOException($"Failed to create directory: {currentPath}, Status: {status}");
                }
            }
        }

        private static FileSystemInfo GetSmbFileInfo(FileSystemPath fsPath)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            return ExecuteSmbOperation(config, (fileStore) =>
            {
                var status = fileStore.CreateFile(out object handle,
                    out FileStatus fileStatus,
                    fsPath.RelativePath,
                    AccessMask.GENERIC_READ,
                    SMBLibrary.FileAttributes.Normal,
                    ShareAccess.Read,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_NON_DIRECTORY_FILE,
                    null);

                if (status != NTStatus.STATUS_SUCCESS)
                    throw new FileNotFoundException($"File not found: {fsPath.RelativePath}");

                status = fileStore.GetFileInformation(out FileInformation fileInfo, handle, FileInformationClass.FileBasicInformation);
                fileStore.CloseFile(handle);

                if (status != NTStatus.STATUS_SUCCESS)
                    throw new IOException($"Failed to get file information: {fsPath.RelativePath}");

                var basicInfo = (FileBasicInformation)fileInfo;
                var fullPath = $@"\\{fsPath.Hostname}\{fsPath.ShareName}\{fsPath.RelativePath}";

                var smbFileInfo = new SmbFileInfo
                {
                    CreationTime = (DateTime)basicInfo.CreationTime.Time,
                    LastWriteTime = (DateTime)basicInfo.LastWriteTime.Time,
                    LastAccessTime = (DateTime)basicInfo.LastAccessTime.Time,
                    Attributes = ConvertAttributes(basicInfo.FileAttributes)
                };
                smbFileInfo.SetName(GetFileName(fsPath.RelativePath));
                smbFileInfo.SetFullName(fullPath);
                return smbFileInfo;
            });
        }

        private static DirectorySystemInfo GetSmbDirectoryInfo(FileSystemPath fsPath)
        {
            var config = GetSMBCredentials(fsPath.Hostname, fsPath.ShareName);

            return ExecuteSmbOperation(config, (fileStore) =>
            {
                var status = fileStore.CreateFile(out object handle,
                    out FileStatus fileStatus,
                    fsPath.RelativePath,
                    AccessMask.GENERIC_READ,
                    SMBLibrary.FileAttributes.Directory,
                    ShareAccess.Read | ShareAccess.Write,
                    CreateDisposition.FILE_OPEN,
                    CreateOptions.FILE_DIRECTORY_FILE,
                    null);

                if (status != NTStatus.STATUS_SUCCESS)
                    throw new DirectoryNotFoundException($"Directory not found: {fsPath.RelativePath}");

                status = fileStore.GetFileInformation(out FileInformation fileInfo, handle, FileInformationClass.FileBasicInformation);
                fileStore.CloseFile(handle);

                if (status != NTStatus.STATUS_SUCCESS)
                    throw new IOException($"Failed to get directory information: {fsPath.RelativePath}");

                var basicInfo = (FileBasicInformation)fileInfo;
                var fullPath = $@"\\{fsPath.Hostname}\{fsPath.ShareName}\{fsPath.RelativePath}";

                return new DirectorySystemInfo
                {
                    FullName = fullPath,
                    Name = GetFileName(fsPath.RelativePath) ?? fsPath.RelativePath,
                    CreationTime = (DateTime)basicInfo.CreationTime.Time,
                    LastWriteTime = (DateTime)basicInfo.LastWriteTime.Time,
                    LastAccessTime = (DateTime)basicInfo.LastAccessTime.Time,
                    Attributes = ConvertAttributes(basicInfo.FileAttributes)
                };
            });
        }

        private static System.IO.FileAttributes ConvertAttributes(SMBLibrary.FileAttributes smbAttributes)
        {
            System.IO.FileAttributes attributes = 0;

            if ((smbAttributes & SMBLibrary.FileAttributes.ReadOnly) != 0)
                attributes |= System.IO.FileAttributes.ReadOnly;
            if ((smbAttributes & SMBLibrary.FileAttributes.Hidden) != 0)
                attributes |= System.IO.FileAttributes.Hidden;
            if ((smbAttributes & SMBLibrary.FileAttributes.System) != 0)
                attributes |= System.IO.FileAttributes.System;
            if ((smbAttributes & SMBLibrary.FileAttributes.Directory) != 0)
                attributes |= System.IO.FileAttributes.Directory;
            if ((smbAttributes & SMBLibrary.FileAttributes.Archive) != 0)
                attributes |= System.IO.FileAttributes.Archive;
            if ((smbAttributes & SMBLibrary.FileAttributes.Normal) != 0)
                attributes |= System.IO.FileAttributes.Normal;
            if ((smbAttributes & SMBLibrary.FileAttributes.Temporary) != 0)
                attributes |= System.IO.FileAttributes.Temporary;
            if ((smbAttributes & SMBLibrary.FileAttributes.Compressed) != 0)
                attributes |= System.IO.FileAttributes.Compressed;
            if ((smbAttributes & SMBLibrary.FileAttributes.Encrypted) != 0)
                attributes |= System.IO.FileAttributes.Encrypted;

            return attributes;
        }

        private static T ExecuteSmbOperation<T>(SMBConnectionConfig config, Func<ISMBFileStore, T> operation)
        {
            SMB2Client client = new();

            try
            {
                bool isConnected = client.Connect(config.Hostname, SMBTransportType.DirectTCPTransport);
                if (!isConnected)
                    throw new IOException($"Failed to connect to SMB server: {config.Hostname}");

                var status = client.Login(config.Domain, config.Username, config.Password);
                if (status != NTStatus.STATUS_SUCCESS)
                    throw new UnauthorizedAccessException($"SMB authentication failed with status: {status}");

                ISMBFileStore fileStore = client.TreeConnect(config.ShareName, out status);
                if (status != NTStatus.STATUS_SUCCESS)
                    throw new IOException($"Failed to connect to share: {config.ShareName}");

                return operation(fileStore);
            }
            finally
            {
                client.Logoff();
                client.Disconnect();
            }
        }
    }   
    
    public static class SMBUsageExample
    {
        public static void DemonstrateFileOperations()
        {
            string smbPath = @"\\svmtawpsbpvdev\workspace_apps\wsBendFW\FundWizardInput\FW_240605104401.xml";
            string content = FileManagerSMB.ReadAllText(smbPath);

            string outputPath = @"\\svmtawpsbpvdev\workspace_apps\wsBendFW\FW_240605104401.xml";
            FileManagerSMB.WriteAllText(outputPath, "Hello from SMB wrapper!");

            string prodSource = @"\\svmtawpsbpvdev\workspace_apps\wsBendFW\FundWizardInput\FW_240605104401.xml";
            string backupDest = @"\\svmtawpsbpvdev\workspace_apps\wsBendFW\FW_240605104402.xml";
            FileManagerSMB.Copy(prodSource, backupDest);
        }
    }
}
