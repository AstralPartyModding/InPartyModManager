// <copyright file="BackupModels.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using AstralPartyModManager.Models;

namespace AstralPartyModManager.Models
{
    // 数据类使用 record 类型（C# 9+ 推荐写法）

    /// <summary>
    /// 备份信息
    /// </summary>
    /// <param name="ModName">Mod名称</param>
    /// <param name="BackupTime">备份时间</param>
    /// <param name="ModType">Mod类型</param>
    /// <param name="Files">备份文件列表</param>
    public record BackupInfo(
        string ModName,
        DateTime BackupTime,
        ModType ModType,
        List<BackupFile> Files
    );

    /// <summary>
    /// 备份文件信息
    /// </summary>
    /// <param name="FullPath">完整路径</param>
    /// <param name="IsNewFile">是否是新文件</param>
    /// <param name="OriginalHash">原始文件哈希</param>
    public record BackupFile(string FullPath, bool IsNewFile, string OriginalHash);

    /// <summary>
    /// 备份结果
    /// </summary>
    /// <param name="Success">是否成功</param>
    /// <param name="Message">消息</param>
    /// <param name="BackedUpCount">备份数量</param>
    /// <param name="NewFileCount">新文件数量</param>
    /// <param name="Errors">错误列表</param>
    public record BackupResult(
        bool Success,
        string Message,
        int BackedUpCount,
        int NewFileCount,
        List<string> Errors
    );

    /// <summary>
    /// 恢复结果
    /// </summary>
    /// <param name="Success">是否成功</param>
    /// <param name="Message">消息</param>
    /// <param name="RestoredCount">恢复数量</param>
    /// <param name="DeletedCount">删除数量</param>
    /// <param name="Errors">错误列表</param>
    public record RestoreResult(
        bool Success,
        string Message,
        int RestoredCount,
        int DeletedCount,
        List<string> Errors
    );
}
