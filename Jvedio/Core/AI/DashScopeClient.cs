using Jvedio.Core.Logs;
using Newtonsoft.Json;
using SuperUtils.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static Jvedio.App;

namespace Jvedio.Core.AI
{
    /// <summary>
    /// 千问API客户端
    /// </summary>
    public class DashScopeClient
    {
        private static readonly string _envFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            ".env"
        );

        private static string _apiKey;
        private static string _apiBase;

        static DashScopeClient()
        {
            LoadConfig();
        }

        /// <summary>
        /// 从.env文件加载配置
        /// </summary>
        private static void LoadConfig()
        {
            try
            {
                if (!File.Exists(_envFilePath))
                {
                    Logger.Error($"环境配置文件不存在: {_envFilePath}");
                    return;
                }

                var lines = File.ReadAllLines(_envFilePath, Encoding.UTF8);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    var parts = trimmed.Split('=');
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();

                        if (key == "DASHSCOPE_API_KEY")
                        {
                            _apiKey = value.Trim('"');
                        }
                        else if (key == "DASHSCOPE_API_BASE")
                        {
                            _apiBase = value.Trim('"');
                        }
                    }
                }

                Logger.Info($"千问API配置加载成功: {_apiBase}");
            }
            catch (Exception ex)
            {
                Logger.Error($"加载千问API配置失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 检查是否已配置
        /// </summary>
        public static bool IsConfigured()
        {
            return !string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiBase);
        }

        /// <summary>
        /// 发送聊天请求
        /// </summary>
        public static async Task<ChatResponse> ChatAsync(ChatRequest request)
        {
            if (!IsConfigured())
            {
                throw new Exception("千问API未配置，请检查.env文件");
            }

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(30);

                    var json = JsonConvert.SerializeObject(request);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var url = $"{_apiBase}/chat/completions";

                    var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
                    httpRequest.Content = content;
                    httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

                    Logger.Debug($"千问API请求: {url}");

                    var response = await httpClient.SendAsync(httpRequest);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Logger.Error($"千问API请求失败: {response.StatusCode}, {responseContent}");
                        throw new Exception($"API请求失败: {response.StatusCode}");
                    }

                    var chatResponse = JsonConvert.DeserializeObject<ChatResponse>(responseContent);
                    return chatResponse;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"千问API调用异常: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public static void ReloadConfig()
        {
            LoadConfig();
        }
    }

    #region API数据模型

    /// <summary>
    /// 聊天请求
    /// </summary>
    public class ChatRequest
    {
        [JsonProperty("model")]
        public string Model { get; set; } = "qwen-turbo";

        [JsonProperty("messages")]
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        [JsonProperty("temperature")]
        public float Temperature { get; set; } = 0.3f;

        [JsonProperty("max_tokens")]
        public int MaxTokens { get; set; } = 500;

        [JsonProperty("response_format")]
        public ResponseFormat ResponseFormat { get; set; } = new ResponseFormat { Type = "json_object" };
    }

    /// <summary>
    /// 聊天消息
    /// </summary>
    public class ChatMessage
    {
        [JsonProperty("role")]
        public string Role { get; set; }  // system, user, assistant

        [JsonProperty("content")]
        public string Content { get; set; }
    }

    /// <summary>
    /// 响应格式
    /// </summary>
    public class ResponseFormat
    {
        [JsonProperty("type")]
        public string Type { get; set; }
    }

    /// <summary>
    /// 聊天响应
    /// </summary>
    public class ChatResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("object")]
        public string Object { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("model")]
        public string Model { get; set; }

        [JsonProperty("choices")]
        public List<Choice> Choices { get; set; }

        [JsonProperty("usage")]
        public Usage Usage { get; set; }
    }

    /// <summary>
    /// 选择项
    /// </summary>
    public class Choice
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("message")]
        public ChatMessage Message { get; set; }

        [JsonProperty("finish_reason")]
        public string FinishReason { get; set; }
    }

    /// <summary>
    /// 使用情况
    /// </summary>
    public class Usage
    {
        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }

    #endregion
}
