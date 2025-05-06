using System;
using System.Collections.Generic;
using DragonLi.Core;
using DragonLi.Network;
using Game;
using Newtonsoft.Json;

namespace Data
{
    public class ChessBoardHandler : SandboxHandlerBase, IMessageReceiver
    {
        private const string kPlayerInvestKey = "player-invest";
        private const string kPlayerInviteeKey = "player-invitee";
        private const string kPlayerInvitersKey = "player-inviters";
        private const string kPlayerStandKey = "player-stand-index";
        private const string kPlayerChessBordTileKey = "player-chess-bord-tile";
        
        #region Properties - Default - Data

        public List<FLandGrade> LandGrade { get; private set; }

        public List<FBuildArea> BuildAreas { get; private set; }
        
        public List<FCharacterShopInfo> CharactersShopInfos { get; private set; }
        
        public List<FChance> Chances { get; private set; }
        
        public List<FLandPrice> LandPrices { get; private set; }
        
        public List<FBuildSlot> BuildSlots { get; private set; }
        
        public List<FLand> Lands { get; private set; }
        
        public List<FShort> Shorts { get; private set; }
        
        public List<FChessBoard> ChessBoards { get; private set; }
        
        public List<FBuildSlotRate> BuildSlotRates { get; private set; }

        #endregion

        #region Properties - Data

        /// <summary>
        /// 银行资金
        /// </summary>
        public long InvestCoin
        {
            get => SandboxValue.GetValue<long>(kPlayerInvestKey);
            set => SandboxValue.SetValue(kPlayerInvestKey, value);
        }

        /// <summary>
        /// 被邀请者
        /// </summary>
        public string Invitee
        {
            get => SandboxValue.GetValue<string>(kPlayerInviteeKey);
            set => SandboxValue.SetValue(kPlayerInviteeKey, value);
        }

        /// <summary>
        /// 已经邀请成功的人
        /// </summary>
        public FBankData Inviters
        {
            get => SandboxValue.GetValue<FBankData>(kPlayerInvitersKey);
            set => SandboxValue.SetValue(kPlayerInvitersKey, value);
        }
        
        /// <summary>
        /// 当前站立的位置
        /// </summary>
        public int StandIndex
        {
            get => SandboxValue.GetValue<int>(kPlayerStandKey);
            set => SandboxValue.SetValue(kPlayerStandKey, value);
        }
        
        /// <summary>
        /// 棋盘格子数据
        /// items - 物品
        /// lands - 地块
        /// </summary>
        public FChessBoardData ChessBoardData
        {
            get => SandboxValue.GetValue<FChessBoardData>(kPlayerChessBordTileKey);
            set => SandboxValue.SetValue(kPlayerChessBordTileKey, value);
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
            
            // TODO: 监听 sandbox 里面值的改变
            // ...
            
        }

        protected override void OnInit()
        {
            base.OnInit();
            GameSessionAPI.ChessBoardAPI.Query();
            GameSessionAPI.ChessBoardAPI.QueryBank();
            GameSessionAPI.ChessBoardAPI.QueryGameData(response =>
            {
                if (!response.IsSuccess())
                {
                    this.LogErrorEditorOnly($"Failed to get chess board response: {response.error}");
                    return;
                }
                var landGradeJson = response.GetAttachmentAsString("land-grade");
                var buildAreaJson = response.GetAttachmentAsString("build-area");
                var characterJson = response.GetAttachmentAsString("character");
                var chanceJson = response.GetAttachmentAsString("chance");
                var landPriceJson = response.GetAttachmentAsString("land-price");
                var buildSlotJson = response.GetAttachmentAsString("build-slot");
                var landJson = response.GetAttachmentAsString("land");
                var shortJson = response.GetAttachmentAsString("short");
                var chessboardJson = response.GetAttachmentAsString("chessboard");
                var buildSlotRateJson = response.GetAttachmentAsString("build-slot-rate");
                    
                LandGrade = JsonConvert.DeserializeObject<List<FLandGrade>>(landGradeJson);
                BuildAreas = JsonConvert.DeserializeObject<List<FBuildArea>>(buildAreaJson);
                CharactersShopInfos = JsonConvert.DeserializeObject<List<FCharacterShopInfo>>(characterJson);
                Chances = JsonConvert.DeserializeObject<List<FChance>>(chanceJson);
                LandPrices = JsonConvert.DeserializeObject<List<FLandPrice>>(landPriceJson);
                BuildSlots = JsonConvert.DeserializeObject<List<FBuildSlot>>(buildSlotJson);
                Lands = JsonConvert.DeserializeObject<List<FLand>>(landJson);
                Shorts = JsonConvert.DeserializeObject<List<FShort>>(shortJson);
                ChessBoards = JsonConvert.DeserializeObject<List<FChessBoard>>(chessboardJson);
                BuildSlotRates = JsonConvert.DeserializeObject<List<FBuildSlotRate>>(buildSlotRateJson);
            });
        }

        #endregion

        #region Function - IMessageReceiver

        public void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            if (service != GameSessionAPI.ChessBoardAPI.ServiceName) return;
            if (method == GSChessBoardAPI.MethodQuery)
            {
                StandIndex = response.GetAttachmentAsInt("stand");
                ChessBoardData = JsonConvert.DeserializeObject<FChessBoardData>(response.GetAttachmentAsString("data"));
            }
            else if (method == GSChessBoardAPI.MethodQueryBank)
            {
                InvestCoin = response.GetAttachmentAsLong("invest");
                Invitee = response.GetAttachmentAsString("inviter");
                var bankJson = response.GetAttachmentAsString("bank"); 
                Inviters = JsonConvert.DeserializeObject<FBankData>(bankJson);
            }
        }

        #endregion

    }

}