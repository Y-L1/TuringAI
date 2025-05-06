using System;
using System.Collections.Generic;
using Game;
using UnityEngine;

public static class TilesAPI
{
    // public static bool IsValidIndex(this ChessTile tile, int index)
    // {
    //     
    // }
    
    /// <summary>
    /// 曼哈顿距离
    /// </summary>
    /// <param name="from">起点</param>
    /// <param name="to">终点</param>
    /// <returns>曼哈顿距离</returns>
    public static float GetManhattanDistance(Vector3 from, Vector3 to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y) + Mathf.Abs(from.z - to.z);
    }
    
    public static float GetManhattanDistance(Vector2 from, Vector2 to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static int[] GetHouseLevels(int level)
    {
        var levelArray = new int[4];
        if (level == 0) return levelArray;
        var curLevel = level;
        while (curLevel > 0)
        {
            levelArray[(curLevel - 1) % 4]++;
            curLevel--;
        }
        return levelArray;
    }

}
