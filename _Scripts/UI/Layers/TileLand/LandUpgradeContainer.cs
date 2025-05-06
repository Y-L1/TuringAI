using System;
using System.Collections.Generic;
using _Scripts.UI.Common.Grids;
using Data;
using UnityEngine;

namespace Game
{
    public class LandUpgradeContainer : GridsContainerBase
    {
        #region Properties

        private Dictionary<int, UpgradeLand> UpgradeLands { get; set; } = new();
        private Dictionary<int, FLand> FLands { get; set; } = new();

        #endregion
        #region GridsContainerBase

        public override void SpawnAllGrids(params object[] args)
        {
            base.SpawnAllGrids(args);
            
            UpgradeLands.Clear();
            FLands.Clear();
            foreach (var land in PlayerSandbox.Instance.ChessBoardHandler.Lands)
            {
                FLands.TryAdd(land.level, land);
            }

            for (var i = 1; i <= FLands.Count; i++)
            {
                var slot = SpawnGrid<UpgradeLand>();
                slot.SetLevel(i);
                slot.SetCoin((long) Mathf.Round(FLands[i].standMul * 100));
                slot.transform.localScale = Vector3.one;
                UpgradeLands.TryAdd(i, slot);
            }
        }


        #endregion

        #region API

        public UpgradeLand GetUpgradeLandByLevel(int level)
        {
            return UpgradeLands.GetValueOrDefault(level);
        }

        #endregion
    }

}