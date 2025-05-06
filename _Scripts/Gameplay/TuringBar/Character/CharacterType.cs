using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game
{
    [System.Serializable]

    public struct FCharacterMaterial
    {
        public int torso;
        public int torsoColor;
        
        public int legs;
        public int legsColor;
        
        public int hands;
        public int handsColor;
        
        public int fullTorso;
        public int fullTorsoColor;
        
        public int torsoProps;
        public int torsoPropsColor;
    }

    [System.Serializable]
    public struct FCharacterChildInfo
    {
        public string name;
        public List<string> materials;
    }
    
    [SerializeField]

    public static class TuringCharacterAPI
    {
        
    } 

    public class CharacterType
    {

    }
}