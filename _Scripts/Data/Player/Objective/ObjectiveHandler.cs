using System;
using System.Collections.Generic;
using _Scripts.Utils;
using DragonLi.Core;
using DragonLi.Network;
using Game;
using Newtonsoft.Json;

namespace Data
{
    public class ObjectiveHandler : SandboxHandlerBase, IMessageReceiver
    {
        private const string kDayExpKey = "day-exp";
        private const string kWeekExpKey = "week-exp";
        private const string kObjectiveDailyKey = "objective-daily";
        private const string kObjectiveWeeklyKey = "objective-weekly";

        #region Properties - Event

        public event Action<int?, int> DayExpChanged;
        public event Action<int?, int> WeekExpChanged;
        public event Action<FObjectiveDaily, FObjectiveDaily> ObjectiveDailyChanged;
        public event Action<FObjectiveWeekly, FObjectiveWeekly> ObjectiveWeeklyChanged;

        #endregion

        #region Properties

        private int QueryTimeStamp { get; set; }

        #endregion

        #region Properties - Data

        public int DayExp
        {
            get => SandboxValue.GetValue<int>(kDayExpKey);
            set => SandboxValue.SetValue(kDayExpKey, value);
        }

        public int WeekExp
        {
            get => SandboxValue.GetValue<int>(kWeekExpKey);
            set => SandboxValue.SetValue(kWeekExpKey, value);
        }

        public FObjectiveDaily Daily
        {
            get => SandboxValue.GetValue<FObjectiveDaily>(kObjectiveDailyKey);
            set => SandboxValue.SetValue(kObjectiveDailyKey, value);
        }

        public FObjectiveWeekly Weekly
        {
            get => SandboxValue.GetValue<FObjectiveWeekly>(kObjectiveWeeklyKey);
            set => SandboxValue.SetValue(kObjectiveWeeklyKey, value);
        }

        #endregion

        #region API

        /// <summary>
        /// 获取每日任务结束时间戳
        /// </summary>
        /// <returns></returns>
        public int GetDailyFinishTimeStamp()
        {
            return DayExp + QueryTimeStamp;
        }

        /// <summary>
        /// 获取每周任务结束时间戳
        /// </summary>
        /// <returns></returns>
        public int GetWeekFinishTimeStamp()
        {
            return WeekExp + QueryTimeStamp;
        }

        #endregion

        #region Function - SandboxHandlerBase

        protected override void OnInitSandboxCallbacks(Dictionary<string, Action<object, object>> sandboxCallbacks)
        {
            base.OnInitSandboxCallbacks(sandboxCallbacks);
            if (sandboxCallbacks == null)
            {
                throw new ArgumentNullException(nameof(sandboxCallbacks));
            }

            sandboxCallbacks[kDayExpKey] = (preValue, nowValue) => DayExpChanged?.Invoke((int?)preValue, (int)nowValue);
            sandboxCallbacks[kWeekExpKey] = (preValue, nowValue) => WeekExpChanged?.Invoke((int?)preValue, (int)nowValue);
            sandboxCallbacks[kObjectiveDailyKey] = (preValue, nowValue) => ObjectiveDailyChanged?.Invoke((FObjectiveDaily)preValue, (FObjectiveDaily)nowValue);
            sandboxCallbacks[kObjectiveWeeklyKey] = (preValue, nowValue) => ObjectiveWeeklyChanged?.Invoke((FObjectiveWeekly)preValue, (FObjectiveWeekly)nowValue);
        }

        protected override void OnInit()
        {
            base.OnInit();
            GameSessionAPI.ObjectiveAPI.Query();
        }

        #endregion

        #region Function - IMessageReceiver

        public void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            if (service == GameSessionAPI.ObjectiveAPI.ServiceName && method == GSObjectiveAPI.MethodQuery)
            {
                QueryTimeStamp = TimeAPI.GetUtcTimeStamp();
                DayExp = response.GetAttachmentAsInt("day-exp");
                WeekExp = response.GetAttachmentAsInt("week-exp");
                var dailyJson = response.GetAttachmentAsString("daily");
                var weeklyJson = response.GetAttachmentAsString("weekly");

                Daily = JsonConvert.DeserializeObject<FObjectiveDaily>(dailyJson);
                Weekly = JsonConvert.DeserializeObject<FObjectiveWeekly>(weeklyJson);
            }
        }

        #endregion
    }
}