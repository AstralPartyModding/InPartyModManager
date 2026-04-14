// AstralParty Mod Manager - 设置窗口
// Copyright (c) AstralParty Modding Community. All rights reserved.

namespace AstralPartyModManager
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    // 设置对话框
    public partial class SettingsForm : Form
    {
        private ConfigManager configManager;
        private TextBox txtGamePath;
        private TextBox txtModPath;
        private Button btnBrowseGame;
        private Button btnBrowseMod;
        private Button btnSave;
        private Button btnCancel;
        private Button btnVerify;
        private Label lblStatus;
        private Label lblGamePathHint;
        private Label lblModPathHint;
        private GroupBox grpPaths;
        private CheckBox chkBackup;
        private CheckBox chkConflict;

        public SettingsForm(ConfigManager configManager)
        {
            this.configManager = configManager;
            this.InitializeComponent();
            this.LoadConfig();
        }

        private void LoadConfig()
        {
            this.txtGamePath.Text = this.configManager.GamePath;
            this.txtModPath.Text = this.configManager.ModPath;
            this.chkBackup.Checked = this.configManager.BackupEnabled;
            this.chkConflict.Checked = this.configManager.AutoDetectConflicts;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            this.configManager.GamePath = this.txtGamePath.Text;
            this.configManager.ModPath = this.txtModPath.Text;
            this.configManager.BackupEnabled = this.chkBackup.Checked;
            this.configManager.AutoDetectConflicts = this.chkConflict.Checked;

            if (this.configManager.ValidatePaths(out string errorMessage))
            {
                MessageBox.Show(
                    "设置已保存！",
                    "成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                var result = MessageBox.Show(
                    $"路径验证失败：{errorMessage}\n\n是否仍然保存？",
                    "警告",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.OK;
                }
            }
        }

        private void BtnBrowseGame_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择游戏根目录";
                dialog.SelectedPath = this.txtGamePath.Text;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.txtGamePath.Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnBrowseMod_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择 Mods 文件夹";
                dialog.SelectedPath = this.txtModPath.Text;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.txtModPath.Text = dialog.SelectedPath;
                }
            }
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            this.configManager.GamePath = this.txtGamePath.Text;
            this.configManager.ModPath = this.txtModPath.Text;

            if (this.configManager.ValidatePaths(out string errorMessage))
            {
                MessageBox.Show(
                    "路径验证通过！",
                    "成功",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                MessageBox.Show(
                    $"路径验证失败：{errorMessage}",
                    "错误",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private System.ComponentModel.IContainer components = null;

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
            this.grpPaths = new System.Windows.Forms.GroupBox();
            this.txtGamePath = new System.Windows.Forms.TextBox();
            this.txtModPath = new System.Windows.Forms.TextBox();
            this.btnBrowseGame = new System.Windows.Forms.Button();
            this.btnBrowseMod = new System.Windows.Forms.Button();
            this.lblGamePathHint = new System.Windows.Forms.Label();
            this.lblModPathHint = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnVerify = new System.Windows.Forms.Button();
            this.lblStatus = new System.Windows.Forms.Label();
            this.chkBackup = new System.Windows.Forms.CheckBox();
            this.chkConflict = new System.Windows.Forms.CheckBox();
            this.grpPaths.SuspendLayout();
            this.SuspendLayout();

            // grpPaths
            this.grpPaths.Controls.Add(this.lblModPathHint);
            this.grpPaths.Controls.Add(this.lblGamePathHint);
            this.grpPaths.Controls.Add(this.btnBrowseMod);
            this.grpPaths.Controls.Add(this.btnBrowseGame);
            this.grpPaths.Controls.Add(this.txtModPath);
            this.grpPaths.Controls.Add(this.txtGamePath);
            this.grpPaths.Location = new System.Drawing.Point(12, 12);
            this.grpPaths.Name = "grpPaths";
            this.grpPaths.Size = new System.Drawing.Size(560, 130);
            this.grpPaths.Text = "路径设置";

            // txtGamePath
            this.txtGamePath.Location = new System.Drawing.Point(6, 25);
            this.txtGamePath.Name = "txtGamePath";
            this.txtGamePath.Size = new System.Drawing.Size(470, 23);

            // txtModPath
            this.txtModPath.Location = new System.Drawing.Point(6, 80);
            this.txtModPath.Name = "txtModPath";
            this.txtModPath.Size = new System.Drawing.Size(470, 23);

            // btnBrowseGame
            this.btnBrowseGame.Location = new System.Drawing.Point(482, 24);
            this.btnBrowseGame.Name = "btnBrowseGame";
            this.btnBrowseGame.Size = new System.Drawing.Size(72, 25);
            this.btnBrowseGame.Text = "浏览...";
            this.btnBrowseGame.Click += new System.EventHandler(this.BtnBrowseGame_Click);

            // btnBrowseMod
            this.btnBrowseMod.Location = new System.Drawing.Point(482, 79);
            this.btnBrowseMod.Name = "btnBrowseMod";
            this.btnBrowseMod.Size = new System.Drawing.Size(72, 25);
            this.btnBrowseMod.Text = "浏览...";
            this.btnBrowseMod.Click += new System.EventHandler(this.BtnBrowseMod_Click);

            // lblGamePathHint
            this.lblGamePathHint.AutoSize = true;
            this.lblGamePathHint.ForeColor = System.Drawing.Color.Gray;
            this.lblGamePathHint.Location = new System.Drawing.Point(6, 51);
            this.lblGamePathHint.Name = "lblGamePathHint";
            this.lblGamePathHint.Size = new System.Drawing.Size(250, 17);
            this.lblGamePathHint.Text = "📁 选择 8vJXn6CN 文件夹（包含 AstralParty_CN.exe）";

            // lblModPathHint
            this.lblModPathHint.AutoSize = true;
            this.lblModPathHint.ForeColor = System.Drawing.Color.Gray;
            this.lblModPathHint.Location = new System.Drawing.Point(6, 106);
            this.lblModPathHint.Name = "lblModPathHint";
            this.lblModPathHint.Size = new System.Drawing.Size(200, 17);
            this.lblModPathHint.Text = "📦 选择 mods 文件夹（包含 Mod 子文件夹）";

            // chkBackup
            this.chkBackup.AutoSize = true;
            this.chkBackup.Checked = true;
            this.chkBackup.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBackup.Location = new System.Drawing.Point(12, 155);
            this.chkBackup.Name = "chkBackup";
            this.chkBackup.Size = new System.Drawing.Size(150, 21);
            this.chkBackup.Text = "启用自动备份";

            // chkConflict
            this.chkConflict.AutoSize = true;
            this.chkConflict.Checked = true;
            this.chkConflict.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkConflict.Location = new System.Drawing.Point(12, 182);
            this.chkConflict.Name = "chkConflict";
            this.chkConflict.Size = new System.Drawing.Size(180, 21);
            this.chkConflict.Text = "自动检测 Mod 冲突";

            // btnVerify
            this.btnVerify.Location = new System.Drawing.Point(12, 215);
            this.btnVerify.Name = "btnVerify";
            this.btnVerify.Size = new System.Drawing.Size(100, 30);
            this.btnVerify.Text = "验证路径";
            this.btnVerify.Click += new System.EventHandler(this.BtnVerify_Click);

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(330, 215);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(100, 30);
            this.btnSave.Text = "保存";
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);

            // btnCancel
            this.btnCancel.Location = new System.Drawing.Point(440, 215);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 30);
            this.btnCancel.Text = "取消";
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);

            // lblStatus
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(12, 255);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 17);

            // SettingsForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 280);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnVerify);
            this.Controls.Add(this.chkConflict);
            this.Controls.Add(this.chkBackup);
            this.Controls.Add(this.grpPaths);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "设置";
            this.grpPaths.ResumeLayout(false);
            this.grpPaths.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
