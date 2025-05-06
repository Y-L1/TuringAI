using _Scripts.UI.Common;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    public class UIAgentCharacterLayer : UILayer
    {
        #region Proeprty

        [Header("References")]
        [SerializeField] private CharacterCostume characterCostume;

        #endregion
        
        #region UILayer

        protected override void OnInit()
        {
            base.OnInit();
            this["BtnOk"].As<UIBasicButton>().OnClickEvent.AddListener(OnOKClick);
        }

        #endregion

        #region Callback

        private void OnOKClick(UIBasicButton sender)
        {
            // characterCostume.SaveCharacterState();
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            SceneManager.Instance.AddSceneToLoadQueueByName("TuringBar01", 1);
            SceneManager.Instance.StartLoad();
        }

        #endregion
    }
}