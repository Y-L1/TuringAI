using System;
using System.Collections.Generic;
using DragonLi.Core;
using Game;
using MameshibaGames.Kekos.CharacterEditorScene.Customization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    public class CharacterModelHandle : SandboxHandlerBase
    {
        [System.Serializable]
        public class ColorValue
        {
            public float r;
            public float g;
            public float b;
            public float a;

            public float val;

            public ColorValue() {}

            public ColorValue(Color color, float v)
            {
                r = color.r;
                g = color.g;
                b = color.b;
                a = color.a;
                val = v;
            }

            public Color ToColor()
            {
                return new Color(r, g, b, a);
            }
        }
        
        [System.Serializable]
        public class PartBodyData
        {
            public bool isActive;
            public string partName;
            public int databaseIndex;
            public int colorIndex;
            public int itemIndex;
            
            
            public PartBodyData() { }

            public PartBodyData(string partName, int databaseIndex, int colorIndex, int itemIndex, bool isActive = true)
            {
                SetValue(partName, databaseIndex, colorIndex, itemIndex, isActive);
            }

            public void SetValue(string partName, int databaseIndex, int colorIndex, int itemIndex, bool isActive = true)
            {
                this.partName = partName;
                this.databaseIndex = databaseIndex;
                this.colorIndex = colorIndex;
                this.itemIndex = itemIndex;
                this.isActive = isActive;
            }

            public void SetActive(bool active)
            {
                isActive = active;
            }
        }
        
        [System.Serializable]
        public class HairData : PartBodyData
        {
            public bool withCap;
            public ColorValue color;
            
            public HairData() { }

            public HairData(string partName, int databaseIndex, int colorIndex, int itemIndex, bool withCap, Color color, float val,
                bool isActive = true)
            {
                SetValue(partName, databaseIndex, colorIndex, itemIndex, isActive);
                SetWithCap(withCap);
                SetColor(color, val);
            }

            public void SetWithCap(bool withCap)
            {
                this.withCap = withCap;
            }

            public void SetColor(Color color, float val)
            {
                this.color = new ColorValue(color, val);
            }
        }
        
        
        [System.Serializable]
        public class GlassesData : PartBodyData
        {
            public ColorValue color;
            
            public GlassesData() { }

            public GlassesData(string partName, int databaseIndex, int colorIndex, int itemIndex, Color color, float val,  bool isActive = true)
            {
                SetValue(partName, databaseIndex, colorIndex, itemIndex, isActive);
                SetColor(color, val);
            }
            
            public void SetColor(Color color, float val)
            {
                this.color = new ColorValue(color, val);
            }
        }

        [System.Serializable]
        public class EyesData
        {
            public int databaseIndex;
            public ColorValue color;
            
            public EyesData() { }

            public void SetValue(int database)
            {
                databaseIndex = database;
            }

            public void SetColor(Color color, float val)
            {
                this.color = new ColorValue(color, val);
            }
        }
        
        [System.Serializable]
        public class SkinData
        {
            public int baseIndex;
            public int detailIndex;
            
            public SkinData() { }

            public SkinData(int baseIndex, int detailIndex)
            {
                this.baseIndex = baseIndex;
                this.detailIndex = detailIndex;
            }

            public void SetBase(int index)
            {
                this.baseIndex = index;
            }

            public void SetDetail(int index)
            {
                this.detailIndex = index;
            }
        }

        [System.Serializable]
        public class FaceData : PartBodyData
        {
            public ColorValue color;
            public Dictionary<string, float> characteristics;
            
            public void SetColor(Color col, float val)
            {
                this.color = new ColorValue(col, val);
            }
            

            public void ChangeCharacteristicById(float val, string key)
            {
                characteristics ??= new Dictionary<string, float>();
                characteristics[key] = val;
            }
        }
        
        private const string TorsoKey = "torso-key";
        private const string LegsKey = "legs-key";
        private const string FeetKey = "feet-key";
        private const string HandsKey = "hands-key";
        private const string FullTorsoKey = "full-torso-key";
        private const string TorsoPropKey = "torso-prop-key";
        
        private const string HairKey = "hair-key";
        private const string CapKey = "cap-key";
        private const string GlassesKey = "glasses-key";
        private const string EyesKey = "eyes-key";
        private const string SkinKey = "skin-key";
        private const string FaceKey = "face-key";
        
        private const string SaveCharacterModelKey = "save-character-model-key";

        #region Property - Event

        public event Action<PartBodyData, PartBodyData> OnSaveChanged; 

        #endregion

        #region Property - Data

        public PartBodyData Torso;

        public PartBodyData Legs;

        public PartBodyData Feet;

        public PartBodyData Hands;

        public PartBodyData FullTorso;

        public PartBodyData TorsoProp;

        public HairData Hair;

        public PartBodyData Cap;

        public GlassesData Glasses;

        public EyesData Eyes;

        public SkinData Skin;

        public FaceData Face;

        #endregion

        #region SandboxHandlerBase

        protected override void OnInitSandboxCallbacks(Dictionary<string, Action<object, object>> sandboxCallbacks)
        {
            base.OnInitSandboxCallbacks(sandboxCallbacks);
            if (sandboxCallbacks == null)
            {
                throw new ArgumentNullException(nameof(sandboxCallbacks));
            }
            
            sandboxCallbacks[TorsoKey] = (preValue, newValue) => OnSaveChanged?.Invoke((PartBodyData)preValue, (PartBodyData)newValue);
            sandboxCallbacks[LegsKey] = (preValue, newValue) => OnSaveChanged?.Invoke((PartBodyData)preValue, (PartBodyData)newValue);
            sandboxCallbacks[FeetKey] = (preValue, newValue) => OnSaveChanged?.Invoke((PartBodyData)preValue, (PartBodyData)newValue);
            sandboxCallbacks[HandsKey] = (preValue, newValue) => OnSaveChanged?.Invoke((PartBodyData)preValue, (PartBodyData)newValue);
            sandboxCallbacks[FullTorsoKey] = (preValue, newValue) => OnSaveChanged?.Invoke((PartBodyData)preValue, (PartBodyData)newValue);
            sandboxCallbacks[TorsoPropKey] = (preValue, newValue) => OnSaveChanged?.Invoke((PartBodyData)preValue, (PartBodyData)newValue);
        }

        protected override void OnInit()
        {
            base.OnInit();
            
            Application.quitting += SaveToLocal;
            LoadFromLocal();
        }

        private T GetFromLocal<T>(string key)
        {
            if (PlayerPrefs.HasKey(key))
            {
                var json = PlayerPrefs.GetString(key);
                this.LogEditorOnly($"Load === {json}");
                return JsonConvert.DeserializeObject<T>(json);
            }

            return default;
        }

        private void SaveToLocal<T>(string key, T value)
        {
            var json = JsonConvert.SerializeObject(value);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }


        private void SaveToLocal()
        {
            SaveToLocal<PartBodyData>(TorsoKey, Torso);
            SaveToLocal<PartBodyData>(LegsKey, Legs);
            SaveToLocal<PartBodyData>(FeetKey, Feet);
            SaveToLocal<PartBodyData>(HandsKey, Hands);
            SaveToLocal<PartBodyData>(FullTorsoKey, FullTorso);
            SaveToLocal<PartBodyData>(TorsoPropKey, TorsoProp);
            SaveToLocal<HairData>(HairKey, Hair);
            SaveToLocal<PartBodyData>(CapKey, Cap);
            SaveToLocal<GlassesData>(GlassesKey, Glasses);
            SaveToLocal<EyesData>(EyesKey, Eyes);
            SaveToLocal<SkinData>(SkinKey, Skin);
            SaveToLocal<PartBodyData>(FaceKey, Face);
        }

        private void LoadFromLocal()
        {
            Torso = GetFromLocal<PartBodyData>(TorsoKey);
            Legs = GetFromLocal<PartBodyData>(LegsKey);
            Feet = GetFromLocal<PartBodyData>(FeetKey);
            Hands = GetFromLocal<PartBodyData>(HandsKey);
            FullTorso = GetFromLocal<PartBodyData>(FullTorsoKey);
            TorsoProp = GetFromLocal<PartBodyData>(TorsoPropKey);
            
            Hair = GetFromLocal<HairData>(HairKey);
            Cap = GetFromLocal<PartBodyData>(CapKey);
            Glasses = GetFromLocal<GlassesData>(GlassesKey);
            Eyes = GetFromLocal<EyesData>(EyesKey);
            Skin = GetFromLocal<SkinData>(SkinKey);
            Face = GetFromLocal<FaceData>(FaceKey);
        }
        
        
        #endregion
    }
}