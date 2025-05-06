using DragonLi.Core;
using UnityEngine;

namespace Game
{
    public class TuringCharacterInstance : Singleton<TuringCharacterInstance>
    {
        private TuringCharacterSetting settings;

        public TuringCharacterSetting Settings
        {
            get
            {
                if (settings == null)
                {
                    settings = Resources.Load<TuringCharacterSetting>("TuringCharacterSetting");
                }

                return settings;
            }
            set
            {
                if (settings) return;
                settings = value;
            }
        }
        
    }
}