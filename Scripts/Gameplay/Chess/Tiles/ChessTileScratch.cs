using System.Collections.Generic;
using _Scripts.Gameplay.MiniGame.Scratch;
using Data;
using DragonLi.Core;
using DragonLi.Frame;
using DragonLi.Network;
using DragonLi.UI;
using Newtonsoft.Json;
using UnityEngine;

namespace Game
{
    public class ChessTileScratch : ChessTile
    {
        #region Fields

        [Header("References")] 
        [SerializeField] private GameObject RibbonsMajorEffectObject;

        #endregion

        #region Properties

        private bool ReceiveArriveMessage { get; set; } = false;
        
        private int Coin  { get; set; }
        
        private List<int> Results { get; set; }

        #endregion

        #region ChessTile

        public override List<IQueueableEvent> OnArrive()
        {
            GameSessionAPI.ChessBoardAPI.Arrive();
            return new List<IQueueableEvent>
            { 
                new WaitForTrueEvent(() => ReceiveArriveMessage),
                new CustomEvent(() => { ReceiveArriveMessage = false; }),
                new CustomEvent(() =>
                {
                    UIStaticsLayer.HideUIStaticsLayer();
                    UIActivityLayer.HideUIActivityLayer();
                    UIChessboardLayer.HideLayer();
                }),
                new GameObjectVisibilityEvent(RibbonsMajorEffectObject),
                new CustomEvent(() =>
                {
                   SoundAPI.PlaySound(AudioInstance.Instance.Settings.goodBig); 
                }),
                new WaitForSecondEvent(1),
                new CustomEvent(() =>
                {
                    ScratchGameMode.SetData(Results, Coin);
                    UIManager.Instance.GetLayer("UIBlackScreen").Show();
                    SceneManager.Instance.AddSceneToLoadQueueByName("ScratchScene", 1);
                    SceneManager.Instance.StartLoad();
                })
            };
        }

        public override void PlayArriveAnimation(EArriveAnimationType animationType, int playerTileIndex)
        {
            base.PlayArriveAnimation(animationType, playerTileIndex);
            World.GetPlayer<GameCharacter>()?.GetCharacterAnimatorInterface().Idle();
        }

        #endregion

        #region Callbacks

        protected override void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            if (PlayerSandbox.Instance.ChessBoardHandler.StandIndex != TileIndex) return;
            if (!response.IsSuccess()) return;
            if (service != GameSessionAPI.ChessBoardAPI.ServiceName || method != GSChessBoardAPI.MethodArrive) return;
            if(response.GetAttachmentAsString("tile") != "scratch") return;
            ReceiveArriveMessage = true;
            Coin = response.GetAttachmentAsInt("coin");
            response.body.TryGetValue("results", out var results);
            Results = JsonConvert.DeserializeObject<List<int>>(JsonConvert.SerializeObject(results));
        }

        #endregion
    }
}
