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

namespace CustomAlbum
{
    public static class StageUIPatch
    {
        public static readonly string AlbumTagUid = "custom";
        public static readonly string AlbumTagGameObjectName = "AlbumTagCell_Custom";

        public static void DoPatching(Harmony harmony)
        {
            // PnlStage.PreWarm
            var preWarm = AccessTools.Method(typeof(PnlStage), "PreWarm", new Type[] { typeof(int) });
            var preWarmPrefix = AccessTools.Method(typeof(StageUIPatch), "PreWarmPrefix");
            harmony.Patch(preWarm, new HarmonyMethod(preWarmPrefix));
            // ServerManager.SendToUrl
            var sendToUrl = AccessTools.Method(typeof(ServerManager), "SendToUrl");
            var sendToUrlPrefix = AccessTools.Method(typeof(StageUIPatch), "SendToUrlPrefix");
            harmony.Patch(sendToUrl, new HarmonyMethod(sendToUrlPrefix));
            // MusicTagManager.InitNewAlbum
            var initNewAlbum = AccessTools.Method(typeof(MusicTagManager), "InitNewAlbum");
            var initNewAlbumPrefix = AccessTools.Method(typeof(StageUIPatch), "InitNewAlbumPrefix");
            harmony.Patch(initNewAlbum, new HarmonyMethod(initNewAlbumPrefix));
        }
        // PnlStage.PreWarm
        public static void PreWarmPrefix(int slice, ref List<PnlStage.albumInfo> ___m_AllAlbumTagData, ref Transform ___albumFancyScrollViewContent, ref List<GameObject> ___m_AlbumFSVCells)
        {
            if (slice == 0)
            {
                //AddAlbumTagCell(ref ___albumFancyScrollViewContent);
                AddAlbumTagData(ref ___m_AllAlbumTagData);
            }
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

        private static void AddAlbumTagData(ref List<PnlStage.albumInfo> m_AllAlbumTagData)
        {
            m_AllAlbumTagData.Add(new PnlStage.albumInfo
            {
                uid = AlbumTagUid,
                name = CustomAlbum.Languages["ChineseS"],
                list = new List<string>() { CustomAlbum.JsonName },
                nameList = new List<string>() { CustomAlbum.MusicPackge },
                isWeekFree = false,
                isNew = false
            });
        }

        public static void InitNewAlbumPrefix(MusicTagManager __instance)
        {
            Dictionary<uint, MusicTagManager.AlbumInfo> m_AllAlbumTagData = Traverse.Create(__instance).Field("m_AllAlbumTagData").GetValue() as Dictionary<uint, MusicTagManager.AlbumInfo>;

            MusicTagManager.AlbumInfo albumInfo = new MusicTagManager.AlbumInfo()
            {
                uid = AlbumTagUid,
                name = CustomAlbum.Languages["ChineseS"],
                list = new List<string>() { CustomAlbum.JsonName },
                nameList = new List<string>() { CustomAlbum.MusicPackge },
                isWeekFree = false,
                isNew = false
            };
            if (m_AllAlbumTagData.ContainsKey(999U))
                return;
            m_AllAlbumTagData.Add(999U, albumInfo);
            Traverse.Create(__instance).Field("m_AllAlbumTagData").SetValue(m_AllAlbumTagData);
        }
    }
}
