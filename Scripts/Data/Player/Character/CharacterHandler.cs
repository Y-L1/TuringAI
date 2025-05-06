using System;
using System.Collections.Generic;
using DragonLi.Core;
using DragonLi.Network;
using Game;
using Newtonsoft.Json;

namespace Data
{
    public class CharacterHandler : SandboxHandlerBase, IMessageReceiver
    {
        private const string kPlayerCoinKey = "player-coin";
        private const string kPlayerDiceKey = "player-dice";
        private const string kPlayerTokenKey = "player-token";
        private const string kCharacterIdKey = "character-id";
        private const string kChessboardIdKey = "chessboard-id";
        private const string kChessboardsKey = "chessboards";
        private const string kCharactersKey = "characters";
        private const string kItemsKey = "items";
        private const string kBlueprintsKey = "blueprints";

        #region Properties - Event

        public event Action<int?, int> PlayerCoinChanged;
        public event Action<int?, int> PlayerDiceChanged;
        public event Action<int?, int> PlayerTokenChanged;
        
        public event Action<int?, int> CharacterIdChanged;
        public event Action<int?, int> ChessboardIdChanged;
        public event Action<List<int>, List<int>> ChessboardsChanged;
        public event Action<List<int>, List<int>> CharactersChanged;
        public event Action<Dictionary<string, int>, Dictionary<string, int>> ItemsChanged;
        public event Action<List<int>, List<int>> BlueprintsChanged; 

        #endregion
        
        #region Properties - Currency - Data

        /// <summary>
        /// 玩家账户金币数量
        /// </summary>
        public int Coin
        {
            get => SandboxValue.GetValue<int>(kPlayerCoinKey);
            set => SandboxValue.SetValue(kPlayerCoinKey, value < 0 ? 0 : value);
        }
        
        /// <summary>
        /// 玩家账户骰子数量
        /// </summary>
        public int Dice
        {
            get => SandboxValue.GetValue<int>(kPlayerDiceKey);
            set => SandboxValue.SetValue(kPlayerDiceKey, value < 0 ? 0 : value);
        }
        
        /// <summary>
        /// 玩家账户代币数量
        /// </summary>
        public int Token
        {
            get => SandboxValue.GetValue<int>(kPlayerTokenKey);
            set => SandboxValue.SetValue(kPlayerTokenKey, value < 0 ? 0 : value);
        }

        #endregion

        #region Properties - Character - Data

        /// <summary>
        /// 玩家角色模型id
        /// </summary>
        public int CharacterId
        {
            get => SandboxValue.GetValue<int>(kCharacterIdKey);
            set => SandboxValue.SetValue(kCharacterIdKey, value);
        }
        
        /// <summary>
        /// 玩家当前处于棋盘的id
        /// </summary>
        public int ChessboardId
        {
            get => SandboxValue.GetValue<int>(kChessboardIdKey);
            set => SandboxValue.SetValue(kChessboardIdKey, value);
        }
        
        /// <summary>
        /// 玩家已经拥有的所有棋盘id
        /// </summary>
        public List<int> Chessboards
        {
            get => SandboxValue.GetValue<List<int>>(kChessboardsKey);
            set => SandboxValue.SetValue(kChessboardsKey, value);
        }
        
        /// <summary>
        /// 玩家已经拥有所有角色模型的id
        /// </summary>
        public List<int> Characters
        {
            get => SandboxValue.GetValue<List<int>>(kCharactersKey);
            set => SandboxValue.SetValue(kCharactersKey, value);
        }
        
        /// <summary>
        /// 玩家的物品数据
        /// </summary>
        public Dictionary<string, int> Items
        {
            get => SandboxValue.GetValue<Dictionary<string, int>>(kItemsKey);
            set => SandboxValue.SetValue(kItemsKey, value);
        }

        /// <summary>
        /// 玩家拥有的蓝图数据
        /// </summary>
        public List<int> Blueprints
        {
            get => SandboxValue.GetValue<List<int>>(kBlueprintsKey);
            set => SandboxValue.SetValue(kBlueprintsKey, value);
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
            sandboxCallbacks[kPlayerCoinKey] = (preValue, nowValue) => PlayerCoinChanged?.Invoke((int?)preValue, (int)nowValue);
            sandboxCallbacks[kPlayerDiceKey] = (preValue, nowValue) => PlayerDiceChanged?.Invoke((int?)preValue, (int)nowValue);
            sandboxCallbacks[kPlayerTokenKey] = (preValue, nowValue) => PlayerTokenChanged?.Invoke((int?)preValue, (int)nowValue);
            sandboxCallbacks[kCharacterIdKey] = (preValue, nowValue) => CharacterIdChanged?.Invoke((int?)preValue, (int)nowValue);
            sandboxCallbacks[kChessboardIdKey] = (preValue, nowValue) => ChessboardIdChanged?.Invoke((int?)preValue, (int)nowValue);
            sandboxCallbacks[kChessboardsKey] = (preValue, nowValue) => ChessboardsChanged?.Invoke((List<int>)preValue, (List<int>)nowValue);
            sandboxCallbacks[kCharactersKey] = (preValue, nowValue) => CharactersChanged?.Invoke((List<int>)preValue, (List<int>)nowValue);
            sandboxCallbacks[kItemsKey] = (preValue, nowValue) => ItemsChanged?.Invoke((Dictionary<string, int>)preValue, (Dictionary<string, int>)nowValue);
            sandboxCallbacks[kBlueprintsKey] = (preValue, nowValue) => BlueprintsChanged?.Invoke((List<int>)preValue, (List<int>)nowValue);
        }

        protected override void OnInit()
        {
            base.OnInit();
            ResetData();
            GameSessionAPI.CharacterAPI.QueryCharacter();
            GameSessionAPI.CharacterAPI.QueryCurrency();
        }

        #endregion
        
        #region Function - IMessageReceiver

        public void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            if(service != GameSessionAPI.CharacterAPI.ServiceName) return;
            if (method == GSCharacterAPI.MethodQueryCurrency)
            {
                Coin = response.GetAttachmentAsInt("coin");
                Dice = response.GetAttachmentAsInt("dice");
                Token = response.GetAttachmentAsInt("token");
            }
            else if (method == GSCharacterAPI.MethodQueryCharacter)
            {
                var characterJson = response.GetAttachmentAsString("character");
                var character = JsonConvert.DeserializeObject<FCharacter>(characterJson);

                CharacterId = character.characterId;
                ChessboardId = character.chessboardId;
                Chessboards = character.chessboards;
                Characters = character.characters;
                Items = character.items;
                Blueprints = character.blueprints;
            }
        }

        #endregion

        #region Function - API

        public void ResetData()
        {
            Coin = 0;
            Dice = 0;
            Token = 0;
            CharacterId = 0;
            ChessboardId = -1;
        }

        #endregion
    }
}