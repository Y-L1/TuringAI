using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game
{
    public class JoyStickHandle : MonoBehaviour
    {
        #region Property

        private RectTransform Stick { get; set; }

        #endregion

        #region Unity

        private void Awake()
        {
            Stick = GetComponent<RectTransform>();
        }

        #endregion
        
        #region API

        public RectTransform GetStick() { return Stick; }

        #endregion
    }
}