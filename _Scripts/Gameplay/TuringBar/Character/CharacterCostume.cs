using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using DragonLi.Core;
using UnityEngine;
using MameshibaGames.Common.Helpers;
using MameshibaGames.Kekos.CharacterEditorScene.Customization;
using Newtonsoft.Json;
using UnityEngine.Events;
using WebSocketSharp;

namespace Game
{
    public class CharacterCostume : MonoBehaviour
    {
        private const string _MaterialPrefix = "MAT_";
        private const string _HeadModelName = "Bip001 Head";
        
        private static readonly int _DetailAlbedoMap = Shader.PropertyToID("_DetailAlbedoMap");
        private static readonly int _Emission = Shader.PropertyToID("_EmissionMap");
        private static readonly int _DetailMask = Shader.PropertyToID("_DetailMask");
        private static readonly int _MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int _URPMainTex = Shader.PropertyToID("_BaseMap");
        private static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");
        private static readonly int _HDRPEmissionColor = Shader.PropertyToID("_EmissionColorHDRP");
        
        #region Property

        [Header("Settings")] 
        [SerializeField] public bool loadAwake;
        [SerializeField] public bool saveDestroy;
        [SerializeField] public GameObject body;
        
        [Header("Settings - Body")]
        [SerializeField] private string torsoPrefix = "TOR_";
        [SerializeField] private string legsPrefix = "LEG_";
        [SerializeField] private string feetPrefix = "FEE_";
        [SerializeField] private string handsPrefix = "HAN_";
        [SerializeField] private string fullTorsoPrefix = "FTOR_";
        [SerializeField] private string torsoPropsPrefix = "TORP_";
        
        [Header("Settings - Head")]
        [SerializeField] private string hairPrefix = "HAI_";
        [SerializeField] private string capePrefix = "CAP_";
        [SerializeField] private string glassesPrefix = "FACP_";
        [SerializeField] private string eyesPrefix = "";
        [SerializeField] private string skinPrefix = "";
        [SerializeField] private string facePrefix = "FACP_";
        
        private Renderer _skinRenderer;
        private Material _skinMaterial;
        
        private Texture _defaultSkinDetail;
        private Texture _defaultSkinDetailMask;
        private Texture _defaultMainSkin;
        private Texture _defaultURPMainSkin;
        private Texture _defaultEmissive;
        private Color _defaultEmissionColor;
        private Color _defaultHDRPEmissionColor;
        
        private Transform characterHead;
        
        private PartDatabase TorsoDatabase => TuringCharacterInstance.Instance.Settings.torso;
        
        #endregion

        #region Property - Eye

        private static readonly int _Color = Shader.PropertyToID("_Color");
        private static readonly int _URPColor = Shader.PropertyToID("_BaseColor");
        private static readonly int _HDRPMainTex = Shader.PropertyToID("_BaseColorMap");
        
        #endregion

        #region Unity

        private void Awake()
        {
            characterHead = transform.RecursiveFindChild(_HeadModelName);

            if (loadAwake)
            {
                LoadCharacterBody();
            }
        }

        private IEnumerator Start()
        {
            yield return null;
            
            yield return CoroutineTaskManager.Waits.OneSecond;
            PlayerSandbox.Instance.CharacterModelHandle.OnSaveChanged += OnSaveChanged;
        }

        private void OnDestroy()
        {
            PlayerSandbox.Instance.CharacterModelHandle.OnSaveChanged -= OnSaveChanged;
        }

        #endregion

        #region Function
        
        private void ChangeSkinBase(SkinBaseData skinBaseData)
        {
            _skinMaterial.SetTextureIfHasProperty(_MainTex, skinBaseData.baseColor);
            _skinMaterial.SetTextureIfHasProperty(_URPMainTex, skinBaseData.baseColor);
        }
        
        private void ChangeSkinDetail(SkinDetailData skinDetailData)
        {
            _skinMaterial.SetTextureIfHasProperty(_DetailMask, skinDetailData.detailMask);
            _skinMaterial.SetTextureIfHasProperty(_DetailAlbedoMap, skinDetailData.detailBase);
            _skinMaterial.EnableKeyword("_Emission");
            _skinMaterial.EnableKeyword("_DETAIL_MULX2");
            _skinMaterial.SetTextureIfHasProperty(_Emission, skinDetailData.emissive);
            _skinMaterial.SetColorIfHasProperty(_EmissionColor, skinDetailData.emissiveColor);
            _skinMaterial.SetColorIfHasProperty(_HDRPEmissionColor, skinDetailData.emissiveColor);
        }
        
