using Assets.Scripts.GameCore;
using Assets.Scripts.GameCore.Managers;
using Assets.Scripts.PeroTools.AssetBundles;
using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.GeneralLocalization;
using Assets.Scripts.PeroTools.GeneralLocalization.Modles;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Components;
using Assets.Scripts.PeroTools.Nice.Variables;
using Assets.Scripts.UI.Controls;
using Assets.Scripts.UI.Panels;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace MuseDashCustomAlbumMod
{
    public static class CustomAlbum
    {
        private static bool IsInited = false;
        private static Dictionary<string, string> language = new Dictionary<string, string>();
        public static void DoPatching()
        {
            var harmony = new Harmony("com.github.mo10.customalbum");

            var StagePreWarm = AccessTools.Method(typeof(PnlStage), "PreWarm", new Type[] { typeof(int) });
            var StagePreWarmPrefix = AccessTools.Method(typeof(CustomAlbum), "StagePreWarmPrefix");
            harmony.Patch(StagePreWarm, new HarmonyMethod(StagePreWarmPrefix));

            //var InitAlbumDatas = AccessTools.Method(typeof(PnlStage), "InitAlbumDatas");
            //var InitAlbumDatasPrefix = AccessTools.Method(typeof(CustomAlbum), "InitAlbumDatas");
            //harmony.Patch(InitAlbumDatas, new HarmonyMethod(InitAlbumDatasPrefix));

            //var methodRebuildChildren = AccessTools.Method(typeof(FancyScrollView), "RebuildChildren");
            //var methodRCPrefix = AccessTools.Method(typeof(CustomAlbum), "RebuildChildren");
            //harmony.Patch(methodRebuildChildren, new HarmonyMethod(methodRCPrefix));

            var GetJson = AccessTools.Method(typeof(ConfigManager), "GetJson");
            var GetJsonPrefix = AccessTools.Method(typeof(CustomAlbum), "GetJsonPrefix");
            var GetJsonPostfix = AccessTools.Method(typeof(CustomAlbum), "GetJsonPostfix");
            harmony.Patch(GetJson, new HarmonyMethod(GetJsonPrefix), new HarmonyMethod(GetJsonPostfix));

            //var WeekFreeControl = AccessTools.Method(typeof(WeekFreeControl), "OnEnable");
            //var WeekFreeControlPrefix = AccessTools.Method(typeof(CustomAlbum), "OnEnable");
            //harmony.Patch(WeekFreeControl, new HarmonyMethod(WeekFreeControlPrefix));

            //var LoadFromName = AccessTools.Method(typeof(AssetBundleManager), "LoadFromName", new Type[] { typeof(string) }, new Type[] { typeof(UnityEngine.TextAsset) });
            //var LoadFromNamePrefix = AccessTools.Method(typeof(CustomAlbum), "LoadFromName");
            //harmony.Patch(LoadFromName, new HarmonyMethod(LoadFromNamePrefix));

            language.Add("ChineseT", "自定義谱面");
            language.Add("ChineseS", "自定义谱面");
            language.Add("English", "Custom Albums");
            language.Add("Korean", "Custom Albums");
            language.Add("Japanese", "Custom Albums");
        }
        private static byte[] ReadEmbeddedFile(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            byte[] buffer;
            using (var stream = assembly.GetManifestResourceStream($"MuseDashCustomAlbumMod.{file}"))
            {
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                // TODO: use the buffer that was read
            }
            return buffer;
        }
        private static void AddAlbumTagCell(ref Transform albumFancyScrollViewContent)
        {
            if (albumFancyScrollViewContent.Find($"AlbumTagCell_Custom") == null)
            {
                string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");

                // Clone gameobject
                var sourceObj = albumFancyScrollViewContent.GetChild(albumFancyScrollViewContent.childCount - 2).gameObject;
                var newObj = GameObject.Instantiate(sourceObj, sourceObj.transform.parent);
                // Initialization
                newObj.name = $"AlbumTagCell_Custom";
                newObj.transform.SetSiblingIndex(albumFancyScrollViewContent.childCount - 1);
                // Localization
                var txtTagNameObj = newObj.transform.Find("TxtTagName");
                if (txtTagNameObj != null)
                {
                    txtTagNameObj.GetComponent<UnityEngine.UI.Text>().text = language[activeOption];
                    var l10n = txtTagNameObj.GetComponent<Localization>();
                    foreach (var opt in l10n.optionPairs)
                    {
                        ((TextOption)opt.option).value = language[opt.optionEntry.name];
                        ModLogger.Debug($" opt:{opt.optionEntry.name}  val:{((TextOption)opt.option).value.ToString()}");
                    }
                }
                // AlbumCell
                var newObjAlbumCell = newObj.GetComponent<AlbumCell>();
                var sourceObjAlbumCell = sourceObj.GetComponent<AlbumCell>();
                var sourceIcon = sourceObj.transform.Find("TxtTagName").transform.Find("ImgCollab").GetComponent<Image>();
                // Icon GameObject
                var icon = txtTagNameObj.transform.Find("ImgCollab");
                var img = icon.GetComponent<Image>();
                try
                {
                    if (icon != null)
                    {
                        Sprite newSprite = new Sprite();
                        Texture2D newTex = new Texture2D(512, 512);

                        icon.name = "ImgCustom";
                        // Load default image from embedded resources.
                        ImageConversion.LoadImage(newTex, ReadEmbeddedFile("Resources.AlbumIcon.png"));
                        //newTex.LoadImage(ReadEmbeddedFile("Resources.AlbumIcon.png"));
                        newSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0, 0), 100);
                        newSprite.name = "ImgCustom";

                        img.sprite = newSprite;
                    }
                }catch(Exception ex)
                {
                    ModLogger.Debug(ex);
                }
                // Print                
                //if (albumCell)
                //{

                //    ModLogger.Debug($"uid: {albumCell.uid} data_index:{albumCell.GetDataIndex()} ");
                //    //albumCell.SetUid("custom");
                //    //albumCell.SetLock(true);
                //}
                ModLogger.Debug("Add custom map tab");

            }
        }
        private static void AddAlbumTagData(ref List<PnlStage.albumInfo> m_AllAlbumTagData)
        {
            m_AllAlbumTagData.Add(new PnlStage.albumInfo
            {
                uid = "custom",
                name = "自定义谱面",
                list = new List<string>(),
                nameList = new List<string>(),
                isWeekFree = false,
                isNew = false
            });
        }
        private static void AddAlbumDatas(ref Dictionary<string, JArray> m_Dictionary)
        {

        }
        public static void StagePreWarmPrefix(int slice, ref List<PnlStage.albumInfo> ___m_AllAlbumTagData, ref Transform ___albumFancyScrollViewContent, ref List<GameObject> ___m_AlbumFSVCells)
        {
            if (slice != 0)
            {
                return;
            }

            AddAlbumTagCell(ref ___albumFancyScrollViewContent);
            AddAlbumTagData(ref ___m_AllAlbumTagData);

            //ModLogger.Debug($"slice:{slice}");
            //ModLogger.Debug($"albumFancyScrollViewContent.Count: {___albumFancyScrollViewContent.childCount}");
            //ModLogger.Debug($"m_AlbumFSVCells.Count: {___m_AlbumFSVCells.Count}");

            //for (int i = 0; i < ___albumFancyScrollViewContent.childCount; i++)
            //{
            //    CompTrack(___albumFancyScrollViewContent.GetChild(i).gameObject);
            //}
            //foreach (var albumTag in ___m_AllAlbumTagData)
            //{
            //    ModLogger.Debug($"{albumTag.uid} {albumTag.name} {albumTag.list} {albumTag.nameList} {albumTag.isWeekFree} {albumTag.isNew}");
            //}
        }
        public static void CompTrack(GameObject gameObject, int layer = 0)
        {
            foreach (var comps in gameObject.GetComponents(typeof(object)))
            {
                ModLogger.Debug($"{layer} Name:{gameObject.name} Component:{comps.GetType()}");
            }
            if (gameObject.transform.childCount > 0)
            {
                ++layer;
                for (var i = 0; i < gameObject.transform.childCount; i++)
                {
                    CompTrack(gameObject.transform.GetChild(i).gameObject, layer);
                }
            }
        }
        public static void GetJsonPrefix(string name, bool localization,ref Dictionary<string, JArray> ___m_Dictionary)
        {
            ModLogger.Debug($"name:{name} l10n:{localization}");
            try
            {
                string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");

                if (name.StartsWith("ALBUMCUSTOM"))
                {
                    string albums_lang = $"ALBUMCUSTOM_{activeOption}";
                    // Load localization custom album songs, title and another
                    if (!___m_Dictionary.ContainsKey(albums_lang))
                    {
                        ModLogger.Debug($"Load custom album tab: {albums_lang}");
                        var obj = new JObject();
                        obj.Add("name", "测试");
                        obj.Add("author", "某10");

                        var arr = new JArray();
                        arr.Add(obj);

                        ___m_Dictionary.Add(albums_lang, arr);
                    }
                    // Load songs
                    if (!___m_Dictionary.ContainsKey(name))
                    {
                        ModLogger.Debug($"Load custom songs list: {name}");
                        var obj = new JObject();
                        obj.Add("uid", "custom-0");
                        obj.Add("name", "Test");
                        obj.Add("author", "某10");
                        obj.Add("bpm", "128");
                        obj.Add("music", "iyaiya_music");
                        obj.Add("demo", "iyaiya_demo");
                        obj.Add("cover", "iyaiya_cover");
                        obj.Add("noteJson", "iyaiya_map");
                        obj.Add("scene", "scene_05");
                        obj.Add("levelDesigner", "Lyt99");
                        obj.Add("difficulty1", "1");
                        obj.Add("difficulty2", "20");
                        obj.Add("difficulty3", "0");
                        obj.Add("unlockLevel", "1");

                        var arr = new JArray();
                        arr.Add(obj);

                        ___m_Dictionary.Add(name, arr);
                    }
                    
                }
            }catch(Exception ex)
            {
                ModLogger.Debug(ex);
            }
            
        }
        public static void GetJsonPostfix(string name, bool localization, ref Dictionary<string, JArray> ___m_Dictionary,ref JArray __result)
        {
            ModLogger.Debug($"name:{name} l10n:{localization}");

            try
            {
                string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
                // Load localization album title
                if (localization && name.StartsWith("albums_"))
                {
                    string albums_lang = $"albums_{activeOption}";
                    var found = false;
                    foreach (var obj in ___m_Dictionary[albums_lang])
                    {
                        if (obj.Value<string>("title") == language[activeOption])
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // add custom title
                        ModLogger.Debug($"Add Custom l10n Title: {language[activeOption]}");
                        var @object = new JObject();
                        @object.Add("title", language[activeOption]);
                        ___m_Dictionary[albums_lang].Add(@object);
                        // return new result
                        __result = ___m_Dictionary[albums_lang];
                    }
                    return;
                }
                // Load album info
                if (!localization && name == "albums")
                {
                    var found = false;
                    foreach (var obj in ___m_Dictionary[name])
                    {
                        if (obj.Value<string>("uid") == "custom_0")
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        // add custom title
                        ModLogger.Debug($"Add Custom Info");
                        var jobj = new JObject();
                        jobj.Add("uid", "custom_0");
                        jobj.Add("title", "Custom Albums");
                        jobj.Add("prefabsName", "AlbumDiscoCustom");
                        jobj.Add("price", "¥25.00");
                        jobj.Add("jsonName", "ALBUMCUSTOM");
                        jobj.Add("needPurchase", true);
                        jobj.Add("free", false);

                        ___m_Dictionary[name].Add(jobj);
                        // return new result
                        __result = ___m_Dictionary[name];
                    }
                    return;
                }
            }
            catch(Exception ex)
            {
                ModLogger.Debug(ex);
            }
        }
        public static bool InitAlbumDatas()
        {
            try
            {
                string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
                string albums_lang = $"albums_{activeOption}";
                var configManager = Singleton<ConfigManager>.instance;
                var m_Dictionary = (Dictionary<string, JArray>)AccessTools.Field(typeof(ConfigManager), "m_Dictionary").GetValue(configManager);

                if (!m_Dictionary.ContainsKey(albums_lang))
                {
                    // Trigger the load
                    ModLogger.Debug($"Trigger the load{albums_lang}");
                    var notused = Singleton<ConfigManager>.instance["albums"];
                }
                if (!m_Dictionary.ContainsKey(albums_lang))
                {
                    // Trigger failed
                    ModLogger.Debug("Cannot Trigger load Albums title");
                    return true;
                }
                ModLogger.Debug("Trigger load Albums success");

                var found = false;
                foreach (var obj in m_Dictionary[albums_lang])
                {
                    ModLogger.Debug($"{obj}");
                    if (obj.Value<string>("title") == language[activeOption])
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    // Not Found Custom
                    ModLogger.Debug($"Add {language[activeOption]}");
                    var @object = new JObject();
                    @object.Add("title", language[activeOption]);
                    m_Dictionary[albums_lang].Add(@object);
                }

                var jobj = new JObject();
                jobj.Add("uid", "custom");
                jobj.Add("title", "Custom Albums");
                jobj.Add("prefabsName", "AlbumCustom");
                jobj.Add("price", "Free");
                jobj.Add("jsonName", "custom");
                jobj.Add("needPurchase", false);
                jobj.Add("free", true);
                m_Dictionary["albums"].Add(jobj);

            }
            catch (Exception ex)
            {
                ModLogger.Debug(ex);
            }

            return true;
        }
        public static void RebuildChildren(FancyScrollView __instance)
        {
            if (__instance.name == "FancyScrollAlbum")
            {
                string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
                
                if (GameObject.Find($"ImgAlbumCustom") == null)
                {
                    // Add new tab gameobject
                    var gameobject = GameObject.Find("ImgAlbumCollection");
                    GameObject customTab = GameObject.Instantiate(gameobject, gameobject.transform.parent);
                    customTab.name = $"ImgAlbumCustom";

                    customTab.transform.SetSiblingIndex(2);
                    // CompTrack(gameobject.transform.parent.gameObject);
                    CompTrack(customTab);
                    var a = customTab.GetComponent<VariableBehaviour>();
                    if (a != null)
                    {
                        ModLogger.Debug($"{a.result}:{a.variable.result}  {a.variable}");
                    }
                    var b = customTab.transform.Find("TxtAlbumTitle");
                    if (b != null)
                    {
                        b.GetComponent<UnityEngine.UI.Text>().text = language[activeOption];
                        var l18n = b.GetComponent<Localization>();
                        foreach (var opt in l18n.optionPairs)
                        {
                            ((TextOption)opt.option).value = language[opt.optionEntry.name];
                            ModLogger.Debug($" opt:{opt.optionEntry.name}  val:{((TextOption)opt.option).value.ToString()}");
                        }
                    }
                    ModLogger.Debug("Add custom map tab");
                }
            }
        }
        public static bool OnEnable()
        {
            try
            {
                int num = Singleton<ConfigManager>.instance["albums"].Count - 2;
                int[] freeAlbumIndexs = Singleton<WeekFreeManager>.instance.freeAlbumIndexs;
                Dictionary<int, Transform> dictionary = new Dictionary<int, Transform>();
                for (int i = 1; i < num; i++)
                {
                    Transform value = GameObject.Find(string.Format("ImgAlbum{0}", i)).transform;
                    int key = num - i + 2;
                    dictionary.Add(key, value);
                }
                dictionary = (from d in dictionary
                              orderby d.Key descending
                              select d).ToDictionary((KeyValuePair<int, Transform> d) => d.Key, (KeyValuePair<int, Transform> d) => d.Value);
                foreach (KeyValuePair<int, Transform> keyValuePair in dictionary)
                {
                    keyValuePair.Value.SetSiblingIndex(keyValuePair.Key);
                }
                if (freeAlbumIndexs != null && freeAlbumIndexs.Length > 0)
                {
                    for (int j = 0; j < freeAlbumIndexs.Length; j++)
                    {
                        int num2 = freeAlbumIndexs[j];
                        Transform transform = GameObject.Find(string.Format("ImgAlbum{0}", num2)).transform;
                        transform.SetSiblingIndex(j + 3);
                    }
                }
            }
            catch (Exception ex)
            {

                ModLogger.Debug(ex);
            }

            return false;
        }
        public static void WeekFreeControlPostfix()
        {
            int[] freeAlbumIndexs = Singleton<WeekFreeManager>.instance.freeAlbumIndexs;
            if (freeAlbumIndexs != null && freeAlbumIndexs.Length > 0)
            {
                for (int j = 0; j < freeAlbumIndexs.Length; j++)
                {
                    int num2 = freeAlbumIndexs[j];
                    Transform transform = GameObject.Find($"ImgAlbum{num2}").transform;
                    transform.SetSiblingIndex(j + 3);
                }
            }
        }
        public static void LoadFromName(string name)
        {
            //if(type != typeof(TextAsset))
            //{
            //    ModLogger.Debug($"type {type} {name}");
            //    return;
            //}
            ModLogger.Debug($"json {name}");
            //string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
            //string albums_lang = $"albums_{activeOption}";
            //if (name.StartsWith(albums_lang))
            //{
            //    ModLogger.Debug($"language {name}");
            //}else if (name.StartsWith("albums"))
            //{
            //    ModLogger.Debug($"json {name}");
            //}

        }
    }
}
