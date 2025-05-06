using System;
using System.Collections;
using Data;
using DragonLi.Core;
using DragonLi.Network;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

namespace Game
{
    public class ConnectToServerComponent : MonoBehaviour
    {
        #region Properties

        [Header("Settings")]
        [SerializeField] private string eventName = "CONNECT-STATUS";
        
        [Header("Debug")] 
        [SerializeField] private bool useDevAccount = false;
        [SerializeField] private string devAccount = "dev-player";
        [SerializeField] private bool debugMessage = false;
        
        #endregion
        
        #region Unity

        private void Awake()
        {
            GameSessionConnection.Instance.DebugMessage = debugMessage;
        }

        private IEnumerator Start()
        {
            if(GameSessionConnection.Instance.IsConnected())
            {
                yield break;
            }

            yield return CoroutineTaskManager.Waits.OneSecond;
            
            UnityWebRequest.ClearCookieCache();

            var config = Settings.GetConfiguration();
            TextCryptoUtils.SetDefaultVector(config.cryptoVector);
            TextCryptoUtils.SetDefaultPassword(config.cryptoPassword);
            TextCryptoUtils.SetDefaultKey(config.cryptoKey);

            while (Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return CoroutineTaskManager.Waits.OneSecond;
            }

            var sendSuccess = SendDevLoginRequest();
            this.LogEditorOnly("登陆请求发送状态: " + sendSuccess);
            EventDispatcher.TriggerEvent(eventName, 0, sendSuccess);
        }

        private void OnDestroy()
        {
            EventDispatcher.RemoveEventListener<bool, string>(GameSessionConnection.ConnectionStatusChangeEvent, OnConnectionStatusChanged);
        }

        #endregion

        #region Functions

        private bool SendDevLoginRequest()
        {
            var connection = Settings.GetConfiguration().GetConnectionConfiguration();
            var requestBody = new HttpRequestProtocol();
            requestBody.AddBodyParams("telegram-id", GetDevAccount());
            var loginRequest = new HttpRequest<HttpResponseProtocol>();
            var timeNow = (int) DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return loginRequest
                    .SetBody(requestBody)
                    .SetMethod(EHttpRequestMethod.Post)
                    .SetHeader("secret", TextCryptoUtils.GenerateDynamicPassword_Sha256(timeNow))
                    .SetUrl(connection.httpServer + "gateway/login-dev")
                    .AddCallback(OnLoginResponse)
                    .SendRequestAsync();
        }

        private bool SendLoginRequest()
        {
            return false;
        }
        
        private string GetDevAccount()
        {
            if (useDevAccount)
            {
                return devAccount;
            }

            if (PlayerPrefs.HasKey("dev-account"))
            {
                return PlayerPrefs.GetString("dev-account", "dev-player");
            }
            
            var newDevAccount = "dev-player-" + Random.Range(100000, 999999);
            PlayerPrefs.SetString("dev-account", newDevAccount);
            return newDevAccount;
        }

        #endregion

        #region Callbacks

        private void OnLoginResponse(HttpResponseProtocol response) {
            EventDispatcher.TriggerEvent(eventName, 1, response.IsSuccess());
            if (!response.IsSuccess())
            {
                this.LogEditorOnly("登陆失败! 错误信息: " + response.error);
                return;
            }

            var connection = Settings.GetConfiguration().GetConnectionConfiguration();
            var id = response.GetAttachmentAsString("id");
            var token = response.GetAttachmentAsString("token");
            PlayerSandbox.Instance.ConnectionHandler.UserId = id;
            PlayerSandbox.Instance.ConnectionHandler.UserToken = token;
            this.LogEditorOnly("登陆成功, 用户ID: " + id);
            this.LogEditorOnly("使用Token登陆: " + response.GetAttachmentAsString("token"));
            EventDispatcher.AddEventListener<bool, string>(GameSessionConnection.ConnectionStatusChangeEvent, OnConnectionStatusChanged);
            var result =  GameSessionConnection.Instance.ConnectToServer($"{connection.sessionServer}connect?token={token}&user={id}");
            this.LogEditorOnly("连接服务状态: " + result);
        }

        private void OnConnectionStatusChanged(bool status, string message)
        {
            this.LogEditorOnly("连接服务结束, 成功: " + status);
            if (!status)
            {
                this.LogErrorEditorOnly(message);
            }
            EventDispatcher.TriggerEvent(eventName, 2, status);
        }

        #endregion
    }
}