        private IEnumerable<GameObject> GetItemsList(GameObject kid, string prefix)
        {
            var filteredItemPieces = kid.transform.All(x => x.gameObject.name.StartsWith(prefix));
            return filteredItemPieces.Select(itemPiece => itemPiece.gameObject);
        }
        
        private IEnumerable<GameObject> GetItemsList(PartDatabase database)
        {
            PartObjectDatabase partDatabaseObject = (PartObjectDatabase)database;
            return partDatabaseObject.itemObjects.Select(x => x.itemSubobjects[0]);
        }
        
        private string GetSpriteName(string itemName, string partPrefix)
        {
            if (itemName.IsNullOrEmpty()) return $"{_MaterialPrefix}{partPrefix}Empty";
                
            string[] itemNameParts = itemName.Split('_');

            string spriteName = $"{_MaterialPrefix}{partPrefix}{itemNameParts[2]}";
            if (itemNameParts.Length > 3)
                spriteName += $"_{itemNameParts[3]}";
            return spriteName;
        }

        private Renderer LoadPartBase(string partName, Material mat, IEnumerable<GameObject> items, bool active = true)
        {
            foreach (var obj in items)
            {
                obj.SetActive(active && partName == obj.name);
                if (partName == obj.name)
                {
                    var currentRenderer = obj.GetComponent<Renderer>();
                    if(currentRenderer == null) continue;
                    
                    Material[] materials = currentRenderer.materials;
                    if (materials.Length == 1)
                        materials[0] = mat;
                    else if (materials.Length > 1)
                        materials[1] = mat;
                    currentRenderer.materials = materials;

                    return currentRenderer;
                }
            }
            
            return null;
        }

        private void LoadTorso()
        {
            var database = TuringCharacterInstance.Instance.Settings.torso;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Torso;
            if(data == null) return;
            var spriteName = GetSpriteName(data.partName, torsoPrefix);
            
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if (itemsWithColors == null || itemsWithColors.Count <= 0) return;
            LoadPartBase(data.partName, itemsWithColors[data.colorIndex].primaryMaterial, GetItemsList(body, torsoPrefix), data.isActive);
        }

        private void LoadLegs()
        {
            var database = TuringCharacterInstance.Instance.Settings.legs;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Legs;
            if(data == null) return;
            var spriteName = GetSpriteName(data.partName, legsPrefix);
            
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if (itemsWithColors == null || itemsWithColors.Count <= 0) return;
            LoadPartBase(data.partName, itemsWithColors[data.colorIndex].primaryMaterial, GetItemsList(body, legsPrefix), data.isActive);
        }

        private void LoadFeet()
        {
            var database = TuringCharacterInstance.Instance.Settings.feet;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Feet;
            if(data == null) return;
            var spriteName = GetSpriteName(data.partName, feetPrefix);
            
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if (itemsWithColors == null || itemsWithColors.Count <= 0) return;
            LoadPartBase(data.partName, itemsWithColors[data.colorIndex].primaryMaterial, GetItemsList(body, feetPrefix), data.isActive);
        }

        private void LoadHands()
        {
            var database = TuringCharacterInstance.Instance.Settings.hands;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Hands;
            if(data == null) return;
            var spriteName = GetSpriteName(data.partName, handsPrefix);
            
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if (itemsWithColors == null || itemsWithColors.Count <= 0) return;
            LoadPartBase(data.partName, itemsWithColors[data.colorIndex].primaryMaterial, GetItemsList(body, handsPrefix), data.isActive);
        }

        private void LoadFullTorso()
        {
            var database = TuringCharacterInstance.Instance.Settings.fullTorso;
            var data = PlayerSandbox.Instance.CharacterModelHandle.FullTorso;
            if(data == null) return;
            var spriteName = GetSpriteName(data.partName, fullTorsoPrefix);
            
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if (itemsWithColors == null || itemsWithColors.Count <= 0) return;
            LoadPartBase(data.partName, itemsWithColors[data.colorIndex].primaryMaterial, GetItemsList(body, fullTorsoPrefix), data.isActive);
        }

        private void LoadTorsoProp()
        {
            var database = TuringCharacterInstance.Instance.Settings.torsoProps;
            var data = PlayerSandbox.Instance.CharacterModelHandle.TorsoProp;
            if(data == null) return;
            var spriteName = GetSpriteName(data.partName, torsoPropsPrefix);
            
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if (itemsWithColors == null || itemsWithColors.Count <= 0) return;
            LoadPartBase(data.partName, itemsWithColors[data.colorIndex].primaryMaterial, GetItemsList(body, torsoPropsPrefix), data.isActive);
        }

