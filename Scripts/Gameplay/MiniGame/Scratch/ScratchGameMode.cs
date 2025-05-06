using System.Collections;
using System.Collections.Generic;
using Data;
using DragonLi.Core;
using DragonLi.UI;
using Game;
using NUnit.Framework;
using UnityEngine;

namespace _Scripts.Gameplay.MiniGame.Scratch
{
    [RequireComponent(typeof(DiceRecoverComponent))]
    public class ScratchGameMode : GameModeBase
    {
        #region Fields

        [Header("Settings")]
        [SerializeField] private GameObject dollarPrefab;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private GameObject chestPrefab;
        
        [Header("Settings - Effect")]
        [SerializeField] private GameObject dollarEffect;
        [SerializeField] private GameObject coinEffect;
        [SerializeField] private GameObject chestEffect;

        [Header("References")] 
        [SerializeField] private GameObject container;
        [SerializeField] private ScratchObject[] scratchObjects;

        #endregion

        #region Properties

        private static List<int> Sequences { get; set; } = new List<int>
        {
            1, 2, 2, 1, 2, 1
        };
        private static int Coin { get; set; }
        private Dictionary<int, (GameObject, GameObject)> Routers { get; set; }
        
        private int SelectIndex { get; set; }
        
        private int FinishIndex { get; set; }

        #endregion

        #region Unity
        

        protected override void Start()
        {
            base.Start();
            AudioManager.Instance.StopSound(0, 2f, 1f);
            AudioManager.Instance.PlaySound(1, AudioInstance.Instance.Settings.scratch, SystemSandbox.Instance.VolumeHandler.Volume);
            CoroutineTaskManager.Instance.WaitSecondTodo(StartGame, 2f);
        }

        #endregion
        
        #region GameModeBase

        protected override void OnInit()
        {
            base.OnInit();
            container.SetActive(false);
            Routers = new Dictionary<int, (GameObject, GameObject)>();
            SelectIndex = 0;

            foreach (var scratch in scratchObjects)
            {
                scratch.OnUsedOperated.AddListener(OnScratchObjectClicked);
            }

            AddRouter();

            FinishIndex = GetFinishIndex(Sequences);
        }

        protected override void OnGameStartInternal()
        {
            base.OnGameStartInternal();
            UIManager.Instance.GetLayer("UIBlackScreen").Hide();
            UIStaticsLayer.ShowUIStaticsLayer();
            
            UIScratchLayer.GetLayer()?.SetGroupsCoin(GetFinishValue(Sequences), Coin);
            UIScratchLayer.ShowUIScratchLayer();
            container.SetActive(true);

            if (GameInstance.Instance.HostingHandler.Hosting)
            {
                StartCoroutine(AutoControl());
            }
        }

        protected override void OnGameEndInternal()
        {
            base.OnGameEndInternal();

            var task = new List<IQueueableEvent>
            {
                new CustomEvent(UIScratchLayer.HideUIScratchLayer),
                EffectsAPI.CreateTip(() => EffectsAPI.EEffectType.Coin, () => Coin),
                EffectsAPI.CreateSoundEffect(() => EffectsAPI.EEffectType.Coin),
                EffectsAPI.CreateScreenFullEffect(() => EffectsAPI.EEffectType.Coin, () =>
                {
                    return Sequences[FinishIndex] switch
                    {
                        0 => EffectsAPI.EEffectSizeType.Big,
                        1 => EffectsAPI.EEffectSizeType.Medium,
                        2 => EffectsAPI.EEffectSizeType.Small,
                        _ => EffectsAPI.EEffectSizeType.None
                    };
                }),
                new CustomEvent(() =>
                {
                    UIStaticsLayer.HideUIStaticsLayer();
                    UIManager.Instance.GetLayer("UIBlackScreen").Show();
                    SceneManager.Instance.AddSceneToLoadQueueByName(ChessBoardAPI.GetCurrentChessBoard(), 3);
                    SceneManager.Instance.StartLoad();
                })
            };
            EventQueue.Instance.Enqueue(task);
        }

        #endregion

        #region Function

        private void AddRouter()
        {
            Routers.Clear();
            Routers.Add(0, (chestPrefab, chestEffect));
            Routers.Add(1, (dollarPrefab, dollarEffect));
            Routers.Add(2, (coinPrefab, coinEffect));
        }

        /// <summary>
        /// 返回序列数据：到达第三个相同元素的下标 和 到达第三个相同元素的值
        /// </summary>
        /// <param name="sequences"></param>
        /// <returns>
        /// Item1 - index
        /// Item2 - Value
        /// </returns>
        /// <remarks> 返回 (-1, -1) 表示该元素中没有三个相同的元素，· </remarks>>
        private (int, int) GetSequenceData(List<int> sequences)
        {
            if(sequences is not { Count: > 2 }) return (-1, -1);
            var res = new Dictionary<int, int>();
            for (var i = 0; i < sequences.Count; i++)
            {
                var seq  = sequences[i];
                if (res.ContainsKey(seq))
                {
                    res[seq]++;
                }
                else
                {
                    res.TryAdd(seq, 1);
                }
                
                if(res[seq] >= 3) return (i, seq);
            }
            
            return (-1, -1);
        }
        

        private int GetFinishIndex(List<int> sequences)
        {
            return GetSequenceData(sequences).Item1;
        }

        private int GetFinishValue(List<int> sequences)
        {
            return GetSequenceData(sequences).Item2;
        }
        

        private IEnumerator AutoControl()
        {
            // if(!GameInstance.Instance.HostingHandler.Hosting) yield break;

            yield return CoroutineTaskManager.Waits.TwoSeconds;

            while (!IsEnd())
            {
                var rand = Random.Range(0, scratchObjects.Length);
                var scratch = scratchObjects[rand];
                if(scratch.IsOpened()) continue;
                
                scratch.OnPointerClick(null);
                
                yield return CoroutineTaskManager.Waits.TwoSeconds;
            }
        }

        #endregion

        #region API

        public static void SetData(List<int> sequence, int coin)
        {
            Sequences = sequence;
            Coin = coin;
        }

        #endregion

        #region Callback

        private void OnScratchObjectClicked(ScratchObject scratchObject)
        {
            if (IsEnd()) return;
            scratchObject.SetContent(Routers[Sequences[SelectIndex]].Item1);
            scratchObject.SetEffect(Routers[Sequences[SelectIndex]].Item2);
            scratchObject.FlyDoor();

            switch (Sequences[SelectIndex])
            {
                case 0:
                    UIScratchLayer.AddScratch(UIScratchLayer.EGroupType.Chest);
                    break;
                case 1:
                    UIScratchLayer.AddScratch(UIScratchLayer.EGroupType.Dollar);
                    break;
                case 2:
                    UIScratchLayer.AddScratch(UIScratchLayer.EGroupType.Coin);
                    break;
            }
            
            if (SelectIndex >= FinishIndex)
            {
                EndGame();
                return;
            }
            SelectIndex++;
        }

        #endregion
    }
}