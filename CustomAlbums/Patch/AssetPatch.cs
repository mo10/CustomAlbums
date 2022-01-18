using HarmonyLib;
using PeroTools2.Resources;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using static PeroTools2.Resources.ResourcesManager;

namespace CustomAlbums.Patch
{
    class AssetPatch
    {
        private static Logger Log = new Logger("WebApiPatch");

        public static TextAsset customAsset = new TextAsset("ALBUM1000");
        [HarmonyPatch(typeof(TextAsset), nameof(TextAsset.Internal_CreateInstance))]
        [HarmonyPrefix]
        public static void Init(TextAsset self, string text)
        {
            Log.Debug($"Internal_CreateInstance {self.name}  {text}");
        }

        [HarmonyPatch(typeof(TextAsset), "get_text")]
        [HarmonyPostfix]
        public static void HookTexAsset(TextAsset __instance, ref string __result)
        {
            if(__instance.name == "albums")
            {
                Log.Debug($"albums Called!!! {__result}");
                customAsset.name = "test";
                Log.Debug($"test: {customAsset.name}  {customAsset.text}");
            }
                
        }
    }
}
