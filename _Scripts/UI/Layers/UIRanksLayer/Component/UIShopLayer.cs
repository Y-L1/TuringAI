using System;
using System.Collections.Generic;
using _Scripts.Data.Shop;
using _Scripts.UI.Common;
using Data;
using DragonLi.Network;
using DragonLi.UI;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Game
{
    [RequireComponent(typeof(ReceiveMessageHandler))]
    public class UIShopLayer : UILayer
    {
        #region Properties

        [Header("References")]
        [SerializeField] private ShopItemContainer shopItemContainer;
        [SerializeField] private ShopBlueprintContainer shopBlueprintContainer;
        
        [Header("Settings")]
        [SerializeField] private Color unselectedColor;
        [SerializeField] private Color selectedColor;

        private List<UIBasicButton> TableSelected { get; set; } = new();

        #endregion

        #region Unity

        private void Awake()
        {
            PlayerSandbox.Instance.CharacterHandler.PlayerDiceChanged += OnPlayerDiceChangeCallback;
        }

        private void OnDestroy()
        {
            PlayerSandbox.Instance.CharacterHandler.PlayerDiceChanged -= OnPlayerDiceChangeCallback;
        }

        #endregion

        #region UILayer

        protected override void OnInit()
        {
            base.OnInit();
            this["BtnClose"].As<UIBasicButton>().OnClickEvent.AddListener(OnCloseClick);
            this["ButtonItems"].As<UIBasicButton>().OnClickEvent.AddListener(OnItemClick);
            this["ButtonBlueprints"].As<UIBasicButton>().OnClickEvent.AddListener(OnBlueprintClick);
            GetComponent<ReceiveMessageHandler>().OnReceiveMessageHandler += OnReceiveMessageCallback;
            
            shopItemContainer.PurchaseAction = OnPurchaseAction;
            shopBlueprintContainer.PurchaseAction = OnPurchaseAction;

            TableSelected.Clear();
            TableSelected.Add(this["ButtonItems"].As<UIBasicButton>());
            TableSelected.Add(this["ButtonBlueprints"].As<UIBasicButton>());
        }

        protected override void OnShow()
        {
            base.OnShow();
            Switcher(EShopType.Item);
            shopItemContainer.RecycleAllGrids(null, true);
            shopItemContainer.SpawnAllGrids();
            shopBlueprintContainer.RecycleAllGrids(null, true);
            shopBlueprintContainer.SpawnAllGrids();
        }

        #endregion

        #region Function

        private void Switcher(EShopType type)
        {
            shopItemContainer.gameObject.SetActive(type == EShopType.Item);
            shopBlueprintContainer.gameObject.SetActive(type == EShopType.Blueprint);
        }

        private void Selected(UIBasicButton button)
        {
            foreach (var sender in TableSelected)
            {
                sender.GetComponent<Image>().color = button == sender ? selectedColor : unselectedColor;
            }
        }

        #endregion

        #region API

        public static UIShopLayer GetLayer()
        {
            var layer = UIManager.Instance.GetLayer<UIShopLayer>("UIShopLayer");
            Assert.IsNotNull(layer);
            return layer;
        }

        public static void ShowLayer()
        {
            GetLayer()?.Show();
        }

        public static void HideLayer()
        {
            GetLayer()?.Hide();
        }

        #endregion

        #region Callback

        private void OnCloseClick(UIBasicButton sender)
        {
            Hide();
        }

        private void OnItemClick(UIBasicButton sender)
        {
            Selected(sender);
            Switcher(EShopType.Item);
        }

        private void OnBlueprintClick(UIBasicButton sender)
        {
            Selected(sender);
            Switcher(EShopType.Blueprint);
        }

        private void OnPurchaseAction(string shopId)
        {
            ShopDataRaw data = ShopInstance.Instance.ShopItemSettings.GetShopItemById(shopId);
            UIPaymentLayer.ShowLayer(data.coin, currency:data.money, onConfirm: () =>
            {
                GameSessionAPI.CharacterAPI.Purchase(shopId);
            });
        }

        private void OnReceiveMessageCallback(HttpResponseProtocol response, string service, string method)
        {
            if (!response.IsSuccess())
            {
                this.LogErrorEditorOnly(response.error);
                return;
            }
            
            if (service == GameSessionAPI.CharacterAPI.ServiceName && method == GSCharacterAPI.MethodPurchase)
            {
                var id = response.GetAttachmentAsString("id");
                var count = response.GetAttachmentAsInt("count");

                switch (id)
                {
                    case "dice": PlayerSandbox.Instance.CharacterHandler.Dice += count; break;
                    case "coin": PlayerSandbox.Instance.CharacterHandler.Coin += count; break;
                    default:
                        var tempItems = new Dictionary<string, int>(PlayerSandbox.Instance.CharacterHandler.Items);
                        if (!tempItems.TryAdd(id, count))
                        {
                            tempItems[id] += count;
                        }
                        PlayerSandbox.Instance.CharacterHandler.Items = tempItems;
                        break;
                }
                
                UITipLayer.DisplayTip(
                    this.GetLocalizedText("notice"), 
                    this.GetLocalizedText("purchase-succeed"));
            }
        }

        private void OnPlayerDiceChangeCallback(int? preValue, int newValue)
        {
            if (newValue <= 0)
            {
                UITipLayer.DisplayTip(
                    this.GetLocalizedText("notice"), 
                    this.GetLocalizedText("dice-shot-des"), 
                    UITipLayer.ETipType.Bad,
                    ShowLayer);
            }
        }

        #endregion
    }

}