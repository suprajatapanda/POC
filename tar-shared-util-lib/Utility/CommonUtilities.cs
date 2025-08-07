using System.Diagnostics;
using SIUtil;

namespace TARSharedUtilLib.Utility
{
    public static class CommonUtilities
    {
        public static void RunKerberosScript()
        {
            var process = new Process();
            if (OperatingSystem.IsLinux())
            {
                string arguments = "/app/renew-kbr-ticket.sh";

                process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/sh",
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    },
                    EnableRaisingEvents = true
                };

                try
                {
                    process.Exited += (sender, args) =>
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        if (!string.IsNullOrEmpty(error))
                        {
                            Logger.LogMessage("Kerberos script error: " + error, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        }

                        if (!string.IsNullOrEmpty(output))
                        {
                            Logger.LogMessage("Kerberos script output: " + output, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        }

                        if (process.ExitCode == 0)
                        {
                            Logger.LogMessage("SetResult " + process.ExitCode, Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        }
                        else
                        {
                            Logger.LogMessage($"Kerberos command execution failed at {DateTime.UtcNow}.", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        }

                        process.Dispose();
                        Logger.LogMessage($"Kerberos ticket renewal completed at : {DateTime.UtcNow}", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    };

                    try
                    {
                        process.Start();
                        Logger.LogMessage($"Kerberos process started at {DateTime.UtcNow}.", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                        process.WaitForExit();                        
                    }
                    catch (Exception ex) 
                    {
                        Logger.LogMessage($"Kerberos command execution failed at {DateTime.UtcNow}.", Logger.LoggerType.BendProcessor, Logger.LogInfoType.InfoFormat);
                    }
                   
                }
                catch (Exception ex)
                {
                    throw new Exception("Kerberos script execution failed: " + ex.Message, ex);
                }

            }
        }
    }
}
