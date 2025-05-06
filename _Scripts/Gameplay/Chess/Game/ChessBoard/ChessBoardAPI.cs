using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Game
{
    public static class ChessBoardAPI
    {
        private static Dictionary<int, string> ChessboardRouter { get; set; } = new()
        {
            { 0, "PeaceValle" },
            { 1, "ModernCity" },
            { 2, "CyberCity" }
        };

        public static IReadOnlyDictionary<int, string> GetChessboardRouter()
        {
            return ChessboardRouter;
        }
        
        public static string GetChessBoardByIndex(int index)
        {
            return GetChessboardRouter().GetValueOrDefault(index);
        }
        
        /// <summary>
        /// 得到当前主场景棋盘的场景名
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentChessBoard()
        {
            return GetChessBoardByIndex(PlayerSandbox.Instance.CharacterHandler.ChessboardId);
        }
    }
}