using MelonLoader;
using AstralPartyMod.Core;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[assembly: MelonInfo(typeof(AstralPartyModManager.MelonLoader.ModManagerPlugin), "星引擎Mod管理器", "2.0.0", "AstralPartyModManager")]
[assembly: MelonGame(null, null)]

namespace AstralPartyModManager.MelonLoader
{
    /// <summary>
    /// 星引擎Mod管理器 - MelonLoader版本
    /// 
    /// 基于AstralPartyMod.Core核心库构建，提供运行时资源替换功能
    /// 相比原版文件替换方式，此版本：
    /// - 不修改游戏原始文件
    /// - 支持热重载
    /// - 支持多Mod共存
    /// - 支持运行时启用/禁用
    /// </summary>
    public class ModManagerPlugin : CoreMod
    {
        #region Mod基本信息
        
        protected override string ModName => "星引擎Mod管理器";
        protected override string ModVersion => "2.0.0";
        protected override string ModAuthor => "AstralPartyModManager";
        protected override string[] ResourceDirectories => new[] { "Resources" };
        
        #endregion
        
        #region 配置
        
        protected override KeyCode ReloadKey => KeyCode.F8;
        protected override bool EnableStatistics => true;
        protected virtual KeyCode StatisticsKey => KeyCode.F9;
        protected override bool EnableDetailedLogging => true;
        
        #endregion
        
        #region 组件
        
        /// <summary>
        /// Mod扫描器
        /// </summary>
        private ModScanner _modScanner = null!;
        
        /// <summary>
        /// 已扫描到的Mod列表
        /// </summary>
        private List<ModInfo> _scannedMods = new();
        
        /// <summary>
        /// 已启用的Mod
        /// </summary>
        private Dictionary<string, ModEntry> _enabledMods = new();
        
        /// <summary>
        /// UI管理器
        /// </summary>
        private InPartyModUI? _uiManager;
        
        #endregion
        
        #region 生命周期
        
        public override void OnInitializeMelon()
        {
            // 调用基类初始化
            base.OnInitializeMelon();
            
            MelonLogger.Msg("========================================");
            MelonLogger.Msg("星引擎Mod管理器 v2.0.0 (MelonLoader版)");
            MelonLogger.Msg("基于AstralPartyMod.Core核心库");
            MelonLogger.Msg("========================================");
            
            try
            {
                // 初始化Mod扫描器
                _modScanner = new ModScanner(GetModDirectory());
                
                // 扫描可用Mod
                ScanAvailableMods();
                
                // 自动启用所有扫描到的Mod
                EnableAllScannedMods();
                
                // 初始化UI管理器
                _uiManager = new InPartyModUI(this);
                
                MelonLogger.Msg($"Mod管理器初始化完成，共发现 {_scannedMods.Count} 个Mod");
                MelonLogger.Msg("快捷键：");
                MelonLogger.Msg("  F1 - 打开/关闭Mod管理器UI");
                MelonLogger.Msg("  F8 - 重新加载所有Mod资源");
                MelonLogger.Msg("  F9 - 显示统计信息");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Mod管理器初始化失败: {ex.Message}");
                MelonLogger.Error(ex.StackTrace);
            }
        }
        
        public override void OnUpdate()
        {
            // 调用基类更新（处理热重载和统计）
            base.OnUpdate();
            
            // 更新UI
            _uiManager?.OnUpdate();
        }
        
        public override void OnGUI()
        {
            // 渲染UI
            _uiManager?.OnGUI();
        }
        
        #endregion
        
        #region Mod管理
        
        /// <summary>
        /// 扫描可用Mod
        /// </summary>
        private void ScanAvailableMods()
        {
            MelonLogger.Msg("扫描可用Mod...");
            
            _scannedMods = _modScanner.ScanMods();
            
            foreach (var mod in _scannedMods)
            {
                if (mod.IsValid(out string error))
                {
                    MelonLogger.Msg($"  [发现] {mod.Name} v{mod.Version} by {mod.Author}");
                }
                else
                {
                    MelonLogger.Warning($"  [跳过] {mod.Name} - {error}");
                }
            }
        }
        
