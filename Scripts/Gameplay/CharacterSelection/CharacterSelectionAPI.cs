using System.Linq;
using Data;
using UnityEngine;

namespace Game
{
    public static class CharacterSelectionAPI
    {
        
        /// <summary>
        /// 获取角色售卖信息
        /// 注意：角色 id 是从 1 开始
        /// </summary>
        /// <param name="id">角色id</param>
        /// <returns>角色售卖信息</returns>
        public static FCharacterShopInfo GetCharacterShopInfoById(int id)
        {
            foreach (var characterInfo in PlayerSandbox.Instance.ChessBoardHandler.CharactersShopInfos.Where(characterInfo => characterInfo.id == id))
            {
                return characterInfo;
            }

            return default;
        }
    }

}