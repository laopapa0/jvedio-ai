# 零基础 Git + GitHub 教程

## 📖 这里的内容

- 什么是 Git 和 GitHub
- 如何创建 GitHub 账号
- 如何上传代码
- 如何下载编译好的软件

---

## 第 1 部分：理解基本概念（5 分钟）

### Git 是什么？

**Git** 是一个**版本控制工具**，就像：
- 📝 Word 文档的"撤销/恢复"功能
- 📦 文件的备份系统
- 🔄 记录每次修改的历史

**简单理解**：Git 帮你管理代码的版本，你可以随时回到之前的版本。

### GitHub 是什么？

**GitHub** 是一个**代码托管网站**，就像：
- ☁️ Google Drive 或 Dropbox 的代码版
- 🤝 代码的社交平台
- 🚀 自动编译工具（我们今天要用的）

**简单理解**：GitHub 存放你的代码，并帮你自动编译。

---

## 第 2 部分：创建 GitHub 账号（3 分钟）

### 步骤 1：注册账号

1. 访问：https://github.com/signup
2. 填写信息：
   - 用户名（建议：`jvedio-ai-你的名字`）
   - 邮箱
   - 密码
3. 验证邮箱（GitHub 会发验证邮件）

### 步骤 2：登录

1. 访问：https://github.com/login
2. 输入邮箱和密码登录

---

## 第 3 部分：创建仓库（2 分钟）

### 什么是仓库？

仓库就像一个**项目文件夹**，存放你的代码。

### 创建步骤

1. 登录 GitHub 后，点击右上角 **+** 号
2. 点击 **New repository**
3. 填写信息：
   - **Repository name**: `jvedio-ai`（或者你喜欢的名字）
   - ❌ **不要勾选** "Add a README file"
   - ❌ **不要勾选** "Add .gitignore"
   - ❌ **不要勾选** "Choose a license"
4. 点击绿色按钮 **Create repository**

### 创建后的页面

你会看到一个页面，显示你的仓库地址：
```
https://github.com/你的用户名/jvedio-ai.git
```

**复制这个地址！**（后面要用）

---

## 第 4 部分：上传代码到 GitHub

你有两种方式：

### 方式 A：使用命令行（适合所有操作系统）

这种方式已经在当前环境中配置好了，直接用即可！

#### 步骤 1：查看当前 Git 状态

```bash
cd /workspace/projects/Jvedio
git status
```

你会看到很多文件状态（不要怕，这是正常的）

#### 步骤 2：添加所有文件到暂存区

```bash
git add .
```

**说明**：这就像把文件放进"待上传"的盒子

#### 步骤 3：提交更改（创建快照）

```bash
git commit -m "feat: 添加千问 AI 功能"
```

**说明**：
- `commit` = 提交/确认
- `-m "..."` = 提交信息（描述你做了什么）

#### 步骤 4：连接到你的 GitHub 仓库

```bash
# 替换成你的仓库地址（第 3 部分复制的那个）
git remote set-url origin https://github.com/你的用户名/jvedio-ai.git
```

**示例**（假设你的用户名是 `xiaoming`）：
```bash
git remote set-url origin https://github.com/xiaoming/jvedio-ai.git
```

#### 步骤 5：推送到 GitHub

```bash
git push origin master
```

**第一次推送会提示输入用户名和密码**：
- **Username**: 你的 GitHub 用户名
- **Password**: 你的 **Personal Access Token**（见下方说明）

⚠️ **重要：GitHub 不再支持密码登录，需要使用 Personal Access Token**

##### 如何获取 Personal Access Token？

1. 登录 GitHub
2. 点击右上角头像 → **Settings**
3. 左侧菜单最下方 → **Developer settings**
4. 点击 **Personal access tokens** → **Tokens (classic)**
5. 点击 **Generate new token (classic)**
6. 填写信息：
   - **Note**: `Jvedio AI Upload`
   - **Expiration**: 选择 90 days 或 No expiration
   - **勾选权限**：勾选 `repo`（这个就够了）
7. 点击绿色按钮 **Generate token**
8. **复制生成的 token**（格式：`ghp_xxxxxxxxxxxxxxxx`）

⚠️ **注意**：Token 只显示一次，一定要复制保存！

##### 推送时输入

```bash
git push origin master
Username: xiaoming
Password: ghp_xxxxxxxxxxxxxxxxxxxxxx  # 粘贴你的 token
```

✅ **推送成功！**

---

### 方式 B：使用 GitHub Desktop（推荐新手）

如果觉得命令行太复杂，可以使用图形界面工具。

#### 步骤 1：安装 GitHub Desktop

下载地址：
- Windows: https://desktop.github.com/
- Mac: https://desktop.github.com/

