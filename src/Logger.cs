using System;
using System.IO;

namespace CountdownWidget
{
    internal static class Logger
    {
        private static readonly object _lock = new object();
        private const long MaxLogSizeBytes = 256 * 1024; // 256 KB cap

        private static string GetLogFilePath()
        {
            string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CountdownWidget");
            if (!Directory.Exists(appData)) Directory.CreateDirectory(appData);
            return Path.Combine(appData, "widget.log");
        }

        public static void LogError(string context, Exception ex)
        {
            string message = ex != null ? ex.ToString() : "Unknown error";
            WriteLog("ERROR", string.Format("{0}: {1}", context, message));
        }

        public static void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        private static void WriteLog(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    string logFile = GetLogFilePath();

                    // Roll over if log exceeds cap
                    if (File.Exists(logFile) && new FileInfo(logFile).Length > MaxLogSizeBytes)
                    {
                        string oldLog = Path.Combine(Path.GetDirectoryName(logFile), "widget.old.log");
                        if (File.Exists(oldLog)) File.Delete(oldLog);
                        File.Move(logFile, oldLog);
                    }

                    string entry = string.Format("[{0:yyyy-MM-dd HH:mm:ss}] [{1}] {2}{3}",
                        DateTime.Now, level, message, Environment.NewLine);

                    File.AppendAllText(logFile, entry);
                }
            }
            catch {}
        }
    }
}
