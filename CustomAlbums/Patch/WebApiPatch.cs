using PeroTools2.Commons;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Account;
using System.Reflection;
using UnityEngine.Networking;

namespace CustomAlbums.Patch
{
    class WebApiPatch
    {
        public static void DoPatching(Harmony harmony)
        {
            MethodInfo method;
            MethodInfo methodPrefix;
            MethodInfo methodPostfix;

            // WebUtils.SendToUrl
            method = AccessTools.Method(typeof(WebUtils), "SendToUrl", new Type[] { typeof(WebUtils.PeroWebRequest) });
            methodPrefix = AccessTools.Method(typeof(WebApiPatch), "WebUtilsSendToUrlPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
            // GameAccountSystem.SendToUrl
            method = AccessTools.Method(typeof(GameAccountSystem), "SendToUrl");
            methodPrefix = AccessTools.Method(typeof(WebApiPatch), "SendToUrlPrefix");
            harmony.Patch(method, prefix: new HarmonyMethod(methodPrefix));
        }
        /// <summary>
        /// Hook GameAccountSystem request.
        /// </summary>
        /// <returns></returns>
        public static bool SendToUrlPrefix(
            ref string url,
            ref string method,
            ref Dictionary<string, object> datas,
            ref Action<JObject> succeedCallback,
            ref Action<long, string> faillCallback,
            ref Action startCallback,
            ref Action completeCallback,
            ref Dictionary<string, string> headers
            )
        {
            var originSuccessCallback = succeedCallback;
            var originFailCallback = faillCallback;

            switch (url)
            {
                // Add custom tag.
                case "musedash/v1/music_tag":
                    succeedCallback = delegate (JObject jObject)
                    {
                        var jArray = (JArray)jObject["music_tag_list"];

                        // Replace Cute tag
                        var music_tag = jArray.Find(o => o.Value<int>("sort_key") == 8);
                        music_tag["tag_name"] = JObject.FromObject(AlbumManager.Langs);
                        music_tag["tag_picture"] = "https://mdmc.moe/cdn/melon.png";
                        music_tag["icon_name"] = "";
                        music_tag["music_list"] = JArray.FromObject(AlbumManager.GetAllUid());

                        ModLogger.Debug("Music tag injected.");
                        originSuccessCallback(jObject);
                    };
                    break;
                // Block custom play feedback.
                case "statistics/pc-play-statistics-feedback":
                    var uid = datas["music_uid"] as string;
                    if (uid.StartsWith("999"))
                    {
                        ModLogger.Debug($"Blocked play feedback upload:{(string)datas["music_uid"]}");
                        return false; // block this request
                    }
                    break;
                // Block custom album high score upload.
                case "musedash/v2/pcleaderboard/high-score":
                    var playData = PlayDataHelper.Load(datas);
#if DEBUG
                    ModLogger.Debug(playData.JsonSerialize());
#endif
                    if (playData.SelectedMusicUid.StartsWith("999"))
                    {
                        ModLogger.Debug($"Blocked high score upload:{playData.SelectedMusicUid}");
                        return false; // block this request
                    }
                    break;
                case "musedash/v2/save":
                    break;
            }

            return true;
        }
        /// <summary>
        /// Hook all request.
        /// </summary>
        /// <returns></returns>
        public static bool WebUtilsSendToUrlPrefix(WebUtils.PeroWebRequest webRequest)
        {
            ModLogger.Debug($"Incoming request:{webRequest.method} {webRequest.url}");

#if DEBUG
            var originSuccessCallback = webRequest.succeedCallback;
            var originFailCallback = webRequest.faillCallback;
            ModLogger.Debug($"Request:{webRequest.method} {webRequest.url} headers:{webRequest.headers?.JsonSerialize()} datas:{webRequest.datas?.JsonSerialize()}");
            webRequest.succeedCallback = delegate (DownloadHandler handler)
            {
                ModLogger.Debug($"Response:{webRequest.method} {webRequest.url} content:{handler.text}");
                originSuccessCallback?.Invoke(handler);
            };
            webRequest.faillCallback = delegate (long code, string error)
            {
                ModLogger.Debug($"Response failed:{webRequest.method} {webRequest.url} code:{code} error:{error}");
                originFailCallback?.Invoke(code, error);
            };
#endif
            return true;
        }
    }
}
