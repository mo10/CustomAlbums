using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.UI.Panels;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System.IO;
using UnityEngine.Networking;
using Assets.Scripts.PeroTools.Managers;
using ModHelper;
using Assets.Scripts.PeroTools.Nice.Datas;
using static Assets.Scripts.PeroTools.Managers.AssetBundleConfigManager;
using Assets.Scripts.PeroTools.AssetBundles;

namespace CustomAlbums
{
    public static class StageUIPatch
    {
        //    public static readonly string AlbumTagUid = "custom";
        //    public static readonly string AlbumTagGameObjectName = "AlbumTagCell_Custom";
        private static bool abInjected = false;
        public static void DoPatching(Harmony harmony)
        {
            // PnlStage.PreWarm
            var preWarm = AccessTools.Method(typeof(PnlStage), "PreWarm", new Type[] { typeof(int) });
            var preWarmPrefix = AccessTools.Method(typeof(StageUIPatch), "PreWarmPrefix");
            harmony.Patch(preWarm, new HarmonyMethod(preWarmPrefix));
            // PnlStage.PreWarm
            //var assetBundleConfigManager = AccessTools.Method(typeof(AssetBundleConfigManager), "Get", new Type[] { typeof(string), typeof(Type) });
            //var assetBundleConfigManagerPrefix = AccessTools.Method(typeof(StageUIPatch), "assetBundleConfigManagerPrefix");
            //harmony.Patch(assetBundleConfigManager, new HarmonyMethod(assetBundleConfigManagerPrefix));
            // AssetBundle.LoadFromFile
            var loadFromFile = AccessTools.Method(typeof(AssetBundle), "LoadFromFile", new Type[] { typeof(string), typeof(uint) });
            var loadFromFilePrefix = AccessTools.Method(typeof(StageUIPatch), "loadFromFilePrefix");
            harmony.Patch(loadFromFile, new HarmonyMethod(loadFromFilePrefix));
            // ServerManager.SendToUrl
            var sendToUrl = AccessTools.Method(typeof(ServerManager), "SendToUrl");
            var sendToUrlPrefix = AccessTools.Method(typeof(StageUIPatch), "SendToUrlPrefix");
            harmony.Patch(sendToUrl, new HarmonyMethod(sendToUrlPrefix));
            //// MusicTagManager.InitNewAlbum
            //var initNewAlbum = AccessTools.Method(typeof(MusicTagManager), "InitNewAlbum");
            //var initNewAlbumPrefix = AccessTools.Method(typeof(StageUIPatch), "InitNewAlbumPrefix");
            //harmony.Patch(initNewAlbum, new HarmonyMethod(initNewAlbumPrefix));
        }
        // PnlStage.PreWarm
        public static bool loadFromFilePrefix(string path, uint crc,ref AssetBundle __result)
        {
            var abName = path.Substring(Settings.currentSetting.firstLoadAssetPath.Length+1);
            ModLogger.Debug($"abName: {abName}");

            if(!abInjected && CustomAlbum.abCache.ContainsKey(abName))
            {
                abInjected = true;
                foreach(var name in CustomAlbum.abCache)
                {
                    ABConfig config = new ABConfig();
                    config.extension = ".json";
                    config.fileName = null;
                    config.type = typeof(TextAsset);
                    config.tag = Tag.JsonConfig;

                    config.abName = name.Key;
                    config.directory = CustomAlbum.abDirectory[name.Key];

                    var assetName = CustomAlbum.abName[name.Key];
                    SingletonScriptableObject<AssetBundleConfigManager>.instance.dict.Add(assetName, new List<ABConfig>() { config });
                }
            }
            byte[] abBuffer;
            if (CustomAlbum.abCache.TryGetValue(abName, out abBuffer))
            {
                ModLogger.Debug($"!!!! Inject {abName} !!!!");
                __result = AssetBundle.LoadFromMemory(abBuffer);
                return false;
            }
            return true;

            //if (path == Path.Combine(Settings.currentSetting.firstLoadAssetPath, "datas/configs/others"))
            //{
            //    foreach(var item in CustomAlbum.abCache)
            //    {
            //        ModLogger.Debug($"!!!!!!!!!!!!!!!Inject {item.Key}!!!!!!!!!!!!!!!");
            //        ABConfig config = new ABConfig();
            //        config.extension = ".json";
            //        config.fileName = null;
            //        config.type = typeof(TextAsset);
            //        config.tag = Tag.JsonConfig;

            //        config.abName = item.Key;
            //        config.directory = CustomAlbum.abDirectory[item.Key];

            //        SingletonScriptableObject<AssetBundleConfigManager>.instance.dict.Add(AlbumManager.JsonName, new List<ABConfig>() { config });
            //    }


            //    foreach (var a in SingletonScriptableObject<AssetBundleConfigManager>.instance.dict)
            //    {
            //        ModLogger.Debug(a.Key);
            //    }

            //    __result = AssetBundle.LoadFromMemory(CustomAlbum.newAssetBundle);
            //    return false;
            //}
            //return true;
        }
        public static void PreWarmPrefix(int slice, ref List<PnlStage.albumInfo> ___m_AllAlbumTagData, ref Transform ___albumFancyScrollViewContent, ref List<GameObject> ___m_AlbumFSVCells)
        {
            
            if (slice == 0)
            {
                //AddAlbumTagCell(ref ___albumFancyScrollViewContent);
                //AddAlbumTagData(ref ___m_AllAlbumTagData);
                ModLogger.Debug(Singleton<DataManager>.instance["Account"].ToJson());

            }
        }
        public static void assetBundleConfigManagerPrefix(string assetPath, Type type,ref Dictionary<string, List<AssetBundleConfigManager.ABConfig>> ___dict)
        {
            ModLogger.Debug($"assetPath:{assetPath} type:{typeof(Type)}");

        }
        public static bool SendToUrlPrefix(
            string url,
            string method,
            Dictionary<string, object> datas,
            Action<JObject> callback,
            Action<string> faillCallback,
            ref Dictionary<string, string> headers,
            int failTime,
            bool isAutoSend,
            string appkey)
        {
            if (url == "/musedash/v1/music_tag")
            {
                headers = new Dictionary<string, string>()
                    {
                        {
                            "count",
                            (new DirectoryInfo("Custom_Albums").GetFiles().Length + new DirectoryInfo("Custom_Albums").GetDirectories().Length).ToString()
                        }
                    };

                UnityWebRequest url1 = WebUtils.SendToUrl("https://mdmc.moe/api/tags", method, datas, (handler =>
                {
                    JObject jobject = JsonUtils.Deserialize<JObject>(handler.text);
                    JToken token = jobject["code"];
                    if (callback == null)
                        return;
                    callback(jobject);
                }), faillCallback, headers, failTime, true);
                if (isAutoSend)
                    return false;
                string str1 = string.Format("{0}&appkey={1}", url1.url, appkey);
                string str2 = ServerManager.MD5Compute(str1.Remove(0, str1.LastIndexOf("?") + 1));
                url1.url += string.Format("&signature={0}", str2);
                url1.SendWebRequest();
                return false;
            }
            else
            {
                return true;
            }
        }

