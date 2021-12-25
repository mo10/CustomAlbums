using Assets.Scripts.GameCore;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using PeroTools2.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CustomAlbums.Patch
{
    class JsonPatch
    {
        public static Dictionary<string, string> assetMapping = new Dictionary<string, string>();
        public static void DoPatching(Harmony harmony)
        {
            MethodInfo method;
            MethodInfo methodPrefix;
            MethodInfo methodPostfix;

            // ConfigManager.Init
            method = AccessTools.Method(typeof(ConfigManager), "Init");
            methodPrefix = AccessTools.Method(typeof(JsonPatch), "ConfigManagerInitPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // AssetBundleManager.Init
            method = AccessTools.Method(typeof(ResourcesManager), "Init");
            methodPrefix = AccessTools.Method(typeof(JsonPatch), "ResourcesManagerInitPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // MusicTagManager.CustomTagPaser
            method = AccessTools.Method(typeof(MusicTagManager), "CustomTagPaser");
            methodPostfix = AccessTools.Method(typeof(JsonPatch), "CustomTagPaserPostfix");
            harmony.Patch(method, postfix: new HarmonyMethod(methodPostfix));
#if DEBUG
            ModLogger.Debug($"Application.streamingAssetsPath: {Application.streamingAssetsPath}");
            ModLogger.Debug($"Application.persistentDataPath: {Application.persistentDataPath}");
#endif
        }
        /// <summary>
        /// Make sure that the music selector is in the correct position
        /// </summary>
        /// <param name="___m_SelectedAlbumsID"></param>
        /// <param name="___m_SelectedAlbumsIDPrev"></param>
        public static void CustomTagPaserPostfix(ref uint ___m_SelectedAlbumsID, ref uint ___m_SelectedAlbumsIDPrev,ref List<uint> ___albumRangeList)
        {
            if (___m_SelectedAlbumsID != ___m_SelectedAlbumsIDPrev
                && ___albumRangeList.Contains(___m_SelectedAlbumsIDPrev))
            {
#if DEBUG
                ModLogger.Debug($"Fixed position:{___m_SelectedAlbumsIDPrev}");
#endif
                // Force use m_SelectedAlbumsIDPrev
                ___m_SelectedAlbumsID = ___m_SelectedAlbumsIDPrev;
            }
        }
        /// <summary>
        /// Inject: albums.json album_LANG.json ALBUM1000.json ALBUM1000_LANG.json
        /// </summary>
        /// <param name="___m_Dictionary"></param>
        /// <param name="___m_TextAssets"></param>
        public static void ConfigManagerInitPrefix(ref Dictionary<string, JArray> ___m_Dictionary, ref Dictionary<string, TextAsset> ___m_TextAssets)
        {
            #region Inject albums.json
            //var text = LoadTextAsset("albums", ref ___m_TextAssets);
            //JArray jArray = JsonUtils.ToArray(text);
            JArray jArray = Singleton<ConfigManager>.instance.GetJson("albums", false);

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
            //___m_Dictionary.Add("albums", jArray);
            #endregion
            #region Inject albums_<lang>.json
            foreach (var lang in AlbumManager.Langs)
            {
                //text = LoadTextAsset($"albums_{lang.Key}", ref ___m_TextAssets);
                //jArray = JsonUtils.ToArray(text);
                jArray = Singleton<ConfigManager>.instance.GetJson($"albums_{lang.Key}", false);
                jArray.Add(JObject.FromObject(new
                {
                    title = lang.Value,
                }));
                //___m_Dictionary.Add($"albums_{lang.Key}", jArray);
            }
            #endregion
            #region Inject ALBUM1000.json
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
            ___m_Dictionary.Add(AlbumManager.JsonName, jArray);
            #endregion
            #region Inject ALBUM1000_<lang>.json
            foreach (var lang in AlbumManager.Langs)
            {
                jArray = new JArray();
                foreach (var keyValue in AlbumManager.LoadedAlbums)
                {
                    jArray.Add(JObject.FromObject(new
                    {
                        name = keyValue.Value.Info.GetName(lang.Key),
                        author = keyValue.Value.Info.GetAuthor(lang.Key),
                    }));
                }
                ___m_Dictionary.Add($"{AlbumManager.JsonName}_{lang.Key}", jArray);
            }
            #endregion
            #region Inject defaultTag.json
            //text = LoadTextAsset("defaultTag", ref ___m_TextAssets);
            //jArray = JsonUtils.ToArray(text);
            jArray = Singleton<ConfigManager>.instance.GetJson("defaultTag", false);
            // Replace Cute tag
            var music_tag = jArray.Find(o => o.Value<int>("sort_key") == 8);
            music_tag["tag_name"] = JObject.FromObject(AlbumManager.Langs);
            music_tag["tag_picture"] = "https://mdmc.moe/cdn/melon.png";
            music_tag["pic_name"] = "";
            music_tag["music_list"] = JArray.FromObject(AlbumManager.GetAllUid());
            // ___m_Dictionary.Add("defaultTag", jArray);
            #endregion
            #region Inject AssetBundle
            //var dict = SingletonScriptableObject<AssetBundleConfigManager>.instance.dict;
            //foreach (var keyValue in AlbumManager.LoadedAlbums)
            //{
            //    var key = keyValue.Key;
            //    var album = keyValue.Value;
            //    var info = album.Info;

            //    List<string> configList = new List<string>();

            //    var fileName = $"{key}_cover";
            //    assetMapping.Add(fileName, key);
            //    dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
            //    fileName = $"{key}_demo";
            //    assetMapping.Add(fileName, key);
            //    dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
            //    fileName = $"{key}_music";
            //    assetMapping.Add(fileName, key);
            //    dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });

            //    if (!string.IsNullOrEmpty(info.difficulty1))
            //    {
            //        fileName = $"{key}_map1";
            //        assetMapping.Add(fileName, key);
            //        dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
            //    }
            //    if (!string.IsNullOrEmpty(info.difficulty2))
            //    {
            //        fileName = $"{key}_map2";
            //        assetMapping.Add(fileName, key);
            //        dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
            //    }
            //    if (!string.IsNullOrEmpty(info.difficulty3))
            //    {
            //        fileName = $"{key}_map3";
            //        assetMapping.Add(fileName, key);
            //        dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
            //    }
            //    if (!string.IsNullOrEmpty(info.difficulty4))
            //    {
            //        fileName = $"{key}_map4";
            //        assetMapping.Add(fileName, key);
            //        dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
            //    }
            //}
            #endregion

            Addressables.ResourceManager.ResourceProviders.Add(new CustomAlbumAssetResourceProvider());
            Addressables.AddResourceLocator(new CustomAlbumAssetLocator(), remoteCatalogLocation: new CustomAlbumAssetResourceLocation());
            ModLogger.Debug($"Json injected!");
        }

        /// <summary>
        /// Filled empty asset bundle file to every custom albums.
        /// </summary>
        public static void ResourcesManagerInitPrefix(ref Dictionary<string, ResourcesManager.AssetData> ___m_AssetDatas)
        {
            //var name = $"{Application.streamingAssetsPath}/AssetBundles\\Custom_Albums";
            //var ab = new LoadedAssetBundle(AssetBundle.LoadFromMemory(Utils.ReadEmbeddedFile("Resources.EmptyAssetBundle")));
            //___m_LoadedAssetBundles.Add(name, ab);
            var ab = AssetBundle.LoadFromMemory(Utils.ReadEmbeddedFile("Resources.EmptyAssetBundle"));
            foreach (var keyValue in AlbumManager.LoadedAlbums)
            {
                var albumkey = keyValue.Key;
                var album = keyValue.Value;
                var info = album.Info;

                ___m_AssetDatas.Add($"{albumkey}_demo", new ResourcesManager.AssetData()
                {
                    guid = albumkey,
                    path = "CustomAlbums/demo",
                    type = typeof(AudioClip)
                });
                ___m_AssetDatas.Add($"{albumkey}_music", new ResourcesManager.AssetData()
                {
                    guid = albumkey,
                    path = "CustomAlbums/music",
                    type = typeof(AudioClip)
                });
                ___m_AssetDatas.Add($"{albumkey}_cover", new ResourcesManager.AssetData()
                {
                    guid = albumkey,
                    path = "CustomAlbums/cover",
                    type = typeof(Sprite)
                });

                if (!string.IsNullOrEmpty(info.difficulty1))
                    ___m_AssetDatas.Add($"{albumkey}_map1", new ResourcesManager.AssetData()
                    {
                        guid = albumkey,
                        path = "CustomAlbums/map1",
                        type = typeof(StageInfo)
                    });
                if (!string.IsNullOrEmpty(info.difficulty2))
                    ___m_AssetDatas.Add($"{albumkey}_map2", new ResourcesManager.AssetData()
                    {
                        guid = albumkey,
                        path = "CustomAlbums/map2",
                        type = typeof(StageInfo)
                    });
                if (!string.IsNullOrEmpty(info.difficulty3))
                    ___m_AssetDatas.Add($"{albumkey}_map3", new ResourcesManager.AssetData()
                    {
                        guid = albumkey,
                        path = "CustomAlbums/map3",
                        type = typeof(StageInfo)
                    });
                if (!string.IsNullOrEmpty(info.difficulty4))
                    ___m_AssetDatas.Add($"{albumkey}_map4", new ResourcesManager.AssetData()
                    {
                        guid = albumkey,
                        path = "CustomAlbums/map4",
                        type = typeof(StageInfo)
                    });
            }
        }
        /// <summary>
        /// Add original TextAsset to m_TextAssets.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="m_TextAssets"></param>
        /// <returns></returns>
        public static string LoadTextAsset(string name, ref Dictionary<string, TextAsset> m_TextAssets)
        {
            var textAsset = SingletonScriptableObject<ResourcesManager>.instance.LoadFromName<TextAsset>($"{name}.json");
            m_TextAssets.Add(name, textAsset);
            return textAsset.text;
        }
#if false
        /// <summary>
        /// Create a new ABConfig
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ABConfig CreateABConfig(string fileName)
        {
            ABConfig abConfig = new ABConfig();
            abConfig.directory = AlbumManager.SearchPath;
            abConfig.abName = fileName;
            abConfig.directory = "";
            abConfig.fileName = fileName;

            abConfig.extension = ".bms";
            abConfig.type = typeof(StageInfo);
            if (fileName.EndsWith("_cover"))
            {
                abConfig.extension = ".png";
                abConfig.type = typeof(Sprite);
            }
            if (fileName.EndsWith("_music"))
            {
                abConfig.extension = ".mp3";
                abConfig.type = typeof(AudioClip);
            }
            if (fileName.EndsWith("_demo"))
            {
                abConfig.extension = ".mp3";
                abConfig.type = typeof(AudioClip);
            }

            return abConfig;
        }
#endif
    }
}
