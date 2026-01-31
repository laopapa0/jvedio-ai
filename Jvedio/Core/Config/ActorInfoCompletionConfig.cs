using System;

namespace Jvedio.Core.Config
{
    /// <summary>
    /// 演员信息补全配置
    /// </summary>
    [Serializable]
    public class ActorInfoCompletionConfig
    {
        /// <summary>
        /// 是否启用自动补全
        /// </summary>
        public bool EnableAutoCompletion { get; set; } = false;

        /// <summary>
        /// 信息缺失阈值（0.0-1.0，超过此值才会触发补全）
        /// </summary>
        public float MissingThreshold { get; set; } = 0.5f;

        /// <summary>
        /// 只处理主要女演员（前N名）
        /// </summary>
        public bool OnlyMainActresses { get; set; } = true;

        /// <summary>
        /// 主要女演员数量（只补全前N名）
        /// </summary>
        public int MainActressCount { get; set; } = 3;

        /// <summary>
        /// 最大同时处理数（避免API限流）
        /// </summary>
        public int MaxConcurrentCompletions { get; set; } = 5;

        /// <summary>
        /// 是否使用图片补全（更准确但更慢）
        /// </summary>
        public bool UseImageCompletion { get; set; } = true;

        /// <summary>
        /// 是否记录补全日志
        /// </summary>
        public bool LogCompletion { get; set; } = true;

        /// <summary>
        /// 补全日志保存路径
        /// </summary>
        public string LogFilePath { get; set; } = "actor_completion.log";

        /// <summary>
        /// 是否需要用户确认（补全前先预览，等待用户确认）
        /// </summary>
        public bool RequireConfirmation { get; set; } = false;

        /// <summary>
        /// 检查是否应该补全
        /// </summary>
        public bool ShouldComplete(
            int actorIndex,
            bool isFemale,
            float missingRatio)
        {
            // 只处理女性演员
            if (!isFemale)
            {
                return false;
            }

            // 如果只处理主要女演员，检查索引
            if (OnlyMainActresses && actorIndex >= MainActressCount)
            {
                return false;
            }

            // 检查缺失比例
            if (missingRatio < MissingThreshold)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 获取处理策略
        /// </summary>
        public string GetProcessingStrategy()
        {
            var strategy = "";

            if (OnlyMainActresses)
            {
                strategy += $"只处理前{MainActressCount}名主要女演员，";
            }
            else
            {
                strategy += "处理所有女演员，";
            }

            strategy += $"缺失信息超过{MissingThreshold:P0}时触发补全，";

            if (UseImageCompletion)
            {
                strategy += "优先使用图片补全（更准确），";
            }
            else
            {
                strategy += "仅使用名字补全（更快），";
            }

            if (RequireConfirmation)
            {
                strategy += "补全前需要用户确认";
            }
            else
            {
                strategy += "自动补全";
            }

            return strategy;
        }
    }
}
