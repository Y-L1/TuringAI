using _Scripts.Data.Shop;
using _Scripts.UI.Common.Grids;
using DragonLi.Core;
using DragonLi.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Game
{
    public class UIShopItem : GridBase
    {
        #region Fields

        [Header("References")]
        [SerializeField] private Image imgIcon;
        [SerializeField] private TextMeshProUGUI tmpName;
        [SerializeField] private TextMeshProUGUI tmpNumber;
        [SerializeField] private TextMeshProUGUI tmpPrice;
        [SerializeField] private TextMeshProUGUI tmpOperated;
        [SerializeField] private Button operatedButton;

        #endregion

        #region Properties - Data

        private ShopDataRaw Data { get; set; }
        private UnityAction<string> BuyAction { get; set; }

        #endregion

        #region GridBase

        protected override void OnInitialized()
        {
            base.OnInitialized();
            operatedButton.onClick.RemoveAllListeners();
            operatedButton.onClick.AddListener(() => BuyAction.Invoke(Data.id));
        }

        public override void SetGrid<T>(params object[] args)
        {
            base.SetGrid<T>(args);
            
            Data = (ShopDataRaw)args[0];
            BuyAction = (UnityAction<string>)args[1];
            
            // TODO: 设置 id 相关数据
            // ...
            SetIcon(Data.icon);
            SetName(this.GetLocalizedText(Data.id));
            SetNumber(Data.count);
            SetPrice(Data.money);
        }

        #endregion

        #region Function

        private void SetIcon(Sprite icon)
        {
            imgIcon.sprite = icon;
        }

        private void SetName(string gridName)
        {
            tmpName.text = gridName;
        }

        private void SetNumber(long gridNumber)
        {
            tmpNumber.text = NumberUtils.GetDisplayNumberStringAsCurrency(gridNumber);
        }

        private void SetPrice(float gridPrice)
        {
            tmpPrice.text = $"${gridPrice}";
        }

        private void SetOperated(string buttonName)
        {
            tmpOperated.text = buttonName;
        }
        
        #endregion
    }

}