using _Scripts.UI.Common;
using Data;
using DragonLi.Core;
using DragonLi.Frame;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    public class UIChessSelectionLayer : UILayer
    {

        #region Properties

        private static readonly int AnimHashBlockIndex = Animator.StringToHash("SceneIndex");
        private const string kCameraAnimatorWorldKey = "Camera-Animator";

        private Animator CameraAnimator { get; set; }
        private int Index { get; set; }
        
        private bool IsProcessing { get; set; }

        #endregion

        #region UILayer

        protected override void OnInit()
        {
            Index = 0;
            var cameraAnimatedRoot = World.GetRegisteredObject(kCameraAnimatorWorldKey);
            Debug.Assert(cameraAnimatedRoot != null, "CameraAnimatedRoot != null");
            CameraAnimator = cameraAnimatedRoot.GetComponent<Animator>();
            
            this["BtnNext"].As<UIBasicButton>().OnClickEvent?.AddListener(OnNextButtonPressed);
            this["BtnPrev"].As<UIBasicButton>().OnClickEvent?.AddListener(OnPrevButtonPressed);
            this["BtnBack"].As<UIBasicButton>().OnClickEvent?.AddListener(OnBackButtonPressed);
            this["BtnGo"].As<UIBasicButton>().OnClickEvent?.AddListener(OnGoClicked);
            this["BtnPrev"].gameObject.SetActive(false);
        }

        protected override void OnShow()
        {
            base.OnShow();
            IsProcessing = false;
            UIStaticsLayer.ShowUIStaticsLayer();
        }

        protected override void OnHide()
        {
            base.OnHide();
            UIStaticsLayer.HideUIStaticsLayer();
        }

        #endregion

        #region Callbacks

        private void OnPrevButtonPressed(UIBasicButton sender)
        {
            if (IsProcessing) return;
            IsProcessing = true;
            CoroutineTaskManager.Instance.WaitSecondTodo(() =>
            {
                IsProcessing = false;
            }, 0.5f);
            Index--;
            CameraAnimator?.SetInteger(AnimHashBlockIndex, Index);
            sender.gameObject.SetActive(Index > 0);
            this["BtnNext"].gameObject.SetActive(true);
        }

        private void OnNextButtonPressed(UIBasicButton sender)
        {
            if (IsProcessing) return;
            IsProcessing = true;
            CoroutineTaskManager.Instance.WaitSecondTodo(() =>
            {
                IsProcessing = false;
            }, 0.5f);
            
            Index++;
            CameraAnimator?.SetInteger(AnimHashBlockIndex, Index);
            
            sender.gameObject.SetActive(Index < ChessBoardAPI.GetChessboardRouter().Count - 1);
            this["BtnPrev"].gameObject.SetActive(true);
        }
        
        private void OnBackButtonPressed(UIBasicButton sender)
        {
            if (IsProcessing) return;
            IsProcessing = true;
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            SceneManager.Instance.AddSceneToLoadQueueByName(ChessBoardAPI.GetCurrentChessBoard(), 1, true);
            SceneManager.Instance.StartLoad();
        }

        private void OnGoClicked(UIBasicButton sender)
        {
            if (IsProcessing) return;
            IsProcessing = true;
            Hide();
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            if (PlayerSandbox.Instance.CharacterHandler.ChessboardId != Index)
            {
                GameSessionAPI.CharacterAPI.SetChessboard(Index);
                PlayerSandbox.Instance.CharacterHandler.ChessboardId = Index;
            }
            SceneManager.Instance.AddSceneToLoadQueueByName(ChessBoardAPI.GetCurrentChessBoard(), 3);
            SceneManager.Instance.StartLoad();
        }

        #endregion
    }
}