        private void LoadFace()
        {
            var database = TuringCharacterInstance.Instance.Settings.face;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Face;
            if(data == null) return;
            var spriteName = GetSpriteName(data.partName, facePrefix);
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if (itemsWithColors == null || itemsWithColors.Count <= 0) return;
            var currentRenderer = LoadPartBase(data.partName, itemsWithColors[data.colorIndex].primaryMaterial, GetItemsList(body, facePrefix), data.isActive);
            ChangeColor(data.color.ToColor(), currentRenderer);
            
        }

        public Renderer LoadExtra(GameObject prefab, Material mat)
        {
            if (prefab == null) return null;
            var obj = Instantiate( prefab, characterHead);
            obj.SetActive(true);
            obj.transform.ResetTransform();
            var currentRenderer = obj.GetComponent<Renderer>();
            if(currentRenderer == null) return null;
            
            Material[] sharedMaterials = currentRenderer.sharedMaterials;
            sharedMaterials[0] = mat;
            currentRenderer.sharedMaterials = sharedMaterials;
            
            // Material[] sharedMaterials = currentRenderer.sharedMaterials;
            // if (sharedMaterials.Length == 1)
            //     sharedMaterials[0] = mat;
            // else if (sharedMaterials.Length > 1)
            //     sharedMaterials[1] = mat;
            // currentRenderer.sharedMaterials = sharedMaterials;

            return currentRenderer;
        }

        private void LoadHair()
        {
            var database = TuringCharacterInstance.Instance.Settings.hair;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Hair;
            var subObject = data.withCap
                ? database.itemObjects[data.databaseIndex].itemSubobjects[1]
                : database.itemObjects[data.databaseIndex].itemSubobjects[0];
            
            var spriteName = GetSpriteName(data.partName, hairPrefix);
            
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if (itemsWithColors == null || itemsWithColors.Count <= 0) return;
            var currentRenderer = LoadExtra(subObject, itemsWithColors[data.colorIndex].primaryMaterial);
            ChangeColor(data.color.ToColor(), currentRenderer);
        }

        private void LoadCap()
        {
            var database = TuringCharacterInstance.Instance.Settings.cap;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Cap;
            if(data == null) return;
            var subObject = database.itemObjects[data.databaseIndex].itemSubobjects[0];
            
            var spriteName = GetSpriteName(data.partName, capePrefix);
            
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if (itemsWithColors == null || itemsWithColors.Count <= 0) return;
            LoadExtra(subObject, itemsWithColors[data.colorIndex].primaryMaterial);
        }

        private void LoadGlasses()
        {
            var database = TuringCharacterInstance.Instance.Settings.glasses;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Glasses;
            if(data == null) return;
            var subObject = database.itemObjects[data.databaseIndex].itemSubobjects[0];
            var spriteName = GetSpriteName(data.partName, glassesPrefix);
            
            ItemDatabase itemDatabase = database.itemsDatabases[data.databaseIndex];
            List<ItemDatabase.ItemInfo> itemsWithColors = itemDatabase.GetSpriteNameStartsWith(spriteName);
            if(itemsWithColors.Count <= 0) return;
            var currentRenderer = LoadExtra(subObject, itemsWithColors[data.colorIndex].primaryMaterial);
            ChangeColor(data.color.ToColor(), currentRenderer);
        }

        private void LoadEyes()
        {
            var database = TuringCharacterInstance.Instance.Settings.eyes;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Eyes;
            if(data == null) return;
            EyesData eyesData = database.eyesData[data.databaseIndex];
            
            Renderer eyesRenderer = transform.RecursiveFindChild("HEAD").GetComponent<Renderer>();
            Material[] sharedMaterials = eyesRenderer.sharedMaterials;
            var _eyeBaseMaterial = sharedMaterials[1];
            var _lightMaterial = sharedMaterials[2];
            
            _eyeBaseMaterial.SetTextureIfHasProperty(_MainTex, eyesData.baseColor);
            _eyeBaseMaterial.SetTextureIfHasProperty(_URPMainTex, eyesData.baseColor);
            _eyeBaseMaterial.SetTextureIfHasProperty(_HDRPMainTex, eyesData.baseColor);
            _lightMaterial.SetTextureIfHasProperty(_MainTex, eyesData.light);
            _lightMaterial.SetTextureIfHasProperty(_URPMainTex, eyesData.light);
            _lightMaterial.SetTextureIfHasProperty(_HDRPMainTex, eyesData.light);
            
            _eyeBaseMaterial.SetColorIfHasProperty(_Color, data.color.ToColor());
            _eyeBaseMaterial.SetColorIfHasProperty(_URPColor, data.color.ToColor());
        }

