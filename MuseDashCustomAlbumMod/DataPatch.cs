using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.GeneralLocalization;
using Assets.Scripts.PeroTools.Managers;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MuseDashCustomAlbumMod
{
    public static class DataPatch
    {
        public static void DoPathcing(Harmony harmony)
        {
            // ConfigManager.Json
            var getJson = AccessTools.Method(typeof(ConfigManager), "GetJson");
            var getJsonPrefix = AccessTools.Method(typeof(DataPatch), "GetJsonPrefix");
            var getJsonPostfix = AccessTools.Method(typeof(DataPatch), "GetJsonPostfix");
            harmony.Patch(getJson, new HarmonyMethod(getJsonPrefix), new HarmonyMethod(getJsonPostfix));
            // AssetBundleConfigManager.Get
            var getAssetBundle = AccessTools.Method(typeof(AssetBundleConfigManager), "Get", new Type[] { typeof(string), typeof(Type) });
            var getAssetBundlePostfix = AccessTools.Method(typeof(DataPatch), "GetAssetBundlePostfix");
            harmony.Patch(getAssetBundle, null, new HarmonyMethod(getAssetBundlePostfix));
            // AssetBundle.LoadAsset
            var loadAsset = AccessTools.Method(typeof(AssetBundle), "LoadAsset", new Type[] { typeof(string), typeof(Type) });
            var loadAssetPostfix = AccessTools.Method(typeof(DataPatch), "LoadAssetPostfix");
            harmony.Patch(loadAsset, null, new HarmonyMethod(loadAssetPostfix));
        }
        public static void GetJsonPrefix(string name, bool localization, ref Dictionary<string, JArray> ___m_Dictionary)
        {
            string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
            try
            {
                // Load <CustomAlbum.JsonName>.json
                if (!localization && name == CustomAlbum.JsonName)
                {
                    // Already loaded?
                    if (___m_Dictionary.ContainsKey(CustomAlbum.JsonName))
                    {
                        return;
                    }
                    // Load all custom albums
                    ModLogger.Debug($"Load custom songs list: {name}");
                    var albumArray = new JArray();
                    int idx = 0;
                    foreach (var album in CustomAlbum.Albums)
                    {
                        var uid = $"{CustomAlbum.MusicPackgeUid}-{idx}";
                        var info = new JObject();

                        info.Add("uid", uid);

                        if (album.Value.name != null)
                            info.Add("name", album.Value.name);
                        else
                            info.Add("name", album.Value.name_zh_hans);
                        if (album.Value.author != null)
                            info.Add("author", album.Value.author);
                        else
                            info.Add("author", album.Value.author_zh_hans);

                        info.Add("bpm", album.Value.bpm);
                        info.Add("music", $"{album.Key}_music");
                        info.Add("demo", $"{album.Key}_demo");
                        info.Add("cover", $"{album.Key}_cover");
                        info.Add("noteJson", $"{album.Key}_map");
                        info.Add("scene", album.Value.scene);

                        if (album.Value.levelDesigner1 != null || album.Value.levelDesigner1 != "")
                        {
                            // level designer of difficulty 1 to 4 
                            info.Add("levelDesigner1", album.Value.levelDesigner1);
                            info.Add("levelDesigner2", album.Value.levelDesigner2);
                            info.Add("levelDesigner3", album.Value.levelDesigner3);
                            info.Add("levelDesigner4", album.Value.levelDesigner4);
                        }
                        else
                        {
                            // level designer of all difficulties
                            info.Add("levelDesigner", album.Value.levelDesigner);
                        }

                        info.Add("difficulty1", album.Value.difficulty1);
                        info.Add("difficulty2", album.Value.difficulty2);
                        info.Add("difficulty3", album.Value.difficulty3);
                        info.Add("difficulty4", album.Value.difficulty4);

                        info.Add("unlockLevel", album.Value.unlockLevel);

                        albumArray.Add(info);
                        album.Value.Uid = uid;
                        idx++;
                    }
                    ___m_Dictionary.Add(name, albumArray);
                    return;
                }
                // Load custom album localization
                // Inject <CustomAlbum.JsonName>_<lang>.json
                string jsonLanguage = $"{CustomAlbum.JsonName}_{activeOption}";
                if (localization && name == jsonLanguage)
                {
                    // Check if already loaded
                    if (___m_Dictionary.ContainsKey(jsonLanguage))
                    {
                        return;
                    }
                    var albumArray = new JArray();
                    foreach (var album in CustomAlbum.Albums)
                    {
                        var lang = new JObject();
                        string albumName = null, albumAuthor = null;
                        switch (activeOption)
                        {
                            case "ChineseT":
                                albumName = album.Value.name_zh_hant;
                                albumAuthor = album.Value.author_zh_hant;
                                break;
                            case "ChineseS":
                                albumName = album.Value.name_zh_hans;
                                albumAuthor = album.Value.author_zh_hans;
                                break;
                            case "English":
                                albumName = album.Value.name_en;
                                albumAuthor = album.Value.author_en;
                                break;
                            case "Korean":
                                albumName = album.Value.name_ko;
                                albumAuthor = album.Value.author_ko;
                                break;
                            case "Japanese":
                                albumName = album.Value.name_ja;
                                albumAuthor = album.Value.author_ja;
                                break;
                        }
                        if (album.Value.name != null)
                            albumName = album.Value.name;
                        if (album.Value.author != null)
                            albumAuthor = album.Value.author;
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
                        if (obj.Value<string>("title") == CustomAlbum.Languages[activeOption])
                        {
                            return;
                        }
                    }
                    // Add custom l10n title
                    ModLogger.Debug($"Add custom l10n title: {CustomAlbum.Languages[activeOption]}");
                    var album_lang = new JObject();
                    album_lang.Add("title", CustomAlbum.Languages[activeOption]);

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
                        if (obj.Value<string>("uid") == CustomAlbum.MusicPackge)
                        {
                            return;
                        }
                    }
                    // Add custom title
                    ModLogger.Debug($"Add custom album");
                    var album = new JObject();
                    album.Add("uid", CustomAlbum.MusicPackge);
                    album.Add("title", "Custom Albums");
                    album.Add("prefabsName", "AlbumDiscoNew");
                    album.Add("price", "¥25.00");
                    album.Add("jsonName", CustomAlbum.JsonName);
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
        public static void GetAssetBundlePostfix(string assetPath, Type type, ref AssetBundleConfigManager.ABConfig __result)
        {
            var dict = SingletonScriptableObject<AssetBundleConfigManager>.instance.dict;

            if (__result == null)
            {
                if (assetPath.StartsWith(CustomAlbum.AlbumPackPath))
                {
                    if (assetPath.EndsWith("_cover"))
                    {
                        AssetBundleConfigManager.ABConfig abConfig = new AssetBundleConfigManager.ABConfig();
                        abConfig.extension = ".png";
                        abConfig.fileName = null;
                        abConfig.abName = CustomAlbum.AlbumPackPath;
                        abConfig.directory = $"{CustomAlbum.AlbumPackPath}_cover";
                        abConfig.type = type;

                    }
                }
                ModLogger.Debug($"Not found asset:{assetPath} type:{type}");
            }
        }
        public static void LoadAssetPostfix(string name, Type type, ref UnityEngine.Object __result)
        {
            if (__result == null)
            {
                ModLogger.Debug($"Not found name:{name} type:{type}");

            }
        }
    }
}
