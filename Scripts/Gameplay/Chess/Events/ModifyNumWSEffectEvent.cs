using System;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    public class ModifyNumWSEffectEvent : IQueueableEvent
    {
        #region Properties
        
        private Vector3 Target { get; set; }
        private UIWorldElement WSPrefab { get; set; } 
        private Func<int> Reward { get; }

        #endregion
        
        public ModifyNumWSEffectEvent(Vector3 target, UIWorldElement wsPrefab, Func<int> coin)
        {
            Target = target;
            WSPrefab = wsPrefab;
            Reward = coin;
        }

        public virtual void OnQueue() {}
        public void OnExecute()
        {
            var layer = UIManager.Instance.GetLayer<UIWorldElementLayer>("UIWorldElementLayer");
            var wsCoin = layer.SpawnWorldElement<UIWSCoinNumber>(WSPrefab, Target + Vector3.up * 3.5f);
            wsCoin.SetCoinNumber(Reward());
        }
        public virtual void OnDequeue() {}
        public virtual void OnCancel() {}
        public virtual bool OnTick() { return true; }
        public virtual void OnFinish() {}

    }
}