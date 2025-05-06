using System;
using System.Collections.Generic;
using _Scripts.Utils;
using Data;
using DragonLi.Core;
using DragonLi.Frame;
using DragonLi.Network;
using DragonLi.UI;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Game
{
    public class ChessTileLand : ChessTile
    {
        #region Fields
        
        [Header("Settings - Level")]
        [SerializeField] private TextMeshProUGUI tmpLevel;
        [SerializeField] private Color[] levelColors;
        
        [Header("References")]
        [SerializeField] private Transform elementTransform;
        [SerializeField] private LevelComponent[] levelComponents;

        [Header("Effects")] 
        [SerializeField] private GameObject cashEffectObject;
        [SerializeField] private GameObject landBanObject;
        [SerializeField] private GameObject landUpgradeObject;


        #endregion

        #region Properties
        private string Item { get; set; }
        
        private bool bReceiveArriveMessage { get; set; } = false;

        public int Coin { get; set; }
        public int Need { get; set; }
        public int Finish { get; set; }
        public string Option { get; set; }

        private UIWSTimer Timer { get; set; }
        private UIWorldElement LockedWorldElement { get; set; }

        #endregion

        #region Properties - TilesData

        private FChessBoardLandTile TileData { get; set; } = new()
        {
            level = 1,
            finishTs = 0,
            locked = false,
            startTs = 0,
        };

        private int Level
        {
            get => TileData.level;
            set
            {
                var data = TileData;
                data.level = value;
                TileData = data;
            }
        }

        private bool Locked
        {
            get => TileData.locked;
            set
            {
                var data = TileData;
                data.locked = value;
                TileData = data;
            }
        }

        #endregion

        #region API
        
        public void InitializedData(FChessBoardLandTile data)
        {
            TileData = data;
            
            if (TimeAPI.GetUtcTimeStamp() < data.finishTs)
            {
                SetUpUpgradeTimer(TileData.finishTs - TimeAPI.GetUtcTimeStamp(), TileData.finishTs - TileData.startTs);
            }
            PerformLocked(data.locked);
            SetHousesLevel(Level);
            UpdateLevelText();
        }

        public void InitializedData(string item)
        {
            Item = item;
        }

        public void SetUpUpgradeTimer(int remainTime, int fullTime)
        {
            SetUpgradeEffectStatus(true);
            if (!Timer)
            {
                var layer = UIManager.Instance.GetLayer<UIWorldElementLayer>("UIWorldElementLayer");
                var element = layer.SpawnWorldElement<UIWorldElement>(EffectInstance.Instance.Settings.uiEffectLandUpgradeTimer, elementTransform.position);
                Timer = element.GetComponent<UIWSTimer>();
            }
            Timer.Initialize(remainTime, fullTime, () =>
            {
                Timer = null;
                FinishUpgrade();
            });
        }

        public FChessBoardLandTile GetTileData()
        {
            return TileData;
        }

        public void PerformLocked(bool locked)
        {
            SetBanEffectStatus(locked);
            var layer = UIManager.Instance.GetLayer<UIWorldElementLayer>("UIWorldElementLayer");

            if (locked)
            {
                if (LockedWorldElement) return;
                LockedWorldElement = layer.SpawnWorldElement<UIWorldElement>(EffectInstance.Instance.Settings.uiEffectLandLocked, GetStandPosition());
            }
            else
            {
                if (!LockedWorldElement) return;
                layer.RemoveElement(LockedWorldElement);
                LockedWorldElement = null;
            }
        }

        public void UpgradeLevelHouse()
        {
            SetHousesLevel(Level);
        }
        
        public void SetUpgradeEffectStatus(bool display)
        {
            landUpgradeObject.SetActive(display);
        }

        #endregion

        #region Function

        private void SetBanEffectStatus(bool display)
        {
            landBanObject.SetActive(display);
        }        

        #endregion

        #region Functions - Level

        private void InitHouses()
        {
            foreach (var t in levelComponents)
            {
                t.SetState(LevelComponent.EHouseLocationType.Disappear);
            }
        }

        private void UpdateLevelText()
        {
            if (!tmpLevel) return;
            tmpLevel.SetText(string.Format(this.GetLocalizedText("level-fmt"), Level));
        }

        private Color GetColorByLevel(int level)
        {
            var index = (level - 1) / 2;
            return levelColors[index];
        }

        private LevelComponent GetSlotByIndex(int index)
        {
            return levelComponents[index];
        }

        private void SetHousesLevel(int level)
        {
            InitHouses();
            var levelInfo = GetHouseRepresentation(level);
            foreach (var info in levelInfo)
            {
                var slot = GetSlotByIndex(info.Key);
                slot.SetColor(info.Value.color);
                slot.SetState(info.Value.half ? LevelComponent.EHouseLocationType.Part : LevelComponent.EHouseLocationType.All);
            }
        }
        
        private void FinishUpgrade()
        {
            PlayerSandbox.Instance.ObjectiveHandler.Daily.AddProgressDailyById("upgrade-land", 1);
            Level++;
            UpdateLevelText();
            UpgradeLevelHouse();
            SetUpgradeEffectStatus(false);
        }

        private Dictionary<int, (Color color, bool half)> GetHouseRepresentation(int level)
        {
            var result = new Dictionary<int, (Color color, bool half)>();
            var levelArray = TilesAPI.GetHouseLevels(level);
            for (var i = 0; i < 4; i++)
            {
                var slotLevel = levelArray[i];
                if (slotLevel <= 0) continue;
                var color = GetColorByLevel(slotLevel);
                result[i] = (color, slotLevel % 2 == 1);
            }

            return result;
        }
        
        #endregion

        #region ChessTile

        public override void Initialize(int tileIndex)
        {
            base.Initialize(tileIndex);
            InitHouses();
            InitializedData(TileData);
        }

        public override List<IQueueableEvent> OnArrive()
        {
            GameSessionAPI.ChessBoardAPI.Arrive();

            return new List<IQueueableEvent>
            {
                new WaitForTrueEvent(() => bReceiveArriveMessage),
                new CustomEvent(() => { bReceiveArriveMessage = false; }),
                new CustomEvent(() => { SoundAPI.PlaySound(AudioInstance.Instance.Settings.goodSmall); }),
                new GameObjectVisibilityEvent(cashEffectObject, 3f),
                new ModifyNumWSEffectEvent(transform.position, EffectInstance.Instance.Settings.uiEffectCoinNumber, () => Coin),
                new CustomEvent(() => { PlayerSandbox.Instance.CharacterHandler.Coin += Coin; }),
                new LandTileOptionEvent(this, Timer),
            };
        }

        #endregion
        
        #region Callbacks

        protected override void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            if (PlayerSandbox.Instance.ChessBoardHandler.StandIndex != TileIndex) return;
            if (!response.IsSuccess()) return;
            if(service != GameSessionAPI.ChessBoardAPI.ServiceName) return;
            if(response.GetAttachmentAsString("tile") != "land") return;
            OnReceiveArriveMessage(response, service, method);
            OnOptionMessage(response, service, method);
            
        }

        private void OnReceiveArriveMessage(HttpResponseProtocol response, string service, string method)
        {
            if (method != GSChessBoardAPI.MethodArrive) return;
            
            bReceiveArriveMessage = true;
            Coin = response.GetAttachmentAsInt("coin");
            Need = response.GetAttachmentAsInt("need");
            Finish = response.GetAttachmentAsInt("finish");
            Option = response.GetAttachmentAsString("option");
        }

        private void OnOptionMessage(HttpResponseProtocol response, string service, string method)
        {
            if(method != "option") return;
            var coinNeed = response.GetAttachmentAsInt("coin_need");
            PlayerSandbox.Instance.CharacterHandler.Coin -= coinNeed;
            // 解锁
            PerformLocked(false);
            EventQueue.Instance.Enqueue(new ModifyNumWSEffectEvent(transform.position, EffectInstance.Instance.Settings.uiEffectMinusCoinNumber, () => -Math.Abs(Coin)));
        }

        #endregion
    }

    internal class LandTileOptionEvent : ChessTileEvent
    {
        #region Properties

        private UIWSTimer Time { get; set; }

        private ChessTile Tile { get; set; }
        
        private Func<int> GetNeed { get; set; }
        private Func<int> GetFinish { get; set; }
        private Func<string> GetOption { get; set; }

        #endregion
        
        #region ChessTileEvent

        public LandTileOptionEvent(ChessTile tile, UIWSTimer upgradeTime ) : base(tile)
        {
            Tile = tile;
            GetNeed = () => ((ChessTileLand)tile).Need;
            GetFinish = () => ((ChessTileLand)tile).Finish;
            GetOption = () => ((ChessTileLand)tile).Option;
            Time = upgradeTime;
        }

        public override void OnExecute()
        {
            base.OnExecute();
            OptionLogic();
        }
        
        #endregion

        #region Functions

        private void OptionLogic()
        {
            var layer = UIManager.Instance.GetLayer<UIWorldElementLayer>("UIWorldElementLayer");
            var landTile = Tile as ChessTileLand;
            Debug.Assert(landTile);

            var tipLocation = landTile.GetStandPosition();
            switch (GetOption())
            {
                case "acc":
                    if (Time)
                    {
                        var remainTime = (int)Time.GetRemainingTime() - 600;
                        if(remainTime < 0) remainTime = 0;
                        Time.UpdateTime(remainTime);
                    }
                    break;
                case "upgrade":
                    UILandUpgradeLayer.ShowUILandUpgradeLayer(GetNeed(), finish =>
                    {
                        var remainTime = (int)finish - TimeAPI.GetUtcTimeStamp();
                        landTile.SetUpUpgradeTimer(remainTime, remainTime);
                    });

                    if (GameInstance.Instance.HostingHandler.Hosting)
                    {
                        UILandUpgradeLayer.GetLayer().OnConfirmClick(null);
                    }
                    break;
                case "need_coin":
                    layer.SpawnWorldElement<UIWorldElement>(EffectInstance.Instance.Settings.uiEffectLandNoCoin, tipLocation);
                    break;
                case "limit":
                    layer.SpawnWorldElement<UIWorldElement>(EffectInstance.Instance.Settings.uiEffectLandLvLimit, tipLocation);
                    break;
                case "locked":
                    landTile.PerformLocked(true);
                    break;
                case "unlock":
                    // TODO: 地块解锁确认
                    // ...
                    UIPaymentLayer.ShowLayer(coin:(long)(200 * PlayerSandbox.Instance.ChessBoardHandler.Lands.GetLandInfoByLevel(landTile.GetTileData().level).standMul), onConfirm: () =>
                    {
                        GameSessionAPI.ChessBoardAPI.Option(null);
                        // landTile.PerformLocked(false);
                    });

                    if (GameInstance.Instance.HostingHandler.Hosting)
                    {
                        UIPaymentLayer.GetLayer().Confirm();
                    }
                    break;
                case "max":
                    layer.SpawnWorldElement<UIWorldElement>(EffectInstance.Instance.Settings.uiEffectLandLvMax, tipLocation);
                    break;
            }
        }

        #endregion
    }

}