using CustomAlbums.Patch;
using UnityEngine;

#if MELON
using MelonLoader;
[assembly: MelonInfo(typeof(CustomAlbums.ModEntry), "CustomAlbums", "3.0.0", "Mo10")]
[assembly: MelonGame("PeroPeroGames", "MuseDash")]
#elif BEPINEX
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
#endif

namespace CustomAlbums
{
    public static class Entry
    {
        public static void DoPatching(HarmonyLib.Harmony harmony)
        {
            Application.runInBackground = true;
            harmony.PatchAll(typeof(WebApiPatch));
        }
    }


#if MELON
    public class ModEntry : MelonMod
    {
        public override void OnApplicationStart()
        {
            LoggerInstance.Msg($"CustomAlbums is loaded!");
            Entry.DoPatching(HarmonyInstance);
        }
    }
#elif BEPINEX
    [BepInPlugin("com.github.mo10.customalbums", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("MuseDash.exe")]
    public class ModEntry : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo($"CustomAlbums is loaded!");
            Harmony harmony = new Harmony("com.github.mo10.customalbums");

            //Directory.CreateDirectory("mmdump"); // or create it manually
            //Environment.SetEnvironmentVariable("MONOMOD_DMD_TYPE", "mb"); // Also "mb" can work if mono runtime supports it; it can be a bit faster
            //Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", "mmdump");
            Entry.DoPatching(harmony);
            //Environment.SetEnvironmentVariable("MONOMOD_DMD_DUMP", ""); // Disable to prevent dumping other stuff
        }
    }
#endif
}
