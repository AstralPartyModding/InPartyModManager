// AstralParty Mod Manager - 日志记录
// Copyright (c) AstralParty Modding Community. All rights reserved.

namespace AstralPartyModManager
{
    using System;
    using System.IO;

    // 静态日志记录器
    public static class Logger
    {
        private static readonly string LogPath;
        private static readonly object LockObj = new();

        static Logger()
        {
            LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mod_manager.log");
        }

        public static void Info(string message) => Log("INFO", message);

        public static void Warning(string message) => Log("WARN", message);

        public static void Warning(string message, Exception ex) =>
            Log("WARN", $"{message}: {ex?.Message}");

        public static void Error(string message) => Log("ERROR", message);

        public static void Error(string message, Exception ex) =>
            Log("ERROR", $"{message}: {ex?.Message}{Environment.NewLine}{ex?.StackTrace}");

        public static void Debug(string message) => Log("DEBUG", message);

        private static void Log(string level, string message)
        {
            try
            {
                lock (LockObj)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logLine = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(LogPath, logLine);
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
                lock (LockObj)
                {
                    if (File.Exists(LogPath))
                    {
                        File.Delete(LogPath);
                    }
                }
            }
            catch { }
        }
    }
}
