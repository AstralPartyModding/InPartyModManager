using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AstralPartyModManager.MelonLoader
{
    /// <summary>
    /// Mod扫描器 - MelonLoader版本
    /// 简化版，移除WinForms依赖
    /// </summary>
    public class ModScanner
    {
        private readonly string _modsDirectory;

        public ModScanner(string modsDirectory)
        {
            _modsDirectory = modsDirectory;
        }

        /// <summary>
        /// 扫描所有可用Mod
        /// </summary>
        public List<ModInfo> ScanMods()
        {
            var mods = new List<ModInfo>();

            if (!Directory.Exists(_modsDirectory))
            {
                return mods;
            }

            // 获取所有子目录（每个子目录视为一个Mod）
            var modDirectories = Directory.GetDirectories(_modsDirectory);

            foreach (var modDir in modDirectories)
            {
                try
                {
                    var modInfo = ScanModDirectory(modDir);
                    if (modInfo != null)
                    {
                        mods.Add(modInfo);
                    }
                }
                catch (Exception ex)
                {
                    global::MelonLoader.MelonLogger.Warning($"扫描Mod目录失败: {modDir} - {ex.Message}");
                }
            }

            return mods;
        }

        /// <summary>
        /// 扫描单个Mod目录
        /// </summary>
        private ModInfo? ScanModDirectory(string modDir)
        {
            string modName = Path.GetFileName(modDir);

            // 尝试读取mod.json配置文件
            string configPath = Path.Combine(modDir, "mod.json");
            if (File.Exists(configPath))
            {
                try
                {
                    string json = File.ReadAllText(configPath);
                    var modInfo = JsonSerializer.Deserialize<ModInfo>(json);
                    if (modInfo != null)
                    {
                        modInfo.FolderPath = modDir;
                        modInfo.Name = modInfo.Name ?? modName;
                        return modInfo;
                    }
                }
                catch (Exception ex)
                {
                    global::MelonLoader.MelonLogger.Warning($"读取mod.json失败: {modDir} - {ex.Message}");
                }
            }

            // 如果没有mod.json，根据目录内容推断
            return InferModInfo(modDir);
        }

        /// <summary>
        /// 根据目录内容推断Mod信息
        /// </summary>
        private ModInfo InferModInfo(string modDir)
        {
            string modName = Path.GetFileName(modDir);
            var modInfo = new ModInfo
            {
                Name = modName,
                FolderPath = modDir,
                Version = "1.0.0",
                Author = "未知作者",
                Type = ModType.MelonLoader
            };

            // 扫描资源文件
            var bundleFiles = Directory.GetFiles(modDir, "*.bundle", SearchOption.AllDirectories);
            modInfo.TargetFiles = bundleFiles.Select(f => Path.GetFileName(f)).ToList();

            return modInfo;
        }
    }

    /// <summary>
    /// Mod信息（简化版）
    /// </summary>
    public class ModInfo
    {
        public string Name { get; set; } = "未知 Mod";
        public string Author { get; set; } = "未知作者";
        public string Version { get; set; } = "1.0.0";
        public string Description { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public List<string> TargetFiles { get; set; } = new();
        public ModType Type { get; set; } = ModType.MelonLoader;
        public bool IsDeprecated { get; set; }

        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrEmpty(FolderPath) || !Directory.Exists(FolderPath))
            {
                errorMessage = "Mod 文件夹不存在";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }

    /// <summary>
    /// Mod类型
    /// </summary>
    public enum ModType
    {
        Unknown,
        Addressables,
        Voice,
        Plugin,
        MelonLoader,
        Comprehensive
    }
}
