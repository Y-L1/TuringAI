using System;
using System.IO;
using HuggingFace.API;
using UnityEngine;
using UnityEngine.Events;

namespace Game
{
    public class RecordComponent : MonoBehaviour
    {
        #region Property
        [SerializeField] private int duration = 10;

        private AudioClip Clip { get; set; }
        private byte[] Bytes { get; set; }
        private bool IsRecording { get; set; }

        public int Duration => duration;
        public UnityEvent<bool, string> RecordResultEvent { get; } = new();
        
        #endregion
        
        #region Unity

        private void Update()
        {
            if (IsRecording && Microphone.GetPosition(null) >= Clip.samples)
            {
                StopRecording();
            }
        }

        #endregion

        #region Function

        private byte[] EncodeAsWAV(float[] samples, int frequency, int channels) {
            using (var memoryStream = new MemoryStream(44 + samples.Length * 2)) {
                using (var writer = new BinaryWriter(memoryStream)) {
                    writer.Write("RIFF".ToCharArray());
                    writer.Write(36 + samples.Length * 2);
                    writer.Write("WAVE".ToCharArray());
                    writer.Write("fmt ".ToCharArray());
                    writer.Write(16);
                    writer.Write((ushort)1);
                    writer.Write((ushort)channels);
                    writer.Write(frequency);
                    writer.Write(frequency * channels * 2);
                    writer.Write((ushort)(channels * 2));
                    writer.Write((ushort)16);
                    writer.Write("data".ToCharArray());
                    writer.Write(samples.Length * 2);

                    foreach (var sample in samples) {
                        writer.Write((short)(sample * short.MaxValue));
                    }
                }
                return memoryStream.ToArray();
            }
        }
        
        private void SendRecording() {
            HuggingFaceAPI.AutomaticSpeechRecognition(Bytes, text => {
                RecordResultEvent.Invoke(true, text);
                RecordResultEvent?.RemoveAllListeners();
            }, error => {
                RecordResultEvent.Invoke(false, error);
                RecordResultEvent?.RemoveAllListeners();
            });
        }

        #endregion

        #region API

        public void StartRecording()
        {
            if (IsRecording) return;
            IsRecording = true;
            this.LogEditorOnly("开始录音。。。");
            Clip = Microphone.Start(null, false, duration, 44100);
        }
        
        public void StopRecording()
        {
            if(!IsRecording) return;
            IsRecording = false;
            this.LogEditorOnly("结束录音。。。");
            var position = Microphone.GetPosition(null);
            Microphone.End(null);
            var samples = new float[position * Clip.channels];
            Clip.GetData(samples, 0);
            Bytes = EncodeAsWAV(samples, Clip.frequency, Clip.channels);
            SendRecording();
        }

        #endregion
    }
}