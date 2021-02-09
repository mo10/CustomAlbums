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
using Assets.Scripts.PeroTools.Nice.Datas;

namespace MuseDashCustomAlbumMod
{
    public static class CustomAlbum
    {
        private static bool isJsonLoad = false;
        private static Dictionary<string, string> language = new Dictionary<string, string>();
        public static void DoPatching()
        {
            var harmony = new Harmony("com.github.mo10.customalbum");

            var StagePreWarm = AccessTools.Method(typeof(PnlStage), "PreWarm", new Type[] { typeof(int) });
            var StagePreWarmPrefix = AccessTools.Method(typeof(CustomAlbum), "StagePreWarmPrefix");
            harmony.Patch(StagePreWarm, new HarmonyMethod(StagePreWarmPrefix));

            var StageRangeStageList = AccessTools.Method(typeof(PnlStage), "RangeStageList");
            var StageRangeStageListPostfix = AccessTools.Method(typeof(CustomAlbum), "StageRangeStageListPostfix");
            harmony.Patch(StageRangeStageList,null, new HarmonyMethod(StageRangeStageListPostfix));

            var AlbumTagName = AccessTools.Method(typeof(AlbumTagName), "GetAlbumTagLocaliztion");
            var GetAlbumTagLocaliztionPostfix = AccessTools.Method(typeof(CustomAlbum), "GetAlbumTagLocaliztionPostfix");
            harmony.Patch(AlbumTagName, null, new HarmonyMethod(GetAlbumTagLocaliztionPostfix));

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
        /// <summary>
        /// Read embedded file from this dll
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private static byte[] ReadEmbeddedFile(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            byte[] buffer;
            using (var stream = assembly.GetManifestResourceStream($"MuseDashCustomAlbumMod.{file}"))
            {
                buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }
        #region Game Stage UI
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
                // Bind DataIndex
                var newObjVarible = newObj.GetComponent<VariableBehaviour>();
                newObjVarible.result = albumFancyScrollViewContent.childCount - 1;
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
            List<string> list = new List<string>();
            List<string> nameList = new List<string>();

            list.Add("ALBUM1000");
            nameList.Add("music_package_999");

            m_AllAlbumTagData.Add(new PnlStage.albumInfo
            {
                uid = "custom",
                name = "自定义谱面",
                list = list,
                nameList = nameList,
                isWeekFree = false,
                isNew = false
            });
        }
        public static void StagePreWarmPrefix(int slice, ref List<PnlStage.albumInfo> ___m_AllAlbumTagData, ref Transform ___albumFancyScrollViewContent, ref List<GameObject> ___m_AlbumFSVCells)
        {
            if (slice != 0)
            {
                return;
            }

            AddAlbumTagCell(ref ___albumFancyScrollViewContent);
            AddAlbumTagData(ref ___m_AllAlbumTagData);

            //try
            //{
            //    var data_manager = Singleton<DataManager>.instance;
            //    data_manager.ToJson();
            //    ModLogger.Debug($"DataManager Value:{data_manager.ToJson()}");
            //    foreach (var data in data_manager.datas)
            //    {
            //        SingletonDataObject singletonDataObject = data.Value as SingletonDataObject;
            //        var json = singletonDataObject.ToJson();
            //        ModLogger.Debug($"Data Key:{data.Key} Value:{json}");
            //    }
            //}catch(Exception ex)
            //{
            //    ModLogger.Debug(ex);
            //}

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
        public static void StageRangeStageListPostfix(ref List<string> ___m_AllOtherAlbumUid, ref List<string> ___m_AllOtherAlbumName_Re, ref List<string> ___m_AllOtherAlbumUid_Re, ref List<string> ___m_AllOtherAlbumName, ref List<PnlStage.albumInfo> ___m_AllAlbumTagData)
        {
            // Rebind data
            ___m_AllOtherAlbumUid.Remove("ALBUM1000");
            ___m_AllOtherAlbumName.Remove("music_package_999");
            ___m_AllOtherAlbumUid_Re.Remove("ALBUM1000");
            ___m_AllOtherAlbumName_Re.Remove("music_package_999");
            ___m_AllAlbumTagData[7].list = ___m_AllOtherAlbumUid;
            ___m_AllAlbumTagData[7].nameList = ___m_AllOtherAlbumName;

        }
        public static void GetAlbumTagLocaliztionPostfix(string albumUid, ref string __result)
        {
            string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
            if (albumUid == "custom")
            {
                __result = language[activeOption];
            }
        }
        #endregion
        public static void GameObjectTracker(GameObject gameObject, int layer = 0)
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
                    GameObjectTracker(gameObject.transform.GetChild(i).gameObject, layer);
                }
            }
        }

