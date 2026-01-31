# ========================================
# Jvedio 发布包清单
# ========================================

## 核心程序
- Jvedio.exe                    # 主程序（编译后生成）
- config.ini                    # 配置文件
- Jvedio.ico                    # 程序图标
- .env                          # AI 配置（用户填写）
- .env.example                  # AI 配置模板

## 依赖库 (Reference/)
- CommonNet.dll                 # 通用工具库
- HtmlAgilityPack.dll           # HTML 解析
- ICSharpCode.AvalonEdit.dll    # 代码编辑器
- JvedioLib.dll                 # Jvedio 核心库
- MediaInfo.dll                 # 媒体信息
- MediaInfoNET.dll              # MediaInfo .NET 封装
- Newtonsoft.Json.dll           # JSON 解析
- PInvoke.dll                   # Windows API 调用
- QueryEngine.dll               # 查询引擎
- SuperControls.Style.dll       # UI 样式库
- SuperUtils.dll                # 工具库
- UsnOperation.dll              # USN 操作

## 数据文件 (Data/)
- x64/SQLite.Interop.dll        # SQLite x64 驱动
- x86/SQLite.Interop.dll        # SQLite x86 驱动

## 插件 (plugins/crawlers/)
- CommonNet.dll                 # 爬虫插件依赖
- HtmlAgilityPack.dll           # HTML 解析依赖

## 文档（可选）
- README.md                     # 项目说明
- BUILD_GUIDE.md                # 编译与使用指南

## 编译生成（Release 模式）
- *.dll                         # 所有依赖 DLL
- *.pdb                         # 调试符号（发布时可删除）
- *.xml                         # XML 文档（发布时可删除）

## 清理规则（发布时）
❌ 删除以下文件：
- *.pdb                         # 调试符号
- *.xml                         # XML 文档
- *.vshost.exe                  # Visual Studio 宿主
- *.vshost.exe.manifest         # 宿主清单

✅ 保留以下文件：
- Jvedio.exe                    # 主程序
- Jvedio.exe.config             # 配置文件
- 所有 .dll 依赖库
- .env                          # AI 配置
- config.ini                    # 软件配置

## 打包命令示例

### 7-Zip 压缩
```bash
7z a Jvedio-5.4.zip Release\* -mx9
```

### Windows PowerShell 压缩
```powershell
Compress-Archive -Path Release\* -DestinationPath Jvedio-5.4.zip
```

### Linux/Mac 压缩（如果交叉编译）
```bash
zip -r Jvedio-5.4.zip Release/
```

## 预估大小

- 未压缩：约 30-40 MB
- 压缩后：约 10-15 MB
