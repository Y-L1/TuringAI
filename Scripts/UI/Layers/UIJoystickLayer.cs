using _Scripts.UI.Common;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    public class UIJoystickLayer : UILayer
    {
        #region Property

        private JoyStickController JoyStick { get; set; }

        #endregion
        
        #region UILayer

        protected override void OnInit()
        {
            base.OnInit();
            this["BtnCharacter"].As<UIBasicButton>().OnClickEvent.AddListener(OnCharacterClick);
            this["BtnAgent"].As<UIBasicButton>().OnClickEvent.AddListener(OnAgentClick);
            JoyStick = this["Joy"].As<JoyStickController>();
        }
        
        #endregion

        #region API

        public static UIJoystickLayer GetLayer()
        {
            var layer = UIManager.Instance.GetLayer<UIJoystickLayer>("UIJoystickLayer");
            // Debug.Assert(layer != null);
            return layer;
        }

        public JoyStickController GetJoyStick()
        {
            return JoyStick;
        }

        #endregion

        #region Callback

        private void OnCharacterClick(UIBasicButton sender)
        {
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            SceneManager.Instance.AddSceneToLoadQueueByName("TuringBar00", 1);
            SceneManager.Instance.StartLoad();
        }
        
        private void OnAgentClick(UIBasicButton sender)
        {
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            SceneManager.Instance.AddSceneToLoadQueueByName("TuringBar", 1);
            SceneManager.Instance.StartLoad();
        }

        #endregion

    }
}