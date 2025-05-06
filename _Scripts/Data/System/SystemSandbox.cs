using DragonLi.Core;
using DragonLi.UI;

namespace Data
{
    public class SystemSandbox : Singleton<SystemSandbox>
    {
        #region Properties - Handler
        
        /// <summary>
        /// 语言类型缓存
        /// </summary>
        public LanguageHandler LanguageHandler { get; private set; }
        
        /// <summary>
        /// 系统音量
        /// </summary>
        public VolumeHandler VolumeHandler { get; private set; }
        
        #endregion

        #region Function

        private void InitData()
        {
            LanguageHandler = new LanguageHandler();
            VolumeHandler = new VolumeHandler();
        }

        #endregion

        #region API

        public void InitializeSystemSandbox()
        {
            InitData();
            
            LocalizationManager.Instance.SetLanguage(LanguageHandler.LanguageType);
        }
        
        public void DebugInitializeSystemSandbox()
        {
#if UNITY_EDITOR
            InitializeSystemSandbox();
#endif
        }

        #endregion
    }
}