// AstralParty Mod Manager - Mod 安装目标模型
// Copyright (c) AstralParty Modding Community. All rights reserved.

namespace AstralPartyModManager.Models
{
    /// <summary>
    /// Mod目标配置
    /// </summary>
    public class ModTarget
    {
        /// <summary>
        /// 目标类型
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// 源文件路径
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// 目标路径
        /// </summary>
        public string TargetPath { get; set; } = string.Empty;
    }
}
