using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Data
{
    public static class ObjectiveType
    {
        /// <summary>
        /// 获取任务进度
        /// </summary>
        /// <param name="objective">任务列表</param>
        /// <param name="id">任务 id</param>
        /// <returns>进度</returns>
        public static int GetProgressById(this FObjectiveDaily objective, string id)
        {
            return objective.progress.GetValueOrDefault(id, 0);
        }

        /// <summary>
        /// 任务是否完成
        /// </summary>
        /// <param name="objective">任务列表</param>
        /// <param name="id">任务 id</param>
        /// <returns>完成？</returns>
        public static bool IsCompletedById(this FObjectiveDaily objective, string id)
        {
            return GetProgressById(objective, id) >= MissionInstance.Instance.Settings.GetMissionDaily(id).maxProgress;
        }

        /// <summary>
        /// 任务奖励是否被领取
        /// </summary>
        /// <param name="objective">任务列表</param>
        /// <param name="id">任务 id</param>
        /// <returns>领取？</returns>
        public static bool IsCollectedById(this FObjectiveDaily objective, string id)
        {
            return objective.rewarded.Contains(id);
        }

        /// <summary>
        /// 领取每日任务奖励
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="id"></param>
        public static void CompletedById(this FObjectiveDaily objective, string id)
        {
            // 修改本地领取数据
            objective.rewarded = new List<string>(objective.rewarded);
            objective.rewarded.Add(id);
            PlayerSandbox.Instance.ObjectiveHandler.Daily = objective;

            // 修改本地分数
            var weekly = PlayerSandbox.Instance.ObjectiveHandler.Weekly;
            weekly.score += MissionInstance.Instance.Settings.GetMissionDaily(id).score;
            PlayerSandbox.Instance.ObjectiveHandler.Weekly = weekly;
        }

        /// <summary>
        /// 本地数据修改 - 更新每日任务进度
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="id">任务id</param>
        /// <param name="progress">进度</param>
        public static void AddProgressDailyById(this FObjectiveDaily objective, string id, int progress)
        {
            var maxProgress = MissionInstance.Instance.Settings.GetMissionDaily(id).maxProgress;
            var curProgress = objective.progress.GetValueOrDefault(id, 0);
            if(curProgress >= maxProgress) return;
            
            objective.progress = new Dictionary<string, int>(objective.progress);
            if (!objective.progress.TryAdd(id, progress))
            {
                objective.progress[id] = Math.Clamp(objective.progress[id] + progress, 0, maxProgress);
            }
            
            PlayerSandbox.Instance.ObjectiveHandler.Daily = objective;
        }

        /// <summary>
        /// id 周任务领取是否解锁
        /// </summary>
        /// <param name="objective"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsUnlockById(this FObjectiveWeekly objective, int id)
        {
            return id switch
            {
                1 => objective.score >= 20,
                2 => objective.score >= 40,
                3 => objective.score >= MissionInstance.Instance.Settings.GetMaxScore(),
                _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
            };
        }
    }
}