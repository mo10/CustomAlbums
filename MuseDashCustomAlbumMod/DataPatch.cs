﻿using Assets.Scripts.GameCore;
using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.GeneralLocalization;
using Assets.Scripts.PeroTools.Managers;
using HarmonyLib;
using ModHelper;
using MuseDashCustomAlbumMod.Managers;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MuseDashCustomAlbumMod.Utils;
using UnityEngine;

namespace MuseDashCustomAlbumMod
{
    public static class DataPatch
    {
        public static Dictionary<string, CustomAlbumInfo> customAssets = new Dictionary<string, CustomAlbumInfo>();
        public static void DoPathcing(Harmony harmony)
        {
            // ConfigManager.Json
            var getJson = AccessTools.Method(typeof(ConfigManager), "GetJson");
            var getJsonPrefix = AccessTools.Method(typeof(DataPatch), "GetJsonPrefix");
            var getJsonPostfix = AccessTools.Method(typeof(DataPatch), "GetJsonPostfix");
            harmony.Patch(getJson, new HarmonyMethod(getJsonPrefix), new HarmonyMethod(getJsonPostfix));
            // AssetBundleConfigManager.Get
            var getAssetBundle = AccessTools.Method(typeof(AssetBundleConfigManager), "Get", new Type[] { typeof(string), typeof(Type) });
            var getAssetBundlePrefix = AccessTools.Method(typeof(DataPatch), "GetAssetBundlePrefix");
            harmony.Patch(getAssetBundle, new HarmonyMethod(getAssetBundlePrefix));
            // AssetBundle.LoadAsset
            var loadAsset = AccessTools.Method(typeof(AssetBundle), "LoadAsset", new Type[] { typeof(string), typeof(Type) });
            var loadAssetPostfix = AccessTools.Method(typeof(DataPatch), "LoadAssetPostfix");
            harmony.Patch(loadAsset, null, new HarmonyMethod(loadAssetPostfix));
            // AssetBundle.LoadAsset
            var loadAssetBundle = AccessTools.Method(typeof(AssetBundleManager), "LoadAssetBundle");
            var loadAssetBundlePostfix = AccessTools.Method(typeof(DataPatch), "LoadAssetBundlePostfix");
            harmony.Patch(loadAssetBundle, null, new HarmonyMethod(loadAssetBundlePostfix));
        }
        // Inject <CustomAlbum.JsonName>.json
        // Inject <CustomAlbum.JsonName>_<lang>.json
        public static void GetJsonPrefix(string name, bool localization, ref Dictionary<string, JArray> ___m_Dictionary)
        {
            string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
            try
            {
                // Load <CustomAlbum.JsonName>.json
                if (!localization && name == CustomInfoManager.JsonName)
                {
                    // Already loaded?
                    if (___m_Dictionary.ContainsKey(CustomInfoManager.JsonName))
                    {
                        return;
                    }
                    // Load all custom albums
                    ModLogger.Debug($"Inject Json: {name}");
                    var albumArray = new JArray();
                    int idx = 0;
                    foreach (var album in CustomInfoManager.GetAlbumInfoDic())
                    {
                        var uid = $"{CustomInfoManager.MUSIC_PACKGE_UID}-{idx}";

                        albumArray.Add(AddNewCustomMetadata(album, uid));
                        RegistCustomAlbumAssert(album);
                        album.Value.uid = uid;
                        idx++;
                    }
                    ___m_Dictionary.Add(name, albumArray);
                    return;
                }
                // Load custom album localization
                // Inject <CustomAlbum.JsonName>_<lang>.json
                string jsonLanguage = $"{CustomInfoManager.JsonName}_{activeOption}";
                if (localization && name == jsonLanguage)
                {
                    // Check if already loaded
                    if (___m_Dictionary.ContainsKey(jsonLanguage))
                    {
                        return;
                    }
                    var albumArray = new JArray();
                    foreach (var albumKvp in CustomInfoManager.GetAlbumInfoDic())
                    {
                        var album = albumKvp.Value;
                        var lang = new JObject();
                        string albumName = null, albumAuthor = null;
                        switch (activeOption)
                        {
                            case "ChineseT":
                                albumName = album.name_zh_hant;
                                albumAuthor = album.author_zh_hant;
                                break;
                            case "ChineseS":
                                albumName = album.name_zh_hans;
                                albumAuthor = album.author_zh_hans;
                                break;
                            case "English":
                                albumName = album.name_en;
                                albumAuthor = album.author_en;
                                break;
                            case "Korean":
                                albumName = album.name_ko;
                                albumAuthor = album.author_ko;
                                break;
                            case "Japanese":
                                albumName = album.name_ja;
                                albumAuthor = album.author_ja;
                                break;
                        }
                        if (album.name != null)
                            albumName = album.name;
                        if (album.author != null)
                            albumAuthor = album.author;
                        lang.Add("name", albumName);
                        lang.Add("author", albumAuthor);
                        albumArray.Add(lang);
                    }

                    ___m_Dictionary.Add(name, albumArray);
                    return;
                }
            }
            catch (Exception ex)
            {
                ModLogger.Debug(ex);
            }

        }
        public static void GetJsonPostfix(string name, bool localization, ref Dictionary<string, JArray> ___m_Dictionary, ref JArray __result)
        {
            string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
            try
            {
                // Load album localization title
                // Inject albums_<lang>.json
                if (localization && name.StartsWith("albums_"))
                {
                    string albums_lang = $"albums_{activeOption}";
                    // Check if already loaded
                    foreach (var obj in ___m_Dictionary[albums_lang])
                    {
                        if (obj.Value<string>("title") == CustomInfoManager.GetTitltTextFromLanguages(activeOption))
                        {
                            return;
                        }
                    }
                    // Add custom l10n title
                    ModLogger.Debug($"Add custom l10n title: {CustomInfoManager.GetTitltTextFromLanguages(activeOption)}");
                    var album_lang = new JObject();
                    album_lang.Add("title", CustomInfoManager.GetTitltTextFromLanguages(activeOption));

                    ___m_Dictionary[albums_lang].Add(album_lang);
                    // return new result
                    __result = ___m_Dictionary[albums_lang];
                    return;
                }
                // Load album
                // Inject albums.json
                if (!localization && name == "albums")
                {
                    // Check if already loaded
                    foreach (var obj in ___m_Dictionary[name])
                    {
                        if (obj.Value<string>("uid") == CustomInfoManager.MusicPackge)
                        {
                            return;
                        }
                    }
                    // Add custom title
                    ModLogger.Debug($"Add custom album json");
                    var album = new JObject();
                    album.Add("uid", CustomInfoManager.MusicPackge);
                    album.Add("title", "Custom Albums");
                    album.Add("prefabsName", "AlbumDiscoNew");
                    album.Add("price", "¥25.00");
                    album.Add("jsonName", CustomInfoManager.JsonName);
                    album.Add("needPurchase", false);
                    album.Add("free", true);

                    ___m_Dictionary[name].Add(album);
                    // Return new result
                    __result = ___m_Dictionary[name];
                    return;
                }
            }
            catch (Exception ex)
            {
                ModLogger.Debug(ex);
            }
        }

