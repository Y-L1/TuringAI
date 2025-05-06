using System;
using System.Collections;
using Data;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    public class TuringBarCityGameMode : GameMode
    {
        #region Unity

        private IEnumerator Start()
        {
            while (!GetGameMode<TuringBarCityGameMode>())
            {
                yield return null;
            }
            
            yield return null;
            
            UIManager.Instance.GetLayer("UIBlackScreen").Hide();
            UIJoystickLayer.GetLayer().Show();
            
            AudioManager.Instance.PlaySound(1, AudioInstance.Instance.Settings.bgTuringWind, 1f, 1);
        }

        private void OnDestroy()
        {
            AudioManager.Instance.StopSound(1, 2f);
        }

        #endregion
    }
}