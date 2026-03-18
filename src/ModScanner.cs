using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AstralPartyModManager
{
    // Mod 信息
    public class ModInfo
    {
        public string Name { get; set; } = "未知 Mod";
        public string Author { get; set; } = "未知作者";
        public string Version { get; set; } = "1.0.0";
        public string Description { get; set; } = string.Empty;
        public string FolderPath { get; set; } = string.Empty;
        public List<string> TargetFiles { get; set; } = new();
        public ModType Type { get; set; } = ModType.Unknown;
        public List<string> Conflicts { get; set; } = new();
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;
        public bool IsDeprecated { get; set; }
        public string DeprecatedReason { get; set; } = string.Empty;
        public string GameVersion { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public string ScanError { get; set; } = string.Empty;

        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrEmpty(FolderPath) || !Directory.Exists(FolderPath))
            {
                errorMessage = "Mod 文件夹不存在";
                return false;
            }

            if (Type == ModType.Unknown)
            {
                errorMessage = "不支持的 Mod 类型（视为已弃用）";
                IsDeprecated = true;
                DeprecatedReason = "不支持的 Mod 类型";
                return false;
            }

            if (TargetFiles == null || TargetFiles.Count == 0)
            {
                errorMessage = "Mod 没有目标文件";
                return false;
            }

            errorMessage = "";
            return true;
        }
    }

    public enum ModType
    {
        Unknown,
        Addressables,
        Voice,
        Plugin,
        Comprehensive
    }

    // 扫描结果
    public class ScanResult
    {
        public List<ModInfo> Mods { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int TotalFolders { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
    }

    // Mod 扫描器
    public class ModScanner
    {
        private readonly string _modPath;

        public ModScanner(string modPath)
        {
            _modPath = modPath;
        }

        public List<ModInfo> ScanMods()
        {
            var result = ScanModsDetailed();
            return result.Mods;
        }

        public ScanResult ScanModsDetailed()
        {
            var result = new ScanResult();
            Logger.Info($"开始扫描 Mod 目录：{_modPath}");

            if (!Directory.Exists(_modPath))
            {
                Logger.Error($"Mod 目录不存在：{_modPath}");
                result.Errors.Add($"Mod 目录不存在：{_modPath}");
                return result;
            }

            string[] directories;
            try
            {
                directories = Directory.GetDirectories(_modPath);
            }
            catch (Exception ex)
            {
                Logger.Error($"无法读取 Mod 目录", ex);
                result.Errors.Add($"无法读取 Mod 目录：{ex.Message}");
                return result;
            }

            result.TotalFolders = directories.Length;
            Logger.Info($"发现 {directories.Length} 个 Mod 文件夹");

            foreach (var dir in directories)
            {
                try
                {
                    var modInfo = ParseModFolder(dir);
                    if (modInfo != null)
                    {
                        if (modInfo.IsValid(out string errorMsg))
                        {
                            result.Mods.Add(modInfo);
                            result.SuccessCount++;
                        }
                        else
                        {
                            modInfo.ScanError = errorMsg;
                            result.Errors.Add($"Mod '{modInfo.Name}' 无效：{errorMsg}");
                            result.FailedCount++;
                            Logger.Warning($"Mod '{modInfo.Name}' 无效：{errorMsg}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    string modName = Path.GetFileName(dir);
                    result.Errors.Add($"解析 Mod '{modName}' 失败：{ex.Message}");
                    Logger.Error($"解析 Mod 文件夹失败：{dir}", ex);
                }
            }

            Logger.Info($"扫描完成：成功 {result.SuccessCount} 个，失败 {result.FailedCount} 个");
            return result;
        }

        private ModInfo ParseModFolder(string folderPath)
        {
            var modInfo = new ModInfo
            {
                FolderPath = folderPath,
                Name = Path.GetFileName(folderPath)
            };

            try
            {
                modInfo.UpdateTime = Directory.GetLastWriteTime(folderPath);
            }
            catch (Exception ex)
            {
                Logger.Warning($"无法获取文件夹修改时间：{folderPath}", ex);
            }

            var modJsonPath = Path.Combine(folderPath, "mod.json");
            if (File.Exists(modJsonPath))
            {
                try
                {
                    ParseModJson(modJsonPath, modInfo);
                }
                catch (Exception ex)
                {
                    Logger.Warning($"解析 mod.json 失败：{modJsonPath}", ex);
                    modInfo.ScanError = $"mod.json 解析失败：{ex.Message}";
                }
            }
            else
            {
                InferModInfo(folderPath, modInfo);
            }

            ScanTargetFiles(folderPath, modInfo);
            ValidateTargetFiles(modInfo);

            return modInfo;
        }

        private void ParseModJson(string jsonPath, ModInfo modInfo)
        {
            string json = File.ReadAllText(jsonPath, System.Text.Encoding.UTF8);
            string jsonWithoutComments = System.Text.RegularExpressions.Regex.Replace(json, @"//.*$", "", System.Text.RegularExpressions.RegexOptions.Multiline);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) }
            };

            try
            {
                var tempMod = JsonSerializer.Deserialize<ModInfo>(jsonWithoutComments, options);
                if (tempMod != null)
                {
                    modInfo.Name = string.IsNullOrEmpty(tempMod.Name) ? modInfo.Name : tempMod.Name;
                    modInfo.Author = tempMod.Author ?? modInfo.Author;
                    modInfo.Version = tempMod.Version ?? modInfo.Version;
                    modInfo.Description = tempMod.Description ?? modInfo.Description;
                    modInfo.Type = tempMod.Type != ModType.Unknown ? tempMod.Type : modInfo.Type;
                    modInfo.GameVersion = tempMod.GameVersion ?? modInfo.GameVersion;
                    modInfo.Tags = tempMod.Tags ?? new List<string>();
                    modInfo.Conflicts = tempMod.Conflicts ?? new List<string>();

                    if (tempMod.TargetFiles != null && tempMod.TargetFiles.Count > 0)
                    {
                        modInfo.TargetFiles = tempMod.TargetFiles;
                    }

                    modInfo.IsDeprecated = tempMod.IsDeprecated ||
                        (tempMod.Tags != null && tempMod.Tags.Contains("deprecated"));
                    modInfo.DeprecatedReason = tempMod.DeprecatedReason;
                }
            }
            catch (JsonException ex)
            {
                Logger.Warning($"mod.json 格式错误：{jsonPath}，错误详情：{ex.Message}");
            }
        }

        private void InferModInfo(string folderPath, ModInfo modInfo)
        {
            var readmePatterns = new[] { "*.txt", "*.md" };
            foreach (var pattern in readmePatterns)
            {
                var readmePath = Directory.GetFiles(folderPath, pattern)
                    .FirstOrDefault(f => f.Contains("说明") || f.Contains("食用方法") || f.Contains("README"));

                if (readmePath != null)
                {
                    try
                    {
                        var content = File.ReadAllText(readmePath);
                        var authorMatch = System.Text.RegularExpressions.Regex.Match(content, @"(?:作者 | 制作 | By)[:：]\s*(.+)");
                        if (authorMatch.Success)
                        {
                            modInfo.Author = authorMatch.Groups[1].Value.Trim();
                        }

                        var versionMatch = System.Text.RegularExpressions.Regex.Match(content, @"[vV](\d+\.\d+\.\d+)");
                        if (versionMatch.Success)
                        {
                            modInfo.Version = versionMatch.Groups[1].Value;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"读取说明文件失败：{readmePath}", ex);
                    }
                    break;
                }
            }

            InferModType(folderPath, modInfo);
        }

        private void InferModType(string folderPath, ModInfo modInfo)
        {
            string folderName = Path.GetFileName(folderPath).ToLower();

            if (Directory.Exists(Path.Combine(folderPath, "AstralParty")))
            {
                var astralDir = Path.Combine(folderPath, "AstralParty");

                if (Directory.Exists(Path.Combine(astralDir, "StreamingAssets")))
                {
                    modInfo.Type = ModType.Addressables;
                    return;
                }
                if (Directory.Exists(Path.Combine(astralDir, "Plugins")))
                {
                    modInfo.Type = ModType.Plugin;
                    return;
                }

                var dllFiles = Directory.GetFiles(astralDir, "*.dll", SearchOption.TopDirectoryOnly);
                if (dllFiles.Length > 0)
                {
                    modInfo.Type = ModType.Plugin;
                    return;
                }
            }

            if (Directory.Exists(Path.Combine(folderPath, "AppData")))
            {
                modInfo.Type = ModType.Voice;
                return;
            }

            if (folderName.Contains("语音") || folderName.Contains("voice"))
            {
                modInfo.Type = ModType.Voice;
                return;
            }

            if (folderName.Contains("加速") || folderName.Contains("插件") || folderName.Contains("plugin"))
            {
                modInfo.Type = ModType.Plugin;
                return;
            }

            if (Directory.GetFiles(folderPath, "*.bundle", SearchOption.AllDirectories).Length > 0)
            {
                modInfo.Type = ModType.Addressables;
                return;
            }

            if (Directory.GetFiles(folderPath, "*.dll", SearchOption.AllDirectories).Length > 0)
            {
                modInfo.Type = ModType.Plugin;
                return;
            }

            modInfo.Type = ModType.Unknown;
        }

        private void ScanTargetFiles(string folderPath, ModInfo modInfo)
        {
            modInfo.TargetFiles.Clear();
            var subdirs = Directory.GetDirectories(folderPath);

            foreach (var subdir in subdirs)
            {
                var subdirName = Path.GetFileName(subdir).ToLower();

                if (subdirName == "astralparty" || subdirName == "appdata")
                {
                    ScanStandardDirectory(subdir, modInfo);
                }
                else if (subdirName.Contains("模组") || subdirName.Contains("根模组") || subdirName.Contains("根目录替换"))
                {
                    ScanDirectoryToTargetFiles(subdir, modInfo);
                }
                else if (subdirName.Contains("纯语音替换") || subdirName.Contains("适配动画的语音替换") || subdirName.Contains("__data"))
                {
                    modInfo.Type = ModType.Voice;
                    ScanDirectoryToTargetFiles(subdir, modInfo);
                }
            }

            if (modInfo.TargetFiles.Count == 0)
            {
                ScanModRootToTargetFiles(folderPath, modInfo);
            }
        }

        private void ScanDirectoryToTargetFiles(string dirPath, ModInfo modInfo)
        {
            try
            {
                var files = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file).ToLower();
                    if (fileName == "mod.json" || fileName == "readme.md" || fileName == "readme.txt")
                    {
                        continue;
                    }
                    if (fileName.EndsWith(".txt") && 
                        (fileName.Contains("使用") || fileName.Contains("说明") || fileName.Contains("方法")))
                    {
                        continue;
                    }
                    modInfo.TargetFiles.Add(file);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"扫描目录失败：{dirPath}", ex);
            }
        }

        private void ScanModRootToTargetFiles(string folderPath, ModInfo modInfo)
        {
            try
            {
                var files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file).ToLower();
                    if (fileName == "mod.json" || fileName == "readme.md" || fileName == "readme.txt")
                    {
                        continue;
                    }
                    if (file.Contains("\\docs\\"))
                    {
                        continue;
                    }
                    if (fileName.EndsWith(".txt") && 
                        (fileName.Contains("使用") || fileName.Contains("说明") || fileName.Contains("方法")))
                    {
                        continue;
                    }
                    if (IsValidGameFile(fileName))
                    {
                        modInfo.TargetFiles.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"扫描根目录失败：{folderPath}", ex);
            }
        }

        private bool IsValidGameFile(string fileName)
        {
            var validExtensions = new[] { ".bundle", ".dll", ".json", ".bytes", ".png", ".jpg", ".jpeg", ".wav", ".ogg", ".mp3" };
            return validExtensions.Any(ext => fileName.EndsWith(ext));
        }

        private void ScanStandardDirectory(string dirPath, ModInfo modInfo)
        {
            try
            {
                var files = Directory.GetFiles(dirPath, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    string fileName = Path.GetFileName(file).ToLower();
                    if (fileName == "mod.json" || fileName == "readme.md" || fileName == "readme.txt")
                    {
                        continue;
                    }
                    if (fileName.EndsWith(".txt") && (fileName.Contains("使用") || 
                        fileName.Contains("说明") || fileName.Contains("方法")))
                    {
                        continue;
                    }
                    modInfo.TargetFiles.Add(file);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"扫描标准目录失败：{dirPath}", ex);
            }
        }

        private void ValidateTargetFiles(ModInfo modInfo)
        {
            var missingFiles = modInfo.TargetFiles.Where(f => !File.Exists(f)).ToList();
            if (missingFiles.Count > 0)
            {
                Logger.Warning($"Mod '{modInfo.Name}' 有 {missingFiles.Count} 个文件不存在");
                modInfo.TargetFiles = modInfo.TargetFiles.Where(f => File.Exists(f)).ToList();
            }
        }
    }
}
