using Jvedio.Core.AI;
using Jvedio.Core.Config;
using Jvedio.Core.Enums;
using Jvedio.Core.Logs;
using Jvedio.Entity;
using SuperUtils.IO;
using System;
using System.IO;
using System.Threading.Tasks;
using static Jvedio.App;

namespace Jvedio.Core.Net
{
    /// <summary>
    /// DownLoadTask 扩展类 - 演员信息补全功能
    /// </summary>
    public static class DownLoadTaskActorCompletionExtensions
    {
        /// <summary>
        /// 处理演员信息补全
        /// </summary>
        public static async Task ProcessActorInfoCompletionAsync(
            ActorInfo actorInfo,
            int actorIndex,
            string imagePath = null)
        {
            try
            {
                // 检查配置
                if (!ConfigManager.ActorInfoCompletionConfig.EnableAutoCompletion)
                {
                    return;
                }

                // 检查千问API配置
                if (!DashScopeClient.IsConfigured())
                {
                    Logger.Warn("千问API未配置，跳过信息补全");
                    return;
                }

                // 计算缺失信息比例
                var missingRatio = CalculateMissingRatio(actorInfo);

                // 检查是否应该补全
                var isFemale = actorInfo.Gender == Gender.Girl || actorInfo.Gender == Gender.Unknown;
                var shouldComplete = ConfigManager.ActorInfoCompletionConfig.ShouldComplete(
                    actorIndex,
                    isFemale,
                    missingRatio
                );

                if (!shouldComplete)
                {
                    return;
                }

                Logger.Info($"开始补全演员信息: {actorInfo.ActorName} (第{actorIndex + 1}名)");

                // 根据配置选择补全方式
                ActorCompletionResult result;

                if (ConfigManager.ActorInfoCompletionConfig.UseImageCompletion &&
                    !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    // 使用图片补全
                    result = await ActorInfoCompletionService.CompleteByImageAsync(
                        actorInfo,
                        imagePath
                    );
                }
                else
                {
                    // 使用名字补全
                    result = await ActorInfoCompletionService.CompleteByNameAsync(actorInfo);
                }

                if (result.Success)
                {
                    // 更新数据库
                    MapperManager.actorMapper.Update(actorInfo);

                    Logger.Info($"✅ 演员信息补全成功: {actorInfo.ActorName}, " +
                               $"置信度: {result.Confidence:P0}");

                    // 记录日志
                    if (ConfigManager.ActorInfoCompletionConfig.LogCompletion)
                    {
                        LogCompletion(actorInfo, result, actorIndex);
                    }
                }
                else
                {
                    Logger.Warn($"⚠️ 演员信息补全失败: {actorInfo.ActorName}, " +
                               $"原因: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"处理演员信息补全异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 批量补全演员信息
        /// </summary>
        public static async Task BatchCompleteActorInfoAsync(
            ActorInfo[] actors,
            long videoDataID)
        {
            var config = ConfigManager.ActorInfoCompletionConfig;

            if (!config.EnableAutoCompletion)
            {
                return;
            }

            if (!DashScopeClient.IsConfigured())
            {
                Logger.Warn("千问API未配置，跳过批量补全");
                return;
            }

            Logger.Info($"开始批量补全演员信息，共 {actors.Length} 名演员");

            var successCount = 0;
            var failCount = 0;

            for (int i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                var imagePath = actor.GetImagePathFromVideoDataID(videoDataID);

                try
                {
                    // 计算缺失比例
                    var missingRatio = CalculateMissingRatio(actor);

                    // 检查是否应该补全
                    var isFemale = actor.Gender == Gender.Girl || actor.Gender == Gender.Unknown;
                    var shouldComplete = config.ShouldComplete(i, isFemale, missingRatio);

                    if (!shouldComplete)
                    {
                        Logger.Info($"跳过: {actor.ActorName} (不需要补全)");
                        continue;
                    }

                    // 执行补全
                    ActorCompletionResult result;

                    if (config.UseImageCompletion &&
                        !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                    {
                        result = await ActorInfoCompletionService.CompleteByImageAsync(
                            actor,
                            imagePath
                        );
                    }
                    else
                    {
                        result = await ActorInfoCompletionService.CompleteByNameAsync(actor);
                    }

                    if (result.Success)
                    {
                        MapperManager.actorMapper.Update(actor);
                        successCount++;
                        Logger.Info($"✅ [{i + 1}/{actors.Length}] {actor.ActorName} 补全成功");
                    }
                    else
                    {
                        failCount++;
                        Logger.Warn($"⚠️ [{i + 1}/{actors.Length}] {actor.ActorName} 补全失败: {result.Message}");
                    }

                    // 限流，避免API调用过快
                    if (i < actors.Length - 1)
                    {
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    failCount++;
                    Logger.Error($"❌ [{i + 1}/{actors.Length}] {actor.ActorName} 补全异常: {ex.Message}");
                }
            }

            Logger.Info($"批量补全完成：成功 {successCount}，失败 {failCount}");
        }

        #region 私有方法

        /// <summary>
        /// 计算缺失信息比例
        /// </summary>
        private static float CalculateMissingRatio(ActorInfo actorInfo)
        {
            var totalFields = 11;
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

            return (float)missingFields / totalFields;
        }

        /// <summary>
        /// 记录补全日志
        /// </summary>
        private static void LogCompletion(
            ActorInfo actorInfo,
            ActorCompletionResult result,
            int actorIndex)
        {
            try
            {
                var logFilePath = ConfigManager.ActorInfoCompletionConfig.LogFilePath;

                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                               $"补全演员: {actorInfo.ActorName} (第{actorIndex + 1}名)\n" +
                               $"  置信度: {result.Confidence:P0}\n" +
                               $"  信息: 生日={actorInfo.Birthday ?? "未知"}, " +
                               $"身高={actorInfo.Height}cm, " +
                               $"三围={actorInfo.Chest}/{actorInfo.Waist}/{actorInfo.Hipline}, " +
                               $"血型={actorInfo.BloodType ?? "未知"}, " +
                               $"出生地={actorInfo.BirthPlace ?? "未知"}\n" +
                               $"  理由: {result.Message}\n";

                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Logger.Error($"记录补全日志失败: {ex.Message}");
            }
        }

        #endregion
    }
}
