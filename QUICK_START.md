# 🚀 使用 GitHub Actions 自动编译 Jvedio（带千问 AI 功能）

## ⚡ 快速开始（3 步）

### 1️⃣ 创建 GitHub 仓库

访问 https://github.com/new 创建新仓库（不要初始化 README）

### 2️⃣ 推送代码

```bash
# 修改远程仓库地址（替换为你的地址）
git remote set-url origin https://github.com/你的用户名/jvedio-ai.git

# 提交并推送
git add .
git commit -m "feat: 添加千问 AI 功能"
git push origin master
```

### 3️⃣ 下载编译好的版本

1. 等待 5-10 分钟
2. 访问 https://github.com/你的用户名/jvedio-ai/actions
3. 点击最新的构建
4. 下载 `Jvedio-Windows` artifact

---

## 📦 编译完成后

1. 解压下载的文件
2. 编辑 `.env` 文件，填入千问 API Key：
   ```env
   DASHSCOPE_API_KEY = "sk-你的API密钥"
   ```
3. 运行 `Jvedio.exe`

---

## 🎯 新功能

✅ **演员性别识别** - 自动识别演员性别（仅限女性）
✅ **演员信息补全** - 自动补全生日、年龄、三围、爱好等
✅ **DashScope API 集成** - 使用阿里云千问大模型

---

## 📚 详细文档

- [GitHub Actions 详细指南](GITHUB_ACTIONS_GUIDE.md)
- [编译与使用说明](BUILD_GUIDE.md)
- [打包方案总结](PACKAGING_SUMMARY.md)

---

## 🔥 手动触发编译

不需要推送代码，直接手动编译：

1. 访问你的仓库
2. 点击 "Actions" 标签
3. 点击 "Build Jvedio with AI Features"
4. 点击 "Run workflow" 按钮

---

## 💡 与官方版本的区别

| 功能 | 官方版本 | 你的版本 |
|------|---------|---------|
| 视频管理 | ✅ | ✅ |
| AI 性别识别 | ❌ | ✅ |
| AI 信息补全 | ❌ | ✅ |
| DashScope API | ❌ | ✅ |

---

**准备好了？现在就推送代码吧！** 🎉
