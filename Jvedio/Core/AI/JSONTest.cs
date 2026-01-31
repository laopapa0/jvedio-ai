using Jvedio.Core.Logs;
using Newtonsoft.Json;

namespace Jvedio.Core.AI
{
    /// <summary>
    /// JSON 解析测试
    /// </summary>
    public static class JSONTest
    {
        /// <summary>
        /// 测试 JSON 解析是否正常
        /// </summary>
        public static bool TestJSONParsing()
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

                Logger.Instance.Warn("开始测试 JSON 解析...");
                Logger.Instance.Warn($"测试 JSON: {testJson}");

                var data = JsonConvert.DeserializeObject<CompletedActorData>(testJson);

                if (data == null)
                {
                    Logger.Instance.Error("JSON 解析返回 null！");
                    return false;
                }

                Logger.Instance.Warn("JSON 解析成功！");
                Logger.Instance.Warn($"Birthday: {data.Birthday}");
                Logger.Instance.Warn($"Age: {data.Age}");
                Logger.Instance.Warn($"Height: {data.Height}");
                Logger.Instance.Warn($"Weight: {data.Weight}");
                Logger.Instance.Warn($"Cup: {data.Cup}");

                return true;
            }
            catch (System.Exception ex)
            {
                Logger.Instance.Error($"JSON 解析测试失败: {ex.Message}");
                Logger.Instance.Error($"堆栈: {ex.StackTrace}");
                return false;
            }
        }
    }
}
