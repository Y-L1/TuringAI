using System;
using DG.Tweening;
using DragonLi.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    [RequireComponent(typeof(RecordComponent))]
    public class HoldProgressButton : UIComponent, IPointerDownHandler, IPointerUpHandler
    {

        #region Property

        [SerializeField] private Image bar;

        [SerializeField] private RecordComponent recordComponent;
        
        public RecordComponent RecordComponent => recordComponent;
        private bool Press { get; set; }
        private float Timer  { get; set; }
        
        public event Action<HoldProgressButton> Pressed;
        public event Action<HoldProgressButton> Released;
        
        #endregion

        private void Update()
        {
            if (Press)
            {
                SetFillAmount((Time.unscaledTime - Timer) / recordComponent.Duration);
            }
        }

        #region UIComponent

        protected override void OnInit()
        {
            base.OnInit();
            Press = false;
        }

        public override void OnShow()
        {
            base.OnShow();
            SetFillAmount(0);
        }

        #endregion

        private void SetFillAmount(float value)
        {
            if(!bar) return;
            bar.fillAmount = value;
        }
        
        
        public void OnPointerDown(PointerEventData eventData)
        {
            // if(Press) return;
            // Press = true;
            // recordComponent.StartRecording();
            // Timer = Time.unscaledTime;
            // transform.DOScale(Vector3.one * 1.2f, 0.5f ).SetEase(Ease.OutBounce);
            // Pressed?.Invoke(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // if(!Press) return;
            // Press = false;
            // recordComponent.StopRecording();
            // SetFillAmount(0f);
            // transform.DOScale(Vector3.one, 0.5f ).SetEase(Ease.OutBounce);
            // Released?.Invoke(this);
        }
    }

}