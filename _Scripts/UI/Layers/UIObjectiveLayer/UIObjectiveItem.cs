using System;
using System.Collections.Generic;
using Data;
using DragonLi.Core;
using DragonLi.Network;
using DragonLi.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    [RequireComponent(typeof(ReceiveMessageHandler))]
    public class UIObjectiveItem : MonoBehaviour
    {
        #region Properties

        [Header("References")]
        [SerializeField] private TextMeshProUGUI tmpDescription;
        [SerializeField] private Button btnClaim;
        [SerializeField] private TextMeshProUGUI tmpBtn;
        [SerializeField] private Image imgClaim;
        
        [Header("Settings")]
        [SerializeField] private Color unclaimedColor;
        [SerializeField] private Color processColor;
        [SerializeField] private Color claimedColor;

        private string Id { get; set; }
        
        #endregion

        #region Unity

        private void Awake()
        {
            btnClaim.onClick.AddListener(OnClickClaim);
            PlayerSandbox.Instance.ObjectiveHandler.ObjectiveDailyChanged += OnObjectiveDailyChanged;
            GetComponent<ReceiveMessageHandler>().OnReceiveMessageHandler += OnReceiveMessage;
        }

        private void OnDestroy()
        {
            PlayerSandbox.Instance.ObjectiveHandler.ObjectiveDailyChanged -= OnObjectiveDailyChanged;
        }

        #endregion

        #region Function

        private void SetContent(string id)
        {
            tmpDescription.text = this.GetLocalizedText(GetDescriptionKey(id));
        }

        private string GetDescriptionKey(string id)
        {
            return MissionInstance.Instance.Settings.GetMissionDaily(id).descriptionKey;
        }

        private void SetTMPButton(string id)
        {
            tmpBtn.text = GetProgress(id);
        }

        private string GetProgress(string id)
        {
            if (PlayerSandbox.Instance.ObjectiveHandler.Daily.IsCollectedById(id))
            {
                return this.GetLocalizedText("claimed");
            }
            
            if (PlayerSandbox.Instance.ObjectiveHandler.Daily.IsCompletedById(id))
            {
                return this.GetLocalizedText("claim");
            }
            
            var maxProgress = MissionInstance.Instance.Settings.GetMissionDaily(id).maxProgress;
            var progress = PlayerSandbox.Instance.ObjectiveHandler.Daily.GetProgressById(id);
            return maxProgress <= 1 ? this.GetLocalizedText("claim") : $"({progress}/{maxProgress})";
        }

        private bool CanClaim(string id)
        {
            var completed = PlayerSandbox.Instance.ObjectiveHandler.Daily.IsCompletedById(id);
            var collected = PlayerSandbox.Instance.ObjectiveHandler.Daily.IsCollectedById(id);
            return completed && !collected;
        }

        private void SetClaimColor(string id)
        {
            var maxProgress = MissionInstance.Instance.Settings.GetMissionDaily(id).maxProgress;
            var progress = PlayerSandbox.Instance.ObjectiveHandler.Daily.GetProgressById(id);
            if (maxProgress <= 1)
            {
                imgClaim.color = CanClaim(id) ? unclaimedColor : claimedColor;
                return;
            }

            if (progress < maxProgress)
            {
                imgClaim.color = processColor;
            }
            
        }

        #endregion

        #region API

        public void Initialize(string id)
        {
            Id = id;
            SetContent(id);
            SetClaimColor(id);
            SetTMPButton(id);
        }

        #endregion

        #region Callback

        private void OnClickClaim()
        {
            if (!CanClaim(Id)) return;
            GameSessionAPI.ObjectiveAPI.RewardDaily(Id);
        }

        private void OnObjectiveDailyChanged(FObjectiveDaily preValue, FObjectiveDaily newValue)
        {
            // 已经被领取
            if (!preValue.IsCollectedById(Id) && newValue.IsCollectedById(Id))
            {
                SetTMPButton(Id);
                SetClaimColor(Id);
            }
        }

        private void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            if (!response.IsSuccess())
            {
                this.LogErrorEditorOnly(response.error);
                return;
            }

            if (service != GameSessionAPI.ObjectiveAPI.ServiceName || method != GSObjectiveAPI.MethodRewardDaily) return;
            if (!response.GetAttachmentAsString("objective").Equals(Id)) return;
            
            // 领取成功
            var dice = response.GetAttachmentAsInt("dice");
            var coin = response.GetAttachmentAsInt("coin");

            if (dice > 0)
            {
                var tasks = new List<IQueueableEvent>
                {
                    EffectsAPI.CreateTip(() => EffectsAPI.EEffectType.Dice, () => dice),
                    EffectsAPI.CreateSoundEffect(() => EffectsAPI.EEffectType.Dice),
                    EffectsAPI.CreateScreenFullEffect(() => EffectsAPI.EEffectType.Dice, () =>
                    {
                        return dice switch
                        {
                            <= 0 => EffectsAPI.EEffectSizeType.None,
                            <= 10 => EffectsAPI.EEffectSizeType.Small,
                            <= 20 => EffectsAPI.EEffectSizeType.Medium,
                            _ => EffectsAPI.EEffectSizeType.Big,
                        };
                    }),
                    new CustomEvent(() => { PlayerSandbox.Instance.CharacterHandler.Dice += dice; })
                };
                UIObjectiveLayer.GetLayer()?.OnHideEvents.AddRange(tasks);
                
            }

            if (coin > 0)
            {
                var tasks = new List<IQueueableEvent>
                {
                    EffectsAPI.CreateTip(() => EffectsAPI.EEffectType.Coin, () => coin),
                    EffectsAPI.CreateSoundEffect(() => EffectsAPI.EEffectType.Coin),
                    EffectsAPI.CreateScreenFullEffect(() => EffectsAPI.EEffectType.Coin, () =>
                    {
                        return coin switch
                        {
                            <= 0 => EffectsAPI.EEffectSizeType.None,
                            <= 100 => EffectsAPI.EEffectSizeType.Small,
                            <= 200 => EffectsAPI.EEffectSizeType.Medium,
                            _ => EffectsAPI.EEffectSizeType.Big,
                        };
                    }),
                    new CustomEvent(() => { PlayerSandbox.Instance.CharacterHandler.Coin += coin; })
                };
                UIObjectiveLayer.GetLayer()?.OnHideEvents.AddRange(tasks);
            }
            
            PlayerSandbox.Instance.ObjectiveHandler.Daily.CompletedById(Id);
        }

        #endregion
    }
}