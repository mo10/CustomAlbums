using HarmonyLib;
using UnityEngine;
using Assets.Scripts.PeroTools.Managers;
using PeroTools2.Resources;
using UnhollowerRuntimeLib;
using SystemGeneric =  System.Collections.Generic;
using Assets.Scripts.Database;
using System;
using Newtonsoft.Json.Linq;
using IL2CppJson = Il2CppNewtonsoft.Json.Linq;
using MEC;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Linq;
using System.Collections.Generic;
using IL2CppGeneric = Il2CppSystem.Collections.Generic;
using Assets.Scripts.GameCore.Managers;
using Assets.Scripts.PeroTools.GeneralLocalization;
using Assets.Scripts.PeroTools.Nice.Actions;
using Assets.Scripts.GameCore;
using UnityEngine.AddressableAssets.ResourceLocators;
using MelonLoader;

namespace CustomAlbums.Patch
{
    class ResourcePatch
    {
        private static readonly Logger Log = new Logger("ResourcePatch");

        public static void LoadFromName(string assetName, Il2CppSystem.Action<TextAsset> callback)
        {
            Log.Debug($"assetName : {assetName} type: TextAsset");
        }
        unsafe public static void DoPatching(HarmonyLib.Harmony harmony)
        {
            //var original = AccessTools.Method(typeof(ResourcesManager), nameof(ResourcesManager.LoadFromNameInternal), generics: new Type[] { typeof(TextAsset) });
            //var prefix = AccessTools.Method(typeof(ResourcePatch), nameof(LoadFromName));

            var methodPtr = Utils.NativeMethod(typeof(ResourcesManager), nameof(ResourcesManager.LoadFromNameInternal));
            var methodPatchPtr = AccessTools.Method(typeof(ResourcePatch), nameof(LoadFromName)).MethodHandle.GetFunctionPointer();

            MelonUtils.NativeHookAttach((IntPtr)(&methodPtr), methodPatchPtr);

        }
    }
    class AssetPatch
    {
        private static readonly Logger Log = new Logger("AssetPatch");

        public static TextAsset customAsset = new TextAsset("ALBUM1000");
        public static bool JsonInjected = false;
        public static bool DBConfigInjected = false;
        public static SystemGeneric.List<string> InjectedAssets = new SystemGeneric.List<string>();

        public static Dictionary<string, TextAsset> assets = new Dictionary<string, TextAsset>();
        public static Dictionary<string, BaseDBConfigObject> dbconfigs = new Dictionary<string, BaseDBConfigObject>();
#if false
        [HarmonyPatch(typeof(TextAsset), "get_text")]
        [HarmonyPostfix]
        public static void TextAssetInjector(TextAsset __instance, ref string __result)
        {
            if ("albums" == __instance.name)
            {
                var jArray = __result.JsonDeserialize<JArray>();
                jArray.Add(JObject.FromObject(
                    new
                    {
                        uid = AlbumManager.MusicPackge,
                        title = "Custom Albums",
                        prefabsName = $"AlbumDisco{AlbumManager.Uid}",
                        price = "¥25.00",
                        jsonName = AlbumManager.JsonName,
                        needPurchase = false,
                        free = true,
                    }));
                __result = jArray.JsonSerialize();

                Log.Debug($"albums injected");
            }
        }
#endif

        //[HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.LoadFromNameInternal))]
        //[HarmonyPrefix]
        //public static void DoPatching(string assetName, Il2CppSystem.Action<Sprite> callback)
        //{
        //    Log.Debug($"assetName : {assetName} type: Sprite");
        //}
        //[HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.LoadFromNameInternal))]
        //[HarmonyPrefix]

