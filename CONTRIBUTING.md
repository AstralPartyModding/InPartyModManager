# 贡献指南

感谢你对本项目的关注！欢迎以任何方式参与贡献。

## 如何贡献

### 1. 报告问题 (Bug Report)

在提交 Issue 前，请确认：
- 已搜索现有 Issue，确认不是重复问题
- 使用的是最新版本
- 问题可以稳定复现

**提交 Issue 时请包含：**
- 问题描述（清晰简洁）
- 复现步骤
- 预期行为
- 实际行为
- 截图（如适用）
- 环境信息（操作系统、.NET 版本等）

### 2. 功能请求 (Feature Request)

**提交功能请求时请包含：**
- 功能描述
- 使用场景
- 实现建议（可选）

### 3. 提交代码 (Pull Request)

```bash
# 1. Fork 本仓库
# 2. 克隆到本地
git clone https://github.com/YOUR_USERNAME/AstralPartyModManager.git

# 3. 创建功能分支
git checkout -b feature/your-feature-name

# 4. 编译项目
cd src
dotnet build

# 5. 运行程序
dotnet run
```

#### PR 指南
- 代码遵循 C# 命名规范
- 提交信息清晰简洁
- 如有必要，请更新文档

## 开发环境

- .NET 8.0 SDK+
- Visual Studio 2022 或 VS Code

## 项目结构

```
src/
├── MainForm.cs           # 主窗口
├── ModScanner.cs         # Mod 扫描器
├── ModManager.cs         # Mod 管理
├── BackupManager.cs      # 备份管理
└── ConfigManager.cs      # 配置管理
```

## 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE)
