using System.Collections.Generic;
using _Scripts.UI.Common;
using Data;
using DragonLi.UI;
using DragonLi.Core;
using DragonLi.Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    [RequireComponent(typeof(ReceiveMessageHandler))]
    public class UICharacterSelectionLayer : UILayer, IMessageReceiver
    {
        #region Fields

        [Header("References")]
        [SerializeField] private List<GameObject> characters = new();
        [SerializeField] private TextMeshProUGUI tmpNameText;
        [SerializeField] private TextMeshProUGUI tmpDiceText;
        [SerializeField] private TextMeshProUGUI tmpCoinText;
        [SerializeField] private TextMeshProUGUI tmpPriceText;
        [SerializeField] private TextMeshProUGUI tmpDescriptionText;
        [Header("Settings")]
        [SerializeField] [Range(0, 255)] private int validAlpha = 255; 
        [SerializeField] [Range(0, 255)] private int invalidAlpha = 80;

        #endregion

        #region Properties
        
        private Image PreImage { get; set; }
        private Image NextImage { get; set; }

        private int _selectedCharacter;
        private int SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (_selectedCharacter == value) return;
                _selectedCharacter = value;
                OnSelectedChange(value);
            }
        }
        
        // private int CharacterId => _selectedCharacter;

        #endregion
        
        #region UILayer

        protected override void OnInit()
        {
            base.OnInit();
            GetComponent<ReceiveMessageHandler>().OnReceiveMessageHandler += OnReceiveMessage;
            this["BtnNext"].As<UIBasicButton>().OnClickEvent?.AddListener(OnNextButtonPressed);
            this["BtnPrev"].As<UIBasicButton>().OnClickEvent?.AddListener(OnPrevButtonPressed);
            this["BtnUnlock"].As<UIBasicButton>().OnClickEvent?.AddListener(OnUnlockButtonPressed);
            this["BtnGet"].As<UIBasicButton>().OnClickEvent?.AddListener(OnGetButtonPressed);
            this["BtnBack"].As<UIBasicButton>().OnClickEvent?.AddListener(OnBackPressed);
            PreImage = this["BtnPrev"].GetComponent<Image>();
            NextImage = this["BtnNext"].GetComponent<Image>();
        }

        protected override void OnShow()
        {
            base.OnShow();
            SetUp();
            UIStaticsLayer.ShowUIStaticsLayer();
        }

        protected override void OnHide()
        {
            base.OnHide();
            UIStaticsLayer.HideUIStaticsLayer();
        }

        #endregion

        #region API

        public int GetMaxCharacters()
        {
            return characters?.Count ?? 0;
        }

        #endregion

        #region Functiuons

        private void SetName(string characterName)
        {
            tmpNameText.text = characterName;
        }

        private void SetDice(int dice)
        {
            
            tmpDiceText.text = string.Format(this.GetLocalizedText("more-dice-fmt"), dice);
        }

        private void SetCoin(float coin)
        {
            tmpCoinText.text = string.Format(this.GetLocalizedText("more-coin-fmt"), $"{(int)(coin * 100f)}%");
        }

        private void SetPrice(float price)
        {
            tmpPriceText.text = string.Format(this.GetLocalizedText("dollar-sign-fmt"), price);
        }

        private void SetDescription(bool unlocked)
        {
            tmpDescriptionText.text = this.GetLocalizedText(!unlocked ? "unlock-character-des" : "get-character-des");
        }

        private void SetUp()
        {
            SwitchCharacter(PlayerSandbox.Instance.CharacterHandler.CharacterId);
            SelectedCharacter = PlayerSandbox.Instance.CharacterHandler.CharacterId;
        }

        private void SwitchCharacter(int character)
        {
            for (var i = 0; i < GetMaxCharacters(); i++)
            {
                characters[i].SetActive(i == character);
            }

            var characterInfo = CharacterSelectionAPI.GetCharacterShopInfoById(SelectedCharacter);
            var characterUnlock = IsCharacterUnlocked(SelectedCharacter);
            SetName(this.GetLocalizedText($"character-{characterInfo.id}"));
            SetDice(characterInfo.dice);
            SetCoin(characterInfo.coinMul);
            SetPrice(characterInfo.price);
            SetDescription(characterUnlock);
            
            this["BtnUnlock"].gameObject.SetActive(!characterUnlock);
            this["BtnGet"].gameObject.SetActive(characterUnlock);
        }

        private void SetImageAlphaValid(Image image, bool valid)
        {
            var preColor = image.color;
            preColor.a = (valid ? validAlpha : invalidAlpha) / 255f;
            image.color = preColor;
        }

        private bool IsCharacterUnlocked(int characterId)
        {
            return true;
            return PlayerSandbox.Instance.CharacterHandler.Characters.Contains(characterId);
        }

        #endregion

        #region Callbacks

        private void OnSelectedChange(int character)
        {
            SetImageAlphaValid(PreImage, character != 0);
            SetImageAlphaValid(NextImage, character != GetMaxCharacters() - 1);
            SwitchCharacter(character);
        }

        private void OnNextButtonPressed(UIBasicButton sender)
        {
            if (SelectedCharacter >= GetMaxCharacters() - 1) return;
            SelectedCharacter++;
        }

        private void OnPrevButtonPressed(UIBasicButton sender)
        {
            if(SelectedCharacter <= 0) return;
            SelectedCharacter--;
        }

        private void OnUnlockButtonPressed(UIBasicButton sender)
        {
            // TODO: 购买角色相关逻辑处理
            // ...
        }

        private void OnGetButtonPressed(UIBasicButton sender)
        {
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            GameSessionAPI.CharacterAPI.SetCharacter(SelectedCharacter);
            PlayerSandbox.Instance.CharacterHandler.CharacterId = SelectedCharacter;
            SceneManager.Instance.AddSceneToLoadQueueByName(ChessBoardAPI.GetCurrentChessBoard(), 2, true);
            SceneManager.Instance.StartLoad();
            Hide();
        }

        private void OnBackPressed(UIBasicButton sender)
        {
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            SceneManager.Instance.AddSceneToLoadQueueByName(ChessBoardAPI.GetCurrentChessBoard(), 2, true);
            SceneManager.Instance.StartLoad();
            Hide();
        }
        
        #endregion

        #region Callback - Socket Receiver

        public void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            // TODO: 购买角色服务器响应
            // ...
        }

        #endregion
    }

}