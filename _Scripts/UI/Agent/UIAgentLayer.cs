using System;
using System.Collections;
using _Scripts.UI.Common;
using Data;
using Data.Type;
using DragonLi.Core;
using DragonLi.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

namespace Game
{
    public class UIAgentLayer : UILayer
    {
        private const string FirstGameKey = "first-time-game-key";
        
        #region Property

        [Header("References")]
        [SerializeField] private UIConversationContainer container;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private AwsTTS tts;
        // [SerializeField] private TextMeshProUGUI content;
        [SerializeField] private Button sendButton;
        [SerializeField] private AudioSourceListener audioSource;
        
        private TuringBarGameMode TuringGameMode { get; set; }
        
        private HoldProgressButton HoldButton { get; set; }
        
        private Coroutine LoadingCoroutine { get; set; }
        
        public AudioSourceListener ASListener => audioSource;

        #endregion

        #region Unity

        private void Awake()
        {
            audioSource.OnAudioStarted += OnPlayStart;
            audioSource.OnAudioEnded += OnPlayEnd;
        }

        private IEnumerator Start()
        {
            while (!(TuringGameMode = GameMode.GetGameMode<TuringBarGameMode>()))
            {
                yield return null;
            }
        }

        private void OnDestroy()
        {
            audioSource.OnAudioStarted -= OnPlayStart;
            audioSource.OnAudioEnded -= OnPlayEnd;
        }

        #endregion

        #region UILayer

        protected override void OnInit()
        {
            base.OnInit();
            PlayerSandbox.Instance.AIChatHandler.OnSessionMessage += OnSessionMessage;
            this["WorldButton"].As<UIBasicButton>().OnClickEvent.AddListener(OnWorldClick);
            this["BtnSend"].As<UIBasicButton>().OnClickEvent.AddListener(OnSendClick);
            HoldButton = this["MicphoneHold"].As<HoldProgressButton>();
            
            HoldButton.RecordComponent.RecordResultEvent.AddListener((res, text) =>
            {
                if (res)
                {
                    // content.SetText(text);
                }
                else
                {
                    UITipLayer.DisplayTip(this.GetLocalizedText("turing"), "Speech recognition was not successful!");
                }
            });
            
            
        }

        protected override void OnShow()
        {
            base.OnShow();
            HoldButton.Pressed += HoldPressed;
            HoldButton.Released += HoldReleased;
            
            container.RecycleAllGrids();
            container.SpawnAllGrids(PlayerSandbox.Instance.AIChatHandler.GetMessages());
            CoroutineTaskManager.Instance.WaitFrameEnd(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
                if(scrollRect)
                    scrollRect.verticalNormalizedPosition = 0;
            });
        }

        protected override void OnHide()
        {
            base.OnHide();
            HoldButton.Pressed -= HoldPressed;
            HoldButton.Released -= HoldReleased;
        }

        #endregion

        #region Function

        private IEnumerator PlayAnimateDots()
        {
            var dotCount = 0;
            while (true)
            {
                dotCount = (dotCount + 1) % 7;
                var dots = new string('.', dotCount);
                // content.SetText(dots);
                yield return CoroutineTaskManager.Waits.TwoSeconds;
            }
        }

        #endregion

        #region API

        public static UIAgentLayer GetLayer()
        {
            var layer = UIManager.Instance.GetLayer<UIAgentLayer>("UIAgentLayer");
            Debug.Assert(layer != null);
            return layer;
        }

        #endregion

        #region Callback

        private void OnPlayStart(AudioSource source)
        {
            if(!TuringGameMode) return;
            var thinkController = TuringGameMode.GetThinkController();
            thinkController.PlayByType(ThinkController.EThinkType.Talking);
        }

        private void OnPlayEnd(AudioSource source)
        {
            if(!TuringGameMode) return;
            var thinkController = TuringGameMode.GetThinkController();
            thinkController.PlayByType(ThinkController.EThinkType.Idle);
        }
        
        private async void OnSessionMessage(AIChatType.TChatMessage message)
        {
            // message.timestamp = TimeAPI.GetUtcTimeStamp();
            AudioClip clip = null;
            if (message.chatType == AIChatType.EChatType.Owner)
            {
                
            }
            
            if (message.chatType == AIChatType.EChatType.Agent)
            {
                if (LoadingCoroutine != null)
                {
                    StopCoroutine(LoadingCoroutine);
                    LoadingCoroutine = null;
                }
                
                // sendButton.interactable = true;
                // content.SetText(message.message);
                clip = await tts.TTS(message.message, message.timestamp);
                
                // audioSource.StopWithListener();
                // audioSource.PlayWithListener(clip);
            }
            
            container.SpawnMessage(message, clip);
            
            CoroutineTaskManager.Instance.WaitFrameEnd(() =>
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
                if(scrollRect)
                    scrollRect.verticalNormalizedPosition = 0;
            });
        }

        private void OnWorldClick(UIBasicButton sender)
        {
            UIManager.Instance.GetLayer("UIBlackScreen").Show();
            
            if (!PlayerPrefs.HasKey(FirstGameKey) || PlayerPrefs.GetInt(FirstGameKey) == 0)
            {
                PlayerPrefs.SetInt(FirstGameKey, 1);
                PlayerPrefs.Save();
                SceneManager.Instance.AddSceneToLoadQueueByName("TuringBar00", 1);
            }
            else
            {
                SceneManager.Instance.AddSceneToLoadQueueByName("TuringBar01", 1);
            }
            SceneManager.Instance.StartLoad();
        }

        private void OnSendClick(UIBasicButton sender)
        {
            if (LoadingCoroutine != null) return;
            UIInputLayer.ShowLayer();
            UIInputLayer.GetLayer().OnSubmitAction = text =>
            {
                GameMode.GetGameMode<TuringBarGameMode>().ChatTo(text);
                // LoadingCoroutine = StartCoroutine(PlayAnimateDots());
            };
        }

        private void HoldPressed(HoldProgressButton sender)
        {
            this.LogEditorOnly("HoldPressed");
        }

        private void HoldReleased(HoldProgressButton sender)
        {
            this.LogEditorOnly("HoldReleased");
        }

        #endregion
    }
}
