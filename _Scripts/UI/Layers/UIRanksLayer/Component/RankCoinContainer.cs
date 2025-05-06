using _Scripts.UI.Common.Grids;
using Data;
using Data.Type;
using UnityEngine;

namespace Game
{
    public class RankCoinContainer : GridsContainerBase
    {
        #region GridsContainerBase
        
        public override void SpawnAllGrids(params object[] args)
        {
            base.SpawnAllGrids(args);
            var data = PlayerSandbox.Instance.RankHandler.Ranks.GetRanksOfCoin();

            foreach (var user in data)
            {
                var grid = SpawnGrid<RankUser>();
                grid.SetGrid(user, data.IndexOf(user));
            }
        }

        #endregion
    }
}