        public static void LoadAssetBundlePostfix(string assetBundleName, bool async, ref Dictionary<string, LoadedAssetBundle> ___m_LoadedAssetBundles)
        {
            //ModLogger.Debug($"Load Asset Bundle found:{assetBundleName}");

            if (___m_LoadedAssetBundles.ContainsKey(assetBundleName))
            {
                if (assetBundleName.StartsWith("Custom"))
                    if (___m_LoadedAssetBundles[assetBundleName].assetBundle == null)
                    {
                        ModLogger.Debug($"Load empty asset bundle");
                        ___m_LoadedAssetBundles[assetBundleName].assetBundle = AssetBundle.LoadFromMemory(OtherUtils.ReadEmbeddedFile("Resources.EmptyAssetBundle"));
                    }
            }
        }
        public static void GetAssetBundlePrefix(string assetPath, Type type)
        {
            if (assetPath != null && assetPath.StartsWith(CustomInfoManager.ALBUM_PACK_PATH))
            {
                var dict = SingletonScriptableObject<AssetBundleConfigManager>.instance.dict;
                string dictKey = Path.GetFileNameWithoutExtension(assetPath);

                // Create new ABConfig
                AssetBundleConfigManager.ABConfig newABConfig = new AssetBundleConfigManager.ABConfig();
                newABConfig.directory = CustomInfoManager.ALBUM_PACK_PATH;
                newABConfig.abName = CustomInfoManager.ALBUM_PACK_PATH;
                newABConfig.directory = "";
                newABConfig.fileName = assetPath;

                newABConfig.type = type;
                
                newABConfig.extension = ".bms";
                if (assetPath.EndsWith("_cover"))
                {
                    newABConfig.extension = ".png";
                }
                if (assetPath.EndsWith("_music"))
                {
                    newABConfig.extension = ".mp3";
                }
                if (assetPath.EndsWith("_demo"))
                {
                    newABConfig.extension = ".mp3";
                }
                if (dict.TryGetValue(dictKey, out List<AssetBundleConfigManager.ABConfig> abConfigs))
                {
                    foreach (var config in abConfigs)
                    {
                        if (config.type == type)
                        {
                            // Exist ABConfig, Do nothing.
                            return;
                        }
                    }
                    // Add other type of value
                    abConfigs.Add(newABConfig);
                    ModLogger.Debug($"Append asset key: {dictKey} type: {type}");
                }
                else
                {
                    // Not exist dictKey, Create new
                    dict.Add(dictKey, new List<AssetBundleConfigManager.ABConfig>() { newABConfig });
                    ModLogger.Debug($"Add asset dict key: {dictKey} type: {type}");
                }
                // ModLogger.Debug($"Not found asset: {assetPath} type: {type}");
            }
        }
        public static void LoadAssetPostfix(string name, Type type, ref UnityEngine.Object __result)
        {
            if (__result == null)
            {
                if (customAssets.TryGetValue(name, out CustomAlbumInfo albumInfo))
                {
                    // Load cover image 
                    if (type == typeof(Sprite))
                    {
                        __result = albumInfo.GetCoverSprite();
                    }
                    // Load audio
                    else if (type == typeof(AudioClip))
                    {
                        if (name.EndsWith("_demo.mp3"))
                        {
                            __result = albumInfo.GetAudioClip("demo");
                        }
                        else if (name.EndsWith("_music.mp3"))
                        {
                            __result = albumInfo.GetAudioClip("music");
                        }
                    }
                    // Load map
                    else if (type == typeof(StageInfo))
                    {
                        if (name.EndsWith("_map1.bms"))
                        {
                            __result = albumInfo.GetMap(1);
                        }
                        else if (name.EndsWith("_map2.bms"))
                        {
                            __result = albumInfo.GetMap(2);
                        }
                        else if (name.EndsWith("_map3.bms"))
                        {
                            __result = albumInfo.GetMap(3);
                        }
                        else if (name.EndsWith("_map4.bms"))
                        {
                            __result = albumInfo.GetMap(4);
                        }
                    }
                }

                if(__result == null)
                    ModLogger.Debug($"Asset not found: {name} type: {type}");
            }
        }

