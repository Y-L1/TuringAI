using System.Collections.Generic;
using Data;
using DragonLi.Core;
using DragonLi.Frame;
using DragonLi.Network;
using UnityEngine;

namespace Game
{
    public class ChessTileCompany : ChessTile
    {
        #region Fields

        [Header("References")] 
        [SerializeField] private GameObject CashEffectObect;

        #endregion

        #region Prperties

        private bool ReceiveArriveMessage { get; set; }
        
        private int Coin  { get; set; }

        #endregion

        #region ChessTile

        public override List<IQueueableEvent> OnArrive()
        {
            GameSessionAPI.ChessBoardAPI.Arrive();
            World.GetPlayer<GameCharacter>()?.GetCharacterAnimatorInterface().Happy();
            return new List<IQueueableEvent>
            {
                new WaitForTrueEvent(() => ReceiveArriveMessage),
                new CustomEvent(() => { ReceiveArriveMessage = false; }),
                new GameObjectVisibilityEvent(CashEffectObect),
                new ModifyNumWSEffectEvent(transform.position, EffectInstance.Instance.Settings.uiEffectCoinNumber, () => Coin),
                new CustomEvent(() => PlayerSandbox.Instance.CharacterHandler.Coin += Coin ),
            };
        }

        #endregion

        #region Callback

        protected override void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            base.OnReceiveMessage(response, service, method);
            if (PlayerSandbox.Instance.ChessBoardHandler.StandIndex != TileIndex) return;
            if (!response.IsSuccess()) return;
            if (service != GameSessionAPI.ChessBoardAPI.ServiceName || method != GSChessBoardAPI.MethodArrive) return;
            if(response.GetAttachmentAsString("tile") != "company") return;
            ReceiveArriveMessage = true;
            Coin = response.GetAttachmentAsInt("coin");
        }
        #endregion

    }

}