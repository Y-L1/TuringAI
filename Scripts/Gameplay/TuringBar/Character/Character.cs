using System;
using Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class Character : MonoBehaviour
    {

        #region Property

        [Header("Settings")]
        [SerializeField] private float volume = 0.125f;
        private int StepLength { get; set; }

        #endregion

        #region Unity

        private void Awake()
        {
            StepLength = AudioInstance.Instance.Settings.footstep.Length;
        }

        #endregion
        
        public void FootStep()
        {
            var clip = AudioInstance.Instance.Settings.footstep[Random.Range(0, StepLength)];

            var audioSource = SoundAPI.PlaySound(clip);
            audioSource.volume = volume;
        }
    }
}