        private void LoadSkin()
        {
            var database = TuringCharacterInstance.Instance.Settings.skin;
            var data = PlayerSandbox.Instance.CharacterModelHandle.Skin;
            if(data == null) return;
            SkinBaseData skinData = database.skinBaseData[data.baseIndex];
            SkinDetailData skinDetailData = database.skinDetailData[data.detailIndex];
            
            _skinRenderer = transform.RecursiveFindChild("HEAD").GetComponent<Renderer>();
            _skinMaterial = _skinRenderer.sharedMaterials[0];
            
            _skinMaterial.SetTextureIfHasProperty(_MainTex, skinData.baseColor);
            _skinMaterial.SetTextureIfHasProperty(_URPMainTex, skinData.baseColor);
            
            _skinMaterial.SetTextureIfHasProperty(_DetailMask, skinDetailData.detailMask);
            _skinMaterial.SetTextureIfHasProperty(_DetailAlbedoMap, skinDetailData.detailBase);
            _skinMaterial.EnableKeyword("_Emission");
            _skinMaterial.EnableKeyword("_DETAIL_MULX2");
            _skinMaterial.SetTextureIfHasProperty(_Emission, skinDetailData.emissive);
            _skinMaterial.SetColorIfHasProperty(_EmissionColor, skinDetailData.emissiveColor);
            _skinMaterial.SetColorIfHasProperty(_HDRPEmissionColor, skinDetailData.emissiveColor);
        }

        #endregion

        #region API

        public void LoadCharacterBody()
        {
            LoadTorso();
            LoadLegs();
            LoadFeet();
            LoadHands();
            LoadFullTorso();
            LoadTorsoProp();

            LoadHair();
            LoadCap();
            LoadGlasses();
            LoadEyes();
            LoadSkin();
            LoadFace();
        }

        #endregion

        #region Callback

        private void ChangeColor(Color newColor, Renderer currentRenderer)
        {
            if(currentRenderer == null) return;
            Color normalColor = newColor;
            Color urpColor = newColor;
            
            if (currentRenderer.materials.Length == 1)
            {
                if (currentRenderer.materials[0].HasProperty(_Color))
                {
                    currentRenderer.materials[0].SetColor(_Color, normalColor);
                }

                if (currentRenderer.materials[0].HasProperty(_URPColor))
                {
                    currentRenderer.materials[0].SetColor(_URPColor, urpColor);
                }
            }
            else if (currentRenderer.materials.Length > 1)
            {
                if (currentRenderer.materials[1].HasProperty(_Color))
                {
                    currentRenderer.materials[1].SetColor(_Color, normalColor);
                }

                if (currentRenderer.materials[1].HasProperty(_URPColor))
                {
                    currentRenderer.materials[1].SetColor(_URPColor, urpColor);
                }
            }
        }

