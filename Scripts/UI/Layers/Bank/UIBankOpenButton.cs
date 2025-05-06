using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class UIBankOpenButton : MonoBehaviour
    {
        #region Properties

        [Header("References")]
        [SerializeField] private Button btnOpen;

        #endregion

        #region Unity

        private void Awake()
        {
            Debug.Assert(btnOpen);
            btnOpen.onClick.AddListener(OnOpenClick);
        }

        #endregion

        #region Function

        private static void OnOpenClick()
        {
            UIBankLayer.ShowLayer();
        }

        #endregion
    }
}