        /// <summary>
        /// 启用所有扫描到的Mod
        /// </summary>
        private void EnableAllScannedMods()
        {
            foreach (var modInfo in _scannedMods)
            {
                if (!modInfo.IsValid(out string error))
                {
                    continue;
                }
                
                try
                {
                    EnableMod(modInfo);
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"启用Mod '{modInfo.Name}' 失败: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// 启用单个Mod
        /// </summary>
        private void EnableMod(ModInfo modInfo)
        {
            if (_enabledMods.ContainsKey(modInfo.Name))
            {
                MelonLogger.Warning($"Mod '{modInfo.Name}' 已启用，跳过");
                return;
            }
            
            MelonLogger.Msg($"启用Mod: {modInfo.Name}");
            
            // 创建Mod条目
            var modEntry = new ModEntry
            {
                Info = modInfo,
                ResourcePaths = new List<string>()
            };
            
            // 扫描Mod资源文件
            ScanModResources(modEntry);
            
            // 注册到资源替换器
            foreach (var resourcePath in modEntry.ResourcePaths)
            {
                string fileName = Path.GetFileName(resourcePath);
                ResourceReplacer.AddReplacement(fileName, resourcePath);
                
                if (EnableDetailedLogging)
                {
                    MelonLogger.Msg($"  [注册] {fileName}");
                }
            }
            
            _enabledMods[modInfo.Name] = modEntry;
            
            MelonLogger.Msg($"  已启用，注册了 {modEntry.ResourcePaths.Count} 个资源");
        }
        
        /// <summary>
        /// 禁用单个Mod
        /// </summary>
        private void DisableMod(ModInfo modInfo)
        {
            if (!_enabledMods.TryGetValue(modInfo.Name, out var modEntry))
            {
                return;
            }
            
            MelonLogger.Msg($"禁用Mod: {modInfo.Name}");
            
            // 从资源替换器中移除
            foreach (var resourcePath in modEntry.ResourcePaths)
            {
                string fileName = Path.GetFileName(resourcePath);
                ResourceReplacer.RemoveReplacement(fileName);
            }
            
            _enabledMods.Remove(modInfo.Name);
            
            MelonLogger.Msg($"  已禁用，移除了 {modEntry.ResourcePaths.Count} 个资源");
        }
        
        /// <summary>
        /// 扫描Mod资源文件
        /// </summary>
        private void ScanModResources(ModEntry modEntry)
        {
            if (!Directory.Exists(modEntry.Info.FolderPath))
            {
                return;
            }
            
            // 扫描所有.bundle文件
            var bundleFiles = Directory.GetFiles(
                modEntry.Info.FolderPath, 
                "*.bundle", 
                SearchOption.AllDirectories
            );
            
            modEntry.ResourcePaths.AddRange(bundleFiles);
        }
        
        /// <summary>
        /// 重新加载资源（重写基类方法）
        /// </summary>
        protected override void ReloadResources()
        {
            MelonLogger.Msg("重新加载所有Mod资源...");
            
            // 清空当前资源
            ResourceReplacer.Clear();
            _enabledMods.Clear();
            
            // 重新扫描和启用
            ScanAvailableMods();
            EnableAllScannedMods();
            
            MelonLogger.Msg($"重新加载完成，共 {_enabledMods.Count} 个Mod，{ResourceReplacer.Count} 个资源");
            
            _uiManager?.ShowStatusMessage("已刷新所有Mod");
        }
        
        /// <summary>
        /// 显示统计信息
        /// </summary>
        protected void ShowStatistics()
        {
            MelonLogger.Msg("========================================");
            MelonLogger.Msg("星引擎Mod管理器 统计信息");
            MelonLogger.Msg("========================================");
            MelonLogger.Msg($"已加载Mod: {_enabledMods.Count}");
            MelonLogger.Msg($"总资源数: {ResourceReplacer.Count}");
            MelonLogger.Msg($"已替换次数: {ReplacedCount}");
            
            if (_enabledMods.Count > 0)
            {
                MelonLogger.Msg("---");
                MelonLogger.Msg("已启用的Mod:");
                foreach (var mod in _enabledMods.Values)
                {
                    MelonLogger.Msg($"  - {mod.Info.Name} v{mod.Info.Version} ({mod.ResourcePaths.Count} 资源)");
                }
            }
            
            MelonLogger.Msg("========================================");
        }
        
        #endregion
        
        #region UI接口方法
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public UIStatistics GetStatistics()
        {
            return new UIStatistics
            {
                EnabledModCount = _enabledMods.Count,
                AvailableModCount = _scannedMods.Count,
                TotalResources = ResourceReplacer.Count,
                ReplacedCount = ReplacedCount
            };
        }
        
        /// <summary>
        /// 获取所有Mod列表
        /// </summary>
        public List<ModListItem> GetAllMods()
        {
            var result = new List<ModListItem>();
            
            foreach (var modInfo in _scannedMods)
            {
                bool isEnabled = _enabledMods.ContainsKey(modInfo.Name);
                int resourceCount = isEnabled ? _enabledMods[modInfo.Name].ResourcePaths.Count : 0;
                
                result.Add(new ModListItem
                {
                    Name = modInfo.Name,
                    Version = modInfo.Version,
                    Author = modInfo.Author,
                    IsEnabled = isEnabled,
                    ResourceCount = resourceCount
                });
            }
            
            return result;
        }
        
        /// <summary>
        /// 启用指定Mod
        /// </summary>
        public void EnableMod(string modName)
        {
            var modInfo = _scannedMods.FirstOrDefault(m => m.Name == modName);
            if (modInfo != null && !modInfo.IsValid(out _))
            {
                modInfo = null;
            }
            
            if (modInfo == null)
            {
                MelonLogger.Warning($"无法启用Mod '{modName}'：未找到或无效");
                return;
            }
            
            try
            {
                EnableMod(modInfo);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"启用Mod '{modName}' 失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 禁用指定Mod
        /// </summary>
        public void DisableMod(string modName)
        {
            var modInfo = _scannedMods.FirstOrDefault(m => m.Name == modName);
            if (modInfo == null) return;
            
            try
            {
                DisableMod(modInfo);
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"禁用Mod '{modName}' 失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 启用所有Mod
        /// </summary>
        public void EnableAllMods()
        {
            foreach (var modInfo in _scannedMods)
            {
                if (!_enabledMods.ContainsKey(modInfo.Name))
                {
                    EnableMod(modInfo);
                }
            }
        }
        
        /// <summary>
        /// 禁用所有Mod
        /// </summary>
        public void DisableAllMods()
        {
            // 复制列表避免遍历时修改
            var modNames = _enabledMods.Keys.ToList();
            foreach (var modName in modNames)
            {
                var modInfo = _scannedMods.FirstOrDefault(m => m.Name == modName);
                if (modInfo != null)
                {
                    DisableMod(modInfo);
                }
            }
        }
        
        /// <summary>
        /// 刷新所有Mod
        /// </summary>
        public void ReloadAllMods()
        {
            ReloadResources();
        }
        
        /// <summary>
        /// 获取Mod的分类信息
        /// </summary>
        public List<CategoryInfo> GetModCategories(string modName)
        {
            var result = new List<CategoryInfo>();
            
            // 从Mod目录结构推断分类
            var modInfo = _scannedMods.FirstOrDefault(m => m.Name == modName);
            if (modInfo == null || !Directory.Exists(modInfo.FolderPath))
            {
                return result;
            }
            
            // 扫描子目录作为分类
            var subDirs = Directory.GetDirectories(modInfo.FolderPath);
            foreach (var dir in subDirs)
            {
                string categoryName = Path.GetFileName(dir);
                int fileCount = Directory.GetFiles(dir, "*.bundle", SearchOption.AllDirectories).Length;
                
                result.Add(new CategoryInfo
                {
                    Name = categoryName,
                    Description = $"{fileCount} 个资源",
                    IsEnabled = true  // 默认启用，实际应从配置读取
                });
            }
            
            // 如果没有子目录，添加默认分类
            if (result.Count == 0)
            {
                int fileCount = Directory.GetFiles(modInfo.FolderPath, "*.bundle", SearchOption.AllDirectories).Length;
                result.Add(new CategoryInfo
                {
                    Name = "默认",
                    Description = $"{fileCount} 个资源",
                    IsEnabled = true
                });
            }
            
            return result;
        }
        
        /// <summary>
        /// 设置分类启用状态
        /// </summary>
        public void SetCategoryEnabled(string modName, string categoryName, bool enabled)
        {
            MelonLogger.Msg($"设置Mod '{modName}' 的分类 '{categoryName}' 为 {(enabled ? "启用" : "禁用")}");
            // 实际实现需要维护分类状态并重新加载资源
            // 这里仅记录日志，完整实现需要更复杂的资源管理
        }
        
        #endregion
    }
    
    /// <summary>
    /// Mod条目
    /// </summary>
    public class ModEntry
    {
        public ModInfo Info { get; set; } = null!;
        public List<string> ResourcePaths { get; set; } = new();
    }
}
