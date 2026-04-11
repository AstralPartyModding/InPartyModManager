// <copyright file="ScanResult.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Collections.Generic;
using AstralPartyModManager.Models;

namespace AstralPartyModManager.Models
{
    /// <summary>
    /// 扫描结果
    /// </summary>
    public class ScanResult
    {
        /// <summary>
        /// 扫描到的Mod列表
        /// </summary>
        public List<ModInfo> Mods { get; set; } = new();

        /// <summary>
        /// 错误列表
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// 总文件夹数
        /// </summary>
        public int TotalFolders { get; set; }

        /// <summary>
        /// 成功扫描数
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失败扫描数
        /// </summary>
        public int FailedCount { get; set; }
    }
}