        //    private static void AddAlbumTagData(ref List<PnlStage.albumInfo> m_AllAlbumTagData)
        //    {
        //        m_AllAlbumTagData.Add(new PnlStage.albumInfo
        //        {
        //            uid = AlbumTagUid,
        //            name = CustomAlbum.Languages["ChineseS"],
        //            list = new List<string>() { CustomAlbum.JsonName },
        //            nameList = new List<string>() { CustomAlbum.MusicPackge },
        //            isWeekFree = false,
        //            isNew = false
        //        });
        //    }

        //    public static void InitNewAlbumPrefix(MusicTagManager __instance)
        //    {
        //        Dictionary<uint, MusicTagManager.AlbumInfo> m_AllAlbumTagData = Traverse.Create(__instance).Field("m_AllAlbumTagData").GetValue() as Dictionary<uint, MusicTagManager.AlbumInfo>;

        //        MusicTagManager.AlbumInfo albumInfo = new MusicTagManager.AlbumInfo()
        //        {
        //            uid = AlbumTagUid,
        //            name = CustomAlbum.Languages["ChineseS"],
        //            list = new List<string>() { CustomAlbum.JsonName },
        //            nameList = new List<string>() { CustomAlbum.MusicPackge },
        //            isWeekFree = false,
        //            isNew = false
        //        };
        //        if (m_AllAlbumTagData.ContainsKey(999U))
        //            return;
        //        m_AllAlbumTagData.Add(999U, albumInfo);
        //        Traverse.Create(__instance).Field("m_AllAlbumTagData").SetValue(m_AllAlbumTagData);
        //    }
    }
}
