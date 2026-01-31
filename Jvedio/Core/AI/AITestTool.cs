using Jvedio.Core.Logs;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Jvedio.Core.AI
{
    /// <summary>
    /// AI 功能测试工具
    /// </summary>
    public static class AITestTool
    {
        /// <summary>
        /// 测试 API 配置
        /// </summary>
        public static async Task<string> TestAPIConfig()
        {
            try
            {
                if (!DashScopeClient.IsConfigured())
                {
                    return "❌ API 未配置：请检查 .env 文件中的 DASHSCOPE_API_KEY";
                }

                var request = new ChatRequest
                {
                    Model = "qwen-max",
                    Messages = new System.Collections.Generic.List<ChatMessage>
                    {
                        new ChatMessage
                        {
                            Role = "user",
                            Content = "请回复'测试成功'，不要包含其他内容"
                        }
                    }
                };

                var response = await DashScopeClient.ChatAsync(request);

                if (response?.Choices != null && response.Choices.Count > 0)
                {
                    var content = response.Choices[0].Message.Content;
                    return $"✅ API 配置正常\n\n服务器响应: {content}";
                }
                else
                {
                    return "❌ API 调用失败：返回数据为空";
                }
            }
            catch (Exception ex)
            {
                return $"❌ API 测试失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 测试 JSON 解析
        /// </summary>
        public static string TestJSONParsing()
        {
            try
            {
                var testJson = @"{
  ""birthday"": ""1995-03-15"",
  ""age"": 29,
  ""bloodType"": ""A"",
  ""height"": 165,
  ""weight"": 48,
  ""cup"": ""C"",
  ""chest"": 86,
  ""waist"": 58,
  ""hipline"": 88,
  ""birthPlace"": ""东京都"",
  ""hobby"": ""旅行、摄影"",
  ""reason"": ""测试数据""
}";

                var data = JsonConvert.DeserializeObject<CompletedActorData>(testJson);

                if (data != null)
                {
                    var result = $"✅ JSON 解析正常\n\n";
                    result += $"解析结果:\n";
                    result += $"- 生日: {data.Birthday}\n";
                    result += $"- 年龄: {data.Age}\n";
                    result += $"- 身高: {data.Height}\n";
                    result += $"- 置信度: {CalculateTestConfidence(data):P0}";

                    return result;
                }
                else
                {
                    return "❌ JSON 解析失败：反序列化返回 null";
                }
            }
            catch (Exception ex)
            {
                return $"❌ JSON 解析测试失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 计算测试置信度
        /// </summary>
        private static float CalculateTestConfidence(CompletedActorData data)
        {
            var completedFields = 0;
            var totalFields = 11;

            if (!string.IsNullOrEmpty(data.Birthday)) completedFields++;
            if (data.Age.HasValue) completedFields++;
            if (!string.IsNullOrEmpty(data.BloodType)) completedFields++;
            if (data.Height.HasValue) completedFields++;
            if (data.Weight.HasValue) completedFields++;
            if (data.Cup.HasValue) completedFields++;
            if (data.Chest.HasValue) completedFields++;
            if (data.Waist.HasValue) completedFields++;
            if (data.Hipline.HasValue) completedFields++;
            if (!string.IsNullOrEmpty(data.BirthPlace)) completedFields++;
            if (!string.IsNullOrEmpty(data.Hobby)) completedFields++;

            return (float)completedFields / totalFields;
        }

        /// <summary>
        /// 运行完整诊断
        /// </summary>
        public static async Task<string> RunDiagnostics()
        {
            Logger.Instance.Info("开始 AI 功能诊断...");

            var result = "=== Jvedio AI 功能诊断报告 ===\n\n";

            // 1. 测试 API 配置
            result += "【1. API 配置测试】\n";
            var apiTest = await TestAPIConfig();
            result += apiTest + "\n\n";

            // 2. 测试 JSON 解析
            result += "【2. JSON 解析测试】\n";
            var jsonTest = TestJSONParsing();
            result += jsonTest + "\n\n";

            // 3. 检查 .env 文件
            result += "【3. 配置文件检查】\n";
            var envPath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                ".env"
            );
            if (System.IO.File.Exists(envPath))
            {
                result += $"✅ .env 文件存在: {envPath}\n";
                var lines = System.IO.File.ReadAllLines(envPath);
                var hasApiKey = false;
                foreach (var line in lines)
                {
                    if (line.Trim().StartsWith("DASHSCOPE_API_KEY=") &&
                        !line.Contains("your_api_key_here"))
                    {
                        hasApiKey = true;
                        break;
                    }
                }
                if (hasApiKey)
                {
                    result += "✅ API Key 已配置\n";
                }
                else
                {
                    result += "❌ API Key 未配置或使用默认值\n";
                }
            }
            else
            {
                result += $"❌ .env 文件不存在: {envPath}\n";
            }
            result += "\n";

            // 4. 总结
            result += "【诊断建议】\n";
            if (apiTest.Contains("✅") && jsonTest.Contains("✅"))
            {
                result += "✅ 所有测试通过，AI 功能应该可以正常使用\n";
            }
            else
            {
                result += "❌ 存在问题，请按照上述错误信息进行修复\n";
                result += "💡 建议：查看日志文件获取更详细的错误信息\n";
                result += "📝 日志位置: %AppData%\\Jvedio\\log\\";
            }

            Logger.Instance.Info("AI 功能诊断完成");
            return result;
        }
    }
}
