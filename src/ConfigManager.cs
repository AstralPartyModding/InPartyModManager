// AstralParty Mod Manager - 配置管理
// Copyright (c) AstralParty Modding Community. All rights reserved.

namespace AstralPartyModManager
{
    using System;
    using System.IO;
    using System.Text.Json;

    // 应用程序配置管理器
    public class ConfigManager
    {
        private readonly string configPath;
        private AppConfig config;

        public ConfigManager(string configPath)
        {
            this.configPath = configPath;
            this.config = this.LoadConfig();
        }

        public string GamePath
        {
            get => this.config.GamePath;
            set
            {
                this.config.GamePath = value;
                this.SaveConfig();
            }
        }

        public string ModPath
        {
            get => this.config.ModPath;
            set
            {
                this.config.ModPath = value;
                this.SaveConfig();
            }
        }

        public bool BackupEnabled
        {
            get => this.config.BackupEnabled;
            set
            {
                this.config.BackupEnabled = value;
                this.SaveConfig();
            }
        }

        public bool AutoDetectConflicts
        {
            get => this.config.AutoDetectConflicts;
            set
            {
                this.config.AutoDetectConflicts = value;
                this.SaveConfig();
            }
        }

        public bool DebugMode
        {
            get => this.config.DebugMode;
            set
            {
                this.config.DebugMode = value;
                this.SaveConfig();
            }
        }

        private AppConfig LoadConfig()
        {
            if (File.Exists(this.configPath))
            {
                try
                {
                    var json = File.ReadAllText(this.configPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json);
                    if (config != null)
                    {
                        if (
                            !string.IsNullOrEmpty(config.GamePath)
                            && !Path.IsPathRooted(config.GamePath)
                        )
                        {
                            config.GamePath = Path.GetFullPath(
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.GamePath)
                            );
                        }

                        if (
                            !string.IsNullOrEmpty(config.ModPath)
                            && !Path.IsPathRooted(config.ModPath)
                        )
                        {
                            config.ModPath = Path.GetFullPath(
                                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, config.ModPath)
                            );
                        }

                        return config;
                    }
                }
                catch
                { /* ignore load error */
                }
            }

            return new AppConfig
            {
                GamePath = Path.GetFullPath(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..")
                ),
                ModPath = Path.GetFullPath(
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "mods")
                ),
                BackupEnabled = true,
                AutoDetectConflicts = true,
            };
        }

        private void SaveConfig()
        {
            try
            {
                var json = JsonSerializer.Serialize(
                    this.config,
                    new JsonSerializerOptions { WriteIndented = true }
                );
                File.WriteAllText(this.configPath, json);
            }
            catch
            { /* ignore save error */
            }
        }

        public bool ValidatePaths(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (!Directory.Exists(this.GamePath))
            {
                errorMessage = $"游戏目录不存在：{this.GamePath}";
                return false;
            }

            if (!Directory.Exists(this.ModPath))
            {
                errorMessage = $"Mods 目录不存在：{this.ModPath}";
                return false;
            }

            if (!File.Exists(Path.Combine(this.GamePath, "AstralParty_CN.exe")))
            {
                errorMessage =
                    "游戏目录不正确：在所选路径中找不到 AstralParty_CN.exe\n\n请选择包含 AstralParty_CN.exe 的游戏根目录";
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
