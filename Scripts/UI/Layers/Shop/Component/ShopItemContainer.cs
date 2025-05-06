using System.Collections.Generic;
using _Scripts.Data.Shop;
using _Scripts.UI.Common.Grids;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class ShopItemContainer : GridsContainerBase
    {
        #region Properties

        private IReadOnlyCollection<ShopDataRaw> Items { get; set; } 
        
        public UnityAction<string> PurchaseAction { get; set; }

        #endregion

        #region GridsContainerBase

        public override void SpawnAllGrids(params object[] args)
        {
            base.SpawnAllGrids(args);
            Items = ShopInstance.Instance.ShopItemSettings.GetShopsByType(EShopType.Item);
            foreach (var item in Items)
            {
                var grid = SpawnGrid<UIShopItem>();
                grid.SetGrid(item, PurchaseAction);
            }
        }

        #endregion
    }
}