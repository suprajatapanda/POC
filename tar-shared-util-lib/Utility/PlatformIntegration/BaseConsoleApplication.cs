using SIUtil;
using TARSharedUtilLib.Utility;
using TRS.IT.TrsAppSettings;

namespace TRS.IT.SharedLib.PlatformIntegration
{
    public abstract class BaseConsoleApplication
    {
        protected string ApplicationName { get; private set; }
        protected string[] CommandLineArgs { get; private set; }
        public void Run(string[] args)
        {
            try
            {
                CommandLineArgs = args;
                ApplicationName = GetApplicationNameFromArgs(args);
                Logger.LogMessage($"{ApplicationName} starting...");
                AppSettings.Initialize();
                if (RequiresKerberos())
                {
                    SetupKerberos();
                    RunKerberos();
                }

                Logger.LogMessage($"{ApplicationName} started successfully");
                Execute();
                Logger.LogMessage($"{ApplicationName} completed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"{ex.Message} {ex.StackTrace} {ApplicationName} failed",Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
                throw;
            }
        }
        private string GetApplicationNameFromArgs(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Logger.LogMessage("No command line arguments provided, using default application name.");
                return GetApplicationName();
            }
            if (args.Length == 1 && !string.IsNullOrEmpty(args[0]) && args[0] != "null" )
            {
                return args[0];
            }
            if (args.Length > 1)
            {
                Logger.LogMessage("Multiple command line arguments provided, using first argument as application name.");
                return args[0];
            }
            return GetApplicationName();
        }
        private void SetupKerberos()
        {
            try
            {
                var username = AppSettings.GetValue("CyberArkUserName");
                var domain = AppSettings.GetValue("CyberArkDomain");
                var password = AppSettings.GetPassword("CyberArkPassword");

                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    Environment.SetEnvironmentVariable("BEND_FW_USER", $"{username}@{domain.ToUpper()}");
                    Environment.SetEnvironmentVariable("BEND_FW_PWD", password);

                    Logger.LogMessage($"Kerberos environment variables set successfully");
                }
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Failed to setup Kerberos: {ex.Message}",Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
            }
        }

        private void RunKerberos()
        {
            try
            {
                Logger.LogMessage("Executing Kerberos script...");
                CommonUtilities.RunKerberosScript();
                Logger.LogMessage("Kerberos script executed successfully");
            }
            catch (Exception ex)
            {
                Logger.LogMessage($"Kerberos script execution failed: {ex.Message}",Logger.LoggerType.BFL, Logger.LogInfoType.ErrorFormat);
            }
        }

        protected abstract string GetApplicationName();
        protected abstract void Execute();
        protected virtual bool RequiresKerberos() => true;
    }
}