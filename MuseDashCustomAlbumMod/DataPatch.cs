using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.GeneralLocalization;
using Assets.Scripts.PeroTools.Managers;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var getAssetBundlePostfix = AccessTools.Method(typeof(DataPatch), "GetAssetBundlePostfix");
            harmony.Patch(getAssetBundle, null, new HarmonyMethod(getAssetBundlePostfix));
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
                if (!localization && name == CustomAlbum.JsonName)
                {
                    // Already loaded?
                    if (___m_Dictionary.ContainsKey(CustomAlbum.JsonName))
                    {
                        return;
                    }
                    // Load all custom albums
                    ModLogger.Debug($"Inject Json: {name}");
                    var albumArray = new JArray();
                    int idx = 0;
                    foreach (var album in CustomAlbum.Albums)
                    {
                        var uid = $"{CustomAlbum.MusicPackgeUid}-{idx}";
                        
                        albumArray.Add(AddNewCustomMetadata(album, uid));
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
                    ModLogger.Debug($"Add custom album json");
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

        public static void LoadAssetBundlePostfix(string assetBundleName, bool async, ref Dictionary<string, LoadedAssetBundle> ___m_LoadedAssetBundles)
        {
            //ModLogger.Debug($"Load Asset Bundle found:{assetBundleName}");

            if (!___m_LoadedAssetBundles.ContainsKey(assetBundleName))
            {
                //ModLogger.Debug($"Asset Not found:{assetBundleName}");
            }
            else
            {
                if (assetBundleName.StartsWith("Custom"))
                    if (___m_LoadedAssetBundles[assetBundleName].assetBundle == null)
                    {
                        ModLogger.Debug($"Add empty asset bundle: {assetBundleName}");
                        ___m_LoadedAssetBundles[assetBundleName].assetBundle = AssetBundle.LoadFromMemory(Utils.ReadEmbeddedFile("Resources.EmptyAssetBundle"));
                    }
            }
        }
        public static void GetAssetBundlePostfix(string assetPath, Type type, ref AssetBundleConfigManager.ABConfig __result)
        {
            var dict = SingletonScriptableObject<AssetBundleConfigManager>.instance.dict;
            string dictKey = Path.GetFileNameWithoutExtension(assetPath);
            if (__result == null)
            {
                ModLogger.Debug($"Create abConfig: {assetPath} type: {type}");
                if (assetPath.StartsWith(CustomAlbum.AlbumPackPath))
                {
                    AssetBundleConfigManager.ABConfig newABConfig = new AssetBundleConfigManager.ABConfig();
                    newABConfig.directory = CustomAlbum.AlbumPackPath;
                    newABConfig.abName = CustomAlbum.AlbumPackPath;
                    newABConfig.directory = "";
                    newABConfig.fileName = assetPath;

                    newABConfig.type = type;
                    
                    if (assetPath.EndsWith("_cover"))
                    {
                        newABConfig.extension = ".png";
                    }
                    if (assetPath.EndsWith("_music"))
                    {
                        newABConfig.extension = ".wav";
                    }
                    if (assetPath.EndsWith("_demo"))
                    {
                        newABConfig.extension = ".wav";
                    }
                    if (assetPath.EndsWith("_map"))
                    {
                        newABConfig.extension = ".json";
                    }

                    AssetBundleConfigManager.ABConfig existABConfig = null;
                    if (dict.TryGetValue(dictKey, out List<AssetBundleConfigManager.ABConfig> abConfigs))
                    {
                        // Find exist dict key
                        foreach (var config in abConfigs)
                        {
                            if(config.type == type)
                            {
                                existABConfig = config;
                                ModLogger.Debug($"Found item {dictKey} type:{type}");
                                break;
                            }
                        }
                        // Exist key, add other value
                        if (existABConfig == null)
                        {
                            abConfigs.Add(newABConfig);
                            existABConfig = newABConfig;
                            ModLogger.Debug($"Add item {dictKey} type:{type}");
                        }
                    }
                    else
                    {
                        // Add new dict item
                        dict.Add(dictKey, new List<AssetBundleConfigManager.ABConfig>() { newABConfig });
                        ModLogger.Debug($"Add dict {dictKey} type: {type}");
                    }
                    __result = existABConfig;
                    ModLogger.Debug($"Return custom asset: {dictKey} type: {type}");
                    return;
                }
                ModLogger.Debug($"Not found asset: {assetPath} type: {type}");
            }
        }
        public static void LoadAssetPostfix(string name, Type type, ref UnityEngine.Object __result)
        {
            // && name.StartsWith($"Assets/Static Resources/{CustomAlbum.AlbumPackPath}/")
            if (__result == null)
            {
                if (customAssets.TryGetValue(name,out CustomAlbumInfo albumInfo))
                {
                    if (type == typeof(UnityEngine.Sprite))
                    {
                        // Load cover 
                        Texture2D newTex = new Texture2D(440, 440);
                        ImageConversion.LoadImage(newTex, albumInfo.GetCover());
                        Sprite newSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0, 0), 100);
                        __result = newSprite;

                        return;
                    }
                }
                ModLogger.Debug($"Asset not found: {name} type: {type}");

                //string file = name.Replace($"Assets/Static Resources/{CustomAlbum.AlbumPackPath}/", "");
                //ModLogger.Debug($"Asset not found: {name} type: {type} target: {file}");
            }
        }

        public static void AddABConfig()
        {

        }
        public static JObject AddNewCustomMetadata(KeyValuePair<string,CustomAlbumInfo> valuePair,string uid)
        {
            var metadata = new JObject();

            metadata.Add("uid", uid);

            // If set "name", ingore l10n options
            if (valuePair.Value.name != null)
                metadata.Add("name", valuePair.Value.name);
            else
                metadata.Add("name", valuePair.Value.name_zh_hans);

            // If set "author", ingore l10n options
            if (valuePair.Value.author != null)
                metadata.Add("author", valuePair.Value.author);
            else
                metadata.Add("author", valuePair.Value.author_zh_hans);

            metadata.Add("bpm", valuePair.Value.bpm);

            // Custom_Albums_package_music
            string path = $"{CustomAlbum.AlbumPackPath}_{valuePair.Key}".Replace('\\', '_').Replace('/', '_').Replace('.', '_');
            metadata.Add("music", $"{path}_music");
            metadata.Add("demo", $"{path}_demo");
            metadata.Add("cover", $"{path}_cover");
            metadata.Add("noteJson", $"{path}_map");
            metadata.Add("scene", valuePair.Value.scene);

            customAssets.Add($"Assets/Static Resources/{path}_music.wav", valuePair.Value);
            customAssets.Add($"Assets/Static Resources/{path}_demo.wav", valuePair.Value);
            customAssets.Add($"Assets/Static Resources/{path}_cover.png", valuePair.Value);
            customAssets.Add($"Assets/Static Resources/{path}_map.json", valuePair.Value);

            if (valuePair.Value.levelDesigner1 != null || valuePair.Value.levelDesigner1 != "")
            {
                // level designer of difficulty 1 to 4 
                metadata.Add("levelDesigner1", valuePair.Value.levelDesigner1);
                metadata.Add("levelDesigner2", valuePair.Value.levelDesigner2);
                metadata.Add("levelDesigner3", valuePair.Value.levelDesigner3);
                metadata.Add("levelDesigner4", valuePair.Value.levelDesigner4);
            }
            else
            {
                // level designer of all difficulties
                metadata.Add("levelDesigner", valuePair.Value.levelDesigner);
            }

            metadata.Add("difficulty1", valuePair.Value.difficulty1);
            metadata.Add("difficulty2", valuePair.Value.difficulty2);
            metadata.Add("difficulty3", valuePair.Value.difficulty3);
            metadata.Add("difficulty4", valuePair.Value.difficulty4);

            metadata.Add("unlockLevel", valuePair.Value.unlockLevel);

            return metadata;
        }
    }
}
