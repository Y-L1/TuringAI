using System;
using System.Collections;
using Data;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Game
{
    public class StartScene : MonoBehaviour
    {
        #region Properties
        
        [Header("Settings")]
        [SerializeField] private bool cleanPlayerPrefs = false;
        
        private UILoginLayer LoginLayer { get; set; }

        #endregion

        #region Unity

        private void Awake()
        {
            if(cleanPlayerPrefs) PlayerPrefs.DeleteAll();
#if UNITY_WEBGL && !UNITY_EDITOR
            // TelegramIntegration.LoadTelegramSDK();
            // TelegramIntegration.TelegramMiniAppFullScreen();
            Application.targetFrameRate = 45;
#endif
            Settings.LoadSettings();
        }

        private IEnumerator Start()
        {
            EventDispatcher.AddEventListener<int, bool>("CONNECT-STATUS", OnConnectionStatusChanged);
            
            yield return null;
            UIManager.Instance.GetLayer("UIBlackScreen").Hide();
            LoginLayer = UIManager.Instance.GetLayer<UILoginLayer>("UILoginLayer");
            
#if UNITY_IOS || UNITY_ANDROID
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif
        }

        private void OnDestroy()
        {
            EventDispatcher.RemoveEventListener<int, bool>("CONNECT-STATUS", OnConnectionStatusChanged);
            PlayerSandbox.Instance.CharacterHandler.ChessboardIdChanged -= OnCharacterDataReceived;
        }

        #endregion
        
        #region Callbacks
        
        private void OnConnectionStatusChanged(int step, bool status)
        {
            switch (step)
            {
                case 0:
                    LoginLayer.SetProgressText(status ? "Logging In" : "Login Error");
                    return;
                case 1:
                    LoginLayer.SetProgressText(status ? "Connecting" : "Connect Error");
                    return;
            }

            if(status)
            {
                LoginLayer.SetProgressText("Success");
                UIManager.Instance.GetLayer("UIBlackScreen").Show();
                
                GameInstance.Instance.Initialize();
                SystemSandbox.Instance.InitializeSystemSandbox();
                PlayerSandbox.Instance.InitializePlayerSandbox();
                PlayerSandbox.Instance.CharacterHandler.ChessboardIdChanged += OnCharacterDataReceived;
            }
            else
            {
                LoginLayer.SetProgressText("Connect Failed");
            }
        }

        private void OnCharacterDataReceived(int? preVal, int nowVal)
        {
            if (nowVal < 0) return; 
            // SceneManager.Instance.AddSceneToLoadQueueByName(ChessBoardAPI.GetCurrentChessBoard(), 2);
            // SceneManager.Instance.StartLoad();

            SceneManager.Instance.AddSceneToLoadQueueByName("TuringBar", 1);
            SceneManager.Instance.StartLoad();
        }
        
        #endregion
    }
}


