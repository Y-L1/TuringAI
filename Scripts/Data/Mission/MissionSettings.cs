using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game
{
    [CreateAssetMenu(fileName = "MissionSettings", menuName = "Scriptable Objects/MissionSettings")]
    public class MissionSettings : ScriptableObject
    {
        [Header("Daily")]
        [SerializeField] private List<FMissionDaily> missionDaily;
        
        [Space]
        
        [Header("Weekly")]
        [SerializeField] private int maxScore = 70;
        [SerializeField] private List<FMissionWeekly> missionWeekly;
        
        public IReadOnlyList<FMissionDaily> MissionDaily => missionDaily.AsReadOnly();
        public IReadOnlyList<FMissionWeekly> MissionWeekly => missionWeekly.AsReadOnly();

        #region Function - Daily

        public FMissionDaily GetMissionDaily(string id)
        {
            foreach (var daily in MissionDaily)
            {
                if(daily.id == id) return daily;
            }
            return default;
        }

        #endregion


        #region Function - Weekly

        public FMissionWeekly GetMissionWeekly(string id)
        {
            foreach (var weekly in MissionWeekly)
            {
                if(weekly.id == id) return weekly;
            }
            return default;
        }

        public int GetMaxScore()
        {
            return maxScore;
        }

        #endregion
    }

    [System.Serializable]
    public struct FMissionDaily
    {
        [SerializeField] public string id;
        [SerializeField] public string descriptionKey;
        [SerializeField] public int maxProgress;
        [SerializeField] public int coin;
        [SerializeField] public int dice;
        [SerializeField] public int score;
    }

    [System.Serializable]
    public struct FMissionWeekly
    {
        [SerializeField] public string id;
        [SerializeField] public int progress;
        [SerializeField] public string description;
        [SerializeField] public int coin;
        [SerializeField] public int dice;
    }
}