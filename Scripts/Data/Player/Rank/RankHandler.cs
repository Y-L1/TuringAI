using System;
using System.Collections.Generic;
using Data.Type;
using DragonLi.Core;
using Game;
using Newtonsoft.Json;

namespace Data
{
    public class RankHandler : SandboxHandlerBase
    {
        private const string GameRankKey = "game-rank";

        #region Properties - Event

        public event Action<RankHandlerType.FRanks, RankHandlerType.FRanks> OnRanksChanged; 

        #endregion
        
        #region Properties - Data

        public RankHandlerType.FRanks Ranks
        {
            get => SandboxValue.GetValue<RankHandlerType.FRanks>(GameRankKey);
            set => SandboxValue.SetValue(GameRankKey, value);
        }

        #endregion
        
        #region SandboxHandlerBase

        protected override void OnInitSandboxCallbacks(Dictionary<string, Action<object, object>> sandboxCallbacks)
        {
            base.OnInitSandboxCallbacks(sandboxCallbacks);
            if (sandboxCallbacks == null)
            {
                throw new ArgumentNullException(nameof(sandboxCallbacks));
            }

            sandboxCallbacks[GameRankKey] = (preValue, newValue) => OnRanksChanged?.Invoke((RankHandlerType.FRanks)preValue, (RankHandlerType.FRanks)newValue);
        }

        protected override void OnInit()
        {
            base.OnInit();
            QueryRanks();
        }

        #endregion

        #region Function - Query Data

        private void QueryRanks()
        {
            GameSessionAPI.ChessBoardAPI.QueryGameRanks(response =>
            {
                if (!response.IsSuccess())
                {
                    this.LogErrorEditorOnly($"Failed to get ranks response: {response.error}");
                    return;
                }
                
                var ranksJson = response.GetAttachmentAsString("data");
                Ranks = JsonConvert.DeserializeObject<RankHandlerType.FRanks>(ranksJson);
            });
        }

        #endregion
    }
}