        public static void AddCustomAssets(IL2CppGeneric.Dictionary<string, ResourcesManager.AssetData> ___m_AssetDatas)
        {
            foreach (var keyValue in AlbumManager.LoadedAlbums)
            {
                var albumkey = keyValue.Key;
                var album = keyValue.Value;
                var info = album.Info;

                ___m_AssetDatas.Add($"{albumkey}_demo", new ResourcesManager.AssetData()
                {
                    guid = albumkey,
                    path = "CustomAlbums/demo",
                    type = Il2CppType.Of<AudioClip>()
                });;
                ___m_AssetDatas.Add($"{albumkey}_music", new ResourcesManager.AssetData()
                {
                    guid = albumkey,
                    path = "CustomAlbums/music",
                    type = Il2CppType.Of<AudioClip>()
                });
                ___m_AssetDatas.Add($"{albumkey}_cover", new ResourcesManager.AssetData()
                {
                    guid = albumkey,
                    path = "CustomAlbums/cover",
                    type = Il2CppType.Of<Sprite>()
                });

                if (!string.IsNullOrEmpty(info.difficulty1))
                    ___m_AssetDatas.Add($"{albumkey}_map1", new ResourcesManager.AssetData()
                    {
                        guid = albumkey,
                        path = "CustomAlbums/map1",
                        type = Il2CppType.Of<StageInfo>()
                    });
                if (!string.IsNullOrEmpty(info.difficulty2))
                    ___m_AssetDatas.Add($"{albumkey}_map2", new ResourcesManager.AssetData()
                    {
                        guid = albumkey,
                        path = "CustomAlbums/map2",
                        type = Il2CppType.Of<StageInfo>()
                    });
                if (!string.IsNullOrEmpty(info.difficulty3))
                    ___m_AssetDatas.Add($"{albumkey}_map3", new ResourcesManager.AssetData()
                    {
                        guid = albumkey,
                        path = "CustomAlbums/map3",
                        type = Il2CppType.Of<StageInfo>()
                    });
                if (!string.IsNullOrEmpty(info.difficulty4))
                    ___m_AssetDatas.Add($"{albumkey}_map4", new ResourcesManager.AssetData()
                    {
                        guid = albumkey,
                        path = "CustomAlbums/map4",
                        type = Il2CppType.Of<StageInfo>()
                    });
            }
        }
        /// <summary>
        /// Load and inject needed json
        /// </summary>
        /// <param name="__instance"></param>
        [HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.GetAssetData))]
        [HarmonyPrefix]
        public static void GetAssetData(ResourcesManager __instance, ref string assetName)
        {
            Log.Debug($"GetAssetData {assetName}");
            if (JsonInjected)
                return;

            JsonInjected = true; // Whether it is successful or not, it is injected only once

            var configManager = Assets.Scripts.PeroTools.Commons.Singleton<ConfigManager>.instance;
            var resourceManager = __instance;
            string language = Assets.Scripts.PeroTools.Commons.SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");

            string name = null;
            TextAsset textAsset = null;
            TextAsset newTextAsset = null;
            JArray jArray = null;

            #region Inject albums_<lang>.json
            var localAlbumsDict = new Il2CppSystem.Collections.Generic.Dictionary<int, DBConfigLocalAlbums>();
            var startIdx = 1;
            foreach (var lang in AlbumManager.Langs)
            {
                name = $"albums_{lang.Key}";
                textAsset = LoadAsset<TextAsset>(name);
                jArray = textAsset.text.JsonDeserialize<JArray>();
                jArray.Add(JObject.FromObject(new
                {
                    title = lang.Value,
                }));
                newTextAsset = CreateTextAsset(name, jArray.JsonSerialize());
                // Remove Existed
                if (configManager.m_TextAssets.ContainsKey(name))
                    configManager.m_TextAssets.Remove(name);
                if (configManager.m_Dictionary.ContainsKey(name))
                    configManager.m_Dictionary.Remove(name);
                if (assets.ContainsKey(name))
                    assets.Remove(name);
                // Add
                configManager.m_TextAssets.Add(name, newTextAsset);
                configManager.m_Dictionary.Add(name, jArray.JsonSerialize().IL2CppJsonDeserialize<IL2CppJson.JArray>());
                assets.Add(name, newTextAsset);
                resourceManager.m_AssetDatas.TryAdd(name, new ResourcesManager.AssetData());
                var localAlbums = new DBConfigLocalAlbums();
                localAlbums.Deserialize(newTextAsset.text);
                localAlbums.m_FileName = name;
                localAlbumsDict.Add(startIdx, localAlbums);
                startIdx++;
                // Default language
                if (language == lang.Key)
                    localAlbumsDict.Add(0, localAlbums);
                Log.Debug($"Injected: {name}.json");
            }
            #endregion
            #region Inject albums.json
            name = "albums";
            textAsset = LoadAsset<TextAsset>(name);
            jArray = textAsset.text.JsonDeserialize<JArray>();
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
            newTextAsset = CreateTextAsset(name, jArray.JsonSerialize());
            DBConfigAlbums dbconfig = new DBConfigAlbums();
            dbconfig.Deserialize(newTextAsset.text);
            dbconfig.m_FileName = name;
            dbconfig.m_LocalDic = localAlbumsDict;
            dbconfigs.Add(name, dbconfig);

            // Remove Existed
            if (configManager.m_TextAssets.ContainsKey(name))
                configManager.m_TextAssets.Remove(name);
            if (configManager.m_Dictionary.ContainsKey(name))
                configManager.m_Dictionary.Remove(name);
            if (assets.ContainsKey(name))
                assets.Remove(name);
            if (configManager.m_DbConfig.GetDBConfigObject(name, out _))
                configManager.m_DbConfig.RemoveDBConfigObject(name);
            // Add
            configManager.m_TextAssets.Add(name, newTextAsset);
            configManager.m_Dictionary.Add(name, jArray.JsonSerialize().IL2CppJsonDeserialize<IL2CppJson.JArray>());
            assets.Add(name, newTextAsset);
            resourceManager.m_AssetDatas.TryAdd(name, new ResourcesManager.AssetData());
            configManager.m_DbConfig.AddDBConfigObject(name, dbconfig);
            Log.Debug($"Injected: {name}.json");
            #endregion
            #region Inject ALBUM1000_<lang>.json
            var localAlbumDict = new Il2CppSystem.Collections.Generic.Dictionary<int, DBConfigLocalALBUM>();
            startIdx = 1;
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
                // Remove Existed
                if (configManager.m_TextAssets.ContainsKey(name))
                    configManager.m_TextAssets.Remove(name);
                if (configManager.m_Dictionary.ContainsKey(name))
                    configManager.m_Dictionary.Remove(name);
                if (assets.ContainsKey(name))
                    assets.Remove(name);
                // Add
                configManager.m_TextAssets.Add(name, newTextAsset);
                configManager.m_Dictionary.Add(name, jArray.JsonSerialize().IL2CppJsonDeserialize<IL2CppJson.JArray>());
                assets.Add(name, newTextAsset);
                resourceManager.m_AssetDatas.TryAdd(name, new ResourcesManager.AssetData());
                // Add language 
                var localAlbum = new DBConfigLocalALBUM();
                localAlbum.Deserialize(newTextAsset.text);
                localAlbum.m_FileName = name;
                localAlbumDict.Add(startIdx, localAlbum);
                startIdx++;
                // Default language
                if (language == lang.Key)
                    localAlbumDict.Add(0, localAlbum);
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
                jObject.Add("unlockLevel", string.IsNullOrEmpty(info.unlockLevel) ? "0" : info.unlockLevel);
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
            newTextAsset = CreateTextAsset(name, jArray.JsonSerialize());
            DBConfigALBUM dbconfig2 = new DBConfigALBUM();
            dbconfig2.Deserialize(newTextAsset.text);
            dbconfig2.m_FileName = name;
            dbconfig2.m_LocalDic = localAlbumDict;
            dbconfigs.Add(name, dbconfig);

            // Remove Existed
            if (configManager.m_TextAssets.ContainsKey(name))
                configManager.m_TextAssets.Remove(name);
            if (configManager.m_Dictionary.ContainsKey(name))
                configManager.m_Dictionary.Remove(name);
            if (assets.ContainsKey(name))
                assets.Remove(name);
            if (configManager.m_DbConfig.GetDBConfigObject(name, out _))
                configManager.m_DbConfig.RemoveDBConfigObject(name);
            // Add
            configManager.m_TextAssets.Add(name, newTextAsset);
            configManager.m_Dictionary.Add(name, jArray.JsonSerialize().IL2CppJsonDeserialize<IL2CppJson.JArray>());
            assets.Add(name, newTextAsset);
            resourceManager.m_AssetDatas.TryAdd(name, new ResourcesManager.AssetData());
            configManager.m_DbConfig.AddDBConfigObject(name, dbconfig2);
            Log.Debug($"Injected: {name}.json");
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
            if (assets.ContainsKey(name))
                assets.Remove(name);
            assets.Add(name, newTextAsset);
            Log.Debug($"Injected: {name}.json");
            #endregion

            AddCustomAssets(resourceManager.m_AssetDatas);
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


        [HarmonyPatch(typeof(ConfigManager), nameof(ConfigManager.LoadConfigFile))]
        [HarmonyPrefix]
        public static bool LoadConfigFile(ref ConfigManager __instance, ref BaseDBConfigObject __result, ref string name, ref Il2CppSystem.Type type, ref bool cache)
        {
            Log.Debug($"LoadConfigFile: name: {name} type:{type.FullName} cache:{cache}");

            if (dbconfigs.TryGetValue(name, out var config) && !__instance.m_DbConfig.GetDBConfigObject(name, out var config2))
            {
                Log.Debug($"Add DBConfig: {name}");
                __instance.m_DbConfig.AddDBConfigObject(name, config);
                __result = config;
                return false;
            }

            return true;
        }

        //[HarmonyPatch(typeof(ResourcesManager), nameof(ResourcesManager.LoadFromNameInternal))]
        //[HarmonyPrefix]
        //public static bool LoadFromNameInernal(ref string assetName)
        //{
        //    Log.Debug($"LoadFromNameInernal: name: {assetName}");

        //    return true;
        //}

        //[HarmonyPatch(typeof(ConfigManager), nameof(ConfigManager.LoadConfigFile))]
        //[HarmonyPrefix]
        //public static void ConfigManagerInit(ref ConfigManager __instance)
        //{
        //    Log.Debug($"ConfigManagerInit {__instance.m_DbConfig.}")
        //}
        [HarmonyPatch(typeof(ConfigManager), nameof(ConfigManager.LoadConfigFile))]
        [HarmonyPostfix]
        public static void LoadConfigFilePostfix(ref ConfigManager __instance, ref BaseDBConfigObject __result, ref string name, ref Il2CppSystem.Type type, ref bool cache)
        {

            if (__result == null)
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
            if (type == Il2CppType.Of<DBConfigAlbums>())
            {
                var a = __result.Cast<DBConfigAlbums>();
                for (int i = 0; i < 5; i++)
                {
                    Log.Debug($" lang idx: {i} , lang:{a.GetLocal(i).fileName}");
                }
            }
            if (type == Il2CppType.Of<DBConfigALBUM>())
            {
                var a = __result.Cast<DBConfigALBUM>();
                for (int i = 0; i < 6; i++)
                {
                    Log.Debug($" lang idx: {i} , lang:{a.GetLocal(i).fileName}");
                }
            }
            Log.Debug($"LoadConfigFile: name: {name} fileName:{__result.fileName} listInterface:{__result.listInterface} type:{type.Name} text:{__result.text} count:{__result.count}  fileName:{__result.fileName}");
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
        public static TextAsset CreateTextAsset(string name, string text)
        {
            var newAsset = new TextAsset(text);
            newAsset.name = name;
            return newAsset;
        }
    }
}
