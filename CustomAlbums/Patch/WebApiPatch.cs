using Assets.Scripts.PeroTools.Commons;
using Assets.Scripts.PeroTools.Managers;
using Assets.Scripts.PeroTools.Nice.Datas;
using Assets.Scripts.PeroTools.Nice.Interface;
using HarmonyLib;
using ModHelper;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomAlbums.Patch
{
    class WebApiPatch
    {
        public static void DoPatching(Harmony harmony)
        {
            // ServerManager.SendToUrl
            var sendToUrl = AccessTools.Method(typeof(ServerManager), "SendToUrl");
            var sendToUrlPrefix = AccessTools.Method(typeof(WebApiPatch), "SendToUrlPrefix");
            harmony.Patch(sendToUrl, prefix: new HarmonyMethod(sendToUrlPrefix));
        }
        /// <summary>
        /// Hook any web request.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <param name="datas"></param>
        /// <param name="callback"></param>
        /// <param name="faillCallback">Typo</param>
        /// <param name="headers"></param>
        /// <param name="failTime"></param>
        /// <param name="isAutoSend"></param>
        /// <param name="appkey"></param>
        /// <returns></returns>
        public static bool SendToUrlPrefix(ref string url, ref string method, ref Dictionary<string, object> datas,
            ref Action<JObject> callback,
            ref Action<string> faillCallback,
            ref Dictionary<string, string> headers,
            ref int failTime,
            ref bool isAutoSend,
            ref string appkey)
        {
            var originSuccessCallback = callback;
            var originFailCallback = faillCallback;

#if DEBUG
            ModLogger.Debug($"Incoming request:{method} {url}");
#endif
            switch (url)
            {
                // Add custom tag.
                case "/musedash/v1/music_tag":
                    callback = delegate (JObject jObject)
                    {
                        var jArray = (JArray)jObject["music_tag_list"];

                        // Add new music tag
                        var music_tag = jArray.Find(o => o.Value<int>("sort_key") == 8);
                        music_tag["tag_name"] = JObject.FromObject(AlbumManager.Langs);
                        music_tag["tag_picture"] = "https://mdmc.moe/cdn/melon.png";
                        music_tag["pic_name"] = "";
                        music_tag["music_list"] = JArray.FromObject(AlbumManager.GetAllUid());

                        //jArray.Add(JObject.FromObject(new
                        //{
                        //    object_id = "3d2be24f837b2ec1e5e119bb",
                        //    created_at = "2021-10-24T00:00:00.000Z",
                        //    updated_at = "2021-10-24T00:00:00.000Z",
                        //    tag_name = JObject.FromObject(AlbumManager.Langs),
                        //    tag_picture = "https://mdmc.moe/cdn/melon.png",
                        //    pic_name = "",
                        //    music_list = AlbumManager.GetAllUid(),
                        //    anchor_pattern = false,
                        //    sort_key = jArray.Count + 1,
                        //}));
                        ModLogger.Debug("Music tag injected.");
                        originSuccessCallback(jObject);
                    };
                    break;
                // Block custom play feedback.
                case "statistics/pc-play-statistics-feedback":
                    if (((string)datas["music_uid"]).StartsWith("999"))
                    {
                        ModLogger.Debug($"Blocked play feedback upload:{(string)datas["music_uid"]}");
                        return false; // block this request
                    }
                    break;
                // Block custom album high score upload.
                case "musedash/v2/pcleaderboard/high-score":
                case "musedash/v2/exhileaderboard/high-score":
                    var selectedUid = Singleton<DataManager>.instance["Account"]["SelectedMusicUid"].GetResult<string>();
                    if (selectedUid.StartsWith("999-"))
                    {
                        ModLogger.Debug($"Blocked high score upload:{selectedUid}");
                        return false; // block this request
                    }
                    break;
                // Clean the cloud saves
                case "musedash/v2/save":
                    goto default;
                    if (method != "PUT")
                        goto default;
                    var save = datas["save"] as Dictionary<string, string>;
                    var account = save["Account"].JsonDeserialize<JObject>();
                    var achievement = save["Achievement"].JsonDeserialize<JObject>();

                    save["Account"] = SavesPatch.Clean(account).JsonSerialize();
                    save["Achievement"] = SavesPatch.Clean(achievement).JsonSerialize();
#if DEBUG
                    ModLogger.Debug(save.JsonSerialize());
#endif
                    break;
                default:
                    var innerMethod = method;
                    var innerUrl = url;
                    var innerHeaders = headers;
                    var innerDatas = datas;

#if DEBUG
                    ModLogger.Debug($"Request:{innerMethod} {innerUrl} headers:{innerHeaders?.JsonSerialize()} datas:{innerDatas?.JsonSerialize()}");
#endif
                    callback = delegate (JObject jObject)
                    {
#if DEBUG
                        ModLogger.Debug($"Response:{innerMethod} {innerUrl} result:{jObject?.JsonSerialize()}");
#endif
                        originSuccessCallback?.Invoke(jObject);
                    };
                    faillCallback = delegate (string str)
                    {
#if DEBUG
                        ModLogger.Debug($"Response failed:{innerMethod} {innerUrl} result:{str}");
#endif
                        originFailCallback?.Invoke(str);
                    };
                    break;
            }
            return true;
        }
    }
}
