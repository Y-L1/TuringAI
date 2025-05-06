using DG.Tweening;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    public class UIWSScreenEffectTip : UIWorldElement
    {
        [Header("Settings")] 
        [SerializeField] private RectTransform animatedRoot;
        [SerializeField] private RectTransform animatedBackground;
        [SerializeField] private UIAnimatedNumberText animatedNumber;

        public void Play(int number)
        {
            animatedNumber.SetNumberDirectly(0);
            animatedRoot.localScale = Vector3.zero;
            animatedRoot.DOScale(Vector3.one, 0.35f)
                .SetEase(Ease.OutBounce)
                .SetDelay(0.2f)
                .onComplete = () =>
            {
                animatedNumber.SetNumber(number);
            };
            
            var originalHeight = animatedBackground.sizeDelta.y;
            animatedBackground.sizeDelta = new Vector2(1, originalHeight);
            animatedBackground.DOSizeDelta(new Vector2(1500, originalHeight), 0.35f).SetEase(Ease.OutCubic);
        }
    }
}


