using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.UI.Panels;
using System;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;
using Assets.Scripts.PeroTools.Managers;

namespace CustomAlbums
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
            // ConfigManager.GetJson
            var getJson = typeof(ConfigManager).GetMethods().Where(x => (x.Name == "GetJson" && !x.IsGenericMethod)).First();
            var getJsonPostfix = AccessTools.Method(typeof(StageUIPatch), "GetJsonPostfix");
            harmony.Patch(getJson, null, new HarmonyMethod(getJsonPostfix));
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
                        CustomAlbum.Albums.Count.ToString()
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
        
        public static void GetJsonPostfix(string name, ref JArray __result)
        {
            if (name == "defaultTag")
            {
                JArray tags = JArray.Parse("[\n    {\n      \"object_id\": \"612762dbef104a23208bf231\",\n      \"created_at\": \"2021-08-26T09:46:03.514Z\",\n      \"updated_at\": \"2021-09-06T11:33:45.41Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"热门\",\n        \"ChineseT\": \"熱門\",\n        \"English\": \"Hottest\",\n        \"Japanese\": \"ホット\",\n        \"Korean\": \"인기 곡\"\n      },\n      \"tag_picture\": \"https://the-surrounding.oss-cn-shenzhen.aliyuncs.com/1629970433IconHot.png\",\n      \"music_list\": [\n        \"0-8\",\n        \"0-9\",\n        \"0-16\",\n        \"0-18\",\n        \"0-26\",\n        \"0-28\",\n        \"0-29\",\n        \"0-45\",\n        \"0-49\",\n        \"2-0\",\n        \"2-1\",\n        \"4-1\",\n        \"7-1\",\n        \"8-1\",\n        \"17-1\",\n        \"17-4\",\n        \"22-1\",\n        \"19-5\",\n        \"25-0\",\n        \"25-2\",\n        \"27-0\",\n        \"27-3\",\n        \"29-3\"\n      ],\n      \"anchor_pattern\": false,\n      \"sort_key\": 1,\n      \"icon_name\": \"IconHot\"\n    },\n    {\n      \"object_id\": \"61275fcf3344beea9c23cf0a\",\n      \"created_at\": \"2021-08-26T09:33:03.767Z\",\n      \"updated_at\": \"2021-08-26T09:57:22.709Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"萌新\",\n        \"ChineseT\": \"萌新\",\n        \"English\": \"Easy\",\n        \"Japanese\": \"初心者\",\n        \"Korean\": \"초보\"\n      },\n      \"tag_picture\": \"https://the-surrounding.oss-cn-shenzhen.aliyuncs.com/1629969527IconFreshMan.png\",\n      \"music_list\": [\n        \"0-48\",\n        \"0-8\",\n        \"0-29\",\n        \"0-30\",\n        \"0-31\",\n        \"0-46\",\n        \"0-49\",\n        \"3-0\",\n        \"25-0\",\n        \"13-3\",\n        \"17-0\",\n        \"23-2\",\n        \"25-1\",\n        \"14-3\",\n        \"21-0\",\n        \"23-0\",\n        \"30-1\",\n        \"33-0\",\n        \"33-9\",\n        \"37-0\"\n      ],\n      \"anchor_pattern\": false,\n      \"sort_key\": 2,\n      \"icon_name\": \"IconFreshMan\"\n    },\n    {\n      \"object_id\": \"611df89eb016f3293158d1f4\",\n      \"created_at\": \"2021-08-19T06:22:22.205Z\",\n      \"updated_at\": \"2021-09-11T08:40:34.068Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"流行\",\n        \"ChineseT\": \"流行\",\n        \"English\": \"Pop\",\n        \"Japanese\": \"ポップ\",\n        \"Korean\": \"팝송\"\n      },\n      \"tag_picture\": \"https://the-surrounding.oss-cn-shenzhen.aliyuncs.com/1629369037歌曲分类_流行.png\",\n      \"music_list\": [\n        \"0-2\",\n        \"0-3\",\n        \"0-4\",\n        \"0-6\",\n        \"0-7\",\n        \"0-8\",\n        \"0-10\",\n        \"0-24\",\n        \"0-30\",\n        \"0-31\",\n        \"0-34\",\n        \"0-35\",\n        \"0-37\",\n        \"0-40\",\n        \"0-41\",\n        \"0-43\",\n        \"0-44\",\n        \"0-46\",\n        \"0-47\",\n        \"0-49\",\n        \"1-0\",\n        \"1-1\",\n        \"1-2\",\n        \"1-4\",\n        \"1-5\",\n        \"0-38\",\n        \"3-0\",\n        \"5-0\",\n        \"5-2\",\n        \"5-4\",\n        \"6-1\",\n        \"7-0\",\n        \"8-2\",\n        \"9-0\",\n        \"9-1\",\n        \"9-4\",\n        \"10-3\",\n        \"11-0\",\n        \"11-3\",\n        \"13-0\",\n        \"13-1\",\n        \"13-3\",\n        \"13-4\",\n        \"13-5\",\n        \"14-0\",\n        \"15-0\",\n        \"15-1\",\n        \"15-2\",\n        \"15-3\",\n        \"15-4\",\n        \"15-5\",\n        \"17-0\",\n        \"17-2\",\n        \"18-0\",\n        \"18-1\",\n        \"18-2\",\n        \"18-3\",\n        \"18-4\",\n        \"18-5\",\n        \"20-1\",\n        \"20-3\",\n        \"21-0\",\n        \"21-1\",\n        \"21-2\",\n        \"23-0\",\n        \"23-1\",\n        \"23-2\",\n        \"23-3\",\n        \"23-4\",\n        \"23-5\",\n        \"24-1\",\n        \"25-0\",\n        \"25-1\",\n        \"25-2\",\n        \"25-3\",\n        \"25-4\",\n        \"25-5\",\n        \"27-0\",\n        \"27-1\",\n        \"27-2\",\n        \"27-4\",\n        \"27-5\",\n        \"29-0\",\n        \"30-0\",\n        \"30-1\",\n        \"30-2\",\n        \"30-3\",\n        \"30-4\",\n        \"30-5\",\n        \"31-0\",\n        \"31-3\",\n        \"31-4\",\n        \"33-0\",\n        \"33-1\",\n        \"33-4\",\n        \"33-9\",\n        \"33-10\",\n        \"33-12\",\n        \"35-0\",\n        \"35-5\",\n        \"36-1\",\n        \"36-2\",\n        \"36-3\",\n        \"37-0\",\n        \"37-2\",\n        \"37-3\",\n        \"37-4\",\n        \"37-5\",\n        \"38-1\",\n        \"39-0\",\n        \"39-1\",\n        \"39-2\",\n        \"39-3\",\n        \"39-4\",\n        \"39-5\",\n        \"39-6\",\n        \"39-7\",\n        \"40-0\",\n        \"40-1\",\n        \"40-2\"\n      ],\n      \"anchor_pattern\": false,\n      \"sort_key\": 3,\n      \"icon_name\": \"IconPopularity\"\n    },\n    {\n      \"object_id\": \"611dfe7fadb84ec383f38d0a\",\n      \"created_at\": \"2021-08-19T06:47:27.729Z\",\n      \"updated_at\": \"2021-09-11T08:39:41.641Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"动感\",\n        \"ChineseT\": \"動感\",\n        \"English\": \"Dynamic\",\n        \"Japanese\": \"ダイナミック\",\n        \"Korean\": \"다이나믹\"\n      },\n      \"tag_picture\": \"https://the-surrounding.oss-cn-shenzhen.aliyuncs.com/1629369051歌曲分类_动感.png\",\n      \"music_list\": [\n        \"0-9\",\n        \"0-11\",\n        \"0-13\",\n        \"0-18\",\n        \"0-19\",\n        \"0-20\",\n        \"0-21\",\n        \"0-24\",\n        \"0-26\",\n        \"0-28\",\n        \"0-29\",\n        \"0-30\",\n        \"0-32\",\n        \"0-34\",\n        \"0-35\",\n        \"0-46\",\n        \"1-0\",\n        \"1-1\",\n        \"1-3\",\n        \"2-1\",\n        \"2-2\",\n        \"2-3\",\n        \"2-4\",\n        \"2-5\",\n        \"3-1\",\n        \"5-1\",\n        \"5-4\",\n        \"6-0\",\n        \"7-0\",\n        \"7-5\",\n        \"8-1\",\n        \"8-4\",\n        \"8-5\",\n        \"9-5\",\n        \"10-2\",\n        \"10-5\",\n        \"11-0\",\n        \"11-2\",\n        \"11-5\",\n        \"12-4\",\n        \"14-3\",\n        \"14-4\",\n        \"14-5\",\n        \"16-1\",\n        \"16-4\",\n        \"17-4\",\n        \"17-5\",\n        \"19-0\",\n        \"19-2\",\n        \"19-3\",\n        \"19-5\",\n        \"20-1\",\n        \"20-2\",\n        \"20-4\",\n        \"22-0\",\n        \"22-1\",\n        \"22-2\",\n        \"24-5\",\n        \"26-1\",\n        \"26-3\",\n        \"26-4\",\n        \"28-1\",\n        \"29-2\",\n        \"29-5\",\n        \"31-0\",\n        \"31-1\",\n        \"32-4\",\n        \"32-5\",\n        \"33-0\",\n        \"33-3\",\n        \"33-6\",\n        \"33-11\",\n        \"34-1\",\n        \"34-2\",\n        \"34-5\",\n        \"35-1\",\n        \"35-4\",\n        \"35-5\",\n        \"36-4\",\n        \"38-0\",\n        \"38-1\",\n        \"38-2\",\n        \"40-0\",\n        \"40-1\",\n        \"40-5\",\n        \"41-0\",\n        \"41-1\",\n        \"41-2\",\n        \"41-4\",\n        \"41-5\"\n      ],\n      \"anchor_pattern\": false,\n      \"sort_key\": 4,\n      \"icon_name\": \"IconDynamic\"\n    },\n    {\n      \"object_id\": \"611e02032f74812fa7f5f61c\",\n      \"created_at\": \"2021-08-19T07:02:27.689Z\",\n      \"updated_at\": \"2021-08-26T09:59:09.815Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"欢快\",\n        \"ChineseT\": \"歡快\",\n        \"English\": \"Cheerful\",\n        \"Japanese\": \"陽気\",\n        \"Korean\": \"경쾌\"\n      },\n      \"tag_picture\": \"https://the-surrounding.oss-cn-shenzhen.aliyuncs.com/1629369070歌曲分类_欢快.png\",\n      \"music_list\": [\n        \"0-5\",\n        \"0-9\",\n        \"0-16\",\n        \"0-23\",\n        \"0-29\",\n        \"0-31\",\n        \"0-36\",\n        \"0-40\",\n        \"0-42\",\n        \"0-44\",\n        \"0-45\",\n        \"0-48\",\n        \"1-2\",\n        \"1-4\",\n        \"2-3\",\n        \"4-0\",\n        \"4-1\",\n        \"4-2\",\n        \"4-4\",\n        \"4-5\",\n        \"6-1\",\n        \"6-3\",\n        \"6-4\",\n        \"7-0\",\n        \"7-4\",\n        \"8-2\",\n        \"8-3\",\n        \"8-4\",\n        \"9-5\",\n        \"10-2\",\n        \"10-3\",\n        \"11-4\",\n        \"12-4\",\n        \"12-5\",\n        \"13-2\",\n        \"14-3\",\n        \"16-0\",\n        \"17-0\",\n        \"18-4\",\n        \"19-1\",\n        \"19-4\",\n        \"20-4\",\n        \"20-5\",\n        \"22-4\",\n        \"23-5\",\n        \"24-3\",\n        \"24-4\",\n        \"26-2\",\n        \"26-5\",\n        \"27-0\",\n        \"28-0\",\n        \"29-3\",\n        \"31-0\",\n        \"31-3\",\n        \"31-5\",\n        \"32-1\",\n        \"32-3\",\n        \"33-1\",\n        \"33-2\",\n        \"34-3\",\n        \"35-3\",\n        \"36-0\",\n        \"36-1\",\n        \"36-2\",\n        \"37-1\",\n        \"37-5\",\n        \"38-0\",\n        \"40-3\",\n        \"41-1\",\n        \"41-4\"\n      ],\n      \"anchor_pattern\": false,\n      \"sort_key\": 5,\n      \"icon_name\": \"IconCheerful\"\n    },\n    {\n      \"object_id\": \"611dfc5c2f74812fa7f5cd97\",\n      \"created_at\": \"2021-08-19T06:38:20.184Z\",\n      \"updated_at\": \"2021-08-26T09:59:13.692Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"复古\",\n        \"ChineseT\": \"復古\",\n        \"English\": \"Retro\",\n        \"Japanese\": \"レトロ\",\n        \"Korean\": \"레트로\"\n      },\n      \"tag_picture\": \"https://the-surrounding.oss-cn-shenzhen.aliyuncs.com/1629369045歌曲分类_复古.png\",\n      \"music_list\": [\n        \"0-43\",\n        \"0-50\",\n        \"0-51\",\n        \"13-0\",\n        \"15-0\",\n        \"18-0\",\n        \"18-1\",\n        \"20-4\",\n        \"25-0\",\n        \"25-1\",\n        \"25-2\",\n        \"25-3\",\n        \"25-4\",\n        \"25-5\",\n        \"30-5\",\n        \"39-0\",\n        \"39-1\",\n        \"39-2\",\n        \"39-3\",\n        \"39-4\",\n        \"39-5\",\n        \"39-6\",\n        \"39-7\"\n      ],\n      \"anchor_pattern\": false,\n      \"sort_key\": 6,\n      \"icon_name\": \"IconRetro\"\n    },\n    {\n      \"object_id\": \"611e012f2f74812fa7f5f069\",\n      \"created_at\": \"2021-08-19T06:58:55.785Z\",\n      \"updated_at\": \"2021-09-11T08:38:42.747Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"华丽\",\n        \"ChineseT\": \"華麗\",\n        \"English\": \"Glam\",\n        \"Japanese\": \"華麗\",\n        \"Korean\": \"화려\"\n      },\n      \"tag_picture\": \"https://the-surrounding.oss-cn-shenzhen.aliyuncs.com/1629369064歌曲分类_古典.png\",\n      \"music_list\": [\n        \"0-12\",\n        \"0-13\",\n        \"0-14\",\n        \"0-16\",\n        \"0-19\",\n        \"0-22\",\n        \"0-27\",\n        \"0-33\",\n        \"0-39\",\n        \"0-50\",\n        \"0-51\",\n        \"0-20\",\n        \"0-21\",\n        \"2-0\",\n        \"2-2\",\n        \"2-3\",\n        \"2-4\",\n        \"3-0\",\n        \"3-1\",\n        \"3-2\",\n        \"3-3\",\n        \"3-4\",\n        \"4-0\",\n        \"4-1\",\n        \"4-3\",\n        \"5-2\",\n        \"5-3\",\n        \"6-0\",\n        \"6-2\",\n        \"6-5\",\n        \"7-3\",\n        \"7-4\",\n        \"8-0\",\n        \"9-3\",\n        \"10-0\",\n        \"10-1\",\n        \"11-1\",\n        \"12-0\",\n        \"12-1\",\n        \"12-2\",\n        \"12-3\",\n        \"12-5\",\n        \"14-0\",\n        \"14-1\",\n        \"14-2\",\n        \"16-2\",\n        \"16-3\",\n        \"16-5\",\n        \"17-3\",\n        \"17-4\",\n        \"20-0\",\n        \"22-3\",\n        \"22-5\",\n        \"24-0\",\n        \"24-2\",\n        \"26-0\",\n        \"28-3\",\n        \"28-5\",\n        \"29-1\",\n        \"31-0\",\n        \"31-4\",\n        \"33-5\",\n        \"33-7\",\n        \"33-8\",\n        \"33-9\",\n        \"33-10\",\n        \"35-1\",\n        \"35-2\",\n        \"36-0\",\n        \"38-0\",\n        \"40-2\",\n        \"40-4\",\n        \"41-0\",\n        \"41-3\"\n      ],\n      \"anchor_pattern\": false,\n      \"sort_key\": 7,\n      \"icon_name\": \"IconClassical\"\n    },\n    {\n      \"object_id\": \"611dff44b016f32931590108\",\n      \"created_at\": \"2021-08-19T06:50:44.014Z\",\n      \"updated_at\": \"2021-08-26T09:59:23.072Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"自定义\",\n        \"ChineseT\": \"自定義\",\n        \"English\": \"Custom Albums\",\n        \"Japanese\": \"Custom Albums\",\n        \"Korean\": \"Custom Albums\"\n      },\n      \"tag_picture\": \"https://mdmc.moe/cdn/melon.png\",\n      \"music_list\": [\n        \n      ],\n      \"anchor_pattern\": false,\n      \"sort_key\": 8,\n      \"icon_name\": \"\"\n    },\n    {\n      \"object_id\": \"61557604a75ed5015c2e439a\",\n      \"created_at\": \"2021-09-30T08:32:04.247Z\",\n      \"updated_at\": \"2021-09-30T08:43:41.177Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"联动\",\n        \"ChineseT\": \"聯動\",\n        \"English\": \"Collab\",\n        \"Japanese\": \"コラボ\",\n        \"Korean\": \"콜라보\"\n      },\n      \"tag_picture\": \"https://the-surrounding.oss-cn-shenzhen.aliyuncs.com/1632990763标签.png\",\n      \"music_list\": [\n        \"42-0\",\n        \"42-1\",\n        \"42-2\",\n        \"42-3\",\n        \"42-4\",\n        \"42-5\",\n        \"43-3\",\n        \"41-0\",\n        \"41-1\",\n        \"41-2\",\n        \"41-3\",\n        \"41-4\",\n        \"41-5\",\n        \"38-0\",\n        \"38-1\",\n        \"38-2\",\n        \"34-0\",\n        \"34-1\",\n        \"34-2\",\n        \"34-3\",\n        \"34-4\",\n        \"34-5\",\n        \"33-0\",\n        \"33-1\",\n        \"33-2\",\n        \"33-3\",\n        \"33-4\",\n        \"33-5\",\n        \"33-6\",\n        \"33-7\",\n        \"33-8\",\n        \"33-9\",\n        \"33-10\",\n        \"33-11\",\n        \"32-0\",\n        \"32-1\",\n        \"32-2\",\n        \"32-3\",\n        \"32-4\",\n        \"32-5\",\n        \"30-0\",\n        \"30-1\",\n        \"30-2\",\n        \"29-0\",\n        \"29-1\",\n        \"29-2\",\n        \"29-3\",\n        \"29-4\",\n        \"29-5\",\n        \"27-0\",\n        \"27-1\",\n        \"27-2\",\n        \"27-3\",\n        \"27-4\",\n        \"27-5\",\n        \"21-0\",\n        \"21-1\",\n        \"21-2\",\n        \"0-32\",\n        \"0-42\",\n        \"0-44\",\n        \"0-46\",\n        \"0-49\",\n        \"0-50\",\n        \"0-51\"\n      ],\n      \"anchor_pattern\": false,\n      \"sort_key\": 9,\n      \"icon_name\": \"\"\n    },\n    {\n      \"object_id\": \"613600ff77afba48c6a4ffef\",\n      \"created_at\": \"2021-09-06T11:52:31.685Z\",\n      \"updated_at\": \"2021-09-30T07:40:33.996Z\",\n      \"tag_name\": {\n        \"ChineseS\": \"有里谱\",\n        \"ChineseT\": \"有里譜\",\n        \"English\": \"With Hidden Sheet\",\n        \"Japanese\": \"裏譜面あり\",\n        \"Korean\": \"히든 채보\"\n      },\n      \"tag_picture\": \"https://the-surrounding.oss-cn-shenzhen.aliyuncs.com/1631187159IconHideMap.png\",\n      \"music_list\": [\n        \"43-3\",\n        \"43-1\",\n        \"42-0\",\n        \"42-1\",\n        \"42-2\",\n        \"42-3\",\n        \"42-4\",\n        \"42-5\",\n        \"41-0\",\n        \"41-5\",\n        \"0-11\",\n        \"0-45\",\n        \"39-0\",\n        \"38-2\",\n        \"35-4\",\n        \"34-1\",\n        \"34-2\",\n        \"34-3\",\n        \"34-4\",\n        \"34-5\",\n        \"33-2\",\n        \"33-3\",\n        \"31-1\",\n        \"31-5\",\n        \"29-1\",\n        \"29-3\",\n        \"29-5\",\n        \"28-1\",\n        \"22-1\",\n        \"20-2\",\n        \"8-3\",\n        \"8-4\",\n        \"6-4\",\n        \"5-1\",\n        \"5-3\",\n        \"4-5\"\n      ],\n      \"anchor_pattern\": true,\n      \"sort_key\": 10,\n      \"icon_name\": \"IconHideMap\"\n    }\n  ]");
                string[] uids = new string[CustomAlbum.Albums.Values.Count];
                for (int i = 0; i < CustomAlbum.Albums.Values.Count; i++)
                {
                    uids[i] = "999-" + i;
                }
                tags[7]["music_list"] = JArray.FromObject(uids);
                __result = tags;
            }
        }
    }
}
