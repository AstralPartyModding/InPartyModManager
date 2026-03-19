// <copyright file="BackupManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AstralPartyModManager
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text.Json;

    // 数据类使用 record 类型（C# 9+ 推荐写法）
    public record BackupInfo(
        string ModName,
        DateTime BackupTime,
        ModType ModType,
        List<BackupFile> Files
    );

    public record BackupFile(string FullPath, bool IsNewFile, string OriginalHash);

    public record BackupResult(
        bool Success,
        string Message,
        int BackedUpCount,
        int NewFileCount,
        List<string> Errors
    );

    public record RestoreResult(
        bool Success,
        string Message,
        int RestoredCount,
        int DeletedCount,
        List<string> Errors
    );

    // 备份管理器
    public class BackupManager
    {
        private readonly string gamePath;
        private readonly string backupRoot;
        private readonly string dataPath;

        public BackupManager(string gamePath, string dataPath = null, string backupPath = null)
        {
            this.gamePath = gamePath;
            this.dataPath = dataPath ?? Path.Combine(gamePath, "AstralParty_CN_Data");

            if (!string.IsNullOrEmpty(backupPath))
            {
                this.backupRoot = backupPath;
            }
            else
            {
                string executableDir = AppDomain.CurrentDomain.BaseDirectory;
                this.backupRoot = Path.Combine(executableDir, "data", "backups");
            }

            if (!Directory.Exists(this.backupRoot))
            {
                Directory.CreateDirectory(this.backupRoot);
            }

            Logger.Info(
                $"BackupManager 初始化 - 游戏路径：{gamePath}, 备份根目录：{this.backupRoot}"
            );
        }

        /// <summary>
        /// 根据 Mod 类型获取目标目录.
        /// </summary>
        /// <returns></returns>
        public string GetTargetDirectory(ModType modType)
        {
            switch (modType)
            {
                case ModType.Addressables:
                    return Path.Combine(
                        this.dataPath,
                        "StreamingAssets",
                        "aa",
                        "StandaloneWindows64"
                    );
                case ModType.Voice:
                    string appData = Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData
                    );
                    return Path.Combine(
                        appData,
                        "Low",
                        "feimo",
                        "AstralParty_CN",
                        "com.unity.addressables",
                        "AssetBundles"
                    );
                case ModType.Plugin:
                    return this.gamePath;
                default:
                    return this.gamePath;
            }
        }

        /// <summary>
        /// 获取文件在游戏目录中的实际路径
        /// 已废弃：此方法不再使用，备份时直接存储完整路径.
        /// </summary>
        /// <returns></returns>
        public string GetGameFilePath(string modFile, string targetDir, ModType modType)
        {
            if (modType == ModType.Plugin)
            {
                string fileName = Path.GetFileName(modFile);
                string gameFilePath = Path.Combine(this.gamePath, fileName);
                return gameFilePath;
            }

            if (modFile.Contains("StreamingAssets") || modFile.Contains("StandaloneWindows64"))
            {
                var parts = modFile.Split(
                    new[] { "StreamingAssets", "StandaloneWindows64" },
                    StringSplitOptions.None
                );
                if (parts.Length > 1)
                {
                    return Path.Combine(
                            this.dataPath,
                            "StreamingAssets",
                            "aa",
                            "StandaloneWindows64"
                        ) + parts[1];
                }
            }

            string fileNameOnly = Path.GetFileName(modFile);
            return Path.Combine(targetDir, fileNameOnly);
        }

        /// <summary>
        /// 根据已经安装完的Mod文件列表进行备份（用于Comprehensive类型）.
        /// </summary>
        /// <returns></returns>
        public BackupResult PrepareEnableModFromFiles(
            string modName,
            List<string> targetRelativePaths
        )
        {
            var result = new BackupResult(false, string.Empty, 0, 0, new List<string>());
            Logger.Info($"准备启用 Mod '{modName}'，开始备份游戏文件...");

            var backupInfo = new BackupInfo(
                modName,
                DateTime.Now,
                ModType.Comprehensive,
                new List<BackupFile>()
            );

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupDir = Path.Combine(this.backupRoot, timestamp, "game_files");

            Logger.Debug($"Mod '{modName}' 将安装 {targetRelativePaths.Count} 个文件");

            foreach (var relativePath in targetRelativePaths)
            {
                // 相对路径相对于游戏根目录（对于外部目录是特殊转换后的路径）
                string gameFilePath;
                if (relativePath.Contains(':'))
                {
                    // 对于带冒号的路径，说明是完整路径？不，我们已经转换了冒号
                    // drive:\path 转换为 drive\path
                    // 所以我们需要把它转换回来：C\Users\name -> C:\Users\name
                    string driveLetter = relativePath.Split('\\')[0];
                    string restPath = string.Join("\\", relativePath.Split('\\').Skip(1));
                    gameFilePath = $"{driveLetter}:\\{restPath}";
                }
                else
                {
                    gameFilePath = Path.Combine(this.gamePath, relativePath);
                }

                string gameFileDirectory = Path.GetDirectoryName(gameFilePath);

                if (!Directory.Exists(gameFileDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(gameFileDirectory);
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"创建目标目录失败 {gameFileDirectory}: {ex.Message}");
                        Logger.Warning($"创建目标目录失败：{gameFileDirectory}", ex);
                        continue;
                    }
                }

                bool fileExists = File.Exists(gameFilePath);

                backupInfo.Files.Add(
                    new BackupFile(
                        gameFilePath,
                        !fileExists,
                        fileExists ? ComputeFileHash(gameFilePath) : null
                    )
                );

                if (!fileExists)
                {
                    result = result with { NewFileCount = result.NewFileCount + 1 };
                    Logger.Debug($"新文件：{relativePath}");
                }
                else
                {
                    try
                    {
                        string backupPath = Path.Combine(backupDir, relativePath);
                        string backupFileDir = Path.GetDirectoryName(backupPath);

                        if (!Directory.Exists(backupFileDir))
                        {
                            Directory.CreateDirectory(backupFileDir);
                        }

                        File.Copy(gameFilePath, backupPath, overwrite: true);
                        result = result with { BackedUpCount = result.BackedUpCount + 1 };
                        Logger.Debug($"已备份：{relativePath}");
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"备份文件失败 {relativePath}: {ex.Message}");
                        Logger.Warning($"备份文件失败：{gameFilePath}", ex);
                    }
                }
            }

            this.SaveBackupInfo(backupInfo, timestamp);

            result = result with
            {
                Success = result.Errors.Count == 0,
                Message =
                    result.Errors.Count == 0
                        ? $"备份完成：{result.BackedUpCount} 个文件已备份，{result.NewFileCount} 个新文件"
                        : $"备份完成，但有 {result.Errors.Count} 个错误",
            };

            Logger.Info($"Mod '{modName}' 备份完成：{result.Message}");
            return result;
        }

        /// <summary>
        /// 获取相对路径.
        /// </summary>
        /// <returns></returns>
        public static string GetRelativePath(string fullPath, string basePath)
        {
            fullPath = Path.GetFullPath(fullPath);
            basePath = Path.GetFullPath(basePath);

            if (!fullPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileName(fullPath);
            }

            return fullPath.Substring(basePath.Length).TrimStart('\\', '/');
        }

        /// <summary>
        /// 准备启用 Mod：备份即将被覆盖的文件.
        /// </summary>
        /// <returns></returns>
        public BackupResult PrepareEnableMod(
            string modName,
            List<string> modFolderFiles,
            ModType modType,
            string modFolderPath
        )
        {
            var result = new BackupResult(false, string.Empty, 0, 0, new List<string>());
            Logger.Info($"准备启用 Mod '{modName}'，开始备份游戏文件...");

            var backupInfo = new BackupInfo(modName, DateTime.Now, modType, new List<BackupFile>());

            string targetDir = this.GetTargetDirectory(modType);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupDir = Path.Combine(this.backupRoot, timestamp, "game_files");

            var filesToInstall = this.GetFilesToInstall(modFolderPath, modType, targetDir);

            Logger.Debug($"Mod '{modName}' 将安装 {filesToInstall.Count} 个文件");

            foreach (var installFile in filesToInstall)
            {
                string gameFilePath = installFile.Value;
                string relativePath = GetRelativePath(gameFilePath, this.gamePath);

                bool fileExists = File.Exists(gameFilePath);

                backupInfo.Files.Add(
                    new BackupFile(
                        gameFilePath,
                        !fileExists,
                        fileExists ? ComputeFileHash(gameFilePath) : null
                    )
                );

                if (!fileExists)
                {
                    result = result with { NewFileCount = result.NewFileCount + 1 };
                    Logger.Debug($"新文件：{relativePath}");
                }
                else
                {
                    try
                    {
                        string backupPath = Path.Combine(backupDir, relativePath);
                        string backupFileDir = Path.GetDirectoryName(backupPath);

                        if (!Directory.Exists(backupFileDir))
                        {
                            Directory.CreateDirectory(backupFileDir);
                        }

                        File.Copy(gameFilePath, backupPath, overwrite: true);
                        result = result with { BackedUpCount = result.BackedUpCount + 1 };
                        Logger.Debug($"已备份：{relativePath}");
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"备份文件失败 {relativePath}: {ex.Message}");
                        Logger.Warning($"备份文件失败：{gameFilePath}", ex);
                    }
                }
            }

            this.SaveBackupInfo(backupInfo, timestamp);

            result = result with
            {
                Success = result.Errors.Count == 0,
                Message =
                    result.Errors.Count == 0
                        ? $"备份完成：{result.BackedUpCount} 个文件已备份，{result.NewFileCount} 个新文件"
                        : $"备份完成，但有 {result.Errors.Count} 个错误",
            };

            Logger.Info($"Mod '{modName}' 备份完成：{result.Message}");
            return result;
        }

        /// <summary>
        /// 获取会被安装到游戏目录的文件列表.
        /// </summary>
        private Dictionary<string, string> GetFilesToInstall(
            string modFolderPath,
            ModType modType,
            string targetDir
        )
        {
            var result = new Dictionary<string, string>();

            string standardDir = null;
            if (modType == ModType.Addressables || modType == ModType.Plugin)
            {
                standardDir = FindStandardDirectory(modFolderPath, "astralparty");
            }
            else if (modType == ModType.Voice)
            {
                standardDir = FindStandardDirectory(modFolderPath, "appdata");
            }

            if (standardDir != null)
            {
                this.ScanStandardDirectory(standardDir, modType, targetDir, result);
                return result;
            }

            string subDir = null;
            if (modType == ModType.Addressables)
            {
                subDir = FindModSubdirectory(modFolderPath);
            }
            else if (modType == ModType.Voice)
            {
                subDir = FindVoiceSubdirectory(modFolderPath);
            }

            string sourceDir = subDir ?? modFolderPath;

            foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file).ToLower() == "mod.json")
                {
                    continue;
                }

                if (file.Contains("\\docs\\"))
                {
                    continue;
                }

                string relativePath = GetRelativePath(file, sourceDir);
                string gameFilePath;

                if (modType == ModType.Plugin)
                {
                    gameFilePath = Path.Combine(this.gamePath, relativePath);
                }
                else
                {
                    gameFilePath = Path.Combine(targetDir, relativePath);
                }

                result[file] = gameFilePath;
            }

            return result;
        }

        /// <summary>
        /// 扫描标准格式目录.
        /// </summary>
        private void ScanStandardDirectory(
            string standardDir,
            ModType modType,
            string targetDir,
            Dictionary<string, string> result
        )
        {
            foreach (
                var file in Directory.GetFiles(standardDir, "*.*", SearchOption.AllDirectories)
            )
            {
                if (Path.GetFileName(file).ToLower() == "mod.json")
                {
                    continue;
                }

                if (file.Contains("\\docs\\"))
                {
                    continue;
                }

                string relativePath = GetRelativePath(file, standardDir);
                string gameFilePath;

                if (modType == ModType.Addressables)
                {
                    if (
                        relativePath.StartsWith(
                            "StreamingAssets",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                    {
                        string subPath = relativePath
                            .Substring("StreamingAssets".Length)
                            .TrimStart('\\', '/');
                        gameFilePath = Path.Combine(
                            this.dataPath,
                            "StreamingAssets",
                            "aa",
                            "StandaloneWindows64",
                            subPath
                        );
                    }
                    else if (relativePath.StartsWith("Plugins", StringComparison.OrdinalIgnoreCase))
                    {
                        string subPath = relativePath
                            .Substring("Plugins".Length)
                            .TrimStart('\\', '/');
                        gameFilePath = Path.Combine(this.gamePath, subPath);
                    }
                    else
                    {
                        gameFilePath = Path.Combine(this.gamePath, relativePath);
                    }
                }
                else if (modType == ModType.Voice)
                {
                    string appData = Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData
                    );
                    if (relativePath.StartsWith("Low", StringComparison.OrdinalIgnoreCase))
                    {
                        string subPath = relativePath.Substring("Low".Length).TrimStart('\\', '/');
                        gameFilePath = Path.Combine(
                            appData,
                            "Low",
                            "feimo",
                            "AstralParty_CN",
                            subPath
                        );
                    }
                    else
                    {
                        gameFilePath = Path.Combine(
                            appData,
                            "Low",
                            "feimo",
                            "AstralParty_CN",
                            relativePath
                        );
                    }
                }
                else if (modType == ModType.Plugin)
                {
                    gameFilePath = Path.Combine(this.gamePath, relativePath);
                }
                else
                {
                    gameFilePath = Path.Combine(this.gamePath, relativePath);
                }

                result[file] = gameFilePath;
            }
        }

        /// <summary>
        /// 查找模组子文件夹.
        /// </summary>
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

        /// <summary>
        /// 查找语音文件子文件夹.
        /// </summary>
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

        /// <summary>
        /// 查找标准格式目录.
        /// </summary>
        private static string FindStandardDirectory(string folderPath, string dirName)
        {
            return Directory
                .GetDirectories(folderPath)
                .FirstOrDefault(d => Path.GetFileName(d).ToLower() == dirName);
        }

        /// <summary>
        /// 保存备份信息.
        /// </summary>
        private void SaveBackupInfo(BackupInfo info, string timestamp)
        {
            try
            {
                string jsonPath = Path.Combine(this.backupRoot, timestamp, "backup_info.json");
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(info, options);
                File.WriteAllText(jsonPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存备份信息失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 加载备份信息.
        /// </summary>
        private BackupInfo LoadBackupInfo(string timestamp)
        {
            try
            {
                string jsonPath = Path.Combine(this.backupRoot, timestamp, "backup_info.json");
                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    try
                    {
                        // 尝试加载新格式（FullPath）
                        return JsonSerializer.Deserialize<BackupInfo>(json);
                    }
                    catch (JsonException)
                    {
                        // 如果失败，尝试兼容加载旧格式（RelativePath）
                        try
                        {
                            var oldBackup = JsonSerializer.Deserialize<OldBackupInfo>(json);
                            if (oldBackup != null)
                            {
                                // 转换为新格式
                                var files = oldBackup
                                    .Files.Select(oldFile => new BackupFile(
                                        Path.Combine(this.gamePath, oldFile.RelativePath),
                                        oldFile.IsNewFile,
                                        oldFile.OriginalHash
                                    ))
                                    .ToList();
                                return new BackupInfo(
                                    oldBackup.ModName,
                                    oldBackup.BackupTime,
                                    oldBackup.ModType,
                                    files
                                );
                            }
                        }
                        catch (Exception ex2)
                        {
                            Logger.Warning($"加载旧格式备份失败：{jsonPath}", ex2);
                        }

                        Logger.Warning($"加载备份信息失败，格式不兼容：{jsonPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"加载备份信息失败：{ex.Message}", ex);
            }

            return null;
        }

        // 兼容旧版本备份格式
        private record OldBackupInfo(
            string ModName,
            DateTime BackupTime,
            ModType ModType,
            List<OldBackupFile> Files
        );

        private record OldBackupFile(string RelativePath, bool IsNewFile, string OriginalHash);

        /// <summary>
        /// 禁用 Mod：恢复被覆盖的文件，删除新增的文件.
        /// </summary>
        /// <returns></returns>
        public RestoreResult DisableMod(string modName, ModType modType)
        {
            var result = new RestoreResult(false, string.Empty, 0, 0, new List<string>());
            Logger.Info($"准备禁用 Mod '{modName}'，开始恢复游戏文件...");

            var backupInfo = this.FindLatestBackupInfo(modName);

            if (backupInfo == null || backupInfo.Files.Count == 0)
            {
                var (success, message) = this.DisableModByType(modName, modType, result);
                result = result with { Success = success, Message = message };
                return result;
            }

            string timestamp = GetTimestampFromBackupInfo(backupInfo);
            string backupDir = Path.Combine(this.backupRoot, timestamp, "game_files");

            Logger.Debug($"找到备份信息：{backupInfo.Files.Count} 个文件，备份时间：{timestamp}");

            foreach (var file in backupInfo.Files)
            {
                string gameFilePath = file.FullPath;
                string relativePath = GetRelativePath(gameFilePath, this.gamePath);
                try
                {
                    if (file.IsNewFile)
                    {
                        if (File.Exists(gameFilePath))
                        {
                            File.Delete(gameFilePath);
                            result = result with { DeletedCount = result.DeletedCount + 1 };
                            Logger.Debug($"已删除新文件：{relativePath}");
                        }
                        else
                        {
                            Logger.Debug($"文件不存在，跳过删除：{relativePath}");
                        }
                    }
                    else
                    {
                        // 备份文件路径依然使用相对路径（相对于游戏根目录保存在备份文件夹中）
                        string backupPath = Path.Combine(backupDir, relativePath);
                        if (File.Exists(backupPath))
                        {
                            string destDir = Path.GetDirectoryName(gameFilePath);
                            if (!Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                            }

                            File.Copy(backupPath, gameFilePath, overwrite: true);
                            result = result with { RestoredCount = result.RestoredCount + 1 };
                            Logger.Debug($"已恢复文件：{relativePath}");
                        }
                        else if (File.Exists(gameFilePath))
                        {
                            File.Delete(gameFilePath);
                            result = result with { DeletedCount = result.DeletedCount + 1 };
                            Logger.Debug($"备份不存在，已删除文件：{relativePath}");
                        }
                        else
                        {
                            // relativePath 已经在上面声明过了
                            Logger.Debug($"文件和备份都不存在，跳过：{relativePath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"恢复文件失败 {relativePath}: {ex.Message}");
                    Logger.Warning($"恢复文件失败：{relativePath}", ex);
                }
            }

            result = result with
            {
                Success = result.Errors.Count == 0,
                Message =
                    result.Errors.Count == 0
                        ? $"恢复完成：{result.RestoredCount} 个文件已恢复，{result.DeletedCount} 个文件已删除"
                        : $"恢复完成，但有 {result.Errors.Count} 个错误",
            };

            Logger.Info($"Mod '{modName}' 禁用完成：{result.Message}");

            // 恢复完成后删除备份，节省空间
            if (backupInfo != null)
            {
                this.DeleteBackup(backupInfo);
            }

            return result;
        }

        /// <summary>
        /// 根据 Mod 类型禁用（没有备份信息时使用）.
        /// </summary>
        private (bool Success, string Message) DisableModByType(
            string modName,
            ModType modType,
            RestoreResult result
        )
        {
            if (modType == ModType.Plugin)
            {
                var commonPluginFiles = new[]
                {
                    "version.dll",
                    "speedhack_config.json",
                    "winmm.dll",
                    "dinput8.dll",
                    "UnityDoorstop.dll",
                    "doorstop_config.ini",
                };
                int deletedCount = 0;

                foreach (var fileName in commonPluginFiles)
                {
                    try
                    {
                        string filePath = Path.Combine(this.gamePath, fileName);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                            deletedCount++;
                            Logger.Debug($"已删除（无备份）: {fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"删除文件失败 {fileName}: {ex.Message}");
                        Logger.Warning($"删除文件失败：{fileName}", ex);
                    }
                }

                bool success = result.Errors.Count == 0;
                string message = success
                    ? $"已尝试禁用插件（无备份信息），{deletedCount} 个文件已删除"
                    : $"禁用部分文件失败：{string.Join("; ", result.Errors)}";
                return (success, message);
            }

            if (modType == ModType.Addressables || modType == ModType.Comprehensive)
            {
                // 对于Comprehensive类型，不应该批量删除所有bundle文件
                // Comprehensive有完整的备份信息，应该走正常恢复流程
                // 如果真的走到这里，说明备份丢失，只记录不操作
                if (modType == ModType.Comprehensive)
                {
                    Logger.Warning(
                        $"Comprehensive类型Mod {modName} 没有找到备份信息，跳过批量删除"
                    );
                    result.Errors.Add(
                        $"Comprehensive类型Mod没有找到备份信息，无法自动恢复，请使用验证游戏文件完整性修复"
                    );
                    return (
                        false,
                        $"Comprehensive类型Mod没有找到备份信息，请在Steam中验证游戏文件完整性恢复"
                    );
                }

                // Addressables 类型仍然保留旧逻辑（不推荐使用这种类型）
                string targetDir = Path.Combine(
                    this.dataPath,
                    "StreamingAssets",
                    "aa",
                    "StandaloneWindows64"
                );
                int deletedCount = 0;

                if (Directory.Exists(targetDir))
                {
                    try
                    {
                        var bundleFiles = Directory.GetFiles(targetDir, "*.bundle");
                        foreach (var bundleFile in bundleFiles)
                        {
                            try
                            {
                                File.Delete(bundleFile);
                                deletedCount++;
                                Logger.Debug(
                                    $"已删除bundle文件（无备份）: {Path.GetFileName(bundleFile)}"
                                );
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add(
                                    $"删除文件失败 {Path.GetFileName(bundleFile)}: {ex.Message}"
                                );
                                Logger.Warning($"删除bundle文件失败：{bundleFile}", ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"遍历目录失败 {targetDir}: {ex.Message}");
                        Logger.Warning($"遍历目录失败：{targetDir}", ex);
                    }
                }

                bool success = result.Errors.Count == 0;
                string message = success
                    ? $"已尝试禁用 {modType} 类型 Mod（无备份信息），{deletedCount} 个bundle文件已删除\n游戏启动时会自动重新下载原版文件"
                    : $"禁用完成，但有 {result.Errors.Count} 个错误：\n{string.Join("; ", result.Errors)}";
                return (success, message);
            }

            if (modType == ModType.Voice)
            {
                // 语音文件都在 AssetBundles 目录下
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
                int deletedCount = 0;

                if (Directory.Exists(targetDir))
                {
                    try
                    {
                        var bundleFiles = Directory.GetFiles(targetDir, "*.bundle");
                        foreach (var bundleFile in bundleFiles)
                        {
                            try
                            {
                                File.Delete(bundleFile);
                                deletedCount++;
                                Logger.Debug(
                                    $"已删除语音bundle文件（无备份）: {Path.GetFileName(bundleFile)}"
                                );
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add(
                                    $"删除文件失败 {Path.GetFileName(bundleFile)}: {ex.Message}"
                                );
                                Logger.Warning($"删除语音bundle文件失败：{bundleFile}", ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"遍历目录失败 {targetDir}: {ex.Message}");
                        Logger.Warning($"遍历目录失败：{targetDir}", ex);
                    }
                }

                bool success = result.Errors.Count == 0;
                string message = success
                    ? $"已尝试禁用语音Mod（无备份信息），{deletedCount} 个文件已删除\n游戏启动时会自动重新下载原版文件"
                    : $"禁用完成，但有 {result.Errors.Count} 个错误：\n{string.Join("; ", result.Errors)}";
                return (success, message);
            }

            Logger.Warning(
                $"无法确定 {modType} 类型 Mod 的具体文件，请使用'恢复纯净'功能或手动清理"
            );
            result.Errors.Add($"无法确定 {modType} 类型 Mod 的具体文件");
            return (false, $"无法确定 {modType} 类型 Mod 的具体文件");
        }

        /// <summary>
        /// 查找指定 Mod 的最新备份信息.
        /// </summary>
        private BackupInfo FindLatestBackupInfo(string modName)
        {
            var backupDirs = new List<(string Path, DateTime Time)>();

            foreach (var dir in Directory.GetDirectories(this.backupRoot))
            {
                string timestamp = Path.GetFileName(dir);
                if (
                    DateTime.TryParseExact(
                        timestamp,
                        "yyyyMMdd_HHmmss",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime time
                    )
                )
                {
                    backupDirs.Add((dir, time));
                }
                else
                {
                    // 如果不能解析时间，使用目录的最后修改时间
                    try
                    {
                        time = Directory.GetLastWriteTime(dir);
                        backupDirs.Add((dir, time));
                    }
                    catch { }
                }
            }

            // 按时间降序排序，最新的在最前面
            foreach (var dirInfo in backupDirs.OrderByDescending(d => d.Time))
            {
                string timestamp = Path.GetFileName(dirInfo.Path);
                var info = this.LoadBackupInfo(timestamp);
                if (info != null && info.ModName == modName)
                {
                    return info;
                }
            }

            return null;
        }

        /// <summary>
        /// 从备份信息获取时间戳.
        /// </summary>
        private static string GetTimestampFromBackupInfo(BackupInfo info)
        {
            return info.BackupTime.ToString("yyyyMMdd_HHmmss");
        }

        /// <summary>
        /// 恢复所有备份（完全恢复游戏）.
        /// </summary>
        public void RestoreAllBackups()
        {
            var backupDirs = Directory.GetDirectories(this.backupRoot);
            if (backupDirs.Length == 0)
            {
                throw new Exception("没有找到备份文件");
            }

            var allBackupFiles = new Dictionary<string, string>();

            foreach (var backupDir in backupDirs)
            {
                string gameFilesDir = Path.Combine(backupDir, "game_files");
                if (!Directory.Exists(gameFilesDir))
                {
                    continue;
                }

                foreach (
                    var file in Directory.GetFiles(gameFilesDir, "*.*", SearchOption.AllDirectories)
                )
                {
                    string relativePath = GetRelativePath(file, gameFilesDir);
                    if (!allBackupFiles.ContainsKey(relativePath))
                    {
                        allBackupFiles[relativePath] = file;
                    }
                }
            }

            foreach (var kvp in allBackupFiles)
            {
                try
                {
                    string destFile = Path.Combine(this.gamePath, kvp.Key);
                    string destDir = Path.GetDirectoryName(destFile);

                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    File.Copy(kvp.Value, destFile, overwrite: true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"恢复文件失败 {kvp.Key}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 清理旧备份（保留最近 5 个）.
        /// </summary>
        public void CleanupOldBackups(int keepCount = 5)
        {
            var backupDirs = Directory.GetDirectories(this.backupRoot);
            if (backupDirs.Length <= keepCount)
            {
                return;
            }

            var sortedDirs = new List<(string Path, DateTime Time)>();

            foreach (var dir in backupDirs)
            {
                string dirName = Path.GetFileName(dir);
                if (
                    DateTime.TryParseExact(
                        dirName,
                        "yyyyMMdd_HHmmss",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime time
                    )
                )
                {
                    sortedDirs.Add((dir, time));
                }
            }

            var toDelete = sortedDirs
                .OrderByDescending(x => x.Time)
                .Skip(keepCount)
                .Select(x => x.Path);

            foreach (var dir in toDelete)
            {
                try
                {
                    Directory.Delete(dir, recursive: true);
                    Logger.Info($"已清理旧备份：{dir}");
                }
                catch (Exception ex)
                {
                    Logger.Warning($"删除备份失败：{dir}", ex);
                }
            }
        }

        /// <summary>
        /// 删除指定备份.
        /// </summary>
        private void DeleteBackup(BackupInfo backupInfo)
        {
            try
            {
                string timestamp = GetTimestampFromBackupInfo(backupInfo);
                string backupDir = Path.Combine(this.backupRoot, timestamp);
                if (Directory.Exists(backupDir))
                {
                    Directory.Delete(backupDir, recursive: true);
                    Logger.Info($"已删除Mod '{backupInfo.ModName}' 的备份：{backupDir}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"删除备份失败：{backupInfo.ModName}", ex);
            }
        }

        /// <summary>
        /// 计算文件 SHA256 哈希值.
        /// </summary>
        private static string ComputeFileHash(string filePath)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    return BitConverter
                        .ToString(hash)
                        .Replace("-", string.Empty)
                        .ToLowerInvariant();
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"计算文件哈希失败：{filePath}", ex);
                return null;
            }
        }
    }
}
