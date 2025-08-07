namespace SIUtil
{
    public static class Logger
    {
        public enum LoggerType
        {
            BFL = 1,
            BendProcessor = 9
        }

        public enum LogInfoType
        {
            InfoFormat = 1,
            ErrorFormat = 2,
            DebugFormat = 3
        }

        private static readonly object _lockObject = new object();

        public static void LogMessage(string message, LoggerType logType = LoggerType.BFL, LogInfoType logInfoType = LogInfoType.InfoFormat)
        {
            var level = logInfoType switch
            {
                LogInfoType.DebugFormat => "DEBUG",
                LogInfoType.ErrorFormat => "ERROR",
                _ => "INFO"
            };

            WriteLog(level, logType.ToString(), message);
        }
        public static void LogDebug(string message, params object[] args)
        {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            WriteLog("DEBUG", "PlatformCore", formattedMessage);
        }

        public static void LogInformation(string message, params object[] args)
        {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            WriteLog("INFO", "PlatformCore", formattedMessage);
        }

        public static void LogWarning(string message, params object[] args)
        {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            WriteLog("WARN", "PlatformCore", formattedMessage);
        }

        public static void LogError(string message, params object[] args)
        {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            WriteLog("ERROR", "PlatformCore", formattedMessage);
        }

        public static void LogError(Exception ex, string message, params object[] args)
        {
            var formattedMessage = args.Length > 0 ? string.Format(message, args) : message;
            WriteLog("ERROR", "PlatformCore", formattedMessage);

            if (ex != null)
            {
                WriteLog("ERROR", "PlatformCore", $"Exception: {ex.GetType().Name}: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    WriteLog("ERROR", "PlatformCore", $"StackTrace: {ex.StackTrace}");
                }
                var innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    WriteLog("ERROR", "PlatformCore", $"Inner Exception: {innerEx.GetType().Name}: {innerEx.Message}");
                    innerEx = innerEx.InnerException;
                }
            }
        }
        private static void WriteLog(string level, string source, string message)
        {
            lock (_lockObject)
            {
                try
                {
                    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var output = $"[{timestamp} UTC] [{level}] [{source}] {message}";

                    Console.WriteLine(output);
                    Console.Out.Flush();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[LOGGER ERROR] Failed to log: {ex.Message}");
                    Console.WriteLine($"[LOGGER ERROR] Original message: {message}");
                }
            }
        }
    }
}
