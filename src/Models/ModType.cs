// <copyright file="ModType.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace AstralPartyModManager.Models
{
    /// <summary>
    /// Mod类型枚举
    /// </summary>
    public enum ModType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        Unknown,

        /// <summary>
        /// Addressables资源包
        /// </summary>
        Addressables,

        /// <summary>
        /// 语音替换Mod
        /// </summary>
        Voice,

        /// <summary>
        /// 插件Mod
        /// </summary>
        Plugin,

        /// <summary>
        /// 综合Mod（多文件）
        /// </summary>
        Comprehensive,

        /// <summary>
        /// MelonLoader插件
        /// </summary>
        MelonLoader,

        /// <summary>
        /// 新格式Mod：包含Mods/和ModResources/目录结构
        /// </summary>
        InPartyMod,
    }
}
