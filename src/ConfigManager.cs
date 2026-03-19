using System;
using System.IO;
using System.Text.Json;

namespace AstralPartyModManager
{
    // 应用程序配置管理器
    public class ConfigManager
    {
        private readonly string _configPath;
        private AppConfig _config;

        public ConfigManager(string configPath)
        {
            _configPath = configPath;
            _config = LoadConfig();
        }

        public string GamePath
        {
            get => _config.GamePath;
            set { _config.GamePath = value; SaveConfig(); }
        }

        public string ModPath
        {
            get => _config.ModPath;
            set { _config.ModPath = value; SaveConfig(); }
        }

        public bool BackupEnabled
        {
            get => _config.BackupEnabled;
            set { _config.BackupEnabled = value; SaveConfig(); }
        }

        public bool AutoDetectConflicts
        {
            get => _config.AutoDetectConflicts;
            set { _config.AutoDetectConflicts = value; SaveConfig(); }
        }

        public bool DebugMode
        {
            get => _config.DebugMode;
            set { _config.DebugMode = value; SaveConfig(); }
        }

        private AppConfig LoadConfig()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    var json = File.ReadAllText(_configPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null)
                    {
                        if (!string.IsNullOrEmpty(config.GamePath) && !Path.IsPathRooted(config.GamePath))
                        {
                            config.GamePath = Path.GetFullPath(Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory, config.GamePath));
                        }
                        if (!string.IsNullOrEmpty(config.ModPath) && !Path.IsPathRooted(config.ModPath))
                        {
                            config.ModPath = Path.GetFullPath(Path.Combine(
                                AppDomain.CurrentDomain.BaseDirectory, config.ModPath));
                        }
                        return config;
                    }
                }
                catch { /* ignore load error */ }
            }

            return new AppConfig
            {
                GamePath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..")),
                ModPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "mods")),
                BackupEnabled = true,
                AutoDetectConflicts = true
            };
        }

        private void SaveConfig()
        {
            try
            {
                var json = JsonSerializer.Serialize(_config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_configPath, json);
            }
            catch { /* ignore save error */ }
        }

        public bool ValidatePaths(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!Directory.Exists(GamePath))
            {
                errorMessage = $"游戏目录不存在：{GamePath}";
                return false;
            }

            if (!Directory.Exists(ModPath))
            {
                errorMessage = $"Mods 目录不存在：{ModPath}";
                return false;
            }

            if (!File.Exists(Path.Combine(GamePath, "AstralParty_CN.exe")))
            {
                errorMessage = "游戏目录不正确：在所选路径中找不到 AstralParty_CN.exe\n\n请选择包含 AstralParty_CN.exe 的游戏根目录";
                return false;
            }

            return true;
        }

        private class AppConfig
        {
            public string GamePath { get; set; } = string.Empty;
            public string ModPath { get; set; } = string.Empty;
            public bool BackupEnabled { get; set; } = true;
            public bool AutoDetectConflicts { get; set; } = true;
            public bool DebugMode { get; set; } = false;
        }
    }
}
