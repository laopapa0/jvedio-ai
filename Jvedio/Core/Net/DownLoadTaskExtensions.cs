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
    /// DownLoadTask 扩展类 - 性别识别功能
    /// </summary>
    public static class DownLoadTaskExtensions
    {
        /// <summary>
        /// 删除男性演员
        /// </summary>
        public static void DeleteMaleActor(long actorID, long videoDataID, string actorName = "")
        {
            try
            {
                // 删除关联表记录
                string sql = $"delete from metadata_to_actor where ActorID='{actorID}' and DataID='{videoDataID}'";
                MapperManager.metaDataMapper.ExecuteNonQuery(sql);

                // 检查该演员是否还关联到其他视频
                var actorInOtherVideos = MapperManager.metaDataMapper.ExecuteQuery(
                    $"SELECT COUNT(*) FROM metadata_to_actor WHERE ActorID='{actorID}'"
                );

                // 如果该演员只关联到当前视频，删除演员记录
                if (actorInOtherVideos != null && actorInOtherVideos.Rows.Count > 0)
                {
                    var count = Convert.ToInt32(actorInOtherVideos.Rows[0][0]);
                    if (count == 0)
                    {
                        MapperManager.actorMapper.DeleteById(actorID);
                        Logger.Info($"已删除男性演员记录: {actorName} (ActorID: {actorID})");
                    }
                    else
                    {
                        Logger.Info($"已解除关联: {actorName} (ActorID: {actorID}), 演员仍存在于其他影片");
                    }
                }

                // 记录删除日志
                if (ConfigManager.GenderRecognitionConfig.LogDeletedActors)
                {
                    LogDeletedActor(actorName, actorID, videoDataID);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"删除男性演员失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 记录删除日志
        /// </summary>
        private static void LogDeletedActor(string actorName, long actorID, long videoDataID)
        {
            try
            {
                var logFilePath = ConfigManager.GenderRecognitionConfig.LogFilePath;
                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " +
                               $"删除男性演员: {actorName}, " +
                               $"ActorID: {actorID}, " +
                               $"VideoDataID: {videoDataID}";

                File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Logger.Error($"记录删除日志失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 处理演员性别识别和删除
        /// </summary>
        public static async Task ProcessActorGenderRecognitionAsync(
            ActorInfo actorInfo,
            long videoDataID,
            string imagePath = null)
        {
            try
            {
                // 检查配置
                if (!ConfigManager.GenderRecognitionConfig.AutoDeleteMaleActors)
                {
                    return;
                }

                if (!ConfigManager.GenderRecognitionConfig.UseLLMRecognition)
                {
                    return;
                }

                // 检查千问API配置
                if (!DashScopeClient.IsConfigured())
                {
                    Logger.Warn("千问API未配置，跳过性别识别");
                    return;
                }

                // 检查白名单
                if (ConfigManager.GenderRecognitionConfig.IsInWhitelist(actorInfo.ActorName))
                {
                    Logger.Info($"演员在白名单中，跳过: {actorInfo.ActorName}");
                    return;
                }

                // 识别性别
                var result = await GenderRecognitionService.RecognizeAsync(
                    imagePath,
                    actorInfo.ActorName
                );

                if (!result.Success)
                {
                    Logger.Warn($"性别识别失败: {actorInfo.ActorName}, 原因: {result.Reason}");
                    return;
                }

                // 更新演员性别
                actorInfo.Gender = result.Gender;
                MapperManager.actorMapper.Update(actorInfo);

                Logger.Info($"性别识别完成: {actorInfo.ActorName} -> {result.Gender} " +
                           $"(置信度: {result.Confidence:F2}, 理由: {result.Reason})");

                // 判断是否删除
                if (ConfigManager.GenderRecognitionConfig.ShouldDelete(
                    result.Gender, result.Confidence, actorInfo.ActorName))
                {
                    // 启用复检机制
                    if (ConfigManager.GenderRecognitionConfig.EnableDoubleCheck)
                    {
                        Logger.Info($"启用复检机制: {actorInfo.ActorName}");
                        var doubleCheckResult = await GenderRecognitionService.RecognizeAsync(
                            imagePath,
                            actorInfo.ActorName
                        );

                        if (doubleCheckResult.Success && 
                            doubleCheckResult.Gender == Gender.Boy)
                        {
                            DeleteMaleActor(actorInfo.ActorID, videoDataID, actorInfo.ActorName);
                        }
                        else
                        {
                            Logger.Warn($"复检不一致，取消删除: {actorInfo.ActorName}");
                        }
                    }
                    else
                    {
                        DeleteMaleActor(actorInfo.ActorID, videoDataID, actorInfo.ActorName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"处理演员性别识别异常: {ex.Message}");
            }
        }
    }
}
