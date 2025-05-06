using _Scripts.UI.Common.Grids;
using Data.Type;
using DragonLi.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class RankUser : GridBase
    {
        #region Properties

        [Header("Settings")]
        [SerializeField] private Color colorFirst;
        [SerializeField] private Color colorSecond;
        [SerializeField] private Color colorThird;
        [SerializeField] private Color colorOther;
        
        [Header("References")]
        [SerializeField] private TextMeshProUGUI tmpRank;
        [SerializeField] private Image imgRank;
        [SerializeField] private Image imgAvatar;
        [SerializeField] private TextMeshProUGUI tmpName;
        [SerializeField] private TextMeshProUGUI tmpNum;

        #endregion

        #region GridBase

        public override void SetGrid<T>(params object[] args)
        {
            base.SetGrid<T>(args);
            var data = args[0] as RankHandlerType.FUser? ?? default;
            var rank = (int)args[1];
            SetName(data.name);
            SetNum(data.data);
            SetRank(rank);
        }

        #endregion

        #region Function

        private void SetRank(int rank)
        {
            if(tmpRank == null) return;
            rank = Mathf.Clamp(rank, 0, int.MaxValue);
            tmpRank.text = rank <= 999 ? $"{rank}" : "999+";
            
            SetRankColor(rank);
        }

        private void SetRankColor(int rank)
        {
            if(imgRank == null) return;
            imgRank.color = rank switch
            {
                0 => colorFirst,
                1 => colorSecond,
                2 => colorThird,
                _ => colorOther
            };
        }

        private void SetAvatar(Sprite avatar)
        {
            if(imgAvatar == null || avatar == null) return;
            imgAvatar.sprite = avatar;
        }

        private void SetName(string userName)
        {
            if(tmpName == null) return;
            tmpName.text = userName.Length <= 12 ? userName : $"{userName[..9]}...";
        }

        private void SetNum(long num)
        {
            if(tmpNum == null) return;
            tmpNum.text = num <= 99999 ? $"{NumberUtils.GetDisplayNumberStringAsCurrency(num)}" : $"{NumberUtils.GetDisplayNumberStringAsCurrency(99999)}+";
        }

        #endregion
        
        #region API

        public void SetUpRank(int rank, Sprite avatar, string userName, long num)
        {
            SetRank(rank);
            SetAvatar(avatar);
            SetName(userName);
            SetNum(num);
        }

        #endregion
    }
}