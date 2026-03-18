# 星引擎 (Astral Party) Mod 管理器

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0+-purple.svg)](https://dotnet.microsoft.com/download/dotnet)

为 Unity 引擎游戏《星引擎》(Astral Party) 开发的开源 Mod 管理工具。

---

## 快速开始

### 下载
从 [Releases](https://github.com/YOUR_USERNAME/AstralPartyModManager/releases) 下载最新版本

### 安装
1. 解压到游戏根目录的 `AstralPartyModManager` 文件夹
2. 运行 `AstralPartyModManager.exe`
3. 点击 **刷新** 扫描 Mod
4. 选择 Mod 后点击 **启用/禁用**

---

## 功能特性

- **自动扫描** - 识别 mods 文件夹中的可用 Mod
- **类型识别** - 支持 Addressables/语音/插件 等类型
- **备份管理** - 自动备份原始游戏文件
- **一键切换** - 快速启用/禁用 Mod
- **冲突检测** - 检测多 Mod 修改同一文件

---

## 项目结构

```
AstralPartyModManager/
├── src/                    # 源代码
├── data/backups/           # 备份文件
├── profiles/               # 配置档案
├── config.json             # 配置文件
├── LICENSE                 # MIT 许可证
└── README.md               # 本文件

mods/                       # Mod 文件夹（游戏目录）
├── 汉娜/                   # 原始 Mod
├── 汉娜语音替换凛 [标准格式]/ # 标准格式 Mod
└── 星引擎加速插件 [标准格式]/
```

---

## 开发指南

### 环境要求
- .NET 8.0 SDK+
- Visual Studio 2022 或 VS Code

### 构建
```bash
cd src
dotnet publish -c Release -r win-x64 --self-contained true -o ../publish
```

### 运行
```bash
dotnet run --project src
```

---

## Mod 制作

### 基础结构
```
[Mod 名称]/
├── mod.json              # Mod 描述文件
└── AstralParty/          # 游戏文件替换目录
    └── StreamingAssets/
        └── aa/
            └── StandaloneWindows64/
                └── *.bundle
```

### mod.json 示例
```json
{
  "name": "我的 Mod",
  "author": "作者名",
  "version": "1.0.0",
  "type": "plugin",
  "targets": [{"path": "", "files": []}]
}
```

完整文档请查看 `docs/` 目录。

---

## 参与贡献

- 报告问题：[Issues](https://github.com/YOUR_USERNAME/AstralPartyModManager/issues)
- 功能建议：[Discussions](https://github.com/YOUR_USERNAME/AstralPartyModManager/discussions)
- 提交代码：[Pull Requests](https://github.com/YOUR_USERNAME/AstralPartyModManager/pulls)

详见 [CONTRIBUTING.md](CONTRIBUTING.md)

---

## 许可证

MIT License - 详见 [LICENSE](LICENSE)

---

## 免责声明

- 本工具仅供学习研究使用
- 使用 Mod 可能导致游戏崩溃，请自行权衡风险
- 操作前请备份游戏文件
- 本工具与游戏官方无关

---

如果这个项目对你有帮助，请给一个 Star 支持！
