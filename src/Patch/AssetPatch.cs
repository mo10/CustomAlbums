using HarmonyLib;
using UnityEngine;
using Assets.Scripts.PeroTools.Managers;
using PeroTools2.Resources;
using UnhollowerRuntimeLib;
using SystemGeneric =  System.Collections.Generic;
using Assets.Scripts.Database;
using System;
using Newtonsoft.Json.Linq;

namespace CustomAlbums.Patch
{
    class AssetPatch
    {
        private static readonly Logger Log = new Logger("AssetPatch");

        public static TextAsset customAsset = new TextAsset("ALBUM1000");
        public static bool Injected = false;
        public static SystemGeneric.List<string> InjectedAssets = new SystemGeneric.List<string>();
        [HarmonyPatch(typeof(TextAsset), "get_text")]
        [HarmonyPostfix]
        public static void TextAssetInjector(TextAsset __instance, ref string __result)
        {
            if("albums" == __instance.name)
            {
                var jArray = __result.JsonDeserialize<JArray>();
                jArray.Add(new
                {
                    uid = AlbumManager.MusicPackge,
                    title = "Custom Albums",
                    prefabsName = $"AlbumDisco{AlbumManager.Uid}",
                    price = "¥25.00",
                    jsonName = AlbumManager.JsonName,
                    needPurchase = false,
                    free = true,
                });
                __result = jArray.JsonSerialize();

                Log.Debug($"albums injected");
            }
        }
        [HarmonyPatch(typeof(ResourcesManager),nameof(ResourcesManager.GetAssetData))]
        [HarmonyPrefix]
        public static void GetAssetData(ResourcesManager __instance, ref string assetName)
        {
            Log.Debug($"GetAssetData: {assetName}");

            if (!Injected)
            {
                var data = new ResourcesManager.AssetData();
                data.guid = "";
                data.path = "";
                data.type = Il2CppType.Of<TextAsset>();
                __instance.m_AssetDatas.Add(AlbumManager.JsonName, new ResourcesManager.AssetData());
                Injected = true;
                Log.Debug($"Injected: {AlbumManager.JsonName}");
            }
        }

        [HarmonyPatch(typeof(ConfigManager), nameof(ConfigManager.LoadConfigFile))]
        [HarmonyPrefix]
        public static bool LoadConfigFile(ref ConfigManager __instance, ref string name, ref Il2CppSystem.Type type, ref bool cache)
        {
            Log.Debug($"LoadConfigFile: name: {name} type:{type.FullName} cache:{cache}");
            return true;
        }

        [HarmonyPatch(typeof(ConfigManager), nameof(ConfigManager.LoadConfigFile))]
        [HarmonyPostfix]
        public static void LoadConfigFile(ref ConfigManager __instance,ref BaseDBConfigObject __result, ref string name, ref Il2CppSystem.Type type, ref bool cache)
        {
            if (type == Il2CppType.Of<DBConfigALBUM>())
            {
                var a = __result.Cast<DBConfigALBUM>();
                a.list.ForEach(new Action<DBConfigALBUM.MusicInfo>(obj =>
                {
                    Log.Debug(obj);
                }));
            }
            Log.Debug($"LoadConfigFile: name: {name} fileName:{__result.fileName} listInterface:{__result.listInterface} text:{__result.text} count:{__result.count} ");
        }
    }
}
