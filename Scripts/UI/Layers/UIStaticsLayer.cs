using System;
using System.Collections;
using _Scripts.UI.Common;
using Data;
using DragonLi.Core;
using DragonLi.Frame;
using DragonLi.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using SceneManager = DragonLi.Core.SceneManager;

namespace Game
{
    public class UIStaticsLayer : UILayer
    {
        #region Fields

        [Header("References")] 
        [SerializeField] private TextMeshProUGUI tmpLevel;
        [SerializeField] private UIAnimatedNumberText animTextCoin;
        [SerializeField] private UIAnimatedNumberText animTextDice;
        [SerializeField] private UIAnimatedNumberText animTextToken;

        [Header("Settings")] 
        [SerializeField] private bool canChanceCharacter;

        #endregion

        #region Properties

        private ChessGameBoard ChessBoardRef { get; set; }

        private bool Loaded { get; set; } = false;
        
        private Action<int?, int> SetLevelAction { get; set; }
        private Action<int?, int> SetCoinAction { get; set; }
        private Action<int?, int> SetDiceAction { get; set; }
        private Action<int?, int> SetTokenAction { get; set; }

        #endregion
        
        #region Unity

        private void Awake()
        {
            SystemSandbox.Instance.LanguageHandler.OnLanguageChanged += OnLanguageChanged;
        }

        private void OnDestroy()
        {
            SystemSandbox.Instance.LanguageHandler.OnLanguageChanged -= OnLanguageChanged;
        }

        private IEnumerator Start()
        {
            while (!(ChessBoardRef = World.GetRegisteredObject<ChessGameBoard>(ChessGameBoard.WorldObjectRegisterKey)))
            {
                yield return null;
            }
            
            Loaded = true;
        }

        #endregion

        #region UILayer

        protected override void OnInit()
        {
            base.OnInit();
            this["BtnFace"].As<UIBasicButton>()?.OnClickEvent.AddListener(OnCharacterSelectionClick);

            SetLevelAction = (oldV, newV) => { SetLevel(newV); };
            SetCoinAction = (oldV, newV) => { SetCoin(newV); };
            SetDiceAction = (oldV, newV) => { SetDice(newV); };
            SetTokenAction = (oldV, newV) => { SetToken(newV); };
        }

        protected override void OnShow()
        {
            base.OnShow();
            SetLevel(0);
            SetCoin(PlayerSandbox.Instance.CharacterHandler.Coin);
            SetDice(PlayerSandbox.Instance.CharacterHandler.Dice);
            SetToken(PlayerSandbox.Instance.CharacterHandler.Token);
            PlayerSandbox.Instance.CharacterHandler.PlayerCoinChanged += SetCoinAction;
            PlayerSandbox.Instance.CharacterHandler.PlayerDiceChanged += SetDiceAction;
            PlayerSandbox.Instance.CharacterHandler.PlayerTokenChanged += SetTokenAction;
        }

        protected override void OnHide()
        {
            base.OnHide();
            PlayerSandbox.Instance.CharacterHandler.PlayerCoinChanged -= SetCoinAction;
            PlayerSandbox.Instance.CharacterHandler.PlayerDiceChanged -= SetDiceAction;
            PlayerSandbox.Instance.CharacterHandler.PlayerTokenChanged -= SetTokenAction;
        }

        #endregion

        #region Function

        private void ShowLayer()
        {
            Show();
        }

        private void SetLevel(int level)
        {
            if (tmpLevel == null) return;
            tmpLevel.text = string.Format(this.GetLocalizedText("level-fmt"), NumberUtils.GetDisplayNumberString(level));
        }

        private void SetCoin(int coin)
        {
            if(animTextCoin == null) return;
            animTextCoin.SetNumber(coin);
        }

        private void SetDice(int dice)
        {
            if(animTextDice == null) return;
            animTextDice.SetNumber(dice);
        }

        private void SetToken(int token)
        {
            if(animTextToken == null) return;
            animTextToken.SetNumber(token);
        }

        #endregion

        #region API

        public static void ShowUIStaticsLayer()
        {
            var layer = UIManager.Instance.GetLayer<UIStaticsLayer>("UIStaticsLayer");
            Assert.IsNotNull(layer);
            layer.ShowLayer();
        }

        public static void HideUIStaticsLayer()
        {
            var layer = UIManager.Instance.GetLayer<UIStaticsLayer>("UIStaticsLayer");
            Assert.IsNotNull(layer);
            layer.Hide();
        }

        #endregion

        #region Callabck

        private void OnCharacterSelectionClick(UIBasicButton sender)
        {
            if (!canChanceCharacter) return;
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            SceneManager.Instance.AddSceneToLoadQueueByName("CharacterSelection", 2);
            SceneManager.Instance.StartLoad();
        }

        private void OnLanguageChanged(string preVal, string newVal)
        {
            SetLevel(0);
        }

        #endregion
    }
}