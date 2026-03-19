// <copyright file="ModManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AstralPartyModManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    // 安装结果记录
    public class InstallResult
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public List<string> InstalledFiles { get; set; }

        public List<string> ReplacedFiles { get; set; }

        public List<string> Errors { get; set; }

        public InstallResult()
        {
            this.Success = false;
            this.Message = string.Empty;
            this.InstalledFiles = new();
            this.ReplacedFiles = new();
            this.Errors = new();
        }

        public InstallResult(
            bool success,
            string message,
            List<string> installedFiles,
            List<string> replacedFiles,
            List<string> errors
        )
        {
            this.Success = success;
            this.Message = message;
            this.InstalledFiles = installedFiles;
            this.ReplacedFiles = replacedFiles;
            this.Errors = errors;
        }
    }

    // Mod 安装管理器
    public class ModManager
    {
        private readonly string gamePath;
        private readonly string dataPath;

        public ModManager(string gamePath, string dataPath)
        {
            this.gamePath = gamePath;
            this.dataPath = dataPath;
            Logger.Info($"ModManager 初始化 - 游戏路径：{gamePath}, 数据路径：{dataPath}");
        }

        public InstallResult EnableMod(ModInfo modInfo)
        {
            var result = new InstallResult();
            Logger.Info($"开始启用 Mod: {modInfo.Name} (类型：{modInfo.Type})");

            if (!modInfo.IsValid(out string errorMsg))
            {
                result.Success = false;
                result.Message = $"Mod 信息无效：{errorMsg}";
                return result;
            }

            if (!Directory.Exists(this.gamePath))
            {
                result.Success = false;
                result.Message = $"游戏目录不存在：{this.gamePath}";
                return result;
            }

            if (!Directory.Exists(modInfo.FolderPath))
            {
                result.Success = false;
                result.Message = $"Mod 文件夹不存在：{modInfo.FolderPath}";
                return result;
            }

            try
            {
                switch (modInfo.Type)
                {
                    case ModType.Addressables:
                        result = this.InstallAddressablesMod(modInfo);
                        break;
                    case ModType.Voice:
                        result = InstallVoiceMod(modInfo);
                        break;
                    case ModType.Plugin:
                        result = this.InstallPluginMod(modInfo);
                        break;
                    case ModType.Comprehensive:
                        result = this.InstallComprehensiveMod(modInfo);
                        break;
                    default:
                        result = this.InstallGenericMod(modInfo);
                        break;
                }

                if (result.Success)
                {
                    Logger.Info(
                        $"Mod '{modInfo.Name}' 启用成功，安装了 {result.InstalledFiles.Count} 个文件"
                    );
                }
                else
                {
                    Logger.Error($"Mod '{modInfo.Name}' 启用失败：{result.Message}");
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                result.Success = false;
                result.Message = $"安装过程中发生错误：{ex.Message}";
                Logger.Error($"Mod '{modInfo.Name}' 安装失败", ex);
            }

            return result;
        }

        public static void DisableMod(ModInfo modInfo)
        {
            Logger.Info($"准备禁用 Mod: {modInfo.Name}");
        }

        private InstallResult InstallAddressablesMod(ModInfo modInfo)
        {
            var result = new InstallResult();
            string targetDir = Path.Combine(
                this.dataPath,
                "StreamingAssets",
                "aa",
                "StandaloneWindows64"
            );

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            var standardDir = FindStandardDirectory(modInfo.FolderPath, "astralparty");
            if (standardDir != null)
            {
                string streamingAssetsSource = Path.Combine(
                    standardDir,
                    "StreamingAssets",
                    "aa",
                    "StandaloneWindows64"
                );
                if (Directory.Exists(streamingAssetsSource))
                {
                    CopyModFiles(streamingAssetsSource, targetDir, result);
                    result.Success = result.Errors.Count == 0;
                    result.Message =
                        result.Errors.Count == 0
                            ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                            : $"安装完成，但有 {result.Errors.Count} 个错误";
                    return result;
                }

                var subDirs = Directory.GetDirectories(standardDir);
                foreach (var subDir in subDirs)
                {
                    CopyModFiles(subDir, targetDir, result);
                }

                result.Success = result.Errors.Count == 0;
                result.Message =
                    result.Errors.Count == 0
                        ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                        : $"安装完成，但有 {result.Errors.Count} 个错误";
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

            result.Success = result.Errors.Count == 0;
            result.Message =
                result.Errors.Count == 0
                    ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                    : $"安装完成，但有 {result.Errors.Count} 个错误";
            return result;
        }

        private static InstallResult InstallVoiceMod(ModInfo modInfo)
        {
            var result = new InstallResult();

            string appData = Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData
            );
            string targetDir = Path.Combine(
                appData,
                "Low",
                "feimo",
                "AstralParty_CN",
                "com.unity.addressables",
                "AssetBundles"
            );

            var standardDir = FindStandardDirectory(modInfo.FolderPath, "appdata");
            if (standardDir != null)
            {
                string appDataBaseSource = Path.Combine(
                    standardDir,
                    "Low",
                    "feimo",
                    "AstralParty_CN"
                );
                if (Directory.Exists(appDataBaseSource))
                {
                    CopyModFiles(
                        appDataBaseSource,
                        Path.Combine(appData, "Low", "feimo", "AstralParty_CN"),
                        result
                    );
                    result.Success = result.Errors.Count == 0;
                    result.Message =
                        result.Errors.Count == 0
                            ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                            : $"安装完成，但有 {result.Errors.Count} 个错误";
                    return result;
                }

                var subDirs = Directory.GetDirectories(standardDir);
                foreach (var subDir in subDirs)
                {
                    string relativePath = Path.GetRelativePath(standardDir, subDir);
                    string destDir = Path.Combine(appData, relativePath);
                    CopyModFiles(subDir, destDir, result);
                }

                result.Success = result.Errors.Count == 0;
                result.Message =
                    result.Errors.Count == 0
                        ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                        : $"安装完成，但有 {result.Errors.Count} 个错误";
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

            result.Success = result.Errors.Count == 0;
            result.Message =
                result.Errors.Count == 0
                    ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                    : $"安装完成，但有 {result.Errors.Count} 个错误";
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
                    CopyModFiles(pluginsDir, this.gamePath, result);
                    result.Success = result.Errors.Count == 0;
                    result.Message =
                        result.Errors.Count == 0
                            ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                            : $"安装完成，但有 {result.Errors.Count} 个错误";
                    return result;
                }

                CopyModFiles(standardDir, this.gamePath, result);
                result.Success = result.Errors.Count == 0;
                result.Message =
                    result.Errors.Count == 0
                        ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                        : $"安装完成，但有 {result.Errors.Count} 个错误";
                return result;
            }

            CopyModFiles(modInfo.FolderPath, this.gamePath, result);
            result.Success = result.Errors.Count == 0;
            result.Message =
                result.Errors.Count == 0
                    ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                    : $"安装完成，但有 {result.Errors.Count} 个错误";
            return result;
        }

        private InstallResult InstallGenericMod(ModInfo modInfo)
        {
            var result = new InstallResult();
            var bundleFiles = modInfo
                .TargetFiles.Where(f => f.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (bundleFiles.Count > 0)
            {
                return this.InstallAddressablesMod(modInfo);
            }

            return this.InstallPluginMod(modInfo);
        }

        private InstallResult InstallComprehensiveMod(ModInfo modInfo)
        {
            var result = new InstallResult();

            // 处理配置中的每个target
            if (modInfo.Targets != null && modInfo.Targets.Count > 0)
            {
                foreach (var target in modInfo.Targets)
                {
                    InstallResult targetResult;
                    string basePath;

                    // 根据类型确定基础路径
                    switch (target.Type.ToLower())
                    {
                        case "root":
                        case "data":
                        case "addressables":
                        case "gamestar":
                            // 根数据目录 = AstralParty_CN_Data
                            basePath = this.dataPath;
                            break;
                        case "cache":
                        case "voice":
                        case "appdata":
                            // AppData 缓存目录
                            string appData = Environment.GetFolderPath(
                                Environment.SpecialFolder.LocalApplicationData
                            );
                            basePath = Path.Combine(
                                appData,
                                "Low",
                                "feimo",
                                "AstralParty_CN",
                                "com.unity.addressables"
                            );
                            break;
                        case "plugin":
                        case "game":
                        case "rootgame":
                            // 游戏根目录
                            basePath = this.gamePath;
                            break;
                        case "custom":
                        default:
                            // 自定义路径直接使用 targetPath 作为完整路径
                            if (!string.IsNullOrEmpty(target.TargetPath))
                            {
                                basePath = target.TargetPath;
                            }
                            else
                            {
                                Logger.Warning(
                                    $"未知的target类型: {target.Type}，且未提供targetPath，已跳过"
                                );
                                continue;
                            }

                            break;
                    }

                    // 拼接 targetPath 得到最终目标目录
                    string targetDir = string.IsNullOrEmpty(target.TargetPath)
                        ? basePath
                        : Path.Combine(basePath, target.TargetPath);

                    // 将源文件夹内容复制到目标目录
                    targetResult = this.InstallComprehensiveTarget(
                        modInfo,
                        target.Source,
                        targetDir,
                        basePath
                    );

                    // 合并结果
                    result.InstalledFiles.AddRange(targetResult.InstalledFiles);
                    result.ReplacedFiles.AddRange(targetResult.ReplacedFiles);
                    result.Errors.AddRange(targetResult.Errors);
                }

                result.Success = result.Errors.Count == 0;
                result.Message =
                    result.Errors.Count == 0
                        ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件到多个位置"
                        : $"安装完成，但有 {result.Errors.Count} 个错误";
            }
            else
            {
                // 如果没有配置targets，回退到自动检测
                Logger.Info($"Comprehensive mod 没有配置targets，尝试自动检测");
                var hasRoot =
                    Directory.Exists(Path.Combine(modInfo.FolderPath, "AstralParty"))
                    || Directory.Exists(Path.Combine(modInfo.FolderPath, "游戏根目录"));
                var hasCache =
                    Directory.Exists(Path.Combine(modInfo.FolderPath, "AssetBundles"))
                    || Directory.Exists(Path.Combine(modInfo.FolderPath, "缓存"));

                if (hasRoot)
                {
                    var rootResult = this.InstallAddressablesMod(modInfo);
                    MergeResult(result, rootResult);
                }

                if (hasCache)
                {
                    var cacheResult = InstallVoiceMod(modInfo);
                    MergeResult(result, cacheResult);
                }

                result.Success = result.Errors.Count == 0;
                result.Message =
                    result.Errors.Count == 0
                        ? $"自动检测完成，成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                        : $"自动检测完成，但有 {result.Errors.Count} 个错误";
            }

            return result;
        }

        private InstallResult InstallComprehensiveTarget(
            ModInfo modInfo,
            string sourceDirName,
            string targetDir,
            string basePath
        )
        {
            var result = new InstallResult();
            string sourceDir = Path.Combine(modInfo.FolderPath, sourceDirName);

            if (!Directory.Exists(sourceDir))
            {
                result.Errors.Add($"源目录不存在: {sourceDir}");
                result.Success = false;
                result.Message = $"安装失败：源目录不存在 {sourceDir}";
                Logger.Warning($"源目录不存在，跳过: {sourceDir}");
                return result;
            }

            this.CopyModFilesWithRelativeBase(sourceDir, targetDir, result, basePath);
            result.Success = result.Errors.Count == 0;
            result.Message =
                result.Errors.Count == 0
                    ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
                    : $"安装完成，但有 {result.Errors.Count} 个错误";
            return result;
        }

        private static void MergeResult(InstallResult mainResult, InstallResult subResult)
        {
            mainResult.InstalledFiles.AddRange(subResult.InstalledFiles);
            mainResult.ReplacedFiles.AddRange(subResult.ReplacedFiles);
            mainResult.Errors.AddRange(subResult.Errors);
        }

        private static string FindModSubdirectory(string folderPath)
        {
            var priorityNames = new[] { "模组", "根模组", "根目录替换", "StandaloneWindows64" };
            foreach (var name in priorityNames)
            {
                var subdir = Directory
                    .GetDirectories(folderPath)
                    .FirstOrDefault(d => Path.GetFileName(d).Contains(name));
                if (subdir != null)
                {
                    return subdir;
                }
            }

            return null;
        }

        private static string FindVoiceSubdirectory(string folderPath)
        {
            var priorityNames = new[] { "纯语音替换", "适配动画的语音替换", "__data" };
            foreach (var name in priorityNames)
            {
                var subdir = Directory
                    .GetDirectories(folderPath)
                    .FirstOrDefault(d => Path.GetFileName(d).Contains(name));
                if (subdir != null)
                {
                    var dataSubdir = Directory.GetDirectories(subdir).FirstOrDefault();
                    return dataSubdir ?? subdir;
                }
            }

            return null;
        }

        private static string FindStandardDirectory(string folderPath, string dirName)
        {
            try
            {
                return Directory
                    .GetDirectories(folderPath)
                    .FirstOrDefault(d => Path.GetFileName(d).ToLower() == dirName);
            }
            catch (Exception ex)
            {
                Logger.Warning($"查找标准目录失败：{folderPath}", ex);
                return null;
            }
        }

        private static void CopyModFiles(string sourceDir, string targetDir, InstallResult result)
        {
            if (!Directory.Exists(sourceDir))
            {
                result.Errors.Add($"源目录不存在：{sourceDir}");
                result.Success = false;
                result.Message = $"安装失败：源目录不存在 {sourceDir}";
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
                    if (
                        fileName == "mod.json"
                        || fileName == "readme.md"
                        || fileName == "readme.txt"
                    )
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

                    Logger.Debug(
                        $"复制文件: {file} -> {destFile} {(isReplace ? "(替换)" : "(新建)")}"
                    );

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

            // result.Success = result.Errors.Count == 0;
            // result.Message = result.Errors.Count == 0
            //     ? $"成功安装 {result.InstalledFiles.Count + result.ReplacedFiles.Count} 个文件"
            //     : $"安装完成，但有 {result.Errors.Count} 个错误";
        }

        private void CopyModFilesWithRelativeBase(
            string sourceDir,
            string targetDir,
            InstallResult result,
            string basePath
        )
        {
            if (!Directory.Exists(sourceDir))
            {
                result.Errors.Add($"源目录不存在：{sourceDir}");
                result.Success = false;
                result.Message = $"安装失败：源目录不存在 {sourceDir}";
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
                    if (
                        fileName == "mod.json"
                        || fileName == "readme.md"
                        || fileName == "readme.txt"
                    )
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

                    Logger.Debug(
                        $"复制文件: {file} -> {destFile} {(isReplace ? "(替换)" : "(新建)")}"
                    );

                    // 需要存储相对于游戏根目录的路径用于备份
                    string relativeToGame = GetRelativePath(destFile, this.gamePath);
                    if (isReplace)
                    {
                        result.ReplacedFiles.Add(relativeToGame);
                    }
                    else
                    {
                        result.InstalledFiles.Add(relativeToGame);
                    }

                    File.Copy(file, destFile, overwrite: true);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"复制文件失败 {file}: {ex.Message}");
                    Logger.Warning($"复制文件失败：{file}", ex);
                }
            }
        }

        private static string GetRelativePath(string fullPath, string basePath)
        {
            fullPath = Path.GetFullPath(fullPath);
            basePath = Path.GetFullPath(basePath);

            if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                // 如果不在游戏根目录下，需要特殊处理？不，直接返回完整文件名？
                // 对于AppData的路径，仍然返回相对于游戏根的绝对路径？不，返回相对于游戏根的完整路径不切实际
                // 这里我们存储相对于%LOCALAPPDATA%的路径？不需要，我们在备份恢复时使用完整路径直接存储
                // 但是MainForm传递给PrepareEnableModFromFiles的需要是相对游戏根目录的路径
                // 对于AppData这种外部路径，它其实不应该出现在这里，备份的时候会怎么处理？
                // 最简单方式：直接返回绝对路径，然后在备份时直接使用绝对路径
                // 但 PrepareEnableModFromFiles 期望的是相对路径，所以我们需要特殊处理这种情况
                // 直接返回全路径，备份时会拼接 _gamePath？这不对
                // 我们返回相对于 %LOCALAPPDATA% 的路径？不行，我们需要正确处理
                // 对于不在游戏目录的文件，它们都是新文件，卸载的时候也应该能正确删除
                // 让我们做一个判断：如果是AppData，我们存储相对于LOCALAPPDATA的路径？不，备份恢复需要完整路径
                // 实际上，BackupInfo存储FullPath，所以备份信息没问题，问题只出在我们需要相对路径来保存在备份目录
                // 哦，没关系，即使文件在游戏目录外面，我们只需要备份哈希，不需要备份内容？
                // 实际上AppData是缓存目录，重新下载也可以恢复，所以直接删除就行
                // 所以我们存储相对于LOCALAPPDATA？或者我们存储相对于C盘？这会导致备份目录创建问题
                // 实际上用户AppData文件一般都是新文件，所以只需要删除即可，不需要恢复
                // 我们这里返回的相对路径只是用来在备份目录中存放原文件，对于AppData文件，如果有原有文件，我们也需要备份
                // 所以最好是用相对于根目录的绝对路径，比如 "C:/Users/name/AppData/..." 这样，但Windows路径不能包含冒号
                // 我们需要做一些转换：把 C:\Users\... 转换成 C/Users/...，这样就可以在目录结构中存储
                string drive = Path.GetPathRoot(fullPath);
                string noRoot = fullPath.Substring(drive.Length);
                string driveLetter = drive.Trim('\\', '/').Replace(":", string.Empty);
                return Path.Combine(driveLetter, noRoot).Replace('/', '\\');
            }

            return fullPath.Substring(basePath.Length).TrimStart('\\', '/');
        }

        public List<string> DetectConflicts(ModInfo modInfo, List<ModInfo> installedMods)
        {
            var conflicts = new List<string>();

            foreach (var installed in installedMods)
            {
                if (
                    modInfo.Conflicts.Contains(installed.Name)
                    || installed.Conflicts.Contains(modInfo.Name)
                )
                {
                    conflicts.Add(installed.Name);
                    continue;
                }

                var modFiles = this.GetGameFilePaths(modInfo);
                var installedFiles = this.GetGameFilePaths(installed);
                var commonFiles = modFiles.Intersect(installedFiles).ToList();

                if (commonFiles.Count > 0)
                {
                    conflicts.Add(
                        $"{installed.Name} (冲突文件：{string.Join(", ", commonFiles.Select(Path.GetFileName).Take(3))})"
                    );
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
                        paths.Add(
                            Path.Combine(
                                this.dataPath,
                                "StreamingAssets",
                                "aa",
                                "StandaloneWindows64",
                                Path.GetFileName(file)
                            )
                        );
                    }

                    break;
                case ModType.Plugin:
                    foreach (var file in modInfo.TargetFiles)
                    {
                        paths.Add(Path.Combine(this.gamePath, Path.GetFileName(file)));
                    }

                    break;
                default:
                    foreach (var file in modInfo.TargetFiles)
                    {
                        paths.Add(Path.Combine(this.gamePath, Path.GetFileName(file)));
                    }

                    break;
            }

            return paths;
        }
    }
}
