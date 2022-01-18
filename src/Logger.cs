#if BEPINEX
using BepInEx.Logging;
#elif MELON
using MelonLoader;
#endif

namespace CustomAlbums
{
    public class Logger
    {
#if BEPINEX
        private ManualLogSource Log;
#elif MELON
        private MelonLogger.Instance Log;
#endif
        public Logger(string sourceName)
        {
#if BEPINEX
            Log = new ManualLogSource(sourceName);
#elif MELON
            Log =  new MelonLogger.Instance(sourceName);
#endif
        }
        public void Debug(object data)
        {
#if BEPINEX
            Log.LogDebug(data);
#elif MELON
            Log.Msg(data);
#endif
        }
        public void Info(object data)
        {
#if BEPINEX
            Log.LogInfo(data);
#elif MELON
            Log.Msg(data);
#endif
        }
        public void Warning(object data)
        {
#if BEPINEX
            Log.LogWarning(data);
#elif MELON
            Log.Msg(System.ConsoleColor.Yellow, data);
#endif
        }
        public void Error(object data)
        {
#if BEPINEX
            Log.LogError(data);
#elif MELON
            Log.Msg(System.ConsoleColor.Red, data);
#endif
        }
    }
}
