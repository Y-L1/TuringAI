using System;
using System.Collections;
using System.Collections.Generic;
using _Scripts.Utils;
using Data;
using Data.Type;
using DragonLi.Core;
using DragonLi.Frame;
using DragonLi.Network;
using DragonLi.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace Game
{
    public class TuringBarGameMode : GameMode
    {
        private const string kCameraAnimatorWorldKey = "Camera-Animator";
        private const string kThinkControllerWorldKey = "Think-Controller";
        
        #region Properties

        [SerializeField] private Animator animator;
        [SerializeField] private string agent = "turing";

        private Animator CameraAnimator { get; set; }
        
        private ThinkController ThinkController { get; set; }

        #endregion

        #region Unity

        private void Awake()
        {
            PlayerSandbox.Instance.AIChatHandler.OnSessionMessage += OnSessionMessage;
        }

        private void OnDestroy()
        {
            PlayerSandbox.Instance.AIChatHandler.OnSessionMessage -= OnSessionMessage;
        }

        private IEnumerator Start()
        {
            // AudioManager.Instance.StopSound(0, 2f);
            // AudioManager.Instance.PlaySound(1, AudioInstance.Instance.Settings.turingBar, 0.125f, SystemSandbox.Instance.VolumeHandler.Volume);
            while (!GetGameMode<TuringBarGameMode>())
            {
                yield return null;
            }

            while (!(CameraAnimator = World.GetRegisteredObject(kCameraAnimatorWorldKey)?.GetComponent<Animator>()))
            {
                yield return null;
            }

            while (!(ThinkController = World.GetRegisteredObject<ThinkController>(kThinkControllerWorldKey)?.GetComponent<ThinkController>()))
            {
                yield return null;
            }

            yield return null;
            PlayerSandbox.Instance.InitReceiveListener();
            
            UIManager.Instance.GetLayer("UIBlackScreen").Hide();

            // while (!PlayerSandbox.Instance.AIChatHandler.IsConnected)
            // {
            //     yield return null;
            // }
            
            // UIStaticsLayer.ShowUIStaticsLayer();
            // UIBigCenterLayer.ShowUIBigCenterLayer("UITuringBarLayer", sender =>
            // {
            //     // if (!PlayerSandbox.Instance.AIChatHandler.IsConnected)
            //     // {
            //     //     UIReconnectingLayer.ShowLayer("disconnected-des");
            //     //     return;
            //     // }
            //     
            //     CameraAnimator.SetBool(MoveToTarget, true);
            //     UIBigCenterLayer.HideUIBigCenterLayer("UITuringBarLayer");
            //     CoroutineTaskManager.Instance.WaitSecondTodo(UIConversationLayer.ShowLayer, 1.5f);
            // });
            
            UIAgentLayer.GetLayer().Show();
        }

        #endregion

        #region API

        public ThinkController GetThinkController()
        {
            return ThinkController;
        }

        public void ChatTo(string message)
        {
            ThinkController.PlayByType(ThinkController.EThinkType.Thinking);
            GameSessionAPI.AgentAPI.Talk(agent, message);
            PlayerSandbox.Instance.AIChatHandler.AddMessage(new AIChatType.TChatMessage()
            {
                timestamp = TimeAPI.GetUtcTimeStamp(),
                chatType = AIChatType.EChatType.Owner,
                message = message
            });
        }

        #endregion
        
        #region Callbacks

        private void OnSessionMessage(AIChatType.TChatMessage message)
        {
            // if (message.chatType == AIChatType.EChatType.Agent)
            // {
            //     ThinkController.PlayByType(ThinkController.EThinkType.Idle);
            // }
        }

        #endregion
    }
}