using System.Collections.Generic;
using Data;
using DragonLi.Core;
using DragonLi.Network;
using UnityEngine;

namespace Game
{
    public class ChessTileSpecialEvent : ChessTile
    {
        #region Properties

        private bool ReceiveArriveMessage { get; set; } = false;

        #endregion

        #region ChessTile

        public override List<IQueueableEvent> OnArrive()
        {
            GameSessionAPI.ChessBoardAPI.Arrive();
            return new List<IQueueableEvent>
            {
                new WaitForTrueEvent(() => ReceiveArriveMessage),
                new CustomEvent(() => { ReceiveArriveMessage = false; }),
            };
        }

        protected override void OnReceiveMessage(HttpResponseProtocol response, string service, string method)
        {
            base.OnReceiveMessage(response, service, method);
            if (PlayerSandbox.Instance.ChessBoardHandler.StandIndex != TileIndex) return;
            if (!response.IsSuccess()) return;
            if (service != GameSessionAPI.ChessBoardAPI.ServiceName || method != GSChessBoardAPI.MethodArrive) return;
            if(response.GetAttachmentAsString("tile") != "event") return;

            ReceiveArriveMessage = true;
        }

        #endregion
    }
}