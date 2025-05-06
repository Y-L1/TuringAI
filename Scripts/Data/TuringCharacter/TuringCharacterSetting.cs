using System.Collections.Generic;
using MameshibaGames.Kekos.CharacterEditorScene.Customization;
using UnityEngine;

[CreateAssetMenu(fileName = "TuringCharacterSetting", menuName = "Scriptable Objects/TuringCharacterSetting")]
public class TuringCharacterSetting : ScriptableObject
{
    [Header("Character - Body")]
    [SerializeField] public PartDatabase torso;
    [SerializeField] public PartDatabase legs;
    [SerializeField] public PartDatabase feet;
    [SerializeField] public PartDatabase hands;
    [SerializeField] public PartDatabase fullTorso;
    [SerializeField] public PartDatabase torsoProps;

    [Header("Character - Head")]
    [SerializeField] public PartObjectDatabase hair;
    [SerializeField] public PartObjectDatabase cap;
    [SerializeField] public PartObjectDatabase glasses;
    [SerializeField] public EyesDatabase eyes;
    [SerializeField] public SkinDatabase skin;
    [SerializeField] public PartDatabase face;

    public Dictionary<string, Material> GetMaterials()
    {
        var materials = new Dictionary<string, Material>();
        foreach (var data in torso.itemsDatabases)
        {
            foreach (var itemInfo in data.items)
            {
                if(itemInfo.primaryMaterial == null) continue;
                materials.TryAdd(itemInfo.primaryMaterial.name, itemInfo.primaryMaterial);
            }
        }
        
        foreach (var data in legs.itemsDatabases)
        {
            foreach (var itemInfo in data.items)
            {
                if(itemInfo.primaryMaterial == null) continue;
                materials.TryAdd(itemInfo.primaryMaterial.name, itemInfo.primaryMaterial);
            }
        }
        
        foreach (var data in feet.itemsDatabases)
        {
            foreach (var itemInfo in data.items)
            {
                if(itemInfo.primaryMaterial == null) continue;
                materials.TryAdd(itemInfo.primaryMaterial.name, itemInfo.primaryMaterial);
            }
        }
        
        foreach (var data in hands.itemsDatabases)
        {
            foreach (var itemInfo in data.items)
            {
                if(itemInfo.primaryMaterial == null) continue;
                materials.TryAdd(itemInfo.primaryMaterial.name, itemInfo.primaryMaterial);
            }
        }
        
        foreach (var data in fullTorso.itemsDatabases)
        {
            foreach (var itemInfo in data.items)
            {
                if(itemInfo.primaryMaterial == null) continue;
                materials.TryAdd(itemInfo.primaryMaterial.name, itemInfo.primaryMaterial);
            }
        }
        
        foreach (var data in torsoProps.itemsDatabases)
        {
            foreach (var itemInfo in data.items)
            {
                if(itemInfo.primaryMaterial == null) continue;
                materials.TryAdd(itemInfo.primaryMaterial.name, itemInfo.primaryMaterial);
            }
        }
        
        // foreach (var data in eyes)
        // {
        //     foreach (var itemInfo in data.items)
        //     {
        //         if(itemInfo.primaryMaterial == null) continue;
        //         materials.TryAdd(itemInfo.primaryMaterial.name, itemInfo.primaryMaterial);
        //     }
        // }
        
        
        return materials;
    }

    public Material GetMaterialByName(string name)
    {
        return GetMaterials().ContainsKey(name) ? GetMaterials()[name] : null;
    }
}
