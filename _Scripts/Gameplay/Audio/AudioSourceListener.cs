using System;
using System.Collections;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceListener : MonoBehaviour
    {
        #region Property

        public event Action<AudioSource> OnAudioStarted;
        public event Action<AudioSource> OnAudioEnded;
        
        private AudioSource audioSource;
        private Coroutine playingCoroutine;

        #endregion

        #region Unity

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        #endregion

        #region API

        public void StopWithListener()
        {
            if (audioSource == null || !audioSource.isPlaying) return;
            
            if (playingCoroutine != null) StopCoroutine(playingCoroutine);
            audioSource.Stop();
            OnAudioEnded?.Invoke(audioSource);
        }

        public void PlayWithListener(AudioClip clip = null)
        {
            if (audioSource == null) return;

            // 通知开始播放
            OnAudioStarted?.Invoke(audioSource);

            // 开始播放
            if(audioSource && !audioSource.isPlaying) StopWithListener();
            if (clip != null)
            {
                audioSource.clip = clip;
            }
            audioSource.Play();

            // 启动监听协程
            if (playingCoroutine != null) StopCoroutine(playingCoroutine);
            playingCoroutine = StartCoroutine(CheckAudioEnd());
        }

        #endregion

        #region Function

        private IEnumerator CheckAudioEnd()
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            OnAudioEnded?.Invoke(audioSource);
        }

        #endregion
    }
}