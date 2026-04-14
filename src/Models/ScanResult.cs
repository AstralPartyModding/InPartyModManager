// AstralParty Mod Manager - 扫描结果模型
// Copyright (c) AstralParty Modding Community. All rights reserved.

using System.Collections.Generic;

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
