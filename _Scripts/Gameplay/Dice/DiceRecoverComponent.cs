using System;
using System.Collections;
using Data;
using DragonLi.Network;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(ReceiveMessageHandler))]
    public class DiceRecoverComponent : MonoBehaviour, IMessageReceiver
    {
        #region Properties

        [SerializeField] private int dicePreDay = 60;
        [SerializeField] private float recoverInterval = 1440f;
        
        /// <summary>
        /// 上次恢复时间
        /// </summary>
        private float RecoverTs { get; set; }
        
        private float CurrentTs { get; set; }

        private float OneDicePerSecond => dicePreDay / 24.0f / 3600.0f;

        #endregion
        
        #region Unity

        private void Awake()
        {
            GetComponent<ReceiveMessageHandler>().OnReceiveMessageHandler += OnReceiveMessage;

            RecoverTs = Time.unscaledTime;
            CurrentTs = Time.unscaledTime;
        }

        private void FixedUpdate()
        {
            if (Time.unscaledTime - CurrentTs >= 1f)
            {
                CurrentTs = Time.unscaledTime;

                if (CurrentTs - RecoverTs >= recoverInterval)
                {
                    PlayerSandbox.Instance.CharacterHandler.Dice += Mathf.RoundToInt(OneDicePerSecond * recoverInterval);
                    RecoverTs = CurrentTs;
                }
            }
        }

        #endregion

        #region Callback - Socket Receiver

        public void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            
        }

        #endregion
    }
}