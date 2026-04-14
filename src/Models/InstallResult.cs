// AstralParty Mod Manager - 安装结果模型
// Copyright (c) AstralParty Modding Community. All rights reserved.

using System.Collections.Generic;

namespace AstralPartyModManager.Models
{
    /// <summary>
    /// 安装结果记录
    /// </summary>
    public class InstallResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 已安装的文件列表
        /// </summary>
        public List<string> InstalledFiles { get; set; } = new();

        /// <summary>
        /// 被替换的文件列表
        /// </summary>
        public List<string> ReplacedFiles { get; set; } = new();

        /// <summary>
        /// 错误列表
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallResult"/> class.
        /// </summary>
        public InstallResult()
        {
            this.Success = false;
            this.Message = string.Empty;
            this.InstalledFiles = new();
            this.ReplacedFiles = new();
            this.Errors = new();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstallResult"/> class.
        /// </summary>
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
}
