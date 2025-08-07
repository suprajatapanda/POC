using System.Net;
using TRS.IT.BendProcessor.Model;

namespace TRS.IT.BendProcessor.Util
{
    public class FTPUtility
    {
        private string _hostname;
        private string _username;
        private string _password;
        private string _currentDirectory = "/";
        private string _lastDirectory = "";
        public FTPUtility(string Hostname, string Username, string Password)
        {
            _hostname = Hostname;
            _username = Username;
            _password = Password;
        }
        public string Hostname
        {
            get
            {
                if (_hostname.StartsWith("ftp://"))
                {
                    return _hostname;
                }
                else
                {
                    return "ftp://" + _hostname;
                }

            }
            set { _hostname = value; }
        }

        public string Username
        {
            get { return (string.IsNullOrEmpty(_username) ? "anonymous" : _username); }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string CurrentDirectory
        {
            get
            {
                return _currentDirectory + ((_currentDirectory.EndsWith("/")) ? "" : "/").ToString();
            }
            set
            {
                if (!value.StartsWith("/"))
                {
                    throw (new ApplicationException("Directory should start with /"));
                }
                _currentDirectory = value;
            }
        }
        public bool UploadFile(string fileName, string targetFilename, ref string sError)
        {
            bool bReturn = false;
            FileInfo ff = new(fileName);
            string target = null;
            if (string.IsNullOrEmpty(targetFilename.Trim()))
            {
                target = this.CurrentDirectory + Path.GetFileName(fileName);
            }
            else if (targetFilename.Contains("/"))
            {
                target = AdjustDir(targetFilename);
            }
            else
            {
                target = CurrentDirectory + targetFilename;
            }

            string URI = Hostname + target;

            FtpWebRequest ftpRequest = GetRequest(URI);

            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            ftpRequest.UseBinary = true;
            ftpRequest.ContentLength = ff.Length;

            const int BufferSize = 2048;
            byte[] content = new byte[BufferSize - 1 + 1];
            int dataRead;

            using (FileStream fs = ff.OpenRead())
            {
                try
                {
                    using (Stream rs = ftpRequest.GetRequestStream())
                    {
                        do
                        {
                            dataRead = fs.Read(content, 0, BufferSize);
                            rs.Write(content, 0, dataRead);
                        } while (!(dataRead < BufferSize));
                        rs.Close();

                        bReturn = true;
                    }
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    fs.Flush();
                    fs.Close();
                    bReturn = false;
                    throw;
                }
                finally
                {
                    if (fs != null)
                        fs.Close();
                }
            }
            ftpRequest = null;

            return bReturn;
        }
        public bool Download(string sourceFilename, string localFilename, bool PermitOverwrite)
        {
            FileInfo targetFI = new(localFilename);
            if (targetFI.Exists && !(PermitOverwrite))
            {
                throw (new ApplicationException("Target file already exists"));
            }

            string target;
            if (sourceFilename.Trim() == "")
            {
                throw (new ApplicationException("File not specified"));
            }
            else if (sourceFilename.Contains("/"))
            {
                target = AdjustDir(sourceFilename);
            }
            else
            {
                target = CurrentDirectory + sourceFilename;
            }

            string URI = Hostname + target;

            FtpWebRequest ftp = GetRequest(URI);

            ftp.Method = WebRequestMethods.Ftp.DownloadFile;
            ftp.UseBinary = true;

            using (FtpWebResponse response = (FtpWebResponse)ftp.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    using (FileStream fs = targetFI.OpenWrite())
                    {
                        try
                        {
                            byte[] buffer = new byte[2048];
                            int read = 0;
                            do
                            {
                                read = responseStream.Read(buffer, 0, buffer.Length);
                                fs.Write(buffer, 0, read);
                            } while (!(read == 0));
                            responseStream.Close();
                            fs.Flush();
                            fs.Close();
                        }
                        catch (Exception ex)
                        {
                            Utils.LogError(ex);
                            fs.Flush();
                            fs.Close();
                            targetFI.Delete();
                            throw;
                        }
                    }
                    responseStream.Close();
                }

                response.Close();
            }

            return true;
        }
        public bool FtpFileExists(string filename)
        {
            try
            {
                long size = GetFileSize(filename);
                return true;
            }
            catch (WebException ex)
            {
                Utils.LogError(ex);
                FtpWebResponse response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

        }
        public long GetFileSize(string filename)
        {
            string path = null;
            if (filename.Contains("/"))
            {
                path = AdjustDir(filename);
            }
            else
            {
                path = CurrentDirectory + filename;
            }

            string URI = Hostname + path;
            FtpWebRequest ftp = GetRequest(URI);
            ftp.Method = WebRequestMethods.Ftp.GetFileSize;

            return GetSize(ftp);

        }
        public bool FtpRename(string sourceFilename, string newName)
        {
            string sErr = "";
            return FtpRename(sourceFilename, newName, ref sErr, false);

        }
        public bool FtpRename(string sourceFilename, string newName, ref string sError, bool bSupressException)
        {
            sError = "";
            bool bReturn = false;
            string source = GetFullPath(sourceFilename);
            string target = GetFullPath(newName);

            if (FtpFileExists(target))
            {
                sError = "Target file " + target + " already exists";
                if (bSupressException == true)
                {
                    return false;
                }
                else
                {
                    throw new ApplicationException(sError);
                }

            }
            string URI = Hostname + source;
            FtpWebRequest ftp = GetRequest(URI);

            string sNet40Workaround = string.Empty;
            string sWorking = source.Substring(0, source.LastIndexOf("/"));
            foreach (char s in sWorking)
            {
                if (s == '/')
                    sNet40Workaround += "../";
            }

            ftp.Method = WebRequestMethods.Ftp.Rename;
            ftp.RenameTo = sNet40Workaround + target;

            try
            {
                string str = GetStringResponse(ftp);
                bReturn = true;
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                bReturn = false;
                sError = "ftpRename failed for: " + target + " Error: " + ex.Message;
                if (bSupressException == true)
                {
                    bReturn = false;
                }
                else
                {
                    throw new Exception(sError);
                }
            }

            return bReturn;
        }
        public bool FtpDelete(string sourceFilename)
        {
            string source = GetFullPath(sourceFilename);

            if (!FtpFileExists(source))
            {
                throw new FileNotFoundException("File " + source + " not found");
            }

            string URI = Hostname + source;
            FtpWebRequest ftp = GetRequest(URI);

            ftp.Method = WebRequestMethods.Ftp.DeleteFile;

            try
            {
                string str = GetStringResponse(ftp);
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                return false;
            }

            return true;

        }
        public bool FtpRenameBlind(string sourceFilename, string newName)
        {
            string source = GetFullPath(sourceFilename);

            string target = GetFullPath(newName);
            if (target == source)
            {
                throw new ApplicationException("Source and target are the same");
            }

            string URI = Hostname + source;
            FtpWebRequest ftp = GetRequest(URI);

            ftp.Method = WebRequestMethods.Ftp.Rename;
            ftp.RenameTo = target;

            string str = GetStringResponse(ftp);


            return true;

        }
        public bool FtpCreateDirectory(string dirpath, ref string sError)
        {
            string URI = Hostname + AdjustDir(dirpath);
            FtpWebRequest ftp = GetRequest(URI);
            ftp.Method = WebRequestMethods.Ftp.MakeDirectory;
            try
            {
                string str = GetStringResponse(ftp);
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                sError = ex.Message;
                return false;
            }
            return true;
        }
        public List<string> ListDirectory(string directory)
        {
            FtpWebRequest ftp = GetRequest(GetDirectory(directory));
            ftp.Method = WebRequestMethods.Ftp.ListDirectory;

            string str = GetStringResponse(ftp);
            str = str.Replace("\r\n", "\r").TrimEnd('\r');
            List<string> result = new();
            if (!string.IsNullOrEmpty(str))
            {
                result.AddRange(str.Split('\r'));
            }
            return result;
        }
        public FTPdirectory ListDirectoryDetail(string directory)
        {
            FtpWebRequest ftp = GetRequest(GetDirectory(directory));
            ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            string str = GetStringResponse(ftp);
            str = str.Replace("\r\n", "\r").TrimEnd('\r');
            return new FTPdirectory(str, _lastDirectory);
        }
        public ResultReturn MoveFtpFile(FTPfileInfo a_ff, string a_sTargetFtpFolder, string a_sFtpErrorFolder, bool a_bOverWriteExistingFile)
        {

            ResultReturn oReturn = new();
            oReturn.returnStatus = ReturnStatusEnum.Succeeded;

            string sErrorInfo = "";
            string sErr = "";
            bool bTemp = false;
            bool bMovedOk = false;

            try
            {
                bMovedOk = FtpRename(a_ff.FullName, Path.Combine(a_sTargetFtpFolder, a_ff.Filename));
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                if (ex.Message.Contains("already exists"))
                {
                    sErrorInfo = "";
                    if (IsDirectoryExist(a_sFtpErrorFolder, true, ref sErrorInfo))
                    {
                        string sDuplicateFolder = a_sFtpErrorFolder + "/Duplicate";
                        sErrorInfo = "";
                        if (IsDirectoryExist(sDuplicateFolder, true, ref sErrorInfo))
                        {
                            if (a_bOverWriteExistingFile == true)
                            {
                                bTemp = FtpRename(Path.Combine(a_sTargetFtpFolder, a_ff.Filename), Path.Combine(sDuplicateFolder, a_ff.Filename), ref sErr, true);
                                if (!bTemp)
                                {
                                    if (sErr.Contains("already exists"))
                                    {
                                        bTemp = this.FtpRename(a_sTargetFtpFolder + a_ff.Filename, sDuplicateFolder + "/" + a_ff.NameOnly + DateTime.Now.ToString("_HHmmss.") + a_ff.Extension, ref sErr, true);
                                    }
                                    if (!bTemp) oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder. While Processing File: " + a_ff.Filename, ErrorSeverityEnum.Failed));
                                }

                                if (bTemp)
                                {
                                    bMovedOk = this.FtpRename(a_ff.FullName, Path.Combine(a_sTargetFtpFolder, a_ff.Filename), ref sErr, true);
                                    if (!bMovedOk)
                                        oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to target folder.  While Processing File: " + a_ff.Filename, ErrorSeverityEnum.Failed));
                                    else
                                    {
                                        oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " file already existed and was replaced.  File name: " + a_ff.Filename, ErrorSeverityEnum.Warning));
                                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                                        return oReturn;
                                    }
                                }
                            }
                            else
                            {
                                bTemp = FtpRename(a_ff.FullName, Path.Combine(sDuplicateFolder, a_ff.Filename), ref sErr, true);
                                if ((!bTemp) && (sErr.Contains("already exists")))
                                {
                                    bTemp = FtpRename(a_ff.FullName, Path.Combine(sDuplicateFolder, a_ff.NameOnly + DateTime.Now.ToString("_HHmmss.") + a_ff.Extension), ref sErr, true);
                                    if (!bTemp)
                                    {
                                        oReturn.Errors.Add(new ErrorInfo(-1, a_ff.FullName + " could not move to error folder (Overwrite is set to No). While Processing File: " + a_ff.Filename, ErrorSeverityEnum.Failed));
                                        oReturn.returnStatus = ReturnStatusEnum.Failed;
                                    }
                                }
                            }

                        }
                        else
                        {
                            oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + sDuplicateFolder + "  While Processing File: " + a_ff.Filename + " Error: " + sErrorInfo, ErrorSeverityEnum.Failed));
                        }
                    }
                    else
                    {
                        oReturn.Errors.Add(new ErrorInfo(-1, "Cannot create folder: " + a_sFtpErrorFolder + "  While Processing File: " + a_ff.Filename + " Error: " + sErrorInfo, ErrorSeverityEnum.Failed));
                    }
                }
                else
                {
                    oReturn.Errors.Add(new ErrorInfo(-1, "Exception  While Processing File: " + a_ff.Filename + "   Exception Msg: " + ex.Message, ErrorSeverityEnum.Failed));
                }

            }

            if (!bMovedOk)
            {
                oReturn.returnStatus = ReturnStatusEnum.Failed;
            }
            else
                oReturn.returnStatus = ReturnStatusEnum.Succeeded;


            return oReturn;

        }
        public FtpFolderTracking GetFtpFolderInfo(string a_sRootFolder)
        {
            List<string> directories = new();
            string sError = "";
            FtpFolderTracking oFtpFolder = new();
            oFtpFolder.rootFolder = a_sRootFolder;
            oFtpFolder.yearFolder = DateTime.Now.Year.ToString();
            oFtpFolder.maxLimit = Convert.ToInt32(TrsAppSettings.AppSettings.GetValue("MaxFilesPerFolder"));

            if (IsDirectoryExist(oFtpFolder.rootFolder + oFtpFolder.yearFolder, true, ref sError))
            {
                directories = ListDirectory(oFtpFolder.rootFolder + oFtpFolder.yearFolder);
                if (directories.Count > 0)
                    oFtpFolder.subFolderCount = directories.Count - 1;
                else
                    oFtpFolder.subFolderCount = 0;
                if (CreateFtpSubFolder(oFtpFolder, ref sError))
                {
                    if (!oFtpFolder.IsUnderLimit)
                    {
                        oFtpFolder.subFolderCount++;
                        if (CreateFtpSubFolder(oFtpFolder, ref sError))
                            oFtpFolder.returnStatus = ReturnStatusEnum.Succeeded;
                        else
                        {
                            oFtpFolder.returnStatus = ReturnStatusEnum.Failed;
                            oFtpFolder.errors.Add(new ErrorInfo(-1, "Folder: " + oFtpFolder.yearFolder + " Error: " + sError, ErrorSeverityEnum.Failed));
                        }

                    }
                    else
                        oFtpFolder.returnStatus = ReturnStatusEnum.Succeeded;
                }
                else
                {
                    oFtpFolder.returnStatus = ReturnStatusEnum.Failed;
                    oFtpFolder.errors.Add(new ErrorInfo(-1, "Folder: " + oFtpFolder.yearFolder + " Error: " + sError, ErrorSeverityEnum.Failed));

                }
            }
            else
            {
                oFtpFolder.returnStatus = ReturnStatusEnum.Failed;
                oFtpFolder.errors.Add(new ErrorInfo(-1, "Folder: " + oFtpFolder.yearFolder + " Error: " + sError, ErrorSeverityEnum.Failed));
            }
            return oFtpFolder;
        }
        private bool CreateFtpSubFolder(FtpFolderTracking a_oFtpFolder, ref string sError)
        {
            bool bOk = false;
            a_oFtpFolder.currentFolder = a_oFtpFolder.subFolderCount.ToString("00000");
            if (IsDirectoryExist(a_oFtpFolder.GetCurrentFolder, true, ref sError))
            {
                FTPdirectory oDir = ListDirectoryDetail(a_oFtpFolder.GetCurrentFolder);
                a_oFtpFolder.initialCount = oDir.Count;
                a_oFtpFolder.runningCount = a_oFtpFolder.initialCount;
                bOk = true;
            }
            else
                throw new Exception("Could not create directoty: " + a_oFtpFolder.currentFolder + "0/  Error: " + sError);

            return bOk;
        }
        public bool IsDirectoryExist(string a_sDirectory, bool a_bCreate, ref string a_sError)
        {

            bool bReturn = false;
            bool bIsGood;
            List<string> result = new();
            try
            {
                try
                {
                    result = ListDirectory(a_sDirectory);
                    bReturn = true;
                }
                catch (Exception ex)
                {
                    Utils.LogError(ex);
                    bReturn = false;
                    a_sError = ex.Message;
                }

                if (!bReturn && a_bCreate)
                {
                    bIsGood = FtpCreateDirectory(a_sDirectory, ref a_sError);
                    bReturn = bIsGood;
                }
            }
            catch (Exception ex)
            {
                Utils.LogError(ex);
                bReturn = false;
                a_sError = ex.Message;
            }
            return bReturn;
        }
        private FtpWebRequest GetRequest(string URI)
        {
            Uri uri = new(URI);
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(uri);
            ftpRequest.Credentials = GetCredentials();
            ftpRequest.KeepAlive = false;
            ftpRequest.UsePassive = true;
            return ftpRequest;
        }
        private NetworkCredential GetCredentials()
        {
            return new NetworkCredential(Username, Password);
        }
        private string AdjustDir(string path)
        {
            return Convert.ToString((path.StartsWith("/") ? "" : "/")) + path;
        }
        private string GetDirectory(string directory)
        {
            string URI;
            if (directory == "")
            {
                URI = Hostname + CurrentDirectory;
                _lastDirectory = CurrentDirectory;
            }
            else
            {
                directory = ((directory.StartsWith("/")) ? "" : "/").ToString() + directory;
                directory = directory + ((directory.EndsWith("/")) ? "" : "/").ToString();

                URI = Hostname + directory;
                _lastDirectory = directory;
            }
            return URI;
        }
        private string GetFullPath(string file)
        {
            if (file.Contains("/"))
            {
                return AdjustDir(file);
            }
            else
            {
                return CurrentDirectory + file;
            }
        }
        private string GetStringResponse(FtpWebRequest ftpRequest)
        {
            string result = "";
            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
            {
                long size = response.ContentLength;
                using (Stream datastream = response.GetResponseStream())
                {
                    using (StreamReader sr = new(datastream))
                    {
                        result = sr.ReadToEnd();
                        sr.Close();
                    }
                    datastream.Close();
                }
                response.Close();
            }

            return result;
        }
        private long GetSize(FtpWebRequest ftpRequest)
        {
            long size = 0;
            using (FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse())
            {
                size = response.ContentLength;
                response.Close();
            }
            return size;
        }
    }
}
