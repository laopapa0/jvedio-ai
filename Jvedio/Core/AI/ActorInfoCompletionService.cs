using Jvedio.Core.Enums;
using Jvedio.Core.Logs;
using Jvedio.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Jvedio.Core.AI
{
    /// <summary>
    /// 演员信息补全结果
    /// </summary>
    public class ActorCompletionResult
    {
        public bool Success { get; set; } = false;
        public string Message { get; set; } = "";
        public ActorInfo CompletedInfo { get; set; } = null;
        public float Confidence { get; set; } = 0f;
        public string RawResponse { get; set; } = "";
    }

    /// <summary>
    /// 补全的演员信息（从API返回）
    /// </summary>
    public class CompletedActorData
    {
        [JsonProperty("birthday")]
        public string Birthday { get; set; } = null;

        [JsonProperty("age")]
        public int? Age { get; set; } = null;

        [JsonProperty("bloodType")]
        public string BloodType { get; set; } = null;

        [JsonProperty("height")]
        public int? Height { get; set; } = null;

        [JsonProperty("weight")]
        public int? Weight { get; set; } = null;

        [JsonProperty("cup")]
        public char? Cup { get; set; } = null;

        [JsonProperty("chest")]
        public int? Chest { get; set; } = null;

        [JsonProperty("waist")]
        public int? Waist { get; set; } = null;

        [JsonProperty("hipline")]
        public int? Hipline { get; set; } = null;

        [JsonProperty("birthPlace")]
        public string BirthPlace { get; set; } = null;

        [JsonProperty("hobby")]
        public string Hobby { get; set; } = null;

        [JsonProperty("reason")]
        public string Reason { get; set; } = "";
    }

    /// <summary>
    /// 演员信息补全服务
    /// </summary>
    public static class ActorInfoCompletionService
    {
        /// <summary>
        /// 检查演员信息是否需要补全
        /// </summary>
        public static bool NeedCompletion(ActorInfo actorInfo, float missingThreshold = 0.7f)
        {
            // 只处理女性演员
            if (actorInfo.Gender != Gender.Girl && actorInfo.Gender != Gender.Unknown)
            {
                return false;
            }

            // 计算缺失信息比例
            var totalFields = 11; // Birthday, Age, BloodType, Height, Weight, Cup, Chest, Waist, Hipline, BirthPlace, Hobby
            var missingFields = 0;

            if (string.IsNullOrEmpty(actorInfo.Birthday)) missingFields++;
            if (actorInfo.Age == 0) missingFields++;
            if (string.IsNullOrEmpty(actorInfo.BloodType)) missingFields++;
            if (actorInfo.Height == 0) missingFields++;
            if (actorInfo.Weight == 0) missingFields++;
            if (actorInfo.Cup == 'Z') missingFields++;
            if (actorInfo.Chest == 0) missingFields++;
            if (actorInfo.Waist == 0) missingFields++;
            if (actorInfo.Hipline == 0) missingFields++;
            if (string.IsNullOrEmpty(actorInfo.BirthPlace)) missingFields++;
            if (string.IsNullOrEmpty(actorInfo.Hobby)) missingFields++;

            var missingRatio = (float)missingFields / totalFields;

            // 如果缺失比例超过阈值，需要补全
            var needCompletion = missingRatio >= missingThreshold;

            if (needCompletion)
            {
                Logger.Instance.Info($"演员 {actorInfo.ActorName} 信息缺失 {missingFields}/{totalFields} ({missingRatio:P0})，需要补全");
            }

            return needCompletion;
        }

        /// <summary>
        /// 补全演员信息（基于名字）
        /// </summary>
        public static async Task<ActorCompletionResult> CompleteByNameAsync(ActorInfo actorInfo)
        {
            var result = new ActorCompletionResult();

            try
            {
                if (string.IsNullOrWhiteSpace(actorInfo.ActorName))
                {
                    result.Message = "演员名字为空";
                    return result;
                }

                // 检查是否需要补全
                if (!NeedCompletion(actorInfo))
                {
                    result.Message = "演员信息完整，无需补全";
                    result.Success = true;
                    return result;
                }

                // 构建提示词
                var prompt = BuildCompletionPrompt(actorInfo.ActorName, null, actorInfo);

                // 构建请求
                var request = new ChatRequest
                {
                    Model = "qwen-max",  // 使用更强的模型
                    Messages = new List<ChatMessage>
                    {
                        new ChatMessage
                        {
                            Role = "system",
                            Content = "你是一个专业的日本AV行业女演员信息助手。请根据演员名字补全演员的详细信息。"
                        },
                        new ChatMessage
                        {
                            Role = "user",
                            Content = prompt
                        }
                    },
                    Temperature = 0.2f,  // 降低温度，提高准确性
                    MaxTokens = 1000
                };

                // 调用API
                var response = await DashScopeClient.ChatAsync(request);

                if (response?.Choices != null && response.Choices.Count > 0)
                {
                    var content = response.Choices[0].Message.Content;
                    result.RawResponse = content;

                    Logger.Instance.Debug($"演员信息补全原始返回: {actorInfo.ActorName} -> {content}");

                    // 解析JSON响应
                    var completedData = JsonConvert.DeserializeObject<CompletedActorData>(content);
                    if (completedData != null)
                    {
                        Logger.Instance.Debug($"反序列化结果: Birthday={completedData.Birthday}, Age={completedData.Age}, Height={completedData.Height}");

                        // 应用补全的信息
                        ApplyCompletion(actorInfo, completedData);

                        result.CompletedInfo = actorInfo;
                        result.Confidence = CalculateConfidence(completedData);
                        result.Success = true;
                        result.Message = $"成功补全 {completedData.Reason}";

                        Logger.Instance.Info($"演员信息补全成功: {actorInfo.ActorName}, 理由: {completedData.Reason}, 置信度: {result.Confidence:P0}");
                    }
                    else
                    {
                        Logger.Instance.Warn($"演员信息反序列化失败: {actorInfo.ActorName}");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = $"补全异常: {ex.Message}";
                Logger.Instance.Error($"演员信息补全异常: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 补全演员信息（基于图片）
        /// </summary>
        public static async Task<ActorCompletionResult> CompleteByImageAsync(
            ActorInfo actorInfo,
            string imagePath)
        {
            var result = new ActorCompletionResult();

            try
            {
                if (string.IsNullOrWhiteSpace(actorInfo.ActorName))
                {
                    result.Message = "演员名字为空";
                    return result;
                }

                if (!File.Exists(imagePath))
                {
                    result.Message = "图片文件不存在";
                    return result;
                }

                // 检查是否需要补全
                if (!NeedCompletion(actorInfo))
                {
                    result.Message = "演员信息完整，无需补全";
                    result.Success = true;
                    return result;
                }

                // 转换图片为Base64
                var base64Image = ConvertImageToBase64(imagePath);
                if (string.IsNullOrEmpty(base64Image))
                {
                    result.Message = "图片转换失败";
                    return result;
                }

                // 构建提示词
                var prompt = BuildCompletionPrompt(actorInfo.ActorName, base64Image, actorInfo);

                // 构建请求
                var request = new ChatRequest
                {
                    Model = "qwen-vl-max",  // 使用视觉模型
                    Messages = new List<ChatMessage>
                    {
                        new ChatMessage
                        {
                            Role = "system",
                            Content = "你是一个专业的日本AV行业女演员信息助手。请根据演员照片和名字补全详细信息。"
                        },
                        new ChatMessage
                        {
                            Role = "user",
                            Content = new
                            {
                                type = "image",
                                image = base64Image
                            }.ToString()
                        },
                        new ChatMessage
                        {
                            Role = "user",
                            Content = prompt
                        }
                    },
                    Temperature = 0.2f,
                    MaxTokens = 1000
                };

                // 调用API
                var response = await DashScopeClient.ChatAsync(request);

                if (response?.Choices != null && response.Choices.Count > 0)
                {
                    var content = response.Choices[0].Message.Content;
                    result.RawResponse = content;

                    Logger.Instance.Debug($"图片补全原始返回: {actorInfo.ActorName} -> {content}");

                    // 解析JSON响应
                    var completedData = JsonConvert.DeserializeObject<CompletedActorData>(content);
                    if (completedData != null)
                    {
                        Logger.Instance.Debug($"反序列化结果: Birthday={completedData.Birthday}, Age={completedData.Age}, Height={completedData.Height}");

                        // 应用补全的信息
                        ApplyCompletion(actorInfo, completedData);

                        result.CompletedInfo = actorInfo;
                        result.Confidence = CalculateConfidence(completedData) + 0.1f; // 图片+名字置信度更高
                        result.Success = true;
                        result.Message = $"成功补全 {completedData.Reason}";

                        Logger.Instance.Info($"演员信息补全成功: {actorInfo.ActorName}, 理由: {completedData.Reason}, 置信度: {result.Confidence:P0}");
                    }
                    else
                    {
                        Logger.Instance.Warn($"演员信息反序列化失败: {actorInfo.ActorName}");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Message = $"补全异常: {ex.Message}";
                Logger.Instance.Error($"演员信息补全异常: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 智能补全（优先图片，其次名字）
        /// </summary>
        public static async Task<ActorCompletionResult> CompleteAsync(
            ActorInfo actorInfo,
            string imagePath = null)
        {
            // 优先级1：有图片，使用图片补全
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                return await CompleteByImageAsync(actorInfo, imagePath);
            }

            // 优先级2：有名字，使用名字补全
            if (!string.IsNullOrEmpty(actorInfo.ActorName))
            {
                return await CompleteByNameAsync(actorInfo);
            }

            return new ActorCompletionResult
            {
                Message = "无图片和名字信息，无法补全"
            };
        }

        #region 私有方法

        /// <summary>
        /// 构建补全提示词
        /// </summary>
        private static string BuildCompletionPrompt(
            string actorName,
            string base64Image,
            ActorInfo existingInfo)
        {
            var imageHint = !string.IsNullOrEmpty(base64Image) ? "\n已提供演员照片" : "";

            return $@"重要提示：这是**日本AV行业女演员**，请根据现有信息补全详细信息。{imageHint}

演员名字：{actorName}

已有信息：
- 性别：女性
- 生日：{existingInfo.Birthday ?? "未提供"}
- 年龄：{existingInfo.Age}
- 身高：{existingInfo.Height}
- 体重：{existingInfo.Weight}
- 血型：{existingInfo.BloodType ?? "未提供"}
- 三围：{existingInfo.Chest}/{existingInfo.Waist}/{existingInfo.Hipline}
- 出生地：{existingInfo.BirthPlace ?? "未提供"}
- 爱好：{existingInfo.Hobby ?? "未提供"}

请以JSON格式返回补全的信息，只补全缺失的字段：
{{
  ""birthday"": ""YYYY-MM-DD""或null（如果不知道确切日期，可以留空或用年份）,
  ""age"": 整数或null,
  ""bloodType"": ""A""或""B""或""AB""或""O""或null,
  ""height"": 厘米整数或null,
  ""weight"": 公斤整数或null,
  ""cup"": ""A""-""Z""单字母或null,
  ""chest"": 厘米整数或null,
  ""waist"": 厘米整数或null,
  ""hipline"": 厘米整数或null,
  ""birthPlace"": ""出生地""或null（如：东京都、大阪府等）,
  ""hobby"": ""爱好""或null,
  ""reason"": ""补全信息来源说明""
}}

重要规则：
1. 只补全确实缺失的字段，已有信息不要修改
2. 如果无法确定某个字段值，返回null，不要编造
3. 对于知名女演员，可以根据公开信息补全
4. 对于不出名的演员，只能根据照片推断（如身高、体型）
5. 生日格式必须为YYYY-MM-DD，或仅年份YYYY，不确定则返回null
6. 三围范围要合理（Chest: 70-120cm, Waist: 50-80cm, Hipline: 70-120cm）
7. 身高范围：140-180cm，体重：35-80kg
8. Cup范围：A-Z（日本常见为B-F）";
        }

        /// <summary>
        /// 应用补全的信息
        /// </summary>
        private static void ApplyCompletion(ActorInfo actorInfo, CompletedActorData completedData)
        {
            if (!string.IsNullOrEmpty(completedData.Birthday))
            {
                actorInfo.Birthday = completedData.Birthday;
            }

            if (completedData.Age.HasValue && completedData.Age > 0)
            {
                actorInfo.Age = completedData.Age.Value;
            }

            if (!string.IsNullOrEmpty(completedData.BloodType))
            {
                actorInfo.BloodType = completedData.BloodType;
            }

            if (completedData.Height.HasValue && completedData.Height > 0)
            {
                actorInfo.Height = completedData.Height.Value;
            }

            if (completedData.Weight.HasValue && completedData.Weight > 0)
            {
                actorInfo.Weight = completedData.Weight.Value;
            }

            if (completedData.Cup.HasValue && completedData.Cup.Value != 'Z')
            {
                actorInfo.Cup = completedData.Cup.Value;
            }

            if (completedData.Chest.HasValue && completedData.Chest > 0)
            {
                actorInfo.Chest = completedData.Chest.Value;
            }

            if (completedData.Waist.HasValue && completedData.Waist > 0)
            {
                actorInfo.Waist = completedData.Waist.Value;
            }

            if (completedData.Hipline.HasValue && completedData.Hipline > 0)
            {
                actorInfo.Hipline = completedData.Hipline.Value;
            }

            if (!string.IsNullOrEmpty(completedData.BirthPlace))
            {
                actorInfo.BirthPlace = completedData.BirthPlace;
            }

            if (!string.IsNullOrEmpty(completedData.Hobby))
            {
                actorInfo.Hobby = completedData.Hobby;
            }

            actorInfo.Gender = Gender.Girl;  // 明确标记为女性
        }

        /// <summary>
        /// 计算置信度
        /// </summary>
        private static float CalculateConfidence(CompletedActorData data)
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
        /// 转换图片为Base64
        /// </summary>
        private static string ConvertImageToBase64(string imagePath)
        {
            try
            {
                var bytes = File.ReadAllBytes(imagePath);
                var base64 = Convert.ToBase64String(bytes);

                // 根据文件扩展名添加数据URI前缀
                var extension = Path.GetExtension(imagePath).ToLower();
                string mimeType;
                if (extension == ".jpg" || extension == ".jpeg")
                {
                    mimeType = "image/jpeg";
                }
                else if (extension == ".png")
                {
                    mimeType = "image/png";
                }
                else if (extension == ".webp")
                {
                    mimeType = "image/webp";
                }
                else
                {
                    mimeType = "image/jpeg";
                }

                return $"data:{mimeType};base64,{base64}";
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"图片转换Base64失败: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
