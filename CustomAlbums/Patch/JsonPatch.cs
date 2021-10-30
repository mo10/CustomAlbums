using Assets.Scripts.GameCore;
using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using CustomAlbums.Data;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using static Assets.Scripts.PeroTools.Managers.AssetBundleConfigManager;

namespace CustomAlbums.Patch
{
    class JsonPatch
    {
        public static Dictionary<string, string> assetMapping = new Dictionary<string, string>();
        public static void DoPatching(Harmony harmony)
        {
            // ConfigManager.Init
            var method = AccessTools.Method(typeof(ConfigManager), "Init");
            var methodPrefix = AccessTools.Method(typeof(JsonPatch), "ConfigManagerInitPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // AssetBundleManager.Init
            method = AccessTools.Method(typeof(AssetBundleManager), "Init");
            methodPrefix = AccessTools.Method(typeof(JsonPatch), "AssetBundleManagerInitPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));

            ModLogger.Debug($"Application.streamingAssetsPath: {Application.streamingAssetsPath}/AssetBundles/Custom_Albums");
            ModLogger.Debug($"Application.persistentDataPath: {Application.persistentDataPath}");
        }

        /// <summary>
        /// Inject albums.json album_<lang>.json ALBUM1000.json ALBUM1000_<lang>.json
        /// </summary>
        /// <param name="___m_Dictionary"></param>
        /// <param name="___m_TextAssets"></param>
        public static void ConfigManagerInitPrefix(ref Dictionary<string, JArray> ___m_Dictionary, ref Dictionary<string, TextAsset> ___m_TextAssets)
        {
            #region Inject albums.json
            var text = LoadTextAsset("albums", ref ___m_TextAssets);
            JArray jArray = JsonUtils.ToArray(text);
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
            ___m_Dictionary.Add("albums", jArray);
            #endregion
            #region Inject albums_<lang>.json
            foreach (var lang in AlbumManager.Langs)
            {
                text = LoadTextAsset($"albums_{lang.Key}", ref ___m_TextAssets);
                jArray = JsonUtils.ToArray(text);
                jArray.Add(JObject.FromObject(new
                {
                    title = lang.Value,
                }));
                ___m_Dictionary.Add($"albums_{lang.Key}", jArray);
            }
            #endregion
            #region Inject ALBUM1000.json
            jArray = new JArray();
            foreach (var keyValue in AlbumManager.LoadedAlbums)
            {
                var key = keyValue.Key;
                var album = keyValue.Value;
                var info = album.Info;
                var jObject = new JObject
                {
                    { "name", info.GetName() },
                    { "author", info.GetAuthor() },
                    { "music", $"{key}_music" },
                    { "demo", $"{key}_demo" },
                    { "cover", $"{key}_cover" },
                    { "noteJson", $"{key}_map" }
                };
                
                foreach (PropertyInfo prop in typeof(AlbumInfo).GetProperties())
                {
                    string propName = prop.Name;
                    string propVal = (string) prop.GetValue(info, null);
                    if (!(propName.StartsWith("name") || propName.StartsWith("author"))
                        && !string.IsNullOrEmpty(propVal))
                    {
                        jObject.Add(propName, propVal);
                    }
                }

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
            text = LoadTextAsset("defaultTag", ref ___m_TextAssets);
            jArray = JsonUtils.ToArray(text);
            jArray.Add(JObject.FromObject(new
            {
                object_id = "3d2be24f837b2ec1e5e119bb",
                created_at = "2021-10-24T00:00:00.000Z",
                updated_at = "2021-10-24T00:00:00.000Z",
                tag_name = JObject.FromObject(AlbumManager.Langs),
                tag_picture = "https://mdmc.moe/cdn/melon.png",
                pic_name = "ImgCollab",
                music_list = AlbumManager.GetAllUid(),
                anchor_pattern = false,
                sort_key = jArray.Count + 1,
            }));
            #endregion
            #region Inject AssetBundle
            var dict = SingletonScriptableObject<AssetBundleConfigManager>.instance.dict;
            foreach (var keyValue in AlbumManager.LoadedAlbums)
            {
                var key = keyValue.Key;
                var album = keyValue.Value;
                var info = album.Info;

                List<string> configList = new List<string>();

                var fileName = $"{key}_cover";
                assetMapping.Add(fileName, key);
                dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
                fileName = $"{key}_demo";
                assetMapping.Add(fileName, key);
                dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
                fileName = $"{key}_music";
                assetMapping.Add(fileName, key);
                dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });

                // add difficulty configs
                foreach (PropertyInfo prop in typeof(AlbumInfo).GetProperties()
                                        .Where(x => x.Name.StartsWith("difficulty")))
                {
                    string suffixNum = prop.Name.Replace("difficulty", "");
                    fileName = $"{key}_map{suffixNum}";
                    dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
                }
            }
            #endregion
            ModLogger.Debug($"Json injected!");
        }

        public static void AssetBundleManagerInitPrefix(ref Dictionary<string, LoadedAssetBundle> ___m_LoadedAssetBundles)
        {
            //var name = $"{Application.streamingAssetsPath}/AssetBundles\\Custom_Albums";
            //var ab = new LoadedAssetBundle(AssetBundle.LoadFromMemory(Utils.ReadEmbeddedFile("Resources.EmptyAssetBundle")));
            //___m_LoadedAssetBundles.Add(name, ab);
            foreach (var keyValue in AlbumManager.LoadedAlbums)
            {
                var key = keyValue.Key;
                var album = keyValue.Value;
                var info = album.Info;

                var ab = new LoadedAssetBundle(AssetBundle.LoadFromMemory(Utils.ReadEmbeddedFile("Resources.EmptyAssetBundle")));
                ___m_LoadedAssetBundles.Add(key, ab);

            }

        }
        public static string LoadTextAsset(string name, ref Dictionary<string, TextAsset> m_TextAssets)
        {
            var textAsset = Singleton<AssetBundleManager>.instance.LoadFromName<TextAsset>($"{name}.json");
            m_TextAssets.Add(name, textAsset);
            return textAsset.text;
        }

        public static ABConfig CreateABConfig(string fileName)
        {
            // Create new ABConfig
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
    }
}