        #region JSON Injection
        public static void GetJsonPrefix(string name, bool localization,ref Dictionary<string, JArray> ___m_Dictionary)
        {
            // ModLogger.Debug($"name:{name} l10n:{localization}");

            string activeOption = SingletonScriptableObject<LocalizationSettings>.instance.GetActiveOption("Language");
            try
            {
                // Load custom album
                // Inject ALBUM1000.json
                if (!localization && name == "ALBUM1000")
                {
                    // Check if already loaded
                    if (___m_Dictionary.ContainsKey("ALBUM1000"))
                    {
                        return;
                    }
                    // Load albums
                    ModLogger.Debug($"Load custom songs list: {name}");
                    var obj = new JObject();
                    obj.Add("uid", "999-0");

                    obj.Add("name", "AlbumString1");
                    obj.Add("author", "AlbumString2");

                    obj.Add("bpm", "128");
                    obj.Add("music", "iyaiya_music");
                    obj.Add("demo", "iyaiya_demo");
                    obj.Add("cover", "iyaiya_cover");
                    obj.Add("noteJson", "iyaiya_map");
                    obj.Add("scene", "scene_05");

                    // level designer of all difficulties
                    obj.Add("levelDesigner", "AlbumString3");
                    // level designer of difficulty 1 to 4 
                    obj.Add("levelDesigner1", "AlbumString4");
                    obj.Add("levelDesigner2", "AlbumString5");
                    obj.Add("levelDesigner3", "AlbumString6");
                    obj.Add("levelDesigner4", "AlbumString7");

                    obj.Add("difficulty1", "10");
                    obj.Add("difficulty2", "20");
                    obj.Add("difficulty3", "30");
                    obj.Add("difficulty4", "40");

                    obj.Add("unlockLevel", "0");

                    var album_list = new JArray();
                    album_list.Add(obj);

                    ___m_Dictionary.Add(name, album_list);
                    return;
                }
                // Load custom album localization
                // Inject ALBUM1000_<lang>.json
                if (localization && name.StartsWith("ALBUM1000_"))
                {
                    string albums_lang = $"ALBUM1000_{activeOption}";
                    // Check if already loaded
                    if (___m_Dictionary.ContainsKey(albums_lang))
                    {
                        return;
                    }
                    var album_lang = new JObject();
                    album_lang.Add("name", "测试");
                    album_lang.Add("author", "某10");

                    var album_lang_list = new JArray();
                    album_lang_list.Add(album_lang);

                    ___m_Dictionary.Add(albums_lang, album_lang_list);
                    return;
                }
            }
            catch(Exception ex)
            {
                ModLogger.Debug(ex);
            }
            
        }
        public static void GetJsonPostfix(string name, bool localization, ref Dictionary<string, JArray> ___m_Dictionary,ref JArray __result)
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
                        if (obj.Value<string>("title") == language[activeOption])
                        {
                            return;
                        }
                    }
                    // Add custom l10n title
                    ModLogger.Debug($"Add custom l10n title: {language[activeOption]}");
                    var album_lang = new JObject();
                    album_lang.Add("title", language[activeOption]);

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
                        if (obj.Value<string>("uid") == "music_package_999")
                        {
                            return;
                        }
                    }
                    // Add custom title
                    ModLogger.Debug($"Add custom album");
                    var album = new JObject();
                    album.Add("uid", "music_package_999");
                    album.Add("title", "Custom Albums");
                    album.Add("prefabsName", "AlbumDiscoNew");
                    album.Add("price", "¥25.00");
                    album.Add("jsonName", "ALBUM1000");
                    album.Add("needPurchase", true);
                    album.Add("free", false);

                    ___m_Dictionary[name].Add(album);
                    // Return new result
                    __result = ___m_Dictionary[name];
                    return;
                }
            }
            catch(Exception ex)
            {
                ModLogger.Debug(ex);
            }
        }
        #endregion




#if false


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
                    GameObjectTracker(customTab);
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
#endif
    }
}
