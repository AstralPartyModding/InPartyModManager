# 安全策略

## 报告安全问题

如果你发现安全漏洞，请通过以下方式报告：

1. **不要** 公开披露安全问题
2. 通过 [GitHub Security Advisories](https://github.com/YOUR_USERNAME/AstralPartyModManager/security/advisories) 提交

## 安全设计

- 不收集用户数据
- 不联网（所有操作本地执行）
- 不修改游戏核心文件（仅替换 Addressables 资源）
- 自动备份原始文件
- 用户明确同意后才执行文件操作

## 已知限制

- 需要游戏目录写入权限
- 需要创建备份文件（占用磁盘空间）
