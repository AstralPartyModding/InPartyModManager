using System;
using System.IO;

namespace AstralPartyModManager
{
    // 静态日志记录器
    public static class Logger
    {
        private static readonly string _logPath;
        private static readonly object _lockObj = new();

        static Logger()
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mod_manager.log");
        }

        public static void Info(string message) => Log("INFO", message);

        public static void Warning(string message) => Log("WARN", message);

        public static void Warning(string message, Exception ex)
            => Log("WARN", $"{message}: {ex?.Message}");

        public static void Error(string message) => Log("ERROR", message);

        public static void Error(string message, Exception ex)
            => Log("ERROR", $"{message}: {ex?.Message}{Environment.NewLine}{ex?.StackTrace}");

        public static void Debug(string message) => Log("DEBUG", message);

        private static void Log(string level, string message)
        {
            try
            {
                lock (_lockObj)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logLine = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logPath, logLine);
                }
            }
            catch
            {
                // 日志失败不影响主程序
            }
        }

        public static void Clear()
        {
            try
            {
                lock (_lockObj)
                {
                    if (File.Exists(_logPath))
                    {
                        File.Delete(_logPath);
                    }
                }
            }
            catch { }
        }
    }
}