        public static void AddABConfig()
        {

        }
        public static JObject AddNewCustomMetadata(KeyValuePair<string, CustomAlbumInfo> valuePair, string uid)
        {
            var metadata = new JObject();

            metadata.Add("uid", uid);

            // Custom_Albums_package_music
            string path = $"{CustomInfoManager.ALBUM_PACK_PATH}_{valuePair.Key}".Replace('\\', '_').Replace('/', '_').Replace('.', '_');
            metadata.Add("music", $"{path}_music");
            metadata.Add("demo", $"{path}_demo");
            metadata.Add("cover", $"{path}_cover");
            metadata.Add("noteJson", $"{path}_map");
            metadata.Add("scene", valuePair.Value.scene);
            metadata.Add("bpm", valuePair.Value.bpm);

            // If set "name", ingore l10n options
            if (!string.IsNullOrEmpty(valuePair.Value.name))
            {
                metadata.Add("name", valuePair.Value.name);
            }
            else
            {
                metadata.Add("name", valuePair.Value.name_zh_hans);
            }
            // If set "author", ingore l10n options
            if (!string.IsNullOrEmpty(valuePair.Value.author))
            {
                metadata.Add("author", valuePair.Value.author);
            }
            else
            {
                metadata.Add("author", valuePair.Value.author_zh_hans);
            }

            if(!string.IsNullOrEmpty(valuePair.Value.levelDesigner))
            {
                metadata.Add("levelDesigner", valuePair.Value.levelDesigner);
            }
            else
            {
                if (!string.IsNullOrEmpty(valuePair.Value.levelDesigner1))
                {
                    metadata.Add("levelDesigner1", valuePair.Value.levelDesigner1);
                }
                if (!string.IsNullOrEmpty(valuePair.Value.levelDesigner2))
                {
                    metadata.Add("levelDesigner2", valuePair.Value.levelDesigner2);
                }
                if (!string.IsNullOrEmpty(valuePair.Value.levelDesigner3))
                {
                    metadata.Add("levelDesigner3", valuePair.Value.levelDesigner3);
                }
                if (!string.IsNullOrEmpty(valuePair.Value.levelDesigner4))
                {
                    metadata.Add("levelDesigner4", valuePair.Value.levelDesigner4);
                }
            }
            if (!string.IsNullOrEmpty(valuePair.Value.difficulty1))
            {
                metadata.Add("difficulty1", valuePair.Value.difficulty1);
            }
            if (!string.IsNullOrEmpty(valuePair.Value.difficulty2))
            {
                metadata.Add("difficulty2", valuePair.Value.difficulty2);
            }
            if (!string.IsNullOrEmpty(valuePair.Value.difficulty3))
            {
                metadata.Add("difficulty3", valuePair.Value.difficulty3);
            }
            if (!string.IsNullOrEmpty(valuePair.Value.difficulty4))
            {
                metadata.Add("difficulty4", valuePair.Value.difficulty4);
            }

            metadata.Add("unlockLevel", valuePair.Value.unlockLevel);



            ModLogger.Debug(metadata);
            return metadata;
        }

