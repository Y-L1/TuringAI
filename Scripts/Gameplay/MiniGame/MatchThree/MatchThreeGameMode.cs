using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using DG.Tweening;
using DragonLi.Core;
using DragonLi.UI;
using Game;
using TMPro;
using UnityEngine;

namespace _Scripts.Gameplay.MiniGame.MatchThree
{
    [RequireComponent(typeof(DiceRecoverComponent))]
    public class MatchThreeGameMode : GameModeBase
    {
        #region Fields
        
        [Header("References")]
        [SerializeField] private TextMeshProUGUI remainTimeText;

        [Header("Settings")] 
        [SerializeField] private int life = 30;
        [SerializeField] public int maxRow;
        [SerializeField] public int maxColumn;
        [SerializeField] public MatchThreeContainer container;
        [SerializeField] private List<MatchThreeElementType> elements;

        #endregion

        #region Properties

        private float FinishTime { get; set; }
        private float NextTime { get; set; }
        
        private ISandbox ScoreSandbox { get; set; }
        private WaitForSeconds OneSecond { get; set; }
        private WaitForSeconds HalfSecond { get; set; }
        private WaitForSeconds OnePointSecond { get; set; }

        private Dictionary<string, GameObject> MatchesDesign { get; set; }
        private Dictionary<string, GameObject> EffectDesign { get; set; }


        private List<MatchThreeElement> Selects { get; set; } = new();
        public MatchThreeElement SelectedElement;

        public bool Processing { get; set; } = false;
        
        public bool Press { get; set; } = false;
        
        #endregion

        #region Unity

        protected override void Start()
        {
            base.Start();
            AudioManager.Instance.StopSound(0, 2f, 1f);
            AudioManager.Instance.PlaySound(1, AudioInstance.Instance.Settings.matchThree, SystemSandbox.Instance.VolumeHandler.Volume);
            CoroutineTaskManager.Instance.WaitSecondTodo(StartGame, 4f);
            CoroutineTaskManager.Instance.WaitSecondTodo(EndGame, life + 4f);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            remainTimeText.text = $"{Math.Clamp((int)(FinishTime - Time.time), 0, int.MaxValue)}";
            if (NextTime < Time.time && FinishTime - Time.time > 0)
            {
                NextTime++;
                var sequence = DOTween.Sequence();
                sequence.Append(remainTimeText.transform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.InCubic));
                sequence.Append(remainTimeText.transform.DOScale(Vector3.one, 0.1f));
                sequence.Play();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ScoreSandbox.OnValueChanged -= OnScoreChanged;
            ScoreSandbox.Reset();
        }

        #endregion

        #region GameModeBase

        protected override void OnInit()
        {
            base.OnInit();
            OneSecond = new WaitForSeconds(1f);
            HalfSecond = new WaitForSeconds(0.5f);
            OnePointSecond = new WaitForSeconds(0.1f);
            SpawnManager.Instance.Init();
            AddRegistryInRouter();

            InitSandbox();
        }

        protected override void OnGameStartInternal()
        {
            base.OnGameStartInternal();

            UIManager.Instance.GetLayer("UIBlackScreen").Hide();
            // 初始化地图
            // ...
            SpawnMatches();
            
            UIStaticsLayer.ShowUIStaticsLayer();
            UIMatchThreeLayer.ShowUIMatchThreeLayer();

            FinishTime = Time.time + life;
            NextTime = Time.time + 1;
            Processing = false;
        }

        protected override void OnGameEndInternal()
        {
            base.OnGameEndInternal();
            var coin = UnityEngine.Random.Range(50, 300);
            var task = new List<IQueueableEvent>
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
                new CustomEvent(() =>
                {
                    UIStaticsLayer.HideUIStaticsLayer();
                    UIMatchThreeLayer.HideUIMatchThreeLayer();
                }),
                new WaitForTrueEvent(() => !Processing),
                new CustomEvent(() =>
                {
                    GameSessionAPI.ChessBoardAPI.Option((response) =>
                    {
                        // score - (100 ~ 300)
                        response.AddBodyParams("score", 200);
                    });
                    UIManager.Instance.GetLayer("UIBlackScreen").Show();
                    SceneManager.Instance.AddSceneToLoadQueueByName(ChessBoardAPI.GetCurrentChessBoard(), 3);
                    SceneManager.Instance.StartLoad();
                })
            };
            EventQueue.Instance.Enqueue(task);
        }

        #endregion

        #region Function - MatchesThree

        /// <summary>
        ///     随机生成列表
        /// </summary>
        /// <param name="designMatches">生成元素的默认种类</param>
        /// <returns>生成后的 map 列表</returns>
        private List<List<string>> GetRandomMatches(HashSet<string> designMatches)
        {
            if (maxRow <= 0 || maxColumn <= 0)
                throw new ArgumentException("maxRow and maxColumn must be greater than 0");

            if (designMatches == null || designMatches.Count == 0)
                throw new ArgumentException("designMatches is null or empty");

            var router = designMatches.ToList();
            var map = new List<List<string>>();

            for (var i = 0; i < maxRow; i++)
            {
                map.Add(new List<string>());
                for (var j = 0; j < maxColumn; j++)
                {
                    string element;
                    do
                    {
                        element = router[UnityEngine.Random.Range(0, router.Count)];
                    } while (IsThreeInARow(map, i, j, element));

                    map[i].Add(element);
                }
            }

            return map;
        }


