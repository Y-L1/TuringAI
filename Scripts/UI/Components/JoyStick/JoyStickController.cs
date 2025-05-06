using DragonLi.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class JoyStickController : UIComponent
    {
        #region Property

        [Header("Settings")]
        [SerializeField] private float distance = 100f;

        [Header("References")]
        [SerializeField] private PointerEventComponent pointerEvent;
        [SerializeField] private JoyStickHandle handle;
        [SerializeField] private RectTransform background;
        
        public Vector2 Direction { get; private set; }

        private RectTransform stick => handle.GetStick();
        
        private Vector2 _startPos;
        
        public JoyStickHandle Handle => handle;

        #endregion

        #region UIComponent

        protected override void OnInit()
        {
            base.OnInit();
            pointerEvent.OnPointerDownHandler += OnPointerDownCallback;
            pointerEvent.OnPointerUpHandler += OnPointerUpCallback;
            pointerEvent.OnPointerDragHandler += OnPointerDragCallback;
        }
        
        public override void OnHide()
        {
            base.OnHide();
            pointerEvent.OnPointerDownHandler -= OnPointerDownCallback;
            pointerEvent.OnPointerUpHandler -= OnPointerUpCallback;
            pointerEvent.OnPointerDragHandler -= OnPointerDragCallback;
        }

        #endregion

        #region Callbacks

        private void OnPointerDownCallback(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out _startPos);
        }

        private void OnPointerUpCallback(PointerEventData eventData)
        {
            stick.anchoredPosition = Vector2.zero;
            Direction = Vector2.zero;
        }
        
        private void OnPointerDragCallback(PointerEventData eventData)
        {
            if(!pointerEvent.IsPressed) return;
            // Vector2 currentPos;
            // if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //         background, eventData.position, eventData.pressEventCamera, out currentPos))
            // {
            //     Vector2 offset = currentPos - _startPos;
            //     Vector2 clampedOffset = Vector2.ClampMagnitude(offset, distance);
            //     stick.anchoredPosition = clampedOffset;
            //     Direction = clampedOffset.normalized * -1f;
            // }
            
            Vector2 pos;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    background, eventData.position, eventData.pressEventCamera, out pos))
            {
                Vector2 offset = pos - _startPos;
                Vector2 clampedOffset = Vector2.ClampMagnitude(offset, distance);

                stick.anchoredPosition = clampedOffset;
                Direction = clampedOffset.normalized * -1f;
            }
        }

        #endregion
    }
}