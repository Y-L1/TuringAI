using System.Collections;
using _Scripts.UI.Common;
using Data;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Game
{
    public class UIChessboardLayer : UILayer
    {
        #region Propeties
        
        [Header("References")]
        [SerializeField] private Image imgGo;
        [SerializeField] private Toggle toggleHost;
        
        public bool Loaded { get; private set; } = false;

        private ChessGameMode GameModeRef { get; set; }
        
        private Coroutine UpdateGoCoroutine { get; set; }

        #endregion

        #region Unity

        private IEnumerator Start()
        {
            while (!(GameModeRef = GameMode.GetGameMode<ChessGameMode>(ChessGameMode.WorldObjectRegisterKey)))
            {
                yield return null;
            }

            Loaded = true;
        }

        private void OnEnable()
        {
            if (UpdateGoCoroutine != null)
            {
                StopCoroutine(UpdateGoCoroutine);
            }
            UpdateGoCoroutine = StartCoroutine(UpdateGoIEnumerator());
        }

        private void OnDisable()
        {
            if(UpdateGoCoroutine == null) return;
            StopCoroutine(UpdateGoCoroutine);
        }

        #endregion
        
        #region UILayer

        protected override void OnInit()
        {
            base.OnInit();
            this["BtnGO"].As<UIBasicButton>().OnClickEvent?.AddListener(OnGoClicked);
            this["ITEMS"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickItems);
            this["SHOP"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickShop);
            this["RANK"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickRank);
            this["Settings"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickSettings);

            toggleHost.onValueChanged.AddListener(val => { GameInstance.Instance.HostingHandler.Hosting = val; });
        }

        protected override void OnShow()
        {
            base.OnShow();
            toggleHost.isOn = GameInstance.Instance.HostingHandler.Hosting;
            var gameMode = GameMode.GetGameMode<ChessGameMode>(ChessGameMode.WorldObjectRegisterKey);
            gameMode.PlayerCameraControllerRef.SetControllerEnable(true);
        }

        protected override void OnHide()
        {
            base.OnHide();
            GameModeRef.PlayerCameraControllerRef.SetControllerEnable(false);
            UIShopLayer.HideLayer();
            UIInventoryLayer.HideLayer();
            UISettingsLayer.HideLayer();
        }

        #endregion

        #region Functions

        private static UIChessboardLayer GetLayer()
        {
            var layer = UIManager.Instance.GetLayer<UIChessboardLayer>("UIChessboardLayer");
            Assert.IsNotNull(layer);
            return layer;
        }

        private IEnumerator UpdateGoIEnumerator()
        {
            while (!Loaded)
            {
                yield return null;
            }

            while (!GameModeRef.RollDiceRef)
            {
                yield return null;
            }
            
            while (true)
            {
                if (imgGo)
                {
                    imgGo.color = GameModeRef.RollDiceRef.CanRollDice() ? Color.white : Color.black;
                }

                if (this["BtnGo"])
                {
                    this["BtnGo"].GetComponent<Button>().enabled = GameModeRef.RollDiceRef.CanRollDice();
                }

                yield return CoroutineTaskManager.Waits.HalfSecond;
            }
        }

        #endregion

        #region API

        public static void ShowLayer()
        {
            GetLayer()?.Show();
        }

        public static void HideLayer()
        {
            GetLayer()?.Hide();
        }

        #endregion

        #region Callbacks

        private void OnGoClicked(UIBasicButton sender)
        {
            if(!Loaded) return;
            if (PlayerSandbox.Instance.CharacterHandler.Dice <= 0)
            {
                UITipLayer.DisplayTip("Ops", "You don't have dice to GO!.");
                return;
            }
            
            if (!GameSessionConnection.Instance.IsConnected())
            {
                UITipLayer.DisplayTip("Ops", "The connection is closed. Please exit the game and try again.", UITipLayer.ETipType.Bad);
                return;
            }

            GameModeRef?.RollDiceRef.Move();
        }

        private void OnClickItems(UIBasicButton sender)
        {
            UIInventoryLayer.ShowLayer();
        }

        private void OnClickShop(UIBasicButton sender)
        {
            UIShopLayer.ShowLayer();
        }

        private void OnClickRank(UIBasicButton sender)
        {
            UIRanksLayer.ShowLayer();
        }

        private void OnClickSettings(UIBasicButton sender)
        {
            UISettingsLayer.ShowLayer();
        }

        #endregion
    }

}