#### 步骤 2：登录

安装后打开 GitHub Desktop，用你的 GitHub 账号登录。

#### 步骤 3：连接本地仓库

1. 点击 **File** → **Add Local Repository**
2. 选择你的 Jvedio 文件夹路径
3. GitHub Desktop 会显示所有更改

#### 步骤 4：推送

1. 在左下角的 **Summary** 框中输入："feat: 添加千问 AI 功能"
2. 点击 **Commit to master**
3. 点击 **Publish branch**
4. 选择你的 GitHub 仓库

✅ **推送成功！**

---

## 第 5 部分：下载编译好的软件（5 分钟）

推送成功后，等待 **5-10 分钟**，然后：

### 步骤 1：访问 Actions 页面

1. 访问你的仓库：https://github.com/你的用户名/jvedio-ai
2. 点击顶部的 **Actions** 标签

### 步骤 2：查看编译状态

你会看到：
- 🟡 黄色圆圈 = 编译中
- ✅ 绿色勾 = 编译成功
- ❌ 红叉 = 编译失败

等待它变成 **绿色勾**（约 5-10 分钟）

### 步骤 3：下载编译好的版本

1. 点击最新的构建（最上面那个）
2. 向下滚动找到 **Artifacts** 部分
3. 点击 **Jvedio-Windows** 下载

### 步骤 4：安装使用

1. 解压下载的文件
2. 打开文件夹
3. 编辑 `.env` 文件（用记事本）：
   ```env
   DASHSCOPE_API_KEY = "sk-你的千问API密钥"
   ```
4. 双击 `Jvedio.exe` 启动

---

## 第 6 部分：常见问题解决

### 问题 1：git 命令不存在

**症状**：
```
bash: git: command not found
```

**解决**：在当前环境中不需要安装，git 已经配置好了。直接用命令即可。

### 问题 2：推送失败 - Authentication failed

**症状**：
```
remote: Invalid username or password.
fatal: Authentication failed
```

**原因**：GitHub 不再支持密码登录

**解决**：使用 Personal Access Token（见上方说明）

### 问题 3：推送失败 - Repository not found

**症状**：
```
remote: Repository not found
```

**原因**：仓库地址错误或没有权限

**解决**：
1. 检查仓库地址是否正确
2. 确认你已经创建了仓库

### 问题 4：编译失败

**症状**：Actions 页面显示 ❌ 红叉

**解决**：
1. 点击失败的构建
2. 查看日志（红色文字）
3. 截图发给我，我帮你解决

---

## 第 7 部分：完整命令清单（复制粘贴版）

```bash
# 1. 进入项目目录
cd /workspace/projects/Jvedio

# 2. 查看状态（可选）
git status

# 3. 添加所有文件
git add .

# 4. 提交更改
git commit -m "feat: 添加千问 AI 功能"

# 5. 连接到你的仓库（替换用户名）
git remote set-url origin https://github.com/你的用户名/jvedio-ai.git

# 6. 推送到 GitHub
git push origin master
```

**第 5 步示例**（假设你的用户名是 `xiaoming`）：
```bash
git remote set-url origin https://github.com/xiaoming/jvedio-ai.git
```

---

## 第 8 部分：图形化方式（命令行恐惧症专用）

如果你完全不想用命令行：

### 使用网页直接上传

1. 创建仓库后，在仓库页面点击 **Add file** → **Upload files**
2. 拖拽所有文件到网页上
3. 在底部输入："feat: 添加千问 AI 功能"
4. 点击 **Commit changes**

⚠️ **注意**：这种方式**不会自动编译**！因为缺少 GitHub Actions 配置文件。

**推荐**：还是使用命令行方式。

---

## 第 9 部分：获取千问 API Key

编译后的软件需要配置 API Key 才能使用 AI 功能。

### 步骤

1. 访问：https://dashscope.console.aliyun.com/apiKey
2. 登录阿里云账号
3. 点击 **创建 API Key**
4. 复制生成的 Key（格式：`sk-xxxxxxxxxxxx`）
5. 把 Key 填入 `.env` 文件

---

## 🎯 总结：完整流程

1. **注册 GitHub 账号** → https://github.com/signup
2. **创建仓库** → 命名为 `jvedio-ai`
3. **上传代码** → 使用上面的命令
4. **等待编译** → 5-10 分钟
5. **下载软件** → 从 Actions 页面下载
6. **配置 AI** → 填入千问 API Key
7. **运行** → 双击 Jvedio.exe

---

## 💡 需要帮助？

如果在任何步骤遇到问题：
1. 截图发给我
2. 告诉我你在哪一步
3. 我会一步步帮你解决

---

**准备好了？现在就开始吧！** 🚀