        /// <summary>
        ///     检查在给定位置添加新元素后，是否会导致三连
        /// </summary>
        /// <param name="map">当前棋盘</param>
        /// <param name="row">行号</param>
        /// <param name="col">列号</param>
        /// <param name="element">待检查的元素</param>
        /// <returns>是否会形成三连</returns>
        private static bool IsThreeInARow(List<List<string>> map, int row, int col, string element)
        {
            // 检查横向三连
            if (col >= 2 &&
                map[row][col - 1] == element &&
                map[row][col - 2] == element)
                return true;

            // 检查纵向三连
            if (row >= 2 &&
                map[row - 1][col] == element &&
                map[row - 2][col] == element)
                return true;

            return false;
        }

        private bool IsNearMatch(Vector2Int from, Vector2Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y) <= 1;
        }

        #endregion

        #region Functions

        private void InitSandbox()
        {
            ScoreSandbox = new Sandbox();
            foreach (var elementName in GetMatchesType())
            {
                ScoreSandbox.SetValue(elementName, 0);
            }

            ScoreSandbox.OnValueChanged += OnScoreChanged;
        }

        private void AddRegistryInRouter()
        {
            MatchesDesign = new Dictionary<string, GameObject>();
            EffectDesign = new Dictionary<string, GameObject>();
            foreach (var element in elements)
            {
                var succeed = MatchesDesign.TryAdd(element.name, element.gameObject);
                if (!succeed) this.LogEditorOnly(element.name + " is already registered");
                
                EffectDesign.Add(element.name, element.effect);
            }
        }

        public HashSet<string> GetMatchesType()
        {
            var matches = new HashSet<string>();
            foreach (var kv in MatchesDesign) matches.Add(kv.Key);
            return matches;
        }

        private void SpawnMatches()
        {
            var designMatches = GetMatchesType();
            var map = GetRandomMatches(designMatches);
            container.RecycleAllGrids();
            container.SelectAction = OnSelectedCallback;
            container.SpawnAllGrids(map);
        }

        #endregion

        #region API

        public GameObject GetMatchThreeElement(string elementName)
        {
            return MatchesDesign.GetValueOrDefault(elementName);
        }

        public GameObject GetMatchThreeEffect(string elementName)
        {
            return EffectDesign.GetValueOrDefault(elementName);
        }

        public IReadOnlyCollection<GameObject> GetMatchThreeElements()
        {
            return MatchesDesign.Values;
        }
        

        #endregion

        #region Callbacks

        private void OnScoreChanged(string elementName, object preScore, object postScore)
        {
            UIMatchThreeLayer.SetScore(elementName, (int)postScore);
        }

        private void OnSelectedCallback(MatchThreeElement element)
        {
            if (IsEnd()) return;

            if (Selects.Count <= 0)
            {
                Selects.Add(element);
            }
            else if(Selects.Count == 1)
            {
                if (Selects[0] == element)
                {
                    element.UnSelect();
                    Selects.Remove(element);
                }
                else
                {
                    Selects.Add(element);
                    StartCoroutine(SwapLogic());
                }
            }
            else
            {
                element.UnSelect();
            }
        }

        private IEnumerator SwapLogic()
        {
            Processing = true;
            Selects[0].UnSelect();
            Selects[1].UnSelect();
            var firstPos = container.GetGridPosition(Selects[0].GetOwner(), maxRow, maxColumn);
            var secondPos = container.GetGridPosition(Selects[1].GetOwner(), maxRow, maxColumn);
            if (!IsNearMatch(firstPos, secondPos))
            {
                Selects.Clear();
                Processing = false;
                yield break;
            }
            
            // 做交换
            yield return container.SwapGrid(Selects[0].GetOwner(), Selects[1].GetOwner());

            // 交换失败
            var preMatch =  container.ProcessMatch(Selects[0].GetOwner(), Selects[1].GetOwner(), maxRow, maxColumn);
            if (!preMatch.MoveNext() || preMatch.Current == null)
            {
                // 做交换
                yield return container.SwapGrid(Selects[0].GetOwner(), Selects[1].GetOwner());
                Selects.Clear();
                Processing = false;
                yield break;
            }

            Selects.Clear();
            
            while (true)
            {
                // 1. 获取当前全局三联格子 Grid[]
                var matchProcess = container.CheckForChains(maxRow, maxColumn);

                // 2. 如果没有三联格子 -------> 返回结束
                if (matchProcess == null || !matchProcess.MoveNext()) break;
                
                // 3. 如果有，做消除
                var allMatches = matchProcess.Current;       
                Debug.Assert(allMatches != null);
                if (allMatches.Count == 0) break;
                foreach (var grid in allMatches)
                {
                    var elementName = (grid as MatchThreeGrid)?.GetName();
                    var score = ScoreSandbox.GetValue<int>(elementName);
                    ScoreSandbox.SetValue(elementName, score + 1);
                    grid.RecycleElement(true);
                }
                SoundAPI.PlaySound(AudioInstance.Instance.Settings.comboFinish);
                
                yield return CoroutineTaskManager.Waits.HalfSecond;

                // 4. 做移动 （只做一层）
                yield return container.FallDown(allMatches, maxRow, maxColumn);
                yield return CoroutineTaskManager.Waits.HalfSecond;
            }

            Processing = false;
            Press = false;
        }

        #endregion
    }
}