// <copyright file="ModInfo.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using AstralPartyModManager.Models;

namespace AstralPartyModManager.Models
{
    /// <summary>
    /// Mod信息
    /// </summary>
    public class ModInfo
    {
        /// <summary>
        /// Mod名称
        /// </summary>
        public string Name { get; set; } = "未知 Mod";

        /// <summary>
        /// 作者
        /// </summary>
        public string Author { get; set; } = "未知作者";

        /// <summary>
        /// 版本
        /// </summary>
        public string Version { get; set; } = "1.0.0";

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Mod文件夹路径
        /// </summary>
        public string FolderPath { get; set; } = string.Empty;

        /// <summary>
        /// 目标文件列表
        /// </summary>
        public List<string> TargetFiles { get; set; } = new();

        /// <summary>
        /// 目标配置列表
        /// </summary>
        public List<ModTarget> Targets { get; set; } = new();

        /// <summary>
        /// Mod类型
        /// </summary>
        public ModType Type { get; set; } = ModType.Unknown;

        /// <summary>
        /// 冲突文件列表
        /// </summary>
        public List<string> Conflicts { get; set; } = new();

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; } = DateTime.MinValue;

        /// <summary>
        /// 是否已弃用
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// 弃用原因
        /// </summary>
        public string DeprecatedReason { get; set; } = string.Empty;

        /// <summary>
        /// 支持的游戏版本
        /// </summary>
        public string GameVersion { get; set; } = string.Empty;

        /// <summary>
        /// 标签列表
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// 扫描错误信息
        /// </summary>
        public string ScanError { get; set; } = string.Empty;

        /// <summary>
        /// 验证Mod信息是否有效
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrEmpty(this.FolderPath) || !Directory.Exists(this.FolderPath))
            {
                errorMessage = "Mod 文件夹不存在";
                return false;
            }

            if (this.Type == ModType.Unknown)
            {
                errorMessage = "不支持的 Mod 类型（视为已弃用）";
                this.IsDeprecated = true;
                this.DeprecatedReason = "不支持的 Mod 类型";

                // 不返回 false，仍然加载但标记为弃用
            }

            // Comprehensive 类型不需要 TargetFiles，它使用 Targets 配置
            if (
                this.Type != ModType.Comprehensive
                && (this.TargetFiles == null || this.TargetFiles.Count == 0)
            )
            {
                // 如果是 Comprehensive 但 Targets 也是空的，才认为无效
                if (
                    this.Type != ModType.Comprehensive
                    || this.Targets == null
                    || this.Targets.Count == 0
                )
                {
                    errorMessage = "Mod 没有目标文件";
                    return false;
                }
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
