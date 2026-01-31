# GitHub Actions 自动编译指南

## 🚀 快速开始

### 方式 1：推送到远程仓库触发编译（推荐）

1. **检查远程仓库配置**
   ```bash
   git remote -v
   ```

2. **如果还没有远程仓库，先创建一个**
   - 访问 https://github.com/new
   - 创建新仓库（可以命名为 `Jvedio-AI` 或 `jvedio-fork`）
   - 不要初始化 README

3. **关联远程仓库**
   ```bash
   # 使用你的仓库地址
   git remote set-url origin https://github.com/你的用户名/你的仓库名.git
   ```

4. **推送代码触发编译**
   ```bash
   # 添加所有文件
   git add .

   # 提交
   git commit -m "feat: 添加千问 AI 功能和自动编译配置"

   # 推送到 GitHub
   git push origin master
   ```

5. **等待编译完成（约 5-10 分钟）**
   - 访问：https://github.com/你的用户名/你的仓库名/actions
   - 查看 "Build Jvedio with AI Features" 工作流
   - 等待显示 ✅ 绿色勾

6. **下载编译好的版本**
   - 在 Actions 页面点击最新的构建
   - 滚动到 "Artifacts" 部分
   - 下载 `Jvedio-Windows`
   - 或者下载 `Jvedio-dev.zip`

---

### 方式 2：手动触发编译

如果你不想推送代码，可以手动触发：

1. 访问你的仓库
2. 点击 "Actions" 标签
3. 选择 "Build Jvedio with AI Features"
4. 点击 "Run workflow" 按钮
5. 选择分支，点击绿色的 "Run workflow"

---

### 方式 3：发布正式版本

如果你想发布一个正式版本：

1. **创建标签**
   ```bash
   git tag v5.4.1-ai
   git push origin v5.4.1-ai
   ```

2. **自动创建 Release**
   - 推送标签后，GitHub Actions 会自动：
     - 编译项目
     - 创建 Release
     - 上传 `Jvedio-v5.4.1-ai.zip`

3. **访问 Release 页面**
   - 访问：https://github.com/你的用户名/你的仓库名/releases
   - 下载你的版本

---

## 📦 编译产物说明

每次编译会生成：

### 1. Artifact (持续 30 天)
- **名称**：`Jvedio-Windows`
- **包含**：
  - `Jvedio-v5.4.1-ai.zip`（压缩包）
  - `Release/`（完整文件夹）

### 2. Release (仅标签触发)
- **自动创建**：仅当你推送标签时
- **包含**：`Jvedio-vx.x.x.zip`
- **下载地址**：Releases 页面

---

## ⚙️ GitHub Actions 配置说明

编译配置文件位于：`.github/workflows/build.yml`

### 触发条件

```yaml
on:
  push:
    branches: [ master, main ]    # 推送到主分支
    tags:
      - 'v*'                      # 推送标签（如 v5.4.1）
  pull_request:                   # Pull Request
  workflow_dispatch:              # 手动触发
```

### 编译环境

- **Runner**：`windows-latest`
- **框架**：.NET Framework 4.7.2
- **工具**：MSBuild + NuGet

### 编译步骤

1. ✅ 检出代码
2. ✅ 设置 MSBuild
3. ✅ 缓存 NuGet 包（加速）
4. ✅ 恢复 NuGet 依赖
5. ✅ 清理旧版本
6. ✅ 编译 Release 版本
7. ✅ 准备发布包
8. ✅ 创建压缩包
9. ✅ 上传 Artifact
10. ✅ 创建 Release（如果打标签）

---

## 🎯 使用编译好的软件

### 1. 下载

从 GitHub Actions 下载：
```
Artifacts → Jvedio-Windows → Jvedio-dev.zip
```

或从 Release 下载：
```
Releases → v5.4.1-ai → Jvedio-v5.4.1-ai.zip
```

### 2. 解压

解压到任意目录：
```
D:\Apps\Jvedio\
```

### 3. 配置 AI 功能

编辑 `.env` 文件：
```env
# 千问API配置
DASHSCOPE_API_KEY = "sk-你的API密钥"
DASHSCOPE_API_BASE = "https://dashscope.aliyuncs.com/compatible-mode/v1"
```

### 4. 运行

双击 `Jvedio.exe` 启动软件。

---

## 🔧 故障排除

### 问题 1：编译失败

**症状**：Actions 页面显示 ❌ 红叉

**解决**：
1. 点击失败的构建查看日志
2. 检查错误信息（通常是 NuGet 恢复失败或编译错误）
3. 常见问题：
   - `packages.config` 文件路径错误
   - NuGet 包无法下载
   - 代码有编译错误

### 问题 2：Artifact 无法下载

**症状**：Artifacts 区域为空

**解决**：
1. 检查工作流是否完成（必须显示 ✅）
2. 等待 2-3 分钟后刷新页面
3. 确认有上传步骤成功

### 问题 3：无法创建 Release

**症状**：没有自动创建 Release

**原因**：只有推送标签才会创建 Release

**解决**：
```bash
git tag v5.4.1
git push origin v5.4.1
```

### 问题 4：编译后缺少文件

**症状**：运行时提示缺少 DLL

**解决**：
- 检查 `build.yml` 中的复制路径
- 确保所有 Reference 文件都被正确复制

---

## 📊 对比：官方 vs 你的版本

| 特性 | 官方版本 | 你的版本（AI） |
|------|---------|---------------|
| **基础功能** | ✅ 视频管理 | ✅ 视频管理 |
| **AI 性别识别** | ❌ 无 | ✅ 千问 AI |
| **AI 信息补全** | ❌ 无 | ✅ 千问 AI |
| **DashScope API** | ❌ 无 | ✅ 集成 |
| **编译方式** | 官方手动编译 | GitHub Actions 自动编译 |
| **更新频率** | 不定期 | 随时更新 |

---

## 🎉 下一步

1. **推送到你的 GitHub 仓库**
   ```bash
   git remote set-url origin https://github.com/你的用户名/jvedio-ai.git
   git push origin master
   ```

2. **等待编译完成**

3. **下载并测试**

4. **享受 AI 功能！**

---

## 💡 高级用法

### 修改编译配置

编辑 `.github/workflows/build.yml`，可以修改：
- 编译配置（Debug/Release）
- NuGet 包缓存策略
- Release 描述模板

### 多平台编译（如果将来迁移到 .NET Core）

可以添加 Linux 和 macOS 构建任务：
```yaml
jobs:
  build-windows:
    runs-on: windows-latest
    # ...

  build-linux:
    runs-on: ubuntu-latest
    # ...
```

### 定时编译

添加定时触发：
```yaml
on:
  schedule:
    - cron: '0 2 * * *'  # 每天凌晨 2 点
```

---

## 📞 技术支持

- GitHub Actions 文档：https://docs.github.com/actions
- MSBuild 文档：https://docs.microsoft.com/visualstudio/msbuild

---

**准备好了吗？现在就推送代码，让 GitHub 自动为你编译吧！** 🚀
