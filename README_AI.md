# Jvedio AI 功能配置指南

## 功能概述

Jvedio 集成了阿里云千问大模型，提供以下 AI 功能：

1. **演员信息补全** - 根据演员名字或照片自动补全演员详细信息
   - 生日、年龄、血型
   - 身高、体重、罩杯
   - 三围（胸围、腰围、臀围）
   - 出生地、爱好

2. **性别识别** - 基于演员照片或名字识别性别

## 配置步骤

### 1. 获取 API Key

1. 访问 [阿里云百炼控制台](https://dashscope.console.aliyun.com/apiKey)
2. 登录阿里云账号
3. 创建 API Key
4. 复制 API Key

### 2. 配置 .env 文件

在 Jvedio 应用程序目录下创建 `.env` 文件：

```bash
# Windows 示例
C:\Users\YourName\AppData\Roaming\Jvedio\.env

# 复制示例文件
cp .env.example .env
```

编辑 `.env` 文件，填入你的 API Key：

```
DASHSCOPE_API_KEY=sk-xxxxxxxxxxxxxxxxxxxxxxxxxx
DASHSCOPE_API_BASE=https://dashscope.aliyuncs.com/compatible-mode/v1
```

### 3. 验证配置

启动 Jvedio 应用程序，尝试使用 AI 功能。如果配置正确，功能应该可以正常使用。

## 使用方法

### 演员信息补全

1. 打开演员编辑窗口
2. 点击"AI 补全信息"按钮
3. 等待 AI 处理完成
4. 查看补全结果和置信度

### 性别识别

性别识别功能在演员信息管理界面自动执行。

## 故障排查

### 问题 1：显示"未配置千问 API"

**原因**：`.env` 文件不存在或 API Key 未配置

**解决方案**：
1. 确认 `.env` 文件存在于应用程序目录
2. 检查 `DASHSCOPE_API_KEY` 是否已正确填写
3. 确认 API Key 格式正确（通常以 `sk-` 开头）

### 问题 2：显示"AI 补全成功！置信度：0%"

**原因**：API 返回的数据无法正确解析

**解决方案**：
1. 检查网络连接是否正常
2. 查看日志文件获取详细错误信息
   - 日志位置：`%AppData%\Jvedio\log\`
3. 确认 API Key 有效且有足够的配额

### 问题 3：显示"反序列化失败：无法解析返回数据"

**原因**：AI 返回的 JSON 格式不符合预期

**解决方案**：
1. 查看日志文件中的原始返回内容
2. 检查 API 是否返回了错误信息
3. 确认使用的模型是否支持此功能

## 日志查看

日志文件位于：
```
%AppData%\Jvedio\log\YYYY-MM-DD.log
```

关键日志关键词：
- `演员信息补全原始返回` - API 原始返回内容
- `反序列化结果` - 解析后的数据
- `JSON反序列化异常` - JSON 解析错误
- `演员信息补全成功` - 补全成功的信息
- `千问API配置加载成功` - API 配置状态

## 注意事项

1. **API 配额**：阿里云千问 API 有配额限制，请根据实际使用情况购买
2. **网络要求**：需要能够访问阿里云 API 服务
3. **数据准确性**：AI 补全的信息仅供参考，建议人工核对
4. **隐私安全**：不要泄露 API Key，.env 文件不会被提交到 Git

## API 费用

阿里云千问 API 采用按量计费：
- 免费额度：新用户有一定的免费额度
- 计费方式：根据 Token 使用量计费
- 具体价格：参考 [阿里云百炼价格页](https://help.aliyun.com/zh/dashscope/developer-reference/price-of-qwen)

## 技术支持

如果遇到问题，请：
1. 查看日志文件获取详细错误信息
2. 检查 API Key 配置是否正确
3. 确认网络连接和 API 服务状态
4. 提交 Issue 到 GitHub 仓库

## 更新日志

### v1.0.0 (2026-01-31)
- 初始版本，支持演员信息补全功能
- 支持性别识别功能