        public void SaveData(string objName, PartCustomization.ItemData data, bool active = true)
        {
            this.LogEditorOnly($"[Save] {objName} {active}");
            if (objName.StartsWith(torsoPrefix))
            {
                PlayerSandbox.Instance.CharacterModelHandle.Torso ??= new CharacterModelHandle.PartBodyData();
                PlayerSandbox.Instance.CharacterModelHandle.Torso.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex, active);
            } else if (objName.StartsWith(legsPrefix))
            {
                PlayerSandbox.Instance.CharacterModelHandle.Legs ??= new CharacterModelHandle.PartBodyData();
                PlayerSandbox.Instance.CharacterModelHandle.Legs.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex, active);
            } else if (objName.StartsWith(feetPrefix))
            {
                PlayerSandbox.Instance.CharacterModelHandle.Feet ??= new CharacterModelHandle.PartBodyData();
                PlayerSandbox.Instance.CharacterModelHandle.Feet.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex, active);
            } else if (objName.StartsWith(handsPrefix))
            {
                PlayerSandbox.Instance.CharacterModelHandle.Hands ??= new CharacterModelHandle.PartBodyData();
                PlayerSandbox.Instance.CharacterModelHandle.Hands.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex, active);
            } else if (objName.StartsWith(fullTorsoPrefix))
            {
                PlayerSandbox.Instance.CharacterModelHandle.FullTorso ??= new CharacterModelHandle.PartBodyData();
                PlayerSandbox.Instance.CharacterModelHandle.FullTorso.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex, active);
            } else if (objName.StartsWith(torsoPropsPrefix))
            {
                PlayerSandbox.Instance.CharacterModelHandle.TorsoProp ??= new CharacterModelHandle.PartBodyData();
                PlayerSandbox.Instance.CharacterModelHandle.TorsoProp.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex, active);
            }

            if (objName.StartsWith(hairPrefix))
            {
                PlayerSandbox.Instance.CharacterModelHandle.Hair ??= new CharacterModelHandle.HairData();
                PlayerSandbox.Instance.CharacterModelHandle.Hair.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex, active);
            } 
            else if (objName.StartsWith(capePrefix))
            {
                PlayerSandbox.Instance.CharacterModelHandle.Cap ??= new CharacterModelHandle.PartBodyData();
                PlayerSandbox.Instance.CharacterModelHandle.Cap.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex, active);
            }

            else if(objName.StartsWith(facePrefix))
            {
                PlayerSandbox.Instance.CharacterModelHandle.Face ??= new CharacterModelHandle.FaceData();
                PlayerSandbox.Instance.CharacterModelHandle.Face.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex, active);
            }
        }

        public void SaveHairColor(Color color, float val)
        {
            PlayerSandbox.Instance.CharacterModelHandle.Hair ??= new CharacterModelHandle.HairData();
            PlayerSandbox.Instance.CharacterModelHandle.Hair.SetColor(color, val);
            
            this.LogEditorOnly($"SaveHairColor {color} {val}");
        }

        public void SaveGlassesDataSaveData(string objName, PartCustomization.ItemData data, bool active = true)
        {
            PlayerSandbox.Instance.CharacterModelHandle.Glasses ??= new CharacterModelHandle.GlassesData();
            PlayerSandbox.Instance.CharacterModelHandle.Glasses.SetValue(objName, data.databaseIndex, data.colorIndex, data.itemIndex,active);
        }
        
        public void SaveGlassesColor(Color color, float val)
        {
            PlayerSandbox.Instance.CharacterModelHandle.Glasses ??= new CharacterModelHandle.GlassesData();
            PlayerSandbox.Instance.CharacterModelHandle.Glasses.SetColor(color, val);
        }

        public void SaveFaceColor(Color color, float val)
        {
            PlayerSandbox.Instance.CharacterModelHandle.Face ??= new CharacterModelHandle.FaceData();
            PlayerSandbox.Instance.CharacterModelHandle.Face.SetColor(color, val);
            this.LogEditorOnly($"SaveFaceColor {color} {val}");
        }

        private void OnSaveChanged(CharacterModelHandle.PartBodyData preValue, CharacterModelHandle.PartBodyData newValue)
        {
            this.LogEditorOnly($"Save {JsonConvert.SerializeObject(newValue)}");
        }

        #endregion
    }

    /// <summary>
    /// 一个身体部分，如身体，上半身，下半身，鞋子，
    /// </summary>
    [Serializable]
    public class CharacterMeshPart
    {
        public enum EPart
        {
            Base,
            
            UpperBody,
            LowerBody,
            Foot,
            Hand,
            Head,
            Face,
            
            /// <summary>
            /// UpperBody + LowerBody
            /// </summary>
            Suit01,
            
            /// <summary>
            /// UpperBody + LowerBody + Foot
            /// </summary>
            Suit02,
        }
        
        public int meshId;
        public int materialId;

        public CharacterMeshPart(int mesh, int material)
        {
            meshId = mesh;
            materialId = material;
        }

        public CharacterMeshPart()
        {
            meshId = -1;
            materialId = -1;
        }
    }

    [Serializable]
    public class CharacterCustomization
    {
        [NonSerialized]
        public Dictionary<CharacterMeshPart.EPart, List<CharacterMeshPart.EPart>> PartConflicts;
        public Dictionary<CharacterMeshPart.EPart, CharacterMeshPart> Meshes = new();
        
        public UnityEvent<CharacterMeshPart.EPart, int, int> onCustomizeChanged = new();
        
        public CharacterCustomization()
        {
            PartConflicts = new Dictionary<CharacterMeshPart.EPart, List<CharacterMeshPart.EPart>>()
            {
                
                { CharacterMeshPart.EPart.Suit01, new List<CharacterMeshPart.EPart>() },
                { CharacterMeshPart.EPart.Suit02, new List<CharacterMeshPart.EPart>() },
            };
        }

        public void SetMeshPart(CharacterMeshPart.EPart part, int mesh, int material)
        {
            var conflicts = PartConflicts[part];
            foreach (var p in conflicts)
            {
                SetMeshPart(p, -1, -1);
            }
            
            Meshes.Add(part, new CharacterMeshPart(mesh, material));
            onCustomizeChanged.Invoke(part, mesh, material);
        }

        public void Save()
        {
            var jsonString = JsonConvert.SerializeObject(this);
            PlayerPrefs.SetString("", jsonString);
        }
    }

    public class UILayer1
    {
        
    }
    
}