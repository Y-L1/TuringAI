using System;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;
using DragonLi.Core;
using DragonLi.Network;
using Newtonsoft.Json;
using UnityEngine;

namespace Game
{
    public class TestScene : MonoBehaviour
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void OpenInputModal();
#endif
        
        #region Properties

        // private AIChatSession ChatSession { get; set; }

        #endregion

        #region Unity

        private IEnumerator Start()
        {
            yield return CoroutineTaskManager.Waits.TwoSeconds;
            
#if UNITY_WEBGL && !UNITY_EDITOR
            OpenInputModal();
            WebGLInput.captureAllKeyboardInput = false;
#endif
            // ChatSession = GetComponent<AIChatSession>();
            // yield return CoroutineTaskManager.Waits.TwoSeconds;
            //
            // EventDispatcher.AddEventListener(AIChatSession.AIChatSessionOpenEvent, OnSessionOpen);
            // EventDispatcher.AddEventListener<HttpResponseProtocol>(AIChatSession.AIChatSessionMessageEvent, OnSessionMessage);

            // 玩家需要先正常登陆后才能发起此连接, id传入服务端返回的id
            // 这里是示例
            // ...
            // var playerId = "a5332795-98b5-449b-a3e4-c2442e20ab46";
            // var connectionUrl = Settings.GetActiveConnectionConfiguration().chatbotServer;

            // ****重要：退出聊天场景会自动断开连接，因为聊天服务无法承载那么多的连接
            // 此功能暂时只用作展示
            // ...
            // ChatSession.Connect(connectionUrl + $"?id={playerId}");
        }

        #endregion

        #region Callbacks

        private void OnSessionOpen()
        {
            // ChatSession.ChatTo("eliza", "Can you tell me what's my name is?");
        }
        
        private void OnSessionMessage(HttpResponseProtocol response)
        {
            this.LogEditorOnly(JsonConvert.SerializeObject(response));
        }

        #endregion
    }
}

