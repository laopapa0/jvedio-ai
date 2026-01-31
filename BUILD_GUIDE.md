# Jvedio 打包与使用指南

## 📦 快速开始

### 方式一：下载预编译版本（推荐）

1. 访问 [GitHub Releases](https://github.com/hitchao/Jvedio/releases)
2. 下载最新版本的 `Jvedio-x.x.x.zip`
3. 解压到任意目录
4. 编辑 `.env` 文件（见下方配置说明）
5. 运行 `Jvedio.exe`

### 方式二：自行编译

#### 前置要求

- Windows 10/11
- Visual Studio 2017+ 或 .NET Framework 4.7.2 SDK
- [NuGet](https://www.nuget.org/downloads) （可选，VS 会自动安装）

#### 编译步骤

1. **克隆或下载项目**
   ```bash
   git clone https://github.com/hitchao/Jvedio.git
   cd Jvedio
   ```

2. **运行编译脚本**
   ```bash
   build.bat
   ```
   编译完成后，`Release` 文件夹包含所有运行文件。

3. **打包发布**
   将 `Release` 文件夹压缩为 `Jvedio-5.4.zip` 即可。

---

## ⚙️ AI 功能配置

### 1. 获取千问 API Key

1. 访问 [阿里云 DashScope 控制台](https://dashscope.console.aliyun.com/apiKey)
2. 登录阿里云账号
3. 点击"创建 API Key"
4. 复制生成的 API Key

### 2. 配置 .env 文件

在软件根目录下创建或编辑 `.env` 文件：

```env
# 千问API配置
DASHSCOPE_API_KEY = "sk-你的API密钥"
DASHSCOPE_API_BASE = "https://dashscope.aliyuncs.com/compatible-mode/v1"
```

### 3. 测试 AI 功能

1. 启动软件
2. 进入"设置" → "AI 配置"
3. 点击"测试连接"按钮
4. 如果成功，可以开始使用性别识别和信息补全功能

---

## 📁 发布包文件结构

```
Jvedio/
├── Jvedio.exe                 # 主程序
├── .env                       # AI 配置文件 ⚠️ 需要用户填写
├── config.ini                 # 软件配置
├── Jvedio.ico                 # 图标文件
│
├── Reference/                 # 核心依赖库
│   ├── CommonNet.dll
│   ├── HtmlAgilityPack.dll
│   ├── ICSharpCode.AvalonEdit.dll
│   ├── JvedioLib.dll
│   ├── MediaInfo.dll
│   ├── MediaInfoNET.dll
│   ├── Newtonsoft.Json.dll
│   ├── PInvoke.dll
│   ├── QueryEngine.dll
│   ├── SuperControls.Style.dll
│   ├── SuperUtils.dll
│   └── UsnOperation.dll
│
├── Data/                      # 数据文件
│   ├── x64/
│   │   └── SQLite.Interop.dll
│   └── x86/
│       └── SQLite.Interop.dll
│
└── plugins/                   # 插件目录
    └── crawlers/              # 爬虫插件
        └── CommonNet.dll
```

---

## 🔧 编译脚本说明

### build.bat 工作流程

1. **检查环境** - 验证 MSBuild 是否已安装
2. **恢复依赖** - 使用 NuGet 恢复项目依赖
3. **清理旧版本** - 删除旧的编译输出
4. **编译项目** - 使用 Release 配置编译
5. **准备发布包** - 复制文件到 Release 目录
6. **清理临时文件** - 删除调试符号和 XML 文档

### 手动编译（如果不使用脚本）

```bash
# 1. 恢复 NuGet 包
cd Jvedio
nuget restore packages.config

# 2. 编译
msbuild Jvedio.csproj /t:Build /p:Configuration=Release

# 3. 复制文件（手动执行 PostBuildEvent）
xcopy bin\Release\* ..\Release\ /E /I /Y
```

---

## 🚀 AI 功能使用说明

### 演员性别识别

1. 进入"演员管理"页面
2. 选中性别未知的演员
3. 右键 → "AI 识别性别"
4. 系统自动调用千问 API 识别性别

**提示词策略**：限定为"日本AV行业演员"以提高识别准确率

### 演员信息补全

1. 选择信息不完整的演员
2. 右键 → "AI 补全信息"
3. 系统自动补全以下信息：
   - 生日、年龄
   - 血型
   - 身高、体重、三围
   - 出生地
   - 爱好

**智能判断**：仅当缺失信息超过 70% 时才会触发补全

---

## ⚠️ 常见问题

### 1. 编译失败

**问题**：`找不到 EntityFramework.props`

**解决**：
```bash
# 手动安装 NuGet 包
nuget restore packages.config
```

### 2. 运行时缺少 DLL

**问题**：`无法加载 DLL`

**解决**：检查 `plugins/crawlers` 目录是否包含 `CommonNet.dll`

### 3. AI 功能不可用

**问题**：点击 AI 按钮无反应

**解决**：
1. 检查 `.env` 文件是否正确配置
2. 检查 API Key 是否有效
3. 检查网络连接是否正常
4. 查看日志文件 `Logs/app.log`

### 4. FFmpeg 截图失败

**问题**：无法截取视频截图

**解决**：
1. 确保已安装 FFmpeg
2. 检查 FFmpeg 路径配置
3. 检查视频文件是否损坏

---

## 📝 版本信息

- **当前版本**：5.4
- **.NET Framework**：4.7.2
- **支持平台**：Windows 10/11
- **License**：GPL-3.0

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

项目地址：https://github.com/hitchao/Jvedio
