using DragonLi.UI;
using TMPro;
using UnityEngine;

namespace Game
{
    public class UILoginLayer : UILayer
    {
        #region Properties

        [Header("Settings")] 
        [SerializeField] private TextMeshProUGUI progressText;

        #endregion

        #region API

        public void SetProgressText(string text)
        {
            progressText.text = text;
        }

        #endregion
    }
}


