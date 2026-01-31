using Jvedio.Core.Enums;
using Jvedio.Core.Logs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using static Jvedio.App;

namespace Jvedio.Core.AI
{
    /// <summary>
    /// 性别识别结果
    /// </summary>
    public class GenderRecognitionResult
    {
        public Gender Gender { get; set; } = Gender.Unknown;
        public float Confidence { get; set; } = 0f;
        public string Reason { get; set; } = "";
        public string RawResponse { get; set; } = "";
        public bool Success { get; set; } = false;
    }

    /// <summary>
    /// 性别识别服务
    /// </summary>
    public class GenderRecognitionService
    {
        /// <summary>
        /// 基于演员图片识别性别
        /// </summary>
        public static async Task<GenderRecognitionResult> RecognizeByImageAsync(
            string imagePath,
            string actorName = "")
        {
            var result = new GenderRecognitionResult();

            try
            {
                if (!File.Exists(imagePath))
                {
                    result.Reason = "图片文件不存在";
                    Logger.Instance.Warn($"图片不存在: {imagePath}");
                    return result;
                }

                // 将图片转换为Base64
                var base64Image = ConvertImageToBase64(imagePath);
                if (string.IsNullOrEmpty(base64Image))
                {
                    result.Reason = "图片转换失败";
                    Logger.Instance.Warn($"图片转换失败: {imagePath}");
                    return result;
                }

                // 构建提示词
                var prompt = BuildImagePrompt(actorName);
                
                // 构建请求
                var request = new ChatRequest
                {
                    Model = "qwen-vl-max",  // 使用视觉模型
                    Messages = new List<ChatMessage>
                    {
                        new ChatMessage
                        {
                            Role = "system",
                            Content = "你是一个专业的演员性别识别助手。请根据演员照片判断性别，返回JSON格式结果。"
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
                    }
                };

                // 调用API
                var response = await DashScopeClient.ChatAsync(request);

                if (response?.Choices != null && response.Choices.Count > 0)
                {
                    var content = response.Choices[0].Message.Content;
                    result.RawResponse = content;

                    // 解析JSON响应
                    var recognitionResult = JsonConvert.DeserializeObject<RecognitionResult>(content);
                    if (recognitionResult != null)
                    {
                        result.Gender = ParseGender(recognitionResult.Gender);
                        result.Confidence = recognitionResult.Confidence;
                        result.Reason = recognitionResult.Reason;
                        result.Success = true;

                        Logger.Instance.Info($"图片识别成功: {actorName} -> {result.Gender} (置信度: {result.Confidence})");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Reason = $"识别异常: {ex.Message}";
                Logger.Instance.Error($"图片识别异常: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 基于演员名字识别性别
        /// </summary>
        public static async Task<GenderRecognitionResult> RecognizeByNameAsync(string actorName)
        {
            var result = new GenderRecognitionResult();

            try
            {
                if (string.IsNullOrWhiteSpace(actorName))
                {
                    result.Reason = "演员名字为空";
                    return result;
                }

                // 构建提示词
                var prompt = BuildNamePrompt(actorName);

                // 构建请求
                var request = new ChatRequest
                {
                    Model = "qwen-turbo",
                    Messages = new List<ChatMessage>
                    {
                        new ChatMessage
                        {
                            Role = "system",
                            Content = "你是一个专业的演员性别识别助手。请根据演员名字判断性别，返回JSON格式结果。"
                        },
                        new ChatMessage
                        {
                            Role = "user",
                            Content = prompt
                        }
                    }
                };

                // 调用API
                var response = await DashScopeClient.ChatAsync(request);

                if (response?.Choices != null && response.Choices.Count > 0)
                {
                    var content = response.Choices[0].Message.Content;
                    result.RawResponse = content;

                    // 解析JSON响应
                    var recognitionResult = JsonConvert.DeserializeObject<RecognitionResult>(content);
                    if (recognitionResult != null)
                    {
                        result.Gender = ParseGender(recognitionResult.Gender);
                        result.Confidence = recognitionResult.Confidence;
                        result.Reason = recognitionResult.Reason;
                        result.Success = true;

                        Logger.Instance.Info($"名字识别成功: {actorName} -> {result.Gender} (置信度: {result.Confidence})");
                    }
                }
            }
            catch (Exception ex)
            {
                result.Reason = $"识别异常: {ex.Message}";
                Logger.Instance.Error($"名字识别异常: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 智能识别（优先图片，其次名字）
        /// </summary>
        public static async Task<GenderRecognitionResult> RecognizeAsync(
            string imagePath,
            string actorName)
        {
            // 优先级1：有图片，使用图片识别
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                return await RecognizeByImageAsync(imagePath, actorName);
            }

            // 优先级2：有名字，使用名字识别
            if (!string.IsNullOrEmpty(actorName))
            {
                return await RecognizeByNameAsync(actorName);
            }

            // 都没有，返回未知
            return new GenderRecognitionResult
            {
                Reason = "无图片和名字信息",
                Success = false
            };
        }

        #region 私有方法

        /// <summary>
        /// 构建图片识别提示词
        /// </summary>
        private static string BuildImagePrompt(string actorName)
        {
            var nameHint = string.IsNullOrEmpty(actorName) ? "" : $"\n演员名字：{actorName}";

            return $@"重要提示：以下演员都是**日本AV行业演员**，请结合日本AV行业的特点进行判断。{nameHint}

请以JSON格式返回结果，格式如下：
{{
  ""gender"": ""男性""或""女性"",
  ""confidence"": 0.0到1.0之间的数值，
  ""reason"": ""简要说明判断依据，如面部特征、发型、妆容、穿着等""
}}

注意：
1. 如果照片中没有人脸或模糊不清，confidence设为0，gender设为""未知""
2. 如果是明显的中性打扮或无法确定，confidence设为0.5以下
3. 只有在非常确定的情况下，confidence才设为0.9以上
4. 参考日本AV行业的常见特征：女性演员通常有明显的女性化特征，男性演员通常较为成熟";
        }

        /// <summary>
        /// 构建名字识别提示词
        /// </summary>
        private static string BuildNamePrompt(string actorName)
        {
            return $@"重要提示：以下演员是**日本AV行业演员**，请结合日本AV行业的命名规则和文化背景进行判断。

演员名字：{actorName}

请以JSON格式返回结果，格式如下：
{{
  ""gender"": ""男性""或""女性"",
  ""confidence"": 0.0到1.0之间的数值，
  ""reason"": ""简要说明判断依据，如名字特征、性别暗示、汉字含义等""
}}

注意：
1. 日本男性常见名字：健太、大介、拓也、翔太、雄二、田中、佐藤等，confidence设为0.9以上
2. 日本女性常见名字：美咲、結衣、愛子、花子、桜子、鈴木、高橋等，confidence设为0.9以上
3. 注意汉字的含义：如""咲""、""美""、""愛""多为女性，""健""、""太""、""介""多为男性
4. 如果名字中性或无法确定，confidence设为0.5以下
5. 如果完全没有线索，gender设为""未知""，confidence设为0";
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
                string mimeType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    _ => "image/jpeg"
                };

                return $"data:{mimeType};base64,{base64}";
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"图片转换Base64失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析性别
        /// </summary>
        private static Gender ParseGender(string genderText)
        {
            if (string.IsNullOrEmpty(genderText))
                return Gender.Unknown;

            if (genderText.Contains("男") || genderText.Contains("Male"))
                return Gender.Boy;

            if (genderText.Contains("女") || genderText.Contains("Female"))
                return Gender.Girl;

            if (genderText.Contains("未知"))
                return Gender.Unknown;

            return Gender.Unknown;
        }

        #endregion
    }

    #region 识别结果模型

    /// <summary>
    /// 识别结果（从API返回的JSON）
    /// </summary>
    public class RecognitionResult
    {
        [JsonProperty("gender")]
        public string Gender { get; set; }

        [JsonProperty("confidence")]
        public float Confidence { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }
    }

    #endregion
}
