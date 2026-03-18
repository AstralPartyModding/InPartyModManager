using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AstralPartyModManager
{
    // 安装结果记录
    public record InstallResult(
        bool Success,
        string Message,
        List<string> InstalledFiles,
        List<string> ReplacedFiles,
        List<string> Errors
    )
    {
        public InstallResult() : this(false, "", new(), new(), new()) { }
    }

    // Mod 安装管理器
    public class ModManager
    {
        private readonly string _gamePath;
        private readonly string _dataPath;

        public ModManager(string gamePath, string dataPath)
        {
            _gamePath = gamePath;
            _dataPath = dataPath;
            Logger.Info($"ModManager 初始化 - 游戏路径：{gamePath}, 数据路径：{dataPath}");
        }

        public InstallResult EnableMod(ModInfo modInfo)
        {
            var result = new InstallResult();
            Logger.Info($"开始启用 Mod: {modInfo.Name} (类型：{modInfo.Type})");

            if (!modInfo.IsValid(out string errorMsg))
            {
                return result with { Success = false, Message = $"Mod 信息无效：{errorMsg}" };
            }

            if (!Directory.Exists(_gamePath))
            {
                return result with { Success = false, Message = $"游戏目录不存在：{_gamePath}" };
            }

            if (!Directory.Exists(modInfo.FolderPath))
            {
                return result with { Success = false, Message = $"Mod 文件夹不存在：{modInfo.FolderPath}" };
            }

            try
            {
                switch (modInfo.Type)
                {
                    case ModType.Addressables:
                        result = InstallAddressablesMod(modInfo);
                        break;
                    case ModType.Voice:
                        result = InstallVoiceMod(modInfo);
                        break;
                    case ModType.Plugin:
                        result = InstallPluginMod(modInfo);
                        break;
                    default:
                        result = InstallGenericMod(modInfo);
                        break;
                }

                if (result.Success)
                {
                    Logger.Info($"Mod '{modInfo.Name}' 启用成功，安装了 {result.InstalledFiles.Count} 个文件");
                }
                else
                {
                    Logger.Error($"Mod '{modInfo.Name}' 启用失败：{result.Message}");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result = result with
                {
                    Success = false,
                    Message = $"安装过程中发生错误：{ex.Message}"
                };
                Logger.Error($"Mod '{modInfo.Name}' 安装失败", ex);
            }

            return result;
        }

        public void DisableMod(ModInfo modInfo)
        {
            Logger.Info($"准备禁用 Mod: {modInfo.Name}");
        }

        private InstallResult InstallAddressablesMod(ModInfo modInfo)
        {
            var result = new InstallResult();
            string targetDir = Path.Combine(_dataPath, "StreamingAssets", "aa", "StandaloneWindows64");

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            var standardDir = FindStandardDirectory(modInfo.FolderPath, "astralparty");
            if (standardDir != null)
            {
                string streamingAssetsSource = Path.Combine(standardDir, "StreamingAssets", "aa", "StandaloneWindows64");
                if (Directory.Exists(streamingAssetsSource))
                {
                    CopyModFiles(streamingAssetsSource, targetDir, result);
                    return result;
                }

                var subDirs = Directory.GetDirectories(standardDir);
                foreach (var subDir in subDirs)
                {
                    CopyModFiles(subDir, targetDir, result);
                }
                return result;
            }

            var modSubdir = FindModSubdirectory(modInfo.FolderPath);
            if (modSubdir != null)
            {
                CopyModFiles(modSubdir, targetDir, result);
            }
            else
            {
                CopyModFiles(modInfo.FolderPath, targetDir, result);
            }

            return result;
        }

        private InstallResult InstallVoiceMod(ModInfo modInfo)
        {
            var result = new InstallResult();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string targetDir = Path.Combine(appData, "Low", "feimo", "AstralParty_CN",
                "com.unity.addressables", "AssetBundles");

            var standardDir = FindStandardDirectory(modInfo.FolderPath, "appdata");
            if (standardDir != null)
            {
                string appDataBaseSource = Path.Combine(standardDir, "Low", "feimo", "AstralParty_CN");
                if (Directory.Exists(appDataBaseSource))
                {
                    CopyModFiles(appDataBaseSource, Path.Combine(appData, "Low", "feimo", "AstralParty_CN"), result);
                    return result;
                }

                var subDirs = Directory.GetDirectories(standardDir);
                foreach (var subDir in subDirs)
                {
                    string relativePath = Path.GetRelativePath(standardDir, subDir);
                    string destDir = Path.Combine(appData, relativePath);
                    CopyModFiles(subDir, destDir, result);
                }
                return result;
            }

            var voiceSubdir = FindVoiceSubdirectory(modInfo.FolderPath);
            if (voiceSubdir != null)
            {
                CopyModFiles(voiceSubdir, targetDir, result);
            }
            else
            {
                CopyModFiles(modInfo.FolderPath, targetDir, result);
            }

            return result;
        }

        private InstallResult InstallPluginMod(ModInfo modInfo)
        {
            var result = new InstallResult();

            var standardDir = FindStandardDirectory(modInfo.FolderPath, "astralparty");
            if (standardDir != null)
            {
                var pluginsDir = Path.Combine(standardDir, "Plugins");
                if (Directory.Exists(pluginsDir))
                {
                    CopyModFiles(pluginsDir, _gamePath, result);
                    return result;
                }

                CopyModFiles(standardDir, _gamePath, result);
                return result;
            }

            CopyModFiles(modInfo.FolderPath, _gamePath, result);
            return result;
        }

        private InstallResult InstallGenericMod(ModInfo modInfo)
        {
            var result = new InstallResult();
            var bundleFiles = modInfo.TargetFiles.Where(f => f.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase)).ToList();

            if (bundleFiles.Count > 0)
            {
                return InstallAddressablesMod(modInfo);
            }

            return InstallPluginMod(modInfo);
        }

        private string FindModSubdirectory(string folderPath)
        {
            var priorityNames = new[] { "模组", "根模组", "根目录替换", "StandaloneWindows64" };
            foreach (var name in priorityNames)
            {
                var subdir = Directory.GetDirectories(folderPath)
                    .FirstOrDefault(d => Path.GetFileName(d).Contains(name));
                if (subdir != null)
                {
                    return subdir;
                }
            }
            return null;
        }

        private string FindVoiceSubdirectory(string folderPath)
        {
            var priorityNames = new[] { "纯语音替换", "适配动画的语音替换", "__data" };
            foreach (var name in priorityNames)
            {
                var subdir = Directory.GetDirectories(folderPath)
                    .FirstOrDefault(d => Path.GetFileName(d).Contains(name));
                if (subdir != null)
                {
                    var dataSubdir = Directory.GetDirectories(subdir).FirstOrDefault();
                    return dataSubdir ?? subdir;
                }
            }
            return null;
        }

        private string FindStandardDirectory(string folderPath, string dirName)
        {
            try
            {
                return Directory.GetDirectories(folderPath)
                    .FirstOrDefault(d => Path.GetFileName(d).ToLower() == dirName);
            }
            catch (Exception ex)
            {
                Logger.Warning($"查找标准目录失败：{folderPath}", ex);
                return null;
            }
        }

        private void CopyModFiles(string sourceDir, string targetDir, InstallResult result)
        {
            if (!Directory.Exists(sourceDir))
            {
                result = result with { Success = false, Message = $"源目录不存在：{sourceDir}" };
                result.Errors.Add(result.Message);
                return;
            }

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                try
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

                    string relativePath = Path.GetRelativePath(sourceDir, file);
                    string destFile = Path.Combine(targetDir, relativePath);

                    string destDir = Path.GetDirectoryName(destFile);
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    bool isReplace = File.Exists(destFile);
                    if (isReplace)
                    {
                        result.ReplacedFiles.Add(relativePath);
                    }
                    else
                    {
                        result.InstalledFiles.Add(relativePath);
                    }

                    File.Copy(file, destFile, overwrite: true);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"复制文件失败 {file}: {ex.Message}");
                    Logger.Warning($"复制文件失败：{file}", ex);
                }
            }

            result = result with
            {
                Success = result.Errors.Count == 0,
                Message = result.Errors.Count == 0
                    ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                    : $"安装完成，但有 {result.Errors.Count} 个错误"
            };
        }

        public List<string> DetectConflicts(ModInfo modInfo, List<ModInfo> installedMods)
        {
            var conflicts = new List<string>();

            foreach (var installed in installedMods)
            {
                if (modInfo.Conflicts.Contains(installed.Name) || installed.Conflicts.Contains(modInfo.Name))
                {
                    conflicts.Add(installed.Name);
                    continue;
                }

                var modFiles = GetGameFilePaths(modInfo);
                var installedFiles = GetGameFilePaths(installed);
                var commonFiles = modFiles.Intersect(installedFiles).ToList();

                if (commonFiles.Count > 0)
                {
                    conflicts.Add($"{installed.Name} (冲突文件：{string.Join(", ", commonFiles.Select(Path.GetFileName).Take(3))})");
                }
            }

            return conflicts;
        }

        private List<string> GetGameFilePaths(ModInfo modInfo)
        {
            var paths = new List<string>();

            switch (modInfo.Type)
            {
                case ModType.Addressables:
                    foreach (var file in modInfo.TargetFiles)
                    {
                        paths.Add(Path.Combine(_dataPath, "StreamingAssets", "aa", "StandaloneWindows64", Path.GetFileName(file)));
                    }
                    break;
                case ModType.Plugin:
                    foreach (var file in modInfo.TargetFiles)
                    {
                        paths.Add(Path.Combine(_gamePath, Path.GetFileName(file)));
                    }
                    break;
                default:
                    foreach (var file in modInfo.TargetFiles)
                    {
                        paths.Add(Path.Combine(_gamePath, Path.GetFileName(file)));
                    }
                    break;
            }

            return paths;
        }
    }
}
