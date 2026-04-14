// AstralParty Mod Manager - 主窗口
// Copyright (c) AstralParty Modding Community. All rights reserved.

namespace AstralPartyModManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    public partial class MainForm : Form
    {
        private ModManager modManager;
        private ModScanner modScanner;
        private BackupManager backupManager;
        private ConfigManager configManager;
        private ModStateManager modStateManager;

        private static readonly Color PrimaryColor = Color.FromArgb(13, 110, 253);
        private static readonly Color PrimaryHoverColor = Color.FromArgb(10, 84, 200);
        private static readonly Color SuccessColor = Color.FromArgb(25, 135, 84);
        private static readonly Color WarningColor = Color.FromArgb(255, 193, 7);
        private static readonly Color DangerColor = Color.FromArgb(220, 53, 69);
        private static readonly Color BackgroundColor = Color.FromArgb(248, 249, 250);
        private static readonly Color PanelBackColor = Color.White;
        private static readonly Color GridHeaderBackColor = Color.FromArgb(233, 236, 239);
        private static readonly Color GridHeaderForeColor = Color.FromArgb(33, 37, 41);
        private static readonly Color GridHeaderSelectionColor = Color.FromArgb(13, 110, 253);

        private System.Windows.Forms.Button btnInstallZip;
        private System.Windows.Forms.Label lblDragHint;

        public MainForm()
        {
            this.InitializeComponent();
            this.InitializeConfig();
            this.InitializeStateManager();
            this.InitializeManagers();
            this.ApplyModernStyles();
            this.InitializeDragDrop();
            this.InitializeDebugMode();
            this.LoadModList();
        }

        private void InitializeStateManager()
        {
            string statePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "mod_states.txt"
            );
            this.modStateManager = new ModStateManager(statePath);
        }

        private void InitializeConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            this.configManager = new ConfigManager(configPath);
        }

        private void InitializeManagers()
        {
            string gamePath = this.configManager.GamePath;
            string modPath = this.configManager.ModPath;
            string dataPath = Path.Combine(gamePath, "AstralParty_CN_Data");

            if (!Directory.Exists(gamePath) || !Directory.Exists(modPath))
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                gamePath = Path.GetFullPath(Path.Combine(basePath, ".."));
                modPath = Path.Combine(gamePath, "mods");

                this.configManager.GamePath = gamePath;
                this.configManager.ModPath = modPath;
            }

            this.modManager = new ModManager(gamePath, dataPath);
            this.modScanner = new ModScanner(modPath);

            string backupPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "data",
                "backups"
            );
            this.backupManager = new BackupManager(gamePath, dataPath, backupPath);
        }

        private void ApplyModernStyles()
        {
            this.BackColor = BackgroundColor;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new Size(900, 600);
            this.MinimumSize = new Size(800, 500);
            this.MaximumSize = new Size(1200, 800);

            this.dgvMods.BackgroundColor = PanelBackColor;
            this.dgvMods.BorderStyle = BorderStyle.None;
            this.dgvMods.GridColor = Color.FromArgb(230, 230, 230);
            this.dgvMods.RowHeadersVisible = false;
            this.dgvMods.AllowUserToAddRows = false;
            this.dgvMods.AllowUserToDeleteRows = false;
            this.dgvMods.ReadOnly = true;
            this.dgvMods.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvMods.MultiSelect = false;
            this.dgvMods.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            this.dgvMods.ColumnHeadersDefaultCellStyle.Font = new Font(
                "Microsoft YaHei UI",
                9F,
                FontStyle.Bold
            );
            this.dgvMods.ColumnHeadersDefaultCellStyle.ForeColor = GridHeaderForeColor;
            this.dgvMods.ColumnHeadersDefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            this.dgvMods.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBackColor;
            this.dgvMods.EnableHeadersVisualStyles = false;
            this.dgvMods.ColumnHeadersHeight = 35;
            this.dgvMods.ColumnHeadersDefaultCellStyle.SelectionForeColor =
                GridHeaderSelectionColor;

            this.dgvMods.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F);
            this.dgvMods.DefaultCellStyle.Padding = new Padding(5);
            this.dgvMods.RowTemplate.Height = 35;

            this.grpMods.BackColor = Color.Transparent;
            this.grpMods.ForeColor = Color.FromArgb(80, 80, 80);
            this.grpMods.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

            this.grpActions.BackColor = Color.Transparent;
            this.grpActions.ForeColor = Color.FromArgb(80, 80, 80);
            this.grpActions.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

            StyleButton(this.btnRefresh, PrimaryColor);
            StyleButton(this.btnEnable, SuccessColor);
            StyleButton(this.btnDisable, WarningColor);
            StyleButton(this.btnRestore, DangerColor);
            StyleButton(this.btnLaunchGame, Color.FromArgb(102, 126, 234));
            
            if (this.btnInstallZip != null)
                StyleButton(this.btnInstallZip, PrimaryColor);

            this.lblStatus.Font = new Font("Microsoft YaHei UI", 9F);
            this.lblStatus.ForeColor = Color.FromArgb(80, 80, 80);

            this.menuStrip.BackColor = PanelBackColor;
            this.menuStrip.ForeColor = Color.FromArgb(80, 80, 80);
            this.menuStrip.Font = new Font("Microsoft YaHei UI", 9F);

            // 美化拖放提示区域
            if (this.lblDragHint != null)
            {
                this.lblDragHint.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Italic);
                this.lblDragHint.ForeColor = Color.FromArgb(108, 117, 125);
                this.lblDragHint.BackColor = Color.FromArgb(240, 248, 255);
                this.lblDragHint.BorderStyle = BorderStyle.FixedSingle;
                this.lblDragHint.TextAlign = ContentAlignment.MiddleCenter;
            }
        }

        private void InitializeDragDrop()
        {
            // 启用拖放
            this.AllowDrop = true;
            this.dgvMods.AllowDrop = true;
            
            // 注册事件
            this.DragEnter += MainForm_DragEnter;
            this.DragDrop += MainForm_DragDrop;
            
            this.AppendDebugLog("已启用拖放支持");
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                // 检查是否有zip文件
                if (files.Any(f => f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)))
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var zipFiles = files.Where(f => f.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (!zipFiles.Any())
            {
                MessageBox.Show("只支持拖放ZIP文件，请拖放Mod打包后的ZIP文件。", "格式错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 处理每个拖放的ZIP文件
            int successCount = 0;
            int failCount = 0;
            List<string> errors = new List<string>();

            foreach (var zipPath in zipFiles)
            {
                this.AppendDebugLog($"开始处理拖放ZIP: {zipPath}");
                
                if (!InstallZipMod(zipPath, out string error))
                {
                    failCount++;
                    errors.Add($"{Path.GetFileName(zipPath)}: {error}");
                    this.AppendDebugLog($"安装失败: {error}");
                }
                else
                {
                    successCount++;
                    this.AppendDebugLog($"安装成功: {zipPath}");
                }
            }

            // 刷新列表
            this.LoadModList();

            // 显示结果
            string resultMessage =
                $"处理完成:\n成功: {successCount} 个\n失败: {failCount} 个\n\n" +
                string.Join("\n", errors);

            if (failCount == 0)
            {
                MessageBox.Show($"成功安装 {successCount} 个Mod!", "安装完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(resultMessage, "部分安装失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 安装ZIP Mod到mods目录
        /// </summary>
        private bool InstallZipMod(string zipPath, out string errorMessage)
        {
            errorMessage = string.Empty;
            
            try
            {
                string gamePath = this.configManager.GamePath;
                if (!Directory.Exists(gamePath))
                {
                    errorMessage = "游戏目录不存在，请先在设置中配置正确的游戏路径";
                    return false;
                }

                string modPath = this.configManager.ModPath;
                if (!Directory.Exists(modPath))
                {
                    Directory.CreateDirectory(modPath);
                }

                // 使用ZipArchive解压
                using var zip = System.IO.Compression.ZipFile.OpenRead(zipPath);
                
                this.AppendDebugLog($"ZIP包含 {zip.Entries.Count} 个文件");

                // 检查ZIP结构，判断是否是标准Mod包
                bool hasStandardStructure = zip.Entries.Any(e =>
                    e.FullName.StartsWith("Mods/", StringComparison.OrdinalIgnoreCase) ||
                    e.FullName.StartsWith("ModResources/", StringComparison.OrdinalIgnoreCase)
                );

                if (hasStandardStructure)
                {
                    // 标准结构，直接解压到游戏根目录（Mods和ModResources会自动对应）
                    this.AppendDebugLog($"检测到标准打包结构，解压到游戏根目录");
                    ExtractZipToDirectory(zip, gamePath);
                }
                else
                {
                    // 获取ZIP名称作为Mod名称
                    string modName = Path.GetFileNameWithoutExtension(zipPath);
                    // 清理名称 - 移除版本后缀如 -v1.0.0-for-Player
                    modName = System.Text.RegularExpressions.Regex.Replace(modName, @"[-_]v?\d+(\.\d+)*.*$", "").Trim('-', '_');
                    
                    string targetDir = Path.Combine(modPath, modName);
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);
                    
                    this.AppendDebugLog($"非标准结构，创建Mod目录: {targetDir}");
                    ExtractZipToDirectory(zip, targetDir);
                }

                this.AppendDebugLog($"解压完成: {zipPath}");
                return true;
            }
            catch (IOException ioEx)
            {
                errorMessage = $"文件访问错误: {ioEx.Message}\n请检查文件是否被占用";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 解压ZIP到目标目录
        /// </summary>
        private void ExtractZipToDirectory(System.IO.Compression.ZipArchive zip, string targetDir)
        {
            foreach (var entry in zip.Entries)
            {
                // 跳过目录条目
                if (string.IsNullOrEmpty(entry.Name))
                    continue;

                // 处理正斜杠路径
                string entryPath = entry.FullName.Replace('/', Path.DirectorySeparatorChar);
                string targetPath = Path.Combine(targetDir, entryPath);
                string? targetDirPath = Path.GetDirectoryName(targetPath);
                
                if (!string.IsNullOrEmpty(targetDirPath) && !Directory.Exists(targetDirPath))
                {
                    Directory.CreateDirectory(targetDirPath);
                }

                // 如果文件已存在，删除
                if (File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }
                
                // 手动解压，避免扩展方法依赖问题
                using (var fileStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
                using (var entryStream = entry.Open())
                {
                    entryStream.CopyTo(fileStream);
                }
            }
        }

        /// <summary>
        /// 手动选择ZIP文件安装
        /// </summary>
        private void BtnInstallZip_Click(object sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "ZIP压缩包 (*.zip)|*.zip|所有文件 (*.*)|*.*";
            openFileDialog.Title = "选择Mod ZIP包";
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string zipPath = openFileDialog.FileName;
                
                if (InstallZipMod(zipPath, out string error))
                {
                    this.LoadModList();
                    MessageBox.Show($"安装成功!\n已安装: {Path.GetFileName(zipPath)}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"安装失败: {error}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InitializeDebugMode()
        {
            this.pnlDebug.Visible = this.configManager.DebugMode;
            this.menuDebugMode.Checked = this.configManager.DebugMode;

            if (this.configManager.DebugMode)
            {
                this.AppendDebugLog("=== Debug 模式已启用 ===");
                this.AppendDebugLog($"游戏路径：{this.configManager.GamePath}");
                this.AppendDebugLog($"Mods 路径：{this.configManager.ModPath}");
                this.AppendDebugLog(string.Empty);
            }
        }

        private void ToggleDebugMode()
        {
            this.configManager.DebugMode = !this.configManager.DebugMode;
            this.pnlDebug.Visible = this.configManager.DebugMode;
            this.menuDebugMode.Checked = this.configManager.DebugMode;

            if (this.configManager.DebugMode)
            {
                this.AppendDebugLog("=== Debug 模式已启用 ===");
                this.AppendDebugLog($"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                this.AppendDebugLog($"游戏路径：{this.configManager.GamePath}");
                this.AppendDebugLog($"Mods 路径：{this.configManager.ModPath}");
                this.AppendDebugLog(string.Empty);
            }
            else
            {
                this.AppendDebugLog("=== Debug 模式已禁用 ===");
            }
        }

        private void AppendDebugLog(string message)
        {
            if (!this.configManager.DebugMode)
            {
                return;
            }

            this.txtDebugLog.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}"
            );
            this.txtDebugLog.ScrollToCaret();
        }

        private static void StyleButton(Button button, Color baseColor)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = baseColor;
            button.ForeColor = Color.White;
            button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            button.Cursor = Cursors.Hand;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(baseColor, 0.1f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(baseColor, 0.1f);
        }

        private void LoadModList()
        {
            this.AppendDebugLog("--- 开始扫描 Mod ---");
            var scanResult = this.modScanner.ScanModsDetailed();
            var mods = scanResult.Mods;

            this.AppendDebugLog(
                $"扫描完成：发现 {mods.Count} 个 Mod，失败 {scanResult.FailedCount} 个"
            );

            if (this.configManager.DebugMode && scanResult.Errors.Count > 0)
            {
                this.AppendDebugLog("扫描错误/警告:");
                foreach (var error in scanResult.Errors)
                {
                    this.AppendDebugLog($"  - {error}");
                }
            }

            this.dgvMods.Rows.Clear();
            int rowIndex = 1;

            foreach (var mod in mods)
            {
                string updateTimeStr =
                    mod.UpdateTime == DateTime.MinValue
                        ? "未知"
                        : mod.UpdateTime.ToString("yyyy-MM-dd HH:mm");

                string deprecatedStr = mod.IsDeprecated ? "是" : "否";

                bool isEnabled = this.modStateManager.IsEnabled(mod.Name);
                string enabledStr = isEnabled ? "✅ 已启用" : "❌ 未启用";

                this.dgvMods.Rows.Add(
                    rowIndex++,
                    mod.Name,
                    mod.Author,
                    mod.Version,
                    updateTimeStr,
                    deprecatedStr,
                    enabledStr
                );

                this.dgvMods.Rows[this.dgvMods.Rows.Count - 1].Tag = mod;

                if (mod.IsDeprecated)
                {
                    this.dgvMods.Rows[this.dgvMods.Rows.Count - 1].DefaultCellStyle.BackColor =
                        Color.LightCoral;
                }
                else if (isEnabled)
                {
                    this.dgvMods.Rows[this.dgvMods.Rows.Count - 1].DefaultCellStyle.BackColor =
                        Color.LightGreen;
                }

                if (this.configManager.DebugMode)
                {
                    this.AppendDebugLog(
                        $"[{mod.Name}] 类型={mod.Type}, 文件数={mod.TargetFiles.Count}, 弃用={mod.IsDeprecated}"
                    );
                    if (!string.IsNullOrEmpty(mod.ScanError))
                    {
                        this.AppendDebugLog($"  扫描错误：{mod.ScanError}");
                    }
                }
            }

            this.lblStatus.Text = $"发现 {mods.Count} 个 Mod";
            this.AppendDebugLog("--- Mod 列表加载完成 ---");

            // 如果有扫描错误，提示用户
            if (scanResult.Errors.Any())
            {
                string message =
                    $"扫描完成，但发现 {scanResult.Errors.Count} 个错误/警告。\n\n"
                    + $"成功加载: {scanResult.SuccessCount} 个\n"
                    + $"失败/跳过: {scanResult.FailedCount} 个\n\n"
                    + $"请打开 Debug 模式查看详细错误信息。";

                if (scanResult.FailedCount > 0)
                {
                    MessageBox.Show(
                        message,
                        "扫描完成有错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            this.LoadModList();
        }

        private void BtnEnable_Click(object sender, EventArgs e)
        {
            if (this.dgvMods.CurrentRow?.Tag is ModInfo modInfo)
            {
                if (modInfo.IsDeprecated)
                {
                    var confirmResult = MessageBox.Show(
                        $"⚠️ 警告：Mod '{modInfo.Name}' 已被标记为弃用或不支持的类型。\n\n"
                            + $"原因：{modInfo.DeprecatedReason}\n\n"
                            + $"继续启用可能会导致游戏不稳定或其他问题。\n\n"
                            + $"确定要继续启用吗？",
                        "弃用 Mod 警告",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (confirmResult != DialogResult.Yes)
                    {
                        return;
                    }
                }

                try
                {
                    this.AppendDebugLog($"开始启用 Mod: {modInfo.Name}");
                    this.AppendDebugLog($"当前启用状态: {this.modStateManager.IsEnabled(modInfo.Name)}");

                    // 修复：先预测文件，再备份原始文件，最后安装Mod
                    // 1. 预测Mod会影响哪些文件
                    var targetFiles = this.modManager.GetModTargetFiles(modInfo);

                    if (targetFiles.Count == 0)
                    {
                        this.UpdateStatusLabel($"❌ 启用失败：无法检测到Mod文件");
                        Logger.Error($"启用 Mod 失败：{modInfo.Name} - 无法检测到Mod文件");
                        MessageBox.Show(
                            $"启用失败：无法检测到Mod文件\n请检查Mod文件夹结构是否正确。",
                            "启用 Mod 失败",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return;
                    }

                    this.AppendDebugLog($"预测到 {targetFiles.Count} 个目标文件");
                    foreach (var file in targetFiles.Take(10))
                    {
                        string fullPath;
                        string[] parts = file.Split('\\');
                        if (parts.Length > 0 && parts[0].Length == 1 && !parts[0].Contains('.'))
                        {
                            string driveLetter = parts[0];
                            string restPath = string.Join("\\", parts.Skip(1));
                            fullPath = $"{driveLetter}:\\{restPath}";
                        }
                        else if (file.Contains(':'))
                        {
                            fullPath = file;
                        }
                        else
                        {
                            fullPath = Path.Combine(this.configManager.GamePath, file);
                        }
                        this.AppendDebugLog($"  - {file}: 目标存在={File.Exists(fullPath)}, 当前已是Mod文件={this.modStateManager.IsEnabled(modInfo.Name)}");
                    }
                    if (targetFiles.Count > 10)
                    {
                        this.AppendDebugLog($"  ... 还有 {targetFiles.Count - 10} 个文件未列出");
                    }

                    // 2. 备份原始文件（此时文件还是原始状态）
                    var backupResult = this.backupManager.PrepareEnableModFromFiles(
                        modInfo.Name,
                        targetFiles
                    );

                    if (!backupResult.Success)
                    {
                        Logger.Warning($"备份警告：{backupResult.Message}");
                        if (this.configManager.DebugMode)
                        {
                            this.AppendDebugLog($"备份警告：{backupResult.Message}");
                        }
                    }
                    else if (this.configManager.DebugMode)
                    {
                        this.AppendDebugLog($"备份完成");
                        this.AppendDebugLog($"备份文件数：{backupResult.BackedUpCount}");
                    }

                    // 3. 安装Mod（此时才覆盖文件）
                    var installResult = this.modManager.EnableMod(modInfo);

                    if (installResult.Success)
                    {
                        this.modStateManager.SetEnabled(modInfo.Name, true);
                        this.UpdateStatusLabel($"✅ 已启用 Mod: {modInfo.Name}");
                        Logger.Info($"已启用 Mod: {modInfo.Name}");

                        if (this.configManager.DebugMode)
                        {
                            this.AppendDebugLog(
                                $"安装成功：安装了 {installResult.InstalledFiles.Count} 个文件，替换了 {installResult.ReplacedFiles.Count} 个文件"
                            );
                            foreach (var file in installResult.InstalledFiles)
                            {
                                this.AppendDebugLog($"  + 新增：{file}");
                            }

                            foreach (var file in installResult.ReplacedFiles)
                            {
                                this.AppendDebugLog($"  ~ 替换：{file}");
                            }
                        }
                    }
                    else
                    {
                        // 如果Message为空，但有错误，使用第一个错误作为消息
                        string statusMessage;
                        if (!string.IsNullOrEmpty(installResult.Message))
                        {
                            statusMessage = installResult.Message;
                        }
                        else if (installResult.Errors.Any())
                        {
                            statusMessage = installResult.Errors.First();
                        }
                        else
                        {
                            statusMessage = "未知错误（错误列表为空，请到日志查看详情）";
                        }

                        this.UpdateStatusLabel($"❌ 启用失败：{statusMessage}");
                        Logger.Error($"启用 Mod 失败：{modInfo.Name} - {statusMessage}");

                        // 始终输出错误到debug日志，无论是否启用debug模式？不，debug面板只在debug模式显示
                        this.AppendDebugLog($"安装失败：{statusMessage}");
                        foreach (var error in installResult.Errors)
                        {
                            this.AppendDebugLog($"  错误：{error}");
                        }

                        // 无论是否弃用，都显示错误对话框给用户，包含所有错误详情
                        string errorTitle = modInfo.IsDeprecated
                            ? "启用弃用 Mod 失败"
                            : "启用 Mod 失败";
                        string displayMessage;
                        if (!string.IsNullOrEmpty(installResult.Message))
                        {
                            displayMessage = installResult.Message;
                        }
                        else if (installResult.Errors.Any())
                        {
                            displayMessage = installResult.Errors.First();
                        }
                        else
                        {
                            displayMessage = "未知错误，请检查Debug日志和应用程序日志";
                        }

                        string errorMessage = $"启用失败：{displayMessage}\n\n";

                        if (installResult.Errors.Any())
                        {
                            errorMessage += "详细错误：\n";
                            foreach (var error in installResult.Errors.Take(10))
                            {
                                errorMessage += $"  - {error}\n";
                            }

                            if (installResult.Errors.Count > 10)
                            {
                                errorMessage +=
                                    $"  ... 还有 {installResult.Errors.Count - 10} 个错误\n";
                            }

                            errorMessage += "\n";
                        }

                        errorMessage += "请检查 Debug 日志查看完整信息。";

                        MessageBox.Show(
                            errorMessage,
                            errorTitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    }

                    // 只更新当前行的启用状态，不重新加载整个列表，保留 Debug 日志
                    this.UpdateCurrentRowState();
                }
                catch (Exception ex)
                {
                    Logger.Error($"启用 Mod 失败：{modInfo.Name}", ex);
                    this.UpdateStatusLabel($"❌ 启用失败：{ex.Message}");

                    // 无论是否弃用，都显示错误对话框给用户，包含完整堆栈信息
                    string errorTitle = modInfo.IsDeprecated
                        ? "启用弃用 Mod 失败"
                        : "启用 Mod 失败";
                    string errorMessage =
                        $"启用失败：{ex.Message}\n\n"
                        + $"异常类型：{ex.GetType().Name}\n"
                        + $"堆栈跟踪：\n{ex.StackTrace}\n\n"
                        + $"详细信息已记录到日志。";
                    MessageBox.Show(
                        errorMessage,
                        errorTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void BtnDisable_Click(object sender, EventArgs e)
        {
            if (this.dgvMods.CurrentRow?.Tag is ModInfo modInfo)
            {
                if (modInfo.IsDeprecated)
                {
                    var confirmResult = MessageBox.Show(
                        $"⚠️ 警告：Mod '{modInfo.Name}' 已被标记为弃用。\n\n"
                            + $"原因：{modInfo.DeprecatedReason}\n\n"
                            + $"确定要禁用此 Mod 吗？",
                        "弃用 Mod 确认",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (confirmResult != DialogResult.Yes)
                    {
                        return;
                    }
                }

                try
                {
                    this.AppendDebugLog($"开始禁用 Mod: {modInfo.Name}");

                    var restoreResult = this.backupManager.DisableMod(modInfo.Name, modInfo.Type);

                    if (restoreResult.Success)
                    {
                        this.modStateManager.SetEnabled(modInfo.Name, false);
                        this.UpdateStatusLabel($"✅ 已禁用 Mod: {modInfo.Name}");
                        Logger.Info($"已禁用 Mod: {modInfo.Name}");

                        if (this.configManager.DebugMode)
                        {
                            this.AppendDebugLog(
                                $"恢复成功：删除了 {restoreResult.DeletedCount} 个文件，恢复了 {restoreResult.RestoredCount} 个文件"
                            );
                        }
                    }
                    else
                    {
                        this.UpdateStatusLabel($"⚠️ 禁用完成但有错误：{restoreResult.Message}");
                        Logger.Warning(
                            $"禁用 Mod 出现问题：{modInfo.Name} - {restoreResult.Message}"
                        );

                        if (this.configManager.DebugMode)
                        {
                            this.AppendDebugLog($"恢复出现问题：{restoreResult.Message}");
                        }
                    }

                    // 只更新当前行的启用状态，不重新加载整个列表，保留 Debug 日志
                    this.UpdateCurrentRowState();
                }
                catch (Exception ex)
                {
                    Logger.Error($"禁用 Mod 失败：{modInfo.Name}", ex);
                    this.UpdateStatusLabel($"❌ 禁用失败：{ex.Message}");

                    // 无论是否弃用，都显示错误对话框给用户，包含完整堆栈信息
                    string errorTitle = modInfo.IsDeprecated
                        ? "禁用弃用 Mod 失败"
                        : "禁用 Mod 失败";
                    string errorMessage =
                        $"禁用失败：{ex.Message}\n\n"
                        + $"异常类型：{ex.GetType().Name}\n"
                        + $"堆栈跟踪：\n{ex.StackTrace}\n\n"
                        + $"详细信息已记录到日志。";
                    MessageBox.Show(
                        errorMessage,
                        errorTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void UpdateStatusLabel(string message)
        {
            this.lblStatus.Text = message;
            this.lblActionHint.Text = "✅ 操作完成";

            var timer = new System.Windows.Forms.Timer { Interval = 3000 };
            timer.Tick += (s, e) =>
            {
                this.lblStatus.Text = "状态：就绪";
                this.lblActionHint.Text = "💡 选择一个 Mod 后点击启用或禁用";
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "这将恢复所有被修改的游戏文件到原始状态。\n确定要继续吗？",
                "确认恢复",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    this.backupManager.RestoreAllBackups();
                    MessageBox.Show(
                        "游戏文件已恢复到原始状态",
                        "成功",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"恢复失败：{ex.Message}",
                        "错误",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
                }
            }
        }

        private void MenuSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(this.configManager))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    this.InitializeManagers();
                    this.LoadModList();
                }
            }
        }

        private void MenuDebugMode_Click(object sender, EventArgs e)
        {
            this.ToggleDebugMode();
        }

        private void BtnLaunchGame_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(
                    new ProcessStartInfo("steam://rungameid/2622000") { UseShellExecute = true }
                );
                this.UpdateStatusLabel("🎮 正在启动游戏...");
                Logger.Info("正在启动游戏 (steam://rungameid/2622000)");
            }
            catch (Exception ex)
            {
                Logger.Error("启动游戏失败", ex);
                MessageBox.Show(
                    $"启动游戏失败：{ex.Message}\n\n请确保已安装 Steam 客户端。",
                    "启动游戏失败",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvMods;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnEnable;
        private System.Windows.Forms.Button btnDisable;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnLaunchGame;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblWarning;
        private System.Windows.Forms.GroupBox grpMods;
        private System.Windows.Forms.GroupBox grpActions;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.menuDebugMode = new System.Windows.Forms.ToolStripMenuItem();
            this.grpMods = new System.Windows.Forms.GroupBox();
            this.dgvMods = new System.Windows.Forms.DataGridView();
            this.grpActions = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnEnable = new System.Windows.Forms.Button();
            this.btnDisable = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnLaunchGame = new System.Windows.Forms.Button();
            this.btnInstallZip = new System.Windows.Forms.Button();
            this.lblDragHint = new System.Windows.Forms.Label();
            this.lblActionHint = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblWarning = new System.Windows.Forms.Label();
            this.pnlDebug = new System.Windows.Forms.Panel();
            this.txtDebugLog = new System.Windows.Forms.TextBox();
            this.lblDebugTitle = new System.Windows.Forms.Label();
            this.grpMods.SuspendLayout();
            this.grpActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.dgvMods).BeginInit();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();

            // menuStrip
            this.menuStrip.Items.AddRange(
                new System.Windows.Forms.ToolStripItem[] { this.menuTools }
            );
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(784, 25);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "菜单栏";

            // menuTools
            this.menuTools.DropDownItems.AddRange(
                new System.Windows.Forms.ToolStripItem[] { this.menuSettings, this.menuDebugMode }
            );
            this.menuTools.Name = "menuTools";
            this.menuTools.Size = new System.Drawing.Size(50, 21);
            this.menuTools.Text = "工具 (&T)";

            // menuSettings
            this.menuSettings.Name = "menuSettings";
            this.menuSettings.Size = new System.Drawing.Size(120, 22);
            this.menuSettings.Text = "设置 (&S)";
            this.menuSettings.Click += new System.EventHandler(this.MenuSettings_Click);

            // menuDebugMode
            this.menuDebugMode.Name = "menuDebugMode";
            this.menuDebugMode.Size = new System.Drawing.Size(120, 22);
            this.menuDebugMode.Text = "Debug 模式 (&D)";
            this.menuDebugMode.CheckOnClick = true;
            this.menuDebugMode.Click += new System.EventHandler(this.MenuDebugMode_Click);

            // grpMods
            this.grpMods.Controls.Add(this.dgvMods);
            this.grpMods.Location = new System.Drawing.Point(12, 35);
            this.grpMods.Name = "grpMods";
            this.grpMods.Size = new System.Drawing.Size(856, 380);
            this.grpMods.TabIndex = 1;
            this.grpMods.TabStop = false;
            this.grpMods.Text = "Mod 列表";
            this.grpMods.BackColor = Color.Transparent;
            this.grpMods.ForeColor = Color.FromArgb(80, 80, 80);
            this.grpMods.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            this.grpMods.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

            // dgvMods
            this.dgvMods.AllowUserToAddRows = false;
            this.dgvMods.AllowUserToDeleteRows = false;
            this.dgvMods.AutoSizeColumnsMode = System
                .Windows
                .Forms
                .DataGridViewAutoSizeColumnsMode
                .Fill;
            this.dgvMods.ColumnHeadersHeightSizeMode = System
                .Windows
                .Forms
                .DataGridViewColumnHeadersHeightSizeMode
                .AutoSize;
            this.dgvMods.Columns.AddRange(
                new System.Windows.Forms.DataGridViewColumn[]
                {
                    CreateNumberColumn("序号", 50),
                    CreateColumn("名称", 150),
                    CreateColumn("作者", 100),
                    CreateColumn("版本", 70),
                    CreateColumn("更新时间", 130),
                    CreateColumn("是否弃用", 80),
                    CreateColumn("启用状态", 90),
                }
            );
            this.dgvMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMods.Location = new System.Drawing.Point(3, 22);
            this.dgvMods.MultiSelect = false;
            this.dgvMods.Name = "dgvMods";
            this.dgvMods.ReadOnly = true;
            this.dgvMods.RowHeadersVisible = false;
            this.dgvMods.SelectionMode = System
                .Windows
                .Forms
                .DataGridViewSelectionMode
                .FullRowSelect;
            this.dgvMods.Size = new System.Drawing.Size(850, 395);
            this.dgvMods.TabIndex = 0;
            this.dgvMods.DefaultCellStyle.SelectionBackColor = GridHeaderSelectionColor;
            this.dgvMods.DefaultCellStyle.SelectionForeColor = Color.White;
            this.dgvMods.ColumnHeadersDefaultCellStyle.SelectionBackColor =
                GridHeaderSelectionColor;
            this.dgvMods.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;

            // grpActions
            this.grpActions.Controls.Add(this.btnRestore);
            this.grpActions.Controls.Add(this.btnDisable);
            this.grpActions.Controls.Add(this.btnEnable);
            this.grpActions.Controls.Add(this.btnRefresh);
            this.grpActions.Controls.Add(this.btnLaunchGame);
            this.grpActions.Controls.Add(this.lblActionHint);
            this.grpActions.Location = new System.Drawing.Point(12, 461);
            this.grpActions.Controls.Add(this.btnRefresh);
            this.grpActions.Controls.Add(this.btnEnable);
            this.grpActions.Controls.Add(this.btnDisable);
            this.grpActions.Controls.Add(this.btnRestore);
            this.grpActions.Controls.Add(this.btnInstallZip);
            this.grpActions.Controls.Add(this.btnLaunchGame);
            this.grpActions.Controls.Add(this.lblDragHint);
            this.grpActions.Controls.Add(this.lblActionHint);
            this.grpActions.Name = "grpActions";
            this.grpActions.Size = new System.Drawing.Size(856, 130);
            this.grpActions.TabIndex = 2;
            this.grpActions.TabStop = false;
            this.grpActions.Text = "操作";
            this.grpActions.BackColor = Color.Transparent;
            this.grpActions.ForeColor = Color.FromArgb(80, 80, 80);
            this.grpActions.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            this.grpActions.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // btnRefresh
            this.btnRefresh.Location = new System.Drawing.Point(10, 25);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 35);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "🔄 刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            this.btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnEnable
            this.btnEnable.Location = new System.Drawing.Point(120, 25);
            this.btnEnable.Name = "btnEnable";
            this.btnEnable.Size = new System.Drawing.Size(100, 35);
            this.btnEnable.TabIndex = 1;
            this.btnEnable.Text = "✅ 启用";
            this.btnEnable.UseVisualStyleBackColor = true;
            this.btnEnable.Click += new System.EventHandler(this.BtnEnable_Click);
            this.btnEnable.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnDisable
            this.btnDisable.Location = new System.Drawing.Point(230, 25);
            this.btnDisable.Name = "btnDisable";
            this.btnDisable.Size = new System.Drawing.Size(100, 35);
            this.btnDisable.TabIndex = 2;
            this.btnDisable.Text = "⏸️ 禁用";
            this.btnDisable.UseVisualStyleBackColor = true;
            this.btnDisable.Click += new System.EventHandler(this.BtnDisable_Click);
            this.btnDisable.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnRestore
            this.btnRestore.Location = new System.Drawing.Point(340, 25);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(110, 35);
            this.btnRestore.TabIndex = 3;
            this.btnRestore.Text = "♻️ 恢复纯净";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.BtnRestore_Click);
            this.btnRestore.Anchor = AnchorStyles.Top | AnchorStyles.Left;

            // btnInstallZip
            this.btnInstallZip.Location = new System.Drawing.Point(460, 25);
            this.btnInstallZip.Name = "btnInstallZip";
            this.btnInstallZip.Size = new System.Drawing.Size(140, 35);
            this.btnInstallZip.TabIndex = 5;
            this.btnInstallZip.Text = "📦 安装ZIP";
            this.btnInstallZip.UseVisualStyleBackColor = true;
            this.btnInstallZip.Click += new System.EventHandler(this.BtnInstallZip_Click);
            this.btnInstallZip.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // btnLaunchGame
            this.btnLaunchGame.Location = new System.Drawing.Point(610, 25);
            this.btnLaunchGame.Name = "btnLaunchGame";
            this.btnLaunchGame.Size = new System.Drawing.Size(110, 35);
            this.btnLaunchGame.TabIndex = 4;
            this.btnLaunchGame.Text = "🎮 启动游戏";
            this.btnLaunchGame.UseVisualStyleBackColor = true;
            this.btnLaunchGame.Click += new System.EventHandler(this.BtnLaunchGame_Click);
            this.btnLaunchGame.Anchor = AnchorStyles.Top | AnchorStyles.Right;

            // lblDragHint
            this.lblDragHint.Location = new System.Drawing.Point(10, 70);
            this.lblDragHint.Name = "lblDragHint";
            this.lblDragHint.Size = new System.Drawing.Size(830, 32);
            this.lblDragHint.Text = "📥 拖动ZIP文件到此处即可自动安装Mod";
            this.lblDragHint.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.lblDragHint.BackColor = Color.FromArgb(240, 248, 255);
            this.lblDragHint.ForeColor = Color.FromArgb(50, 80, 120);
            this.lblDragHint.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Italic);
            // lblActionHint
            this.lblActionHint.AutoSize = true;
            this.lblActionHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblActionHint.ForeColor = Color.FromArgb(100, 100, 100);
            this.lblActionHint.Location = new System.Drawing.Point(10, 70);
            this.lblActionHint.Name = "lblActionHint";
            this.lblActionHint.Size = new System.Drawing.Size(200, 17);
            this.lblActionHint.Text = "💡 选择一个 Mod 后点击启用或禁用";

            // lblWarning
            this.lblWarning.AutoSize = true;
            this.lblWarning.Font = new System.Drawing.Font("Microsoft YaHei UI", 8.5F);
            this.lblWarning.ForeColor = Color.FromArgb(220, 53, 69);
            this.lblWarning.Location = new System.Drawing.Point(12, 605);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(860, 17);
            this.lblWarning.TabIndex = 3;
            this.lblWarning.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.lblWarning.Text =
                "⚠️ 提示：Mod 禁用可能失效，建议恢复纯净后使用 Steam 验证游戏完整性检查";
            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            this.lblStatus.Location = new System.Drawing.Point(12, 628);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(80, 17);
            this.lblStatus.TabIndex = 4;
            this.lblStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.lblStatus.Text = "状态：就绪";
            // pnlDebug
            this.pnlDebug.Location = new System.Drawing.Point(12, 655);
            this.pnlDebug.Name = "pnlDebug";
            this.pnlDebug.Size = new System.Drawing.Size(856, 150);
            this.pnlDebug.TabIndex = 5;
            this.pnlDebug.BackColor = Color.FromArgb(30, 30, 30);
            this.pnlDebug.BorderStyle = BorderStyle.FixedSingle;
            this.pnlDebug.Controls.Add(this.txtDebugLog);
            this.pnlDebug.Controls.Add(this.lblDebugTitle);
            this.pnlDebug.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            // lblDebugTitle
            this.lblDebugTitle.AutoSize = true;
            this.lblDebugTitle.Font = new System.Drawing.Font(
                "Microsoft YaHei UI",
                9F,
                FontStyle.Bold
            );
            this.lblDebugTitle.ForeColor = Color.FromArgb(0, 255, 100);
            this.lblDebugTitle.Location = new System.Drawing.Point(10, 8);
            this.lblDebugTitle.Name = "lblDebugTitle";
            this.lblDebugTitle.Size = new System.Drawing.Size(100, 17);
            this.lblDebugTitle.Text = "🔧 Debug 日志";

            // txtDebugLog
            this.txtDebugLog.Location = new System.Drawing.Point(10, 30);
            this.txtDebugLog.Name = "txtDebugLog";
            this.txtDebugLog.Size = new System.Drawing.Size(836, 110);
            this.txtDebugLog.TabIndex = 0;
            this.txtDebugLog.Multiline = true;
            this.txtDebugLog.ScrollBars = ScrollBars.Vertical;
            this.txtDebugLog.ReadOnly = true;
            this.txtDebugLog.BackColor = Color.FromArgb(20, 20, 20);
            this.txtDebugLog.ForeColor = Color.FromArgb(0, 255, 100);
            this.txtDebugLog.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtDebugLog.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = BackgroundColor;
            this.ClientSize = new System.Drawing.Size(884, 820);
            this.Controls.Add(this.pnlDebug);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.grpActions);
            this.Controls.Add(this.grpMods);
            this.Controls.Add(this.menuStrip);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = true;
            this.MinimumSize = new Size(800, 700);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "✨ APmodManager";
            this.grpMods.ResumeLayout(false);
            this.grpActions.ResumeLayout(false);
            this.grpActions.PerformLayout();
            this.pnlDebug.ResumeLayout(false);
            this.pnlDebug.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)this.dgvMods).EndInit();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuTools;
        private System.Windows.Forms.ToolStripMenuItem menuSettings;
        private System.Windows.Forms.ToolStripMenuItem menuDebugMode;
        private System.Windows.Forms.Label lblActionHint;
        private System.Windows.Forms.Panel pnlDebug;
        private System.Windows.Forms.TextBox txtDebugLog;
        private System.Windows.Forms.Label lblDebugTitle;

        private static System.Windows.Forms.DataGridViewColumn CreateColumn(
            string headerText,
            int width
        )
        {
            var column = new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                HeaderText = headerText,
                Width = width,
                FillWeight = width,
                SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable,
            };
            return column;
        }

        private static System.Windows.Forms.DataGridViewColumn CreateNumberColumn(
            string headerText,
            int width
        )
        {
            var column = new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                HeaderText = headerText,
                Width = width,
                FillWeight = width,
                SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    BackColor = Color.FromArgb(240, 240, 240),
                },
            };
            return column;
        }

        /// <summary>
        /// 更新当前选中行的启用状态，不重新加载整个列表.
        /// </summary>
        private void UpdateCurrentRowState()
        {
            if (this.dgvMods.CurrentRow == null)
            {
                return;
            }

            var modInfo = this.dgvMods.CurrentRow.Tag as ModInfo;
            if (modInfo == null)
            {
                return;
            }

            bool isEnabled = this.modStateManager.IsEnabled(modInfo.Name);
            string enabledStr = isEnabled ? "✅ 已启用" : "❌ 未启用";

            // 启用状态在第 6 列（索引从 0 开始）
            this.dgvMods.CurrentRow.Cells[6].Value = enabledStr;

            // 更新背景色
            if (modInfo.IsDeprecated)
            {
                this.dgvMods.CurrentRow.DefaultCellStyle.BackColor = Color.LightCoral;
            }
            else if (isEnabled)
            {
                this.dgvMods.CurrentRow.DefaultCellStyle.BackColor = Color.LightGreen;
            }
            else
            {
                this.dgvMods.CurrentRow.DefaultCellStyle.BackColor = this.dgvMods
                    .DefaultCellStyle
                    .BackColor;
            }
        }
    }
}
