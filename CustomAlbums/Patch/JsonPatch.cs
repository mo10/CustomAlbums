using Assets.Scripts.GameCore;
using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
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

            //ModLogger.Debug($"Application.streamingAssetsPath: {Application.streamingAssetsPath}");
            //ModLogger.Debug($"Application.persistentDataPath: {Application.persistentDataPath}");
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

                if (!string.IsNullOrEmpty(info.difficulty1))
                {
                    fileName = $"{key}_map1";
                    assetMapping.Add(fileName, key);
                    dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
                }
                if (!string.IsNullOrEmpty(info.difficulty2))
                {
                    fileName = $"{key}_map2";
                    assetMapping.Add(fileName, key);
                    dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
                }
                if (!string.IsNullOrEmpty(info.difficulty3))
                {
                    fileName = $"{key}_map3";
                    assetMapping.Add(fileName, key);
                    dict.Add(fileName, new List<ABConfig>() { CreateABConfig(fileName) });
                }
                if (!string.IsNullOrEmpty(info.difficulty4))
                {
                    fileName = $"{key}_map4";
                    assetMapping.Add(fileName, key);
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
