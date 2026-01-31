using Jvedio.Core.Enums;
using System;

namespace Jvedio.Core.Config
{
    /// <summary>
    /// 性别识别配置
    /// </summary>
    [Serializable]
    public class GenderRecognitionConfig
    {
        /// <summary>
        /// 是否自动删除男性演员
        /// </summary>
        public bool AutoDeleteMaleActors { get; set; } = false;

        /// <summary>
        /// 是否使用大模型识别性别
        /// </summary>
        public bool UseLLMRecognition { get; set; } = true;

        /// <summary>
        /// 置信度阈值（低于此值不删除）
        /// </summary>
        public float ConfidenceThreshold { get; set; } = 0.7f;

        /// <summary>
        /// 是否启用复检机制
        /// </summary>
        public bool EnableDoubleCheck { get; set; } = false;

        /// <summary>
        /// 是否记录删除日志
        /// </summary>
        public bool LogDeletedActors { get; set; } = true;

        /// <summary>
        /// 删除日志保存路径
        /// </summary>
        public string LogFilePath { get; set; } = "deleted_actors.log";

        /// <summary>
        /// 是否启用白名单（白名单中的演员不会被删除）
        /// </summary>
        public bool EnableWhitelist { get; set; } = false;

        /// <summary>
        /// 白名单演员名字列表（用逗号分隔）
        /// </summary>
        public string WhitelistActors { get; set; } = "";

        /// <summary>
        /// 检查是否在白名单中
        /// </summary>
        public bool IsInWhitelist(string actorName)
        {
            if (!EnableWhitelist || string.IsNullOrEmpty(WhitelistActors))
                return false;

            var whitelist = WhitelistActors.Split(new[] { ',', '，', ';' }, 
                StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in whitelist)
            {
                if (item.Trim().Equals(actorName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 是否应该删除（综合考虑置信度和白名单）
        /// </summary>
        public bool ShouldDelete(Gender gender, float confidence, string actorName)
        {
            // 检查白名单
            if (IsInWhitelist(actorName))
            {
                return false;
            }

            // 检查性别
            if (gender != Gender.Boy)
            {
                return false;
            }

            // 检查置信度
            if (confidence < ConfidenceThreshold)
            {
                return false;
            }

            return true;
        }
    }
}
