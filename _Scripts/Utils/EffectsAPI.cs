using System;
using System.Collections.Generic;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    public static class EffectsAPI
    {
        public enum EEffectType
        {
            None = 0,
            Coin,
            Dice,
            Token,
        }
        
        public enum EEffectSizeType
        {
            None = 0,
            Small,
            Medium,
            Big,
        }
        
        public static Vector3 GetScreenCenterWorldPosition()
        {
            var screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 10);
            return Camera.main.ScreenToWorldPoint(screenCenter);
        }

        public static IQueueableEvent CreateTip(Func<EEffectType> getType, Func<int> getNum)
        {
            if(getType() == EEffectType.None || getNum() <= 0) return null;
            return new CustomEvent(() =>
            {
                var layer = UIManager.Instance.GetLayer<UIWorldElementLayer>("UIWorldElementLayer");
                if (layer == null)
                {
                    Debug.LogError($"The world element layer is null, please add UIWorldElementLayer!");
                    return;
                }
                var tip = layer.SpawnWorldElement<UIWSScreenEffectTip>(getType() switch
                {
                    EEffectType.Coin => EffectInstance.Instance.Settings.tipCoin,
                    EEffectType.Dice => EffectInstance.Instance.Settings.tipDice,
                    EEffectType.Token => EffectInstance.Instance.Settings.tipToken,
                    _ => throw new ArgumentOutOfRangeException()
                }, EffectsAPI.GetScreenCenterWorldPosition());
                tip.Play(getNum());
            });
        }
        public static IQueueableEvent CreateScreenFullEffect(Func<EEffectType> getType, Func<EEffectSizeType> getSizeType)
        {
            if(getType() == EEffectType.None) return null;
            return new PlayFullscreenEffectEvent(() =>
            {
                return getType() switch
                {
                    EEffectType.Coin => getSizeType() switch
                    {
                        EEffectSizeType.Small => EffectInstance.Instance.Settings.vfxCash2DSmall,
                        EEffectSizeType.Medium => EffectInstance.Instance.Settings.vfxCash2DMedium,
                        EEffectSizeType.Big => EffectInstance.Instance.Settings.vfxCash2DBig,
                        EEffectSizeType.None => null,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    EEffectType.Dice => getSizeType() switch
                    {
                        EEffectSizeType.Small => EffectInstance.Instance.Settings.vfxDice2DSmall,
                        EEffectSizeType.Medium => EffectInstance.Instance.Settings.vfxDice2DMedium,
                        EEffectSizeType.Big => EffectInstance.Instance.Settings.vfxDice2DBig,
                        EEffectSizeType.None => null,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    EEffectType.Token => getSizeType() switch
                    {
                        EEffectSizeType.Small => EffectInstance.Instance.Settings.vfxToken2DSmall,
                        EEffectSizeType.Medium => EffectInstance.Instance.Settings.vfxToken2DMedium,
                        EEffectSizeType.Big => EffectInstance.Instance.Settings.vfxToken2DBig,
                        EEffectSizeType.None => null,
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    EEffectType.None => null,
                    _ => throw new ArgumentOutOfRangeException()
                };
            });
        }

        public static IQueueableEvent CreateSoundEffect(Func<EEffectType> getType)
        {
            return new CustomEvent(() =>
            {
                if(getType() == EEffectType.None) return;
                SoundAPI.PlaySound(getType() switch
                {
                    EEffectType.None => null,
                    EEffectType.Coin => AudioInstance.Instance.Settings.moneyGain,
                    EEffectType.Dice => AudioInstance.Instance.Settings.diceGain,
                    EEffectType.Token => AudioInstance.Instance.Settings.moneyGain,
                    _ => throw new ArgumentOutOfRangeException()
                });
            });
        }
    }
}
