using System;
using System.Collections;
using Data;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    public class TuringBarCharacterGameMode : GameMode
    {
        #region Property

        [SerializeField] private bool debugSandbox = false;

        #endregion
        
        #region Unity

        private IEnumerator Start()
        {
            while (!GetGameMode<TuringBarCharacterGameMode>())
            {
                yield return null;
            }

            if (debugSandbox)
            {
                PlayerSandbox.Instance.DebugInitializePlayerSandbox();
            }
            
            yield return null;
            
            UIManager.Instance.GetLayer("UIBlackScreen").Hide();
            UIManager.Instance.GetLayer("UIAgentCharacterLayer").Show();
            
            AudioManager.Instance.PlaySound(0, AudioInstance.Instance.Settings.bgTuringDefault, 0.5f, 1);
        }

        private void OnDestroy()
        {
            AudioManager.Instance.StopSound(0, 2f);
        }

        #endregion
    }
}