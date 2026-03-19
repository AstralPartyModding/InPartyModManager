using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace AstralPartyModManager
{
    public partial class MainForm : Form
    {
        private ModManager _modManager;
        private ModScanner _modScanner;
        private BackupManager _backupManager;
        private ConfigManager _configManager;
        private ModStateManager _modStateManager;

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

        public MainForm()
        {
            InitializeComponent();
            InitializeConfig();
            InitializeStateManager();
            InitializeManagers();
            ApplyModernStyles();
            InitializeDebugMode();
            LoadModList();
        }

        private void InitializeStateManager()
        {
            string statePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mod_states.txt");
            _modStateManager = new ModStateManager(statePath);
        }

        private void InitializeConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            _configManager = new ConfigManager(configPath);
        }

        private void InitializeManagers()
        {
            string gamePath = _configManager.GamePath;
            string modPath = _configManager.ModPath;
            string dataPath = Path.Combine(gamePath, "AstralParty_CN_Data");

            if (!Directory.Exists(gamePath) || !Directory.Exists(modPath))
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                gamePath = Path.GetFullPath(Path.Combine(basePath, ".."));
                modPath = Path.Combine(gamePath, "mods");

                _configManager.GamePath = gamePath;
                _configManager.ModPath = modPath;
            }

            _modManager = new ModManager(gamePath, dataPath);
            _modScanner = new ModScanner(modPath);

            string backupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "backups");
            _backupManager = new BackupManager(gamePath, dataPath, backupPath);
        }

        private void ApplyModernStyles()
        {
            this.BackColor = BackgroundColor;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;

            dgvMods.BackgroundColor = PanelBackColor;
            dgvMods.BorderStyle = BorderStyle.None;
            dgvMods.GridColor = Color.FromArgb(230, 230, 230);
            dgvMods.RowHeadersVisible = false;
            dgvMods.AllowUserToAddRows = false;
            dgvMods.AllowUserToDeleteRows = false;
            dgvMods.ReadOnly = true;
            dgvMods.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMods.MultiSelect = false;
            dgvMods.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dgvMods.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            dgvMods.ColumnHeadersDefaultCellStyle.ForeColor = GridHeaderForeColor;
            dgvMods.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvMods.ColumnHeadersDefaultCellStyle.BackColor = GridHeaderBackColor;
            dgvMods.EnableHeadersVisualStyles = false;
            dgvMods.ColumnHeadersHeight = 35;
            dgvMods.ColumnHeadersDefaultCellStyle.SelectionForeColor = GridHeaderSelectionColor;

            dgvMods.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F);
            dgvMods.DefaultCellStyle.Padding = new Padding(5);
            dgvMods.RowTemplate.Height = 30;

            grpMods.BackColor = Color.Transparent;
            grpMods.ForeColor = Color.FromArgb(80, 80, 80);
            grpMods.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

            grpActions.BackColor = Color.Transparent;
            grpActions.ForeColor = Color.FromArgb(80, 80, 80);
            grpActions.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);

            StyleButton(btnRefresh, PrimaryColor);
            StyleButton(btnEnable, SuccessColor);
            StyleButton(btnDisable, WarningColor);
            StyleButton(btnRestore, DangerColor);

            lblStatus.Font = new Font("Microsoft YaHei UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(80, 80, 80);

            menuStrip.BackColor = PanelBackColor;
            menuStrip.ForeColor = Color.FromArgb(80, 80, 80);
            menuStrip.Font = new Font("Microsoft YaHei UI", 9F);
        }

        private void InitializeDebugMode()
        {
            pnlDebug.Visible = _configManager.DebugMode;
            menuDebugMode.Checked = _configManager.DebugMode;

            if (_configManager.DebugMode)
            {
                AppendDebugLog("=== Debug 模式已启用 ===");
                AppendDebugLog($"游戏路径：{_configManager.GamePath}");
                AppendDebugLog($"Mods 路径：{_configManager.ModPath}");
                AppendDebugLog("");
            }
        }

        private void ToggleDebugMode()
        {
            _configManager.DebugMode = !_configManager.DebugMode;
            pnlDebug.Visible = _configManager.DebugMode;
            menuDebugMode.Checked = _configManager.DebugMode;

            if (_configManager.DebugMode)
            {
                AppendDebugLog("=== Debug 模式已启用 ===");
                AppendDebugLog($"时间：{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                AppendDebugLog($"游戏路径：{_configManager.GamePath}");
                AppendDebugLog($"Mods 路径：{_configManager.ModPath}");
                AppendDebugLog("");
            }
            else
            {
                AppendDebugLog("=== Debug 模式已禁用 ===");
            }
        }

        private void AppendDebugLog(string message)
        {
            if (!_configManager.DebugMode) return;

            txtDebugLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            txtDebugLog.ScrollToCaret();
        }

        private void StyleButton(Button button, Color baseColor)
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
            AppendDebugLog("--- 开始扫描 Mod ---");
            var scanResult = _modScanner.ScanModsDetailed();
            var mods = scanResult.Mods;

            AppendDebugLog($"扫描完成：发现 {mods.Count} 个 Mod，失败 {scanResult.FailedCount} 个");

            if (_configManager.DebugMode && scanResult.Errors.Count > 0)
            {
                AppendDebugLog("扫描错误/警告:");
                foreach (var error in scanResult.Errors)
                {
                    AppendDebugLog($"  - {error}");
                }
            }

            dgvMods.Rows.Clear();
            int rowIndex = 1;

            foreach (var mod in mods)
            {
                string updateTimeStr = mod.UpdateTime == DateTime.MinValue
                    ? "未知"
                    : mod.UpdateTime.ToString("yyyy-MM-dd HH:mm");

                string deprecatedStr = mod.IsDeprecated ? "是" : "否";

                bool isEnabled = _modStateManager.IsEnabled(mod.Name);
                string enabledStr = isEnabled ? "✅ 已启用" : "❌ 未启用";

                dgvMods.Rows.Add(
                    rowIndex++,
                    mod.Name,
                    mod.Author,
                    mod.Version,
                    updateTimeStr,
                    deprecatedStr,
                    enabledStr
                );

                dgvMods.Rows[dgvMods.Rows.Count - 1].Tag = mod;

                if (mod.IsDeprecated)
                {
                    dgvMods.Rows[dgvMods.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightCoral;
                }
                else if (isEnabled)
                {
                    dgvMods.Rows[dgvMods.Rows.Count - 1].DefaultCellStyle.BackColor = Color.LightGreen;
                }

                if (_configManager.DebugMode)
                {
                    AppendDebugLog($"[{mod.Name}] 类型={mod.Type}, 文件数={mod.TargetFiles.Count}, 弃用={mod.IsDeprecated}");
                    if (mod.ScanError != null)
                    {
                        AppendDebugLog($"  扫描错误：{mod.ScanError}");
                    }
                }
            }

            lblStatus.Text = $"发现 {mods.Count} 个 Mod";
            AppendDebugLog("--- Mod 列表加载完成 ---");

            // 如果有扫描错误，提示用户
            if (scanResult.Errors.Any())
            {
                string message = $"扫描完成，但发现 {scanResult.Errors.Count} 个错误/警告。\n\n" +
                                 $"成功加载: {scanResult.SuccessCount} 个\n" +
                                 $"失败/跳过: {scanResult.FailedCount} 个\n\n" +
                                 $"请打开 Debug 模式查看详细错误信息。";
                
                if (scanResult.FailedCount > 0)
                {
                    MessageBox.Show(message, "扫描完成有错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadModList();
        }

        private void btnEnable_Click(object sender, EventArgs e)
        {
            if (dgvMods.CurrentRow?.Tag is ModInfo modInfo)
            {
                if (modInfo.IsDeprecated)
                {
                    var confirmResult = MessageBox.Show(
                        $"⚠️ 警告：Mod '{modInfo.Name}' 已被标记为弃用或不支持的类型。\n\n" +
                        $"原因：{modInfo.DeprecatedReason}\n\n" +
                        $"继续启用可能会导致游戏不稳定或其他问题。\n\n" +
                        $"确定要继续启用吗？",
                        "弃用 Mod 警告",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult != DialogResult.Yes)
                    {
                        return;
                    }
                }

                try
                {
                    AppendDebugLog($"开始启用 Mod: {modInfo.Name}");

                    // 对于Comprehensive类型，需要让ModManager收集完整的安装文件列表
                    // 然后再进行备份，这样才能正确备份所有会被覆盖的文件
                    var installResult = _modManager.EnableMod(modInfo);

                    if (installResult.Success)
                    {
                        // Comprehensive类型已经在安装过程中收集了所有目标文件
                        var allTargetFiles = new List<string>();
                        foreach (var file in installResult.InstalledFiles)
                        {
                            allTargetFiles.Add(file);
                        }
                        foreach (var file in installResult.ReplacedFiles)
                        {
                            allTargetFiles.Add(file);
                        }

                        var backupResult = _backupManager.PrepareEnableModFromFiles(modInfo.Name, allTargetFiles);

                        if (!backupResult.Success)
                        {
                            Logger.Warning($"备份警告：{backupResult.Message}");
                            if (_configManager.DebugMode)
                            {
                                AppendDebugLog($"备份警告：{backupResult.Message}");
                            }
                        }
                        else if (_configManager.DebugMode)
                        {
                            AppendDebugLog($"备份完成");
                            AppendDebugLog($"备份文件数：{backupResult.BackedUpCount}");
                        }
                    }

                    if (installResult.Success)
                    {
                        _modStateManager.SetEnabled(modInfo.Name, true);
                        UpdateStatusLabel($"✅ 已启用 Mod: {modInfo.Name}");
                        Logger.Info($"已启用 Mod: {modInfo.Name}");

                        if (_configManager.DebugMode)
                        {
                            AppendDebugLog($"安装成功：安装了 {installResult.InstalledFiles.Count} 个文件，替换了 {installResult.ReplacedFiles.Count} 个文件");
                            foreach (var file in installResult.InstalledFiles)
                            {
                                AppendDebugLog($"  + 新增：{file}");
                            }
                            foreach (var file in installResult.ReplacedFiles)
                            {
                                AppendDebugLog($"  ~ 替换：{file}");
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
                        
                        UpdateStatusLabel($"❌ 启用失败：{statusMessage}");
                        Logger.Error($"启用 Mod 失败：{modInfo.Name} - {statusMessage}");

                        // 始终输出错误到debug日志，无论是否启用debug模式？不，debug面板只在debug模式显示
                        AppendDebugLog($"安装失败：{statusMessage}");
                        foreach (var error in installResult.Errors)
                        {
                            AppendDebugLog($"  错误：{error}");
                        }

                        // 无论是否弃用，都显示错误对话框给用户，包含所有错误详情
                        string errorTitle = modInfo.IsDeprecated ? "启用弃用 Mod 失败" : "启用 Mod 失败";
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
                                errorMessage += $"  ... 还有 {installResult.Errors.Count - 10} 个错误\n";
                            }
                            errorMessage += "\n";
                        }
                        errorMessage += "请检查 Debug 日志查看完整信息。";
                        
                        MessageBox.Show(errorMessage,
                            errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // 只更新当前行的启用状态，不重新加载整个列表，保留 Debug 日志
                    UpdateCurrentRowState();
                }
                catch (Exception ex)
                {
                    Logger.Error($"启用 Mod 失败：{modInfo.Name}", ex);
                    UpdateStatusLabel($"❌ 启用失败：{ex.Message}");

                    // 无论是否弃用，都显示错误对话框给用户，包含完整堆栈信息
                    string errorTitle = modInfo.IsDeprecated ? "启用弃用 Mod 失败" : "启用 Mod 失败";
                    string errorMessage = $"启用失败：{ex.Message}\n\n" +
                                         $"异常类型：{ex.GetType().Name}\n" +
                                         $"堆栈跟踪：\n{ex.StackTrace}\n\n" +
                                         $"详细信息已记录到日志。";
                    MessageBox.Show(errorMessage, 
                        errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDisable_Click(object sender, EventArgs e)
        {
            if (dgvMods.CurrentRow?.Tag is ModInfo modInfo)
            {
                if (modInfo.IsDeprecated)
                {
                    var confirmResult = MessageBox.Show(
                        $"⚠️ 警告：Mod '{modInfo.Name}' 已被标记为弃用。\n\n" +
                        $"原因：{modInfo.DeprecatedReason}\n\n" +
                        $"确定要禁用此 Mod 吗？",
                        "弃用 Mod 确认",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (confirmResult != DialogResult.Yes)
                    {
                        return;
                    }
                }

                try
                {
                    AppendDebugLog($"开始禁用 Mod: {modInfo.Name}");

                    var restoreResult = _backupManager.DisableMod(modInfo.Name, modInfo.Type);

                    if (restoreResult.Success)
                    {
                        _modStateManager.SetEnabled(modInfo.Name, false);
                        UpdateStatusLabel($"✅ 已禁用 Mod: {modInfo.Name}");
                        Logger.Info($"已禁用 Mod: {modInfo.Name}");

                        if (_configManager.DebugMode)
                        {
                            AppendDebugLog($"恢复成功：删除了 {restoreResult.DeletedCount} 个文件，恢复了 {restoreResult.RestoredCount} 个文件");
                        }
                    }
                    else
                    {
                        UpdateStatusLabel($"⚠️ 禁用完成但有错误：{restoreResult.Message}");
                        Logger.Warning($"禁用 Mod 出现问题：{modInfo.Name} - {restoreResult.Message}");

                        if (_configManager.DebugMode)
                        {
                            AppendDebugLog($"恢复出现问题：{restoreResult.Message}");
                        }
                    }

                    // 只更新当前行的启用状态，不重新加载整个列表，保留 Debug 日志
                    UpdateCurrentRowState();
                }
                catch (Exception ex)
                {
                    Logger.Error($"禁用 Mod 失败：{modInfo.Name}", ex);
                    UpdateStatusLabel($"❌ 禁用失败：{ex.Message}");

                    // 无论是否弃用，都显示错误对话框给用户，包含完整堆栈信息
                    string errorTitle = modInfo.IsDeprecated ? "禁用弃用 Mod 失败" : "禁用 Mod 失败";
                    string errorMessage = $"禁用失败：{ex.Message}\n\n" +
                                         $"异常类型：{ex.GetType().Name}\n" +
                                         $"堆栈跟踪：\n{ex.StackTrace}\n\n" +
                                         $"详细信息已记录到日志。";
                    MessageBox.Show(errorMessage,
                        errorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateStatusLabel(string message)
        {
            lblStatus.Text = message;
            lblActionHint.Text = "✅ 操作完成";

            var timer = new System.Windows.Forms.Timer { Interval = 3000 };
            timer.Tick += (s, e) =>
            {
                lblStatus.Text = "状态：就绪";
                lblActionHint.Text = "💡 选择一个 Mod 后点击启用或禁用";
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "这将恢复所有被修改的游戏文件到原始状态。\n确定要继续吗？",
                "确认恢复",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    _backupManager.RestoreAllBackups();
                    MessageBox.Show("游戏文件已恢复到原始状态", "成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"恢复失败：{ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void menuSettings_Click(object sender, EventArgs e)
        {
            using (var settingsForm = new SettingsForm(_configManager))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    InitializeManagers();
                    LoadModList();
                }
            }
        }

        private void menuDebugMode_Click(object sender, EventArgs e)
        {
            ToggleDebugMode();
        }

        #region Windows Form Designer

        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dgvMods;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnEnable;
        private System.Windows.Forms.Button btnDisable;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.GroupBox grpMods;
        private System.Windows.Forms.GroupBox grpActions;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.lblActionHint = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pnlDebug = new System.Windows.Forms.Panel();
            this.txtDebugLog = new System.Windows.Forms.TextBox();
            this.lblDebugTitle = new System.Windows.Forms.Label();
            this.grpMods.SuspendLayout();
            this.grpActions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMods)).BeginInit();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuTools});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(784, 25);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "菜单栏";
            // 
            // menuTools
            // 
            this.menuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.menuSettings,
                this.menuDebugMode});
            this.menuTools.Name = "menuTools";
            this.menuTools.Size = new System.Drawing.Size(50, 21);
            this.menuTools.Text = "工具 (&T)";
            // 
            // menuSettings
            // 
            this.menuSettings.Name = "menuSettings";
            this.menuSettings.Size = new System.Drawing.Size(120, 22);
            this.menuSettings.Text = "设置 (&S)";
            this.menuSettings.Click += new System.EventHandler(this.menuSettings_Click);
            // 
            // menuDebugMode
            // 
            this.menuDebugMode.Name = "menuDebugMode";
            this.menuDebugMode.Size = new System.Drawing.Size(120, 22);
            this.menuDebugMode.Text = "Debug 模式 (&D)";
            this.menuDebugMode.CheckOnClick = true;
            this.menuDebugMode.Click += new System.EventHandler(this.menuDebugMode_Click);
            // 
            // grpMods
            // 
            this.grpMods.Controls.Add(this.dgvMods);
            this.grpMods.Location = new System.Drawing.Point(12, 35);
            this.grpMods.Name = "grpMods";
            this.grpMods.Size = new System.Drawing.Size(856, 420);
            this.grpMods.TabIndex = 1;
            this.grpMods.TabStop = false;
            this.grpMods.Text = "Mod 列表";
            this.grpMods.BackColor = Color.Transparent;
            this.grpMods.ForeColor = Color.FromArgb(80, 80, 80);
            this.grpMods.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            // 
            // dgvMods
            // 
            this.dgvMods.AllowUserToAddRows = false;
            this.dgvMods.AllowUserToDeleteRows = false;
            this.dgvMods.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMods.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMods.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
                CreateNumberColumn("序号", 50),
                CreateColumn("名称", 150),
                CreateColumn("作者", 100),
                CreateColumn("版本", 70),
                CreateColumn("更新时间", 130),
                CreateColumn("是否弃用", 80),
                CreateColumn("启用状态", 90)
            });
            this.dgvMods.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMods.Location = new System.Drawing.Point(3, 22);
            this.dgvMods.MultiSelect = false;
            this.dgvMods.Name = "dgvMods";
            this.dgvMods.ReadOnly = true;
            this.dgvMods.RowHeadersVisible = false;
            this.dgvMods.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvMods.Size = new System.Drawing.Size(850, 395);
            this.dgvMods.TabIndex = 0;
            this.dgvMods.DefaultCellStyle.SelectionBackColor = GridHeaderSelectionColor;
            this.dgvMods.DefaultCellStyle.SelectionForeColor = Color.White;
            this.dgvMods.ColumnHeadersDefaultCellStyle.SelectionBackColor = GridHeaderSelectionColor;
            this.dgvMods.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
            // 
            // grpActions
            // 
            this.grpActions.Controls.Add(this.btnRestore);
            this.grpActions.Controls.Add(this.btnDisable);
            this.grpActions.Controls.Add(this.btnEnable);
            this.grpActions.Controls.Add(this.btnRefresh);
            this.grpActions.Controls.Add(this.lblActionHint);
            this.grpActions.Location = new System.Drawing.Point(12, 461);
            this.grpActions.Name = "grpActions";
            this.grpActions.Size = new System.Drawing.Size(856, 100);
            this.grpActions.TabIndex = 2;
            this.grpActions.TabStop = false;
            this.grpActions.Text = "操作";
            this.grpActions.BackColor = Color.Transparent;
            this.grpActions.ForeColor = Color.FromArgb(80, 80, 80);
            this.grpActions.Font = new Font("Microsoft YaHei UI", 10F, FontStyle.Bold);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(10, 25);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(100, 35);
            this.btnRefresh.TabIndex = 0;
            this.btnRefresh.Text = "🔄 刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnEnable
            // 
            this.btnEnable.Location = new System.Drawing.Point(120, 25);
            this.btnEnable.Name = "btnEnable";
            this.btnEnable.Size = new System.Drawing.Size(100, 35);
            this.btnEnable.TabIndex = 1;
            this.btnEnable.Text = "✅ 启用";
            this.btnEnable.UseVisualStyleBackColor = true;
            this.btnEnable.Click += new System.EventHandler(this.btnEnable_Click);
            // 
            // btnDisable
            // 
            this.btnDisable.Location = new System.Drawing.Point(230, 25);
            this.btnDisable.Name = "btnDisable";
            this.btnDisable.Size = new System.Drawing.Size(100, 35);
            this.btnDisable.TabIndex = 2;
            this.btnDisable.Text = "⏸️ 禁用";
            this.btnDisable.UseVisualStyleBackColor = true;
            this.btnDisable.Click += new System.EventHandler(this.btnDisable_Click);
            // 
            // btnRestore
            // 
            this.btnRestore.Location = new System.Drawing.Point(740, 25);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(110, 35);
            this.btnRestore.TabIndex = 3;
            this.btnRestore.Text = "♻️ 恢复纯净";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // lblActionHint
            // 
            this.lblActionHint.AutoSize = true;
            this.lblActionHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblActionHint.ForeColor = Color.FromArgb(100, 100, 100);
            this.lblActionHint.Location = new System.Drawing.Point(10, 70);
            this.lblActionHint.Name = "lblActionHint";
            this.lblActionHint.Size = new System.Drawing.Size(200, 17);
            this.lblActionHint.Text = "💡 选择一个 Mod 后点击启用或禁用";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            this.lblStatus.Location = new System.Drawing.Point(12, 575);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(80, 17);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "状态：就绪";
            // 
            // pnlDebug
            // 
            this.pnlDebug.Location = new System.Drawing.Point(12, 595);
            this.pnlDebug.Name = "pnlDebug";
            this.pnlDebug.Size = new System.Drawing.Size(856, 150);
            this.pnlDebug.TabIndex = 4;
            this.pnlDebug.BackColor = Color.FromArgb(30, 30, 30);
            this.pnlDebug.BorderStyle = BorderStyle.FixedSingle;
            this.pnlDebug.Controls.Add(this.txtDebugLog);
            this.pnlDebug.Controls.Add(this.lblDebugTitle);
            // 
            // lblDebugTitle
            // 
            this.lblDebugTitle.AutoSize = true;
            this.lblDebugTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            this.lblDebugTitle.ForeColor = Color.FromArgb(0, 255, 100);
            this.lblDebugTitle.Location = new System.Drawing.Point(10, 8);
            this.lblDebugTitle.Name = "lblDebugTitle";
            this.lblDebugTitle.Size = new System.Drawing.Size(100, 17);
            this.lblDebugTitle.Text = "🔧 Debug 日志";
            // 
            // txtDebugLog
            // 
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
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = BackgroundColor;
            this.ClientSize = new System.Drawing.Size(884, 750);
            this.Controls.Add(this.pnlDebug);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.grpActions);
            this.Controls.Add(this.grpMods);
            this.Controls.Add(this.menuStrip);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "✨ APmodManager";
            this.grpMods.ResumeLayout(false);
            this.grpActions.ResumeLayout(false);
            this.grpActions.PerformLayout();
            this.pnlDebug.ResumeLayout(false);
            this.pnlDebug.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMods)).EndInit();
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

        private System.Windows.Forms.DataGridViewColumn CreateColumn(string headerText, int width)
        {
            var column = new System.Windows.Forms.DataGridViewTextBoxColumn
            {
                HeaderText = headerText,
                Width = width,
                FillWeight = width,
                SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable
            };
            return column;
        }

        private System.Windows.Forms.DataGridViewColumn CreateNumberColumn(string headerText, int width)
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
                    BackColor = Color.FromArgb(240, 240, 240)
                }
            };
            return column;
        }

        /// <summary>
        /// 更新当前选中行的启用状态，不重新加载整个列表
        /// </summary>
        private void UpdateCurrentRowState()
        {
            if (dgvMods.CurrentRow == null) return;

            var modInfo = dgvMods.CurrentRow.Tag as ModInfo;
            if (modInfo == null) return;

            bool isEnabled = _modStateManager.IsEnabled(modInfo.Name);
            string enabledStr = isEnabled ? "✅ 已启用" : "❌ 未启用";

            // 启用状态在第 6 列（索引从 0 开始）
            dgvMods.CurrentRow.Cells[6].Value = enabledStr;

            // 更新背景色
            if (modInfo.IsDeprecated)
            {
                dgvMods.CurrentRow.DefaultCellStyle.BackColor = Color.LightCoral;
            }
            else if (isEnabled)
            {
                dgvMods.CurrentRow.DefaultCellStyle.BackColor = Color.LightGreen;
            }
            else
            {
                dgvMods.CurrentRow.DefaultCellStyle.BackColor = dgvMods.DefaultCellStyle.BackColor;
            }
        }

        #endregion
    }
}

