using System;
using System.Collections;
using DragonLi.Core;
using DragonLi.UI;
using UnityEngine;

namespace Game
{
    public class CharacterSelection : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private IEnumerator Start()
        {
            yield return CoroutineTaskManager.Waits.QuarterSecond;
            UIManager.Instance.GetLayer("UIBlackScreen").Hide();
            yield return CoroutineTaskManager.Waits.HalfSecond;
            UIManager.Instance.GetLayer("UICharacterSelectionLayer").Show();
        }
    }
}


