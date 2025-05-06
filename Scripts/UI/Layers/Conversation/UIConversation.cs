using System;
using System.Collections;
using _Scripts.UI.Common.Grids;
using Data.Type;
using DragonLi.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    [RequireComponent(typeof(DummyPoolObject))]
    public class UIConversation : GridBase
    {
        [SerializeField] private Color colorAgent;
        [SerializeField] private Color colorPlayer;
        [SerializeField] private TextMeshProUGUI tmpConversation;
        [SerializeField] private Button btnPlay;
        // [SerializeField] private AudioSourceListener audioSource;
        
        private AudioClip audioClip;
        
        private TuringBarGameMode TuringGameMode { get; set; }

        public override void SetGrid(params object[] args)
        {
            base.SetGrid(args);
            audioClip = null;
            var messageInfo = (AIChatType.TChatMessage)args[0];
            btnPlay?.gameObject.SetActive(messageInfo.chatType == AIChatType.EChatType.Agent);
            btnPlay?.onClick.RemoveAllListeners();
            btnPlay?.onClick.AddListener(() =>
            {
                // ((Action<string>)args[1])?.Invoke(messageInfo.message);
                // PlaySpeech();
                // TuringGameMode = GameMode.GetGameMode<TuringBarGameMode>();
                
                UIAgentLayer.GetLayer()?.ASListener.PlayWithListener(audioClip);
                
                // audioSource.PlayWithListener();
            });
            if (messageInfo.chatType == AIChatType.EChatType.Agent)
            {
                tmpConversation.color = colorAgent;
                tmpConversation.alignment = TextAlignmentOptions.Left;
                audioClip = args[1] as AudioClip;
                
                UIAgentLayer.GetLayer()?.ASListener.PlayWithListener(audioClip);
                // audioSource.PlayWithListener(audioClip);
                // audioSource.clip = audioClip;
                // PlaySpeech();
            }
            else
            {
                tmpConversation.color = colorPlayer;
                tmpConversation.alignment = TextAlignmentOptions.Right;
            }
            
            tmpConversation.SetText(messageInfo.message);
        }

        // public void PlaySpeech()
        // {
        //     if (audioClip && audioSource && !audioSource.isPlaying)
        //     {
        //         // SoundAPI.PlaySound(audioClip, 0.3f);
        //         audioSource.volume = 1f;
        //         audioSource.Play();
        //     }
        // }
    }
}