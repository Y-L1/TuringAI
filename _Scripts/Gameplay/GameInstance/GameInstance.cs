using DragonLi.Core;

namespace Game
{
    public class GameInstance : Singleton<GameInstance>
    {
        #region Properties - Handler

        public HostingHandler HostingHandler { get; private set; }

        #endregion

        #region API

        public void Initialize()
        {
            HostingHandler = new HostingHandler();
        }

        #endregion
    }
}