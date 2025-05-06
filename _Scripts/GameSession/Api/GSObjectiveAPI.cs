using System;
using System.Collections.Generic;

namespace Game
{
    public class GSObjectiveAPI : GameSessionAPIImpl
    {
        public static readonly string MethodQuery = "query";
        public static readonly string MethodRewardDaily = "reward_daily";
        public static readonly string MethodRewardWeekly = "reward_weekly";
        
        /// <summary>
        /// 仅用于开发调试
        /// </summary>
        public static readonly string MethodComplete = "complete";
        
        /// <summary>
        /// 仅用于开发调试
        /// </summary>
        public static readonly string MethodCompleteWeek = "complete_week";
        
        /// <summary>
        /// 查询任务详情，必须先调用这个方法
        /// day-exp: 每日任务结束倒计时
        /// week-exp: 每周任务刷新时间
        /// weekly: 每周任务数据
        /// daily: 每日任务数据
        /// </summary>
        public void Query()
        {
            SendMessage(CreateRequest(MethodQuery));
        }

        /// <summary>
        /// 增加每日任务进度，仅用于开发调试
        /// </summary>
        public void CompleteDay(string id, int count)
        {
            var request = CreateRequest(MethodComplete);
            request.AddBodyParams("objective", id);
            request.AddBodyParams("count", count);
            SendMessage(request);
        }
        
        /// <summary>
        /// 增加每周任务进度，仅用于开发调试
        /// </summary>
        public void CompleteWeek(int score)
        { 
            var request = CreateRequest(MethodCompleteWeek);
            request.AddBodyParams("score", score);
            SendMessage(request);
        }
        
        /// <summary>
        /// 领取每日奖励
        /// </summary>
        /// <param name="id"></param>
        public void RewardDaily(string id)
        {
            var request = CreateRequest(MethodRewardDaily);
            request.AddBodyParams("objective", id);
            SendMessage(request);
        }
        
        /// <summary>
        /// 领取每周进度奖励
        /// </summary>
        /// <param name="rank"></param>
        public void RewardWeekly(string rank)
        {
            var request = CreateRequest(MethodRewardWeekly);
            request.AddBodyParams("rank", rank);
            SendMessage(request);
        }
        
        protected override string GetServiceName()
        {
            return "objective";
        }
    }

    [Serializable]
    public struct FObjectiveDaily
    {
        public Dictionary<string, int> progress;
        public List<string> rewarded;
    }
    
    [Serializable]
    public struct FObjectiveWeekly
    {
        /// <summary>
        /// 0 - 70 （20 - 40 - 70）
        /// </summary>
        public int score;
        
        /// <summary>
        /// 1，2，3
        /// </summary>
        public List<int> rewarded;
    }
}