using UnityEngine;
using System.Collections;
using DragonLi.UI;
using UnityEngine.iOS;
using UnityEngine.Android;

namespace Game
{
    public class Authorization : MonoBehaviour
    {
        #region Property

        [Header("Settings - MobileDevice")]
        [SerializeField] private bool micphone;

        #endregion

        #region Unity

        private void Start()
        {
            if (micphone)
            {
                CheckMicrophonePermission();
            }
        }

        #endregion

        #region Function

        private void CheckMicrophonePermission()
        {
#if UNITY_IOS
        // iOS: 先判断是否已授权麦克风
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            Debug.Log("未授权麦克风，开始请求权限...");
            StartCoroutine(RequestMicrophonePermission());
        }
        else
        {
            Debug.Log("麦克风权限已授权！");
        }
#elif UNITY_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.Log("请求 Android 麦克风权限...");
                Permission.RequestUserPermission(Permission.Microphone);
            }
            else
            {
                Debug.Log("Android 麦克风权限已授权！");
            }
#else
        Debug.Log("当前平台不需要请求麦克风权限。");
#endif
        }
        
        private IEnumerator RequestMicrophonePermission()
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                Debug.Log("用户授权了麦克风权限！");
            }
            else
            {
                UITipLayer.DisplayTip(this.GetLocalizedText("notice"), 
                    "To use voice features, please enable microphone access in:\nSettings > Privacy & Security > Microphone > Turing AI City.",
                    UITipLayer.ETipType.Normal,
                    Application.Quit);
                Debug.LogWarning("用户拒绝了麦克风权限！");
            }
        }

        #endregion
    }
}