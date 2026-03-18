using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace AstralPartyModManager
{
    // 数据类使用 record 类型（C# 9+ 推荐写法）
    public record BackupInfo(
        string ModName,
        DateTime BackupTime,
        ModType ModType,
        List<BackupFile> Files
    );

    public record BackupFile(
        string RelativePath,
        bool IsNewFile,
        string OriginalHash
    );

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
        private readonly string _gamePath;
        private readonly string _backupRoot;
        private readonly string _dataPath;

        public BackupManager(string gamePath, string dataPath = null, string backupPath = null)
        {
            _gamePath = gamePath;
            _dataPath = dataPath ?? Path.Combine(gamePath, "AstralParty_CN_Data");

            if (!string.IsNullOrEmpty(backupPath))
            {
                _backupRoot = backupPath;
            }
            else
            {
                string executableDir = AppDomain.CurrentDomain.BaseDirectory;
                _backupRoot = Path.Combine(executableDir, "data", "backups");
            }

            if (!Directory.Exists(_backupRoot))
            {
                Directory.CreateDirectory(_backupRoot);
            }

            Logger.Info($"BackupManager 初始化 - 游戏路径：{gamePath}, 备份根目录：{_backupRoot}");
        }

        /// <summary>
        /// 根据 Mod 类型获取目标目录
        /// </summary>
        public string GetTargetDirectory(ModType modType)
        {
            switch (modType)
            {
                case ModType.Addressables:
                    return Path.Combine(_dataPath, "StreamingAssets", "aa", "StandaloneWindows64");
                case ModType.Voice:
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    return Path.Combine(appData, "Low", "feimo", "AstralParty_CN", "com.unity.addressables", "AssetBundles");
                case ModType.Plugin:
                    return _gamePath;
                default:
                    return _gamePath;
            }
        }

        /// <summary>
        /// 获取文件在游戏目录中的实际路径
        /// </summary>
        public string GetGameFilePath(string modFile, string targetDir, ModType modType)
        {
            if (modType == ModType.Plugin)
            {
                string fileName = Path.GetFileName(modFile);
                string gameFilePath = Path.Combine(_gamePath, fileName);
                return gameFilePath;
            }

            if (modFile.Contains("StreamingAssets") || modFile.Contains("StandaloneWindows64"))
            {
                var parts = modFile.Split(new[] { "StreamingAssets", "StandaloneWindows64" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    return Path.Combine(_dataPath, "StreamingAssets", "aa", "StandaloneWindows64") + parts[1];
                }
            }

            string fileNameOnly = Path.GetFileName(modFile);
            return Path.Combine(targetDir, fileNameOnly);
        }

        /// <summary>
        /// 获取相对路径
        /// </summary>
        public string GetRelativePath(string fullPath, string basePath)
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
        /// 准备启用 Mod：备份即将被覆盖的文件
        /// </summary>
        public BackupResult PrepareEnableMod(string modName, List<string> modFolderFiles, ModType modType, string modFolderPath)
        {
            var result = new BackupResult(false, string.Empty, 0, 0, new List<string>());
            Logger.Info($"准备启用 Mod '{modName}'，开始备份游戏文件...");

            var backupInfo = new BackupInfo(modName, DateTime.Now, modType, new List<BackupFile>());

            string targetDir = GetTargetDirectory(modType);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupDir = Path.Combine(_backupRoot, timestamp, "game_files");

            var filesToInstall = GetFilesToInstall(modFolderPath, modType, targetDir);

            Logger.Debug($"Mod '{modName}' 将安装 {filesToInstall.Count} 个文件");

            foreach (var installFile in filesToInstall)
            {
                string gameFilePath = installFile.Value;
                string relativePath = GetRelativePath(gameFilePath, _gamePath);

                bool fileExists = File.Exists(gameFilePath);

                backupInfo.Files.Add(new BackupFile(
                    relativePath,
                    !fileExists,
                    fileExists ? ComputeFileHash(gameFilePath) : null
                ));

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

            SaveBackupInfo(backupInfo, timestamp);

            result = result with
            {
                Success = result.Errors.Count == 0,
                Message = result.Errors.Count == 0
                    ? $"备份完成：{result.BackedUpCount} 个文件已备份，{result.NewFileCount} 个新文件"
                    : $"备份完成，但有 {result.Errors.Count} 个错误"
            };

            Logger.Info($"Mod '{modName}' 备份完成：{result.Message}");
            return result;
        }

        /// <summary>
        /// 获取会被安装到游戏目录的文件列表
        /// </summary>
        private Dictionary<string, string> GetFilesToInstall(string modFolderPath, ModType modType, string targetDir)
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
                ScanStandardDirectory(standardDir, modType, targetDir, result);
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

            if (modType == ModType.Plugin)
            {
                sourceDir = modFolderPath;
            }

            foreach (var file in Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file).ToLower() == "mod.json") continue;
                if (file.Contains("\\docs\\")) continue;

                string fileName = Path.GetFileName(file);
                string gameFilePath;

                if (modType == ModType.Plugin)
                {
                    gameFilePath = Path.Combine(_gamePath, fileName);
                }
                else
                {
                    gameFilePath = Path.Combine(targetDir, fileName);
                }

                result[file] = gameFilePath;
            }

            return result;
        }

        /// <summary>
        /// 扫描标准格式目录
        /// </summary>
        private void ScanStandardDirectory(string standardDir, ModType modType, string targetDir, Dictionary<string, string> result)
        {
            foreach (var file in Directory.GetFiles(standardDir, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(file).ToLower() == "mod.json") continue;
                if (file.Contains("\\docs\\")) continue;

                string relativePath = GetRelativePath(file, standardDir);
                string gameFilePath;

                if (modType == ModType.Addressables)
                {
                    if (relativePath.StartsWith("StreamingAssets", StringComparison.OrdinalIgnoreCase))
                    {
                        string subPath = relativePath.Substring("StreamingAssets".Length).TrimStart('\\', '/');
                        gameFilePath = Path.Combine(_dataPath, "StreamingAssets", "aa", "StandaloneWindows64", subPath);
                    }
                    else if (relativePath.StartsWith("Plugins", StringComparison.OrdinalIgnoreCase))
                    {
                        string subPath = relativePath.Substring("Plugins".Length).TrimStart('\\', '/');
                        gameFilePath = Path.Combine(_gamePath, subPath);
                    }
                    else
                    {
                        gameFilePath = Path.Combine(_gamePath, relativePath);
                    }
                }
                else if (modType == ModType.Voice)
                {
                    string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if (relativePath.StartsWith("Low", StringComparison.OrdinalIgnoreCase))
                    {
                        string subPath = relativePath.Substring("Low".Length).TrimStart('\\', '/');
                        gameFilePath = Path.Combine(appData, "Low", "feimo", "AstralParty_CN", subPath);
                    }
                    else
                    {
                        gameFilePath = Path.Combine(appData, "Low", "feimo", "AstralParty_CN", relativePath);
                    }
                }
                else if (modType == ModType.Plugin)
                {
                    gameFilePath = Path.Combine(_gamePath, relativePath);
                }
                else
                {
                    gameFilePath = Path.Combine(_gamePath, relativePath);
                }

                result[file] = gameFilePath;
            }
        }

        /// <summary>
        /// 查找模组子文件夹
        /// </summary>
        private string FindModSubdirectory(string folderPath)
        {
            var priorityNames = new[] { "模组", "根模组", "根目录替换", "StandaloneWindows64" };
            foreach (var name in priorityNames)
            {
                var subdir = Directory.GetDirectories(folderPath)
                    .FirstOrDefault(d => Path.GetFileName(d).Contains(name));
                if (subdir != null) return subdir;
            }
            return null;
        }

        /// <summary>
        /// 查找语音文件子文件夹
        /// </summary>
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

        /// <summary>
        /// 查找标准格式目录
        /// </summary>
        private string FindStandardDirectory(string folderPath, string dirName)
        {
            return Directory.GetDirectories(folderPath)
                .FirstOrDefault(d => Path.GetFileName(d).ToLower() == dirName);
        }

        /// <summary>
        /// 保存备份信息
        /// </summary>
        private void SaveBackupInfo(BackupInfo info, string timestamp)
        {
            try
            {
                string jsonPath = Path.Combine(_backupRoot, timestamp, "backup_info.json");
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
        /// 加载备份信息
        /// </summary>
        private BackupInfo LoadBackupInfo(string timestamp)
        {
            try
            {
                string jsonPath = Path.Combine(_backupRoot, timestamp, "backup_info.json");
                if (File.Exists(jsonPath))
                {
                    string json = File.ReadAllText(jsonPath);
                    return JsonSerializer.Deserialize<BackupInfo>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载备份信息失败：{ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// 禁用 Mod：恢复被覆盖的文件，删除新增的文件
        /// </summary>
        public RestoreResult DisableMod(string modName, ModType modType)
        {
            var result = new RestoreResult(false, string.Empty, 0, 0, new List<string>());
            Logger.Info($"准备禁用 Mod '{modName}'，开始恢复游戏文件...");

            var backupInfo = FindLatestBackupInfo(modName);

            if (backupInfo == null || backupInfo.Files.Count == 0)
            {
                var (success, message) = DisableModByType(modType, result);
                result = result with { Success = success, Message = message };
                return result;
            }

            string timestamp = GetTimestampFromBackupInfo(backupInfo);
            string backupDir = Path.Combine(_backupRoot, timestamp, "game_files");

            Logger.Debug($"找到备份信息：{backupInfo.Files.Count} 个文件，备份时间：{timestamp}");

            foreach (var file in backupInfo.Files)
            {
                try
                {
                    string gameFilePath = Path.Combine(_gamePath, file.RelativePath);

                    if (file.IsNewFile)
                    {
                        if (File.Exists(gameFilePath))
                        {
                            File.Delete(gameFilePath);
                            result = result with { DeletedCount = result.DeletedCount + 1 };
                            Logger.Debug($"已删除新文件：{file.RelativePath}");
                        }
                        else
                        {
                            Logger.Debug($"文件不存在，跳过删除：{file.RelativePath}");
                        }
                    }
                    else
                    {
                        string backupPath = Path.Combine(backupDir, file.RelativePath);
                        if (File.Exists(backupPath))
                        {
                            string destDir = Path.GetDirectoryName(gameFilePath);
                            if (!Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                            }
                            File.Copy(backupPath, gameFilePath, overwrite: true);
                            result = result with { RestoredCount = result.RestoredCount + 1 };
                            Logger.Debug($"已恢复文件：{file.RelativePath}");
                        }
                        else if (File.Exists(gameFilePath))
                        {
                            File.Delete(gameFilePath);
                            result = result with { DeletedCount = result.DeletedCount + 1 };
                            Logger.Debug($"备份不存在，已删除文件：{file.RelativePath}");
                        }
                        else
                        {
                            Logger.Debug($"文件和备份都不存在，跳过：{file.RelativePath}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"恢复文件失败 {file.RelativePath}: {ex.Message}");
                    Logger.Warning($"恢复文件失败：{file.RelativePath}", ex);
                }
            }

            result = result with
            {
                Success = result.Errors.Count == 0,
                Message = result.Errors.Count == 0
                    ? $"恢复完成：{result.RestoredCount} 个文件已恢复，{result.DeletedCount} 个文件已删除"
                    : $"恢复完成，但有 {result.Errors.Count} 个错误"
            };

            Logger.Info($"Mod '{modName}' 禁用完成：{result.Message}");
            return result;
        }

        /// <summary>
        /// 根据 Mod 类型禁用（没有备份信息时使用）
        /// </summary>
        private (bool Success, string Message) DisableModByType(ModType modType, RestoreResult result)
        {
            if (modType == ModType.Plugin)
            {
                var commonPluginFiles = new[] { "version.dll", "speedhack_config.json", "winmm.dll", "dinput8.dll", "UnityDoorstop.dll", "doorstop_config.ini" };
                int deletedCount = 0;

                foreach (var fileName in commonPluginFiles)
                {
                    try
                    {
                        string filePath = Path.Combine(_gamePath, fileName);
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

            Logger.Warning($"无法确定 {modType} 类型 Mod 的具体文件，请使用'恢复纯净'功能或手动清理");
            result.Errors.Add($"无法确定 {modType} 类型 Mod 的具体文件");
            return (false, $"无法确定 {modType} 类型 Mod 的具体文件");
        }

        /// <summary>
        /// 查找指定 Mod 的最新备份信息
        /// </summary>
        private BackupInfo FindLatestBackupInfo(string modName)
        {
            var backupDirs = Directory.GetDirectories(_backupRoot)
                .OrderByDescending(d => d)
                .ToList();

            foreach (var dir in backupDirs)
            {
                string timestamp = Path.GetFileName(dir);
                var info = LoadBackupInfo(timestamp);
                if (info != null && info.ModName == modName)
                {
                    return info;
                }
            }

            return null;
        }

        /// <summary>
        /// 从备份信息获取时间戳
        /// </summary>
        private string GetTimestampFromBackupInfo(BackupInfo info)
        {
            return info.BackupTime.ToString("yyyyMMdd_HHmmss");
        }

        /// <summary>
        /// 恢复所有备份（完全恢复游戏）
        /// </summary>
        public void RestoreAllBackups()
        {
            var backupDirs = Directory.GetDirectories(_backupRoot);
            if (backupDirs.Length == 0)
            {
                throw new Exception("没有找到备份文件");
            }

            var allBackupFiles = new Dictionary<string, string>();

            foreach (var backupDir in backupDirs)
            {
                string gameFilesDir = Path.Combine(backupDir, "game_files");
                if (!Directory.Exists(gameFilesDir)) continue;

                foreach (var file in Directory.GetFiles(gameFilesDir, "*.*", SearchOption.AllDirectories))
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
                    string destFile = Path.Combine(_gamePath, kvp.Key);
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
        /// 清理旧备份（保留最近 5 个）
        /// </summary>
        public void CleanupOldBackups(int keepCount = 5)
        {
            var backupDirs = Directory.GetDirectories(_backupRoot);
            if (backupDirs.Length <= keepCount) return;

            var sortedDirs = new List<(string Path, DateTime Time)>();

            foreach (var dir in backupDirs)
            {
                string dirName = Path.GetFileName(dir);
                if (DateTime.TryParseExact(dirName, "yyyyMMdd_HHmmss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out DateTime time))
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
        /// 计算文件 SHA256 哈希值
        /// </summary>
        private string ComputeFileHash(string filePath)
        {
            try
            {
                using (var sha256 = SHA256.Create())
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
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
