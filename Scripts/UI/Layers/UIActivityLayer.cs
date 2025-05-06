using _Scripts.UI.Common;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game
{
    public class UIActivityLayer : UILayer
    {
        #region Properties

        private bool IsProcessing { get; set; }

        #endregion
        
        #region UILayer

        protected override void OnInit()
        {
            base.OnInit();
            this["BtnAnnouncement"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickAnnouncement);
            this["BtnTask"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickTask);
            this["BtnSign"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickSign);
            this["BtnBuildArea"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickBuildArea);
            this["BtnTeleport"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickTeleport);
            this["BtnAgent"].As<UIBasicButton>().OnClickEvent?.AddListener(OnClickAgent);
        }

        protected override void OnHide()
        {
            base.OnHide();
            UIStaticsLayer.HideUIStaticsLayer();
            UIChessboardLayer.HideLayer();
            UIObjectiveLayer.HideLayer();
        }

        #endregion

        #region Functions

        private void ShowLayer()
        {
            IsProcessing = false;
            Show();
        }

        #endregion

        #region API

        public static void ShowUIActivityLayer()
        {
            var layer = UIManager.Instance.GetLayer<UIActivityLayer>("UIActivityLayer");
            Assert.IsNotNull(layer);
            layer.ShowLayer();
        }

        public static void HideUIActivityLayer()
        {
            var layer = UIManager.Instance.GetLayer<UIActivityLayer>("UIActivityLayer");
            Assert.IsNotNull(layer);
            layer.Hide();
        }

        #endregion

        #region Callbacks

        private void OnClickAnnouncement(UIBasicButton sender)
        {
            UIManager.Instance.GetLayer("UIAnnLayer")?.Show();
        }

        private void OnClickTask(UIBasicButton sender)
        {
            UIObjectiveLayer.ShowLayer();
        }
        private void OnClickSign(UIBasicButton sender) { }

        private void OnClickBuildArea(UIBasicButton sender)
        {
            if (IsProcessing) return;
            IsProcessing = true;
            Hide();
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            SceneManager.Instance.AddSceneToLoadQueueByName("AreaSelection", 1);
            SceneManager.Instance.StartLoad();
        }

        private void OnClickTeleport(UIBasicButton sender)
        {
            if (IsProcessing) return;
            IsProcessing = true;
            Hide();
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            SceneManager.Instance.AddSceneToLoadQueueByName("ChessSelection", 1);
            SceneManager.Instance.StartLoad();
        }

        private void OnClickAgent(UIBasicButton sender)
        {
            if (IsProcessing) return;
            IsProcessing = true;
            Hide();
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            SceneManager.Instance.AddSceneToLoadQueueByName("TuringBar", 1);
            SceneManager.Instance.StartLoad();
        }

        #endregion
    }

}