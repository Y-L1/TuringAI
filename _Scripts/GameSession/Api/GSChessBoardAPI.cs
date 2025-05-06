using System;
using System.Collections.Generic;
using DragonLi.Core;
using DragonLi.Network;

namespace Game
{
    public class GSChessBoardAPI : GameSessionAPIImpl
    {
        public static readonly string MethodQuery = "query";
        public static readonly string MethodArrive = "arrive";
        public static readonly string MethodQueryBank = "query_bank";
        public static readonly string MethodWithdrawBank = "withdraw_bank";

        /// <summary>
        /// 玩家移动，调用这个函数会向服务器发送移动请求，服务端接受到请求后，会返回相应消息
        /// 必然会返回的消息：移动结果
        /// 可能会返回的消息：经过格子结果
        /// </summary>
        public void Move(int multiplier = 1)
        {
            var request = CreateRequest("move");
            request.AddBodyParams("mul", multiplier);
    
            SendMessage(request);
        }

        public void MoveDev(int diceA, int diceB, int multiplier = 1)
        {
            var request = CreateRequest("move-debug");
            request.AddBodyParams("mul", multiplier);
            request.AddBodyParams("a", diceA);
            request.AddBodyParams("b", diceB);
            SendMessage(request);
        }

        /// <summary>
        /// 停留一回合
        /// </summary>
        public void Stay(int multiplier = 1)
        {
            var request = CreateRequest("stay");
            request.AddBodyParams("mul", multiplier);
            SendMessage(request);
        }
        
        /// <summary>
        /// 在调用Move以后可以调用Arrive，此时会触发到达格子效果，服务端接受到请求后，会返回相应消息
        /// 一次Move对应一次Arrive，否则无效
        /// 一定会返回的消息：到达格子结果
        /// </summary>
        public void Arrive()
        {
            SendMessage(CreateRequest(MethodArrive));
        }

        /// <summary>
        /// 对目前站立的格子实施Option操作
        /// </summary>
        /// <param name="onCreateBody">可在此填写需要的参数列表</param>
        public void Option(Action<HttpRequestProtocol> onCreateBody)
        {
            var request = CreateRequest("option");
            onCreateBody?.Invoke(request);
            SendMessage(request);
        }

        /// <summary>
        /// 查询当前棋盘信息
        /// </summary>
        public void Query()
        {
            SendMessage(CreateRequest(MethodQuery));
        }
        
        /// <summary>
        /// 查询银行数据
        /// inviter: 绑定的邀请人
        /// invest: 拥有的银行资金
        /// bank: FBankData 邀请过得有效的玩家id，可以通过Https请求查询这些玩家的基本信息，名字等
        ///
        /// Https请求查询这些玩家的基本信息url: user/queryUsers，传入参数为字符串users: string.
        /// 参数为查询的玩家列表，每个玩家用','隔开，如"sandy,tommy,paul"
        /// </summary>
        public void QueryBank()
        {
            SendMessage(CreateRequest(MethodQueryBank));
        }
        
        /// <summary>
        /// 提取银行资金
        /// 返回coin：所提取的数量
        /// 或者相关的错误信息
        /// </summary>
        public void WithdrawBank()
        {
            SendMessage(CreateRequest(MethodWithdrawBank));
        }

        /// <summary>
        /// 拉取游戏数据
        /// </summary>
        /// <param name="finish"></param>
        public void QueryGameData(Action<HttpResponseProtocol> finish)
        {
            var connection = Settings.GetConfiguration().GetConnectionConfiguration();
            var timeNow = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            new HttpRequest<HttpResponseProtocol>()
                .SetMethod(EHttpRequestMethod.Get)
                .SetHeader("secret", TextCryptoUtils.GenerateDynamicPassword_Sha256(timeNow))
                .SetUrl(connection.httpServer + "data-supply/query-game-data")
                .AddCallback(finish)
                .SendRequestAsync();
        }
        
        public void QueryGameRanks(Action<HttpResponseProtocol> finish)
        {
            var connection = Settings.GetConfiguration().GetConnectionConfiguration();
            var timeNow = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            new HttpRequest<HttpResponseProtocol>()
                .SetMethod(EHttpRequestMethod.Get)
                .SetHeader("secret", TextCryptoUtils.GenerateDynamicPassword_Sha256(timeNow))
                .SetUrl(connection.httpServer + "data-supply/query-ranks")
                .AddCallback(finish)
                .SendRequestAsync();
        }
        
        
        protected override string GetServiceName()
        {
            return "chess";
        }
    }
    
    [Serializable]
    public struct FChessBoardData
    {
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnassignedField.Global
        public Dictionary<int, string> items;
        
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnassignedField.Global
        public Dictionary<int, FChessBoardLandTile> lands;
    }

    [Serializable]
    public struct FChessBoardLandTile
    {
        public int level;
        public int startTs;
        public int finishTs;
        public bool locked;
    }

    [Serializable]
    public struct FChessBoardBankLogs
    {
        public List<FChessBoardBankLog> logs;
    }
        
    [Serializable]
    public struct FChessBoardBankLog
    {
        public int time;
        public int coin;
        public string user;
        public string name;
        public int avt;
    }
    
    [Serializable]
    public struct FLandGrade
    {
        public int level;
        public float priceMul;
        public float rewardMul;
    }

    [Serializable]
    public struct FLand
    {
        public int level;
        public float standMul;
        public float upgradeMulCoin;
        public float upgradeMulToken;
        public int upgradeDuration;
        public int requireLevel;
    }

    [Serializable]
    public struct FLandPrice
    {
        public int level;
        public int required;
        public int price;
        public int rowId;
    }
    
    [Serializable]
    public struct FShort
    {
        public int index;
        
        /// <summary>
        /// 抢夺倍率
        /// </summary>
        public float mul;
        
        /// <summary>
        /// 出现概率（这个值在客户端无作用）
        /// </summary>
        public float chance;

        public int rowId;
    }

    [Serializable]
    public struct FChance
    {
        public int index;
        public float coinMul;
        public int dice;
        public float chance;
        public bool special;
        public int rowId;
    }

    [Serializable]
    public struct FChessBoard
    {
        public int id;
        public float coinMul;
        public int dice;
        public float price;
        public int rowId;
    }
        
    [Serializable]
    public struct FBankData
    {
        private List<FChessBoardBankLog> logs;
        private List<string> invites;

        public IReadOnlyList<FChessBoardBankLog> GetLogs()
        {
            return logs == null ? new List<FChessBoardBankLog>() : logs.AsReadOnly();
        }
        
        public IReadOnlyList<string> GetInvites()
        {
            return invites == null ? new List<string>() : invites.AsReadOnly();
        }
    }
}