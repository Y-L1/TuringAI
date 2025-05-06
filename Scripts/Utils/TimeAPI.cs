using System;

namespace _Scripts.Utils
{
    public static class TimeAPI
    {
        public static int GetUtcTimeStamp()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
