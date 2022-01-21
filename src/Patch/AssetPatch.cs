using HarmonyLib;
using UnityEngine;
using Assets.Scripts.PeroTools.Managers;
using PeroTools2.Resources;
using UnhollowerRuntimeLib;
using SystemGeneric =  System.Collections.Generic;
using Assets.Scripts.Database;
using System;
using Newtonsoft.Json.Linq;
using MEC;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Linq;

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
            //if("albums" == __instance.name)
            //{
            //    var jArray = __result.JsonDeserialize<JArray>();
            //    jArray.Add(JObject.FromObject(
            //        new
            //        {
            //            uid = AlbumManager.MusicPackge,
            //            title = "Custom Albums",
            //            prefabsName = $"AlbumDisco{AlbumManager.Uid}",
            //            price = "¥25.00",
            //            jsonName = AlbumManager.JsonName,
            //            needPurchase = false,
            //            free = true,
            //        }));
            //    __result = jArray.JsonSerialize();

            //    Log.Debug($"albums injected");
            //}
        }
        /// <summary>
        /// Load and inject needed json
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(ResourcesManager),nameof(ResourcesManager.GetAssetData))]
        [HarmonyPrefix]
        public static void GetAssetData(ResourcesManager __instance, ref string assetName)
        {
            //Log.Debug($"GetAssetData: {assetName}");
            if (!Injected)
            {
                Injected = true; // Whether it is successful or not, it is injected only once
                #region Inject albums.json
                var name = "albums";
                var textAsset = LoadAsset<TextAsset>(name);
                var jArray = textAsset.text.JsonDeserialize<JArray>();
                jArray.Add(JObject.FromObject(new
                {
                    uid = AlbumManager.MusicPackge,
                    title = "Custom Albums",
                    prefabsName = $"AlbumDisco{AlbumManager.Uid}",
                    price = "¥25.00",
                    jsonName = AlbumManager.JsonName,
                    needPurchase = false,
                    free = true,
                }));
                // Add
                var newTextAsset = CreateTextAsset(name, jArray.JsonSerialize());
                if (__instance.m_LoadedAssetHandles.ContainsKey(name))
                    __instance.m_LoadedAssetHandles.Remove(name);
                __instance.m_LoadedAssetHandles.Add(name, newTextAsset);
                Log.Debug($"Injected: {name}.json");
                #endregion
                #region Inject albums_<lang>.json
                foreach (var lang in AlbumManager.Langs)
                {
                    name = $"albums_{lang.Key}";
                    textAsset = LoadAsset<TextAsset>(name);
                    jArray = textAsset.text.JsonDeserialize<JArray>();
                    jArray.Add(JObject.FromObject(new
                    {
                        title = lang.Value,
                    }));
                    // Add
                    newTextAsset = CreateTextAsset(name, jArray.JsonSerialize());
                    if (__instance.m_LoadedAssetHandles.ContainsKey(name))
                        __instance.m_LoadedAssetHandles.Remove(name);
                    __instance.m_LoadedAssetHandles.Add(name, newTextAsset);
                    Log.Debug($"Injected: {name}.json");
                }
                #endregion
                #region Inject ALBUM1000.json
                name = AlbumManager.JsonName;
                jArray = new JArray();
                foreach (var keyValue in AlbumManager.LoadedAlbums)
                {
                    var key = keyValue.Key;
                    var album = keyValue.Value;
                    var info = album.Info;
                    var jObject = new JObject();
                    jObject.Add("uid", $"{AlbumManager.Uid}-{album.Index}");
                    jObject.Add("name", info.GetName());
                    jObject.Add("author", info.GetAuthor());
                    jObject.Add("bpm", info.bpm);
                    jObject.Add("music", $"{key}_music");
                    jObject.Add("demo", $"{key}_demo");
                    jObject.Add("cover", $"{key}_cover");
                    jObject.Add("noteJson", $"{key}_map");
                    jObject.Add("scene", info.scene);
                    jObject.Add("unlockLevel", info.unlockLevel);
                    if (!string.IsNullOrEmpty(info.levelDesigner))
                        jObject.Add("levelDesigner", info.levelDesigner);
                    if (!string.IsNullOrEmpty(info.levelDesigner1))
                        jObject.Add("levelDesigner1", info.levelDesigner1);
                    if (!string.IsNullOrEmpty(info.levelDesigner2))
                        jObject.Add("levelDesigner2", info.levelDesigner2);
                    if (!string.IsNullOrEmpty(info.levelDesigner3))
                        jObject.Add("levelDesigner3", info.levelDesigner3);
                    if (!string.IsNullOrEmpty(info.levelDesigner4))
                        jObject.Add("levelDesigner4", info.levelDesigner4);
                    if (!string.IsNullOrEmpty(info.difficulty1))
                        jObject.Add("difficulty1", info.difficulty1);
                    if (!string.IsNullOrEmpty(info.difficulty2))
                        jObject.Add("difficulty2", info.difficulty2);
                    if (!string.IsNullOrEmpty(info.difficulty3))
                        jObject.Add("difficulty3", info.difficulty3);
                    if (!string.IsNullOrEmpty(info.difficulty4))
                        jObject.Add("difficulty4", info.difficulty4);

                    jArray.Add(jObject);
                }
                // Add
                newTextAsset = CreateTextAsset(name, jArray.JsonSerialize());
                if (__instance.m_LoadedAssetHandles.ContainsKey(name))
                    __instance.m_LoadedAssetHandles.Remove(name);
                __instance.m_LoadedAssetHandles.Add(name, newTextAsset);
                Log.Debug($"Injected: {name}.json");
                #endregion
                #region Inject ALBUM1000_<lang>.json
                foreach (var lang in AlbumManager.Langs)
                {
                    name = $"{AlbumManager.JsonName}_{lang.Key}";
                    jArray = new JArray();
                    foreach (var keyValue in AlbumManager.LoadedAlbums)
                    {
                        jArray.Add(JObject.FromObject(new
                        {
                            name = keyValue.Value.Info.GetName(lang.Key),
                            author = keyValue.Value.Info.GetAuthor(lang.Key),
                        }));
                    }
                    // Add
                    newTextAsset = CreateTextAsset(name, jArray.JsonSerialize());
                    if (__instance.m_LoadedAssetHandles.ContainsKey(name))
                        __instance.m_LoadedAssetHandles.Remove(name);
                    __instance.m_LoadedAssetHandles.Add(name, newTextAsset);
                    Log.Debug($"Injected: {name}.json");
                }
                #endregion
                #region Inject defaultTag.json
                name = "defaultTag";
                textAsset = LoadAsset<TextAsset>(name);
                jArray = textAsset.text.JsonDeserialize<JArray>();
                // Replace Cute tag
                var music_tag = jArray.First(o => o.Value<int>("sort_key") == 8);
                music_tag["tag_name"] = JObject.FromObject(AlbumManager.Langs);
                music_tag["tag_picture"] = "https://mdmc.moe/cdn/melon.png";
                music_tag["pic_name"] = "";
                music_tag["music_list"] = JArray.FromObject(AlbumManager.GetAllUid());
                // Add
                newTextAsset = CreateTextAsset(name, jArray.JsonSerialize());
                if (__instance.m_LoadedAssetHandles.ContainsKey(name))
                    __instance.m_LoadedAssetHandles.Remove(name);
                __instance.m_LoadedAssetHandles.Add(name, newTextAsset);
                Log.Debug($"Injected: {name}.json");
                #endregion
                // Inject ALBUM1000.json

                // ALBUM1000.json
                //var data = new ResourcesManager.AssetData();
                //data.guid = "";
                //data.path = "";
                //data.type = Il2CppType.Of<TextAsset>();
                //__instance.m_AssetDatas.Add(AlbumManager.JsonName, new ResourcesManager.AssetData());
                //Injected = true;
                //Log.Debug($"Injected: {AlbumManager.JsonName}");
            }
        }

        [HarmonyPatch(typeof(ConfigManager), nameof(ConfigManager.LoadConfigFile))]
        [HarmonyPrefix]
        public static bool LoadConfigFile(ref ConfigManager __instance, ref string name, ref Il2CppSystem.Type type, ref bool cache)
        {
            //Log.Debug($"LoadConfigFile: name: {name} type:{type.FullName} cache:{cache}");
            return true;
        }
        //[HarmonyPatch(typeof(ConfigManager), nameof(ConfigManager.LoadConfigFile))]
        //[HarmonyPrefix]
        //public static void ConfigManagerInit(ref ConfigManager __instance)
        //{
        //    Log.Debug($"ConfigManagerInit {__instance.m_DbConfig.}")
        //}
        [HarmonyPatch(typeof(ConfigManager), nameof(ConfigManager.LoadConfigFile))]
        [HarmonyPostfix]
        public static void LoadConfigFile(ref ConfigManager __instance,ref BaseDBConfigObject __result, ref string name, ref Il2CppSystem.Type type, ref bool cache)
        {
            if(__result == null)
            {
                Log.Debug($"NULL!!!! LoadConfigFile: name: {name}");
                return;
            }
            //if (type == Il2CppType.Of<DBConfigALBUM>())
            //{
            //    var a = __result.Cast<DBConfigALBUM>();
            //    a.list.ForEach(new Action<DBConfigALBUM.MusicInfo>(obj =>
            //    {
            //        Log.Debug(obj);
            //    }));
            //}
            if (name == "albums")
            {
                if (type == Il2CppType.Of<DBConfigAlbums>())
                {
                    var a = __result.Cast<DBConfigAlbums>();
                    a.list.ForEach(new Action<DBConfigAlbums.AlbumsInfo>(obj =>
                    {
                        Log.Debug(obj.uid);
                    }));
                }
            }
            Log.Debug($"LoadConfigFile: name: {name} fileName:{__result.fileName} listInterface:{__result.listInterface} type:{type.Name} text:{__result.text} count:{__result.count} ");
        }
        /// <summary>
        /// Load asset from Addressables.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static T LoadAsset<T>(string assetName)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetName);
            if (handle.IsValid() && handle.Status != AsyncOperationStatus.Failed)
            {
                handle.WaitForCompletion();
                if (handle.IsDone)
                {
                    return handle.Result;
                }
                Log.Error("Cannot load: " + assetName);
            }
            return default(T);
        }
        /// <summary>
        /// Create a new TextAsset
        /// </summary>
        /// <param name="name"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static TextAsset CreateTextAsset(string name,string text)
        {
            var newAsset = new TextAsset(text);
            newAsset.name = name;
            return newAsset;
        }
    }
}
