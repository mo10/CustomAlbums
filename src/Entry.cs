using CustomAlbums.Patch;
using UnityEngine;
#if BEPINEX
using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
#elif MELON
using MelonLoader;
[assembly: MelonInfo(typeof(CustomAlbums.ModEntry), "CustomAlbums", "3.1.0", "Mo10 & RobotLucca")]
[assembly: MelonGame("PeroPeroGames", "MuseDash")]
#endif

namespace CustomAlbums
{
    public static class Entry
    {
        public static void DoPatching(HarmonyLib.Harmony harmony)
        {
            Application.runInBackground = true;

            WebApiPatch.DoPatching();
            AssetPatch.DoPatching();
            SavesPatch.DoPatching(harmony);

            AlbumManager.LoadAll();
            SaveManager.Load();
            //harmony.PatchAll(typeof(SteamPatch));

            //harmony.PatchAll(typeof(AssetPatch));
            
            //ResourcePatch.DoPatching();
            //harmony.PatchAll(typeof(WebApiPatch));
            //harmony.PatchAll(typeof(ResourcePatch));
            //ResourcePatch.DoPatching(harmony);
        }
    }

#if BEPINEX
    [BepInPlugin("com.github.mo10.customalbums", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("MuseDash.exe")]
    public class ModEntry : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo($"CustomAlbums is loaded!");
            Harmony harmony = new Harmony("com.github.mo10.customalbums");

            Entry.DoPatching(harmony);
        }
    }

#elif MELON
    public class ModEntry : MelonMod
    {
        public override void OnApplicationStart()
        {
            LoggerInstance.Msg($"CustomAlbums is loaded!");
            ModSettings.RegisterSettings();
            Entry.DoPatching(HarmonyInstance);
        }
    }
#endif
}