        public static void RegistCustomAlbumAssert(KeyValuePair<string, CustomAlbumInfo> valuePair)
        {
            string path = $"{CustomInfoManager.ALBUM_PACK_PATH}_{valuePair.Key}".Replace('\\', '_').Replace('/', '_').Replace('.', '_');

            // Regist asset path
            customAssets.Add($"Assets/Static Resources/{path}_music.mp3", valuePair.Value);
            customAssets.Add($"Assets/Static Resources/{path}_demo.mp3", valuePair.Value);
            customAssets.Add($"Assets/Static Resources/{path}_cover.png", valuePair.Value);
            if (!string.IsNullOrEmpty(valuePair.Value.difficulty1))
            {
                customAssets.Add($"Assets/Static Resources/{path}_map1.bms", valuePair.Value);
            }
            if (!string.IsNullOrEmpty(valuePair.Value.difficulty2))
            {
                customAssets.Add($"Assets/Static Resources/{path}_map2.bms", valuePair.Value);
            }
            if (!string.IsNullOrEmpty(valuePair.Value.difficulty3))
            {
                customAssets.Add($"Assets/Static Resources/{path}_map3.bms", valuePair.Value);
            }
            if (!string.IsNullOrEmpty(valuePair.Value.difficulty4))
            {
                customAssets.Add($"Assets/Static Resources/{path}_map4.bms", valuePair.Value);
            }
        }
    }
}